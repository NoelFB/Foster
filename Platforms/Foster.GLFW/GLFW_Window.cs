﻿using System;
using System.Collections.Generic;
using System.Text;
using Foster.Framework;

namespace Foster.GLFW
{
    public class GLFW_Window : Window
    {
        private GLFW_System system;
        private GLFW.Window handle;
        private string title;
        private bool opened;
        private bool visible;

        public override RectInt Bounds
        {
            get
            {
                GLFW.GetWindowPos(handle, out int x, out int y);
                GLFW.GetWindowSize(handle, out int w, out int h);

                return new RectInt(x, y, w, h);
            }

            set
            {
                GLFW.SetWindowPos(handle, value.X, value.Y);
                GLFW.SetWindowSize(handle, value.Width, value.Height);
            }
        }

        public override Point2 DrawSize
        {
            get
            {
                return new Point2();
            }
        }

        public override bool Opened => opened;

        public override string Title
        {
            get => title;
            set => GLFW.SetWindowTitle(handle, title = value);
        }

        public override bool Bordered
        {
            get => GLFW.GetWindowAttrib(handle, GLFW.WindowAttributes.Decorated);
            set => GLFW.SetWindowAttrib(handle, GLFW.WindowAttributes.Decorated, value);
        }

        public override bool Resizable
        {
            get => GLFW.GetWindowAttrib(handle, GLFW.WindowAttributes.Resizable);
            set => GLFW.SetWindowAttrib(handle, GLFW.WindowAttributes.Resizable, value);
        }

        public override bool Fullscreen
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                if (visible)
                    GLFW.ShowWindow(handle);
                else
                    GLFW.HideWindow(handle);
            }
        }

        public GLFW_Window(GLFW_System system, string title, int width, int height, bool visible = true)
        {
            this.system = system;
            this.title = title;

            var share = IntPtr.Zero;
            if (system.Windows.Count > 0)
                share = (system.Windows[0] as GLFW_Window).handle;

            handle = GLFW.CreateWindow(width, height, title, IntPtr.Zero, share);
            opened = true;
            Visible = visible;

            GLFW.SetWindowCloseCallback(handle, (handle) => Close());
            MakeCurrent();
        }

        public override void MakeCurrent()
        {
            if (!opened)
                throw new Exception("This Window has been Closed");

            GLFW.MakeContextCurrent(handle);
        }

        public override void Present()
        {
            if (!opened)
                throw new Exception("This Window has been Closed");

            GLFW.SwapBuffers(handle);
        }

        public override void Close()
        {
            if (opened)
            {
                GLFW.DestroyWindow(handle);
                system.windows.Remove(this);
                opened = false;
            }
        }
    }
}
