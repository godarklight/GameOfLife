using System;
using Gtk;

namespace GameOfLife
{
    class Program
    {
        public const int SIZEX = 1280;
        public const int SIZEY = 1280;

        [STAThread]
        public static void Main(string[] args)
        {
            Application.Init();

            var app = new Application("org.GameOfLife.GameOfLife", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();
            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}
