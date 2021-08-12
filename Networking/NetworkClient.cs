using Godot;
using System;
using System.Net.Sockets;
using System.Text;

public class NetworkClient : Node
{
	private Player parent;
	private UdpClient client;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		parent = GetParent<Player>();
	}

	public void Connect(string hostname, int port)
	{
		client = new UdpClient(hostname, port);
		Byte[] sendBytes = Encoding.ASCII.GetBytes("Connecting");
		try
		{
			client.Send(sendBytes, sendBytes.Length);
		}
		catch ( Exception e )
		{
			GD.PrintErr(e);
		}
	}

	public override void _Process(float delta)
	{
		
	}
}
