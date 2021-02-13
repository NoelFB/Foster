using System;
using System.IO;

namespace Foster.Framework
{
    public class SoundEffect : IDisposable
    {
        public static SoundEffect? FromFile(Audio audio, string filename)
        {
            try
            {
                using var stream = File.OpenRead(filename);
                return new SoundEffect(audio, stream);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load sound effect {filename}");
                Log.Error(e.Message);
                return null;
            }
        }

        public abstract class Platform : IDisposable
        {
            internal abstract TimeSpan Duration { get; }

            public abstract void Dispose();
        }

        private readonly Audio audio;
        internal readonly Platform Implementation;

        public TimeSpan Duration => Implementation.Duration;

        private SoundEffect(Audio audio, Stream stream)
        {
            this.audio = audio;
            Implementation = audio.CreateSoundEffect(stream);
        }

        public void Play(float volume = 1.0f, float pitch = 0.0f)
        {
            var source = audio.AudioSourcePool.Reserve(audio, this);

            source.Volume = volume;
            source.Pitch = pitch;
            source.Loop = false;

            source.Play();
        }

        public SoundEffectInstance CreateInstance()
        {
            return new SoundEffectInstance(audio, this);
        }

        public void Dispose()
        {
            Implementation.Dispose();
        }
    }
}
