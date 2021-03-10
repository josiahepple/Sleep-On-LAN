//UDP Server by Josiah Epple. This program configures a simple UDP server that will run `shutdown /s /f /t 0` if it receives a packet containing
// 0, 1, 2, 3, 4, 5, followed by the MAC address specified in the settings.txt file, in the base project directory.

using System;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace UDP_Server
{
     class Program
     {
          static void Main(string[] args)
          {
               int portNumber = 0;
               UInt64 macAddressInt = 0;
               byte[] macAddress = new byte[6];

               //load settings.txt to get port number and this device's MAC Address
               InitializeSettings(ref portNumber, ref macAddressInt);

               //convert the MAC address into individual bytes
               for (int i = 5; i >= 0; i--)
               {
                    macAddress[i] = (byte)(macAddressInt % 256);
                    macAddressInt /= 256;
               }

               //UDP Client listens on whichever port number is specified in settings.txt
               IPEndPoint listenPoint = new IPEndPoint(IPAddress.Any, portNumber);
               UdpClient udpServer = new UdpClient(listenPoint);
               Console.WriteLine($"UDP Server listening on Port {portNumber}...");

               while(true)
               {
                    bool shouldShutDown = true;
                    int j = 0;
                    byte[] receivedBytes = udpServer.Receive(ref listenPoint);

                    //first 6 bytes must be 0, 1, 2, 3, 4, 5
                    for(int i = 0; i < 6; i++)
                    {
                         if (receivedBytes[i] != (byte)i)
                         {
                              shouldShutDown = false;
                         }
                    }

                    //next 6 bytes must be the MAC Address from settings.txt
                    for(int i = 6; i < 12; i++)
                    {
                         if (receivedBytes[i] != macAddress[j++])
                         {
                              shouldShutDown = false;
                         }
                    }

                    //packet size must be 102 bytes
                    if (receivedBytes.Length != 102)
                         shouldShutDown = false;

                    if(shouldShutDown == true)
                    {
                         Console.WriteLine("Shutdown packet received for this MAC Address");
                         System.Diagnostics.Process.Start($@"..\..\..\shutdown.bat");
                    }
                    Console.WriteLine("Non-Shutdown packet received");
               }
          }
          static void InitializeSettings(ref int portNumber, ref UInt64 macAddressUInt)
          {
               StreamReader sr = new StreamReader($@"..\..\..\settings.txt");
               sr.ReadLine();
               portNumber = int.Parse(sr.ReadLine());
               sr.ReadLine();
               macAddressUInt = UInt64.Parse(sr.ReadLine(), System.Globalization.NumberStyles.HexNumber);
               sr.Close();
          }
     }
}
