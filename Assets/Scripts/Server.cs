using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.Net;
using System.IO;

public class Server : MonoBehaviour {

	public int port = 6231;

	private List <ServerClient> clients;
	private List <ServerClient> disconnectedClients;

	private TcpListener server;
	private bool serverStarted;

	private void Start () {
//		StartServer ();
	}

	private void Update () {
		
		if (!serverStarted)
			return;

		foreach (ServerClient c in clients) {
			
			if (!IsConnected (c.client)) {
				
				c.client.Close ();
				disconnectedClients.Add (c);
				continue;
			}

			NetworkStream s = c.client.GetStream ();
			if (s.DataAvailable) {

				StreamReader reader = new StreamReader (s, true);
				string data = reader.ReadLine ();

				if (data != null)
					OnIncommingData (data, c);
			}
		}
	}

	public void StartServer () {
		clients = new List<ServerClient> ();
		disconnectedClients = new List <ServerClient> ();

		try {
			
			server = new TcpListener (IPAddress.Any, port);
			server.Start ();

			StartListening();
			serverStarted = true;

			Debug.Log ("Server has been started on port " + port); 
		} catch (Exception e) {
			
			Debug.LogError ("[Soccet error]: " + e.Message);
		}
	}

	private void StartListening () {
		server.BeginAcceptTcpClient (AcceptTcpClient, server);
	}

	private void AcceptTcpClient (IAsyncResult ar) {

		TcpListener listener = (TcpListener)ar.AsyncState;

		clients.Add (new ServerClient(listener.EndAcceptTcpClient (ar)));

		Debug.Log ("have new connection"); 

		SendMsgToAll (clients [clients.Count - 1].name + " has connected to server");
		StartListening ();
	}

	private void SendMsgToAll (string msg) {
		foreach (ServerClient c in clients) {
			SendMessage (msg, c);
		}
	}

	private void SendMessage (string msg, ServerClient c) {

		try {

			StreamWriter writer = new StreamWriter (c.client.GetStream ());
			writer.WriteLine (msg);
			writer.Flush ();
		}
		catch (Exception e) {

			Debug.LogError ("[Msg to " + c.name + " error]: " + e.Message);
		}
	}

	private void OnIncommingData (string data, ServerClient c) {
		Debug.Log (c.name + " has sent the message: " + data); 
	}

	private bool IsConnected (TcpClient c) {

		try {

			if (c != null && c.Client != null && c.Client.Connected) {

				if (c.Client.Poll (0, SelectMode.SelectRead)) {
					return ! (c.Client.Receive (new byte[1], SocketFlags.Peek) == 0);
				}

				return true;
			} 

			return false;
				
		} catch {

			return false;
		}

	}

}

public class ServerClient {

	public TcpClient client;
	public string name;

	public ServerClient (TcpClient tcpSoccet) {
		client = tcpSoccet;
		name = "Guest";
	}

}