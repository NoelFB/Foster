using Foster.Framework;
using System;
using System.Runtime.InteropServices;


namespace Foster.OpenAL
{
    internal class AL_Bindings
    {
        private readonly ISystemOpenAL system;

        public AL_Bindings(ISystemOpenAL system)
        {
            this.system = system ?? throw new Exception("AL Module requires a System that implements ProcAddress");

            CreateDelegate(ref alGetString!, "alGetString");
            CreateDelegate(ref alEnable!, "alEnable");
            CreateDelegate(ref alDisable!, "alDisable");
            CreateDelegate(ref alGetIntegerv!, "alGetIntegerv");
            CreateDelegate(ref alIsEnabled!, "alIsEnabled");
            CreateDelegate(ref alGetErrorString!, "alGetErrorString");
            CreateDelegate(ref alGetString!, "alGetString");
            CreateDelegate(ref alGetInteger!, "alGetInteger");
            CreateDelegate(ref alGetFloat!, "alGetFloat");
            CreateDelegate(ref alGetError!, "alGetError");
            CreateDelegate(ref alIsExtensionPresent!, "alIsExtensionPresent");
            CreateDelegate(ref alGetProcAddress!, "alGetProcAddress");
            CreateDelegate(ref alGetEnumValue!, "alGetEnumValue");
            CreateDelegate(ref alListenerf!, "alListenerf");
            CreateDelegate(ref alListener3f!, "alListener3f");
            CreateDelegate(ref alListenerfv!, "alListenerfv");
            CreateDelegate(ref alGetListenerf!, "alGetListenerf");
            CreateDelegate(ref alGetListener3f!, "alGetListener3f");
            CreateDelegate(ref alGetListenerfv!, "alGetListenerfv");
            CreateDelegate(ref alGenSources!, "alGenSources");
            CreateDelegate(ref alDeleteSources!, "alDeleteSources");
            CreateDelegate(ref alIsSource!, "alIsSource");
            CreateDelegate(ref alSourcef!, "alSourcef");
            CreateDelegate(ref alSource3f!, "alSource3f");
            CreateDelegate(ref alSourcei!, "alSourcei");
            CreateDelegate(ref alSource3i!, "alSource3i");
            CreateDelegate(ref alGetSourcef!, "alGetSourcef");
            CreateDelegate(ref alGetSourcei!, "alGetSourcei");
            CreateDelegate(ref alGetSource3f!, "alGetSource3f");
            CreateDelegate(ref alGetSource3i!, "alGetSource3i");
            CreateDelegate(ref alSourcePlayv!, "alSourcePlayv");
            CreateDelegate(ref alSourceStopv!, "alSourceStopv");
            CreateDelegate(ref alSourceRewindv!, "alSourceRewindv");
            CreateDelegate(ref alSourcePausev!, "alSourcePausev");
            CreateDelegate(ref alSourcePlay!, "alSourcePlay");
            CreateDelegate(ref alSourceStop!, "alSourceStop");
            CreateDelegate(ref alSourceRewind!, "alSourceRewind");
            CreateDelegate(ref alSourcePause!, "alSourcePause");
            CreateDelegate(ref alSourceQueueBuffers!, "alSourceQueueBuffers");
            CreateDelegate(ref alSourceUnqueueBuffers!, "alSourceUnqueueBuffers");
            CreateDelegate(ref alGenBuffers!, "alGenBuffers");
            CreateDelegate(ref alDeleteBuffers!, "alDeleteBuffers");
            CreateDelegate(ref alIsBuffer!, "alIsBuffer");
            CreateDelegate(ref alBufferData!, "alBufferData");
            CreateDelegate(ref alGetBufferi!, "alGetBufferi");
            CreateDelegate(ref alDopplerFactor!, "alDopplerFactor");
            CreateDelegate(ref alDopplerVelocity!, "alDopplerVelocity");
            CreateDelegate(ref alSpeedOfSound!, "alSpeedOfSound");
            CreateDelegate(ref alDistanceModel!, "alDistanceModel");                
        }

