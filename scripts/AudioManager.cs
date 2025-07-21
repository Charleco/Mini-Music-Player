using Godot;
using System;
using System.Collections.Generic;
using static SignalBus;
public partial class AudioManager : Node
{
    
    
    [Export]
    private Button _playPauseButton;
    [Export]
    private Button _skipBackButton;
    [Export] 
    private Button _skipForwardButton;
    
    [Export]
    private Button _trackRepeatButton;
    [Export]
    private Button _repeatButton;
    
    public AudioStreamPlayer Player;
    
    public List<MusicResource> Queue = new List<MusicResource>();
    

    public override void _Ready()
    {
        Player = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        SigBus.MusicEntrySelected += MusicEntrySelected;
        
        //_playPauseButton.Pressed += _playPauseButtonPressed;
    }
    private void MusicEntrySelected(MusicResource resource)
    {
        SongChanged(resource);
        //setup queue
    }

    private void SongChanged(MusicResource resource)
    {
        switch (resource.Extension)
        {
            case "mp3":
                Player.Stream = LoadMp3(resource.Path);
                break;
            case "wav":
                Player.Stream = LoadWav(resource.Path);
                break;
            case "ogg":
                Player.Stream = LoadOggVorbis(resource.Path);
                break;
        }
        Player.Play();
        SigBus.EmitSignal(nameof(SigBus.SongChanged),resource);
    }
    private AudioStreamMP3 LoadMp3(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var sound = new AudioStreamMP3();
        sound.Data = file.GetBuffer((long)file.GetLength());
        return sound;
    }

    private AudioStreamWav LoadWav(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var sound = new AudioStreamWav();
        sound.Data = file.GetBuffer((long)file.GetLength());
        return sound;
    }

    private AudioStreamOggVorbis LoadOggVorbis(string path)
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var sound = new AudioStreamOggVorbis();
        sound = AudioStreamOggVorbis.LoadFromFile(path);
        return sound;
    }
    private void _playPauseButtonPressed()
    {
        if (Player.IsPlaying())
        {
            Player.StreamPaused = true;
            //_playPauseButton.Icon = play
        }
        else
        {
            Player.Play();
            //_playPauseButton.Icon = pause
        }
    }
    private void _skipBackButtonPressed()
    {
        if (Player.GetPlaybackPosition() > 1.0)
        {
            Player.Seek(0.0f);
        }
        else
        {
            //go to previous song
        }
    }
    private void _skipForwardButtonPressed()
    {
        //skip to next song
    }
    private void _trackRepeatButtonPressed()
    {
        //switch between no repeat, single repeat, whole repeat
    }
    private void _shuffleButtonPressed()
    {
        //toggle between shuffle and nonshuffle
    }
    private void SetDuration(float duration)
    {
        Player.StreamPaused = true;
        Player.Seek(0);
        Player.StreamPaused = false;
        Player.Play();
    }
    
}
