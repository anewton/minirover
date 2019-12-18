using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using lib;
using static lib.ControllerManager;

namespace gameController
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var controllerMgr = new ControllerManager();
            controllerMgr.ControllerChanged += ControllerChanged;
            Console.ReadLine();
        }

        public static void ControllerChanged(object sender, ControllerEventArgs e)
        {
            Debug.WriteLine(e.StateData);
        }
    }
}
