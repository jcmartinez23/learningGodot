using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public int Speed = 200;
    [Export] public int JumpForce = -420;
    [Export] public int Gravity = 900;

	private AnimationPlayer animationPlayer;
	private Sprite2D sprite;
	private bool isAttacking = false;
    private bool isDead = false;
	private bool isBlocking = false;

	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animationPlayer.AnimationFinished += OnAnimationFinished;
		sprite = GetNode<Sprite2D>("Sprite2D");
		animationPlayer.Play("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isDead) return;

		var velocity = Velocity;

		// Gravity
        if (!IsOnFloor())
            velocity.Y += Gravity * (float)delta;

		float input = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
        velocity.X = isAttacking ? 0 : input * Speed;

		// Flip sprite
        if (input != 0) 
            sprite.FlipH = input < 0;

		// Jump
        if (Input.IsActionJustPressed("jump") && IsOnFloor() && !isAttacking)
        {
            velocity.Y = JumpForce;
            animationPlayer.Play("jump");
        }

		// Attack
        if (Input.IsActionJustPressed("attack") && !isAttacking)
        {
            StartAttack();
        }

	    // Play animations
        if (!isAttacking)
        {
            if (!IsOnFloor())
            {
                animationPlayer.Play("jump");
            }
            else if (input != 0)
            {
                animationPlayer.Play("run");
            }
            else
            {
                animationPlayer.Play("idle");
            }
        }

        Velocity = velocity;
        MoveAndSlide();
	}

	 private void StartAttack()
    {
        isAttacking = true;
        animationPlayer.Play("attack");
    }

	private void OnAnimationFinished(StringName animName)
    {
        if (animName == "attack")
            isAttacking = false;
    }
}
