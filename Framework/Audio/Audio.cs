using System;
using System.IO;

namespace Foster.Framework
{
    /// <summary>
    /// The Core Audio Module, used for playing sounds
    /// </summary>
    public abstract class Audio : AppModule
    {
        public string ApiName { get; protected set; } = "Unknown";
        public Version ApiVersion { get; protected set; } = new Version(0, 0, 0);

        internal readonly AudioSourcePool AudioSourcePool = new AudioSourcePool();

        protected internal abstract AudioSource.Platform CreateAudioSource();

        protected Audio() : base(400) { }

        /// <summary>
        /// The Renderer this Audio Module implements
        /// </summary>
        public abstract Renderer Renderer { get; }

        protected internal sealed override void Update()
        {
            AudioSourcePool.Update();
        }
    }
}