using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Foster.Framework;

namespace Foster.OpenAL
{
    internal static class ALBackup
    {
        public static int MajorVersion;
        public static int MinorVersion;
        public static int AttributesSize;
        public static int AllAttributes;
        public static int CaptureSamples;
        public static int EfxMajorVersion;
        public static int EfxMinorVersion;
        public static int EfxMaxAuxiliarySends;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private static AL_Bindings bindings;
        private static OnError onErrorRef;
#pragma warning restore CS8618

        public static void Init(AL_Audio audio, ISystemOpenAL system)
        {
            bindings = new AL_Bindings(system);

            GetIntegerv((ALEnum)0x1000, out MajorVersion);
            GetIntegerv((ALEnum)0x1001, out MinorVersion);
            GetIntegerv((ALEnum)0x1002, out AttributesSize);
            GetIntegerv((ALEnum)0x1003, out AllAttributes);
            GetIntegerv((ALEnum)0x312, out CaptureSamples);
            GetIntegerv((ALEnum)0x20001, out EfxMajorVersion);
            GetIntegerv((ALEnum)0x20002, out EfxMinorVersion);
            GetIntegerv((ALEnum)0x20003, out EfxMaxAuxiliarySends);
#if DEBUG

#endif
        }

        private delegate void OnError(ALEnum source, ALEnum type, uint id, ALEnum severity, uint length, IntPtr message, IntPtr userParam);

        public static unsafe string GetString(ALEnum name)
        {
            return Marshal.PtrToStringAnsi(bindings.alGetString(name)) ?? "";
        }

        public static void Enable(ALEnum name) => bindings.alEnable(name);

        public static void Disable(ALEnum name) => bindings.alDisable(name);

        public static void GetIntegerv(ALEnum name, out int data) => bindings.alGetIntegerv(name, out data);

        public static bool IsEnabled(ALEnum name) => bindings.alIsEnabled(name);

        public static unsafe string GetErrorString(ALEnum name)
        {
            return Marshal.PtrToStringAnsi(bindings.alGetErrorString(name));
        }
        
        public static uint GetInteger(ALEnum name) => bindings.alGetInteger(name);
        
        public static float GetFloat(ALEnum name) => bindings.alGetFloat(name);

        public static ALEnum GetError() => bindings.alGetError();

        public static bool IsExtensionPresent(string extName) => bindings.alIsExtensionPresent(extName);


        public static unsafe IntPtr GetProcAddress(string fname)
        {
            return bindings.alGetProcAddress(fname);
        }

        public static uint GetEnumValue(string ename) => bindings.alGetEnumValue(ename);

        public static void Listenerf(ALEnum parm, float value) => bindings.alListenerf(parm, value);

        public static void Listener3f(ALEnum parm, float value1, float value2, float value3) => bindings.alListener3f(parm, value1, value2, value3);

        public unsafe static void Listenerfv(ALEnum parm, float* value) => bindings.alListenerfv(parm, value);

        public static void Getlistenerf(ALEnum parm, out float value) => bindings.alGetListenerf(parm, out value);

        public static void Getlistenerf(ALEnum parm, out float value1, out float value2, out float value3) => bindings.alGetListener3f(parm, out value1, out value2, out value3);

        public static void GetListenerfv(ALEnum parm, ref float value) => bindings.alGetListenerfv(parm, ref value);

        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //public delegate void GetListenerfv(ALEnum param, float[] values);
        //public GetListenerfv alGetListenerfv;

        //public unsafe static void GenSources(int n, IntPtr sources) => bindings.alGenSources(n, sources);
        public unsafe static void GenSources(int n, IntPtr sources) => bindings.alGenSources(n, sources);

        public unsafe static uint GenSource()
        {
            uint id;
            bindings.alGenSources(1, new IntPtr(&id));
            return id;
        }


        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //public delegate void GenSources(int n, ref uint sources);
        //public GenSources alGenSources;

        //public unsafe static void DeleteSources(int n, IntPtr sources) => bindings.alDeleteSources(n, sources);
        public unsafe static void DeleteSources(int n, IntPtr sources) => bindings.alDeleteSources(n, sources);

        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //public delegate void DeleteSources(int n, ref int sources);
        //public DeleteSources alDeleteSources;

        public static bool IsSource(uint sid) => bindings.alIsSource(sid);

