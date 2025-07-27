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

        private Vector2 _visibleMousePosition;
        
        public override void _Ready()
        {
            SigBus.SendNotification += (type,message,duration) => SendNotification(type,message,duration);
            SigBus.SongChanged += (resource) => SongChanged(resource);
            
            _durationSlider.ValueChanged += (value) => Player.Seek((float)value);
            _volumeSlider.ValueChanged += (value) => SetVolume((float)value);
            SetVolume(0);
        }
        public override void _Process(double delta)
        {
            if (Player.Stream != null)
            {
                _durationSlider.SetValueNoSignal(Player.GetPlaybackPosition());
                int totalSeconds = ((int)(Player.GetPlaybackPosition()));
                int seconds = (totalSeconds % 60);
                int minutes = totalSeconds / 60;
                _currentSongTimeLabel.Text = $"{minutes}:{seconds:D2}";
            }
        }

        public void PopulateMusicList()
        {
            foreach (var child in _musicListContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var resource in Instance.MusicResources)
            {
                var entry = _musicEntryScene.Instantiate() as MusicEntry;
                entry.MusicResource = resource;
                _musicListContainer.AddChild(entry);
                _musicListContainer.UpdateMinimumSize();
                _musicListContainer.AddChild(new HSeparator());
            }
            //gets rid of last HSeparator
            _musicListContainer.RemoveChild(_musicListContainer.GetChild(_musicListContainer.GetChildCount()-1));
        }

        private void SongChanged(MusicResource resource)
        {
            _durationSlider.MaxValue = Player.Stream.GetLength();
            int totalSeconds = ((int)(Player.Stream.GetLength()));
            int seconds = (totalSeconds % 60);
            int minutes = totalSeconds / 60;
            _songTimeLabel.Text = $"{minutes}:{seconds:D2}";
            //displaying song information
            _songNameLabel.Text = resource.Name;
            _artistLabel.Text = resource.Artist;
            _albumLabel.Text = resource.Album;
            _albumArtRect.Texture = (Texture2D)resource.AlbumArt;
        }
        
        private void PinButtonPressed()
        {
            if(GetWindow().AlwaysOnTop)
            {
                SigBus.EmitSignal(nameof(SigBus.SendNotification), 0, "Unpinned Window", 1);
                GetWindow().AlwaysOnTop = false;
            }
            else
            {
                SigBus.EmitSignal(nameof(SigBus.SendNotification), 0, "Pinned Window", 1);
                GetWindow().AlwaysOnTop = true;
            }
        }
        private void VolumeButtonPressed()
        {
            SetVolume(0f);
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
            notification.SetParams(type,message,duration);
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
            }
        }
        
}
