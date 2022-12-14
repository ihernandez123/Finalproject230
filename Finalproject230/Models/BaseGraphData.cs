using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Finalproject230.Models
{
    public class BaseGraphData          // this class of the project is required for establish the x and y values in the graph
    {
        public int Yaxis { get; set; } = 0;
        public int Yaxis1 { get; set; } = 0;
        public int Xaxis { get; set; } = 0;
        public int[,] pointArray { get; set; }
        public Color lineColor { get; set; }
        public int lineSize { get; set; }
        public bool newGraph { get; set; } = true;
        //default constructor
        public BaseGraphData()
        {                   }
        //constructor
        public BaseGraphData(
            int Yaxis,
            int Yaxis1,
            int Xaxis,
            Color lineColor,
            int lineSize,
            bool newGraph)
        {
            this.Yaxis = Yaxis;
            this.Yaxis = Yaxis1;
            this.Xaxis = Xaxis;
            this.pointArray = new int[1000, 2];
            this.lineColor = lineColor;
            this.lineSize = lineSize;
            this.newGraph = newGraph;
        }
    }
}
