using System;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.Timers;
using System.Threading;
using Microsoft.Maui.Devices;

namespace Finalproject230.View;


public partial class RawDataView : ContentPage
{
    private bool bPortOpen = false;     // boolean variable for the button of open/close
    private string newPacket = "";      // packet received
    private int oldPacketNumber = -1;   // this variable used for rollover
    private int newPacketNumber = 0;    // packet received with nre instances
    private int lostPacketCount = 0;    // number of packets lost
    private int packetRollover = 0;     // number of rollovers
    private int chkSumError = 0;        // checking the sum error

    Random random = new Random();

    StringBuilder stringBuilderSend = new StringBuilder("###1111196");

    SerialPort serialPort = new SerialPort();

    public RawDataView()
    {
        InitializeComponent();
        string[] ports = SerialPort.GetPortNames();                 // look for comports
        Console.WriteLine("the following serial ports were found"); // set comport
        portPicker.ItemsSource = ports;                                             
        portPicker.SelectedIndex = ports.Length;
        Loaded += MainPage_Loaded;                                  // run the next function
        //		foreach (string port in ports)
        //		{
        //          portPicker.Items.Add(port);
        //      }    
    }

    private void MainPage_Loaded(object sender, EventArgs e)
    {
        serialPort.BaudRate = 115200;                       // configuration for seeing the serial data
        serialPort.ReceivedBytesThreshold = 1;              // number of bytes
        serialPort.DataReceived += SerialPort_DataReceived; // increased by one SerialPort_DataReceived
        setUpSerialPort();                                  // set the comport
        var timer = new System.Timers.Timer(10);            // time for the received packets
        timer.Start();                                      // start function
    }

    private void setUpSerialPort()      // this function was not used in this project
    {
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) // assignation of the comport
    {
        newPacket = serialPort.ReadLine();      // assignation of the comport
        MainThread.BeginInvokeOnMainThread(MyMainThreadCode);   // run the followinf fucntion
    }

    private void MyMainThreadCode()     // function for managing the packet received
    {
        if (CheckedBoxHistory.IsChecked == true)                // in case it was selected the box history of received data
        {labelRXdata.Text = newPacket + labelRXdata.Text;}     
        else
        {labelRXdata.Text = newPacket;}                         // in other case, only showing the current packet
        labelPacketLength.Text = newPacket.Length.ToString();
        int calChkSum = 0;          // setting new variable for the checksum
        if (newPacket.Length > 37)          // this function is only taking the packets required more than 37 characteres
        {
            if (newPacket.Substring(0, 3) == "###")  // if the packet contains ###
            {
                newPacketNumber = Convert.ToInt32(newPacket.Substring(3, 3));   // convert in Integer
                labelPacketNum.Text = newPacket.Substring(3, 3);                // assignation of labelPacketNum
                newPacketNumber = Convert.ToInt32(labelPacketNum.Text);         // assignation of newPacketNumber
                labelAN0.Text = newPacket.Substring(6, 4);                      // assignation of labelAN0
                labelAN1.Text = newPacket.Substring(10, 4);                     // assignation of labelAN1
                labelAN2.Text = newPacket.Substring(14, 4);                     // assignation of labelAN2
                labelAN3.Text = newPacket.Substring(18, 4);                     // assignation of labelAN3
                labelAN4.Text = newPacket.Substring(22, 4);                     // assignation of labelAN4
                labelAN5.Text = newPacket.Substring(26, 4);                     // assignation of labelAN5
                labelBin.Text = newPacket.Substring(30, 4);                     // assignation of labelBin
                labelRxChkSum.Text = newPacket.Substring(34, 3);                // assignation of labelRxChkSum

                if (oldPacketNumber > -1)       // this step is for taking the following packets
                {
                    if (newPacketNumber < oldPacketNumber)      // newPacketNumber==0
                    {
                        packetRollover++;       // the rollover is counting
                        labelPacketRollover.Text = packetRollover.ToString();
                        if (oldPacketNumber != 999)         // if it is more than 999 count the 0 again
                        {
                            lostPacketCount += 999 - oldPacketNumber + newPacketNumber;
                            labelPacketLost.Text = lostPacketCount.ToString();
                        }
                    }
                    else             // in other case, counting by 1
                    {
                        if (newPacketNumber != oldPacketNumber + 1)
                        {
                            lostPacketCount += newPacketNumber - oldPacketNumber - 1;
                            labelPacketLost.Text = lostPacketCount.ToString();
                        }
                    }
                }
                for (int i = 3; i < 34; i++)            // this part is helpful for the verification of checksum
                {calChkSum += (byte)newPacket[i];}
                labelCalChkSum.Text = Convert.ToString(calChkSum);
                calChkSum %= 1000;
                int recChkSum = Convert.ToInt32(newPacket.Substring(34, 3));
                if (recChkSum == calChkSum)             // verification of packets
                {
                    oldPacketNumber = newPacketNumber;  
                }
                else                                    // in other case, it is an error
                {
                    chkSumError++;
                    labelChkSumError.Text = chkSumError.ToString();
                }
            }
            string parsedData =    // this part helps to put our string values in dashboard, this covers all th packet.
                        $"{newPacket.Length,-13}" +
                        $"{newPacket.Substring(0, 3),-12}" +
                        $"{newPacket.Substring(3, 3),-14}" +
                        $"{newPacket.Substring(6, 4),-13}" +
                        $"{newPacket.Substring(10, 4),-13}" +
                        $"{newPacket.Substring(14, 4),-13}" +
                        $"{newPacket.Substring(18, 4),-13}" +
                        $"{newPacket.Substring(22, 4),-13}" +
                        $"{newPacket.Substring(26, 4),-12}" +
                        $"{newPacket.Substring(30, 4),-12}" +
                        $"{newPacket.Substring(34, 3),-12}" +
                        $"{calChkSum,-17}" +
                        $"{lostPacketCount,-16}" +
                        $"{chkSumError,-10}" +
                        $"{packetRollover}\r\n";
            if (CheckedParsedBoxHistory.IsChecked == true)  // this part is similar from the recoved packet, but this is showed in parse packet
            {labelParsedData.Text = parsedData + labelParsedData.Text;}
            else
            {labelParsedData.Text = parsedData;}
        }
    }
    private void btnOpenClose_Clicked(object sender, EventArgs e)  // function for managing the button open close
    {
        if (!bPortOpen)     // if it is not pressed
        {
            serialPort.PortName = portPicker.SelectedItem.ToString();
            serialPort.Open();
            btnOpenClose.Text = "Close";
            bPortOpen = true;
        }
        else               // if it is pressed
        {
            serialPort.Close();
            btnOpenClose.Text = "Open";
            bPortOpen = false;
        }
    }
    private void btnClear_Clicked(object sender, EventArgs e)   // function for managing the button open close
    {
        newPacket = "";                 // first values
        serialPort.Close();             // first values
        btnOpenClose.Text = "Open";     // first values
        bPortOpen = false;              // first values
        labelRXdata.Text = "0";         // first values
        labelParsedData.Text = "0";     // first values
        labelPacketNum.Text = "0";      // first values 
        labelPacketLength.Text = "0";   // first values
        labelAN0.Text = "0";            // first values
        labelAN1.Text = "0";            // first values
        labelAN2.Text = "0";            // first values
        labelAN3.Text = "0";            // first values
        labelAN4.Text = "0";            // first values
        labelAN5.Text = "0";            // first values
        labelBin.Text = "0";            // first values
        labelRxChkSum.Text = "0";       // first values
        labelCalChkSum.Text = "0";      // first values
        labelPacketLost.Text = "0";     // first values
        labelChkSumError.Text = "0";    // first values
        labelPacketRollover.Text = "0"; // first values
        oldPacketNumber = -1;           // first values
        newPacketNumber = 0;            // first values
    }

