   using Godot;
using System;

public partial class SignalBus : Node
{
    public static SignalBus SigBus { get; private set; }
    [Signal]
    public delegate void SendNotificationEventHandler(int type, string message,float duration);
    [Signal]
    public delegate void NewDirectorySelectedEventHandler(string directory);
    [Signal]
    public delegate void MusicEntrySelectedEventHandler(MusicResource resource);
    [Signal]
    public delegate void SongChangedEventHandler(MusicResource resource);
    public override void _Ready()
    {
        base._Ready();
        SigBus = this;
    }
}
