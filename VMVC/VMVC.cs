using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using VMVC.Properties;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Security.Permissions;
using System.Timers;
using System.Windows.Input;
using System.Collections.Generic;

namespace VMVC
{
    public partial class VMVC : Form
    {
        private const string V = "VMVC.exe", V1 = "Segoe UI";
        private static string normal = "Normal", ra = "Ra", megafon = "Megafon", overlay = "Overlay";
        public static Button[] Buttons = new Button[4];
        public static Label[] BtLabels = new Label[4];
        public static bool Overlay_Button_Clicked = false;

        public static Overlay Overlay = null;
        public static Overlay2 Overlay2 = null;
        public static Overlay3 Overlay3 = null;
        public static TextBox TB, TB2, switch1, switch2, switch3, switch4, switch5, switch6, switch7, switch8, switch9, switch10;
        public static TextBox TB3, TB4, switch11, switch12, switch13, switch14, switch15, switch16, switch17, switch18, switch19, switch20;
        public static Label lb3, lb4, lb7, lb8, lb9, lb10, lb17, lb18, lb19, lb20;

        Process[] processes;

        private static int _ButtonState;
        public static bool _CapsLockPressed = false;
        private static int overlayIsActivated = 1;
        public static bool _Hooked = false;

        int swi = 0, swi2 = 0;
        bool isRunning = true;
        public Thread TH = null;
        public static bool _KEYSEND = false, _Macro = false, _caps = false, _add = false;
        public TimeSpan grt;
        public static double ic = 0.000;

        public static int ButtonState
        {
            get { return _ButtonState; }
            set
            {
                _ButtonState = value;
                if (Overlay_Button_Clicked)
                {
                    if (Overlay.Opacity > 0 && Overlay2.Opacity > 0)
                    {
                        OverlayThreads();
                    }
                }
            }
        }

        public static bool CapsLockPressed
        {
            get { return _CapsLockPressed; }
            set
            {
                _CapsLockPressed = value;

                if (Control.IsKeyLocked(Keys.CapsLock) && !_CapsLockPressed)
                {
                    ToggleCapsLock.SetCapsLock(false);
                }

                if (Overlay_Button_Clicked)
                {
                    if (Overlay.Opacity > 0 && Overlay2.Opacity > 0)
                    {
                        OverlayThreads();
                    }
                }
            }
        }

        private static void OverlayThreads()
        {
            Overlay.sDXThread();
            Overlay2.sDXThread();
        }

        public VMVC()
        {
            InitializeComponent();
            GameOverlays();
            this.FormClosing += Form1_Closing;
            MaximizeBox = false;
            MinimizeBox = false;
            GC.Collect();
            //Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)12;
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
        }

