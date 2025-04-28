using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Text;

namespace WinHeartApp
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class RustHeart
    {
        // Initialize our external library we use for the signal
        [STAThread]
        [DllImport("rustheart")]
        public static extern IntPtr test_lib();

        /// Freeing
        [DllImport("rustheart")]
        public static extern void free_rust_string(IntPtr pointer_to_clear);

        [DllImport("rustheart")]
        public static extern void free_rust_array(IntPtr pointer_to_clear);

        [DllImport("rustheart")]
        public static extern void free_simple_heart(IntPtr pointer_to_clear);

        // Simple heart handling
        /// <summary>
        /// Creates a simple heart on the rust-heart end with the specified parametres. 
        /// Returns a pointer to the object which needs to be correctly cleared with `free_simple_heart`.
        /// </summary>
        /// <param name="bpm"></param>
        /// <param name="amplitude"></param>
        /// <returns></returns>
        [DllImport("rustheart")]
        public static extern IntPtr build_simple_heart(Int64 bpm, Double amplitude);

        [DllImport("rustheart")]
        public static extern void simple_heart_start(IntPtr handler_pointer, UInt64 freq);


        [DllImport("rustheart")]
        public static extern IntPtr simple_heart_read(IntPtr handler_pointer);

        // Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct F64Array
        {
            public IntPtr data;
            public UIntPtr len;
        }

        /// <summary>
        /// Takes a pointer to a F64Array struct and returns a double array.
        /// Handles cleaning up and freeing the F64Array afterward.
        /// </summary>
        /// <param name="rawDataPtr">Pointer to a F64Array struct.</param>
        /// <returns>double[] array with the data from inside the F64Array struct</returns>
        public static double[] RustF64ToArray(IntPtr rawDataPtr) 
        {
            // Get the pointer to the data struct and nullcheck it
            if (rawDataPtr == IntPtr.Zero) return new double[0];

            // Create the struct from the pointer
            RustHeart.F64Array arrayHolder = Marshal.PtrToStructure<RustHeart.F64Array>(rawDataPtr);

            // Read out the length from the struct we now made and allocate an array for it
            int len = (int)arrayHolder.len;
            double[] read_data = new double[len];

            // Copy the rust data into an array we can work with
            Marshal.Copy(arrayHolder.data, read_data, 0, len);

            // Clear the memory on the rust side as we now copied the data
            RustHeart.free_rust_array(rawDataPtr);

            return read_data;
        }

        public enum NoiseTypes
        {
            MainsNoise,
            RandomNoise
        }

        [DllImport("rustheart")]
        public static extern void simple_heart_add_noise(IntPtr handler_pointer, NoiseTypes noise_type, double amplitude = 0.1f, UInt64 freq = 50);

        [DllImport("rustheart")]
        public static extern void simple_heart_reset_noise(IntPtr handler_pointer);
    }
}
