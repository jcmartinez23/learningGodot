using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public int Speed = 50;
	[Export] public float PatrolDistance = 100f;
	[Export] public int MaxHealth = 40;
	[Export] public float IdleTime = 1.5f;
	[Export] public float StopChasingDistance = 300f;
	private enum State { Walking, Idle, Dying, Chasing }
	private State currentState = State.Walking;
	private int currentHealth;
	private Vector2 startPos;
	private int direction = -1;
	private float idleTimer = 0f;
	private AnimatedSprite2D sprite;
	private bool isChasing = false;
	private Node2D player;


	public override void _Ready()
	{
		currentHealth = MaxHealth;
		startPos = GlobalPosition;

		sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play("move");

        var detectionArea = GetNode<Area2D>("DetectionArea");
		detectionArea.BodyEntered += OnPlayerDetected;
        detectionArea.BodyExited += OnPlayerExited;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (currentState == State.Dying)
		{
			// Handle dying state
			return;
		}

		if (currentState == State.Chasing && IsInstanceValid(player))
        {
            Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * Speed;
            MoveAndSlide();

            // Flip sprite based on direction
            sprite.FlipH = Velocity.X < 0;
            sprite.Play("walk");
        }

		if (currentState == State.Idle)
		{
			idleTimer -= (float)delta;
			if (idleTimer <= 0)
			{
				// Resume walking
				currentState = State.Walking;
				direction *= -1;
				FlipSprite();
				sprite.Play("move");
			}
			return;
		}

		Velocity = new Vector2(direction * Speed, Velocity.Y);
		MoveAndSlide();

		if (Mathf.Abs(GlobalPosition.X - startPos.X) > PatrolDistance && currentState != State.Dying)
		{
			currentState = State.Idle;
			idleTimer = IdleTime;
			Velocity = Vector2.Zero;
			sprite.Play("idle");
		}
	}

	public void TakeDamage(int damage)
	{
		sprite.Play("take_damage");
		currentHealth -= damage;
		if (currentHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		var collider = GetNode<CollisionShape2D>("CollisionShape2D");
		collider.SetDeferred("disabled", true);
		currentState = State.Dying;
		Velocity = Vector2.Zero;
		sprite.Stop();
		sprite.Play("die");
		var timer = new Timer();
		timer.WaitTime = 3.0f; // Set the delay time in seconds
		timer.OneShot = true; // Ensure the timer only runs once
		timer.Timeout += () => QueueFree(); // Connect the timeout signal to remove the enemy
		AddChild(timer); // Add the timer to the scene
		timer.Start(); // Start the timer
	}

	private void FlipSprite()
	{
		var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.FlipH = direction > -1;
	}

	private void OnPlayerDetected(Node2D body)
    {
        if (body.Name == "player") // or use `body is Player`
        {
            player = body;
            currentState = State.Chasing;
		}
    }

    private void OnPlayerExited(Node2D body)
    {
        if (body == player)
        {
            currentState = State.Idle;
            player = null;
        }
    }
}
