using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Foster.Framework
{
    /// <summary>
    /// Foster Application
    /// </summary>
    public static class App
    {

        /// <summary>
        /// The Application Name
        /// </summary>
        public static string Name = "";

        /// <summary>
        /// Foster.Framework Version Number
        /// </summary>
        public static readonly Version Version = new Version(0, 1, 0);

        /// <summary>
        /// Whether the Application is running
        /// </summary>
        public static bool Running { get; private set; } = false;

        /// <summary>
        /// Whether the Application is exiting
        /// </summary>
        public static bool Exiting { get; private set; } = false;

        /// <summary>
        /// A List of all the Application Modules
        /// </summary>
        public static readonly ModuleList Modules = new ModuleList();

        /// <summary>
        /// Gets the System Module
        /// </summary>
        public static System System => Modules.Get<System>();

        /// <summary>
        /// Gets the Graphics Module
        /// </summary>
        public static Graphics Graphics => Modules.Get<Graphics>();

        /// <summary>
        /// Gets the Audio Module
        /// </summary>
        public static Audio Audio => Modules.Get<Audio>();

        /// <summary>
        /// Gets the System Input
        /// </summary>
        public static Input Input => System.Input;

        /// <summary>
        /// Gets the Primary Window
        /// </summary>
        public static Window Window => primaryWindow ?? throw new Exception("Application has not yet created a Primary Window");

        /// <summary>
        /// When set to true, this forces the entire application to use Fixed Timestep, including normal Update methods.
        /// </summary>
        public static bool ForceFixedTimestep;

        /// <summary>
        /// Reference to the Primary Window
        /// </summary>
        private static Window? primaryWindow;

        /// <summary>
        /// Starts running the Application
        /// You must register the System Module before calling this
        /// </summary>
        public static void Start(string title, int width, int height, WindowFlags flags = WindowFlags.ScaleToMonitor, Action? callback = null)
        {
            if (Running)
                throw new Exception("App is already running");

            if (Exiting)
                throw new Exception("App is still exiting");

            if (string.IsNullOrWhiteSpace(Name))
                Name = title;

            Log.Info($"Version: {Version}");
            Log.Info($"Platform: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
            Log.Info($"Framework: {RuntimeInformation.FrameworkDescription}");

#if DEBUG
            Launch();
#else
            try
            {
                Launch();
            }
            catch (Exception e)
            {
                var path = System.DefaultUserDirectory(Name);
                if (Modules.Has<System>())
                    path = Modules.Get<System>().UserDirectory(Name);

                Log.Error(e);
                Log.AppendToFile(Name, Path.Combine(path, "ErrorLog.txt"));
                throw e;
            }
#endif
            void Launch()
            {
                // init modules
                Modules.ApplicationStarted();

                if (!Modules.Has<System>())
                    throw new Exception("App requires a System Module to be registered before it can Start");

                // our primary Window
                primaryWindow = new Window(System, title, width, height, flags);
                Modules.FirstWindowCreated();

                // startup application
                Running = true;
                Modules.Startup();
                callback?.Invoke();
                Run();

            }
        }

        private static void Run()
        {
            // timer
            var timer = Stopwatch.StartNew();
            var lastTime = new TimeSpan();
            var fixedAccumulator = TimeSpan.Zero;
            var framecount = 0;
            var frameticks = 0L;

            while (Running)
            {
                var forceFixedTimestep = ForceFixedTimestep;
                if (!forceFixedTimestep)
                    System.Input.Step();

                Modules.FrameStart();

                // get current time & diff
                var currTime = TimeSpan.FromTicks(timer.Elapsed.Ticks);
                var diffTime = (currTime - lastTime);
                lastTime = currTime;

                // fixed timestep update
                {
                    // fixed delta time is always the same
                    var fixedTarget = TimeSpan.FromSeconds(1f / Time.FixedStepTarget);
                    Time.RawDelta = Time.RawFixedDelta = (float)fixedTarget.TotalSeconds;
                    Time.Delta = Time.FixedDelta = Time.RawFixedDelta * Time.DeltaScale;

                    if (forceFixedTimestep)
                    {
                        Time.RawVariableDelta = Time.RawFixedDelta;
                        Time.VariableDelta = Time.FixedDelta;
                    }

                    // increment time
                    fixedAccumulator += diffTime;

                    // if we're forcing fixed timestep and running too fast,
                    // we should sleep the thread while we wait
                    if (forceFixedTimestep)
                    {
                        while (fixedAccumulator < fixedTarget)
                        {
                            Thread.Sleep((int)((fixedTarget - fixedAccumulator).TotalMilliseconds));

                            currTime = TimeSpan.FromTicks(timer.Elapsed.Ticks);
                            diffTime = (currTime - lastTime);
                            lastTime = currTime;
                            fixedAccumulator += diffTime;
                        }
                    }

                    // Do not allow any update to take longer than our maximum.
                    if (fixedAccumulator > Time.FixedMaxElapsedTime)
                        fixedAccumulator = Time.FixedMaxElapsedTime;

                    // do as many fixed updates as we can
                    while (fixedAccumulator >= fixedTarget)
                    {
                        Time.FixedDuration += fixedTarget;
                        fixedAccumulator -= fixedTarget;

                        if (forceFixedTimestep)
                        {
                            Time.Duration += fixedTarget;

                            System.Input.Step();
                            Modules.FixedUpdate();
                            Modules.Update();
                        }
                        else
                        {
                            Modules.FixedUpdate();
                        }

                        if (Exiting)
                            break;
                    }
                }

                // variable timestep update
                if (!forceFixedTimestep)
                {
                    // get Time values
                    Time.Duration += diffTime;
                    Time.RawDelta = Time.RawVariableDelta = (float)diffTime.TotalSeconds;
                    Time.Delta = Time.VariableDelta = Time.RawDelta * Time.DeltaScale;

                    // update
                    Modules.Update();
                }

                Modules.FrameEnd();

                // Check if the Primary Window has been closed
                if (primaryWindow == null || !primaryWindow.Opened)
                    Exit();

                // render
                if (!Exiting)
                {
                    Modules.BeforeRender();

                    for (int i = 0; i < System.Windows.Count; i++)
                        if (System.Windows[i].Opened)
                            System.Windows[i].Render();

                    for (int i = 0; i < System.Windows.Count; i++)
                        if (System.Windows[i].Opened)
                            System.Windows[i].Present();

                    Modules.AfterRender();
                }

                // get fps
                framecount++;
                if (TimeSpan.FromTicks(timer.Elapsed.Ticks - frameticks).TotalSeconds >= 1)
                {
                    Time.FPS = framecount;
                    frameticks = timer.Elapsed.Ticks;
                    framecount = 0;
                }
            }

            // finalize
            Modules.Shutdown();
            primaryWindow = null;
            Exiting = false;

            Log.Info("Exited");
        }

        /// <summary>
        /// Begins Exiting the Application. This will not exit the application immediately and will finish the current Update.
        /// </summary>
        public static void Exit()
        {
            if (Running && !Exiting)
            {
                Running = false;
                Exiting = true;
            }
        }
    }
}
