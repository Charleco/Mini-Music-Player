using Godot;
using System;
using static SignalBus;

public partial class UiManager : Node
{
        [Export]
        PackedScene _notificationScene;
        [Export]
        VBoxContainer _notificationContainer;
        public override void _Ready()
        {
            SigBus.SendNotification += (type,message,duration) => SendNotification(type,message,duration);
        }
        public override void _Input(InputEvent @event)
        {
            
            base._Input(@event);
            if(@event is InputEventMouseMotion mouseMotion && Input.IsMouseButtonPressed(MouseButton.Right))
            {
                GetTree().GetRoot().Position += (Vector2I)mouseMotion.Relative;
            }
            
        }
        private void SendNotification(int type, string message, float duration)
        {
            var notification = _notificationScene.Instantiate() as Notification;
            _notificationContainer.AddChild(notification);
            _notificationContainer.MoveChild(notification,0);
            notification.SetParams(type,message,duration);
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

        private void MinimizeButtonPressed()
        {
            GetTree().GetRoot().Mode = Window.ModeEnum.Minimized;
        }
}