        public static void Sourcef(uint sid, ALEnum param, float value) => bindings.alSourcef(sid, param, value);

        public static void Source3f(uint sid, ALEnum param, float value1, float value2, float value3) => bindings.alSource3f(sid, param, value1, value2, value3);

        public static void Sourcei(uint sid, ALEnum param, uint value) => bindings.alSourcef(sid, param, value);
        
        public static void Source3i(uint sid, ALEnum param, uint value1, uint value2, uint value3) => bindings.alSource3i(sid, param, value1, value2, value3);

        public unsafe static void GetSourcef(uint sid, ALEnum param, out float value) => bindings.alGetSourcef(sid, param, out value);

        public unsafe static void GetSource3f(uint sid, ALEnum param, out float value1, out float value2, out float value3) => bindings.alGetSource3f(sid, param, out value1, out value2, out value3);

        public unsafe static void GetSourcei(uint sid, ALEnum param, out uint value) => bindings.alGetSourcei(sid, param, out value);

        public unsafe static void GetSource3i(uint sid, ALEnum param, out uint value1, out uint value2, out uint value3) => bindings.alGetSource3i(sid, param, out value1, out value2, out value3);

        public unsafe static void SourcePlayv(int ns, IntPtr sids) => bindings.alSourcePlayv(ns, sids);
        
        public unsafe static void SourceStopv(int ns, IntPtr sids) => bindings.alSourceStopv(ns, sids);

        public unsafe static void SourceRewindv(int ns, IntPtr sids) => bindings.alSourceRewindv(ns, sids);
        
        public unsafe static void SourcePausev(int ns, IntPtr sids) => bindings.alSourcePausev(ns, sids);

        public unsafe static void SourcePlay(uint sid) => bindings.alSourcePlay(sid);

        public unsafe static void SourceStop(uint sid) => bindings.alSourceStop(sid);

        public unsafe static void SourceRewind(uint sid) => bindings.alSourceRewind(sid);

        public unsafe static void SourcePause(uint sid) => bindings.alSourcePause(sid);

        public unsafe static void SourceQueueBuffers(uint sid, uint numEntries, IntPtr bids) => bindings.alSourceQueueBuffers(sid, numEntries, bids);

        public unsafe static void SourceUnqueueBuffers(uint sid, uint numEntries, IntPtr bids) => bindings.alSourceUnqueueBuffers(sid, numEntries, bids);

        public unsafe static void GenBuffers(uint n, IntPtr buffers) => bindings.alGenBuffers(n, buffers);

        public unsafe static uint GenBuffer()
        {        
            uint id;
            bindings.alGenBuffers(1, new IntPtr(&id));
            return id;
        }

        public unsafe static void DeleteBuffers(uint n, [Out] IntPtr buffers) => bindings.alDeleteBuffers(n, buffers);

        public static bool IsBuffer(uint bid) => bindings.alIsBuffer(bid);

        public static void BufferData(uint bid, ALEnum format, IntPtr buffer, uint size, uint freq) => bindings.alBufferData(bid, format, buffer, size, freq);

        public unsafe static void GetBufferi(uint bid, ALEnum param, [Out] out uint value) => bindings.alGetBufferi(bid, param, out value);

        public static uint GetBufferi(ALEnum param)
        {
            uint value = 0;
            bindings.alGetBufferi(1, param, out value);
            return value;
        }
        
        public static void DopplerFactor(float value) => bindings.alDopplerFactor(value);

        public static void DopplerVelocity(float value) => bindings.alDopplerVelocity(value);

        public static void SpeedOfSound(float value) => bindings.alSpeedOfSound(value);

        public static void DistanceModel(ALEnum distanceModel) => bindings.alDistanceModel(distanceModel);
    }


    internal enum ALEnum
    {
        //Capability
        Invalid = -1,

        //Listener
        Gain = 0x100A,
        EfxMetersPerUnit = 0x20004,

        //Vector3 Listener
        Position = 0x1004,
        Velocity = 0x1006,

        //Float[] Listener
        Orientation = 0x100F,

