using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] public int MaxHealth = 3;
	[Export] public float PatrolSpeed = 50f;
	[Export] public float ChaseSpeed = 50f;
	[Export] public float PatrolDistance = 75f;
	[Export] public float ChaseDuration = 5f;
	[Export] public float AttackCooldown = 1f;

	private int currentHealth;
	private bool isDead = false;
	private bool isAttacking = false;
	private bool isChasing = false;
	private bool isPatrolling = true;
	private bool playerInRange = false;
	private bool canAttack = true;
	private bool movingRight = false;
	private bool isPatrolWaiting = false;

	private Vector2 patrolOrigin;
	private Node2D player;
	private AnimationPlayer animationPlayer;
	private Timer deathTimer;

	public override void _Ready()
	{
		currentHealth = MaxHealth;
		patrolOrigin = GlobalPosition;

		GetNode<Area2D>("AttackArea").BodyEntered += (_) => playerInRange = true;
		GetNode<Area2D>("AttackArea").BodyExited += (_) => playerInRange = false;

		GetNode<Timer>("AttackArea/AttackCooldown").Timeout += () => canAttack = true;

		var detectionArea = GetNode<Area2D>("DetectionArea");
		detectionArea.BodyEntered += OnPlayerDetected;
		detectionArea.BodyExited += OnPlayerExited;

		deathTimer = GetNode<Timer>("DeathTimer");
		deathTimer.Timeout += QueueFree;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.AnimationFinished += OnAnimationFinished;
		animationPlayer.Play("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isDead) return;

		if (playerInRange && canAttack && !isAttacking)
		{
			StartAttack();
			return;
		}

		if (isAttacking)
		{
			Velocity = Vector2.Zero;
			return;
		}

		if (isPatrolling)
		{
			Patrol();
		}

		if (isChasing)
		{
			ChasePlayer();
		}
	}

	public void ApplyDamageToPlayer()
	{
		if (playerInRange && IsInstanceValid(player))
		{
			var playerScript = player as Player;
			//playerScript?.TakeDamage(1);
		}
	}

	public void OnAnimationFinished(StringName animationName)
	{
		if (animationName == "attack")
		{
			isAttacking = false;
		}
	}

	public void TakeDamage(int amount)
	{
		if (isDead) return;

		currentHealth -= amount;

		if (isAttacking)
		{
			isAttacking = false;
			DisableAttackHitbox();
		}

		animationPlayer.Play("take_damage");
		if (currentHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		isDead = true;
		Velocity = Vector2.Zero;
		animationPlayer.Play("die");
		deathTimer.Start();
	}

	private void StartAttack()
	{
		isAttacking = true;
		canAttack = false;
		Velocity = Vector2.Zero;
		animationPlayer.Play("attack");
		GetNode<Timer>("AttackArea/AttackCooldown").Start();
	}

	private void ChasePlayer()
	{
		if (player == null || !IsInstanceValid(player) || playerInRange)
		{
			return;
		}

		var direction = (player.GlobalPosition - GlobalPosition).Normalized();
		direction.Y = 0;
		Velocity = direction * ChaseSpeed;
		MoveAndSlide();
		animationPlayer.Play("move");
		FlipSprite(direction.X);
		movingRight = direction.X > 0;
		patrolOrigin = GlobalPosition;
	}

	private void Patrol()
	{
		if (isPatrolWaiting)
		{
			return;
		}

		Vector2 velocity = Velocity;
		animationPlayer.Play("move");

		if (movingRight)
		{
			velocity.X = PatrolSpeed;
			FlipSprite(1);
		}
		else
		{
			velocity.X = -PatrolSpeed;
			FlipSprite(-1);
		}

		Velocity = velocity;
		MoveAndSlide();

		if (GlobalPosition.X >= patrolOrigin.X + PatrolDistance)
		{
			movingRight = false;
			isPatrolWaiting = true;
			animationPlayer.Play("idle");
			GetTree().CreateTimer(2f).Timeout += () => isPatrolWaiting = false;
		}
		else if (GlobalPosition.X <= patrolOrigin.X - PatrolDistance)
		{
			movingRight = true;
			isPatrolWaiting = true;
			animationPlayer.Play("idle");
			GetTree().CreateTimer(2f).Timeout += () => isPatrolWaiting = false;
		}
	}

	private void FlipSprite(float xDirection)
	{
		var sprite = GetNode<Sprite2D>("Sprite2D");
		sprite.FlipH = xDirection > 0;
	}

	private void OnPlayerDetected(Node2D body)
	{
		if (body is Player)
		{
			player = body;
			isChasing = true;
			isPatrolling = false;
		}
	}

	private void OnPlayerExited(Node2D body)
	{
		if (body == player)
		{
			player = null;
			isChasing = false;
			isPatrolling = true;
		}
	}

	private void DisableAttackHitbox()
	{
		var hitbox = GetNode<CollisionShape2D>("AttackArea/Hurtbox");
		hitbox.SetDeferred("disabled", true);
	}
}
