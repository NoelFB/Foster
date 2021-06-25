using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foster.Framework;

namespace Foster.OpenAL
{
    public class AL_Audio : Audio
    {

        // Background Context can be null up until Startup, at which point they never are again
        internal ISystemOpenAL.Context BackgroundContext = null!;

        public override Renderer Renderer => Renderer.OpenAL;

        protected override void ApplicationStarted()
        {
            ApiName = "OpenAL";
        }

        // various resources waiting to be deleted
        internal List<int> BuffersToDelete = new List<int>();
        internal List<int> SourcesToDelete = new List<int>();

        private ALContext context;
        private ALDevice device;

        // stored delegates for deleting graphics resources
        private delegate void DeleteResource(int id);

        protected override void FirstWindowCreated()
        {
            Console.WriteLine("Hello!");
            var devices = ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);
            Console.WriteLine($"Devices: {string.Join(", ", devices)}");

            // Get the default device, then go though all devices and select the AL soft device if it exists.
            string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
            foreach (var d in devices)
            {
                if (d.Contains("OpenAL Soft"))
                {
                    deviceName = d;
                }
            }

           // var allDevices = Extensions.Creative.EnumerateAll.EnumerateAll.GetStringList(Extensions.Creative.EnumerateAll.GetEnumerateAllContextStringList.AllDevicesSpecifier);
           // Console.WriteLine($"All Devices: {string.Join(", ", allDevices)}");

            device = ALC.OpenDevice(deviceName);
            context = ALC.CreateContext(device, (int[])null);
            ALC.MakeContextCurrent(context);

            //AL.Init(this, System);
            //ALC.Init(this, System);

            ALC.GetInteger(device, AlcGetInteger.MajorVersion, 1, out int alcMajorVersion);
            ALC.GetInteger(device, AlcGetInteger.MinorVersion, 1, out int alcMinorVersion);
            string alcExts = ALC.GetString(device, AlcGetString.Extensions);

            ALC.Attributes.MajorVersion = alcMajorVersion;
            ALC.Attributes.MinorVersion = alcMinorVersion;

            var attrs = ALC.GetContextAttributes(device);
            Console.WriteLine($"Attributes: {attrs}");

            string exts = AL.Get(ALGetString.Extensions);
            string rend = AL.Get(ALGetString.Renderer);
            string vend = AL.Get(ALGetString.Vendor);
            string vers = AL.Get(ALGetString.Version);


            ApiVersion = new Version(ALC.Attributes.MajorVersion, ALC.Attributes.MinorVersion);
           // ApiName = AL.GetString(ALEnum.Renderer);
           
           // BackgroundContext = System.CreateALContext();
        }

 
        protected override AudioSource.Platform CreateAudioSource()
        {
            return new AL_AudioSource(this);
        }


        protected override void Shutdown()
        {
            //BackgroundContext.Dispose();

            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(context);
            ALC.CloseDevice(device);
        }
    }
}
