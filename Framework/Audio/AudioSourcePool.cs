using System;
using System.Collections.Concurrent;

namespace Foster.Framework
{
    internal class AudioSourcePool : IDisposable
    {
        private readonly ConcurrentStack<AudioSource> availableSources;
        private readonly ConcurrentHashSet<AudioSource> playingSources;

        public int Count => availableSources.Count + playingSources.Count;

        public AudioSourcePool()
        {
            availableSources = new ConcurrentStack<AudioSource>();
            playingSources = new ConcurrentHashSet<AudioSource>();
        }

        public AudioSource Reserve(Audio audio, SoundEffect soundEffect)
        {
            if (!availableSources.TryPop(out var source))
            {
                source = new AudioSource(audio, soundEffect)
                {
                    IsPooled = true
                };
            }

            playingSources.Add(source);
            return source;
        }

        public void Free(AudioSource source)
        {
            if (!playingSources.TryRemove(source))
            {
                Log.Error("Audio source is not pooled");
            }
        }

        public void Update()
        {
            foreach (var source in playingSources)
            {
                if (source.GetState() == AudioState.Stopped)
                {
                    if (playingSources.TryRemove(source))
                    {
                        availableSources.Push(source);
                    }
                    else
                    {
                        Log.Message("Failed to remove source; possible race condition");
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var source in playingSources)
            {
                source.Dispose();
            }

            foreach (var source in availableSources)
            {
                source.Dispose();
            }
        }
    }
}
