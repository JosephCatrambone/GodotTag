using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class MainGame : Spatial
{
	[Export] public string PlayerResourcePath = "res://Player/Player.tscn"; 
	private List<Godot.Collections.Dictionary> players = new List<Godot.Collections.Dictionary>();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseMode.Captured);
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
				CreateOrJoinServer("", 42069, 16);
			}
			else
			{
				CreateOrJoinServer("127.0.0.1", 42069, 0);
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
			// Broadcast a disconnect before we terminate everything.
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
		RpcId(id, nameof(RegisterPlayer), new object[] { myInfo });
	}

	public void OnClientConnectionSuccess()
	{
		GD.Print("(Client-Side) Player Connected Callback.  ID: ");
		// Only called on clients, not server.
		Godot.Collections.Dictionary myInfo = new Godot.Collections.Dictionary();
		myInfo["player_name"] = "Player " + GetTree().GetNetworkUniqueId();
		myInfo["id"] = GetTree().GetNetworkUniqueId();
		myInfo["ready"] = false;
		RpcId(1, nameof(RegisterPlayer), new object[] { myInfo });
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
	public void RegisterPlayer(Godot.Collections.Dictionary info)
	{
		/*
		(Server Host) Player Connected Callback.  ID: 241530385
		Player 'Player 1' joined the server.
		Client says that its rpc ID is 241530385
		Client info says that its ID is 1
		Client says that its rpc ID is 241530385
		Client info says that its ID is 241530385
		 */
		int rpcID = GetTree().GetRpcSenderId();
		GD.Print("Client says that its rpc ID is " + rpcID);
		GD.Print("Client info says that its ID is " + info["id"]);
		//Debug.Assert((int)info["id"] == rpcID);
		players.Add(info);
	}

	[Remote]
	public void StartGame()
	{
		GetTree().RefuseNewNetworkConnections = true; // Don't let people join late.
		int selfID = GetTree().GetNetworkUniqueId();
		GetTree().Paused = true;

		// Do loading and all that stuff here.
		// Load level, etc.
		//ResourceLoader.Load(PlayerResourcePath4)
		Resource playerResource = GD.Load(PlayerResourcePath);
		
	}
}
