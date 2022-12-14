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
using Finalproject230.Models;

namespace Finalproject230.View;

public partial class GraphView : ContentPage
{
    private bool bPortOpen = false;     // boolean variable for the button of open/close
    private string newPacket = "";      // packet received
    private int oldPacketNumber = -1;   // this variable used for rollover
    private int newPacketNumber = 0;    // packet received with nre instances
    private int lostPacketCount = 0;    // number of packets lost
    private int packetRollover = 0;     // number of rollovers
    private int chkSumError = 0;        // checking the sum error

    Random random = new Random();

    public int Yaxis = 0;           // required for calculations in graph function
    public double degrees = 0;      // required for calculations in graph function
    public int count = 0;   //required for calculations in graph function
    public int graphHeight = 350;   //required for calculations in graph function

    StringBuilder stringBuilderSend = new StringBuilder("###1111196");
    SerialPort serialPort = new SerialPort();
    SolarData solarData = new SolarData();

    public GraphView()
    {
        InitializeComponent();
        string[] ports = SerialPort.GetPortNames();     // look for comports
        Console.WriteLine("the following serial ports were found"); // set comport
        portPicker.ItemsSource = ports;
        portPicker.SelectedIndex = ports.Length;
        Loaded += MainPage_Loaded;      // run the next function
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
        timer.Elapsed += new ElapsedEventHandler(DrawNewPointOnGraph);  // this habilatates the function DrawNewPointOnGraph
        timer.Start();                                      // start function
    }
    private void DrawNewPointOnGraph(object sender, ElapsedEventArgs e) // function for the graphics of our solar panel 
    {
        var graphicsView = this.LineGraphView;      // call the function GraphView
        var lineGraphDrawable = (LineDrawable)graphicsView.Drawable;    // call the function drawable
        lineGraphDrawable.lineGraphs[0].Yaxis = graphHeight - ((int)(solarData.analogVoltage[0]) / 10); // graph of solar voltage
        lineGraphDrawable.lineGraphs[1].Yaxis = graphHeight - ((int)(solarData.analogVoltage[2]) / 10); // graph of battery voltage
        lineGraphDrawable.lineGraphs[2].Yaxis = graphHeight - ((int)(solarData.analogVoltage[5]) / 10); // graph of potentiometer voltage
        lineGraphDrawable.lineGraphs[3].Yaxis = graphHeight - (int)((solarData.analogVoltage[1] - solarData.analogVoltage[4]) / 8); // graph of battery current 
        lineGraphDrawable.lineGraphs[4].Yaxis = graphHeight - (int)((solarData.analogVoltage[1] - solarData.analogVoltage[3]) / 8); // graph of LED1 current
        lineGraphDrawable.lineGraphs[5].Yaxis = graphHeight - (int)((solarData.analogVoltage[1] - solarData.analogVoltage[2]) / 8); // graph of LED2 current
        graphicsView.Invalidate();  // requires to draw itselg
    }
        private void setUpSerialPort()      // this function was not used in this project
    {
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) // assignation of the comport
    {
        newPacket = serialPort.ReadLine();      // assignation of the comport
        MainThread.BeginInvokeOnMainThread(MyMainThreadCode);   // run the followinf fucntion
    }

