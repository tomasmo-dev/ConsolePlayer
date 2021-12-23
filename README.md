# ConsolePlayer

# Only works on Windows!

Plays bad apple in console or notepad window

- warning in console mode audio gets desynced BY A LOT due to c#'s slow console output methods
- Notepad is faster but not perfect either


- ba.mp4 is for loading frames from video
- ba.wav is only audio for win sound player

# Changing output mode
to change from notepad output to console find :

    private static void GetFrame(object sender, EventArgs e)
        {
            if (ptr == Frames.Count - 1) { timer.Stop(); Environment.Exit(0); }

            //buffer = Encoding.UTF8.GetBytes(Frames[ptr]);
            NotePlayer.Program.DrawFrame(Frames[ptr]);
            ptr++;

            //WriteFile(StdHandle, ref buffer[0], buffer.Length, out int _, IntPtr.Zero);
        }
       
  and un-comment buffer = ... and WriteFile(...
  then comment NotePlayer.Program.... 
  leave the rest as is
