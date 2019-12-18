using System;

namespace lib
{
    public class ControllerEventArgs : EventArgs
    {
        public ControllerEventArgs(string stateData)
        {
            StateData = stateData;
        }
        public string StateData { get; set;}
    }
}