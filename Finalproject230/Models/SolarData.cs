using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Finalproject230.Models
{
    [ObservableObject]
    public partial class SolarData
    {
        [ObservableProperty]
        string validPacket;
        [ObservableProperty]
        string openText;
        public double[] analogVoltage = new double[6];
        private static double ResistorValue;
        public SolarData()
        {            ResistorValue = 100;        }
        public void ParseSolarData(string newPacket)
        {
            if (newPacket.Length > 37)
            {
                analogVoltage[0] = Convert.ToDouble(newPacket.Substring(6, 4));
                analogVoltage[1] = Convert.ToDouble(newPacket.Substring(10, 4));
                analogVoltage[2] = Convert.ToDouble(newPacket.Substring(14, 4));
                analogVoltage[3] = Convert.ToDouble(newPacket.Substring(18, 4));
                analogVoltage[4] = Convert.ToDouble(newPacket.Substring(22, 4));
                analogVoltage[5] = Convert.ToDouble(newPacket.Substring(26, 4));
            }
        }
        public double averageVoltage(double voltageToAverage, int indexOfAnalog)
        {
            for (indexOfAnalog = 0; indexOfAnalog < 5; indexOfAnalog++)
            {
                voltageToAverage = 0;
                voltageToAverage = (voltageToAverage + analogVoltage[indexOfAnalog]) / indexOfAnalog;
                voltageToAverage = (voltageToAverage * 3.3) / 3301;
            }
            return voltageToAverage;
        }
        public string GetCurrent(double an1, double an2)
        {   double _an1 = (((an1 - an2) * 3.3) / 3301);
            _an1 = (_an1 * 1000) / ResistorValue;
            return _an1.ToString(" 0.00 mA");        }
        public string GetLedCurrent(double an1, double an2)
        {   double _an1 = ((an1 * 3.3) / 3301) - ((an2 * 3.3) / 3301);
            if (_an1 > 0)
            {   _an1 = (_an1 / ResistorValue) * 1000;
                return _an1.ToString(" 0.00 'mA'");            }
            else
            {   _an1 = 0;
                return _an1.ToString(" 0.00 'mA'");            }
        }
        public string GetVoltage(double analogValue)
        {   double _analogValue = (analogValue * 3.3) / 3301;
            return _analogValue.ToString(" 0.00 V");        }
    }
}
