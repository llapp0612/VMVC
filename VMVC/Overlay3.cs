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
    public partial class Overlay3 : Form
    {
        public WindowRenderTarget device = null;
        private HwndRenderTargetProperties renderProperties;
        private SolidColorBrush solidColorBrush, brush1, brush2;
        private Factory factory;

        //text fonts
        private TextFormat font, fontSmall, font1, fontSmall1;
        private FontFactory fontFactory;
        private const string fontFamily = "Segoe UI", fontFamily1 = fontFamily;//you can edit this of course
        private const float fontSize = 16.0f, fontSize1 = 16.0f;
        private const float fontSizeSmall = 16.0f, fontSizeSmall1 = fontSizeSmall;

        private IntPtr handle;
        public Thread sDX = null;
        public IntPtr GTA_HWND;

        //DllImports
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

        public Overlay3()
        {
            this.handle = Handle;
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            var scrnx = (Screen.PrimaryScreen.WorkingArea.Width / 2) - (this.Width / 2);
            var scrny = (Screen.PrimaryScreen.WorkingArea.Height / 2) - (this.Width + 100);
            SetWindowPos(this.Handle, HWND_TOPMOST, scrnx, scrny, 0, 0, 0);

            SetRegion(180, 50);
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

        public void Overlay3_Load(object sender, EventArgs e)
        {
            LoadDevice();
        }

        public void LoadDevice()
        {
            this.DoubleBuffered = true;
            this.Width = 180;// set your own size
            this.Height = 50;
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
                PixelSize = new Size2(180, 50),
                PresentOptions = PresentOptions.None
            };

            //Init DirectX
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);

            solidColorBrush = new SolidColorBrush(device, Color.LawnGreen);
            // Init font's
            font = new TextFormat(fontFactory, fontFamily, fontSize);
            font1 = new TextFormat(fontFactory, fontFamily, fontSize1);
            fontSmall = new TextFormat(fontFactory, fontFamily, fontSizeSmall);

            brush1 = new SolidColorBrush(device, SharpDX.Color.LawnGreen);
            brush2 = new SolidColorBrush(device, SharpDX.Color.DarkGreen);

            //sDX = new Thread(sDXThread);

            //sDX.Priority = ThreadPriority.Lowest;
            //sDX.IsBackground = true;
            //sDX.Start();
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
                SetWindowLong(owned, -8, owner);
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
            device.BeginDraw();
            device.Clear(Color.Transparent);

            device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype; // you can set another text mode

            var text1 = "Overlay Hooked";

            var rect = new RectangleF(30, 12, 180, 50);
            device.DrawText(text1, font, rect, brush2);

            rect = new RectangleF(30, 12, 180, 50);
            device.DrawText(text1, font1, rect, brush1);

            device.Flush();
            device.EndDraw();

            //SetOverlayForegrounded();
        }
    }
}