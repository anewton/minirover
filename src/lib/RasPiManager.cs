using Renci.SshNet;
using System.Net;
using System;

namespace lib
{
    public class RasPiManager
    {
        private string _user;
        private string _password;
        private IPAddress _rasPiAddress;
        public EventHandler SocketServerInitialized;

        public RasPiManager(string user, string password, IPAddress rasPiAddress)
        {
            _user = user;
            _password = password;
            _rasPiAddress = rasPiAddress;
        }

        public void InitSocketServer()
        {
            SshClient sshClient = null;
            try
            {
                var authenticationMethod = new PasswordAuthenticationMethod(_user, _password);
                var connectionInfo = new ConnectionInfo(_rasPiAddress.ToString(), 22, _user, authenticationMethod);
                using (sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    var getPIDListCommand = "ps -ef | awk '$NF~\"socketServer.py\" {print $2}'";
                    var runGetPIDListCommand = sshClient.CreateCommand(getPIDListCommand);
                    var PIDList = runGetPIDListCommand.Execute();
                    if (!string.IsNullOrEmpty(PIDList))
                    {
                        var PIDs = PIDList.TrimEnd('\n').Split('\n');
                        foreach (var PID in PIDs)
                        {
                            var killcommand = string.Format("sudo kill {0}", PID);
                            var runKillcommand = sshClient.CreateCommand(killcommand);
                            var killResult = runKillcommand.Execute();
                            break;
                        }
                    }
                    var sshCommand = sshClient.CreateCommand("python3 socketServer.py");
                    var asyncCommandResult = sshCommand.BeginExecute();
                    if(SocketServerInitialized != null)
                        SocketServerInitialized(this, null);
                }
            }
            catch { }
        }

        public void DisableSocketServer()
        {
            SshClient sshClient = null;
            try
            {
                var authenticationMethod = new PasswordAuthenticationMethod(_user, _password);
                var connectionInfo = new ConnectionInfo(_rasPiAddress.ToString(), 22, _user, authenticationMethod);
                using (sshClient = new SshClient(connectionInfo))
                {
                    sshClient.Connect();
                    var getPIDListCommand = "ps -ef | awk '$NF~\"socketServer.py\" {print $2}'";
                    var runGetPIDListCommand = sshClient.CreateCommand(getPIDListCommand);
                    var PIDList = runGetPIDListCommand.Execute();
                    if (!string.IsNullOrEmpty(PIDList))
                    {
                        var PIDs = PIDList.TrimEnd('\n').Split('\n');
                        foreach (var PID in PIDs)
                        {
                            var killcommand = string.Format("sudo kill {0}", PID);
                            var runKillcommand = sshClient.CreateCommand(killcommand);
                            var killResult = runKillcommand.Execute();
                            break;
                        }
                    }
                }
            }
            catch { }
        }

    }

}