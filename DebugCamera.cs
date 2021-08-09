using Godot;
using System;

public class DebugCamera : Camera
{
	private Camera startCamera = null;

	public override void _Process(float delta)
	{
		// TOGGLE 3RD PERSON ON F3
		if (Input.IsActionJustReleased("DEBUG_toggle_third_person"))
		{
			if (startCamera == null)
			{
				startCamera = GetViewport().GetCamera();
				this.Current = true;
			}
			else
			{
				this.Current = false;
				startCamera.Current = true;
				startCamera = null;
			}
		}
		
		// If active...
		if (startCamera != null)
		{
			this.LookAt(startCamera.Transform.origin, Vector3.Up);
		}
	}
}
