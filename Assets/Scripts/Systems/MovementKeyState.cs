using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    /// <summary>
    /// Movement axes while crouch is held. Left Control is the duck button, and it is also
    /// a keyboard modifier. Ctrl+A, Ctrl+S, and Ctrl+D are editor shortcuts, so the Input
    /// Manager never sees them. W is not one of those shortcuts, so crouching first could
    /// only start forward movement. A key that was already down kept its axis value, so
    /// crouching mid-stride continued in that direction and would not turn. The physical
    /// key state is read while ducking so every direction can start and change.
    /// </summary>
    public static class MovementKeyState
    {
        /// <summary>
        /// Replaces a missing or stale axis with the crouch movement keys.
        /// When neither or both keys are held, the original axis is kept so a gamepad still works.
        /// </summary>
        public static float ApplyKeys(float axis, bool negative, bool positive)
        {
            if (negative == positive)
            {
                return axis;
            }

            return negative ? -1f : 1f;
        }

        /// <summary>
        /// True when the key is down. Falls back to the physical keyboard while crouching,
        /// because the Input Manager drops modifier combinations.
        /// </summary>
        public static bool Held(KeyCode key)
        {
            if (Input.GetKey(key))
            {
                return true;
            }

            if (!AllowPhysicalKeys())
            {
                return false;
            }

            return NativeKeyDown(key);
        }

        static bool AllowPhysicalKeys()
        {
            if (Application.isFocused)
            {
                return true;
            }

#if UNITY_EDITOR
            // Play mode often reports unfocused while the Game view still has the keyboard,
            // which is exactly when the editor eats Ctrl+A, Ctrl+S, and Ctrl+D.
            return Application.isPlaying;
#else
            return false;
#endif
        }

        static bool nativeFailed;

        static bool NativeKeyDown(KeyCode key)
        {
            if (nativeFailed)
            {
                return false;
            }

            try
            {
                return PlatformKeyDown(key);
            }
            catch (Exception)
            {
                nativeFailed = true;
                return false;
            }
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int virtualKey);

        static bool PlatformKeyDown(KeyCode key)
        {
            var virtualKey = VirtualKey(key);
            if (virtualKey == 0)
            {
                return false;
            }

            return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
        }

        static int VirtualKey(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.A: return 0x41;
                case KeyCode.D: return 0x44;
                case KeyCode.S: return 0x53;
                case KeyCode.W: return 0x57;
                case KeyCode.LeftArrow: return 0x25;
                case KeyCode.UpArrow: return 0x26;
                case KeyCode.RightArrow: return 0x27;
                case KeyCode.DownArrow: return 0x28;
                default: return 0;
            }
        }
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
        static extern byte CGEventSourceKeyState(int stateId, int keyCode);

        static bool PlatformKeyDown(KeyCode key)
        {
            var macKey = MacKey(key);
            if (macKey < 0)
            {
                return false;
            }

            return CGEventSourceKeyState(1, macKey) != 0;
        }

        static int MacKey(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.A: return 0x00;
                case KeyCode.S: return 0x01;
                case KeyCode.D: return 0x02;
                case KeyCode.W: return 0x0D;
                case KeyCode.LeftArrow: return 0x7B;
                case KeyCode.RightArrow: return 0x7C;
                case KeyCode.DownArrow: return 0x7D;
                case KeyCode.UpArrow: return 0x7E;
                default: return -1;
            }
        }
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        [DllImport("libX11")]
        static extern IntPtr XOpenDisplay(IntPtr display);

        [DllImport("libX11")]
        static extern int XKeysymToKeycode(IntPtr display, uint keysym);

        [DllImport("libX11")]
        static extern int XQueryKeymap(IntPtr display, byte[] keys);

        static IntPtr display;
        static bool displayOpened;

        static bool PlatformKeyDown(KeyCode key)
        {
            var keysym = Keysym(key);
            if (keysym == 0)
            {
                return false;
            }

            if (!displayOpened)
            {
                display = XOpenDisplay(IntPtr.Zero);
                displayOpened = true;
            }

            if (display == IntPtr.Zero)
            {
                return false;
            }

            var keycode = XKeysymToKeycode(display, keysym);
            if (keycode <= 0 || keycode >= 256)
            {
                return false;
            }

            var keys = new byte[32];
            if (XQueryKeymap(display, keys) == 0)
            {
                return false;
            }

            return (keys[keycode / 8] & (1 << (keycode % 8))) != 0;
        }

        static uint Keysym(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.A: return 0x0061;
                case KeyCode.D: return 0x0064;
                case KeyCode.S: return 0x0073;
                case KeyCode.W: return 0x0077;
                case KeyCode.LeftArrow: return 0xff51;
                case KeyCode.UpArrow: return 0xff52;
                case KeyCode.RightArrow: return 0xff53;
                case KeyCode.DownArrow: return 0xff54;
                default: return 0;
            }
        }
#else
        static bool PlatformKeyDown(KeyCode key)
        {
            return false;
        }
#endif
    }
}
