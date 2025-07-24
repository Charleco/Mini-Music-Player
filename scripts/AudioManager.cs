using Godot;
using System;
using System.Collections.Generic;
using static SignalBus;
using static Global;
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
    private Button _shuffleButton;
    
    private AudioStreamPlayer _player;
    private List<MusicResource> _queue = new List<MusicResource>();
    private bool _shuffleToggle;
    private MusicResource _nextSong;
    private MusicResource _currentSong;
    private enum TrackRepeat
    {
        NoRepeat,
        SingleTrackRepeat,
        PlaylistRepeat
    }
    TrackRepeat _currentTrackRepeat;
    

    public override void _Ready()
    {
        _player = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        SigBus.MusicEntrySelected += MusicEntrySelected;
        _currentTrackRepeat = TrackRepeat.NoRepeat;
        _playPauseButton.Pressed += _playPauseButtonPressed;
    }
    
    //user clicks music entry, starts new queue
    private void MusicEntrySelected(MusicResource resource)
    {
        SetupQueue(resource);
        SongChanged(resource);
    }

    private void SetupQueue(MusicResource resource)
    {
        _queue.Clear();
        var startIndex = Instance.MusicResources.IndexOf(resource);
        _currentSong = resource;
        
        if (_shuffleToggle)
        {
            
        }
        else
        {
            _queue.AddRange(Instance.MusicResources.GetRange(startIndex, Instance.MusicResources.Count - startIndex));
            if (_currentTrackRepeat == TrackRepeat.PlaylistRepeat && startIndex > 0)
            {
                _queue.AddRange(Instance.MusicResources.GetRange(0, startIndex));
            }
        }
    }
    private void SetNextSong()
    {
        if (_currentTrackRepeat == TrackRepeat.SingleTrackRepeat)
        {
            _nextSong = _currentSong;
        }
        else
        {
            if (_queue.IndexOf(_currentSong)+1 < _queue.Count-1)
            {
                _nextSong = _queue[_queue.IndexOf(_currentSong)+1];
            }
            else if (_currentTrackRepeat == TrackRepeat.PlaylistRepeat)
            {
                SetupQueue(_currentSong);
            }
            else
            {
                _nextSong = null;
            }
            
        }
    }
    // connected to Player.Finished(), can also be triggered by the skip ahead button
    private void PlayNextSong()
    {
        if(_nextSong != null)
        {
            SongChanged(_nextSong);
        }
    }
    
    //load song, tell UI to display the song info via signal, set next song
    private void SongChanged(MusicResource resource)
    {
        switch (resource.Extension)
        {
            case "mp3":
                _player.Stream = LoadMp3(resource.Path);
                break;
            case "wav":
                _player.Stream = LoadWav(resource.Path);
                break;
            case "ogg":
                _player.Stream = LoadOggVorbis(resource.Path);
                break;
        }
        
        _player.Seek(0.0f);
        _player.Play();
        SigBus.EmitSignal(nameof(SigBus.SongChanged),resource);
        SetNextSong();
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
        var sound = AudioStreamOggVorbis.LoadFromFile(path);
        return sound;
    }
    private void _playPauseButtonPressed()
    {
        if (_player.Stream != null)
        {
            if (_player.IsPlaying())
            {
                _player.StreamPaused = true;
                //_playPauseButton.Icon = play
            }
            else
            {
                _player.Play();
                //_playPauseButton.Icon = pause
            }
        }
    }
    private void _skipBackButtonPressed()
    {
        if (_player.GetPlaybackPosition() > 1.0)
        {
            _player.Seek(0.0f);
        }
        else
        {
            //go to previous song
        }
    }
    private void _skipForwardButtonPressed()
    {
        PlayNextSong();
    }
    private void _trackRepeatButtonPressed()
    {
        //switch between no repeat, single repeat, whole repeat
        switch (_currentTrackRepeat)
        {
            case TrackRepeat.NoRepeat:
                _currentTrackRepeat = TrackRepeat.SingleTrackRepeat;
                SetNextSong();
                break;
            case TrackRepeat.SingleTrackRepeat:
                _currentTrackRepeat = TrackRepeat.PlaylistRepeat;
                break;
            case TrackRepeat.PlaylistRepeat:
                _currentTrackRepeat = TrackRepeat.NoRepeat;
                break;
        }

        if (_currentSong != null)
        {
            SetupQueue(_currentSong);
        }
    }
    private void _shuffleButtonPressed()
    {
        _shuffleToggle = !_shuffleToggle;
        SetNextSong();
    }
    private void SetDuration(float duration)
    {
        _player.StreamPaused = true;
        _player.Seek(0);
        _player.StreamPaused = false;
        _player.Play();
    }
    
}
