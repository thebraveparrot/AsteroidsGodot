using Godot;
using System;
using AsteroidsGodot;

public class Bullet : Area2D
{
    [Export] public float Speed = 1000;

    private Vector2 _movementVector;
    private int _outOfBounds;

    public override void _Ready()
    {
        _outOfBounds = GetNode<Sprite>(nameof(Sprite)).Texture.GetHeight();
        _movementVector = new Vector2(0, -Speed).Rotated(Rotation);
    }
    
    public override void _PhysicsProcess(float delta)
    {
        Position += _movementVector * delta;
        RemoveIfOutOfBounds();
    }

    private void RemoveIfOutOfBounds()
    {
        if (Position.x < -_outOfBounds
            || Position.x > OS.WindowSize.x + _outOfBounds
            || Position.y < -_outOfBounds
            || Position.y > OS.WindowSize.y + _outOfBounds)
        {
            QueueFree();
        }
    }

    /// <summary>
    /// Callback function for when a bullet hits something.
    /// </summary>
    public void _on_Bullet_body_entered(Node body)
    {
        if (body is Asteroid asteroid)
        {
            asteroid.CollidedWithBullet();
            QueueFree();
        }
    }
}
