/*
The MIT License(MIT)

Copyright(c) 2015 AlexCatch

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExigoRa4wVPN
{
    //config enum for ease
    public enum OpenVPNManagerConfigurationFile
    {
        Example1,
        Example2,
        Example3,
        Example4,
        Example5,
        Example6,
        Example7,
        Example8,
    }

    public static class OpenVPNManager
    {
        //configFilePath should be the file path to the selected config file after assigned by the switch case
        static String configFilePath;
        static Process openVPN;

        //event handlers
        public static EventHandler SuccessfullyConnected;
        public static EventHandler InvalidCredentials;
        public static EventHandler InvalidConfigurationFile;
        public static EventHandler SuccessfullyDisconnected;
        public static EventHandler FatalErrorOccured;
        public static EventHandler DidBeginConnect;
        public static EventHandler ProcessDidExit;
        public static EventHandler ProcessDidWriteToOutputStream;

        public static void connect(OpenVPNManagerConfigurationFile server, String authFilePath)
        {
            //check for an existing process
            if (openVPN != null)
            {
                EventWaitHandle resetEvent = EventWaitHandle.OpenExisting("CloseConnection");
                resetEvent.Set();
                openVPN = null;
            }
            //Assign configFilePath a value corresponding with selected enum value
            switch (server) {
                case OpenVPNManagerConfigurationFile.Example1:
                    configFilePath = "../../OpenVPN/cert1.ovpn";
                    break;
                case OpenVPNManagerConfigurationFile.Example2:
                    configFilePath = "../../OpenVPN/cert1.ovpn";
                    break;
                case OpenVPNManagerConfigurationFile.Example3:
                    configFilePath = "../../OpenVPN/cert1.ovpn";
                    break;
                case OpenVPNManagerConfigurationFile.Example4:
                    configFilePath = "../../OpenVPN/cert1.ovpn";
                    break;
                case OpenVPNManagerConfigurationFile.Example5:
                    configFilePath = "../../OpenVPN/cert1.ovpn";
                    break;
                case OpenVPNManagerConfigurationFile.Example6:
                    configFilePath = "../../OpenVPN/cert1.ovpn";
                    break;
                case OpenVPNManagerConfigurationFile.Example7:
                    configFilePath = "../../OpenVPN/cert1.ovpn";
                    break;
                case OpenVPNManagerConfigurationFile.Example8:
                    configFilePath = "../../OpenVPN/cert1.ovpn";
                    return;
                default:
                    configFilePath = "";
                    break;
            }

            Action<object, DataReceivedEventArgs> outputHandler = new Action<object, DataReceivedEventArgs>((sendingProcess,  outLine) =>
            {
                //used for handling our output
                String line = outLine.Data;
                if (!String.IsNullOrEmpty(line))
                {
                    if (line.Contains("Initialization Sequence Completed"))
                    {
                        //successfully conntect
                        if (SuccessfullyConnected != null)
                        {
                            SuccessfullyConnected(line, null);
                        }
                    }
                    else if (line.Contains("auth-failure"))
                    {
                        //user failed to auth
                        if (InvalidCredentials != null) {
                            InvalidCredentials(line, null);
                        }
                    }
                    else if (line.Contains("Error opening configuration file"))
                    {
                        //could not find configuration file
                        if (InvalidConfigurationFile != null)
                        {
                            InvalidConfigurationFile(line, null);
                        }
                    }else if (line.Contains("fatal error"))
                    {
                        if (FatalErrorOccured != null)
                        {
                            FatalErrorOccured(line, null);
                        }
                    }
                    //event for output stream
                    if (ProcessDidWriteToOutputStream != null)
                    {
                        ProcessDidWriteToOutputStream(line, null);
                    }
                }
            });

            ProcessStartInfo openVPNProcessInformation = new ProcessStartInfo("openvpn");
            openVPNProcessInformation.WorkingDirectory = Directory.GetCurrentDirectory();
            openVPNProcessInformation.Arguments = String.Format("--config {0} --service CloseConnection 0 --auth-user-pass {1}", configFilePath, authFilePath);
            openVPNProcessInformation.CreateNoWindow = true;
            openVPNProcessInformation.RedirectStandardOutput = true;
            openVPNProcessInformation.RedirectStandardError = true;
            openVPNProcessInformation.UseShellExecute = false;

            openVPN = new Process();
            openVPN.StartInfo = openVPNProcessInformation;
            openVPN.EnableRaisingEvents = true;
            openVPN.OutputDataReceived += new DataReceivedEventHandler(outputHandler);
            openVPN.ErrorDataReceived += new DataReceivedEventHandler(outputHandler);
            openVPN.Exited += (o, k) =>
            {
                if (ProcessDidExit != null)
                {
                    ProcessDidExit(null, null);
                }
            };
            if (DidBeginConnect != null)
            {
                DidBeginConnect(true, null);
            }

            openVPN.Start();
            openVPN.BeginErrorReadLine();
            openVPN.BeginOutputReadLine();
        }

        public static void disconnect()
        {
            EventWaitHandle resetEvent = EventWaitHandle.OpenExisting("CloseConnection");
            resetEvent.Set();
        }
    }
}