        //Float Source
        ReferenceDistance = 0x1020,
        MaxDistance = 0x1023,
        RolloffFactor = 0x1021,
        Pitch = 0x1003,
        //Gain = 0x100A,
        MinGain = 0x100D,
        MaxGain = 0x100E,
        ConeInnerAngle = 0x1001,
        ConeOuterAngle = 0x1002,
        ConeOuterGain = 0x1022,
        SecOffset = 0x1024, // AL_EXT_OFFSET extension.
        EfxAirAbsorptionFactor = 0x20007,
        EfxRoomRolloffFactor = 0x20008,
        EfxConeOuterGainHighFrequency = 0x20009,

        //Vector3 Source
        Direction = 0x1005,

        //8-Bit Boolean Source
        SourceRelative = 0x202,
        Looping = 0x1007,
        EfxDirectFilterGainHighFrequencyAuto = 0x2000A,
        EfxAuxiliarySendFilterGainAuto = 0x2000B,
        EfxAuxiliarySendFilterGainHighFrequencyAuto = 0x2000C,

        //Int32 Source
        ByteOffset = 0x1026,  // AL_EXT_OFFSET extension.
        SampleOffset = 0x1025, // AL_EXT_OFFSET extension.
        Buffer = 0x1009,
        BuffersQueued = 0x1015,
        BuffersProcessed = 0x1016,
        SourceType = 0x1027,
        SourceState = 0x1010,
        EfxDirectFilter = 0x20005,

        //3x Int32 Source
        //Position = 0x1004,
        //Velocity = 0x1006,
        //Direction = 0x1005,

        /// <summary>Deprecated. Specify the channel mask. (Creative) Type: uint Range: [0 - 255]</summary>
        //ChannelMask = 0x3000,

        //Source State
        Initial = 0x1011,
        Playing = 0x1012,
        Paused = 0x1013,
        Stopped = 0x1014,

        //Source Type
        Static = 0x1028,
        Streaming = 0x1029,
        Undetermined = 0x1030,

        //Format
        Mono8 = 0x1100,
        Mono16 = 0x1101,
        Stereo8 = 0x1102,
        Stereo16 = 0x1103,
        MonoALawExt = 0x10016,
        StereoALawExt = 0x10017,
        MonoMuLawExt = 0x10014,
        StereoMuLawExt = 0x10015,
        VorbisExt = 0x10003,
        Mp3Ext = 0x10020,
        MonoIma4Ext = 0x1300,
        StereoIma4Ext = 0x1301,
        MonoFloat32Ext = 0x10010,
        StereoFloat32Ext = 0x10011,
        MonoDoubleExt = 0x10012,
        StereoDoubleExt = 0x10013,
        Multi51Chn16Ext = 0x120B,
        Multi51Chn32Ext = 0x120C,
        Multi51Chn8Ext = 0x120A,
        Multi61Chn16Ext = 0x120E,
        Multi61Chn32Ext = 0x120F,
        Multi61Chn8Ext = 0x120D,
        Multi71Chn16Ext = 0x1211,
        Multi71Chn32Ext = 0x1212,
        Multi71Chn8Ext = 0x1210,
        MultiQuad16Ext = 0x1205,
        MultiQuad32Ext = 0x1206,
        MultiQuad8Ext = 0x1204,
        MultiRear16Ext = 0x1208,
        MultiRear32Ext = 0x1209,
        MultiRear8Ext = 0x1207,

        //Get Buffers Int32
        Frequency = 0x2001,
        Bits = 0x2002,
        Channels = 0x2003,
        Size = 0x2004,

        //Buffer State
        Unused = 0x2010,
        Pending = 0x2011,
        Processed = 0x2012,

        //Errors
        NoError = 0,
        InvalidName = 0xA001,
        IllegalEnum = 0xA002,
        InvalidEnum = 0xA002,
        InvalidValue = 0xA003,
        IllegalCommand = 0xA004,
        InvalidOperation = 0xA004,
        OutOfMemory = 0xA005,

        //Get String
        Vendor = 0xB001,
        Version = 0xB002,
        Renderer = 0xB003,
        Extensions = 0xB004,

        //Get Float
        DopplerFactor = 0xC000,
        DopplerVelocity = 0xC001,
        SpeedOfSound = 0xC003,

        //Get Int
        DistanceModel = 0xD000,

        //Get Distance Model
        None = 0,
        InverseDistance = 0xD001,
        InverseDistanceClamped = 0xD002,
        LinearDistance = 0xD003,
        LinearDistanceClamped = 0xD004,
        ExponentDistance = 0xD005,
        ExponentDistanceClamped = 0xD006,
    }
}
