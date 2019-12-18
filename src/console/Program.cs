using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using lib;
using static lib.ControllerManager;

namespace coreXboxController
{
    class Program
    {
        private static string _rasPiHost = "192.168.10.105";
        private static int _socketPort = 51717;
        private static SocketManager _socketManager = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var rasPiManager = new RasPiManager("pi", "raspberry", IPAddress.Parse(_rasPiHost));
            rasPiManager.SocketServerInitialized += new EventHandler(async (s, e) => await SocketServerInitialized(s, e));
            rasPiManager.InitSocketServer();

            var controllerMgr = new ControllerManager();
           controllerMgr.ControllerChanged += new ControllerStateChangedHandler(async (s, e) => await ControllerChangedAsync(s, e));
            Console.ReadLine();
           _socketManager.DestroySocketConnection();
            rasPiManager.DisableSocketServer();
        }

        public static async Task SocketServerInitialized(object sender, EventArgs e)
        {
            Console.WriteLine("Socket Server Initialized");
             _socketManager = new SocketManager();
            await _socketManager.SendTextDataAsync("Client connection established", _rasPiHost, _socketPort, true);
        }

        public static async Task ControllerChangedAsync(object sender, ControllerEventArgs e)
        {
            Debug.WriteLine(e.StateData);
            await _socketManager.SendTextDataAsync(e.StateData, _rasPiHost, _socketPort, true);
        }

    }
}