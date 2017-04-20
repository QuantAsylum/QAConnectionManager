using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

// Steps required
// 1) Add remoting reference
// 2) Add reference to QA application in the install directory
// 3) Add Using references noted below (4 total)
// 4) 

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Com.QuantAsylum;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using System.Collections;

namespace ConnectToQAApp
{
    public partial class Form1 : Form
    {
        // This is the reference we'll use to control the scope and power supply
        QA100Interface Scope;
        QA300Interface PowerSupply;
        QA400Interface AudioAnalyzer;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Simplest connection to QA100. 
        /// </summary>
        /// <returns></returns>
        private bool ConnectToQA100_Example1()
        {
            try
            {
                TcpChannel tcpChannel = new TcpChannel();
                ChannelServices.RegisterChannel(tcpChannel, false);

                Type requiredType = typeof(Com.QuantAsylum.QA100Interface);

                Scope = (Com.QuantAsylum.QA100Interface)Activator.GetObject(requiredType, "tcp://localhost:9100/QuantAsylumQA100Server");
            }
            catch
            {
                Scope = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Smarter connection to QA100 that also checks if the EXE is running before trying to connect
        /// </summary>
        /// <returns></returns>
        private bool ConnectToQA100_Example2()
        {
            // First, we see if the QA100 application is already running. If there are zero instances of this
            // process, then we can assume it isn't running
            if (Process.GetProcessesByName("QAScope").Length == 0)
            {
                // Power supply application isn't running. Let the user know and then return false
                MessageBox.Show("The QA100 application is not running. Please start that application before attempting to control it remotely.");
                return false;
            }

            // At this point, we're certain the application is running. Now, try to connect to it
            try
            {
                TcpChannel tcpChannel = new TcpChannel();
                ChannelServices.RegisterChannel(tcpChannel, false);

                Type requiredType = typeof(Com.QuantAsylum.QA100Interface);

                Scope = (Com.QuantAsylum.QA100Interface)Activator.GetObject(requiredType, "tcp://localhost:9100/QuantAsylumQA100Server");
            }
            catch
            {
                Scope = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Connect to multiple devices. This is a bit trickier, because we can't use the default remoting connections becuase they
        /// will both want to use the same port. So, we need to add extra code to permit assigning different ports
        /// </summary>
        /// <returns></returns>
        private bool ConnectToQA100And300()
        {
            // First, we see if the QA100 application is already running. If there are zero instances of this
            // process, then we can assume it isn't running
            if (Process.GetProcessesByName("QAScope").Length == 0)
            {
                // Power supply application isn't running. Let the user know and then return false
                MessageBox.Show("The QA100 application is not running. Please start that application before attempting to control it remotely.");
                return false;
            }

            // Next, we see if the QA300 application is already running. If there are zero instances of this
            // process, then we can assume it isn't running
            if (Process.GetProcessesByName("QA Power Supply").Length == 0)
            {
                // Power supply application isn't running. Let the user know and then return false
                MessageBox.Show("The QA300 application is not running. Please start that application before attempting to control it remotely.");
                return false;
            }

// Try to connect to both the scope and power supply. If either fails for any reason, then both will fail
try
{
    // Scope first
    TcpChannel tcpChannel = (TcpChannel)Helper.GetChannel(4301, false);
    ChannelServices.RegisterChannel(tcpChannel, false);

    Type requiredType = typeof(Com.QuantAsylum.QA100Interface);

    Scope = (Com.QuantAsylum.QA100Interface)Activator.GetObject(requiredType, "tcp://localhost:9100/QuantAsylumQA100Server");

    // Power supply next
    tcpChannel = (TcpChannel)Helper.GetChannel(4300, false);
    ChannelServices.RegisterChannel(tcpChannel, false);

    requiredType = typeof(Com.QuantAsylum.QA300Interface);

    PowerSupply = (Com.QuantAsylum.QA300Interface)Activator.GetObject(requiredType, "tcp://localhost:9300/QuantAsylumQA300Server");
}
catch (Exception ex)
{
    Scope = null; PowerSupply = null;
    return false;
}

            // If we get here, then everything worked: Both apps were running and we successfully connected to both
            return true;          
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (ConnectToQA100_Example1() == true)
            {
                // Connection succeed
            }
            else
            {
                // Connection failed
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ConnectToQA100_Example2() == true)
            {
                // Connection succeed
                string s = Scope.GetName();
            }
            else
            {
                // Connection failed
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ConnectToQA100And300() == true)
            {
                // Connection succeed
            }
            else
            {
                // Connection failed
            }
        }

/// <summary>
/// This class is only needed IF we want to connect to multiple devices
/// </summary>
public class Helper
{
    public static IChannel GetChannel(int tcpPort, bool isSecure)
    {
        BinaryServerFormatterSinkProvider serverProv =
            new BinaryServerFormatterSinkProvider();
        serverProv.TypeFilterLevel = TypeFilterLevel.Full;
        IDictionary propBag = new Hashtable();
        propBag["port"] = tcpPort;
        propBag["typeFilterLevel"] = TypeFilterLevel.Full;
        propBag["name"] = Guid.NewGuid().ToString();
        if (isSecure)
        {
            propBag["secure"] = isSecure;
            propBag["impersonate"] = false;
        }
        return new TcpChannel(
            propBag, null, serverProv);
    }
}

        private void button4_Click(object sender, EventArgs e)
        {
            //Object obj = ClassLibrary1.QAConnectionManager.ConnectTo(ClassLibrary1.QAConnectionManager.Devices.QA100);

            //Scope = (Com.QuantAsylum.QA100Interface)obj;

            //string s = Scope.GetName();

            if (QAConnectionManager.IsAppRunning(QAConnectionManager.Devices.QA100) && (QAConnectionManager.IsAppRunning(QAConnectionManager.Devices.QA400)))
            {
                Scope = (QA100Interface)QAConnectionManager.ConnectTo(QAConnectionManager.Devices.QA100);
                AudioAnalyzer = (QA400Interface)QAConnectionManager.ConnectTo(QAConnectionManager.Devices.QA400);
            }
            else
            {

            }

            string s = Scope.GetName();
            s = AudioAnalyzer.GetName();

            AudioAnalyzer.RunSingle();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            QAConnectionManager.LaunchAppIfNotRunning(QAConnectionManager.Devices.QA100);
            Scope = (QA100Interface)QAConnectionManager.ConnectTo(QAConnectionManager.Devices.QA100);

            bool isConnected = Scope.IsConnected();
            string s = Scope.GetName();
        }

       
    }
}
