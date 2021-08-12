using Godot;
using System;

public class MainMenu : Panel
{
	private NetworkHost networkHost;
	private LineEdit playerNameInput;
	private LineEdit hostnameInput;
	private LineEdit portInput;

	public override void _Ready()
	{
		networkHost = GetNode<NetworkHost>("/root/MainGame/NetworkHost");
		playerNameInput = GetNode<LineEdit>("VBoxContainer/PlayerSetup/PlayerNameInput");
		hostnameInput = GetNode<LineEdit>("VBoxContainer/NetworkSetup/HostnameInput");
		portInput = GetNode<LineEdit>("VBoxContainer/NetworkSetup/PortInput");
	}

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("ui_cancel"))
		{
			if (Input.GetMouseMode() == Input.MouseMode.Visible)
			{
				Input.SetMouseMode(Input.MouseMode.Captured);
			}
			else
			{
				Input.SetMouseMode(Input.MouseMode.Visible);
			}
		}

		this.Visible = Input.MouseMode.Visible == Input.GetMouseMode();
	}

	public void OnPlayerNameChanged(string newName)
	{
		GD.Print("New name: " + newName);
	}

	public void OnPlayerNameFinalized(string newName)
	{
		
	}

	public void OnStartHosting()
	{
		networkHost.Port = Int32.Parse(portInput.Text);
		networkHost.Start();
		hostnameInput.Text = "127.0.0.1";
		OnConnectToHost();
	}

	public void OnConnectToHost()
	{
		// Spawn a new player and connect to the host.
		Player player = GetNode<MainGame>("/root/MainGame").SpawnPlayer();
		NetworkClient networkClient = player.GetNode<NetworkClient>("NetworkClient");
		networkClient.Connect(hostnameInput.Text, Int32.Parse(portInput.Text));
	}
}
