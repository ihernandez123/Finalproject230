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
        public SolarData()
        {

        }
        public string getValidPacket(string newPacket)
        {
            return newPacket;
        }
    }
}
