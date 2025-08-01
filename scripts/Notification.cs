using Godot;
using System;

public partial class Notification : MarginContainer
{
    private Timer _timer;
    private Label _label;
    private AnimationPlayer _player;
    public override void _Ready()
    {
        _timer = GetNode<Timer>("Timer");
        _label = GetNode<Label>("PanelContainer/MarginContainer/Label");
        _player = GetNode<AnimationPlayer>("AnimationPlayer");

        _timer.Timeout += QueueFree;
        MinimumSizeChanged += SetPivot;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if(Input.IsMouseButtonPressed(MouseButton.Left))
        {
            QueueFree();
        }
    }

    private void SetPivot()
    {
        var newSize = new Vector2(0, 0);
        PivotOffset = newSize;
    }
    public void SetParams(int type, string message, float duration)
    {
        _label.Text = message;
        if (type == 1)
        {
            _label.ThemeTypeVariation = "ErrorLabel";
        }
        _timer.SetWaitTime(duration);
        _timer.Start();
        _player.Play("ScaleUp");
    }
}
