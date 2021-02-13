using System;

namespace Foster.Framework
{
    /// <summary>
    /// The Core Graphics Module, used for Rendering
    /// </summary>
    public abstract class Graphics : AppModule
    {

        /// <summary>
        /// The underlying Graphics API Name
        /// </summary>
        public string ApiName { get; protected set; } = "Unknown";

        /// <summary>
        /// The underlying Graphics API version
        /// </summary>
        public Version ApiVersion { get; protected set; } = new Version(0, 0, 0);

        /// <summary>
        /// The Underlying Graphics Device Name
        /// </summary>
        public string DeviceName { get; protected set; } = "Unknown";

        /// <summary>
        /// The Maximum Texture Width and Height supported, in pixels
        /// </summary>
        public int MaxTextureSize { get; protected set; } = 0;

        /// <summary>
        /// Whether the Frame Buffer origin is in the bottom left
        /// </summary>
        public bool OriginBottomLeft { get; protected set; } = false;

        /// <summary>
        /// The Renderer this Graphics Module implements
        /// </summary>
        public abstract Renderer Renderer { get; }

        protected Graphics() : base(200)
        {

        }

        protected internal override void Startup()
        {
            Log.Info($"{ApiName} {ApiVersion} ({DeviceName})");
        }

        /// <summary>
        /// Clears the Color of the Target
        /// </summary>
        public void Clear(RenderTarget target, Color color) =>
            Clear(target, Framework.Clear.Color, color, 0, 0, new RectInt(0, 0, target.RenderWidth, target.RenderHeight));

        /// <summary>
        /// Clears the Target
        /// </summary>
        public void Clear(RenderTarget target, Color color, float depth, int stencil) =>
            Clear(target, Framework.Clear.All, color, depth, stencil, new RectInt(0, 0, target.RenderWidth, target.RenderHeight));

        /// <summary>
        /// Clears the Target
        /// </summary>
        public void Clear(RenderTarget target, Clear flags, Color color, float depth, int stencil, RectInt viewport)
        {
            if (!target.Renderable)
                throw new Exception("Render Target cannot currently be drawn to");

            var bounds = new RectInt(0, 0, target.RenderWidth, target.RenderHeight);
            var clamped = viewport.OverlapRect(bounds);

            ClearInternal(target, flags, color, depth, stencil, clamped);
        }

        /// <summary>
        /// Clears the Target
        /// </summary>
        protected abstract void ClearInternal(RenderTarget target, Clear flags, Color color, float depth, int stencil, RectInt viewport);

        /// <summary>
        /// Draws the data from the Render pass to the Render Target.
        /// This will fail if the Target is not Drawable.
        /// </summary>
        public void Render(ref RenderPass pass)
        {
            if (!pass.Target.Renderable)
                throw new Exception("Render Target cannot currently be drawn to");

            if (!(pass.Target is FrameBuffer) && !(pass.Target is Window))
                throw new Exception("RenderTarget must be a Render Texture or a Window");

            if (pass.Mesh == null)
                throw new Exception("Mesh cannot be null when drawing");

            if (pass.Material == null)
                throw new Exception("Material cannot be null when drawing");

            if (pass.Mesh.InstanceCount > 0 && (pass.Mesh.InstanceFormat == null || (pass.Mesh.InstanceCount < pass.Mesh.InstanceCount)))
                throw new Exception("Trying to draw more Instances than exist in the Mesh");

            if (pass.Mesh.IndexCount < pass.MeshIndexStart + pass.MeshIndexCount)
                throw new Exception("Trying to draw more Indices than exist in the Mesh");

            if (pass.Viewport != null)
            {
                var bounds = new RectInt(0, 0, pass.Target.RenderWidth, pass.Target.RenderHeight);
                pass.Viewport = pass.Viewport.Value.OverlapRect(bounds);
            }

            RenderInternal(ref pass);
        }

        protected abstract void RenderInternal(ref RenderPass pass);

        /// <summary>
        /// Creates a new Color Texture of the given size
        /// </summary>
        protected internal abstract Texture.Platform CreateTexture(int width, int height, TextureFormat format);

        /// <summary>
        /// Creates a new render texture of the given size, with the given amount of color and depth buffers
        /// </summary>
        protected internal abstract FrameBuffer.Platform CreateFrameBuffer(int width, int height, TextureFormat[] attachments);

        /// <summary>
        /// Creates a new Shader from the Shader Source
        /// </summary>
        protected internal abstract Shader.Platform CreateShader(ShaderSource source);

        /// <summary>
        /// Creates a new Mesh
        /// </summary>
        protected internal abstract Mesh.Platform CreateMesh();

        /// <summary>
        /// Gets the Shader Source for the Batch2D
        /// </summary>
        protected internal abstract ShaderSource CreateShaderSourceBatch2D();

    }
}
