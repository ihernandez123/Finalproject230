//using Android.Graphics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Finalproject230.Models
{
    public class LineDrawable : BaseGraphData, IDrawable
    {
        private const int numberOfGraphs = 7;
        public BaseGraphData[] lineGraphs = new BaseGraphData[numberOfGraphs];
        public LineDrawable(): base()
        {
            lineGraphs[0] = new BaseGraphData
                (Yaxis: 0,
                Yaxis1: 0,
                Xaxis: 0,
                lineColor: Colors.Red,
                lineSize: 1,
                newGraph: true);
            lineGraphs[1] = new BaseGraphData
                (Yaxis: 0,
                Yaxis1: 0,
                Xaxis: 0,
                lineColor: Colors.Blue,
                lineSize: 1,
                newGraph: true);
            lineGraphs[2] = new BaseGraphData
                (Yaxis: 0,
                Yaxis1: 0,
                Xaxis: 0,
                lineColor: Colors.Green,
                lineSize: 1,
                newGraph: true);
            lineGraphs[3] = new BaseGraphData
                (Yaxis: 0,
                Yaxis1: 0,
                Xaxis: 0,
                lineColor: Colors.DeepSkyBlue,
                lineSize: 1,
                newGraph: true);
            lineGraphs[4] = new BaseGraphData
                (Yaxis: 0,
                Yaxis1: 0,
                Xaxis: 0,
                lineColor: Colors.Purple,
                lineSize: 1,
                newGraph: true);
            lineGraphs[5] = new BaseGraphData
                (Yaxis: 0,
                Yaxis1: 0,
                Xaxis: 0,
                lineColor: Colors.White,
                lineSize: 1,
                newGraph: true);
            lineGraphs[6] = new BaseGraphData
                (Yaxis: 0,
                Yaxis1: 350,
                Xaxis: 0,
                lineColor: Colors.Black,
                lineSize: 1,
                newGraph: true);
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            Random random = new Random();

            for(int graphIndex=0; graphIndex<lineGraphs.Length; graphIndex++)
            {
                Rect lineGraphRect = new(dirtyRect.X, dirtyRect.Y,dirtyRect.Width, dirtyRect.Height);
                DrawLineGraph(canvas, lineGraphRect, lineGraphs[graphIndex]);
                //DrawBarGraph(canvas, lineGraphRect, lineGraphs[graphIndex],graphIndex);
            }
        }

        private void DrawBarGraph(ICanvas canvas, Rect lineGraphRect, BaseGraphData barGraph, int graphNumber)
        {
            int barWidth = 10;
            int lineGraphWidth = 0;
            int barGraphLocation = lineGraphWidth + barWidth / 2 + graphNumber * barWidth;
            int graphHeight = 400;
            canvas.StrokeSize = barWidth;
            canvas.DrawLine(barGraphLocation, graphHeight, barGraphLocation, barGraph.Yaxis);
        }

        private void DrawLineGraph(ICanvas canvas, RectF dirtyRect, BaseGraphData lineGraph)
        {
            if(lineGraph.Xaxis < 2)
            {
                lineGraph.pointArray[lineGraph.Xaxis, 0] = lineGraph.Xaxis;
                lineGraph.pointArray[lineGraph.Xaxis, 1] = lineGraph.Yaxis;
                lineGraph.Xaxis++;
                return;
            }
            else if (lineGraph.Xaxis < 1000)
            {
                lineGraph.pointArray[lineGraph.Xaxis, 0] = lineGraph.Xaxis;
                lineGraph.pointArray[lineGraph.Xaxis, 1] = lineGraph.Yaxis;
                lineGraph.Xaxis++;
            }
            else
            {
                lineGraph.pointArray[999, 1] = lineGraph.Yaxis;
                for (int i=0; i<999; i++)
                {
                    lineGraph.pointArray[i, 1] = lineGraph.pointArray[i + 1, 1];
                }
            }
            for (int i = 0; i < lineGraph.Xaxis - 1; i++)
            {
                canvas.StrokeColor = lineGraph.lineColor;
                canvas.StrokeSize = lineGraph.lineSize;
                canvas.DrawLine(lineGraph.pointArray[i, 0], lineGraph.pointArray[i, 1],
                    lineGraph.pointArray[i+1, 0], lineGraph.pointArray[i+1, 1]);
            }
        }
    }
}
