using Godot;
using System;
using static Global;

public partial class PreferencesMenu : Window
{
    [Export]
    private LineEdit _defaultDirectory;
    private Vector2 _visibleMousePosition;
    private ConfigFile _configFile;
    public override void _Ready()
    {
        VisibilityChanged += SetPosition;
        LoadPreferences();
    }

    private void ExitButtonPressed()
    {
        Visible = false;
    }

    private void LoadPreferences()
    {
        _configFile = new ConfigFile();
        Error err = _configFile.Load("user://pref.cfg");
        if (err != Error.Ok)
        {
            return;
        }
        SetResolution((int)_configFile.GetValue("Preferences", "Scale", 1));
        Instance.FirstDirectoryPath = (string)_configFile.GetValue("Preferences", "DefaultDirectory","C:/");
        Instance.MusicListAlphabeticalSort = (bool) _configFile.GetValue("Preferences", "DefaultSort", false);
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
    }

    private void SortingButtonPressed(int sortingOrder)
    {
        //sort by name
        if (sortingOrder == 0)
        {
            Instance.MusicListAlphabeticalSort = true;
        }
        //sort by tracknumber
        else if (sortingOrder == 1)
        {
            Instance.MusicListAlphabeticalSort = false;
        }
        _configFile.SetValue("Preferences", "DefaultSort", Instance.MusicListAlphabeticalSort);
    }

    private void DirectoryLineEditChanged(string newText)
    {
        Instance.FirstDirectoryPath = ProjectSettings.GlobalizePath(newText) + "/";
        _configFile.SetValue("Preferences", "DefaultDirectory", Instance.FirstDirectoryPath);
    }

    private void SavePreferences()
    {
        _configFile.Save("user://pref.cfg");
    }
    
}
