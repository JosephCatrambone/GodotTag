using Godot;
using System;

// All controllers should expose 

public class Controller : Spatial
{
	// We depend on a parent NODE for rotating this thing in a way that's agreeable.
	// So this shouldn't directly rotate itself -- it should only update heading and volition.  

	public float LookTilt;
	public float LookRotation;
	public Vector3 Volition; // Which way we want to move.
	public bool Jumping; // Are we hitting the jump key?

	protected const float MAX_LOOK_TILT = (float)Math.PI / 2.1111f;
	protected const float MIN_LOOK_TILT = (float)-Math.PI / 2.1111f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LookRotation = 0f;
		LookTilt = 0f;
	}

	public virtual float GetLookRotation()
	{
		return LookRotation;
	}

	public virtual float GetLookTilt()
	{
		return LookTilt;
	}

	public virtual Quat GetHeading()
	{
		// Interpolate from what our current rotation is to where we should be looking.
		Basis target = Basis.Identity;
		target = target.Rotated(Vector3.Right, LookTilt);
		target = target.Rotated(Vector3.Up, LookRotation);
		return target.RotationQuat();
	}

	public virtual Vector3 GetMove()
	{
		return Volition;
	}
}
