using System;

namespace Foster.Framework
{
    public class AudioSource : IDisposable
    {
        public abstract class Platform : IDisposable
        {
            internal abstract float Volume { get; set; }
            internal abstract float Pitch { get; set; }
            internal abstract bool Loop { get; set; }

            internal abstract AudioState GetState();

            internal abstract void Pause();
            internal abstract void Play();
            internal abstract void Resume();
            internal abstract void Stop();

            public abstract void Dispose();
        }

        private readonly Platform implementation;

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

        internal AudioSource(Audio audio, SoundEffect soundEffect)
        {
            implementation = audio.CreateAudioSource(soundEffect);
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
