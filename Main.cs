using System;
using System.Collections.Generic;
using AsteroidsGodot;
using Godot;

public class Main : Node2D
{
    /// <summary>
    /// Don't create asteroids within this radius of pre-existing asteroids.
    /// </summary>
    /// <returns></returns>
    [Export] public int SpawnRadius = 200;
    
    [Signal]
    delegate void RemoveAsteroid(Asteroid which);

    private readonly Random _random = new Random();

    private readonly HashSet<Asteroid> _asteroids = new HashSet<Asteroid>();
    private PackedScene _asteroidScene;
    private Timer _asteroidSpawnTimer;
    private Player _player;
    private int _playerLives = 3;
    private int _score;
    private Overlay _overlay;

    public override void _Ready()
    {
        _asteroidScene = GD.Load<PackedScene>("res://Asteroid.tscn");
        _asteroidSpawnTimer = GetNode<Timer>("AsteroidSpawnTimer");

        _player = GetNode<Player>(nameof(Player));
        _player.Connect(nameof(Player.LoseLife), this, nameof(PlayerLostLife));
        
        _overlay = GetNode<Overlay>(nameof(Overlay));
    }

    public void CreateAsteroid()
    {
        var asteroid = _asteroidScene.Instance<Asteroid>();
        asteroid.Speed = _random.Next(100, 200);
        asteroid.SpinSpeed = (float) (Math.PI * _random.NextDouble());
        asteroid.MoveTowards = new Vector2(_player.Position);
        asteroid.Position = PositionForNewAsteroid();
        asteroid.Type = Asteroid.Size.Big;
        asteroid.Connect(nameof(Asteroid.Shot), this, nameof(AsteroidShot));

        AddChild(asteroid);

        _asteroids.Add(asteroid);
        _asteroidSpawnTimer.Start();
    }

    /// <summary>
    /// Finds a safe position to spawn a new asteroid.
    /// </summary>
    /// <returns></returns>
    private Vector2 PositionForNewAsteroid()
    {
        Vector2 pos;

        do
        {
            var side = _random.Next(4);

            switch (side)
            {
                case 0:
                    pos = new Vector2(_random.Next(0, (int) OS.WindowSize.x), 0);
                    break;
                case 1:
                    pos = new Vector2(OS.WindowSize.x, _random.Next(0, (int) OS.WindowSize.y));
                    break;
                case 2:
                    pos = new Vector2(0, _random.Next(0, (int) OS.WindowSize.y));
                    break;
                default:
                    pos = new Vector2(_random.Next(0, (int) OS.WindowSize.x), OS.WindowSize.y);
                    break;
            }
        } while (PositionNearExistingAsteroid(pos));


        return pos;
    }

    /// <summary>
    /// Checks if pos is within a certain radius from other asteroids.
    /// </summary>
    /// <param name="pos">Position to check</param>
    /// <returns></returns>
    private bool PositionNearExistingAsteroid(Vector2 pos)
    {
        foreach (var asteroid in _asteroids)
        {
            if ((new Vector2(pos) - asteroid.Position).Abs().Length() >= SpawnRadius)
            {
                return false;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Signal handler for when an Asteroid is shot by a bullet.
    /// </summary>
    /// <param name="asteroid">The asteroid that got shot.</param>
    public void AsteroidShot(Asteroid asteroid)
    {
        if (asteroid.Type == Asteroid.Size.Small)
        {
            IncrementScore();
        }
        else
        {
            CallDeferred(nameof(ReplaceWithTwoSmall), asteroid);
        }
        
        _asteroids.Remove(asteroid);
        asteroid.QueueFree();
    }
    
    private void IncrementScore()
    {
        _score++;
        _overlay.UpdateScore(_score);
    }

    private void ReplaceWithTwoSmall(Asteroid asteroid)
    {
        var newAsteroid = _asteroidScene.Instance<Asteroid>();
        newAsteroid.Speed = asteroid.Speed * 1.2f;
        newAsteroid.SpinSpeed = (float) (Math.PI * _random.NextDouble());
        newAsteroid.MoveTowards = new Vector2(asteroid.MoveTowards);
        newAsteroid.Position = new Vector2(asteroid.Position.x - 20, asteroid.Position.y);
        newAsteroid.Type = Asteroid.Size.Small;
        newAsteroid.Connect(nameof(Asteroid.Shot), this, nameof(AsteroidShot));
        AddChild(newAsteroid);
        _asteroids.Add(newAsteroid);
        
        newAsteroid = _asteroidScene.Instance<Asteroid>();
        newAsteroid.Speed = asteroid.Speed * 1.2f;
        newAsteroid.SpinSpeed = (float) (Math.PI * _random.NextDouble());
        newAsteroid.MoveTowards = new Vector2(asteroid.MoveTowards);
        newAsteroid.Position = new Vector2(asteroid.Position.x + 20, asteroid.Position.y);
        newAsteroid.Type = Asteroid.Size.Small;
        newAsteroid.Connect(nameof(Asteroid.Shot), this, nameof(AsteroidShot));
        AddChild(newAsteroid);
        _asteroids.Add(newAsteroid);
    }

    public void PlayerLostLife()
    {
        if (_playerLives == 1)
        {
            // This was the last life...get rid of the player.
            _player.QueueFree();
        }
        
        _overlay.SetLives(--_playerLives);
    }
}
