using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Varginha
{
    /// <summary>Comandos remapeáveis usados pelo protótipo.</summary>
    public enum VarginhaInputAction
    {
        MoveUp,
        MoveDown,
        MoveLeft,
        MoveRight,
        Run,
        Interact,
        Attack,
        AllyCommand,
        Dodge
    }

    /// <summary>
    /// Mapa pequeno e persistente para o menu de controles. Cada comando aceita uma
    /// tecla ou um botão do mouse; a alteração vale imediatamente em todas as cenas.
    /// </summary>
    public static class VarginhaInputBindings
    {
        private const string Prefix = "Varginha.Controls.";
        private const int NoMouse = -1;
        private static readonly Binding[] Defaults =
        {
            new(Key.W, NoMouse),
            new(Key.S, NoMouse),
            new(Key.A, NoMouse),
            new(Key.D, NoMouse),
            new(Key.LeftShift, NoMouse),
            new(Key.E, NoMouse),
            new(Key.J, 0),
            new(Key.L, 1),
            new(Key.LeftCtrl, NoMouse)
        };

        private static Binding[] _bindings;

        private struct Binding
        {
            public Key Keyboard;
            public int MouseButton;

            public Binding(Key keyboard, int mouseButton)
            {
                Keyboard = keyboard;
                MouseButton = mouseButton;
            }
        }

        private static void EnsureLoaded()
        {
            if (_bindings != null) return;
            _bindings = new Binding[Defaults.Length];
            for (int i = 0; i < _bindings.Length; i++)
            {
                var action = (VarginhaInputAction)i;
                _bindings[i] = new Binding(
                    (Key)PlayerPrefs.GetInt(KeyPref(action), (int)Defaults[i].Keyboard),
                    PlayerPrefs.GetInt(MousePref(action), Defaults[i].MouseButton));
            }
        }

        public static Key GetKeyboard(VarginhaInputAction action)
        {
            EnsureLoaded();
            return _bindings[(int)action].Keyboard;
        }

        public static int GetMouseButton(VarginhaInputAction action)
        {
            EnsureLoaded();
            return _bindings[(int)action].MouseButton;
        }

        public static bool IsPressed(VarginhaInputAction action)
        {
            EnsureLoaded();
            var keyboard = Keyboard.current;
            if (keyboard != null && GetKeyboard(action) != Key.None && keyboard[GetKeyboard(action)].isPressed)
                return true;
            if (keyboard != null && IsSecondaryPressed(keyboard, action)) return true;
            return IsMousePressed(GetMouseButton(action));
        }

        public static bool WasPressedThisFrame(VarginhaInputAction action)
        {
            EnsureLoaded();
            var keyboard = Keyboard.current;
            if (keyboard != null && GetKeyboard(action) != Key.None && keyboard[GetKeyboard(action)].wasPressedThisFrame)
                return true;
            if (keyboard != null && WasSecondaryPressed(keyboard, action)) return true;
            return WasMousePressedThisFrame(GetMouseButton(action));
        }

        public static void SetKeyboard(VarginhaInputAction action, Key key)
        {
            EnsureLoaded();
            int index = (int)action;
            _bindings[index].Keyboard = key;
            _bindings[index].MouseButton = NoMouse;
            Save(action);
        }

        public static void SetMouseButton(VarginhaInputAction action, int button)
        {
            EnsureLoaded();
            int index = (int)action;
            _bindings[index].Keyboard = Key.None;
            _bindings[index].MouseButton = Mathf.Clamp(button, 0, 2);
            Save(action);
        }

        public static void Reset(VarginhaInputAction action)
        {
            EnsureLoaded();
            int index = (int)action;
            _bindings[index] = Defaults[index];
            Save(action);
        }

        public static string DisplayName(VarginhaInputAction action)
        {
            Key key = GetKeyboard(action);
            int mouse = GetMouseButton(action);
            if (mouse >= 0 && key != Key.None) return MouseName(mouse) + " + " + KeyName(key);
            if (mouse >= 0) return MouseName(mouse);
            if (key == Key.None) return "NÃO DEFINIDO";
            return KeyName(key);
        }

        public static string ActionName(VarginhaInputAction action)
        {
            switch (action)
            {
                case VarginhaInputAction.MoveUp: return "Mover para cima";
                case VarginhaInputAction.MoveDown: return "Mover para baixo";
                case VarginhaInputAction.MoveLeft: return "Mover para esquerda";
                case VarginhaInputAction.MoveRight: return "Mover para direita";
                case VarginhaInputAction.Run: return "Correr";
                case VarginhaInputAction.Interact: return "Interagir / examinar";
                case VarginhaInputAction.Attack: return "Atacar / combo";
                case VarginhaInputAction.AllyCommand: return "Comandar aluno";
                case VarginhaInputAction.Dodge: return "Esquivar";
                default: return action.ToString();
            }
        }

        public static bool TrySetFromKeyCode(VarginhaInputAction action, KeyCode keyCode)
        {
            if (keyCode == KeyCode.Mouse0 || keyCode == KeyCode.Mouse1 || keyCode == KeyCode.Mouse2)
            {
                SetMouseButton(action, keyCode - KeyCode.Mouse0);
                return true;
            }

            Key key = ToInputSystemKey(keyCode);
            if (key == Key.None) return false;
            SetKeyboard(action, key);
            return true;
        }

        public static Key ToInputSystemKey(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.A: return Key.A;
                case KeyCode.B: return Key.B;
                case KeyCode.C: return Key.C;
                case KeyCode.D: return Key.D;
                case KeyCode.E: return Key.E;
                case KeyCode.F: return Key.F;
                case KeyCode.G: return Key.G;
                case KeyCode.H: return Key.H;
                case KeyCode.I: return Key.I;
                case KeyCode.J: return Key.J;
                case KeyCode.K: return Key.K;
                case KeyCode.L: return Key.L;
                case KeyCode.M: return Key.M;
                case KeyCode.N: return Key.N;
                case KeyCode.O: return Key.O;
                case KeyCode.P: return Key.P;
                case KeyCode.Q: return Key.Q;
                case KeyCode.R: return Key.R;
                case KeyCode.S: return Key.S;
                case KeyCode.T: return Key.T;
                case KeyCode.U: return Key.U;
                case KeyCode.V: return Key.V;
                case KeyCode.W: return Key.W;
                case KeyCode.X: return Key.X;
                case KeyCode.Y: return Key.Y;
                case KeyCode.Z: return Key.Z;
                case KeyCode.UpArrow: return Key.UpArrow;
                case KeyCode.DownArrow: return Key.DownArrow;
                case KeyCode.LeftArrow: return Key.LeftArrow;
                case KeyCode.RightArrow: return Key.RightArrow;
                case KeyCode.Space: return Key.Space;
                case KeyCode.Return: return Key.Enter;
                case KeyCode.KeypadEnter: return Key.NumpadEnter;
                case KeyCode.LeftShift: return Key.LeftShift;
                case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftControl: return Key.LeftCtrl;
                case KeyCode.RightControl: return Key.RightCtrl;
                case KeyCode.LeftAlt: return Key.LeftAlt;
                case KeyCode.RightAlt: return Key.RightAlt;
                case KeyCode.Tab: return Key.Tab;
                case KeyCode.Backspace: return Key.Backspace;
                case KeyCode.Escape: return Key.Escape;
                case KeyCode.Alpha0: return Key.Digit0;
                case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2;
                case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4;
                case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6;
                case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8;
                case KeyCode.Alpha9: return Key.Digit9;
                default: return Key.None;
            }
        }

        public static string KeyName(Key key)
        {
            string value = key.ToString();
            if (value.StartsWith("Digit", StringComparison.Ordinal)) return value.Substring(5);
            if (value == "LeftCtrl") return "CTRL ESQ.";
            if (value == "RightCtrl") return "CTRL DIR.";
            if (value == "LeftShift") return "SHIFT ESQ.";
            if (value == "RightShift") return "SHIFT DIR.";
            if (value == "NumpadEnter") return "ENTER NUM.";
            return value.ToUpperInvariant();
        }

        public static string MouseName(int button)
        {
            switch (button)
            {
                case 0: return "MOUSE ESQUERDO";
                case 1: return "MOUSE DIREITO";
                case 2: return "MOUSE MEIO";
                default: return "NÃO DEFINIDO";
            }
        }

        private static bool IsMousePressed(int button)
        {
            var mouse = Mouse.current;
            if (mouse == null) return false;
            return button == 0 ? mouse.leftButton.isPressed : button == 1 ? mouse.rightButton.isPressed
                : button == 2 && mouse.middleButton.isPressed;
        }

        private static bool IsSecondaryPressed(Keyboard keyboard, VarginhaInputAction action)
        {
            switch (action)
            {
                case VarginhaInputAction.MoveUp: return keyboard.upArrowKey.isPressed;
                case VarginhaInputAction.MoveDown: return keyboard.downArrowKey.isPressed;
                case VarginhaInputAction.MoveLeft: return keyboard.leftArrowKey.isPressed;
                case VarginhaInputAction.MoveRight: return keyboard.rightArrowKey.isPressed;
                case VarginhaInputAction.Run: return keyboard.rightShiftKey.isPressed;
                case VarginhaInputAction.Interact: return keyboard.spaceKey.isPressed || keyboard.enterKey.isPressed;
                case VarginhaInputAction.Dodge: return keyboard.rightCtrlKey.isPressed;
                default: return false;
            }
        }

        private static bool WasSecondaryPressed(Keyboard keyboard, VarginhaInputAction action)
        {
            switch (action)
            {
                case VarginhaInputAction.Interact: return keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame;
                case VarginhaInputAction.Dodge: return keyboard.rightCtrlKey.wasPressedThisFrame;
                default: return false;
            }
        }

        private static bool WasMousePressedThisFrame(int button)
        {
            var mouse = Mouse.current;
            if (mouse == null) return false;
            return button == 0 ? mouse.leftButton.wasPressedThisFrame : button == 1 ? mouse.rightButton.wasPressedThisFrame
                : button == 2 && mouse.middleButton.wasPressedThisFrame;
        }

        private static string KeyPref(VarginhaInputAction action) => Prefix + action + ".Key";
        private static string MousePref(VarginhaInputAction action) => Prefix + action + ".Mouse";

        private static void Save(VarginhaInputAction action)
        {
            int index = (int)action;
            PlayerPrefs.SetInt(KeyPref(action), (int)_bindings[index].Keyboard);
            PlayerPrefs.SetInt(MousePref(action), _bindings[index].MouseButton);
            PlayerPrefs.Save();
        }
    }
}
