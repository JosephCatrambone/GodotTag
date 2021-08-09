using Godot;
using System;

public class MouseAndKeyboardFPSController : Controller
{
	private float mouseXSensitivity = 1.0f;
	private float mouseYSensitivity = 1.0f;

	// We depend on a parent NODE for rotating this thing in a way that's agreeable.
	// So this shouldn't directly rotate itself -- it should only update heading and volition.  

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		base._Process(delta);

		// KEYBOARD MOVE INPUTS
		// This moves the player WITH RESPECT TO THE GLOBAL TRANSFORM OF THE CONTROLLER!
		// That is to say, if the player is still facing forward (but this has heading pointed right), we will 
		// move with respect to the forward direction.
		
		float dx = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		float dy = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");
		
		// Convert dx/dy into the global space of things.
		Vector3 forwardBack = GlobalTransform.basis.z * dy;
		forwardBack = new Vector3(forwardBack.x, 0, forwardBack.z);
		forwardBack = forwardBack.Normalized();

		Vector3 leftRight = GlobalTransform.basis.x * dx;
		leftRight = new Vector3(leftRight.x, 0, leftRight.z).Normalized();
		
		Volition = (leftRight + forwardBack).Normalized();
		
		// JUMP INPUTS
		Jumping = Input.GetActionStrength("jump") > 0.1f;
	}
	
	public override void _Input(InputEvent evt)
	{
		// Mouse in viewport coordinates.
		//if (@event is InputEventMouseButton eventMouseButton)
		//	GD.Print("Mouse Click/Unclick at: ", eventMouseButton.Position);
		//else if (@event is InputEventMouseMotion eventMouseMotion)
		//	GD.Print("Mouse Motion at: ", eventMouseMotion.Position);
		// Print the size of the viewport.
		//GD.Print("Viewport Resolution is: ", GetViewportRect().Size);
		// When the mouse mode is set to Input.MOUSE_MODE_CAPTURED, the event.position value from InputEventMouseMotion is the center of the screen. Use event.relative instead of event.position and event.speed
		if (evt is InputEventMouseMotion mouseMotion)
		{
			LookTilt -= mouseMotion.Relative.y * mouseYSensitivity / GetViewport().Size.y;
			LookRotation -= mouseMotion.Relative.x * mouseXSensitivity / GetViewport().Size.x;
			LookTilt = Math.Max(Math.Min(LookTilt, MAX_LOOK_TILT), MIN_LOOK_TILT);
		}
	}
}
