using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows;
using System.Diagnostics;

namespace Clicker
{
    public partial class Clicker : Form
    {
        private Thread clickerThread;

        private int delaisMS;

        public Clicker()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, EventArgs e)
        {
            erreur.Text = "";

            if(clickerThread == null || !clickerThread.IsAlive)
            {
                if (!Int32.TryParse(textBox1.Text.ToString(), out delaisMS))
                {
                    erreur.Text = "Le délais doit être un nombre";
                    return;
                }
                _shouldStop = false;
                clickerThread = new Thread(new ThreadStart(DoWork));
                clickerThread.Start();
                button.Text = "Stop";
            }
            else
            {
                RequestStop();
                clickerThread.Join();
                button.Text = "Start";
            }
            
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern bool ScreenToClient(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string sClass, string sWindow);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll", SetLastError = false)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);


        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;

        public void DoWork()
        {
            

            // Détermination du handle de la fenêtre en fonction de son titre
            int handle_int = FindWindow(null, "Clicker Heroes");
            IntPtr handle = new IntPtr(handle_int);

            Console.Out.WriteLine(handle);
            if (handle == IntPtr.Zero)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    erreur.Text = "Clicker Heroes n'est pas lancé";
                });
            }
            

            // Conversion des deux 'int' en un 'Point'
            System.Drawing.Point coord_fenetre = new System.Drawing.Point(800, 400);

            // Transformation des coordonnées absolues en coordonnées relatives à la fenêtre
            ScreenToClient(handle, ref coord_fenetre);

            // Mise au point du lParam
            IntPtr lParam = new IntPtr(coord_fenetre.X | (coord_fenetre.Y << 16));
            IntPtr button = (IntPtr)Keys.LButton;

            while (!_shouldStop)
            {

                // Simulation du clic

                PostMessage(handle, WM_LBUTTONDOWN, button, lParam);
                PostMessage(handle, WM_LBUTTONUP, button, lParam);
                Thread.Sleep(delaisMS);
            }
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        private volatile bool _shouldStop;
    }
}
