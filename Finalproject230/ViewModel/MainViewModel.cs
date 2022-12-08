using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Finalproject230.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finalproject230.ViewModel
{
    [ObservableObject]
    public partial class MainViewModel
    {
        private bool bPortOpen = false;
        [ObservableProperty]
        string newPacket;
        SerialPort serialPort = new SerialPort();
        public SolarData solarData { get; set; } = new SolarData();
        public MainViewModel()
        {
            serialPort.BaudRate = 115200;
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DataReceived += SerialPort_DataReceived;
            solarData.OpenText = "Open";
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            solarData.ValidPacket = solarData.getValidPacket(serialPort.ReadLine());
        }

        [RelayCommand]
        void OpenClose()
        {
            if(!bPortOpen)
            {
                serialPort.PortName = "COM6";
                serialPort.Open();
                solarData.OpenText = "Close";
                bPortOpen = true;
            }
            else
            {
                serialPort.Close();
                solarData.OpenText = "Open";
                bPortOpen = false;
            }
        }
    }
}
