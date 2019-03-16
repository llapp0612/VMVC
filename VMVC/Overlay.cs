using System;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;
using SharpDX;
using SharpDX.DirectWrite;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;

namespace VMVC
{
    public partial class Overlay : Form
    {
        public WindowRenderTarget device;
        private HwndRenderTargetProperties renderProperties;
        private SolidColorBrush solidColorBrush, brush1, brush2, brush3, brush4, brush5;
        private Factory factory;

        //text fonts
        private TextFormat font, fontSmall, font1, fontSmall1;
        private FontFactory fontFactory;
        private const string fontFamily = "Segoe UI", fontFamily1 = fontFamily;
        private const float fontSize = 12.0f, fontSize1 = 12.0f;
        private const float fontSizeSmall = 14.0f, fontSizeSmall1 = fontSizeSmall;

        private IntPtr handle;
        public Thread sDX = null;
        public IntPtr GTA_HWND;

        //DllImports
        /// <SharpDX Components>
        //////////////////////////////////////////////////////////////////////////////////////////////////
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("dwmapi.dll")]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);
        //////////////////////////////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string ClassName, string WindowName);

        //Styles
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        public static IntPtr HWND_TOPMOST = new IntPtr(1);

        private const int WS_EX_NOACTIVATE = 0x08000000;

        protected override CreateParams CreateParams
        {
            get
            {
                var Params = base.CreateParams;
                Params.ExStyle |= 0x80;
                Params.ExStyle |= WS_EX_NOACTIVATE;
                return Params;
            }
        }
        
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Cursor.Hide();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            Cursor.Show();
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );
        
        public Overlay()
        {
            this.handle = Handle;
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            SetWindowPos(this.Handle, HWND_TOPMOST, 5, 5, 0, 0, 0);

            SetRegion(180, 385);
            this.Show();

            OnResize(null);
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            
            //Process.GetCurrentProcess().ProcessorAffinity = (System.IntPtr)12;
            GC.Collect();
        }

        public void SetRegion(int width, int height)
        {
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, width, height, 15, 15));
        }

        public void Overlay_Load(object sender, EventArgs e)
        {
            LoadDevice();
        }

        public void LoadDevice()
        {
            this.DoubleBuffered = true;
            this.Width = 180;// set your own size
            this.Height = 385;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |// this reduce the flicker
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.Opaque |
                ControlStyles.CacheText |
                ControlStyles.Opaque |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            //this.TopMost = true;
            this.Visible = true;
            this.Cursor = Cursors.No;
            //Cursor.Hide();

            factory = new SharpDX.Direct2D1.Factory(SharpDX.Direct2D1.FactoryType.MultiThreaded);
            fontFactory = new FontFactory();

            renderProperties = new HwndRenderTargetProperties()
            {
                Hwnd = this.Handle,
                PixelSize = new Size2(180, 385),
                PresentOptions = PresentOptions.None
            };

            //Init DirectX
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);

            solidColorBrush = new SolidColorBrush(device, SharpDX.Color.LawnGreen);
            // Init font's
            font = new TextFormat(fontFactory, fontFamily, fontSize);
            font1 = new TextFormat(fontFactory, fontFamily, fontSize1);
            fontSmall = new TextFormat(fontFactory, fontFamily, fontSizeSmall);

            brush1 = new SolidColorBrush(device, SharpDX.Color.LawnGreen);
            brush2 = new SolidColorBrush(device, SharpDX.Color.DarkGreen);
            brush3 = new SolidColorBrush(device, SharpDX.Color.Orange);
            brush4 = new SolidColorBrush(device, SharpDX.Color.OrangeRed);
            brush5 = new SolidColorBrush(device, SharpDX.Color.DarkGray);

            /*sDX = new Thread(new ParameterizedThreadStart(sDXThread));

            sDX.Priority = ThreadPriority.Lowest;
            sDX.IsBackground = true;
            sDX.Start();*/
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            int[] marg = new int[] { 0, 0, Width, Height };
            DwmExtendFrameIntoClientArea(this.Handle, ref marg);
        }

        public void SetOverlayForegrounded()
        {
            this.handle = Handle;
            var GTA5 = Process.GetProcessesByName("GTA5").FirstOrDefault();
            //GTA_HWND = GTA5.Handle;
            int initialStyle = GetWindowLong(this.Handle, -20);

            if (GTA5 != null)
            {
                var owner = GTA5.MainWindowHandle;
                var owned = this.Handle;
                SetWindowLong(owned, -8 /*GWL_HWNDPARENT*/, owner);
                if (GetForegroundWindow() != IntPtr.Zero)
                {
                    SetForegroundWindow(owner);
                }
            }
            else
            {
                SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            }
        }

        public void sDXThread()
        {
            if (VMVC.Overlay_Button_Clicked)
            {
                device.BeginDraw();
                device.Clear(Color.Transparent);
                string vText = "Normal";
                var brush = brush1;

                if (VMVC.ButtonState == 1)
                {
                    vText = "Ra";
                    brush = brush4;
                }
                else if (VMVC.ButtonState == 2)
                {
                    vText = "Megafon";
                    brush = brush3;
                }

                var txt = "\n" +
                "1007 - Beschäftigt" +
                "\n" +
                "1008 - Verfügbar" +
                "\n\n" +
                "1010 - Schlägerei" +
                "\n" +
                "1020 - Status/Standort" +
                "\n" +
                "1021 - Dispatch erreicht" +
                "\n" +
                "1028 - Kennzeichenabfr." +
                "\n" +
                "1029 - Personenabfr." +
                "\n\n" +
                "1041 - Dienst Anmeldung" +
                "\n" +
                "1042 - Dienst Abmelden" +
                "\n\n" +
                "1043 - Verfolgung" +
                "\n" +
                "1061 - VK/PK" +
                "\n" +
                "1065 - Shopraub" +
                "\n" +
                "1074 - Gefängnisausbruch" +
                "\n" +
                "1090 - Backup" +
                "\n\n" +
                "1099 - Officer in Not" +
                "\n" +
                "Voice:  ";

                device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;
                var rect = new RectangleF(20, 0, this.Width, this.Height);
                device.DrawText(txt, font, rect, brush2);
                device.DrawText(txt, font1, rect, brush1);

                rect = new RectangleF(58, 319, this.Width, this.Height);
                device.DrawText(vText, font, rect, brush2);
                device.DrawText(vText, font1, rect, brush);

                rect = new RectangleF(20, 350, this.Width, this.Height);
                if (VMVC._CapsLockPressed)
                {
                    device.DrawText("Aktiver Funk", font, rect, brush2);
                    device.DrawText("Aktiver Funk", font1, rect, brush3);
                }
                else
                {
                    device.DrawText("Inaktiver Funk", font, rect, brush2);
                    device.DrawText("Inaktiver Funk", font1, rect, brush5);
                }

                var MacVal = "Aus";
                if(VMVC._Macro)
                {
                    MacVal = "An";
                }

                var MacTx = "Macros: " + MacVal;
                rect = new RectangleF(20, 335, this.Width, this.Height);
                if (VMVC._Macro)
                {
                    device.DrawText(MacTx, font, rect, brush2);
                    device.DrawText(MacTx, font1, rect, brush4);
                }
                else
                {
                    device.DrawText(MacTx, font, rect, brush2);
                    device.DrawText(MacTx, font1, rect, brush5);
                }

                device.Flush();
                device.EndDraw();

                //SetOverlayForegrounded();
            }
        }
    }
}