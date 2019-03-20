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
using NAudio;
using NAudio.CoreAudioApi;
using VMVC.Properties;

namespace VMVC
{
    public partial class AudioDevices : Form
    {
        private bool _preventMove = true;

        protected override void WndProc(ref Message message)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

            if (_preventMove)
            {
                switch (message.Msg)
                {
                    case WM_SYSCOMMAND:
                        int command = message.WParam.ToInt32() & 0xfff0;
                        if (command == SC_MOVE)
                            return;
                        break;
                }
            }

            base.WndProc(ref message);
        }

        public AudioDevices()
        {
            InitializeComponent();

            comboBox1.Items.AddRange(GetDevices());
            comboBox2.Items.AddRange(GetDevices());
            this.Restore();
        }

        public static object[] GetDevices()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);

            return devices.ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Save();
            this.Close();
        }

        private void Restore()
        {
            if (Settings.Default.MicHasSetDefaults)
            {
                this.comboBox1.SelectedIndex = Settings.Default.Speaker;
                this.comboBox2.SelectedIndex = Settings.Default.Mic;
            }
        }

        private void Save()
        {
            Settings.Default.Speaker = this.comboBox1.SelectedIndex;
            Settings.Default.Mic = this.comboBox2.SelectedIndex;
            Settings.Default.MicHasSetDefaults = true;
            Settings.Default.Save();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
