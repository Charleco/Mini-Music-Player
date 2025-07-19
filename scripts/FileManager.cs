using Godot;
using System;
using static Global;
using static SignalBus;
public partial class FileManager : Node
{
    [Export]
    public Node UiManager;
    private FileDialog _fileDialog;
    private string _lastDirectoryPath;
    public override void _Ready()
    {
        SigBus.NewDirectorySelected += NewDirectorySelected;
        _fileDialog = GetNode<FileDialog>("FileDialog");
        _fileDialog.DirSelected += NewDirectorySelected;
        _lastDirectoryPath = "D:";
    }

    private void NewDirectorySelected(string directory)
    {
        GD.Print("New directory selected: " + directory);
        _lastDirectoryPath = directory;
        var openDir = DirAccess.Open(directory);
        if (openDir != null)
        {
            SigBus.EmitSignal(nameof(SigBus.SendNotification),0,"Fetching Music...",2);
            Instance.MusicResources.Clear();
            openDir.SetIncludeNavigational(false);
            openDir.SetIncludeHidden(false);
            openDir.ListDirBegin();
            var filename = openDir.GetNext();
            while (filename != "" && filename != "." && filename != "..")
            {
                if (filename.GetExtension() == "mp3" || filename.GetExtension() == "ogg" ||
                    filename.GetExtension() == "wav")
                {
                    var fixedDir = ProjectSettings.GlobalizePath(directory+"/"+filename);
                    GD.Print(fixedDir);
                    using var tfile = TagLib.File.Create(fixedDir);
                    var music = new MusicResource();
                    music.Path = directory + "/" + filename;
                    music.Name =  tfile.Tag.Title ?? "unknown";
                    music.Artist = tfile.Tag.FirstPerformer ?? "unknown";
                    music.Album = tfile.Tag.Album ?? "unknown";
                    //music.AlbumArt = albumArt;
                    music.TrackNumber = (int)tfile.Tag.Track;
                    Instance.MusicResources.Add(music);
                }
                filename = openDir.GetNext();
                
            }
        }
        else
        {
            SigBus.EmitSignal(nameof(SigBus.SendNotification),1,"Directory is not valid, "+DirAccess.GetOpenError(), 2);
        }

        UiManager.Call("PopulateMusicList");
    }

    private void ShowFileDialog()
    {
        _fileDialog.SetVisible(true);
        _fileDialog.SetCurrentDir(_lastDirectoryPath);
    }
}

