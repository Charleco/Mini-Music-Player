using Godot;
using System;
using static Global;
using static SignalBus;
public partial class FileManager : Node
{
    [Export]
    public Node UiManager;
    [Export]
    private Texture _defaultAlbumArtTexture;
    private FileDialog _fileDialog;
    private string _lastDirectoryPath;
    public override void _Ready()
    {
        SigBus.NewDirectorySelected += NewDirectorySelected;
        _fileDialog = GetNode<FileDialog>("FileDialog");
        _fileDialog.DirSelected += NewDirectorySelected;
        _lastDirectoryPath = "D:/Music/";
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
                    //GD.Print(fixedDir);
                    Texture2D albumArt = null;
                    using var tagFile = TagLib.File.Create(fixedDir);
                    var pictureData = tagFile.Tag.Pictures.Length > 0 ? tagFile.Tag.Pictures[0].Data.Data : null;
                    if (pictureData != null)
                    {
                        var albumImage = new Image();
                        var mimeType = tagFile.Tag.Pictures[0].MimeType.ToLower();
                        if (mimeType.Contains("jpeg") || mimeType.Contains("jpg"))
                        {
                            albumImage.LoadJpgFromBuffer(pictureData);
                            albumImage.Resize(200, 200);
                            albumArt = ImageTexture.CreateFromImage(albumImage);
                        }
                        else if (mimeType.Contains("png"))
                        {
                            albumImage.LoadPngFromBuffer(pictureData);
                            albumImage.Resize(200, 200);
                            albumArt = ImageTexture.CreateFromImage(albumImage);
                        }
                        else
                        {
                            albumArt = (Texture2D)_defaultAlbumArtTexture;
                        }
                    }
                    else
                    {
                        albumArt = (Texture2D)_defaultAlbumArtTexture;
                    }
                    var music = new MusicResource();
                    music.Path = directory + "/" + filename;
                    music.Name =  tagFile.Tag.Title ?? "unknown";
                    music.Artist = tagFile.Tag.FirstPerformer ?? "unknown";
                    music.Album = tagFile.Tag.Album ?? "unknown";
                    music.AlbumArt = albumArt;
                    music.TrackNumber = (int)tagFile.Tag.Track;
                    switch (filename.GetExtension())
                    {
                        case "mp3":
                            music.Extension = "mp3";
                            break;
                        case "ogg":
                            music.Extension = "ogg";
                            break;
                        case "wav":
                            music.Extension = "wav";
                            break;
                    }
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

        if(!DirAccess.DirExistsAbsolute(_lastDirectoryPath))
        {
            _fileDialog.SetCurrentPath("C:/");
            _fileDialog.SetVisible(true);
            return;
        }
        _fileDialog.SetCurrentPath(_lastDirectoryPath);
        GD.Print(_lastDirectoryPath);
        _fileDialog.SetVisible(true);
    }
}

