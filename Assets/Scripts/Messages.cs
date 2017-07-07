using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//	Type						TypeID
//	PlayerSpawnMessage			101
//	PLayerConnectMessage		102
//	PLayerDisonnectMessage		103
//	PLayerTransformMessage		104
//	LampStateMessage			105
//	ErrorMessage				106

public class PlayerSpawnMessage : MessageBase {

	public int playerId;

}

public class PLayerConnectMessage : MessageBase {

	public int playerId;

}

public class PLayerDisonnectMessage : MessageBase {

	public int playerId;

}

public class PLayerTransformMessage : MessageBase {

	public int playerId;
//	public Vector3 position;
	public Vector3 eulerAngles;

}

public class LampStateMessage : MessageBase {

	public bool _on;

}

