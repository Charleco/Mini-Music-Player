using Godot;
using System;
using static Global;

public partial class PreferencesMenu : Window
{
    [Export]
    private LineEdit _defaultDirectory;
    private Vector2 _visibleMousePosition;
    public override void _Ready()
    {
        VisibilityChanged += SetPosition;
    }

    private void ExitButtonPressed()
    {
        Visible = false;
    }

    private void SetPosition()
    {
        this.Position = GetTree().GetRoot().Position;
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

    private void SetResolution(int scale)
    {
        _configFile.SetValue("Preferences", "Scale", scale);
        switch (scale)
        {
            
            case 1:
                DisplayServer.WindowSetSize(new Vector2I(600, 400), 0);
                ContentScaleFactor = 1.0f;
                Size = new Vector2I(400, 300);
                break;
            case 2:
                DisplayServer.WindowSetSize(new Vector2I(900, 600), 0);
                Size = new Vector2I(600, 450);
                ContentScaleFactor = 1.5f;
                break;
            case 3:
                DisplayServer.WindowSetSize(new Vector2I(1200, 800), 0);
                Size = new Vector2I(800, 600);
                ContentScaleFactor = 2.0f;
                break;
        }
        _configFile.Save("user://pref.cfg");
    }

    private void SortingButtonPressed(int sortingOrder)
    {
        if (sortingOrder == 0)
        {
            Instance.MusicListAlphabeticalSort = true;
        }
        //sort by tracknumber
        else if (sortingOrder == 1)
        {
            Instance.MusicListAlphabeticalSort = false;
        }
            
    }

    private void DirectoryLineEditChanged(string newText)
    {
        Instance.FirstDirectoryPath = ProjectSettings.GlobalizePath(newText) + "/";
        GD.Print(newText);
        GD.Print("directory changed");
        
    }
    
}
