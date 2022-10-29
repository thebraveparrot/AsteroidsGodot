using System;
using Godot;

// ReSharper disable once CheckNamespace
public class Player : KinematicBody2D
{
    [Export] public float TimeToMaxSpeed = 0.3f;
    [Export] public int MaxSpeed = 500;
    [Export] public float TurnSpeed = (float) Math.PI * 2;
    
    [Signal] public delegate void LoseLife();

    private bool _moving;
    private float _rotate;
    private Vector2 _movementVector;
    private Tween _accelerationTween;
    private Timer _shootCooldown;
    private bool _canShoot = true;
    private PackedScene _bulletScene;
    private float _oob;
    private AudioStreamPlayer _laserSound;
    private Timer _respawnInvincibilityTimer;
    private CollisionPolygon2D _collisionShape;
    private AnimationPlayer _animationPlayer;
    private Sprite _sprite;

    public override void _Ready()
    {
        _bulletScene = GD.Load<PackedScene>("res://Bullet.tscn");
        _accelerationTween = GetNode<Tween>("AccelerationTween");
        // TODO: Add acceleration and deceleration to the player's ship

        _shootCooldown = GetNode<Timer>("ShootCooldown");
        
        _movementVector = new Vector2(0, -MaxSpeed);

        // This will get the hypotenuse formed by width and height of the sprite. This is the max distance the ship can
        // move outside of the side of the screen.
        _sprite = GetNode<Sprite>("Sprite");
        _oob = _sprite.Texture.GetSize().Length();

        _laserSound = GetNode<AudioStreamPlayer>("LaserFire");

        _respawnInvincibilityTimer = GetNode<Timer>("RespawnInvincibility");
        _collisionShape = GetNode<CollisionPolygon2D>(nameof(CollisionPolygon2D));
        _animationPlayer = GetNode<AnimationPlayer>(nameof(AnimationPlayer));
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (Input.IsActionPressed("ui_up"))
        {
            _accelerationTween.Start();
            _moving = true;
        }
        else if (Input.IsActionPressed("ui_left"))
        {
            _rotate = -1;
        }
        else if (Input.IsActionPressed("ui_right"))
        {
            _rotate = 1;
        }

        if (Input.IsActionPressed("ui_select") && _canShoot)
        {
            CallDeferred(nameof(FireBullet));
        }
    }

    public void FireBullet()
    {
        _canShoot = false;
            
        var bullet = _bulletScene.Instance<Bullet>();
        bullet.Position = new Vector2(Position);
        bullet.Rotation = Rotation;
        
        _shootCooldown.Start();
        _laserSound.Play();
        GetParent().AddChild(bullet);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_moving)
        {
            MoveAndCollide(_movementVector.Rotated(Rotation) * delta);
        }
        else if (_rotate != 0)
        {
           Rotate(_rotate * delta * TurnSpeed); 
        }

        WrapIfOutOfBounds();

        // Reset rotation and movement
        _rotate = 0;
        _moving = false;
    }

    private void WrapIfOutOfBounds()
    {
        if (Position.x < -_oob)
        {
            Position = new Vector2(OS.WindowSize.x + _oob - 1, Position.y);
        }
        else if (Position.x > OS.WindowSize.x + _oob)
        {
            Position = new Vector2(-_oob + 1, Position.y);
        }
        else if (Position.y < -_oob)
        {
            Position = new Vector2(Position.x, OS.WindowSize.y + _oob - 1);
        }
        else if (Position.y > OS.WindowSize.y + _oob)
        {
            Position = new Vector2(Position.x, -_oob + 1);
        }
    }

    public void CollidedWithAsteroid()
    {
        EmitSignal(nameof(LoseLife));
        Respawn();
        
        _animationPlayer.Play("blink");
    }

    private void Respawn()
    {
        _respawnInvincibilityTimer.Start();
        _animationPlayer.Play("blink");
        Position = new Vector2(OS.WindowSize / 2);
        _collisionShape.Disabled = true;
    }

    public void _on_ShootCooldown_timeout()
    {
        _canShoot = true;
        _shootCooldown.Start();
    }

    public void _on_RespawnInvincibility_timeout()
    {
        _animationPlayer.Stop();
        _collisionShape.Disabled = false;
    }

}
