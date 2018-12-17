using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace FTP_Client
{
    static class Gateway
    {

        enum ExpectedReply {
            PositiveIncomplete = 1  , // Command Accepted, await further input from server
            PositiveComplete = 2,   //Command accepted, enter another command
            PositiveAwatingFurtherInput = 3, //Command accepted but we need more info from you ex: username and password
            Negative = 5, // You entered something wrong
            NegativeTryAgain = 4  //Something wrong in the server, try again
        };
        static bool isPassive = false;
        static int PassiveAddress, PassivePort; // recieve from server
        static Socket Control = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // socket for connection  
        static Socket Data = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // socket for recieving data
        static readonly byte[] buffer = new byte[1024]; //buffer for data from the server 
        static string SendCommand(string Command, ExpectedReply expectedReply) // sends the commands to the server 
        {
            Control.Send(Encoding.ASCII.GetBytes(Command + "\r\n")); // sends the needed command 
            return ReceiveReply(expectedReply);
        }
        static string ReceiveReply(ExpectedReply expectedReply)
        {
            int n = Control.Receive(buffer); // needed to convert the reply from the server 
            string reply = Encoding.ASCII.GetString(buffer, 0, n);  
            if (int.Parse(reply[0].ToString()) != (int)expectedReply)
            {
                throw new Exception(reply); 
            }
            return reply;
        }
        static public async Task<bool> Connect(string Address, int port) // initiate the connection 
        {
            bool result = true;
            await Task.Run(() =>
            {
                try
                {
                    Control.Connect(new IPEndPoint(IPAddress.Parse(Address), port));
                }
                catch(Exception)
                {
                    result = false;
                    return;
                }
                ReceiveReply(ExpectedReply.PositiveComplete);
            });
            return result; // if coonected or not 
        }
        static public async Task<bool> Login(string Username, string Password) 
        {
            bool result = true;
            await Task.Run(() =>  
            {
                try
                {
                    SendCommand("USER " + Username, ExpectedReply.PositiveAwatingFurtherInput);  
                    SendCommand("PASS " + Password, ExpectedReply.PositiveComplete);
                }
                catch(Exception)
                {
                    result = false;
                }
            });
            return result; // loged or not 
        }
        static public async Task InitiatePassiveTransmission() // recieve the IPaddress and port from the server 
        {
            isPassive = true;
            string s = "";
            await Task.Run(() => 
            {
                s = SendCommand("PASV", ExpectedReply.PositiveComplete);
            });
            string[] address = s.Substring(s.LastIndexOf('(') + 1, s.LastIndexOf(')') - s.LastIndexOf('(') - 1).Split(',');
            byte[] addressBytes = new byte[address.Length];
            for(int i = 0; i < address.Length; i++)
            {
                addressBytes[i] = byte.Parse(address[i]);
            }
            PassiveAddress = BitConverter.ToInt32(addressBytes, 0);
            PassivePort = BitConverter.ToInt16(addressBytes, 4);
        }
        static public async Task InitiateActiveTransmission() /// sends the address and port to the server to connect on 
        {
            Data.Bind(new IPEndPoint(IPAddress.Any, 0));
            Data.Listen(1);
            isPassive = false;
            List<byte> addressBytes = new List<byte>(6);
            addressBytes.AddRange(((IPEndPoint)Control.LocalEndPoint).Address.GetAddressBytes());
            addressBytes.AddRange(BitConverter.GetBytes((short)((IPEndPoint)Data.LocalEndPoint).Port).Reverse());
            /*
            addressBytes[0] = 192;
            addressBytes[1] = 168;
            addressBytes[2] = 43;
            addressBytes[3] = 205;
            */
            string Message = "";
            foreach(byte b in addressBytes)
            {
                Message += b.ToString() + ',';
            }
            await Task.Run(() =>
            {
                SendCommand("PORT " + Message.Substring(0, Message.Length - 1), ExpectedReply.PositiveComplete);
            });
        }
        static async Task NavigateToDirectory(string FullPath)
        {
            await Task.Run(() =>
            {
                SendCommand("CWD /", ExpectedReply.PositiveComplete); // change working directory
                string[] directorires = FullPath.Split('/');
                foreach (string d in directorires)
                {
                    SendCommand("CWD " + d, ExpectedReply.PositiveComplete);
                }
            });
        }
        static public async Task<string[]> GetDirectoriesAndFiles(string s)
        {
            await NavigateToDirectory(s);
            string res = "";
            await Task.Run(() =>
            {
                Socket socket = null;
                Task t = null;
                if (isPassive)
                {
                    Data = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Data.Connect(new IPEndPoint(PassiveAddress, PassivePort));
                    socket = Data;
                }
                if (!isPassive)
                {
                    t = Task.Run(() =>
                    {
                        socket = Data.Accept();
                    });
                }
                SendCommand("TYPE A", ExpectedReply.PositiveComplete);
                SendCommand("NLST", ExpectedReply.PositiveIncomplete);
                if(t != null)
                {
                    t.Wait();
                }
                int n = 0;
                do
                {
                    n = socket.Receive(buffer);
                    res += Encoding.ASCII.GetString(buffer, 0, n);
                }
                while (n != 0);
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                ReceiveReply(ExpectedReply.PositiveComplete);
            });
            List<string> resArr = new List<string>();
            foreach(string str in res.Split('\n', '\r')) 
            {
                if(!string.IsNullOrWhiteSpace(str))
                {
                    resArr.Add(str);
                }
            }
            return resArr.ToArray(); // return directories 
        }
        public delegate void ProgressListener(int Progress);
        static public async Task DownloadFile(string s, ProgressListener listener = null)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = s.Substring(s.LastIndexOf('.')) + " | ",
                FileName = s.Substring(s.LastIndexOf('/') + 1) 
            };
            saveFileDialog.ShowDialog();
            await NavigateToDirectory(s.Substring(0, s.LastIndexOf('/')));
            Socket socket = null;
            if (isPassive)
            {
                await Task.Run(() =>
                {
                    Data = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    Data.Connect(new IPEndPoint(PassiveAddress, PassivePort));
                    socket = Data;
                });
            }
            else
            {
                await Task.Run(() =>
                {
                    socket = Data.Accept();
                });
            }
            int progress = 0;
            await Task.Run(() =>
            {
                SendCommand("TYPE I", ExpectedReply.PositiveComplete);
                SendCommand("RETR " + s.Substring(s.LastIndexOf('/') + 1), ExpectedReply.PositiveIncomplete);
                FileStream file = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write); 
                int n = 0;
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                do
                {
                    n = socket.Receive(buffer);
                    file.Write(buffer, 0, n);
                    if (listener != null)
                    {
                        progress += n;
                        if (stopwatch.Elapsed.Milliseconds >= 100)
                        {
                            stopwatch.Restart();
                            listener(progress);
                        }
                    }
                }
                while (n != 0);
                file.Close();
            });
            socket.Shutdown(SocketShutdown.Both); 
            socket.Close();
            ReceiveReply(ExpectedReply.PositiveComplete);
        }
        static public async Task Rename(string Path, string NewName)
        {
            await NavigateToDirectory(Path.Substring(0, Path.LastIndexOf('/')));
            await Task.Run(() =>
            {
                SendCommand("RNFR " + Path.Substring(Path.LastIndexOf('/') + 1), ExpectedReply.PositiveAwatingFurtherInput);
                SendCommand("RNTO " + NewName, ExpectedReply.PositiveComplete);
            });
        }
        static public async Task Delete(string Path)
        {
            await NavigateToDirectory(Path.Substring(0, Path.LastIndexOf('/')));
            await Task.Run(() =>
            {
                SendCommand("DELE " + Path.Substring(Path.LastIndexOf('/') + 1), ExpectedReply.PositiveComplete);
            });
        }
        static public async Task CreateDirectory(string Path, string DirectoryName)
        {
            await NavigateToDirectory(Path);
            await Task.Run(() => SendCommand("MKD " + DirectoryName, ExpectedReply.PositiveComplete));
        }
    }
}