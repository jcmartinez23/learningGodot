using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 260.0f;
	[Export] public float JumpVelocity = -400.0f;
	[Export] public float Gravity = 850.0f;

	private AnimatedSprite2D sprite;
	private bool isAttacking = false;
	
	public override void _Ready()
	{
		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// Connect animation finished signal to detect when attack ends
		sprite.AnimationFinished += OnAnimationFinished;
		GetNode<Area2D>("AttackPivot/AttackArea").BodyEntered += OnAttackHit;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Gravity
		if (!IsOnFloor())
			velocity.Y += Gravity * (float)delta;

		// Handle attack input
		if (isAttacking && IsOnFloor())
		{
			// Stop movement while attacking
			velocity.X = 0;
			Velocity = velocity;
			MoveAndSlide();
			return; // Skip further movement logic
		}


		// Input
		float direction = 0;
		if (Input.IsActionPressed("ui_right")) direction += 1;
		if (Input.IsActionPressed("ui_left")) direction -= 1;

		velocity.X = direction * Speed;

		// Jump
		if (IsOnFloor() && Input.IsActionJustPressed("ui_up"))
			velocity.Y = JumpVelocity;

		Velocity = velocity;
		MoveAndSlide();

		// Flip sprite
		if (direction != 0) 
		{
			// Assuming `direction.X` controls left/right movement
			bool facingLeft = direction < 0;

			// Flip the sprite
			sprite.FlipH = facingLeft;
			var attackPivot = GetNode<Node2D>("AttackPivot");
			attackPivot.Scale = new Vector2(direction < 0 ? -1 : 1, 1);
		}

		// Handle attack input
		if (!isAttacking && Input.IsActionJustPressed("attack"))
		{
			StartAttack();
			return; // Don't let movement animations override attack
		}

		// Animation logic
		if (!isAttacking)
		{
			if (!IsOnFloor())
				sprite.Play("jump");
			else if (direction != 0)
				sprite.Play("move");
			else
				sprite.Play("idle");
		}
	}

	private void OnAnimationFinished()
	{
		if (sprite.Animation == "attack")
			isAttacking = false;
	}

	private void OnAttackHit(Node body)
	{
		if (body.IsInGroup("enemy"))
		{
			GD.Print("Hit enemy: " + body.Name);
			// You could also call a method on the enemy, like:
			// ((Enemy)body).TakeDamage(1);
		}
	}

	private async void StartAttack()
	{
		isAttacking = true;
		sprite.Play("attack");

		var hitbox = GetNode<CollisionShape2D>("AttackPivot/AttackArea/CollisionShape2DAttack");
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		hitbox.Disabled = false;

		// Wait for a short moment (adjust to match your animation timing)
		await ToSignal(GetTree().CreateTimer(0.4f), "timeout");

		hitbox.Disabled = true;
	}
}
