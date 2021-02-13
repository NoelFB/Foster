using Foster.Framework;
using SDL2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Foster.SDL2
{
    internal class SDL_Input : Input
    {
        private readonly IntPtr[] sdlCursors;
        private readonly IntPtr[] sdlJoysticks;
        private readonly IntPtr[] sdlGamepads;

        public SDL_Input()
        {
            sdlCursors = new IntPtr[(int)SDL.SDL_SystemCursor.SDL_NUM_SYSTEM_CURSORS];
            sdlJoysticks = new IntPtr[64];
            sdlGamepads = new IntPtr[64];
        }

        ~SDL_Input()
        {
            for (int i = 0; i < sdlCursors.Length; i++)
            {
                if (sdlCursors[i] != IntPtr.Zero)
                    SDL.SDL_FreeCursor(sdlCursors[i]);
                sdlCursors[i] = IntPtr.Zero;
            }
        }

        public override void SetMouseCursor(Cursors cursors)
        {
            int index = cursors switch
            {
                Cursors.Default => (int)SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW,
                Cursors.Crosshair => (int)SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR,
                Cursors.Hand => (int)SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND,
                Cursors.HorizontalResize => (int)SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE,
                Cursors.VerticalResize => (int)SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS,
                Cursors.IBeam => (int)SDL.SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM,
                _ => throw new NotImplementedException()
            };

            if (sdlCursors[index] == IntPtr.Zero)
                sdlCursors[index] = SDL.SDL_CreateSystemCursor((SDL.SDL_SystemCursor)index);

            SDL.SDL_SetCursor(sdlCursors[index]);
        }

        public override string? GetClipboardString()
        {
            if (SDL.SDL_HasClipboardText() == SDL.SDL_bool.SDL_TRUE)
                return SDL.SDL_GetClipboardText();
            return null;
        }

        public override void SetClipboardString(string value)
        {
            SDL.SDL_SetClipboardText(value);
        }

        public void ProcessEvent(SDL.SDL_Event e)
        {
            if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)
            {
                OnMouseDown(e.button.button switch
                {
                    (int)SDL.SDL_BUTTON_LEFT => MouseButtons.Left,
                    (int)SDL.SDL_BUTTON_RIGHT => MouseButtons.Right,
                    (int)SDL.SDL_BUTTON_MIDDLE => MouseButtons.Middle,
                    _ => MouseButtons.Unknown,
                });
            }
            else if (e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)
            {
                OnMouseUp(e.button.button switch
                {
                    (int)SDL.SDL_BUTTON_LEFT => MouseButtons.Left,
                    (int)SDL.SDL_BUTTON_RIGHT => MouseButtons.Right,
                    (int)SDL.SDL_BUTTON_MIDDLE => MouseButtons.Middle,
                    _ => MouseButtons.Unknown,
                });
            }
            else if (e.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
            {
                OnMouseWheel(e.wheel.x, e.wheel.y);
            }
            else if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                OnMouseMotion(e.motion.x, e.motion.y);
            }
            // joystick
            else if (e.type == SDL.SDL_EventType.SDL_JOYDEVICEADDED)
            {
                var index = e.jdevice.which;

                if (SDL.SDL_IsGameController(index) == SDL.SDL_bool.SDL_FALSE)
                {
                    var ptr = sdlJoysticks[index] = SDL.SDL_JoystickOpen(index);
                    var name = SDL.SDL_JoystickName(ptr);
                    var buttonCount = SDL.SDL_JoystickNumButtons(ptr);
                    var axisCount = SDL.SDL_JoystickNumAxes(ptr);

                    OnJoystickConnect((uint)index, name, (uint)buttonCount, (uint)axisCount, false);
                }
            }
            else if (e.type == SDL.SDL_EventType.SDL_JOYDEVICEREMOVED)
            {
                var index = e.jdevice.which;

                if (SDL.SDL_IsGameController(index) == SDL.SDL_bool.SDL_FALSE)
                {
                    OnJoystickDisconnect((uint)index);

                    var ptr = sdlJoysticks[index];
                    sdlJoysticks[index] = IntPtr.Zero;
                    SDL.SDL_JoystickClose(ptr);
                }
            }
            else if (e.type == SDL.SDL_EventType.SDL_JOYBUTTONDOWN)
            {
                var index = e.jbutton.which;
                if (SDL.SDL_IsGameController(index) == SDL.SDL_bool.SDL_FALSE)
                    OnJoystickButtonDown((uint)index, e.jbutton.button);
            }
            else if (e.type == SDL.SDL_EventType.SDL_JOYBUTTONUP)
            {
                var index = e.jbutton.which;
                if (SDL.SDL_IsGameController(index) == SDL.SDL_bool.SDL_FALSE)
                    OnJoystickButtonUp((uint)index, e.jbutton.button);
            }
            else if (e.type == SDL.SDL_EventType.SDL_JOYAXISMOTION)
            {
                var index = e.jaxis.which;
                if (SDL.SDL_IsGameController(index) == SDL.SDL_bool.SDL_FALSE)
                {
                    var value = Math.Max(-1f, Math.Min(1f, e.jaxis.axisValue / (float)short.MaxValue));
                    OnJoystickAxis((uint)index, e.jaxis.axis, value);
                }
            }
            // controller
            else if (e.type == SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED)
            {
                var index = e.cdevice.which;
                var ptr = sdlGamepads[index] = SDL.SDL_GameControllerOpen(index);
                var name = SDL.SDL_GameControllerName(ptr);
                OnJoystickConnect((uint)index, name, 15, 6, true);
            }
            else if (e.type == SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED)
            {
                var index = e.cdevice.which;
                OnJoystickDisconnect((uint)index);

                var ptr = sdlGamepads[index];
                sdlGamepads[index] = IntPtr.Zero;
                SDL.SDL_GameControllerClose(ptr);
            }
            else if (e.type == SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN)
            {
                var index = e.cbutton.which;
                var button = GamepadButtonToEnum(e.cbutton.button);

                OnGamepadButtonDown((uint)index, button);
            }
            else if (e.type == SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP)
            {
                var index = e.cbutton.which;
                var button = GamepadButtonToEnum(e.cbutton.button);

                OnGamepadButtonUp((uint)index, button);
            }
            else if (e.type == SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION)
            {
                var index = e.caxis.which;
                var axis = GamepadAxisToEnum(e.caxis.axis);
                var value = Math.Max(-1f, Math.Min(1f, e.caxis.axisValue / (float)short.MaxValue));

                OnGamepadAxis((uint)index, axis, value);
            }
            // keys
            else if (e.type == SDL.SDL_EventType.SDL_KEYDOWN || e.type == SDL.SDL_EventType.SDL_KEYUP)
            {
                if (e.key.repeat == 0)
                {
                    var keycode = e.key.keysym.sym;
                    if (!KeycodeToKeys.TryGetValue(keycode, out Keys key))
                        key = Keys.Unknown;

                    if (e.type == SDL.SDL_EventType.SDL_KEYDOWN)
                        OnKeyDown(key);
                    else
                        OnKeyUp(key);
                }
            }
            // text
            else if (e.type == SDL.SDL_EventType.SDL_TEXTINPUT)
            {
                unsafe
                {
                    int index = 0;
                    while (e.text.text[index] != 0)
                        OnText((char)(e.text.text[index++]));
                }
            }
        }

        private static Buttons GamepadButtonToEnum(byte button)
        {
            return button switch
            {
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A => Buttons.A,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B => Buttons.B,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X => Buttons.X,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y => Buttons.Y,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK => Buttons.Back,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE => Buttons.Select,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START => Buttons.Start,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK => Buttons.LeftStick,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK => Buttons.RightStick,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER => Buttons.LeftShoulder,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER => Buttons.RightShoulder,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP => Buttons.Up,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN => Buttons.Down,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT => Buttons.Left,
                (byte)SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT => Buttons.Right,
                _ => Buttons.None,
            };
        }

        private static Axes GamepadAxisToEnum(byte axes)
        {
            return axes switch
            {
                (byte)SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX => Axes.LeftX,
                (byte)SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY => Axes.LeftY,
                (byte)SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX => Axes.RightX,
                (byte)SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY => Axes.RightY,
                (byte)SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT => Axes.LeftTrigger,
                (byte)SDL.SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT => Axes.RightTrigger,
                _ => Axes.None,
            };
        }

        private static Dictionary<SDL.SDL_Keycode, Keys> KeycodeToKeys = new Dictionary<SDL.SDL_Keycode, Keys>()
        {
            { SDL.SDL_Keycode.SDLK_UNKNOWN, Keys.Unknown },

            { SDL.SDL_Keycode.SDLK_a, Keys.A },
            { SDL.SDL_Keycode.SDLK_b, Keys.B },
            { SDL.SDL_Keycode.SDLK_c, Keys.C },
            { SDL.SDL_Keycode.SDLK_d, Keys.D },
            { SDL.SDL_Keycode.SDLK_e, Keys.E },
            { SDL.SDL_Keycode.SDLK_f, Keys.F },
            { SDL.SDL_Keycode.SDLK_g, Keys.G },
            { SDL.SDL_Keycode.SDLK_h, Keys.H },
            { SDL.SDL_Keycode.SDLK_i, Keys.I },
            { SDL.SDL_Keycode.SDLK_j, Keys.J },
            { SDL.SDL_Keycode.SDLK_k, Keys.K },
            { SDL.SDL_Keycode.SDLK_l, Keys.L },
            { SDL.SDL_Keycode.SDLK_m, Keys.M },
            { SDL.SDL_Keycode.SDLK_n, Keys.N },
            { SDL.SDL_Keycode.SDLK_o, Keys.O },
            { SDL.SDL_Keycode.SDLK_p, Keys.P },
            { SDL.SDL_Keycode.SDLK_q, Keys.Q },
            { SDL.SDL_Keycode.SDLK_r, Keys.R },
            { SDL.SDL_Keycode.SDLK_s, Keys.S },
            { SDL.SDL_Keycode.SDLK_t, Keys.T },
            { SDL.SDL_Keycode.SDLK_u, Keys.U },
            { SDL.SDL_Keycode.SDLK_v, Keys.V },
            { SDL.SDL_Keycode.SDLK_w, Keys.W },
            { SDL.SDL_Keycode.SDLK_x, Keys.X },
            { SDL.SDL_Keycode.SDLK_y, Keys.Y },
            { SDL.SDL_Keycode.SDLK_z, Keys.Z },

            { SDL.SDL_Keycode.SDLK_1, Keys.D1 },
            { SDL.SDL_Keycode.SDLK_2, Keys.D2 },
            { SDL.SDL_Keycode.SDLK_3, Keys.D3 },
            { SDL.SDL_Keycode.SDLK_4, Keys.D4 },
            { SDL.SDL_Keycode.SDLK_5, Keys.D5 },
            { SDL.SDL_Keycode.SDLK_6, Keys.D6 },
            { SDL.SDL_Keycode.SDLK_7, Keys.D7 },
            { SDL.SDL_Keycode.SDLK_8, Keys.D8 },
            { SDL.SDL_Keycode.SDLK_9, Keys.D9 },
            { SDL.SDL_Keycode.SDLK_0, Keys.D0 },

            { SDL.SDL_Keycode.SDLK_RETURN, Keys.Enter },
            { SDL.SDL_Keycode.SDLK_ESCAPE, Keys.Escape },
            { SDL.SDL_Keycode.SDLK_BACKSPACE, Keys.Backspace },
            { SDL.SDL_Keycode.SDLK_TAB, Keys.Tab },
            { SDL.SDL_Keycode.SDLK_SPACE, Keys.Space },

            { SDL.SDL_Keycode.SDLK_MINUS, Keys.Minus },
            { SDL.SDL_Keycode.SDLK_EQUALS, Keys.Equal },
            { SDL.SDL_Keycode.SDLK_LEFTBRACKET, Keys.LeftBracket },
            { SDL.SDL_Keycode.SDLK_RIGHTBRACKET, Keys.RightBracket },
            { SDL.SDL_Keycode.SDLK_BACKSLASH, Keys.BackSlash },
            { SDL.SDL_Keycode.SDLK_SEMICOLON, Keys.Semicolon },
            { SDL.SDL_Keycode.SDLK_COMMA, Keys.Comma },
            { SDL.SDL_Keycode.SDLK_PERIOD, Keys.Period },
            { SDL.SDL_Keycode.SDLK_SLASH, Keys.Slash },
            { SDL.SDL_Keycode.SDLK_BACKQUOTE, Keys.BackQuote },

            { SDL.SDL_Keycode.SDLK_CAPSLOCK, Keys.CapsLock },

            { SDL.SDL_Keycode.SDLK_F1, Keys.F1 },
            { SDL.SDL_Keycode.SDLK_F2, Keys.F2 },
            { SDL.SDL_Keycode.SDLK_F3, Keys.F3 },
            { SDL.SDL_Keycode.SDLK_F4, Keys.F4 },
            { SDL.SDL_Keycode.SDLK_F5, Keys.F5 },
            { SDL.SDL_Keycode.SDLK_F6, Keys.F6 },
            { SDL.SDL_Keycode.SDLK_F7, Keys.F7 },
            { SDL.SDL_Keycode.SDLK_F8, Keys.F8 },
            { SDL.SDL_Keycode.SDLK_F9, Keys.F9 },
            { SDL.SDL_Keycode.SDLK_F10, Keys.F10 },
            { SDL.SDL_Keycode.SDLK_F11, Keys.F11 },
            { SDL.SDL_Keycode.SDLK_F12, Keys.F12 },

            { SDL.SDL_Keycode.SDLK_PRINTSCREEN, Keys.PrintScreen },
            { SDL.SDL_Keycode.SDLK_SCROLLLOCK, Keys.ScrollLock },
            { SDL.SDL_Keycode.SDLK_PAUSE, Keys.Pause },
            { SDL.SDL_Keycode.SDLK_INSERT, Keys.Insert },
            { SDL.SDL_Keycode.SDLK_HOME, Keys.Home },
            { SDL.SDL_Keycode.SDLK_PAGEUP, Keys.PageUp },
            { SDL.SDL_Keycode.SDLK_DELETE, Keys.Delete },
            { SDL.SDL_Keycode.SDLK_END, Keys.End },
            { SDL.SDL_Keycode.SDLK_PAGEDOWN, Keys.PageDown },
            { SDL.SDL_Keycode.SDLK_RIGHT, Keys.Right },
            { SDL.SDL_Keycode.SDLK_LEFT, Keys.Left },
            { SDL.SDL_Keycode.SDLK_DOWN, Keys.Down },
            { SDL.SDL_Keycode.SDLK_UP, Keys.Up },

            { SDL.SDL_Keycode.SDLK_KP_DIVIDE, Keys.KP_Divide },
            { SDL.SDL_Keycode.SDLK_KP_MULTIPLY, Keys.KP_Multiply },
            { SDL.SDL_Keycode.SDLK_KP_MINUS, Keys.KP_Subtract },
            { SDL.SDL_Keycode.SDLK_KP_PLUS, Keys.KP_Add },
            { SDL.SDL_Keycode.SDLK_KP_ENTER, Keys.KP_Enter },
            { SDL.SDL_Keycode.SDLK_KP_1, Keys.KP_1 },
            { SDL.SDL_Keycode.SDLK_KP_2, Keys.KP_2 },
            { SDL.SDL_Keycode.SDLK_KP_3, Keys.KP_3 },
            { SDL.SDL_Keycode.SDLK_KP_4, Keys.KP_4 },
            { SDL.SDL_Keycode.SDLK_KP_5, Keys.KP_5 },
            { SDL.SDL_Keycode.SDLK_KP_6, Keys.KP_6 },
            { SDL.SDL_Keycode.SDLK_KP_7, Keys.KP_7 },
            { SDL.SDL_Keycode.SDLK_KP_8, Keys.KP_8 },
            { SDL.SDL_Keycode.SDLK_KP_9, Keys.KP_9 },
            { SDL.SDL_Keycode.SDLK_KP_0, Keys.KP_0 },
            { SDL.SDL_Keycode.SDLK_KP_PERIOD, Keys.KP_Decimal },

            { SDL.SDL_Keycode.SDLK_KP_EQUALS, Keys.KP_Equal },

            { SDL.SDL_Keycode.SDLK_F13, Keys.F13 },
            { SDL.SDL_Keycode.SDLK_F14, Keys.F14 },
            { SDL.SDL_Keycode.SDLK_F15, Keys.F15 },
            { SDL.SDL_Keycode.SDLK_F16, Keys.F16 },
            { SDL.SDL_Keycode.SDLK_F17, Keys.F17 },
            { SDL.SDL_Keycode.SDLK_F18, Keys.F18 },
            { SDL.SDL_Keycode.SDLK_F19, Keys.F19 },
            { SDL.SDL_Keycode.SDLK_F20, Keys.F20 },
            { SDL.SDL_Keycode.SDLK_F21, Keys.F21 },
            { SDL.SDL_Keycode.SDLK_F22, Keys.F22 },
            { SDL.SDL_Keycode.SDLK_F23, Keys.F23 },
            { SDL.SDL_Keycode.SDLK_F24, Keys.F24 },

            { SDL.SDL_Keycode.SDLK_MENU, Keys.Menu },
            { SDL.SDL_Keycode.SDLK_KP_COMMA, Keys.Comma },

            { SDL.SDL_Keycode.SDLK_LCTRL, Keys.LeftControl },
            { SDL.SDL_Keycode.SDLK_LSHIFT, Keys.LeftShift },
            { SDL.SDL_Keycode.SDLK_LALT, Keys.LeftAlt },
            { SDL.SDL_Keycode.SDLK_RCTRL, Keys.RightControl },
            { SDL.SDL_Keycode.SDLK_RSHIFT, Keys.RightShift },

            { SDL.SDL_Keycode.SDLK_LGUI, Keys.LeftSuper },
            { SDL.SDL_Keycode.SDLK_RGUI, Keys.RightSuper },
        };
    }
}
