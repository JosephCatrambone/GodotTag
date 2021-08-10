using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class MainGame : Spatial
{
	private const int SERVER_PORT = 42069;
	private const int MAX_PLAYERS = 16;
	
	[Export] public string PlayerResourcePath = "res://Player/Player.tscn";
	private PackedScene playerPackedScene;
	private IDictionary<int, Godot.Collections.Dictionary> players = new Dictionary<int, Godot.Collections.Dictionary>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseMode.Captured);
		playerPackedScene = ResourceLoader.Load<PackedScene>(PlayerResourcePath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (Input.GetMouseMode() == Input.MouseMode.Visible)
				Input.SetMouseMode(Input.MouseMode.Captured);
			else
				Input.SetMouseMode(Input.MouseMode.Visible);
		}

		if ((Input.IsKeyPressed((int) KeyList.F5) || Input.IsKeyPressed((int) KeyList.F6)) && GetTree().NetworkPeer == null)
		{
			if (Input.IsKeyPressed((int) KeyList.F5))
			{
				CreateOrJoinServer("", SERVER_PORT, MAX_PLAYERS);
			}
			else
			{
				CreateOrJoinServer("127.0.0.1", SERVER_PORT, 0);
			}
		}
	}

	//
	// Multiplayer Component:
	//
	
	/// <summary>
	/// Set up a network multiplayer game OR connect to a network multiplayer game.
	/// If serverIPAddress is null or empty string, launch as HOST.
	/// </summary>
	/// <param name="serverIpAddress"></param>
	/// <param name="port"></param>
	/// <param name="maxPlayers"></param>
	public void CreateOrJoinServer(string serverIpAddress, int port, int maxPlayers)
	{
		NetworkedMultiplayerENet eNet = new NetworkedMultiplayerENet();
		if (String.IsNullOrEmpty(serverIpAddress))
		{
			eNet.CreateServer(port, maxPlayers);
		}
		else
		{
			eNet.CreateClient(serverIpAddress, port);
		}
		
		// Don't connect before we attach signals.
		// Emitted when this MultiplayerAPI's network_peer connects with a new peer. ID is the peer ID of the new peer.
		// Clients get notified when other clients connect to the same server.
		// Upon connecting to a server, a client also receives this signal for the server (with ID being 1).
		GetTree().Connect("network_peer_connected", this, nameof(OnPlayerConnected));
		//GetTree().Connect("NetworkPeerConnected", this, nameof(PlayerDisconnected));
		// Emitted when this MultiplayerAPI's network_peer successfully connected to a server. Only emitted on clients.
		GetTree().Connect("connected_to_server", this, nameof(OnClientConnectionSuccess));
		// Emitted when this MultiplayerAPI's network_peer fails to establish a connection to a server. Only emitted on clients.
		GetTree().Connect("connection_failed", this, nameof(OnClientConnectionFailure));
		GetTree().Connect("server_disconnected", this, nameof(OnServerDisconnected));
		
		// NOW connect.
		GetTree().NetworkPeer = eNet;
	}

	public void Disconnect()
	{
		if (GetTree().IsNetworkServer())
		{
			NetworkedMultiplayerPeer net = GetTree().NetworkPeer;
			// Broadcast a disconnect before we terminate everything.
			//foreach (var player in players) {}
			//net.SetTargetPeer();  // Default broadcast.
		}

		GetTree().NetworkPeer = null; // Disconnects all.
	}

	public void StartLobby()
	{
		//set_refuse_new_network_connections(bool)
	}

	public void OnPlayerConnected(int id)
	{
		GD.Print("(Server Host) Player Connected Callback.  ID: " + id);
		// Call register here and on local client.
		Godot.Collections.Dictionary myInfo = new Godot.Collections.Dictionary();
		myInfo["player_name"] = "Player " + GetTree().GetNetworkUniqueId();
		myInfo["id"] = id;
		myInfo["ready"] = false;
		GD.Print("Player '" + myInfo["player_name"] + "' joined the server.");
		RpcId(id, nameof(RegisterAndSpawnPlayer), new object[] { myInfo });
	}

	public void OnClientConnectionSuccess()
	{
		GD.Print("(Client-Side) Player Connected Callback.  ID: ");
		// Only called on clients, not server.
		Godot.Collections.Dictionary myInfo = new Godot.Collections.Dictionary();
		myInfo["player_name"] = "Player " + GetTree().GetNetworkUniqueId();
		myInfo["id"] = GetTree().GetNetworkUniqueId();
		myInfo["ready"] = false;
		// Notify the server that we have connected.
		RpcId(1, nameof(RegisterAndSpawnPlayer), new object[] { myInfo });
	}

	public void OnClientConnectionFailure()
	{
		// Couldn't connect.
	}

	public void OnServerDisconnected(int id)
	{
		// Server has kicked client.
	}

	// The remote keyword means that the rpc() call will go via network and execute remotely.
	// The remotesync keyword means that the rpc() call will go via network and execute remotely, but will also execute locally (do a normal function call).
	[Remote]
	public void RegisterAndSpawnPlayer(Godot.Collections.Dictionary info)
	{
		int senderID = GetTree().GetRpcSenderId();
		//int networkID = GetTree().GetNetworkUniqueId(); // Local, not the sender.
		
		if (GetTree().IsNetworkServer())
		{
			// As the network host, I need to send all the players to this newly joined person.
			foreach (var player in players)
			{
				RpcId(senderID, nameof(RegisterAndSpawnPlayer), player.Value);
			}
		}
		
		// I am not the server and have been notified about the arrival of a new person.
		//GD.Print("Client says that its rpc ID is " + rpcID);
		//GD.Print("Client info says that its ID is " + info["id"]);
		//GD.Print("Client says that its network ID is " + networkID);
		players[senderID] = info; // RPC ID or Network ID?

		Spatial newPlayer = playerPackedScene.Instance<Spatial>();
		newPlayer.Name = "Player " + info["player_name"] + " | " + senderID;
		newPlayer.SetNetworkMaster(senderID);
		newPlayer.GetNode("PlayerNameDisplayOffset/FloatingText").Call("set_text", newPlayer.Name);
		GetTree().Root.GetNode("MainGame/Level").AddChild(newPlayer);
	}

	[Remote]
	public void StartGame()
	{
		GetTree().RefuseNewNetworkConnections = true; // Don't let people join late.
		int selfID = GetTree().GetNetworkUniqueId(); // This should be 1 for the server.
		GetTree().Paused = true;

		// Do loading and all that stuff here.
		// Load level, etc.
		//ResourceLoader.Load(PlayerResourcePath4)
		
		
	}
}
