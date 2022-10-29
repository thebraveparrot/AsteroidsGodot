using Godot;

namespace AsteroidsGodot
{
    public class Asteroid : KinematicBody2D
    {
        /// <summary>
        /// Location of the player when this asteroid was created.
        /// </summary>
        [Export] public Vector2 MoveTowards { get; set; }
        
        /// <summary>
        /// Speed at which the asteroid rotates.
        /// </summary>
        [Export] public float SpinSpeed { get; set; }
        
        /// <summary>
        /// Speed at which the asteroid moves.
        /// </summary>
        [Export] public float Speed { get; set; }

        [Export] public Size Type = Size.Big;
        
        [Signal] public delegate void Shot(Asteroid asteroid);

        private Vector2 _movementVector;
        private KinematicCollision2D _collision;

        public override void _Ready()
        {
            _movementVector = (MoveTowards - Position).Normalized() * Speed;

            if (Type == Size.Small)
            {
                ApplyScale(new Vector2(0.5f, 0.5f));
            }
        }

        public override void _Process(float delta)
        {
            Rotation += SpinSpeed * delta;
        }

        public override void _PhysicsProcess(float delta)
        {
            _collision = MoveAndCollide(_movementVector * delta);

            if (_collision?.Collider is Player player)
            {
                player.CollidedWithAsteroid();
            }
        }

        public void CollidedWithBullet()
        {
            EmitSignal(nameof(Shot), this);
        }

        public enum Size
        {
            Small,
            Big
        }
    }
}