        private void CreateDelegate<T>(ref T def, string name) where T : class
        {
            var addr = system.GetALProcAddress(name);
            if (addr != IntPtr.Zero && (Marshal.GetDelegateForFunctionPointer(addr, typeof(T)) is T del))
                def = del;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetString(ALEnum name);
        public GetString alGetString;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Enable(ALEnum mode);
        public Enable alEnable;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Disable(ALEnum mode);
        public Disable alDisable;


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetIntegerv(ALEnum name, out int data);
        public GetIntegerv alGetIntegerv;


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool IsEnabled(ALEnum name);
        public IsEnabled alIsEnabled;


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetErrorString(ALEnum name);
        public GetErrorString alGetErrorString;

        ///// <summary>This function retrieves an OpenAL string property.</summary>
        ///// <param name="param">The human-readable errorstring to be returned.</param>
        ///// <returns>Returns a pointer to a null-terminated string.</returns>
        //public static string GetErrorString(ALError param) => Get((ALGetString)param);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetInteger(ALEnum name);
        public GetInteger alGetInteger;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate float GetFloat(ALEnum name);
        public GetFloat alGetFloat;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate ALEnum GetError();
        public GetError alGetError;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool IsExtensionPresent(string extName); 
        public IsExtensionPresent alIsExtensionPresent;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr GetProcAddress(string fName);
        public GetProcAddress alGetProcAddress;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint GetEnumValue(string ename);
        public GetEnumValue alGetEnumValue;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Listenerf(ALEnum param, float value);
        public Listenerf alListenerf;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Listener3f(ALEnum param, float value1, float value2, float value3);
        public Listener3f alListener3f;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void Listenerfv(ALEnum param, [Out] float* values);
        public Listenerfv alListenerfv;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetListenerf(ALEnum param, out float value);
        public GetListenerf alGetListenerf;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetListener3f(ALEnum param, out float value1, out float value2, out float value3);
        public GetListener3f alGetListener3f;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetListenerfv(ALEnum param, ref float values);
        public GetListenerfv alGetListenerfv;

        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //public delegate void GetListenerfv(ALEnum param, float[] values);
        //public GetListenerfv alGetListenerfv;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void GenSources(int n, IntPtr sources);
        public GenSources alGenSources;


        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //public delegate void GenSources(int n, ref uint sources);
        //public GenSources alGenSources;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void DeleteSources(int n, IntPtr sources);
        public DeleteSources alDeleteSources;

        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //public delegate void DeleteSources(int n, ref uint sources);
        //public DeleteSources alDeleteSources;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool IsSource (uint sid);
        public IsSource alIsSource;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Sourcef (uint sid, ALEnum param, float value);
        public Sourcef alSourcef;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Source3f(uint sid, ALEnum param, float value1, float value2, float value3);
        public Source3f alSource3f;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Sourcei(uint sid, ALEnum param, uint value);
        public Sourcei alSourcei;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void Source3i(uint sid, ALEnum param, uint value1, uint value2, uint value3);
        public Source3i alSource3i;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetSourcef(uint sid, ALEnum param, out float value);
        public GetSourcef alGetSourcef;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetSource3f(uint sid, ALEnum param, out float value1, out float value2, out float value3);
        public GetSource3f alGetSource3f;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetSourcei(uint sid, ALEnum param, out uint value);
        public GetSourcei alGetSourcei;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetSource3i(uint sid, ALEnum param, out uint value1, out uint value2, out uint value3);
        public GetSource3i alGetSource3i;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void SourcePlayv(int ns, IntPtr sids);
        public SourcePlayv alSourcePlayv;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void SourceStopv(int ns, IntPtr sids);
        public SourceStopv alSourceStopv;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void SourceRewindv(int ns, IntPtr sids);
        public SourceRewindv alSourceRewindv;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void SourcePausev(int ns, IntPtr sids);
        public SourcePausev alSourcePausev;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SourcePlay(uint sid);
        public SourcePlay alSourcePlay;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SourceStop(uint sid);
        public SourceStop alSourceStop;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SourceRewind(uint sid);
        public SourceRewind alSourceRewind;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SourcePause(uint sid);
        public SourcePause alSourcePause;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void SourceQueueBuffers(uint sid, uint numEntries, IntPtr bids);
        public SourceQueueBuffers alSourceQueueBuffers;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void SourceUnqueueBuffers(uint sid, uint numEntries, IntPtr bids);
        public SourceUnqueueBuffers alSourceUnqueueBuffers;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void GenBuffers(uint n, IntPtr buffers);
        public GenBuffers alGenBuffers;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public unsafe delegate void DeleteBuffers(uint n, IntPtr buffers);
        public DeleteBuffers alDeleteBuffers;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool IsBuffer(uint bid);
        public IsBuffer alIsBuffer;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void BufferData(uint bid, ALEnum format, IntPtr buffer, uint size, uint freq);
        public BufferData alBufferData;

        // AL_API void AL_APIENTRY alGetBufferf( ALuint bid, ALenum param, ALfloat* value );
        // AL_API void AL_APIENTRY alGetBuffer3f( ALuint bid, ALenum param, ALfloat* value1, ALfloat* value2, ALfloat* value3);
        // AL_API void AL_APIENTRY alGetBufferfv( ALuint bid, ALenum param, ALfloat* values );
        // AL_API void AL_APIENTRY alGetBuffer3i( ALuint bid, ALenum param, ALint* value1, ALint* value2, ALint* value3);
        // AL_API void AL_APIENTRY alGetBufferiv( ALuint bid, ALenum param, ALint* values );
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GetBufferi(uint bid, ALEnum param, [Out] out uint value);
        public GetBufferi alGetBufferi;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DopplerFactor(float value);
        public DopplerFactor alDopplerFactor;

        /// <summary>This function is deprecated and should not be used.</summary>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DopplerVelocity(float value);
        public DopplerVelocity alDopplerVelocity;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SpeedOfSound(float value);
        public SpeedOfSound alSpeedOfSound;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DistanceModel(ALEnum distanceModel);
        public DistanceModel alDistanceModel;    
    }
}