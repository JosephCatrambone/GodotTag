using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class MainGame : Spatial
{
	[Export] public string PlayerResourcePath = "res://Player/Player.tscn";
	private PackedScene playerPackedScene;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseMode.Captured);
		playerPackedScene = ResourceLoader.Load<PackedScene>(PlayerResourcePath);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		
	}

	public Player SpawnPlayer()
	{
		Player newPlayer = playerPackedScene.Instance<Player>();
		newPlayer.GetNode("PlayerNameDisplayOffset/FloatingText").Call("set_text", newPlayer.Name);
		GetTree().Root.GetNode("MainGame/Level").AddChild(newPlayer);
		return newPlayer;
	}
}
