using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using SharpDX.XInput;

namespace lib
{
    public class ControllerState
    {
        // Threshold for determining exact center joysticks due to innaccuracies in controller data
        private const int DEADBAND = 2500;

        public ControllerState(Controller controller)
        {
            UpdateControllerState(controller);
        }

        public delegate void ControllerStateChangedHandler(object sender, ControllerEventArgs e);
        public event ControllerStateChangedHandler ControllerStateChanged;

        public void UpdateControllerState(Controller controller)
        {
            try
            {
                var state = controller.GetState();

                LeftStickX = (Math.Abs((float)state.Gamepad.LeftThumbX) < DEADBAND) ? 0 : (float)state.Gamepad.LeftThumbX / short.MinValue * -100;
                LeftStickY = (Math.Abs((float)state.Gamepad.LeftThumbY) < DEADBAND) ? 0 : (float)state.Gamepad.LeftThumbY / short.MaxValue * 100;
                RightStickX = (Math.Abs((float)state.Gamepad.RightThumbX) < DEADBAND) ? 0 : (float)state.Gamepad.RightThumbX / short.MaxValue * 100;
                RightStickY = (Math.Abs((float)state.Gamepad.RightThumbY) < DEADBAND) ? 0 : (float)state.Gamepad.RightThumbY / short.MaxValue * 100;

                LeftAxis = string.Format("X: {0} Y: {1}", LeftStickX, LeftStickY);
                RightAxis = string.Format("X: {0} Y: {1}", RightStickX, RightStickY);

                Y = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Y);
                B = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B);
                A = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A);
                X = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.X);
                DPadUp = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp);
                DPadRight = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight);
                DPadDown = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadDown);
                DPadLeft = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft);
                LeftShoulder = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftShoulder);
                RightShoulder = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightShoulder);
                LeftThumb = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.LeftThumb);
                RightThumb = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.RightThumb);
                Start = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Start);
                Back = state.Gamepad.Buttons.HasFlag(GamepadButtonFlags.Back);
                LeftTrigger = state.Gamepad.LeftTrigger == 255;
                RightTrigger = state.Gamepad.RightTrigger == 255;

                var statusString = this.ToString();
                if (_lastStatusString != statusString)
                {
                    _lastStatusString = statusString;
                    Debug.WriteLine(this.ToString());

                    if (this.ControllerStateChanged != null)
                    {
                        var stateData = $"{LeftStickX}|{LeftStickY}|{RightStickX}|{RightStickY}";
                        ControllerStateChanged(this, new ControllerEventArgs(stateData));
                    }
                }

            }
            catch (SharpDX.SharpDXException inputEx)
            {
                Debug.WriteLine(inputEx.Message);
                throw;
            }
        }

        private PropertyInfo[] _PropertyInfos = null;
        private string _lastStatusString = string.Empty;

        public override string ToString()
        {
            if (_PropertyInfos == null)
            {
                _PropertyInfos = this.GetType().GetProperties();
            }
            var sb = new StringBuilder();
            foreach (var info in _PropertyInfos)
            {
                var value = info.GetValue(this, null) ?? "(null)";
                sb.AppendLine(info.Name + ": " + value.ToString());
            }
            return sb.ToString();
        }

        public double LeftStickX { get; set; }

        public double LeftStickY { get; set; }

        public double RightStickX { get; set; }

        public double RightStickY { get; set; }

        public bool Y { get; set; }

        public bool B { get; set; }

        public bool A { get; set; }

        public bool X { get; set; }

        public bool DPadUp { get; set; }

        public bool DPadRight { get; set; }

        public bool DPadDown { get; set; }

        public bool DPadLeft { get; set; }

        public bool LeftShoulder { get; set; }

        public bool RightShoulder { get; set; }

        public bool LeftThumb { get; set; }

        public bool RightThumb { get; set; }

        public bool Start { get; set; }

        public bool Back { get; set; }

        public string LeftAxis { get; set; }

        public bool LeftTrigger { get; set; }

        public string RightAxis { get; set; }

        public bool RightTrigger { get; set; }
    }
}