    private void MyMainThreadCode()
    {
        labelPacketLength.Text = newPacket.Length.ToString();
        int calChkSum = 0;          // setting new variable for the checksum
        if (newPacket.Length > 37)          // this function is only taking the packets required more than 37 characteres
        {
            if (newPacket.Substring(0, 3) == "###")  // if the packet contains ###
            {
                newPacketNumber = Convert.ToInt32(newPacket.Substring(3, 3));
                labelPacketNum.Text = newPacket.Substring(3, 3);
                newPacketNumber = Convert.ToInt32(labelPacketNum.Text);
                labelAN0.Text = newPacket.Substring(6, 4);
                labelAN1.Text = newPacket.Substring(10, 4);
                labelAN2.Text = newPacket.Substring(14, 4);
                labelAN3.Text = newPacket.Substring(18, 4);
                labelAN4.Text = newPacket.Substring(22, 4);
                labelAN5.Text = newPacket.Substring(26, 4);
                labelBin.Text = newPacket.Substring(30, 4);
                labelRxChkSum.Text = newPacket.Substring(34, 3);

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
                { calChkSum += (byte)newPacket[i]; }
                labelCalChkSum.Text = Convert.ToString(calChkSum);
                calChkSum %= 1000;
                int recChkSum = Convert.ToInt32(newPacket.Substring(34, 3));
                if (recChkSum == calChkSum)     // verification of packets
                {
                    DisplaySolarData(newPacket);
                    oldPacketNumber = newPacketNumber;
                }
                else                             // in other case, it is an error
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
        }
    }
    private void DisplaySolarData(string validPacket)   // function with the solar class, here are the calculations of analogs
    {
        solarData.ParseSolarData(validPacket);      // calling the variables
        labelSolarVolt.Text = solarData.GetVoltage(solarData.analogVoltage[0]);  // solar voltage value
        labelBatteryVolt.Text = solarData.GetVoltage(solarData.analogVoltage[2]);   // battery voltage value
        labelBatteryCurrent.Text = solarData.GetCurrent(solarData.analogVoltage[1], solarData.analogVoltage[2]);    // battery current value
        labelLed1Current.Text = solarData.GetLedCurrent(solarData.analogVoltage[1], solarData.analogVoltage[4]);    // LED1 current value
        labelLed2Current.Text = solarData.GetLedCurrent(solarData.analogVoltage[1], solarData.analogVoltage[3]);    // LED2 current value
        labelPotVolt.Text = solarData.GetVoltage(solarData.analogVoltage[5]);   // potentiometer voltage value
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
        var graphicsView = this.LineGraphView;      // calling function graphicView
        var lineGraphDrawable = (LineDrawable)graphicsView.Drawable;    // calling function drawable
        newPacket = "";                 // first values
        serialPort.Close();             // first values
        btnOpenClose.Text = "Open";     // first values
        bPortOpen = false;              // first values
        oldPacketNumber = -1;           // first values
        newPacketNumber = 0;            // first values
        labelSolarVolt.Text = "  0.00V";        // first values
        labelBatteryVolt.Text = "  0.00V";      // first values
        labelPotVolt.Text = "  0.00V";          // first values
        labelBatteryCurrent.Text = "  0.00mA";  // first values
        labelLed1Current.Text = "  0.00mA";     // first values
        labelLed2Current.Text = "  0.00mA";     // first values
        solarData.analogVoltage[0] = (-solarData.analogVoltage[0] * 0); // graph to zero
        solarData.analogVoltage[2] = (-solarData.analogVoltage[0] * 0); // graph to zero
        solarData.analogVoltage[5] = (-solarData.analogVoltage[0] * 0); // graph to zero
        solarData.analogVoltage[1] = (-solarData.analogVoltage[0] * 0); // graph to zero
        solarData.analogVoltage[3] = (-solarData.analogVoltage[0] * 0); // graph to zero
        solarData.analogVoltage[4] = (-solarData.analogVoltage[0] * 0); // graph to zero
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
    { ButtonClicked(3); }     // Button LED 4                       
    private void btnBit2_Clicked(object sender, EventArgs e)
    { ButtonClicked(2); }     // Button LED 3
    private void btnBit1_Clicked(object sender, EventArgs e)
    { ButtonClicked(1); }     // Button LED 2
    private void btnBit0_Clicked(object sender, EventArgs e)
    { ButtonClicked(0); }     // Button LED 1
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
            { calSendChkSum += (byte)stringBuilderSend[i]; }
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
        { DisplayAlert("Alert", ex.Message, "Ok"); }
    }
    private void Label_SizeChanged(object sender, EventArgs e)
    { }      // this fucntion is not used in this project
    private void labelBatteryCurrent_Focused(object sender, FocusEventArgs e)
    { }      // this fucntion is not used in this project
    private void imgLED1_Clicked(object sender, EventArgs e)
    { ButtonClicked(0); }// assignation of the button 1 to the LED1
    private void imgLED2_Clicked(object sender, EventArgs e)
    { ButtonClicked(1); }// assignation of the button 2 to the LED2
}
