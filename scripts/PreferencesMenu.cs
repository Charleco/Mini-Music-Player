using Godot;
using System;

public partial class PreferencesMenu : Window
{
    [Export]
    private LineEdit _defaultDirectory;
    private Vector2 _visibleMousePosition;
    public override void _Ready()
    {
        VisibilityChanged += SetPosition;
    }

    private void SetPosition()
    {
        this.Position = GetTree().GetRoot().Position;
    }
    public override void _Input(InputEvent @event)
    {
        //FIXME: Some issue with rapid right clicks and no motion so _visMousePos isnt being set, right click released set mouse pos to center
        base._Input(@event);
        if(@event is InputEventMouseMotion mouseMotion && Input.IsMouseButtonPressed(MouseButton.Right))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Visible)
            {
                _visibleMousePosition = GetViewport().GetMousePosition();
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
            this.Position += (Vector2I)mouseMotion.Relative;
        }

        if (Input.MouseMode == Input.MouseModeEnum.Captured && !Input.IsMouseButtonPressed(MouseButton.Right))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            GetViewport().WarpMouse(_visibleMousePosition);
            var testEvent = new InputEventMouseMotion();
            testEvent.GlobalPosition = _visibleMousePosition;
            Input.ParseInputEvent(testEvent);
        }
    }

    public void SetResolution(int scale)
    {
        switch (scale)
        {
            case 1:
                ProjectSettings.SetSetting("display/window/size/window_width_override", 600);
                ProjectSettings.SetSetting("display/window/size/window_height_override", 400);
                DisplayServer.WindowSetSize(new Vector2I(600, 400), 0);
                
                break;
            case 2:
                ProjectSettings.SetSetting("display/window/size/window_width_override", 900);
                ProjectSettings.SetSetting("display/window/size/window_height_override", 600);
                DisplayServer.WindowSetSize(new Vector2I(900, 600), 0);
                break;
            case 3:
                ProjectSettings.SetSetting("display/window/size/window_width_override", 1200);
                ProjectSettings.SetSetting("display/window/size/window_height_override", 800);
                DisplayServer.WindowSetSize(new Vector2I(1200, 800), 0);
                break;
        }
    }
}
