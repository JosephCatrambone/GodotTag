using Godot;
using System;

public class Player : KinematicBody
{
	[Signal]
	delegate void Tagged(Node byWhom);
	
	public const string TAGGER_GROUP_NAME = "taggers";
	public const string TAGGED_GROUP_NAME = "tagged";

	private bool tagged = false; // Has this person been tagged?

	[Export] public int TicksBetweenChecks = 6;
	[Export] public float MovementSpeed = 1.0f;
	[Export] public float MovementMaxSpeed = 1.0f;
	[Export] public Curve MovementAccelerationCurve;
	[Export] public float MaxMomentumConservingAngle;  // If the player makes a turn sharper than this, they lose momentum and slide/skid.
	[Export] public float AirControl = 0.1f;
	[Export] public float JumpForce = 10.0f;
	[Export] public float JumpForgivenessTime = 0.1f; // How long after the player is airborne can they jump?

	private float airTime;
	private float runTime; // Used to track how long we've kept going in the same direction.
	private Vector3 previousVelocity = Vector3.Zero;
	private Controller controller;
	private RayCast groundRay;
	private RayCast tagRay;
	private int ticksToNextCheck = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.controller = GetNode<Controller>("Controller");
		this.groundRay = GetNode<RayCast>("GroundRay");
		this.tagRay = controller.GetNode<RayCast>("TagRay"); // This is a child of controller because it reaches FROM the camera.
		
		// Add ourselves to the group if so.
	}

	public override void _Process(float delta)
	{
		UpdateLookDirection(delta);
		UpdateMovement(delta);
		CheckForTags();
	}

	public void Tag(Node byWhom)
	{
		if (tagged) return; // Don't re-call.
		
		GD.Print(this.Name + " was tagged by " + byWhom.Name);
		tagged = true;
		AddToGroup(TAGGED_GROUP_NAME);
		EmitSignal(nameof(Tagged), byWhom);
	}

	public void UpdateLookDirection(float delta)
	{
		// Rather than applying the rotation to this _whole object_, which will move the camera forwards and back:
		//this.Transform = new Transform(controller.GetLook(), this.Transform.origin);
		this.Transform = new Transform(Basis.Identity.Rotated(Vector3.Up, controller.GetLookRotation()), this.Transform.origin);
		controller.Transform = new Transform(Basis.Identity.Rotated(Vector3.Right, controller.GetLookTilt()), controller.Transform.origin);
	}

	public void UpdateMovement(float delta)
	{
		// Actually move about.
		bool onGround = groundRay.IsColliding();
		Vector3 velocity = Vector3.Zero;
		if (onGround)
		{
			/*
				var result = GetWorld().DirectSpaceState.IntersectRay(new Vector2(), new Vector2(50, 100));
				{
				position: Vector2 # point in world space for collision
				normal: Vector2 # normal in world space for collision
				collider: Object # Object collided or null (if unassociated)
				collider_id: ObjectID # Object it collided against
				rid: RID # RID it collided against
				shape: int # shape index of collider
				metadata: Variant() # metadata of collider
				}
			 */
			// LERP between our position and the actual ground contact IF WE ARE LANDING!
			if (this.previousVelocity.y <= 0 && airTime > 0f)
			{
				Vector3 collisionPoint = groundRay.GetCollisionPoint();
				this.Translate(collisionPoint - this.Transform.origin); // Snap to ground.
				this.previousVelocity.y = 0; // So we don't continue to accumulate negative velocity after landing.
			}

			// Reset airtime.
			airTime = 0f;
			
			// Control movement and jumping.
			velocity = controller.GetMove() * MovementSpeed;
		}
		else
		{
			airTime += delta;
			//GD.Print("Not on ground." + _random.Next());
			//velocity = ((controller.GetMove() * MovementSpeed) * AirControl) + (previousVelocity * (1.0f - AirControl));
			velocity = previousVelocity + ((Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector") *
										   ((float)ProjectSettings.GetSetting("physics/3d/default_gravity") * delta));
		}
		
		// Allow tiny Wil-E-Coyote jumping.
		if (controller.Jumping && (onGround || airTime < JumpForgivenessTime) && previousVelocity.y <= 0f)
		{
			GD.Print("DEBUG: Jump!");
			velocity += Vector3.Up * JumpForce;
		}
		
		// Apply velocity.
		this.MoveAndSlide(velocity);
		previousVelocity = velocity;
	}

	public void CheckForTags()
	{
		// We do not tag.
		if (!this.IsInGroup(TAGGER_GROUP_NAME)) return;
		
		ticksToNextCheck--;
		if (ticksToNextCheck <= 0)
		{
			ticksToNextCheck = TicksBetweenChecks;
			
			//IsInGroup
			//AddToGroup
			//GetTree().CallGroup("guards", "enter_alert_mode");
			//var guards = GetTree().GetNodesInGroup("guards");
			if (tagRay.IsColliding())
			{
				Node collider = (Node) tagRay.GetCollider();
				if (collider is Player otherPlayer)
				{
					otherPlayer.Tag(this);
				}
			}
		}
	}
}
