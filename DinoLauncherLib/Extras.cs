﻿namespace DinoLauncherLib;

public class Extras
{
    // This does nothing for now
    // It will do something later, but right now it does nothing

    //FileIO fileIO;   // Use to reference and perform directory/file functions
    //Xdelta3 xdelta3; // Use to do patching stuff
    //Git git;         // Use to do git stuff

    //WindowsMediaPlayer mediaPlayer;

    //public void PlayMusic(string path)
    //{
    //    // If using Windows, it's easier to just use WMP
    //    // It seems each OS will require its own media player, which is fine because I don't really care
    //    // But we can prepare for it anyway
    //    //#if WINDOWS
    //    if (File.Exists(path))
    //    {
    //        Debug.WriteLine("Playing file at " + path);
    //        // Would like to figure out how to play from an embedded file instead of a local one
    //        mediaPlayer.URL = (path);

    //        // Volume can be any integer between 0 and 100
    //        mediaPlayer.settings.volume = 50; // Play music at half volume because loud music is annoying AF
    //        mediaPlayer.controls.play();
    //    }
    //    else
    //    {
    //        Debug.WriteLine("No file found at " + path);
    //    }
    //    //#endif
    //}

    public void PauseMusic()
    {
        //mediaPlayer.controls.pause();
    }

    public void StopMusic()
    {
        //mediaPlayer.controls.stop();
    }
}