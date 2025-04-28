using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinHeartApp
{
    public partial class Form1 : Form
    {
        IntPtr initialized_heart_prt = IntPtr.Zero;
        ConcurrentBag<double> read_values = new ConcurrentBag<double>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button_test_Click(object sender, EventArgs e)
        {
            IntPtr test_message_ptr = RustHeart.test_lib();
            MessageBox.Show(Marshal.PtrToStringAnsi(test_message_ptr), "DLL test", MessageBoxButtons.OK);
            RustHeart.free_rust_string(test_message_ptr);
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            initialized_heart_prt = RustHeart.build_simple_heart(60, 1.0f);
            RustHeart.simple_heart_add_noise(initialized_heart_prt, RustHeart.NoiseTypes.MainsNoise, 0.05f);
            RustHeart.simple_heart_add_noise(initialized_heart_prt, RustHeart.NoiseTypes.RandomNoise);
            RustHeart.simple_heart_start(initialized_heart_prt, 1000);
            timer1.Interval = 100;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Read the data and turn it into an Array, this automatically cleans up for us as well
            double[] data_read = RustHeart.RustF64ToArray(RustHeart.simple_heart_read(initialized_heart_prt));

            // Append the data to all our read values
            foreach (double item in data_read)
            {
                read_values.Add(item);
            }


            if (read_values.Count >= 1000)
            {
                timer1.Stop();
                foreach (float item in read_values)
                {
                    chart1.Series["ECG"].Points.Add(item);
                }
                MessageBox.Show("Read of data done", "Data read", MessageBoxButtons.OK);
            }
        }
    }
}
