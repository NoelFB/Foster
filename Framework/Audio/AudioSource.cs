using System;
using System.IO;

namespace Foster.Framework
{
    public class AudioSource : IDisposable
    {
        public abstract class Platform
        {
            protected internal abstract void Init(AudioSource source, Stream stream);

            protected internal abstract float Volume { get; set; }
            protected internal abstract float Pitch { get; set; }
            protected internal abstract bool Loop { get; set; }

            protected internal abstract AudioState GetState();

            protected internal abstract void Pause();
            protected internal abstract void Play();
            protected internal abstract void Resume();

            protected internal abstract void Rewind();

            protected internal abstract void Stop();


            protected internal abstract void Dispose();
        }

        private readonly Platform implementation;

        private readonly Audio audio;

        internal bool IsPooled = false;

        public float Volume
        {
            get => implementation.Volume;
            set => implementation.Volume = value;
        }

        public float Pitch
        {
            get => implementation.Pitch;
            set => implementation.Pitch = value;
        }

        public bool Loop
        {
            get => implementation.Loop;
            set => implementation.Loop = value;
        }

        public AudioSource(Audio audio, Stream stream)
        {
            this.audio = audio;
            implementation = audio.CreateAudioSource();
            implementation.Init(this, stream);
        }

        public AudioSource(Stream stream)
            : this(App.Audio, stream)
        {
        }

        public AudioSource(string fileName)
            : this(App.Audio, File.Open(fileName, FileMode.Open))
        {
        }

        public AudioState GetState()
        {
            return implementation.GetState();
        }

        public void Pause()
        {
            implementation.Pause();
        }

        public void Play()
        {

            implementation.Play();
        }

        public void Resume()
        {
            implementation.Resume();
        }

        public void Stop()
        {
            implementation.Stop();
        }

        public void Dispose()
        {
            implementation.Dispose();
        }
    }
}