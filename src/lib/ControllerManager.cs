using SharpDX.XInput;
using System.Diagnostics;
using System.Timers;

namespace lib
{
    public class ControllerManager
    {
        private Timer _controllerStatusCheckTimer;
        private Controller _controller;
        private bool _isControllerConnected = false;
        private bool _connectionTrueReported = false;
        private ControllerState _controllerState = null;

        public ControllerManager()
        {
            _controllerStatusCheckTimer = new Timer { Interval = 50 };
            _controllerStatusCheckTimer.Elapsed += ControllerCheckTimer_Elapsed;
            _controllerStatusCheckTimer.Start();
        }

        public delegate void ControllerStateChangedHandler(object sender, ControllerEventArgs e);
        public event ControllerStateChangedHandler ControllerChanged;

        private void ControllerCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_controller == null)
            {
                _controller = new Controller(UserIndex.One);
            }
            CheckControllerConnectionState();
            UpdateControllerState();
        }

        private void CheckControllerConnectionState()
        {
            _isControllerConnected = _controller != null && _controller.IsConnected;
            // slow down status checking interval when connected
            if (_isControllerConnected && _controllerStatusCheckTimer.Interval != 10)
            {
                // rest status checking interval
                _controllerStatusCheckTimer.Interval = 10;
                Debug.WriteLine($"Xbox controller connected: {_isControllerConnected}");
                _connectionTrueReported = true;
            }
            // speed up status checking interval when not connected
            else if (!_isControllerConnected)
            {
                _controllerStatusCheckTimer.Interval = 5000;
                _connectionTrueReported = false;
            }
            if (_connectionTrueReported == false)
            {
                Debug.WriteLine($"Xbox controller connected: {_isControllerConnected}");
            }
        }

        private void UpdateControllerState()
        {
            if (_isControllerConnected && _controller != null)
            {
                if (_controllerState == null)
                {
                    _controllerState = new ControllerState(_controller);
                    _controllerState.ControllerStateChanged += ControllerStateChanged;
                }
                else
                {
                    _controllerState.UpdateControllerState(_controller);
                }
            }
        }

        private void ControllerStateChanged(object sender, ControllerEventArgs e)
        {
            if(this.ControllerChanged != null)
            {
                ControllerChanged(sender, e);
            }
        }

    }
}