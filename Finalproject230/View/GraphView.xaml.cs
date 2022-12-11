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

    public int Yaxis = 0;
    public double degrees = 0;
    public int count = 0;
    public int graphHeight = 350;

    StringBuilder stringBuilderSend = new StringBuilder("###1111196");

    SerialPort serialPort = new SerialPort();

    SolarData solarData = new SolarData();

    public GraphView()
    {
        InitializeComponent();
        string[] ports = SerialPort.GetPortNames();
        Console.WriteLine("the following serial ports were found");
        portPicker.ItemsSource = ports;
        portPicker.SelectedIndex = ports.Length;
        Loaded += MainPage_Loaded;
        //		foreach (string port in ports)
        //		{
        //          portPicker.Items.Add(port);
        //      }    

    }

    private void MainPage_Loaded(object sender, EventArgs e)
    {
        serialPort.BaudRate = 115200;
        serialPort.ReceivedBytesThreshold = 1;
        serialPort.DataReceived += SerialPort_DataReceived;
        setUpSerialPort();  //***
        var timer = new System.Timers.Timer(10);
        timer.Elapsed += new ElapsedEventHandler(DrawNewPointOnGraph);
        timer.Start();
    }

    private void DrawNewPointOnGraph(object sender, ElapsedEventArgs e)
    {
        var graphicsView = this.LineGraphView;
        var lineGraphDrawable = (LineDrawable)graphicsView.Drawable;
        lineGraphDrawable.lineGraphs[0].Yaxis = graphHeight - ((int)(solarData.analogVoltage[0]) / 10);
        lineGraphDrawable.lineGraphs[1].Yaxis = graphHeight - ((int)(solarData.analogVoltage[2]) / 10);
        lineGraphDrawable.lineGraphs[2].Yaxis = graphHeight - ((int)(solarData.analogVoltage[5]) / 10);
        lineGraphDrawable.lineGraphs[3].Yaxis = graphHeight - (int)((solarData.analogVoltage[1] - solarData.analogVoltage[4]) / 8);
        lineGraphDrawable.lineGraphs[4].Yaxis = graphHeight - (int)((solarData.analogVoltage[1] - solarData.analogVoltage[3]) / 8);
        lineGraphDrawable.lineGraphs[5].Yaxis = graphHeight - (int)((solarData.analogVoltage[1] - solarData.analogVoltage[2]) / 8);
        graphicsView.Invalidate();
    }




    private void setUpSerialPort()      //***
    {

    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        newPacket = serialPort.ReadLine();      //with this we can see which ports are working
        //labelRXdata.Text=newPacket;			// It is not working
        MainThread.BeginInvokeOnMainThread(MyMainThreadCode);

    }

    private void MyMainThreadCode()
    {
        //Code to run in the main thread
        //if (CheckedBoxHistory.IsChecked == true)
        //{
        //    labelRXdata.Text = newPacket + labelRXdata.Text;
        //}
        //else
        //{

        //    labelRXdata.Text = newPacket;
        //}
        labelPacketLength.Text = newPacket.Length.ToString();
        int calChkSum = 0;
        if (newPacket.Length > 37)
        {
            if (newPacket.Substring(0, 3) == "###")
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


                if (oldPacketNumber > -1)
                {
                    if (newPacketNumber < oldPacketNumber)      //newPacketNumber==0
                    {
                        packetRollover++;
                        labelPacketRollover.Text = packetRollover.ToString();
                        if (oldPacketNumber != 999)
                        {
                            lostPacketCount += 999 - oldPacketNumber + newPacketNumber;
                            labelPacketLost.Text = lostPacketCount.ToString();

                        }
                    }
                    else
                    {
                        if (newPacketNumber != oldPacketNumber + 1)
                        {
                            lostPacketCount += newPacketNumber - oldPacketNumber - 1;
                            labelPacketLost.Text = lostPacketCount.ToString();
                        }
                    }
                }
                for (int i = 3; i < 34; i++)
                {
                    calChkSum += (byte)newPacket[i];
                }
                labelCalChkSum.Text = Convert.ToString(calChkSum);
                calChkSum %= 1000;
                int recChkSum = Convert.ToInt32(newPacket.Substring(34, 3));
                if (recChkSum == calChkSum)
                {
                    DisplaySolarData(newPacket);
                    oldPacketNumber = newPacketNumber;
                }
                else
                {
                    chkSumError++;
                    labelChkSumError.Text = chkSumError.ToString();
                }

            }


            string parsedData =
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



            //if (CheckedParsedBoxHistory.IsChecked == true)
            //{
            //    labelParsedData.Text = parsedData + labelParsedData.Text;
            //}
            //else
            //{
            //    labelParsedData.Text = parsedData;
            //}

        }


    }

    private void DisplaySolarData(string validPacket)
    {
        solarData.ParseSolarData(validPacket);
        labelSolarVolt.Text = solarData.GetVoltage(solarData.analogVoltage[0]);
        labelBatteryVolt.Text = solarData.GetVoltage(solarData.analogVoltage[2]);
        labelBatteryCurrent.Text = solarData.GetCurrent(solarData.analogVoltage[1], solarData.analogVoltage[2]);
        labelLed1Current.Text = solarData.GetLedCurrent(solarData.analogVoltage[1], solarData.analogVoltage[4]);
        labelLed2Current.Text = solarData.GetLedCurrent(solarData.analogVoltage[1], solarData.analogVoltage[3]);
        labelPotVolt.Text = solarData.GetVoltage(solarData.analogVoltage[5]);
    }

    private void btnOpenClose_Clicked(object sender, EventArgs e)
    {
        if (!bPortOpen)
        {
            serialPort.PortName = portPicker.SelectedItem.ToString();
            serialPort.Open();
            btnOpenClose.Text = "Close";
            bPortOpen = true;
        }
        else
        {
            serialPort.Close();
            btnOpenClose.Text = "Open";
            bPortOpen = false;
        }
    }

    private void btnClear_Clicked(object sender, EventArgs e)
    {
        var graphicsView = this.LineGraphView;
        var lineGraphDrawable = (LineDrawable)graphicsView.Drawable;
        newPacket = "";
        serialPort.Close();
        btnOpenClose.Text = "Open";
        bPortOpen = false;
        oldPacketNumber = -1;
        newPacketNumber = 0;
        labelSolarVolt.Text = "  0.00V";
        labelBatteryVolt.Text = "  0.00V";
        labelPotVolt.Text = "  0.00V";
        labelBatteryCurrent.Text = "  0.00mA";
        labelLed1Current.Text = "  0.00mA";
        labelLed2Current.Text = "  0.00mA";
        solarData.analogVoltage[0] = (-solarData.analogVoltage[0] * 0);
        solarData.analogVoltage[2] = (-solarData.analogVoltage[0] * 0);
        solarData.analogVoltage[5] = (-solarData.analogVoltage[0] * 0);
        solarData.analogVoltage[1] = (-solarData.analogVoltage[0] * 0);
        solarData.analogVoltage[3] = (-solarData.analogVoltage[0] * 0);
        solarData.analogVoltage[4] = (-solarData.analogVoltage[0] * 0);
    }

    private void btnSend_Clicked(object sender, EventArgs e)
    {
        try
        {
            entrySend.Text = entrySend.ToString();
            String messageOut = entrySend.ToString();
            messageOut += "\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageOut);
            serialPort.Write(messageBytes, 0, messageBytes.Length);           //possible error
        }
        catch (Exception ex)
        {
            DisplayAlert("Alert", ex.Message, "Ok");
        }
    }

    private void btnBit3_Clicked(object sender, EventArgs e)
    {
        ButtonClicked(3);
    }

    private void btnBit2_Clicked(object sender, EventArgs e)
    {
        ButtonClicked(2);
    }

    private void btnBit1_Clicked(object sender, EventArgs e)
    {
        ButtonClicked(1);
    }

    private void btnBit0_Clicked(object sender, EventArgs e)
    {
        ButtonClicked(0);
    }

    private void ButtonClicked(int i)
    {
        Button[] btnBit = new Button[] { btnBit0, btnBit1, btnBit2, btnBit3 };
        if (btnBit[i].Text == "0")
        {
            btnBit[i].Text = "1";
            stringBuilderSend[i + 3] = '1';
            switch (i)
            {
                case 0:
                    imgLED1.Source = "ledoff.png";
                    break;
                case 1:
                    imgLED2.Source = "ledoff.png";
                    break;
            }
        }
        else
        {
            btnBit[i].Text = "0";
            stringBuilderSend[i + 3] = '0';
            switch (i)
            {
                case 0:
                    imgLED1.Source = "ledon.png";
                    break;
                case 1:
                    imgLED2.Source = "ledon.png";
                    break;
            }
        }
        SendPacket();
    }

    private void SendPacket()
    {
        int calSendChkSum = 0;
        try
        {
            for (int i = 3; i < 7; i++)
            {
                calSendChkSum += (byte)stringBuilderSend[i];
            }
            calSendChkSum %= 1000;
            stringBuilderSend.Remove(7, 3);
            stringBuilderSend.Insert(7, calSendChkSum.ToString());
            string messageOut = stringBuilderSend.ToString();
            entrySend.Text = stringBuilderSend.ToString();
            messageOut += "\r\n";
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageOut);
            serialPort.Write(messageBytes, 0, messageBytes.Length);           //possible error
        }
        catch (Exception ex)
        {
            DisplayAlert("Alert", ex.Message, "Ok");
        }
    }

    private void Label_SizeChanged(object sender, EventArgs e)
    {

    }

    private void labelBatteryCurrent_Focused(object sender, FocusEventArgs e)
    {

    }

    private void imgLED1_Clicked(object sender, EventArgs e)
    {
        ButtonClicked(0);
    }

    private void imgLED2_Clicked(object sender, EventArgs e)
    {
        ButtonClicked(1);
    }
}
