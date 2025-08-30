using Godot;
using System;
using static SignalBus;
using static Global;

public partial class UiManager : Node
{
        [Export]
        public AudioStreamPlayer Player { get; set; }
        [Export]
        private PackedScene _musicEntryScene; 
        [Export]
        private PackedScene _notificationScene;
        [Export]
        private VBoxContainer _notificationContainer;
        [Export]
        private VBoxContainer _musicListContainer;
        [Export]
        private ScrollContainer _musicListScrollContainer;
        [Export]
        private Button _preferencesButton;
        [Export]
        private Window _preferencesWindow;
        
        [Export]
        private HSlider _volumeSlider;
        [Export]
        private Button _volumeButton;
        [Export] 
        private HSlider _durationSlider;
        [Export]
        private Label _currentSongTimeLabel;
        [Export]
        private Label _songTimeLabel;
        
        [Export]
        private Label _songNameLabel;
        [Export]
        private Label _artistLabel;
        [Export]
        private Label _albumLabel;
        [Export]
        private TextureRect _albumArtRect;

        [Export]
        private Texture2D _volume0Icon;
        [Export]
        private Texture2D _volume1Icon;
        [Export]
        private Texture2D _volume2Icon;
        [Export]
        private Texture2D _volume3Icon;
        [Export]
        private Button _playPauseButton;
        [Export]
        private Texture2D _playButtonTexture;
        [Export]
        private Texture2D _pauseButtonTexture;
        [Export]
        private Texture2D _defaultAlbumArtTexture;

        private Vector2 _visibleMousePosition;
        private float _previousVolume;
        
        private MusicEntry _currentMusicEntry;
        public override void _Ready()
        {
            SigBus.SendNotification += (type,message,duration) => SendNotification(type,message,duration);
            SigBus.SongChanged += (resource) => SongChanged(resource);
            _durationSlider.ValueChanged += (value) => Player.Seek((float)value);
            _volumeSlider.ValueChanged += (value) => SetVolume((float)value);
            SetVolume(0);
            _volumeButton.TooltipText = "Unmute";
            _previousVolume = 0.05f;
        }
        
        public override void _Process(double delta)
        {
            if (Player.Stream == null) return;
            _durationSlider.SetValueNoSignal(Player.GetPlaybackPosition());
            var totalSeconds = ((int)(Player.GetPlaybackPosition()));
            var seconds = (totalSeconds % 60);
            var minutes = totalSeconds / 60;
            _currentSongTimeLabel.Text = $"{minutes}:{seconds:D2}";
        }
        private void NewDirectorySelected()
        {
            foreach (var child in _musicListContainer.GetChildren())
                child.QueueFree();
            _songNameLabel.Text = "";
            _artistLabel.Text =  "";
            _albumLabel.Text =  "";
            _albumArtRect.Texture = _defaultAlbumArtTexture;
            _durationSlider.SetValueNoSignal(0.0);
            _currentSongTimeLabel.Text = "0:00";
            _songTimeLabel.Text = "0:00";
            _playPauseButton.Icon = _playButtonTexture;
        }

        private void PopulateMusicList()
        {
            foreach (var resource in Instance.MusicResources)
            {
                var entry = _musicEntryScene.Instantiate() as MusicEntry;
                entry.MusicResource = resource;
                _musicListContainer.AddChild(entry);
                _musicListContainer.UpdateMinimumSize();
                _musicListContainer.AddChild(new HSeparator());
            }
            //Delete Last HSep if songs were found
            if (_musicListContainer.GetChildren().Count > 0)
            { 
                _musicListContainer.RemoveChild(_musicListContainer.GetChild(_musicListContainer.GetChildCount()-1));
            }
            else
            {
                SendNotification(1, "No Music Files Found", 1.0f);
            }
            _musicListScrollContainer.SetDeferred("scroll_vertical",0);
            var musicCount = Instance.MusicResources.Count;
            SigBus.EmitSignal(nameof(SignalBus.SendNotification), 0, $"Found {musicCount} files", 1.5);
        }

        private void PreferencesButtonPressed()
        {
            if (_preferencesWindow.IsVisible())
                _preferencesWindow.Hide();
            else
                _preferencesWindow.Show();
        }

