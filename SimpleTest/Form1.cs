using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Com.QuantAsylum;
using System.Threading;

namespace SimpleTest
{

    public partial class Form1 : Form
    {
        QA100Interface Scope;
        QA300Interface PowerSupply;

        public Form1()
        {
            InitializeComponent();
        }

private void button1_Click(object sender, EventArgs e)
{
    // Set up Scope
    QAConnectionManager.LaunchAppIfNotRunning(QAConnectionManager.Devices.QA100);
    Scope = (QA100Interface)QAConnectionManager.ConnectTo(QAConnectionManager.Devices.QA100);

    // Setup PowerSupply
    QAConnectionManager.LaunchAppIfNotRunning(QAConnectionManager.Devices.QA300);
    PowerSupply = (QA300Interface)QAConnectionManager.ConnectTo(QAConnectionManager.Devices.QA300);
}

private void button2_Click(object sender, EventArgs e)
{
    try
    {
        // Set power supply to 3.3V and 0.1A limiting, then enable output
        PowerSupply.SetVoltage(3.3);
        PowerSupply.SetCurrent(0.1);
        PowerSupply.SetOutput(QA300.OuputState.Enabled);

        // Set scope parameters
        Scope.SetVertSense(QA100.ChanEnum.CH1, 0.1);  // Set to 1V/div on channel 1
        Scope.SetSweepRate(1e-3);                   // Set to 1 mS/div

        // Start acquisition and wait for it to finish
        Scope.RunSingle();
        while (Scope.GetAcquisitionState() != QA100.AcquisitionState.Stopped)
        {
            Thread.Sleep(50);
        }

        // Grab the channel 1 data
        PointF[] data = Scope.GetAnalogData(QA100.ChanEnum.CH1);
    }
    catch
    {
        // If we end up in here, there was an error of some kind
    }
}
    }
}
