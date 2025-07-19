using Godot;
using System;
using static SignalBus;
public partial class MusicEntry : HBoxContainer
{
    public MusicResource MusicResource { get; set; }
    private Label _nameLabel;
    private Label _artistLabel;
    private Label _albumLabel;

    public override void _Ready()
    {
        _nameLabel = GetNode<Label>("SongNameLabel");
        _artistLabel = GetNode<Label>("ArtistLabel");
        _albumLabel = GetNode<Label>("AlbumLabel");
        
        _nameLabel.Text = MusicResource.Name;
        _artistLabel.Text = MusicResource.Artist;
        _albumLabel.Text = MusicResource.Album;
    }
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.DoubleClick)
        {
            SigBus.EmitSignal(nameof(SigBus.MusicEntrySelected), MusicResource);
        }
    }
}