        private void SongChanged(MusicResource resource)
        {
            if (_currentMusicEntry != null && GodotObject.IsInstanceValid(_currentMusicEntry))
                _currentMusicEntry.ChangeLabels(false);
            _currentMusicEntry = FindMusicEntry(resource);
            _currentMusicEntry?.ChangeLabels(true);
            _durationSlider.MaxValue = Player.Stream.GetLength();
            var totalSeconds = ((int)(Player.Stream.GetLength()));
            var seconds = (totalSeconds % 60);
            var minutes = totalSeconds / 60;
            _songTimeLabel.Text = $"{minutes}:{seconds:D2}";
            //displaying song information
            _songNameLabel.Text = resource.Name;
            _artistLabel.Text = resource.Artist;
            _albumLabel.Text = resource.Album;
            _albumArtRect.Texture = (Texture2D)resource.AlbumArt;
        }
        private void VolumeButtonPressed()
        {
            if (_volumeSlider.Value != 0)
            {
                _previousVolume = (float)_volumeSlider.Value;
                _volumeButton.TooltipText = "Unmute";
                _volumeSlider.Value = 0.0;
                SetVolume(0f);
            }
            else
            {
                SetVolume(_previousVolume);
                _volumeSlider.Value = _previousVolume;
                _volumeButton.TooltipText = "Mute";
            }
        }

        private MusicEntry FindMusicEntry(MusicResource resource)
        {
            foreach (var child in _musicListContainer.GetChildren())
            {
                if (child is MusicEntry musicEntry && musicEntry.MusicResource == resource)
                {
                    return musicEntry;
                }
            }
            return null;
        }
        private void SetVolume(float volume)
        {
            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), Mathf.LinearToDb(volume));
            switch (volume)
            {
                case >0.67f:
                    _volumeButton.Icon = _volume3Icon;
                    break;
                case >0.34f:
                    _volumeButton.Icon = _volume2Icon;
                    break;
                case >0:
                    _volumeButton.Icon = _volume1Icon;
                    break;
                case 0:
                    _volumeButton.Icon = _volume0Icon;
                    break;
            }
            _volumeSlider.TooltipText = $"Volume: {Mathf.RoundToInt(volume * 100)}%";
            if (volume > 0)
                _volumeButton.TooltipText = "Mute";
            else
                _volumeButton.TooltipText = "Unmute";
        }
        private void MinimizeButtonPressed()
        {
            GetTree().GetRoot().Mode = Window.ModeEnum.Minimized;
        }

        private void ExitButtonPressed()
        {
            GetTree().Quit();
        }
        private void SendNotification(int type, string message, float duration)
        {
            var notification = _notificationScene.Instantiate() as Notification;
            _notificationContainer.AddChild(notification);
            _notificationContainer.MoveChild(notification,0);
            notification?.SetParams(type,message,duration);
        }
        public override void _Input(InputEvent @event)
        {
            base._Input(@event);
            if(@event is InputEventMouseMotion mouseMotion && Input.IsMouseButtonPressed(MouseButton.Right))
            {
                if (Input.MouseMode == Input.MouseModeEnum.Visible)
                {
                    _visibleMousePosition = GetViewport().GetMousePosition();
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
                GetTree().GetRoot().Position += (Vector2I)mouseMotion.Relative;
            }
            if (Input.MouseMode == Input.MouseModeEnum.Captured && !Input.IsMouseButtonPressed(MouseButton.Right))
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
                GetViewport().WarpMouse(_visibleMousePosition);
                var testEvent = new InputEventMouseMotion();
                testEvent.GlobalPosition = _visibleMousePosition;
                Input.ParseInputEvent(testEvent);
            }
            if (Input.IsActionPressed("VolumeUp"))
                _volumeSlider.Value += 2.0*_volumeSlider.Step;
            if (Input.IsActionPressed("VolumeDown"))
                _volumeSlider.Value -= 2.0*_volumeSlider.Step;

            if (Input.IsActionJustPressed("PlayPause"))
            {
                if (Player.Stream == null) return;
                if (Player.IsPlaying())
                {
                    Player.StreamPaused = true;
                    _playPauseButton.TooltipText = "Play";
                    _playPauseButton.Icon = _playButtonTexture;
                }
                else
                {
                    Player.StreamPaused = false;
                    _playPauseButton.TooltipText = "Pause";
                    _playPauseButton.Icon = _pauseButtonTexture;
                }
            }

            if (Input.IsActionPressed("ScrubForwards"))
            {
                if (Player.GetPlaybackPosition() < Player.Stream.GetLength()-6.0f)
                {
                    Player.Seek(Player.GetPlaybackPosition()+5.0f);
                }
                else
                {
                    Player.Seek((float)Player.Stream.GetLength()-1.0f);
                }
            }
            else if (Input.IsActionPressed("ScrubBackwards"))
            {
                if (Player.GetPlaybackPosition() > 6.0f)
                {
                    Player.Seek(Player.GetPlaybackPosition() - 5.0f);
                }
                else
                {
                    Player.Seek(0.0f);
                }
            }
        }
        
}
