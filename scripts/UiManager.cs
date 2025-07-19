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
        PackedScene _notificationScene;
        [Export]
        VBoxContainer _notificationContainer;
        [Export]
        VBoxContainer _musicListContainer;
        
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
                _currentSongTimeLabel.Text = ((int)(Player.GetPlaybackPosition())).ToString();
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
            _songTimeLabel.Text = ((int)(Player.Stream.GetLength())).ToString();
            //displaying song information
            _songNameLabel.Text = resource.Name;
            _artistLabel.Text = resource.Artist;
            _albumLabel.Text = resource.Album;
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
        }
        private void MinimizeButtonPressed()
        {
            GetTree().GetRoot().Mode = Window.ModeEnum.Minimized;
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
                GetTree().GetRoot().Position += (Vector2I)mouseMotion.Relative;
            }
        }
        
}
