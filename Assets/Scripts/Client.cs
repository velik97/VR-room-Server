using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System;

public class Client : MonoBehaviour {

	public string host = "127.0.0.1";
	public int port = 6321;

	private bool soccetReady = false;

	private TcpClient socket;
	private NetworkStream stream;

	private StreamWriter writer;
	private StreamReader reader;

	private void Update () {

		if (soccetReady) {

			if (stream.DataAvailable) {
				
				string data = reader.ReadLine ();

				if (data != null) {
					OnIncommingData (data);
				}
			}
		}
	}

	private void SendMessage (string msg, ServerClient c) {

		try {
			
			writer.WriteLine (msg);
			writer.Flush ();
		}
		catch (Exception e) {

			Debug.LogError ("[Msg to " + c.name + " error]: " + e.Message);
		}
	}

	public void ConnectToServer () {

		if (soccetReady) {

			Debug.LogWarning ("Already connected");
			return;
		}

		try {

			socket = new TcpClient (host, port);

			stream = socket.GetStream ();
			writer = new StreamWriter (stream);
			reader = new StreamReader (stream);

			soccetReady = true;

			Debug.Log ("Connected to server"); 
			SendMessagesToServer ();
		}
		catch (Exception e) {
			
			Debug.LogError ("[Socket Error]: " + e.Message);
		}
	}

	private void SendMessagesToServer () {
		StartCoroutine (SendMessagesToServerCyclically ());
	}

	IEnumerator SendMessagesToServerCyclically () {
		int i = 0;
		while (true) {
			SendMessage (i.ToString ());
			i++;
			yield return new WaitForSeconds (.1f);
		}
	}

	private void SendMessage (string msg) {

		try {

			writer.WriteLine (msg);
			writer.Flush ();
		}
		catch (Exception e) {

			Debug.LogError ("[Msg to server error]: " + e.Message);
		}
	}

	private void OnIncommingData (string data) {

		Debug.Log ("Server: " + data); 
	}
}
