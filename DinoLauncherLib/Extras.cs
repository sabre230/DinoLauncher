namespace DinoLauncherLib;

public class Extras
{
    public string[] randomStatusMessages =
    {
        "Two roots is never enough!", // Random Snowhorn meme I guess
        "It's good to see you, Kyte.", // Krystal
        "My name is Krystal, and I'm here for the Princess!", // Krystal
        "Can we go to Cloudrunner Fortress now?", // Kyte
        "So this is Discovery Falls!", // Kyte
        "The Quan Ata Lachu are not what you say!", // Sabre
        "My name is Sabre, royal knight from the planet Animus.", // Sabre
        "Hey, look at this!", // Tricky
        "My dad's a King Earthwalker and he'll bash you up!", // Tricky
        "You must go to Discovery Falls, you will find some answers there.", // Randorn
        "My dear, Dinosaur Planet is in extreme danger.", // Randorn
        "Who might you be, animal girl?", //Scales
        "I can't believe the wizard sent a girl to do his dirty work.", // Scales
        "Do not hit Sharpclaw. Me give you Food Bag if you let me go!", // Sharpclaw
        "No food been delivered, me starving!", // Sharpclaw
        "You will find us all, we must continue!", // Quan Ata Lachu
        "I can taste your fear, animal. It is very sweet.", // Drakor
        "As you wish. If you will not see the truth, then you must die!" // Drakor
    };
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