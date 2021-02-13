using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Foster.Framework;
using SDL2;

namespace Foster.SDL2
{
    public class SDL_System : Framework.System, ISystemOpenGL
    {
        public override bool SupportsMultipleWindows => true;
        public override Input Input => input;

        internal readonly Dictionary<IntPtr, SDL_GLContext> glContexts = new Dictionary<IntPtr, SDL_GLContext>();
        internal readonly SDL_Input input;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDPIAware();

        public SDL_System()
        {
            ApiName = "SDL2";

            SDL.SDL_GetVersion(out var ver);
            ApiVersion = new Version(ver.major, ver.major, ver.patch);

            input = new SDL_Input();
        }

        protected override void ApplicationStarted()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                SetProcessDPIAware();

            // init SDL
            if (SDL.SDL_Init(SDL.SDL_INIT_TIMER | SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_EVENTS) != 0)
            {
                var error = SDL.SDL_GetError();
                throw new Exception($"Failed to start SDL2: {error}");
            }

            // OpenGL attributes
            if (App.Graphics is IGraphicsOpenGL)
            {
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 3);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS, (int)SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
            }

            // Displays
            var numDisplays = SDL.SDL_GetNumVideoDisplays();
            for (int i = 0; i < numDisplays; i ++)
                monitors.Add(new SDL_Monitor(i));
        }

        protected override Window.Platform CreateWindow(string title, int width, int height, WindowFlags flags = WindowFlags.None)
        {
            return new SDL_Window(this, title, width, height, flags);
        }

        protected override void Update()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                switch (e.type)
                {
                    // Quit
                    case SDL.SDL_EventType.SDL_QUIT:
                        App.Exit();
                        return;

                    // Window Event
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        {
                            // find the window
                            SDL_Window? window = null;
                            for (int i = 0; i < Windows.Count; i++)
                            {
                                if (Windows[i].Implementation is SDL_Window win && win.SDLWindowID == e.window.windowID)
                                {
                                    window = win;
                                    break;
                                }
                            }

                            if (window == null)
                                continue;

                            switch (e.window.windowEvent)
                            {
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                    window.Resized();
                                    break;

                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                                    window.CloseRequested();
                                    break;

                                // Update visible
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                                    window.Restored();
                                    break;
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN:
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                                    window.Minimized();
                                    break;

                                // Call onFocus
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_TAKE_FOCUS:
                                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                                    window.FocusGained();
                                    break;
                            };
                        }
                        break;
                    
                    // Input Events
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                    case SDL.SDL_EventType.SDL_KEYUP:
                    case SDL.SDL_EventType.SDL_TEXTEDITING:
                    case SDL.SDL_EventType.SDL_TEXTINPUT:
                    case SDL.SDL_EventType.SDL_KEYMAPCHANGED:
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    case SDL.SDL_EventType.SDL_JOYAXISMOTION:
                    case SDL.SDL_EventType.SDL_JOYBALLMOTION:
                    case SDL.SDL_EventType.SDL_JOYHATMOTION:
                    case SDL.SDL_EventType.SDL_JOYBUTTONDOWN:
                    case SDL.SDL_EventType.SDL_JOYBUTTONUP:
                    case SDL.SDL_EventType.SDL_JOYDEVICEADDED:
                    case SDL.SDL_EventType.SDL_JOYDEVICEREMOVED:
                    case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
                    case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                    case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
                    case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                    case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                    case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMAPPED:
                        input.ProcessEvent(e);
                        break;
                }


                var error = SDL.SDL_GetError();
                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine(e.type + ": " + error);
                SDL.SDL_ClearError();
            }
        }

        #region ISystemOpenGL Method Calls

        public IntPtr GetGLProcAddress(string name)
        {
            return SDL.SDL_GL_GetProcAddress(name);
        }

        public ISystemOpenGL.Context CreateGLContext()
        {
            if (!(Windows[0].Implementation is SDL_Window window))
                throw new Exception("All Windows have been closed and a Context cannot be created");

            return new SDL_GLContext(this, window.SDLWindowPtr);
        }

        public ISystemOpenGL.Context GetWindowGLContext(Window window)
        {
            return (window.Implementation as SDL_Window)?.glContext ?? throw new Exception("Window is missing a GL Context");
        }

        public ISystemOpenGL.Context? GetCurrentGLContext()
        {
            var ptr = SDL.SDL_GL_GetCurrentContext();

            if (ptr != IntPtr.Zero)
                return glContexts[ptr];

            return null;
        }

        public void SetCurrentGLContext(ISystemOpenGL.Context? context)
        {
            if (context is SDL_GLContext ctx && ctx != null)
                SDL.SDL_GL_MakeCurrent(ctx.window, ctx.context);
            else
                SDL.SDL_GL_MakeCurrent(IntPtr.Zero, IntPtr.Zero);
        }

        #endregion
    }
}
