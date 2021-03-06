﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections.Generic;

public class MyNetworkManager : MonoSingleton <MyNetworkManager> {

	public Dictionary <int, int> connectionIdPalyerIdDict;
	public List <int> freePlayerIds;

	public bool lampIsOn;

	public Text serverButtonText;

	public const short SpawnPlayerMessageId = 101;
	public const short PLayerConnectMessageId = 102;
	public const short PLayerDisonnectMessageId = 103;
	public const short PLayerTransformMessageId = 104;
	public const short LampStateMessageId = 105;
	public const short ErrorMessageId = 106;

	private bool listening;

	void Awake () {
		
		listening = false;
		serverButtonText.text = "Start server";
	}

	public void OnButtonClicked () {
		
		if (listening) {
			
			NetworkServer.Shutdown ();
			GetComponent <LocalDiscovery> ().StopBroadcast ();
			listening = false;
			serverButtonText.text = "Start server";
			Logger.Instance.Log ("Stopped listening"); 
		} else {
			
			GetComponent <LocalDiscovery> ().StartBroadcasting ();
		}
	}

	public void SetupServer(int port) {

		connectionIdPalyerIdDict = new Dictionary <int,int> ();
		freePlayerIds = new List<int> ();
		for (int i = 0; i < 4; i++) {
			freePlayerIds.Add (i + 1);
		}

		RegisterHandlers ();
		NetworkServer.Listen(port);

		listening = true;
		serverButtonText.text = "Stop server";

		Logger.Instance.Log ("Started listening on port: " + port);
	}

	void RegisterHandlers () {
		
		NetworkServer.RegisterHandler (MsgType.Connect, OnConnectedClient);
		NetworkServer.RegisterHandler (MsgType.Disconnect, OnDisconnectClient);

		NetworkServer.RegisterHandler (LampStateMessageId, OnChangeLampState);
		NetworkServer.RegisterHandler (PLayerTransformMessageId, OnPlayerTransform);
	}

	void OnConnectedClient (NetworkMessage msg) {

		// Checking if there are free playerIds on server
		if (freePlayerIds.Count == 0) {
			Logger.Instance.Log ("[Connection error]: server is full (4 clients)");
			msg.conn.Disconnect ();
			return;
		}

		// Finding playerId and connectionId for new client
		int newConnectionId = msg.conn.connectionId;
		int newPlayerId = freePlayerIds [0];

		// Sending to new client message with his new playerId for him to spawn himself
		PlayerSpawnMessage playerSpawnMessage = new PlayerSpawnMessage ();
		playerSpawnMessage.playerId = newPlayerId;
		NetworkServer.SendToClient (newConnectionId, SpawnPlayerMessageId, playerSpawnMessage);

		// Sending to new client lamp state
		LampStateMessage lampStateMessage = new LampStateMessage ();
		lampStateMessage._on = lampIsOn;
		NetworkServer.SendToClient (newConnectionId, LampStateMessageId, lampStateMessage);

		// Sending to new client messages with playerId of active clients for him to spawn them
		PLayerConnectMessage pLayerConnectionMessage = new PLayerConnectMessage ();
		foreach (int playerId in connectionIdPalyerIdDict.Values) {
			pLayerConnectionMessage.playerId = playerId;
			NetworkServer.SendToClient (newConnectionId, PLayerConnectMessageId, pLayerConnectionMessage);
		}

		// Sending to active clients playerId of new client for them to spawn him
		pLayerConnectionMessage.playerId = newPlayerId;
		foreach (int connectionId in connectionIdPalyerIdDict.Keys) {
			NetworkServer.SendToClient (connectionId, PLayerConnectMessageId, pLayerConnectionMessage);
		}

		// Adding new client to dictionary of 'connetionId - playerId' and deleting his playerId from list of free Ids
		connectionIdPalyerIdDict.Add (newConnectionId, newPlayerId);
		freePlayerIds.Remove (newPlayerId);

		Logger.Instance.Log ("Connected client playerId: " + newPlayerId + ", connectionId: " + newConnectionId); 
	}

	void OnDisconnectClient (NetworkMessage msg) {
		
		int connectionId = msg.conn.connectionId;
		int playerId = connectionIdPalyerIdDict [connectionId];

		if (!freePlayerIds.Contains (playerId)) {
			
			freePlayerIds.Add (playerId);
			connectionIdPalyerIdDict.Remove (connectionId);

			PLayerDisonnectMessage playerDisconnectMessage = new PLayerDisonnectMessage ();
			playerDisconnectMessage.playerId = playerId;

			foreach (int id in connectionIdPalyerIdDict.Keys) {
				NetworkServer.SendToClient (id, PLayerDisonnectMessageId, playerDisconnectMessage);
			}

			Logger.Instance.Log ("Disconnected client. playerId: " + playerId + ", connectionId: " + connectionId); 
		} else {
			
			Logger.Instance.Log ("Trying to disconnect client with playerId " + playerId + ", but it doesn't exist");
		}
	}

	void OnChangeLampState (NetworkMessage msg) {
		
		LampStateMessage lampStateMessage = msg.ReadMessage <LampStateMessage> ();
		lampIsOn = lampStateMessage._on;
		NetworkServer.SendToAll (LampStateMessageId, lampStateMessage);

		Logger.Instance.Log ("PlayerId: " + connectionIdPalyerIdDict[msg.conn.connectionId] + ", lamp state: " + lampStateMessage._on); 
	}

	void OnPlayerTransform (NetworkMessage msg) {
		
		PLayerTransformMessage playerTransformMessage = msg.ReadMessage <PLayerTransformMessage> ();
		int playerId = playerTransformMessage.playerId;

		foreach (int connectionId in connectionIdPalyerIdDict.Keys) {
			
			if (connectionIdPalyerIdDict [connectionId] != playerId) {
				
				NetworkServer.SendToClient (connectionId, PLayerTransformMessageId, playerTransformMessage);
			}
		}
	}

}