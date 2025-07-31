using Godot;
using System;
using System.Collections.Generic;

public partial class Global : Node
{
    public static Global Instance { get; private set; }
    public List<MusicResource> MusicResources = new List<MusicResource>();
    public bool MusicListAlphabeticalSort = false;
    public string FirstDirectoryPath;
    public override void _Ready()
    {
        base._Ready();
        Instance = this;
    }
}
