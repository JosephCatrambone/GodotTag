using Godot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class NetworkHost : Node
{
	[Export] public int Port = 42069;

	private bool started = false;
	private UdpClient server;
	private IPEndPoint listeningEndpoint; // Needed to receive.
	
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void Start()
	{
		server = new UdpClient(Port);
		listeningEndpoint = new IPEndPoint(IPAddress.Any, Port);
		started = true;
	}

	public override void _Process(float delta)
	{
		if (!started) return;
		// Check for new incoming connections.
		//Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
		//string returnData = Encoding.ASCII.GetString(receiveBytes);
		
		// Handle new users connection.
		if (server.Available > 0)
		{
			Byte[] receiveBytes = server.Receive(ref listeningEndpoint);
			string returnData = Encoding.ASCII.GetString(receiveBytes);
			GD.Print(returnData);
		}

		// Handle inbound user movement traffic.

		// Handle broadcast user events.
	}
}
