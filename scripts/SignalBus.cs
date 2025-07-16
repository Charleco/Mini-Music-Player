using Godot;
using System;

public partial class SignalBus : Node
{
    public static SignalBus Instance { get; private set; }
    [Signal]
    public delegate void SendNotificationEventHandler(int type, string message,float duration);
    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }
    
}
