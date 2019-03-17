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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Reflection;

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

        public static Label TBLB1, TBLB2, TBLB3, TBLB4;
        public static Label SLB1, SLB2, SLB3, SLB4, SLB5, SLB6, SLB7, SLB8, SLB9, SLB10, SLB11, SLB12, SLB13, SLB14, SLB15, SLB16;

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

        PrivateFontCollection pfc, pfc2;
        public static string _FSC = "VMVC.Resources.Fonts.";

        public static Bitmap _Button_Green = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("VMVC.Resources.Buttons.button_green.png"));
        public static Bitmap _Button_Gray = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("VMVC.Resources.Buttons.button.png"));
        public static Bitmap _Button_Orange = new Bitmap(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("VMVC.Resources.Buttons.button_orange.png"));

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
            switch (state)
            {
                case "Normal":
                    Buttons[0].BackgroundImage = (Image)_Button_Green;
                    Buttons[1].BackgroundImage = (Image)_Button_Gray;
                    Buttons[2].BackgroundImage = (Image)_Button_Gray;

                    /*ButtonColor(0, 60, 198, 73, 30, 30, 30);
                    ButtonColor(1, 74, 74, 74, 205, 205, 205);
                    ButtonColor(2, 74, 74, 74, 205, 205, 205);*/
                    ButtonState = 0;
                    break;
                case "Ra":
                    Buttons[0].BackgroundImage = (Image)_Button_Gray;
                    Buttons[1].BackgroundImage = (Image)_Button_Green;
                    Buttons[2].BackgroundImage = (Image)_Button_Gray;

                    /*ButtonColor(0, 74, 74, 74, 205, 205, 205);
                    ButtonColor(1, 60, 198, 73, 30, 30, 30);
                    ButtonColor(2, 74, 74, 74, 205, 205, 205);*/
                    ButtonState = 1;
                    break;
                case "Megafon":
                    Buttons[0].BackgroundImage = (Image)_Button_Gray;
                    Buttons[1].BackgroundImage = (Image)_Button_Gray;
                    Buttons[2].BackgroundImage = (Image)_Button_Green;

                    /*ButtonColor(0, 74, 74, 74, 205, 205, 205);
                    ButtonColor(1, 74, 74, 74, 205, 205, 205);
                    ButtonColor(2, 60, 198, 73, 30, 30, 30);*/
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
                Buttons[3].BackgroundImage = (Image)_Button_Orange;

                //ButtonColor(3, 240, 130, 33, 0, 0, 0);
                overlayIsActivated = 0;
            }
            else
            {
                Buttons[3].BackgroundImage = (Image)_Button_Gray;
                //ButtonColor(3, 74, 74, 74, 255, 255, 255);
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
            //Application.SetCompatibleTextRenderingDefault(true);

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
            GC.SuppressFinalize(this);
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
            LoadFont();
            LoadFont2();

            TextField();
            TextField2();
            CreateSwitch();
            CreateSwitch2();
            TrackBar();

            Button(0, normal, 31, 31, normal_Click);
            Button(1, ra, 31, 63, ra_Click);
            Button(2, megafon, 31, 95, mega_Click);
            Button(3, overlay, 31, 127, OverlayB_Click);

            Label(0, 30, 30);
            Label(1, 30, 62);
            Label(2, 30, 94);
            Label(3, 30, 126);

            Buttons[3].Enabled = false;
        }

        public void Label(int i, int LocationX, int LocationY)
        {
            try
            {
                BtLabels[i] = new Label
                {
                    Size = new Size(150, 30),
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
                    Size = new Size(148, 28),
                    Location = new Point(LocationX, LocationY),
                    Text = Name,
                    Font = new Font(pfc2.Families[0], 14f, FontStyle.Regular),//Font(V1, 12),
                    TextAlign = ContentAlignment.BottomCenter,
                    BackColor = Color.FromArgb(74, 74, 74),
                    FlatStyle = FlatStyle.Flat
                };
                Buttons[i].BackgroundImage = Image.FromFile(@"B:\OBS\Img\button.png");
                Buttons[i].FlatAppearance.BorderSize = 0;
                Buttons[i].TabStop = true;
                Buttons[i].ForeColor = Color.FromArgb(205, 205, 205);
                Buttons[i].FlatAppearance.CheckedBackColor = Color.FromArgb(20, 20, 20);
                Buttons[i].UseCompatibleTextRendering = true;
                this.Controls.Add(Buttons[i]);
                Buttons[i].Click += new EventHandler(Click);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }

        public void LoadFont()
        {
            string resource = _FSC + "NoLicense_R-2014.ttf";
            pfc = new PrivateFontCollection();
            Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            System.IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);
            byte[] fontdata = new byte[fontStream.Length];
            fontStream.Read(fontdata, 0, (int)fontStream.Length);
            Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);
            pfc.AddMemoryFont(data, (int)fontStream.Length);
            fontStream.Close();
            Marshal.FreeCoTaskMem(data);
        }

        public void LoadFont2()
        {
            string resource = _FSC + "techno_hideo.ttf";
            pfc2 = new PrivateFontCollection();
            Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            System.IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);
            byte[] fontdata = new byte[fontStream.Length];
            fontStream.Read(fontdata, 0, (int)fontStream.Length);
            Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);
            pfc2.AddMemoryFont(data, (int)fontStream.Length);
            fontStream.Close();
            Marshal.FreeCoTaskMem(data);
        }

        public void TBLBS_Click(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(this.trackBar1.Value))
            {
                TurnOn();
            }
        }

        public void TextField()
        {
            TBLB1 = new Label();
            TBLB1.Size = new Size(150, 30);
            TBLB1.Location = new Point(32, 214);
            TBLB1.BackColor = Color.FromArgb(74, 74, 74);
            TBLB1.ForeColor = Color.FromArgb(205, 205, 205);
            TBLB1.Font = new Font(pfc.Families[0], 9f, FontStyle.Regular);
            TBLB1.TextAlign = ContentAlignment.MiddleCenter;
            TBLB1.Width = 146;
            TBLB1.Height = 23;
            TBLB1.Text = "Keyboard Off";
            TBLB1.UseCompatibleTextRendering = true;
            TBLB1.Click += TBLBS_Click;
            this.Controls.Add(TBLB1);

            TBLB2 = new Label();
            TBLB2.Size = new Size(150, 30);
            TBLB2.Location = new Point(31, 213);
            TBLB2.BackColor = Color.FromArgb(74, 74, 74);
            TBLB2.TabStop = false;
            TBLB2.ForeColor = Color.FromArgb(255, 255, 255);
            TBLB2.BorderStyle = BorderStyle.FixedSingle;
            TBLB2.Font = new Font("Segoe UI", 10);
            TBLB2.Width = 148;
            TBLB2.Height = 26;
            TBLB2.Enabled = false;
            TBLB2.Text = "";
            TBLB2.Click += new EventHandler(TB_Clicked);
            this.Controls.Add(TBLB2);
        }

        public void TBLB3_Click(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(this.trackBar1.Value))
            {
                grt = new TimeSpan();
                TBLB3.Text = "00:00:00";
            }
        }

        public void TextField2()
        {
            TBLB3 = new Label();
            TBLB3.Size = new Size(150, 30);
            TBLB3.Location = new Point(32, 242);
            TBLB3.BackColor = Color.FromArgb(74, 74, 74);
            TBLB3.ForeColor = Color.FromArgb(205, 205, 205);
            TBLB3.Font = new Font(pfc.Families[0], 10.2f, FontStyle.Regular);
            TBLB3.TextAlign = ContentAlignment.MiddleCenter;
            TBLB3.TextAlign = ContentAlignment.TopCenter;
            TBLB3.Width = 146;
            TBLB3.Height = 23;
            TBLB3.Text = "00:00:00";
            TBLB3.UseCompatibleTextRendering = true;
            TBLB3.Click += TBLB3_Click;
            this.Controls.Add(TBLB3);

            TBLB4 = new Label();
            TBLB4.Size = new Size(150, 30);
            TBLB4.Location = new Point(31, 241);
            TBLB4.BackColor = Color.FromArgb(74, 74, 74);
            TBLB4.TabStop = false;
            TBLB4.ForeColor = Color.FromArgb(255, 255, 255);
            TBLB4.BorderStyle = BorderStyle.FixedSingle;
            TBLB4.Font = new Font("Segoe UI", 10);
            TBLB4.Width = 148;
            TBLB4.Height = 26;
            TBLB4.Enabled = false;
            this.Controls.Add(TBLB4);
        }

        public void CreateSwitch()
        {
            SLB8 = new Label();
            SLB8.BackColor = Color.FromArgb(40, 40, 40);
            SLB8.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB8.Name = "SwitchButtonStripe1";
            SLB8.Font = new Font("Segoe UI", 0.5f);
            SLB8.Width = 54;
            SLB8.Height = 1;
            SLB8.Location = new Point(42, 275);
            SLB8.TabStop = false;
            SLB8.Click += TBLBS_Click;
            SLB8.BorderStyle = BorderStyle.None;
            this.Controls.Add(SLB8);

            SLB7 = new Label();
            SLB7.BackColor = Color.FromArgb(40, 40, 40);
            SLB7.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB7.Name = "SwitchButtonStripe2";
            SLB7.Font = new Font("Segoe UI", 0.5f);
            SLB7.Width = 54;
            SLB7.Height = 1;
            SLB7.Location = new Point(42, 278);
            SLB7.TabStop = false;
            SLB7.Click += TBLBS_Click;
            SLB7.BorderStyle = BorderStyle.None;
            this.Controls.Add(SLB7);

            SLB6 = new Label();
            SLB6.BackColor = Color.FromArgb(40, 40, 40);
            SLB6.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB6.Name = "SwitchButtonStripe2";
            SLB6.Font = new Font("Segoe UI", 0.5f);
            SLB6.Width = 54;
            SLB6.Height = 1;
            SLB6.Location = new Point(42, 281);
            SLB6.TabStop = false;
            SLB6.Click += TBLBS_Click;
            SLB6.BorderStyle = BorderStyle.None;
            this.Controls.Add(SLB6);

            SLB5 = new Label();
            SLB5.BackColor = Color.FromArgb(30, 30, 30);
            SLB5.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB5.Name = "SwitchButton";
            SLB5.Font = new Font("Segoe UI", 9); //10
            SLB5.Width = 73;
            SLB5.Height = 18;
            SLB5.Location = new Point(32, 270);
            SLB5.TabStop = false;
            SLB5.BorderStyle = BorderStyle.None;
            SLB5.Click += TBLBS_Click;
            this.Controls.Add(SLB5);

            SLB4 = new Label();
            SLB4.BackColor = Color.FromArgb(60, 60, 60);
            SLB4.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB4.Name = "SwitchButtonSideFrame";
            SLB4.Font = new Font("Segoe UI", 10);
            SLB4.Width = 75;
            SLB4.Height = 18;
            SLB4.Location = new Point(31, 270);
            SLB4.TabStop = false;
            SLB4.Enabled = false;
            SLB4.BorderStyle = BorderStyle.None;
            this.Controls.Add(SLB4);

            SLB3 = new Label();
            SLB3.Visible = true;
            SLB3.Location = new Point(32, 270);
            SLB3.BackColor = Color.FromArgb(60, 198, 73);
            SLB3.ForeColor = Color.FromArgb(20, 20, 20);
            SLB3.Font = new Font(pfc2.Families[0], 14f, FontStyle.Regular);
            SLB3.Text = "ON";
            SLB3.TextAlign = ContentAlignment.BottomCenter;
            SLB3.Width = 74;
            SLB3.Height = 18;
            SLB3.UseCompatibleTextRendering = true;
            SLB3.Click += TBLBS_Click;
            this.Controls.Add(SLB3);

            SLB2 = new Label();
            SLB2.Visible = true;
            SLB2.Location = new Point(104, 270);
            SLB2.BackColor = Color.FromArgb(198, 60, 60);
            SLB2.ForeColor = Color.FromArgb(20, 20, 20);
            SLB2.Font = new Font(pfc2.Families[0], 14f, FontStyle.Regular);
            SLB2.Text = "OFF";
            SLB2.TextAlign = ContentAlignment.BottomCenter;
            SLB2.Width = 74;
            SLB2.Height = 18;
            SLB2.UseCompatibleTextRendering = true;
            SLB2.Click += TBLBS_Click;
            this.Controls.Add(SLB2);

            SLB1 = new Label();
            SLB1.BackColor = Color.FromArgb(60, 60, 60);
            SLB1.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB1.ForeColor = Color.FromArgb(255, 255, 255);
            SLB1.Name = "BackSwitchFrame";
            SLB1.Font = new Font("Segoe UI", 11);
            SLB1.Width = 148;
            SLB1.Height = 20;
            SLB1.Location = new Point(31, 269);
            SLB1.TabStop = false;
            SLB1.Enabled = false;
            SLB1.BorderStyle = BorderStyle.None;
            this.Controls.Add(SLB1);
        }

        private void TBLBS_Click2(object sender, EventArgs e)
        {
            
            MoveSwitch2(!_Macro);
        }

        public void CreateSwitch2()
        {
            SLB16 = new Label();
            SLB16.BackColor = Color.FromArgb(40, 40, 40);
            SLB16.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB16.Name = "SwitchButtonStripe1";
            SLB16.Font = new Font("Segoe UI", 0.5f);
            SLB16.Width = 54;
            SLB16.Height = 1;
            SLB16.Location = new Point(42, 297);
            SLB16.TabStop = false;
            SLB16.BorderStyle = BorderStyle.None;
            SLB16.Click += TBLBS_Click2;
            this.Controls.Add(SLB16);

            SLB15 = new Label();
            SLB15.BackColor = Color.FromArgb(40, 40, 40);
            SLB15.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB15.Name = "SwitchButtonStripe2";
            SLB15.Font = new Font("Segoe UI", 0.5f);
            SLB15.Width = 54;
            SLB15.Height = 1;
            SLB15.Location = new Point(42, 300);
            SLB15.TabStop = false;
            SLB15.BorderStyle = BorderStyle.None;
            SLB15.Click += TBLBS_Click2;
            this.Controls.Add(SLB15);

            SLB14 = new Label();
            SLB14.BackColor = Color.FromArgb(40, 40, 40);
            SLB14.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB14.Name = "SwitchButtonStripe2";
            SLB14.Font = new Font("Segoe UI", 0.5f);
            SLB14.Width = 54;
            SLB14.Height = 1;
            SLB14.Location = new Point(42, 303);
            SLB14.TabStop = false;
            SLB14.BorderStyle = BorderStyle.None;
            SLB14.Click += TBLBS_Click2;
            this.Controls.Add(SLB14);

            SLB13 = new Label();
            SLB13.BackColor = Color.FromArgb(30, 30, 30);
            SLB13.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB13.Name = "SwitchButton";
            SLB13.Font = new Font("Segoe UI", 9);
            SLB13.Width = 73;
            SLB13.Height = 18;
            SLB13.Location = new Point(32, 292);
            SLB13.TabStop = false;
            SLB13.TabIndex = 0;
            SLB13.BorderStyle = BorderStyle.None;
            SLB13.Click += TBLBS_Click2;
            this.Controls.Add(SLB13);

            SLB12 = new Label();
            SLB12.BackColor = Color.FromArgb(60, 60, 60);
            SLB12.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB12.Name = "SwitchButtonSideFrame";
            SLB12.Font = new Font("Segoe UI", 10);
            SLB12.Width = 75;
            SLB12.Height = 18;
            SLB12.Location = new Point(31, 292);
            SLB12.TabStop = false;
            SLB12.Enabled = false;
            SLB12.BorderStyle = BorderStyle.None;
            this.Controls.Add(SLB12);

            SLB11 = new Label();
            SLB11.Visible = true;
            SLB11.Location = new Point(32, 292);
            SLB11.BackColor = Color.FromArgb(60, 198, 73);
            SLB11.ForeColor = Color.FromArgb(20, 20, 20);
            SLB11.Font = new Font(pfc2.Families[0], 14f, FontStyle.Regular);
            SLB11.Text = "ON";
            SLB11.TextAlign = ContentAlignment.BottomCenter;
            SLB11.Width = 74;
            SLB11.Height = 18;
            SLB11.UseCompatibleTextRendering = true;
            SLB11.Click += TBLBS_Click2;
            this.Controls.Add(SLB11);

            SLB10 = new Label();
            SLB10.Visible = true;
            SLB10.Location = new Point(104, 292);
            SLB10.BackColor = Color.FromArgb(198, 60, 60);
            SLB10.ForeColor = Color.FromArgb(20, 20, 20);
            SLB10.Font = new Font(pfc2.Families[0], 14f, FontStyle.Regular);
            SLB10.Text = "OFF";
            SLB10.TextAlign = ContentAlignment.BottomCenter;
            SLB10.Width = 74;
            SLB10.Height = 18;
            SLB10.UseCompatibleTextRendering = true;
            SLB10.Click += TBLBS_Click2;
            this.Controls.Add(SLB10);

            SLB9 = new Label();
            SLB9.BackColor = Color.FromArgb(60, 60, 60);
            SLB9.Cursor = System.Windows.Forms.Cursors.Arrow;
            SLB9.ForeColor = Color.FromArgb(255, 255, 255);
            SLB9.Name = "BackSwitchFrame";
            SLB9.Font = new Font("Segoe UI", 11);
            SLB9.Width = 148;
            SLB9.Height = 20;
            SLB9.Location = new Point(31, 291);
            SLB9.TabStop = false;
            SLB9.Enabled = false;
            SLB9.BorderStyle = BorderStyle.None;
            this.Controls.Add(SLB9);
        }

        public async void MoveSwitch(bool SwitchState)
        {
            await Task.Run(() =>
            {
                if (SwitchState)
                {
                    while (swi < 74)
                    {
                        SLB4.Location = new Point(31 + swi, 270);
                        SLB5.Location = new Point(32 + swi, 270);
                        SLB6.Location = new Point(42 + swi, 275);
                        SLB7.Location = new Point(42 + swi, 278);
                        SLB8.Location = new Point(42 + swi, 281);
                        swi++;
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    while (swi >= 0)
                    {
                        SLB4.Location = new Point(31 + swi, 270);
                        SLB5.Location = new Point(32 + swi, 270);
                        SLB6.Location = new Point(42 + swi, 275);
                        SLB7.Location = new Point(42 + swi, 278);
                        SLB8.Location = new Point(42 + swi, 281);
                        swi--;
                        Thread.Sleep(1);
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
                    while (swi2 < 74)
                    {
                        SLB12.Location = new Point(31 + swi2, 292);
                        SLB13.Location = new Point(32 + swi2, 292);
                        SLB14.Location = new Point(42 + swi2, 297);
                        SLB15.Location = new Point(42 + swi2, 300);
                        SLB16.Location = new Point(42 + swi2, 303);
                        swi2++;
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    while (swi2 >= 0)
                    {
                        SLB12.Location = new Point(31 + swi2, 292);
                        SLB13.Location = new Point(32 + swi2, 292);
                        SLB14.Location = new Point(42 + swi2, 297);
                        SLB15.Location = new Point(42 + swi2, 300);
                        SLB16.Location = new Point(42 + swi2, 303);
                        swi2--;
                        Thread.Sleep(1);
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
            using (var gta = Process.GetProcessesByName("Notepad").FirstOrDefault())
            {
                if (gta != null)
                {
                    grt = DateTime.Now - gta.StartTime;
                    if (TBLB1.Text != "Keyboard On")
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
                    if (TBLB1.Text != "Keyboard Off")
                    {
                        if (this.trackBar1.Value == 1)
                        {
                            TurnOff();
                        }
                    }
                }

                TBLB3.Text = grt.ToString("hh':'mm':'ss");
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
            TBLB1.Text = "Keyboard On";
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
            TBLB1.Text = "Keyboard Off";
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