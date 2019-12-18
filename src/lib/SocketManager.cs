using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Threading.Tasks;

// Adapted from: https://code.msdn.microsoft.com/Communication-through-91a2582b
// NOTE: This resource is no longer available or has been moved and retired.  
//      Here is a link regarding the retirement of this content: https://docs.microsoft.com/en-us/teamblog/announcing-msdn-code-gallery-retirement
//      Here is the archived code sample that originally appeared in the first link: https://github.com/microsoftarchive/msdn-code-gallery-community-a-c/tree/master/Communication%20through%20Sockets
//      Here is the Wayback Machine link to the original post as of 12/10/2012: https://web.archive.org/web/20121210082347/https://code.msdn.microsoft.com/Communication-through-91a2582b )
// Original post author: Houssem Dellai

namespace lib
{
    public class SocketManager
    {
        private Socket _senderSock;
        private IPAddress _computerIPAddress = null;

        public SocketManager()
        {
            _computerIPAddress = GetClientComputerIpv4Address();
        }

        private IPAddress GetClientComputerIpv4Address()
        {
            var clientIPv4Address = GetLocalIPAddress();
            return clientIPv4Address;
        }

        private IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return null;
        }

        private IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length != 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip = null;
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
            int port;
            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }

        public async Task ConnectToSocketServerAsync(string RPiIPAddress, int portNumber)
        {
            IPAddress clientIPAddress = null;
            if (!IPAddress.TryParse(_computerIPAddress.ToString(), out clientIPAddress))
            {
                throw new FormatException("Invalid Computer IP Address");
            }
            IPAddress serverIPAddress = null;
            if (!IPAddress.TryParse(RPiIPAddress, out serverIPAddress))
            {
                throw new FormatException("Invalid RPi IP Address");
            }

            try
            {
                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,    // Connection permission 
                    TransportType.Tcp,        // Defines transport types 
                    _computerIPAddress.ToString(),                       // Gets the IP addresses 
                    SocketPermission.AllPorts // All ports 
                    );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance            
                IPHostEntry ipHost = Dns.GetHostEntry(clientIPAddress.ToString());

                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = ipHost.AddressList.Where(ip => ip.IsIPv4MappedToIPv6 == false &&
                    ip.IsIPv6LinkLocal == false && ip.IsIPv6Multicast == false && ip.IsIPv6SiteLocal == false &&
                    ip.IsIPv6Teredo == false).FirstOrDefault(); //get the IPv4 address

                //// Creates a network endpoint 
                //IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 51717);

                IPEndPoint ipRemoteEndPoint = CreateIPEndPoint(string.Format("{0}:{1}", serverIPAddress.ToString(), portNumber));

                // Create one Socket object to setup Tcp connection 
                if (_senderSock == null)
                {
                    _senderSock = new Socket(
                        ipAddr.AddressFamily,// Specifies the addressing scheme 
                        SocketType.Stream,   // The type of socket  
                        ProtocolType.Tcp     // Specifies the protocols  
                        )
                    {
                        NoDelay = false   // Using the Nagle algorithm 
                    };
                }

                // Establishes a connection to a remote host 
                await _senderSock.ConnectAsync(ipRemoteEndPoint);

                Debug.WriteLine("Socket connected to " + _senderSock.RemoteEndPoint.ToString());
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
            }
        }

        public void DisconnectFromSocketServer()
        {
            try
            {
                // Disables sends and receives on a Socket. 
                _senderSock.Shutdown(SocketShutdown.Both);
                _senderSock.Disconnect(true);

                //Closes the Socket connection and releases all resources 
                _senderSock.Close();
                _senderSock = null;

            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
        }

        public void DestroySocketConnection()
        {
            try
            {
                if (_senderSock != null)
                {
                    _senderSock.Close();
                    _senderSock.Dispose();
                    _senderSock = null;
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
            }
        }

        public async Task SendTextDataAsync(string textData, string RPiIPAddress, int portNumber, bool canConnect = false)
        {
            try
            {
                if (canConnect)
                {
                    if (_senderSock == null || (_senderSock.Available != 0 && !_senderSock.Connected))
                    {
                        await ConnectToSocketServerAsync(RPiIPAddress, portNumber);
                    }
                    if (_senderSock != null && _senderSock.Connected)
                    {
                        // Prepare the reply message  
                        byte[] byteData = Encoding.Unicode.GetBytes(textData);
                        SocketFlags socketFlags = SocketFlags.None;
                        _senderSock.Send(Encoding.UTF8.GetBytes(textData), 0, textData.Length, socketFlags);
                    }
                    //DisconnectFromSocketServer();
                }
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                if (socketEx.ErrorCode == 10053)
                {
                    DestroySocketConnection();
                }
                Debug.WriteLine(socketEx.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void ConnectToSocket_CodeB(string RPiIPAddress, int portNumber)
        {
            try
            {

                IPAddress clientIPAddress = null;
                if (!IPAddress.TryParse(_computerIPAddress.ToString(), out clientIPAddress))
                {
                    throw new FormatException("Invalid Computer IP Address");
                }
                IPAddress serverIPAddress = null;
                if (!IPAddress.TryParse(RPiIPAddress, out serverIPAddress))
                {
                    throw new FormatException("Invalid RPi IP Address");
                }

                // Create one SocketPermission for socket access restrictions  
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,    // Connection permission 
                    TransportType.Tcp,        // Defines transport types 
                    _computerIPAddress.ToString(),                       // Gets the IP addresses 
                    SocketPermission.AllPorts // All ports 
                    );

                // Ensures the code to have permission to access a Socket  
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance             
                IPHostEntry ipHost = Dns.GetHostEntry(clientIPAddress.ToString());

                // Gets first IP address associated with a localhost  
                //IPAddress ipAddr = ipHost.AddressList[0];

                IPAddress ipAddr = ipHost.AddressList.Where(ip => ip.IsIPv4MappedToIPv6 == false &&
                    ip.IsIPv6LinkLocal == false && ip.IsIPv6Multicast == false && ip.IsIPv6SiteLocal == false &&
                    ip.IsIPv6Teredo == false).FirstOrDefault(); //get the IPv4 address

                // Creates a network endpoint  
                //IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 51717);

                IPEndPoint ipRemoteEndPoint = CreateIPEndPoint(string.Format("{0}:{1}", serverIPAddress.ToString(), portNumber));

                // Create one Socket object to setup Tcp connection 
                if (_senderSock == null)
                {
                    _senderSock = new Socket(
                        ipAddr.AddressFamily,// Specifies the addressing scheme 
                        SocketType.Stream,   // The type of socket  
                        ProtocolType.Tcp     // Specifies the protocols  
                        )
                    {
                        NoDelay = false   // Using the Nagle algorithm 
                    };
                }

                // Establishes a connection to a remote host 
                _senderSock.Connect(ipRemoteEndPoint);

                Debug.WriteLine("Socket connected to " + _senderSock.RemoteEndPoint.ToString());
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
            }
        }

        public void SendText_CodeB(string textData)
        {
            try
            {
                // Sending message  
                //<Client Quit> is the sign for end of data  
                // string theMessageToSend = text;
                // byte[] msg = Encoding.Unicode.GetBytes(theMessageToSend);
                // // Sends data to a connected Socket.  
                // int bytesSend = _senderSock.Send(msg);

                byte[] byteData = Encoding.Unicode.GetBytes(textData);
                SocketFlags socketFlags = SocketFlags.None;
                _senderSock.Send(Encoding.UTF8.GetBytes(textData), 0, textData.Length, socketFlags);
                _senderSock.Shutdown(SocketShutdown.Both);
                _senderSock.Close();
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
            }
        }

    }
}