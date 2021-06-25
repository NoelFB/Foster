using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Foster.Framework;

namespace Foster.OpenAL
{
    internal static class AL
    {
        private const string DLL = "openal32.dll";
        internal const CallingConvention ALCallingConvention = CallingConvention.Cdecl;

        [DllImport(DLL, EntryPoint = "alEnable", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Enable(ALCapability capability);

        [DllImport(DLL, EntryPoint = "alDisable", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Disable(ALCapability capability);

        [DllImport(DLL, EntryPoint = "alIsEnabled", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern bool IsEnabled(ALCapability capability);

        [DllImport(DLL, EntryPoint = "alGetString", ExactSpelling = true, CallingConvention = ALCallingConvention, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(ConstCharPtrMarshaler))]
        public static extern string Get(ALGetString param);

        public static string GetErrorString(ALError param) => Get((ALGetString)param);

        [DllImport(DLL, EntryPoint = "alGetInteger", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern int Get(ALGetInteger param);

        [DllImport(DLL, EntryPoint = "alGetFloat", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern float Get(ALGetFloat param);
       
        [DllImport(DLL, EntryPoint = "alGetError", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern ALError GetError();
       
        [DllImport(DLL, EntryPoint = "alIsExtensionPresent", ExactSpelling = true, CallingConvention = ALCallingConvention, CharSet = CharSet.Ansi)]
        public static extern bool IsExtensionPresent([In] string extname);
        
        [DllImport(DLL, EntryPoint = "alGetProcAddress", ExactSpelling = true, CallingConvention = ALCallingConvention, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress([In] string fname);
        
        [DllImport(DLL, EntryPoint = "alGetEnumValue", ExactSpelling = true, CallingConvention = ALCallingConvention, CharSet = CharSet.Ansi)]
        public static extern int GetEnumValue([In] string ename);
      
        [DllImport(DLL, EntryPoint = "alListenerf", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Listener(ALListenerf param, float value);
     
        [DllImport(DLL, EntryPoint = "alListener3f", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Listener(ALListener3f param, float value1, float value2, float value3);
    
        public static void Listener(ALListener3f param, ref Vector3 values)
        {
            Listener(param, values.X, values.Y, values.Z);
        }
    
        [DllImport(DLL, EntryPoint = "alListenerfv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void Listener(ALListenerfv param, float* values);
        
        [DllImport(DLL, EntryPoint = "alListenerfv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Listener(ALListenerfv param, ref float values);
        
        [DllImport(DLL, EntryPoint = "alListenerfv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Listener(ALListenerfv param, float[] values);
       
        public static void Listener(ALListenerfv param, ref Vector3 at, ref Vector3 up)
        {
            Span<float> data = stackalloc float[6];

            data[0] = at.X;
            data[1] = at.Y;
            data[2] = at.Z;

            data[3] = up.X;
            data[4] = up.Y;
            data[5] = up.Z;

            Listener(param, ref data[0]);
        }

        [DllImport(DLL, EntryPoint = "alGetListenerf", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GetListener(ALListenerf param, [Out] out float value);
        
        [DllImport(DLL, EntryPoint = "alGetListener3f", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GetListener(ALListener3f param, [Out] out float value1, [Out] out float value2, [Out] out float value3);
        
        public static void GetListener(ALListener3f param, out Vector3 values)
        {
            GetListener(param, out values.X, out values.Y, out values.Z);
        }

        [DllImport(DLL, EntryPoint = "alGetListenerfv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void GetListener(ALListenerfv param, float* values);
        
        [DllImport(DLL, EntryPoint = "alGetListenerfv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void GetListener(ALListenerfv param, ref float values);
        
        [DllImport(DLL, EntryPoint = "alGetListenerfv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GetListener(ALListenerfv param, [In] float[] values);
        
        public static void GetListener(ALListenerfv param, out Vector3 at, out Vector3 up)
        {
            Span<float> values = stackalloc float[6];
            GetListener(param, ref values[0]);

            at.X = values[0];
            at.Y = values[1];
            at.Z = values[2];

            up.X = values[3];
            up.Y = values[4];
            up.Z = values[5];
        }


        /* Source
         * Sources represent individual sound objects in 3D-space.
         * Sources take the PCM buffer provided in the specified Buffer,
         * apply Source-specific modifications, and then
         * submit them to be mixed according to spatial arrangement etc.
         *
         * Properties include: -
         *

         * Position                          AL_POSITION             ALfloat[3]
         * Velocity                          AL_VELOCITY             ALfloat[3]
         * Direction                         AL_DIRECTION            ALfloat[3]

         * Head Relative Mode                AL_SOURCE_RELATIVE      ALint (AL_TRUE or AL_FALSE)
         * Looping                           AL_LOOPING              ALint (AL_TRUE or AL_FALSE)
         *
         * Reference Distance                AL_REFERENCE_DISTANCE   ALfloat
         * Max Distance                      AL_MAX_DISTANCE         ALfloat
         * RollOff Factor                    AL_ROLLOFF_FACTOR       ALfloat
         * Pitch                             AL_PITCH                ALfloat
         * Gain                              AL_GAIN                 ALfloat
         * Min Gain                          AL_MIN_GAIN             ALfloat
         * Max Gain                          AL_MAX_GAIN             ALfloat
         * Inner Angle                       AL_CONE_INNER_ANGLE     ALint or ALfloat
         * Outer Angle                       AL_CONE_OUTER_ANGLE     ALint or ALfloat
         * Cone Outer Gain                   AL_CONE_OUTER_GAIN      ALint or ALfloat
         *
         * MS Offset                         AL_MSEC_OFFSET          ALint or ALfloat
         * Byte Offset                       AL_BYTE_OFFSET          ALint or ALfloat
         * Sample Offset                     AL_SAMPLE_OFFSET        ALint or ALfloat
         * Attached Buffer                   AL_BUFFER               ALint
         *
         * State (Query only)                AL_SOURCE_STATE         ALint
         * Buffers Queued (Query only)       AL_BUFFERS_QUEUED       ALint
         * Buffers Processed (Query only)    AL_BUFFERS_PROCESSED    ALint
         */

        [DllImport(DLL, EntryPoint = "alGenSources", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void GenSources(int n, [In] int* sources);
       
        [DllImport(DLL, EntryPoint = "alGenSources", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GenSources(int n, ref int sources);
        
        [DllImport(DLL, EntryPoint = "alGenSources", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GenSources(int n, int[] sources);
        
        public static void GenSources(int[] sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }
            GenSources(sources.Length, sources);
        }

        public static void GenSources(Span<int> sources)
        {
            GenSources(sources.Length, ref sources[0]);
        }

        public static int[] GenSources(int n)
        {
            int[] sources = new int[n];
            GenSources(n, sources);
            return sources;
        }

        public static int GenSource()
        {
            int source = 0;
            GenSources(1, ref source);
            return source;
        }

        public static void GenSource(out int source)
        {
            int newSource = 0;
            GenSources(1, ref newSource);
            source = newSource;
        }

        [DllImport(DLL, EntryPoint = "alDeleteSources", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void DeleteSources(int n, [In] int* sources);
        
        [DllImport(DLL, EntryPoint = "alDeleteSources", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void DeleteSources(int n, ref int sources);

        public static void DeleteSources(int[] sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }
            DeleteSources(sources.Length, ref sources[0]);
        }

        public static void DeleteSources(Span<int> sources)
        {
            DeleteSources(sources.Length, ref sources[0]);
        }

        public static void DeleteSource(int source)
        {
            DeleteSources(1, ref source);
        }

        [DllImport(DLL, EntryPoint = "alIsSource", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern bool IsSource(int sid);
        
        [DllImport(DLL, EntryPoint = "alSourcef", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Source(int sid, ALSourcef param, float value);
        
        [DllImport(DLL, EntryPoint = "alSource3f", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Source(int sid, ALSource3f param, float value1, float value2, float value3);
        
        public static void Source(int sid, ALSource3f param, ref Vector3 values)
        {
            Source(sid, param, values.X, values.Y, values.Z);
        }

        [DllImport(DLL, EntryPoint = "alSourcei", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Source(int sid, ALSourcei param, int value);
        
        [DllImport(DLL, EntryPoint = "alSourcei", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Source(int sid, ALSourceb param, bool value);
        
        public static void BindBufferToSource(int source, int buffer)
        {
            Source(source, ALSourcei.Buffer, buffer);
        }

        [DllImport(DLL, EntryPoint = "alSource3i", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void Source(int sid, ALSource3i param, int value1, int value2, int value3);
        
        [DllImport(DLL, EntryPoint = "alGetSourcef", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GetSource(int sid, ALSourcef param, out float value);

        [DllImport(DLL, EntryPoint = "alGetSource3f", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GetSource(int sid, ALSource3f param, out float value1, out float value2, out float value3);
        
        public static void GetSource(int sid, ALSource3f param, out Vector3 values)
        {
            GetSource(sid, param, out values.X, out values.Y, out values.Z);
        }

        [DllImport(DLL, EntryPoint = "alGetSource3i", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GetSource(int sid, ALSource3i param, out int value1, out int value2, out int value3);
        
        public static void GetSource(int sid, ALSource3i param, out Vector3 values)
        {
            int x = 0;
            int y = 0;
            int z = 0;

            GetSource(sid, param, out x, out y, out z);

            values.X = x;
            values.Y = y;
            values.Z = z;
        }
        
        [DllImport(DLL, EntryPoint = "alGetSourcei", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GetSource(int sid, ALGetSourcei param, [Out] out int value);
        
        public static void GetSource(int sid, ALSourceb param, out bool value)
        {
            GetSource(sid, (ALGetSourcei)param, out int result);
            value = result != 0;
        }
        
        [DllImport(DLL, EntryPoint = "alSourcePlayv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void SourcePlay(int ns, [In] int* sids);
        
        [DllImport(DLL, EntryPoint = "alSourcePlayv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourcePlay(int ns, [In] ref int sids);
        
        [DllImport(DLL, EntryPoint = "alSourcePlayv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourcePlay(int ns, [In] int[] sids);
        
        public static void SourcePlay(Span<int> sources)
        {
            SourcePlay(sources.Length, ref sources[0]);
        }

        [DllImport(DLL, EntryPoint = "alSourceStopv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void SourceStop(int ns, [In] int* sids);
        
        [DllImport(DLL, EntryPoint = "alSourceStopv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceStop(int ns, ref int sids);
        
        [DllImport(DLL, EntryPoint = "alSourceStopv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceStop(int ns, int[] sids);
        
        public static void SourceStop(Span<int> sources)
        {
            SourceStop(sources.Length, ref sources[0]);
        }

        [DllImport(DLL, EntryPoint = "alSourceRewindv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void SourceRewind(int ns, [In] int* sids);
        
        [DllImport(DLL, EntryPoint = "alSourceRewindv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceRewind(int ns, ref int sids);
        
        [DllImport(DLL, EntryPoint = "alSourceRewindv", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceRewind(int ns, int[] sids);
        
        public static void SourceRewind(Span<int> sources)
        {
            SourceRewind(sources.Length, ref sources[0]);
        }

        [DllImport(DLL, EntryPoint = "alSourcePausev", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void SourcePause(int ns, [In] int* sids);
        
        [DllImport(DLL, EntryPoint = "alSourcePausev", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourcePause(int ns, ref int sids);
        
        [DllImport(DLL, EntryPoint = "alSourcePausev", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourcePause(int ns, int[] sids);
        
        [DllImport(DLL, EntryPoint = "alSourcePlay", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourcePlay(int sid);
        
        [DllImport(DLL, EntryPoint = "alSourceStop", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceStop(int sid);
        
        [DllImport(DLL, EntryPoint = "alSourceRewind", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceRewind(int sid);
        
        [DllImport(DLL, EntryPoint = "alSourcePause", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourcePause(int sid);
        
        [DllImport(DLL, EntryPoint = "alSourceQueueBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void SourceQueueBuffers(int sid, int numEntries, [In] int* bids);
        
        [DllImport(DLL, EntryPoint = "alSourceQueueBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceQueueBuffers(int sid, int numEntries, int[] bids);
        
        [DllImport(DLL, EntryPoint = "alSourceQueueBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceQueueBuffers(int sid, int numEntries, ref int bids);
        public static void SourceQueueBuffers(int sid, Span<int> buffers)
        {
            SourceQueueBuffers(sid, buffers.Length, ref buffers[0]);
        }

        public static void SourceQueueBuffer(int source, int buffer)
        {
            SourceQueueBuffers(source, 1, ref buffer);
        }

        [DllImport(DLL, EntryPoint = "alSourceUnqueueBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void SourceUnqueueBuffers(int sid, int numEntries, int* bids);
        
        [DllImport(DLL, EntryPoint = "alSourceUnqueueBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceUnqueueBuffers(int sid, int numEntries, int[] bids);

        [DllImport(DLL, EntryPoint = "alSourceUnqueueBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SourceUnqueueBuffers(int sid, int numEntries, ref int bids);

        public static void SourceUnqueueBuffers(int sid, int[] bids)
        {
            SourceUnqueueBuffers(sid, bids.Length, bids);
        }

        public static void SourceUnqueueBuffers(int sid, Span<int> bids)
        {
            SourceUnqueueBuffers(sid, bids.Length, ref bids[0]);
        }

        public static int SourceUnqueueBuffer(int sid)
        {
            int buffer = 0;
            SourceUnqueueBuffers(sid, 1, ref buffer);
            return buffer;
        }

        public static int[] SourceUnqueueBuffers(int sid, int numEntries)
        {
            if (numEntries <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numEntries), "Must be greater than zero.");
            }
            int[] buf = new int[numEntries];
            SourceUnqueueBuffers(sid, numEntries, buf);
            return buf;
        }

        /*
         * Buffer
         * Buffer objects are storage space for sample buffer.
         * Buffers are referred to by Sources. One Buffer can be used
         * by multiple Sources.
         *
         * Properties include: -
         *
         * Frequency (Query only)    AL_FREQUENCY      ALint
         * Size (Query only)         AL_SIZE           ALint
         * Bits (Query only)         AL_BITS           ALint
         * Channels (Query only)     AL_CHANNELS       ALint
         */

        [DllImport(DLL, EntryPoint = "alGenBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void GenBuffers(int n, [Out] int* buffers);

        [DllImport(DLL, EntryPoint = "alGenBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GenBuffers(int n, ref int buffers);

        [DllImport(DLL, EntryPoint = "alGenBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GenBuffers(int n, [Out] int[] buffers);

        public static void GenBuffers(Span<int> buffers)
        {
            GenBuffers(buffers.Length, ref buffers[0]);
        }

        public static int[] GenBuffers(int n)
        {
            int[] buffers = new int[n];
            GenBuffers(buffers.Length, buffers);
            return buffers;
        }

        public static int GenBuffer()
        {
            int buffer = 0;
            GenBuffers(1, ref buffer);
            return buffer;
        }

        public static void GenBuffer(out int buffer)
        {
            int newBuffer = 0;
            GenBuffers(1, ref newBuffer);
            buffer = newBuffer;
        }

        [DllImport(DLL, EntryPoint = "alDeleteBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void DeleteBuffers(int n, [In] int* buffers);

        [DllImport(DLL, EntryPoint = "alDeleteBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void DeleteBuffers(int n, [In] ref int buffers);

        [DllImport(DLL, EntryPoint = "alDeleteBuffers", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void DeleteBuffers(int n, [In] int[] buffers);
 
        public static void DeleteBuffers(int[] buffers)
        {
            if (buffers == null)
            {
                throw new ArgumentNullException(nameof(buffers));
            }
            DeleteBuffers(buffers.Length, buffers);
        }

        public static void DeleteBuffers(Span<int> buffers)
        {
            DeleteBuffers(buffers.Length, ref buffers[0]);
        }

        public static void DeleteBuffer(int buffer)
        {
            DeleteBuffers(1, ref buffer);
        }

        [DllImport(DLL, EntryPoint = "alIsBuffer", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern bool IsBuffer(int bid);

        [DllImport(DLL, EntryPoint = "alBufferData", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void BufferData(int bid, ALFormat format, IntPtr buffer, int size, int freq);

        [DllImport(DLL, EntryPoint = "alBufferData", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static unsafe extern void BufferData(int bid, ALFormat format, void* buffer, int size, int freq);

        [DllImport(DLL, EntryPoint = "alBufferData", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void BufferData(int bid, ALFormat format, ref byte buffer, int size, int freq);

        [DllImport(DLL, EntryPoint = "alBufferData", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void BufferData(int bid, ALFormat format, ref short buffer, int bytes, int freq);

        public static unsafe void BufferData<TBuffer>(int bid, ALFormat format, TBuffer[] buffer, int freq)
            where TBuffer : unmanaged
        {
            fixed (TBuffer* b = buffer)
            {
                BufferData(bid, format, b, buffer.Length * sizeof(TBuffer), freq);
            }
        }

        public static unsafe void BufferData<TBuffer>(int bid, ALFormat format, Span<TBuffer> buffer, int freq)
            where TBuffer : unmanaged
        {
            fixed (TBuffer* b = buffer)
            {
                BufferData(bid, format, b, buffer.Length * sizeof(TBuffer), freq);
            }
        }


        [DllImport(DLL, EntryPoint = "alGetBufferi", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void GetBuffer(int bid, ALGetBufferi param, [Out] out int value);
        
        [DllImport(DLL, EntryPoint = "alDopplerFactor", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void DopplerFactor(float value);

        [DllImport(DLL, EntryPoint = "alDopplerVelocity", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void DopplerVelocity(float value);
        
        [DllImport(DLL, EntryPoint = "alSpeedOfSound", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void SpeedOfSound(float value);
      
        [DllImport(DLL, EntryPoint = "alDistanceModel", ExactSpelling = true, CallingConvention = ALCallingConvention)]
        public static extern void DistanceModel(ALDistanceModel distancemodel);
        
        public static ALDistanceModel GetDistanceModel()
        {
            return (ALDistanceModel)Get(ALGetInteger.DistanceModel);
        }

        public static ALSourceState GetSourceState(int sid)
        {
            GetSource(sid, ALGetSourcei.SourceState, out int state);
            return (ALSourceState)state;
        }

        public static ALSourceType GetSourceType(int sid)
        {
            GetSource(sid, ALGetSourcei.SourceType, out int temp);
            return (ALSourceType)temp;
        }
    }

   
}
