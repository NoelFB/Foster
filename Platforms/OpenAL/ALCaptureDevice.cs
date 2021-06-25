using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foster.OpenAL
{
    public struct ALCaptureDevice
    {
        public static readonly ALCaptureDevice Null = new ALCaptureDevice(IntPtr.Zero);

        public IntPtr Handle;

        public ALCaptureDevice(IntPtr handle)
        {
            Handle = handle;
        }
    }
}