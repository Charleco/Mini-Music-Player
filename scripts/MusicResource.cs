using Godot;
using System;
[GlobalClass]
public partial class MusicResource : Resource
{
    [Export]
    public string Path { get; set; }
    [Export]
    public string Name { get; set; }
    [Export]
    public string Author { get; set; }
    [Export]
    public string Album { get; set; }
    [Export]
    public Texture AlbumArt { get; set; }
    [Export]
    public int TrackNumber { get; set; }
}
