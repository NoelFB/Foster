using System;
using System.IO;

namespace Foster.Framework
{
    /// <summary>
    /// The Core Audio Module, used for playing sounds
    /// </summary>
    public abstract class Audio : Module
    {
        public string ApiName { get; protected set; } = "Unknown";
        public Version ApiVersion { get; protected set; } = new Version(0, 0, 0);

        internal readonly AudioSourcePool AudioSourcePool = new AudioSourcePool();

        internal abstract AudioSource.Platform CreateAudioSource(SoundEffect soundEffect);
        internal abstract SoundEffect.Platform CreateSoundEffect(Stream stream);

        protected Audio() : base(400) { }

        protected internal sealed override void Update()
        {
            AudioSourcePool.Update();
        }

        public abstract void ProcessEvents();
        public abstract void Suspend();
    }
}
