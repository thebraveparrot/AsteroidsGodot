using Godot;
using System;

public class Overlay : Control
{
    private Label _score;
    private TextureRect _lives;
    private Vector2 _livesSize;

    public override void _Ready()
    {
        _score = GetNode<Label>("Score");
        _lives = GetNode<TextureRect>("Lives");
        _livesSize = _lives.Texture.GetSize();
    }

    public void UpdateScore(int score)
    {
        _score.Text = "Score: " + score;
    }

    public void SetLives(int lives)
    {
        _lives.RectSize = new Vector2(_livesSize.x * lives, _livesSize.y);
    }
}
