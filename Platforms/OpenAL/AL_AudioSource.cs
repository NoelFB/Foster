using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Foster.Framework;

namespace Foster.OpenAL
{
    internal class AL_AudioSource : AudioSource.Platform
    {
        public int BufferID { get; private set; }

        public int SourceID { get; private set; }

        private float volume;
        protected override float Volume
        {
            get { return volume; }
            set
            {
                AL.Source(this.SourceID, ALSourcef.Gain, volume = value);               
            }
        }

        private float pitch;
        protected override float Pitch
        {
            get { return pitch; }
            set
            {
                AL.Source(this.SourceID, ALSourcef.Pitch, value);
            }

        }

        private bool loop;
        protected override bool Loop
        {
            get { return loop; }
            set
            {
                AL.Source(this.SourceID, ALSourceb.Looping, value);
            }
        }


        private readonly AL_Audio audio;
        private AudioSource audioSource;

        internal AL_AudioSource(AL_Audio audio)
        {
            this.audio = audio;
            audioSource = null!;
        }

        ~AL_AudioSource()
        {
            Dispose();
        }

        protected override void Init(AudioSource audioSource, Stream stream)
        {
            this.audioSource = audioSource;
            Initialize(stream);
        }

        void Initialize(Stream stream)
        {
            int bits;     
            int rate;
            int frequency;
            int sampleCount;
            byte[] soundData;
            Wave.Format format;
            AudioChannel audioChannel;

            this.BufferID = AL.GenBuffer();       
            int chunkSize;

            Wave.TryLoad(stream, out soundData, out format, out frequency, out audioChannel, out bits, out rate, out chunkSize, out sampleCount);
            AL.BufferData<Byte>(this.BufferID, GetSoundFormat(audioChannel, bits), soundData, frequency);
       
            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                Console.WriteLine("error loading buffer: " + error);
            }

         
            AL.Listener(ALListenerf.Gain, 0.1f);

            this.SourceID = AL.GenSource();

            AL.Listener(ALListener3f.Position, 0, 0, 0);
            AL.Source(this.SourceID, ALSourcef.Gain, 1f);
            AL.Source(this.SourceID, ALSourcei.Buffer, this.BufferID);
        }

        private ALFormat GetSoundFormat(AudioChannel channels, int bitsPerSample)
        {
            switch (channels)
            {
                case AudioChannel.Mono: return bitsPerSample == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
                case AudioChannel.Stereo: return bitsPerSample == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
                default: throw new NotSupportedException("The specified sound format is not supported.");
            }
        }

        protected override AudioState GetState()
        {          
            switch (AL.GetSourceState(this.SourceID))
            {
                case ALSourceState.Playing: { return AudioState.Playing; }
                case ALSourceState.Stopped: { return AudioState.Stopped; }
                case ALSourceState.Paused: { return AudioState.Paused; }         
            }

            return AudioState.Unknown;
        }

        protected override void Stop()
        {
            AL.SourceStop(this.SourceID);
        }
        
        protected override void Play()
        {
            AL.SourcePlay(this.SourceID);
        }

        protected override void Pause()
        {
            AL.SourcePause(this.SourceID);
        }

        protected override void Rewind()
        {
            AL.SourceRewind(this.SourceID);
        }

        protected override void Resume()
        {
            AL.SourcePlay(this.SourceID);
        }

        protected override void Dispose()
        {
            if (this.BufferID != 0)
            {
                this.audio.BuffersToDelete.Add(this.BufferID);
                this.BufferID = 0;
            }
            if (this.SourceID != 0)
            {
                this.audio.SourcesToDelete.Add(this.SourceID);
                this.SourceID = 0;
            }
        }

    }
}
