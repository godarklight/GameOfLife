using System;
using System.Threading;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace GameOfLife
{
    class MainWindow : Window
    {
        //[UI] private Button _button1 = null;
        [UI] private Image DrawArea = null;
        private AutoResetEvent simulatorSync = new AutoResetEvent(false);
        LifeSimulator lifeSimulator;




        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            lifeSimulator = new LifeSimulator(simulatorSync, UpdateFrame);
            builder.Autoconnect(this);
            DeleteEvent += Window_DeleteEvent;
        }

        private void Window_DeleteEvent(object sender, DeleteEventArgs a)
        {
            lifeSimulator.Stop();
            Application.Quit();
        }

        private void UpdateFrame(byte[] drawData)
        {
            Application.Invoke(delegate { DrawArea.Pixbuf = new Gdk.Pixbuf(drawData, Gdk.Colorspace.Rgb, true, 8, Program.SIZEX, Program.SIZEY, Program.SIZEX * 4, null); simulatorSync.Set();});            
        }
    }
}