        private void CheckCantabile()
        {
            processes = Process.GetProcessesByName("Cantabile");

            foreach (Process process in processes)
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle))
                {
                    string[] words = Regex.Split(process.MainWindowTitle, "VoiceMeeter");
                    State(words[1]);
                }
            }
        }

        private static void State(string state)
        {
            switch(state)
            {
                case "Normal":
                    ButtonColor(0, 60, 198, 73, 0, 0, 0);
                    ButtonColor(1, 74, 74, 74, 255, 255, 255);
                    ButtonColor(2, 74, 74, 74, 255, 255, 255);
                    ButtonState = 0;
                    break;
                case "Ra":
                    ButtonColor(0, 74, 74, 74, 255, 255, 255);
                    ButtonColor(1, 60, 198, 73, 0, 0, 0);
                    ButtonColor(2, 74, 74, 74, 255, 255, 255);
                    ButtonState = 1;
                    break;
                case "Megafon":
                    ButtonColor(0, 74, 74, 74, 255, 255, 255);
                    ButtonColor(1, 74, 74, 74, 255, 255, 255);
                    ButtonColor(2, 60, 198, 73, 0, 0, 0);
                    ButtonState = 2;
                    break;
                default:
                    break;
            }
        }

        private static void OverlayIsActivated(int value)
        {
            if (Convert.ToBoolean(value))
            {
                ButtonColor(3, 240, 130, 33, 0, 0, 0);
                overlayIsActivated = 0;
            }
            else
            {
                ButtonColor(3, 74, 74, 74, 255, 255, 255);
                overlayIsActivated = 1;
            }
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            this.RestoreWindowPosition();
            this.CreateButtons();
            StartListener();
            CheckCantabile();
            try
            {
                Overlay.SetOverlayForegrounded();
                Overlay2.SetOverlayForegrounded();
                Overlay3.SetOverlayForegrounded();
            }
            catch(Exception Ex)
            {
                //MessageBox.Show("GTA5 is not running!");
            }

        }

        public void GameOverlays()
        {
            Overlay = new Overlay();
            Overlay2 = new Overlay2();
            Overlay3 = new Overlay3();
            Overlay.LoadDevice();
            Overlay2.LoadDevice();
            Overlay3.LoadDevice();
        }

        public bool Hooked
        {
            get { return _Hooked; }
            set
            {
                _Hooked = value;
            }
        }

        private void StartListener()
        {

        }
               
        private static void AppRestart()
        {
            Process CantabileFile = new Process();
            CantabileFile.StartInfo.FileName = Application.StartupPath + @"\" + V;
            CantabileFile.StartInfo.CreateNoWindow = true;
            CantabileFile.Start();
            Environment.Exit(0);
        }

        private static void CantabileFile(string Name)
        {
            Process CantabileFile = new Process();
            CantabileFile.StartInfo.FileName = Application.StartupPath + @"\Cantabile\VoiceMeeter" + Name + ".cantabileSong";
            CantabileFile.StartInfo.CreateNoWindow = true;
            CantabileFile.Start();
        }
        
        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.SaveWindowPosition();

            Overlay.Close();
            Overlay2.Close();
            Overlay3.Close();

            Environment.Exit(0);
            this.Close();
        }

        private void RestoreWindowPosition()
        {
            if (Settings.Default.HasSetDefaults)
            {
                this.WindowState = Settings.Default.WindowState;
                this.Location = Settings.Default.Location;
                this.Size = Settings.Default.Size;
            }
        }

        private void SaveWindowPosition()
        {
            Settings.Default.WindowState = this.WindowState;

            if (this.WindowState == FormWindowState.Normal)
            {
                Settings.Default.Location = this.Location;
                Settings.Default.Size = this.Size;
            }
            else
            {
                Settings.Default.Location = this.RestoreBounds.Location;
                Settings.Default.Size = this.RestoreBounds.Size;
            }

            Settings.Default.HasSetDefaults = true;
            Settings.Default.Save();
        }

        public void CreateButtons()
        {
            TextField();
            TextField2();
            CreateSwitch();
            CreateSwitch2();
            TrackBar();

            Button(0, normal, 31, 31, normal_Click);
            Button(1, ra, 31, 73, ra_Click);
            Button(2, megafon, 31, 115, mega_Click);
            Button(3, overlay, 31, 157, OverlayB_Click);

            Label(0, 30, 30);
            Label(1, 30, 72);
            Label(2, 30, 114);
            Label(3, 30, 156);

            Buttons[3].Enabled = false;
        }

        public void Label(int i, int LocationX, int LocationY)
        {
            try
            {
                BtLabels[i] = new Label
                {
                    Size = new Size(150, 40),
                    Location = new Point(LocationX, LocationY),
                    Font = new Font(V1, 12),
                    BackColor = Color.FromArgb(100, 100, 100),
                    //FlatStyle = FlatStyle.Flat
                };
                BtLabels[i].TabStop = false;

                this.Controls.Add(BtLabels[i]);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        public void Button(int i, string Name, int LocationX, int LocationY, EventHandler Click)
        {
            try
            {
                Buttons[i] = new Button
                {
                    Size = new Size(148, 38),
                    Location = new Point(LocationX, LocationY),
                    Text = Name,
                    Font = new Font(V1, 12),
                    BackColor = Color.FromArgb(74, 74, 74),
                    FlatStyle = FlatStyle.Flat
                };
                Buttons[i].FlatAppearance.BorderSize = 0;
                Buttons[i].TabStop = true;
                Buttons[i].ForeColor = Color.FromArgb(255, 255, 255);
                Buttons[i].FlatAppearance.CheckedBackColor = Color.FromArgb(0, 0, 0);
                this.Controls.Add(Buttons[i]);
                Buttons[i].Click += new EventHandler(Click);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        public void lb3_Click(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(this.trackBar1.Value))
            {
                TurnOn();
            }
        }

        public void TextField()
        {
            lb3 = new Label();
            lb3.Size = new Size(150, 30);
            lb3.Location = new Point(32, 214);
            lb3.BackColor = Color.FromArgb(74, 74, 74);
            lb3.ForeColor = Color.FromArgb(255, 255, 255);
            lb3.Font = new Font("Segoe UI", 9.2f);
            lb3.TextAlign = ContentAlignment.MiddleCenter;
            lb3.Width = 146;
            lb3.Height = 23;
            lb3.Text = "Keyboard not Hooked";
            lb3.Click += lb3_Click;
            this.Controls.Add(lb3);

            TB = new TextBox();
            TB.Size = new Size(150, 30);
            TB.Location = new Point(31, 213);
            TB.BackColor = Color.FromArgb(74, 74, 74);
            TB.TabStop = false;
            TB.ForeColor = Color.FromArgb(255, 255, 255);
            TB.BorderStyle = BorderStyle.FixedSingle;
            TB.Font = new Font("Segoe UI", 10);
            TB.TextAlign = HorizontalAlignment.Center;
            TB.Width = 148;
            TB.Height = 30;
            TB.Enabled = false;
            TB.Text = "";
            TB.Click += new EventHandler(TB_Clicked);
            this.Controls.Add(TB);
        }

        public void lb4_Click(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(this.trackBar1.Value))
            {
                grt = new TimeSpan();
                lb4.Text = "00:00:00";
            }
        }
        
        public void TextField2()
        {
            lb4 = new Label();
            lb4.Size = new Size(150, 30);
            lb4.Location = new Point(32, 242);
            lb4.BackColor = Color.FromArgb(74, 74, 74);
            lb4.ForeColor = Color.FromArgb(255, 255, 255);
            lb4.Font = new Font("Segoe UI", 9.2f);
            lb4.TextAlign = ContentAlignment.MiddleCenter;
            lb4.Width = 146;
            lb4.Height = 23;
            lb4.Text = "00:00:00";
            lb4.Click += lb4_Click;
            this.Controls.Add(lb4);

            TB2 = new TextBox();
            TB2.Size = new Size(150, 30);
            TB2.Location = new Point(31, 241);
            TB2.BackColor = Color.FromArgb(74, 74, 74);
            TB2.TabStop = false;
            TB2.ForeColor = Color.FromArgb(255, 255, 255);
            TB2.BorderStyle = BorderStyle.FixedSingle;
            TB2.Font = new Font("Segoe UI", 10);
            TB2.TextAlign = HorizontalAlignment.Center;
            TB2.Width = 148;
            TB2.Height = 30;
            TB2.Enabled = false;
            TB2.Text = "";
            TB2.Click += new EventHandler(TB_Clicked);
            this.Controls.Add(TB2);
        }

        public void CreateSwitch()
        {
            lb8 = new Label();
            lb8.BackColor = Color.FromArgb(50, 50, 50);
            lb8.Cursor = System.Windows.Forms.Cursors.Arrow;
            lb8.Name = "SwitchButtonStripe1";
            lb8.Font = new Font("Segoe UI", 0.5f);
            //lb8.TextAlign = HorizontalAlignment.Center;
            //lb8.ReadOnly = true;
            lb8.Width = 55;
            lb8.Height = 1;
            lb8.Location = new Point(42, 275);
            lb8.TabStop = false;
            //lb8.Enabled = false;
            lb8.Click += lb3_Click;
            lb8.BorderStyle = BorderStyle.None;
            this.Controls.Add(lb8);

            lb9 = new Label();
            lb9.BackColor = Color.FromArgb(50, 50, 50);
            lb9.Cursor = System.Windows.Forms.Cursors.Arrow;
            lb9.Name = "SwitchButtonStripe2";
            lb9.Font = new Font("Segoe UI", 0.5f);
            //lb9.TextAlign = HorizontalAlignment.Center;
            //lb9.ReadOnly = true;
            lb9.Width = 55;
            lb9.Height = 1;
            lb9.Location = new Point(42, 278);
            lb9.TabStop = false;
            //lb9.Enabled = false;
            lb9.Click += lb3_Click;
            lb9.BorderStyle = BorderStyle.None;
            this.Controls.Add(lb9);

            lb10 = new Label();
            lb10.BackColor = Color.FromArgb(50, 50, 50);
            lb10.Cursor = System.Windows.Forms.Cursors.Arrow;
            lb10.Name = "SwitchButtonStripe2";
            lb10.Font = new Font("Segoe UI", 0.5f);
            //lb10.TextAlign = HorizontalAlignment.Center;
            //lb10.ReadOnly = true;
            lb10.Width = 55;
            lb10.Height = 1;
            lb10.Location = new Point(42, 281);
            lb10.TabStop = false;
            //lb10.Enabled = false;
            lb10.Click += lb3_Click;
            lb10.BorderStyle = BorderStyle.None;
            this.Controls.Add(lb10);

            lb7 = new Label();
            lb7.BackColor = Color.FromArgb(30, 30, 30);
            lb7.Cursor = System.Windows.Forms.Cursors.Arrow;
            lb7.Name = "SwitchButton";
            lb7.Text = "";
            lb7.Font = new Font("Segoe UI", 9); //10
            //lb7.TextAlign = HorizontalAlignment.Center;
            //lb7.ReadOnly = true;
            lb7.Width = 74;
            lb7.Height = 18;
            lb7.Location = new Point(32, 270);
            lb7.TabStop = false;
            //lb7.Enabled = false;
            lb7.BorderStyle = BorderStyle.None;
            lb7.Click += lb3_Click;
            this.Controls.Add(lb7);

            switch10 = new TextBox();
            switch10.BackColor = Color.FromArgb(60, 60, 60);
            switch10.Cursor = System.Windows.Forms.Cursors.Arrow;
            switch10.Name = "SwitchButtonSideFrame";
            switch10.Text = "";
            switch10.Font = new Font("Segoe UI", 10);
            switch10.TextAlign = HorizontalAlignment.Center;
            switch10.ReadOnly = true;
            switch10.Width = 76;
            switch10.Height = 20;
            switch10.Location = new Point(31, 270);
            switch10.TabStop = false;
            switch10.Enabled = false;
            switch10.BorderStyle = BorderStyle.None;
            this.Controls.Add(switch10);

            Label lb1 = new Label();
            lb1.Visible = true;
            lb1.Location = new Point(32, 270);
            lb1.BackColor = Color.FromArgb(60, 198, 73);
            lb1.Font = new Font("Segoe UI", 9.5f);
            lb1.Text = "ON";
            lb1.TextAlign = ContentAlignment.MiddleCenter;
            lb1.TextAlign = ContentAlignment.TopCenter;
            lb1.Width = 74;
            lb1.Height = 18;
            lb1.Click += lb3_Click;
            this.Controls.Add(lb1);

            Label lb2 = new Label();
            lb2.Visible = true;
            lb2.Location = new Point(104, 270);
            lb2.BackColor = Color.FromArgb(198, 60, 60);
            lb2.Font = new Font("Segoe UI", 9.5f);
            lb2.Text = "OFF";
            lb2.TextAlign = ContentAlignment.MiddleCenter;
            lb2.TextAlign = ContentAlignment.TopCenter;
            lb2.Width = 74;
            lb2.Height = 18;
            lb2.Click += lb3_Click;
            this.Controls.Add(lb2);

            switch4 = new TextBox();
            switch4.BackColor = Color.FromArgb(60, 60, 60);
            switch4.Cursor = System.Windows.Forms.Cursors.Arrow;
            switch4.ForeColor = Color.FromArgb(255, 255, 255);
            switch4.Name = "BackSwitchFrame";
            switch4.Font = new Font("Segoe UI", 11);
            switch4.TextAlign = HorizontalAlignment.Center;
            switch4.Width = 148;
            switch4.Height = 20;
            switch4.Location = new Point(31, 269);
            switch4.TabStop = false;
            switch4.Enabled = false;
            switch4.BorderStyle = BorderStyle.None;
            this.Controls.Add(switch4);
        }

        private void lb18_Click(object sender, EventArgs e)
        {
            
            MoveSwitch2(!_Macro);
        }

        public void CreateSwitch2()
        {
            lb18 = new Label();
            lb18.BackColor = Color.FromArgb(50, 50, 50);
            lb18.Cursor = System.Windows.Forms.Cursors.Arrow;
            lb18.Name = "SwitchButtonStripe1";
            lb18.Font = new Font("Segoe UI", 0.5f);
            //lb18.TextAlign = HorizontalAlignment.Center;
            //lb18.ReadOnly = true;
            lb18.Width = 55;
            lb18.Height = 1;
            lb18.Location = new Point(42, 297);
            lb18.TabStop = false;
            //lb18.Enabled = false;
            lb18.BorderStyle = BorderStyle.None;
            lb18.Click += lb18_Click;
            this.Controls.Add(lb18);

            lb19 = new Label();
            lb19.BackColor = Color.FromArgb(50, 50, 50);
            lb19.Cursor = System.Windows.Forms.Cursors.Arrow;
            lb19.Name = "SwitchButtonStripe2";
            lb19.Font = new Font("Segoe UI", 0.5f);
            //lb19.TextAlign = HorizontalAlignment.Center;
            //lb19.ReadOnly = true;
            lb19.Width = 55;
            lb19.Height = 1;
            lb19.Location = new Point(42, 300);
            lb19.TabStop = false;
            //lb19.Enabled = false;
            lb19.BorderStyle = BorderStyle.None;
            lb19.Click += lb18_Click;
            this.Controls.Add(lb19);

            lb20 = new Label();
            lb20.BackColor = Color.FromArgb(50, 50, 50);
            lb20.Cursor = System.Windows.Forms.Cursors.Arrow;
            lb20.Name = "SwitchButtonStripe2";
            lb20.Font = new Font("Segoe UI", 0.5f);
            //lb20.TextAlign = HorizontalAlignment.Center;
            //lb20.ReadOnly = true;
            lb20.Width = 55;
            lb20.Height = 1;
            lb20.Location = new Point(42, 303);
            lb20.TabStop = false;
            //lb20.Enabled = false;
            lb20.BorderStyle = BorderStyle.None;
            lb20.Click += lb18_Click;
            this.Controls.Add(lb20);

            lb17 = new Label();
            lb17.BackColor = Color.FromArgb(30, 30, 30);
            lb17.Cursor = System.Windows.Forms.Cursors.Arrow;
            lb17.Name = "SwitchButton";
            lb17.Text = "";
            lb17.Font = new Font("Segoe UI", 9);
            //lb17.TextAlign = HorizontalAlignment.Center;
            //lb17.ReadOnly = true;
            lb17.Width = 74;
            lb17.Height = 18;
            lb17.Location = new Point(32, 292);
            lb17.TabStop = false;
            lb17.TabIndex = 0;
            //lb17.Enabled = false;
            lb17.BorderStyle = BorderStyle.None;
            lb17.Click += lb18_Click;
            //lb17.Parent = switch11;
            this.Controls.Add(lb17);

            switch20 = new TextBox();
            switch20.BackColor = Color.FromArgb(60, 60, 60);
            switch20.Cursor = System.Windows.Forms.Cursors.Arrow;
            switch20.Name = "SwitchButtonSideFrame";
            switch20.Text = "";
            switch20.Font = new Font("Segoe UI", 10);
            switch20.TextAlign = HorizontalAlignment.Center;
            switch20.ReadOnly = true;
            switch20.Width = 76;
            switch20.Height = 20;
            switch20.Location = new Point(31, 292);
            switch20.TabStop = false;
            switch20.Enabled = false;
            switch20.BorderStyle = BorderStyle.None;
            this.Controls.Add(switch20);

            Label lb5 = new Label();
            lb5.Visible = true;
            lb5.Location = new Point(32, 292);
            lb5.BackColor = Color.FromArgb(60, 198, 73);
            lb5.Font = new Font("Segoe UI", 9.5f);
            lb5.Text = "ON";
            lb5.TextAlign = ContentAlignment.MiddleCenter;
            lb5.TextAlign = ContentAlignment.TopCenter;
            lb5.Width = 74;
            lb5.Height = 18;
            lb5.Click += lb18_Click;
            this.Controls.Add(lb5);

            Label lb6 = new Label();
            lb6.Visible = true;
            lb6.Location = new Point(104, 292);
            lb6.BackColor = Color.FromArgb(198, 60, 60);
            lb6.Font = new Font("Segoe UI", 9.5f);
            lb6.Text = "OFF";
            lb6.TextAlign = ContentAlignment.MiddleCenter;
            lb6.TextAlign = ContentAlignment.TopCenter;
            lb6.Width = 74;
            lb6.Height = 18;
            lb6.Click += lb18_Click;
            this.Controls.Add(lb6);

            switch14 = new TextBox();
            switch14.BackColor = Color.FromArgb(60, 60, 60);
            switch14.Cursor = System.Windows.Forms.Cursors.Arrow;
            switch14.ForeColor = Color.FromArgb(255, 255, 255);
            switch14.Name = "BackSwitchFrame";
            switch14.Font = new Font("Segoe UI", 11);
            switch14.TextAlign = HorizontalAlignment.Center;
            switch14.Width = 148;
            switch14.Height = 20;
            switch14.Location = new Point(31, 291);
            switch14.TabStop = false;
            switch14.Enabled = false;
            switch14.BorderStyle = BorderStyle.None;
            this.Controls.Add(switch14);
        }

        public async void MoveSwitch(bool SwitchState)
        {
            await Task.Run(() =>
            {
                if (SwitchState)
                {
                    while (swi < 73)
                    {
                        lb7.Location = new Point(32 + swi, 270);
                        lb8.Location = new Point(42 + swi, 275);
                        lb9.Location = new Point(42 + swi, 278);
                        lb10.Location = new Point(42 + swi, 281);
                        switch10.Location = new Point(31 + swi, 270);
                        swi++;
                        Thread.Sleep(2);
                    }
                }
                else
                {
                    while (swi >= 0)
                    {
                        lb7.Location = new Point(32 + swi, 270);
                        lb8.Location = new Point(42 + swi, 275);
                        lb9.Location = new Point(42 + swi, 278);
                        lb10.Location = new Point(42 + swi, 281);
                        switch10.Location = new Point(31 + swi, 270);
                        swi--;
                        Thread.Sleep(2);
                    }
                }
            });
        }

        public async void MoveSwitch2(bool SwitchState)
        {
            await Task.Run(() =>
            {
                if (SwitchState)
                {
                    while (swi2 < 73)
                    {
                        lb17.Location = new Point(32 + swi2, 292);
                        lb18.Location = new Point(42 + swi2, 297);
                        lb19.Location = new Point(42 + swi2, 300);
                        lb20.Location = new Point(42 + swi2, 303);
                        switch20.Location = new Point(31 + swi2, 292);
                        lb20.Width = 55;
                        swi2++;
                        Thread.Sleep(2);
                    }
                }
                else
                {
                    while (swi2 >= 0)
                    {
                        lb17.Location = new Point(32 + swi2, 292);
                        lb18.Location = new Point(42 + swi2, 297);
                        lb19.Location = new Point(42 + swi2, 300);
                        lb20.Location = new Point(42 + swi2, 303);
                        switch20.Location = new Point(31 + swi2, 292);
                        lb20.Width = 55;
                        swi2--;
                        Thread.Sleep(2);
                    }
                }

                _Macro = SwitchState;
            });
        }

        private void RunKB()
        {
            TH = new Thread(KB);
            TH.SetApartmentState(ApartmentState.STA);
            CheckForIllegalCrossThreadCalls = false;
            TH.Start();
            TH.Suspend();
        }

        private async void KB()
        {
            while (isRunning)
            {
                Thread.Sleep(20);

                if (this.trackBar1.Value == 1)
                {
                    if ((Keyboard.GetKeyStates(Key.F12) & KeyStates.Down) > 0)
                    {
                        if (Buttons[3].Enabled)
                        {
                            Overlay_Button();
                            OverlayIsActivated(overlayIsActivated);
                        }
                    }

                    if ((Keyboard.GetKeyStates(Key.Multiply) & KeyStates.Down) > 0)
                    {
                        if (Buttons[2].Enabled)
                        {
                            CantabileFile(megafon);
                            State(megafon);
                            ButtonState = 2;
                        }
                    }

                    if ((Keyboard.GetKeyStates(Key.Divide) & KeyStates.Down) > 0)
                    {
                        if (Buttons[0].Enabled)
                        {
                            CantabileFile(normal);
                            State(normal);
                            ButtonState = 0;
                        }
                    }

                    if ((Keyboard.GetKeyStates(Key.Subtract) & KeyStates.Down) > 0)
                    {
                        if (Buttons[1].Enabled)
                        {
                            CantabileFile(ra);
                            State(ra);
                            ButtonState = 1;
                        }
                    }

                    if ((Keyboard.GetKeyStates(Key.CapsLock) & KeyStates.Down) > 0)
                    {
                        CapsLockPressed = true;
                    }
                    else
                    {
                        CapsLockPressed = false;
                    }

                    if ((Keyboard.GetKeyStates(Key.D0) & KeyStates.Toggled) > 0)
                    {
                        if (!_caps && _Macro)
                        {
                            //SendKeys.SendWait("CAPSLOCK");
                        }
                    }
                    else if ((Keyboard.GetKeyStates(Key.D0) & KeyStates.Down) > 0)
                    {
                        _caps = true;
                    }
                    else
                    {
                        if (_caps)
                        {
                            _caps = false;
                            //SendKeys.SendWait("CAPSLOCK");
                        }
                    }

                    if ((Keyboard.GetKeyStates(Key.D9) & KeyStates.Down) > 0)
                    {
                        if (!_KEYSEND && _Macro)
                        {
                            SendKeys.SendWait("F11");
                            _KEYSEND = true;
                        }
                    }
                    else
                    {
                        if (_KEYSEND)
                        {
                            _KEYSEND = false;
                        }
                    }

                    if ((Keyboard.GetKeyStates(Key.Add) & KeyStates.Down) > 0)
                    {
                        if (!_add)
                        {
                            _add = true;

                            if (!_Macro)
                            {
                                MoveSwitch2(true);
                            }
                            else
                            {
                                MoveSwitch2(false);
                            }
                        }
                    }
                    else
                    {
                        if (_add)
                        {
                            _add = false;
                        }
                    }
                }
            }
        }

        public void TrackBar()
        {
            this.trackBar1.Value = 0;
            this.trackBar1.Enabled = false;
            TheTimer();
            RunKB();
        }

        public static bool SwitchState = false;
        public static System.Timers.Timer DetectTimer;
        public void TheTimer()
        {
            DetectTimer = new System.Timers.Timer();
            DetectTimer.Elapsed += new ElapsedEventHandler(DetectTimer_Elapsed);
            DetectTimer.Interval = 1000;
            DetectTimer.Start();
        }

        public void DetectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            using (var gta = Process.GetProcessesByName("GTA5").FirstOrDefault())
            {
                if (gta != null)
                {
                    grt = DateTime.Now - gta.StartTime;
                    if (lb3.Text != "Keyboard Hooked")
                    {
                        if (this.trackBar1.Value == 0)
                        {
                            if(grt.Hours > 0 | grt.Minutes > 0 | grt.Seconds > 50)
                            {
                                TurnOn();
                            }
                        }
                    }
                }
                else
                {
                    if (lb3.Text != "Keyboard not Hooked")
                    {
                        if (this.trackBar1.Value == 1)
                        {
                            TurnOff();
                        }
                    }
                }

                lb4.Text = grt.ToString("hh':'mm':'ss");
            }
        }

        private async void TurnOn()
        {
            MoveSwitch(true);
            Buttons[3].Enabled = true;
            this.trackBar1.Value = 1;
            Overlay.SetOverlayForegrounded();
            Overlay2.SetOverlayForegrounded();
            Overlay3.SetOverlayForegrounded();

            await Task.Run(() =>
            {
                SetOnTop();
                OverlayHooked();
            });

            TH.Resume();
            lb3.Text = "Keyboard Hooked";
        }

        private void TurnOff()
        {
            if (!Convert.ToBoolean(overlayIsActivated))
            {
                OverlayFadeOut();
                OverlayIsActivated(0);
            }
            MoveSwitch(false);
            Buttons[3].Enabled = false;
            TH.Suspend();
            this.trackBar1.Value = 0;
            lb3.Text = "Keyboard not Hooked";
        }

        private void SetOnTop()
        {
            Overlay.TopMost = true;
            Overlay.TopMost = false;

            Overlay2.TopMost = true;
            Overlay2.TopMost = false;

            Overlay3.TopMost = true;
            Overlay3.TopMost = false;
        }

        private void TB_Clicked(object sender, EventArgs e)
        {

        }

        /*protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(TB.Location.X, TB.Location.Y, TB.ClientSize.Width, TB.ClientSize.Height);
            rect.Inflate(1, 1);
            ControlPaint.DrawBorder(e.Graphics, rect, Color.FromArgb(180, 35, 35, 35), ButtonBorderStyle.Dashed);

            Rectangle rect2 = new Rectangle(TB2.Location.X, TB2.Location.Y, TB2.ClientSize.Width, TB2.ClientSize.Height);
            rect2.Inflate(1, 1);
            ControlPaint.DrawBorder(e.Graphics, rect2, Color.FromArgb(180, 35, 35, 35), ButtonBorderStyle.Dashed);

            base.OnPaint(e);
        }*/

        public void normal_Click(object sender, EventArgs e)
        {
            CantabileFile(normal);
            State(normal);
        }

        public void ra_Click(object sender, EventArgs e)
        {
            CantabileFile(ra);
            State(ra);
        }

        public void mega_Click(object sender, EventArgs e)
        {
            CantabileFile(megafon);
            State(megafon);
        }

        public void OverlayB_Click(object sender, EventArgs e)
        {
            Overlay_Button();
            OverlayIsActivated(overlayIsActivated);
        }

        public static void Overlay_Button()
        {

            if (Overlay_Button_Clicked)
            {
                Buttons[3].Enabled = false;
                OverlayFadeOut();
            }
            else
            {
                Buttons[3].Enabled = false;
                OverlayFadeIn();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        public static void OverlayFadeIn()
        {
            Overlay_Button_Clicked = true;

            Overlay.sDXThread();
            Overlay2.sDXThread();

            while (ic <= 0.700)
            {
                Overlay.Opacity = ic;
                Overlay2.Opacity = ic;
                Thread.Sleep(10);
                ic += 0.007;
            }
            Thread.Sleep(100);
            Buttons[3].Enabled = true;
        }

        public static void OverlayFadeOut()
        {
            Overlay.sDXThread();
            Overlay2.sDXThread();

            while (ic >= 0.000)
            {
                Overlay.Opacity = ic;
                Overlay2.Opacity = ic;
                Thread.Sleep(10);
                ic -= 0.007;
            }
            Thread.Sleep(100);
            Buttons[3].Enabled = true;
            Overlay_Button_Clicked = false;
        }

        public static void OverlayHooked()
        {
            double i = 0.000;
            while (i <= 0.700)
            {
                Overlay3.Opacity = i;
                Overlay3.sDXThread();
                Thread.Sleep(10);
                i += 0.007;
            }

            Thread.Sleep(3000);

            while (i >= 0.000)
            {
                Overlay3.Opacity = i;
                Overlay3.sDXThread();
                Thread.Sleep(10);
                i -= 0.007;
            }
        }

        public static void ButtonColor(int index, int rgbR, int rgbG, int rgbB, int argA, int argR, int argG)
        {
            Buttons[index].BackColor = Color.FromArgb(rgbR, rgbG, rgbB);
            Buttons[index].ForeColor = Color.FromArgb(argA, argR, argG);
        }
    }
}

