// =======================================================================================
// Wovencore
// by Weaver (Fhiz)
// MIT licensed
// =======================================================================================

using wovencode;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace wovencode
{

    // ===================================================================================
	// NetworkManager
	// ===================================================================================
	[RequireComponent(typeof(wovencode.NetworkAuthenticator))]
	[DisallowMultipleComponent]
	public partial class NetworkManager : Mirror.NetworkManager
	{
	
		public static Dictionary<string, GameObject> onlinePlayers = new Dictionary<string, GameObject>();
	
		[HideInInspector]public NetworkState state = NetworkState.Offline;
		
		[Header("Settings")]
		[Tooltip("Set a delay here to make players stay around for a little longer, even after they disconnect.")]
		public float disconnectDelay = 1;
		
		[Header("Servers")]
		public List<ServerInfo> serverList = new List<ServerInfo>()
		{
        	new ServerInfo{name="Local", ip="localhost"}
    	};
		
		[Header("Events")]
		public UnityEvent onStartServer;
		public UnityEvent onStopServer;
		public UnityEventGameObject onStartClient;
		public UnityEventGameObject onStopClient;
				
		[Header("Message Texts")]
		public string msgClientDisconnected 	= "Disconnected.";
		public string msgUserAlreadyOnline		= "User is already online!";
		
		// -------------------------------------------------------------------------------
		public override void Awake()
		{
			base.Awake();
		}

		// -------------------------------------------------------------------------------
		public override void Start()
		{
			base.Start();
			this.InvokeInstanceDevExtMethods("Start");
		}
		
		// -------------------------------------------------------------------------------
		void Update()
		{
			if (ClientScene.localPlayer != null)
				state = NetworkState.Game;
		}
		
		// -------------------------------------------------------------------------------
		public bool AccountLoggedIn(string _name)
		{
			foreach (KeyValuePair<string, GameObject> player in onlinePlayers)
				if (player.Value.name == _name) return true;
			
			return false;
		}
		
		// -------------------------------------------------------------------------------
		public void ServerSendError(NetworkConnection conn, string error, bool disconnect)
		{
			conn.Send(new ErrorMsg{text=error, causesDisconnect=disconnect});
		}
		
		// -------------------------------------------------------------------------------
		void OnClientError(NetworkConnection conn, ErrorMsg message)
		{
			
			if (!String.IsNullOrWhiteSpace(message.text))
				UIPopupConfirm.singleton.Init(message.text);
			
			if (message.causesDisconnect)
			{
				conn.Disconnect();
				if (NetworkServer.active) StopHost();
			}
		}
		
		// -------------------------------------------------------------------------------
		public override void OnStartClient()
		{
			this.InvokeInstanceDevExtMethods(nameof(OnStartClient));
		}
		
		// -------------------------------------------------------------------------------
		public override void OnStartServer()
		{
			onStartServer.Invoke();
			this.InvokeInstanceDevExtMethods(nameof(OnStartServer));
		}
		
		// -------------------------------------------------------------------------------
		public override void OnStopServer()
		{
			onStopServer.Invoke();
			this.InvokeInstanceDevExtMethods(nameof(OnStopServer));
		}
		
		// -------------------------------------------------------------------------------
		public bool IsConnecting() => NetworkClient.active && !ClientScene.ready;
		
		// -------------------------------------------------------------------------------
		public override void OnClientConnect(NetworkConnection conn) {
			this.InvokeInstanceDevExtMethods(nameof(OnClientConnect));
		}
		
		// -------------------------------------------------------------------------------
		public override void OnServerConnect(NetworkConnection conn) {
			this.InvokeInstanceDevExtMethods(nameof(OnServerConnect));
		}
		
		// -------------------------------------------------------------------------------
		public override void OnClientSceneChanged(NetworkConnection conn) {
			this.InvokeInstanceDevExtMethods(nameof(OnClientSceneChanged));
		}

		// -------------------------------------------------------------------------------
		public void LoginPlayer(NetworkConnection conn, string _name)
		{
			if (!AccountLoggedIn(_name))
			{
				GameObject player = DatabaseManager.singleton.LoadData(playerPrefab, _name);
				NetworkServer.AddPlayerForConnection(conn, player);
				onStartClient.Invoke(ClientScene.localPlayer.gameObject);
				state = NetworkState.Game;
			}
			else
				ServerSendError(conn, msgUserAlreadyOnline, true);
		}
		
		// -------------------------------------------------------------------------------
		public override void OnServerAddPlayer(NetworkConnection conn) {}
	
		
		// -------------------------------------------------------------------------------
		public override void OnServerDisconnect(NetworkConnection conn)
		{
			StartCoroutine(DoServerDisconnect(conn, disconnectDelay));
			this.InvokeInstanceDevExtMethods(nameof(OnServerDisconnect));
		}
		
		// -------------------------------------------------------------------------------
		IEnumerator<WaitForSeconds> DoServerDisconnect(NetworkConnection conn, float delay)
		{
			yield return new WaitForSeconds(delay);

			if (conn.identity != null)
			{
				DatabaseManager.singleton.SaveData(conn.identity.gameObject, false);
				Debug.Log("[NetworkManager] Saved player: " + conn.identity.name);
				onStopClient.Invoke(conn.identity.gameObject);
			}

			base.OnServerDisconnect(conn);
		}
		
		// -------------------------------------------------------------------------------
		// OnClientDisconnect
		// @Client
		// -------------------------------------------------------------------------------
		public override void OnClientDisconnect(NetworkConnection conn)
		{
			base.OnClientDisconnect(conn);
			state = NetworkState.Offline;
			UIPopupConfirm.singleton.Init(msgClientDisconnected);
			this.InvokeInstanceDevExtMethods(nameof(OnClientDisconnect));
		}
		
		// -------------------------------------------------------------------------------
		// Quit
		// universal quit function
		// -------------------------------------------------------------------------------
		public static void Quit()
		{
#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	
		// ======================== PUBLIC EVENT METHODS =================================
		
		// -------------------------------------------------------------------------------
		// EventCreateUser
		// Invoked when the NetworkAuthenticator sucessfully registers a new user
		// -------------------------------------------------------------------------------
		public void EventCreateUser(string _name)
		{
			GameObject player = Instantiate(playerPrefab);
#if WOCO_PLAYER
			PlayerComponent playerComponent = player.GetComponent<PlayerComponent>();
			playerComponent.username = _name;
#endif
			player.name = _name;
			DatabaseManager.singleton.CreateDefaultData(player);
			DatabaseManager.singleton.SaveData(player, false);
			Destroy(player);
		}
		
		// -------------------------------------------------------------------------------
		// EventStartPlayer
		// Invoked when the clients player enters the scene
		// -------------------------------------------------------------------------------
		public void EventStartPlayer(GameObject player)
		{
			onlinePlayers[player.name] = player;
			this.InvokeInstanceDevExtMethods(nameof(EventStartPlayer));
		}
	
		// -------------------------------------------------------------------------------
		// EventDestroyPlayer
		// Invoked when the clients player is destroyed / client disconnects
		// -------------------------------------------------------------------------------
		public void EventDestroyPlayer(GameObject player)
		{
			onlinePlayers.Remove(player.name);
			this.InvokeInstanceDevExtMethods(nameof(EventDestroyPlayer));
		}
			
		// -------------------------------------------------------------------------------

	}

}

// =======================================================================================