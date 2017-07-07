using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LocalDiscovery : NetworkDiscovery {

	public void StartBroadcasting () {
		Initialize ();
		StartAsServer ();
		MyNetworkManager.Instance.SetupServer (int.Parse (broadcastData));
	}
}
