using Godot;
using System;

public partial class AttackArea : Area2D
{
	[Export] public int Damage = 20;

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage);
        }
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }
}