    private void btnSend_Clicked(object sender, EventArgs e) // function for managing the button send
    {
        try     // send the packet
        {
            entrySend.Text = entrySend.ToString();
            String messageOut = entrySend.ToString();
            messageOut += "\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageOut);
            serialPort.Write(messageBytes, 0, messageBytes.Length);           //possible error
        }
        catch (Exception ex)    //in case there is not the required packet, try again
        {
            DisplayAlert("Alert", ex.Message, "Ok");
        }
    }

    private void btnBit3_Clicked(object sender, EventArgs e)        
    {        ButtonClicked(3); }     // Button LED 4                       

    private void btnBit2_Clicked(object sender, EventArgs e)
    {        ButtonClicked(2); }     // Button LED 3

    private void btnBit1_Clicked(object sender, EventArgs e)
    {        ButtonClicked(1); }     // Button LED 2

    private void btnBit0_Clicked(object sender, EventArgs e)
    {        ButtonClicked(0); }     // Button LED 1

    private void ButtonClicked(int i)       // function for making the LEDs pictures buttons
    {
        Button[] btnBit = new Button[] { btnBit0, btnBit1, btnBit2, btnBit3 };
        if (btnBit[i].Text == "0")
        {
            btnBit[i].Text = "1";
            stringBuilderSend[i + 3] = '1';
            switch (i)              // assignation the buttons
            {
                case 0:             // assignation of LED1 (off)
                    imgLED1.Source = "ledoff.png";
                    break;
                case 1:             // assignation of LED2 (off)
                    imgLED2.Source = "ledoff.png";
                    break;
            }
        }
        else
        {
            btnBit[i].Text = "0";
            stringBuilderSend[i + 3] = '0';
            switch (i)              // assignation the buttons
            {
                case 0:             // assignation of LED1 (on)
                    imgLED1.Source = "ledon.png";
                    break;
                case 1:             // assignation of LED2 (on)
                    imgLED2.Source = "ledon.png";
                    break;
            }
        }
        SendPacket();       // at the end send the packet
    }

    private void SendPacket()     // function for sending bynary values   
    {
        int calSendChkSum = 0;      // cheking the chkSum
        try
        {
            for (int i = 3; i < 7; i++)
            {                calSendChkSum += (byte)stringBuilderSend[i];            }
            calSendChkSum %= 1000;      // checking the number of packets
            stringBuilderSend.Remove(7, 3);
            stringBuilderSend.Insert(7, calSendChkSum.ToString());  // send to 0 again
            string messageOut = stringBuilderSend.ToString();   
            entrySend.Text = stringBuilderSend.ToString();
            messageOut += "\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageOut);
            serialPort.Write(messageBytes, 0, messageBytes.Length);
        }
        catch (Exception ex)
        {            DisplayAlert("Alert", ex.Message, "Ok");        }
    }

    private void Label_SizeChanged(object sender, EventArgs e)
    {    }      // this fucntion is not used in this project

    private void labelBatteryCurrent_Focused(object sender, FocusEventArgs e)
    { }      // this fucntion is not used in this project

    private void imgLED1_Clicked(object sender, EventArgs e)
    {        ButtonClicked(0);    }// assignation of the button 1 to the LED1

    private void imgLED2_Clicked(object sender, EventArgs e)
    {        ButtonClicked(1);    }// assignation of the button 2 to the LED2
}
