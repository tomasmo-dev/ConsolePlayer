using System;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using System.Threading.Tasks;
using net.sf.jni4net;
using System.Diagnostics;
using System.Collections.Generic;

namespace ConsolePlayer
{

    internal class Program
    {

        const char black = ' ';//'\u2588'; // 'full' character
        const char white = 'X';//'█'; // black character

        static readonly int maxColumn = 852;//568; //maximum chars per line in console
        static readonly int maxLines = 243;//161; // max lines in console without scrolling

        const string AudioPath = @"..\..\..\ba.wav"; //path to file from executable
        const string VideoPath = @"..\..\..\ba.mp4"; //path to file from executable

        static int FPS = 30; // TEMP FPS FROM VIDEO FILE NOT THIS NUMBER

        const byte threshold = 1; // decision between 'white' and 'black' pixel
        const ushort iterations = 1; // how many erosion iterations

        static VideoCapture capture; //video reader
        static System.Windows.Forms.Timer timer; 

        static char[,] renderImage; // current drawn image

        static List<string> Frames = new();
        static int ptr = 0;

        static int frameCount = 0;

        static byte[] buffer;

        static Stream stdo = Console.OpenStandardOutput(); // old output variable

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int DWORD);
        [DllImport("kernel32.dll")]
        static extern int WriteFile(IntPtr handle, ref byte bytes, int BytesLength, out int BytesWritten, IntPtr mustbezero);   // better output method

        //static IntPtr StdHandle;
        public static IntPtr ChildHandle;

        //notepad width 852
        //notepad height 243

        static void Main(string[] args)
        {
            //ChildHandle = NotePlayer.Program.GetChildHandle();
            //NotePlayer.Program.exec();

            Initialize();
            LoadFrames();

            timer = new();

            timer.Interval = 1000 / FPS;
            timer.Tick += GetFrame;

            new Thread(PlayAudio)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            }.Start();

            timer.Start();

            //PlayBack();

            System.Windows.Forms.Application.Run();
        }

        private static void Initialize()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            Console.ReadKey(true);

            Console.OutputEncoding = Encoding.UTF8;

            ChildHandle = NotePlayer.Program.GetChildHandle();

            //StdHandle = GetStdHandle(-11);
            //Console.SetBufferSize(maxColumn, maxLines); //633 175

            char[,] pixels = new char[maxLines, maxColumn];

            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                for (int y = 0; y < pixels.GetLength(1); y++)
                {
                    pixels[i, y] = black;
                }
            }

            renderImage = pixels;

            capture = new(VideoPath);
            FPS = (int)capture.Get(Emgu.CV.CvEnum.CapProp.Fps);
            frameCount = (int)capture.Get(Emgu.CV.CvEnum.CapProp.FrameCount);
        }
        static void PlayAudio()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            SoundPlayer player = new(AudioPath);
            player.PlaySync();
        
        }


        private static void GetFrame(object sender, EventArgs e)
        {
            if (ptr == Frames.Count - 1) { timer.Stop(); Environment.Exit(0); }

            //buffer = Encoding.UTF8.GetBytes(Frames[ptr]);
            NotePlayer.Program.DrawFrame(Frames[ptr]);
            ptr++;

            //WriteFile(StdHandle, ref buffer[0], buffer.Length, out int _, IntPtr.Zero);
        }

        private static async Task PlayBack()
        {
            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < frameCount; i++)
            {
                sw.Restart();

                DrawFrame(i);
                await Task.Delay(50);//((int)(((double)FPS * (double)1000) - (double)sw.ElapsedMilliseconds) / 100);
            }
        }

        private static void DrawFrame(int i)
        {
            Console.SetCursorPosition(0, 0);
            string selectedFrame = Frames[i];
            Console.Write(selectedFrame);
        }

        private static void LoadFrames()
        {
            // save all frames to 'Frames' List
            var frame = capture.QueryFrame();

            int index = 0;
            while (frame != null)
            {
                var fr = frame.ToImage<Gray, byte>().Resize(maxColumn, maxLines, Emgu.CV.CvEnum.Inter.Nearest);

                StringBuilder sb = new StringBuilder();
                
                fr = fr.Erode(iterations);
                fr = fr.Canny(1, 1);

                for (int rows = 0; rows < fr.Rows; rows++)
                {
                    for (int cols = 0; cols < fr.Cols; cols++)
                    {
                        if (fr[rows, cols].Intensity >= threshold) sb.Append(white);
                        else sb.Append(black);
                    }
                    sb.Append('\n');
                }

                Frames.Add(sb.ToString());

                frame = capture.QueryFrame();

                double percentage = ((double)index / (double)frameCount) * 100;

                //Console.WriteLine($"Frame {index} out of {frameCount} | {percentage}");
                Console.Title = $"Frame {index} out of {frameCount} | {percentage}";
                index++;
            }

            Console.Clear();
            Console.Title = "Bad Apple";

        }
        private static void FramePlay(object sender, EventArgs e)
        {
            Image<Gray, byte> frame = capture.QueryFrame().ToImage<Gray, byte>().Resize(maxColumn, maxLines, Emgu.CV.CvEnum.Inter.Nearest); // get frame convert it to grayscale and resize to fit console

            if (frame != null)
            {
                TranslateToConsole(frame);
                Console.CursorVisible = !true; // disable console cursor
            }
            else
            {
                timer.Stop(); // stop video after it ends
            }
        }

        //private static void PlayVideo()
        //{
        //    capture = new VideoCapture(VideoPath);
        //    FPS = (int)capture.Get(Emgu.CV.CvEnum.CapProp.Fps);

        //    Image<Gray, byte> frame = capture.QueryFrame().ToImage<Gray, byte>().Resize(maxColumn, maxLines, Emgu.CV.CvEnum.Inter.Nearest);

        //    while (frame != null)
        //    {

        //        TranslateToConsole(frame);


        //        frame = capture.QueryFrame().ToImage<Gray, byte>().Resize(maxColumn, maxLines, Emgu.CV.CvEnum.Inter.Nearest);
        //        Thread.Sleep(1000 / FPS);
        //    }
        //}

        private static void TranslateToConsole(Image<Gray, byte> fr)
        {

            fr = fr.Erode(iterations);

            for (int i = 0; i < fr.Rows; i++)
            {
                for (int x = 0; x < fr.Cols; x++)
                {
                    if (fr[i, x].Intensity >= threshold)
                    {
                        renderImage[i, x] = white;
                        
                    }
                    else
                    {
                        renderImage[i, x] = black;
                    }
                }
            }
            Render();
        }

        

        static void Render()
        {
            StringBuilder sb = new();

            for (int line = 0; line < renderImage.GetLength(0); line++)
            {
                for (int column = 0; column < renderImage.GetLength(1); column++)
                {
                    sb.Append(renderImage[line, column]);

                    if (column + 1 == renderImage.GetLength(1)) sb.Append('\n');
                }
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            int written = 0;

            //WriteFile(StdHandle, ref buffer[0], buffer.Length, out written, IntPtr.Zero);

            //using (Stream stdout = Console.OpenStandardOutput(Console.BufferWidth * Console.BufferHeight))
            //{
            //    byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            //    stdout.Write(buffer, 0, buffer.Length);
            //}
            //await Console.Out.WriteAsync(sb.ToString());
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = false;


        }


    }

}
