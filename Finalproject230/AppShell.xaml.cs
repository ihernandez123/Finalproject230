//using Android.Graphics;
//using Android.Hardware.Camera2.Params;
using Finalproject230.View;

namespace Finalproject230;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();      // Shell running
        Routing.RegisterRoute(nameof(View.RawDataView), typeof(View.RawDataView));     // The first page "Raw Data View"
    }
    private void Label_SizeChanged(object sender, EventArgs e)      // not used in this project
    {
    }

    private void Shell_SizeChanged(object sender, EventArgs e)      // not used in this project
    {
    }
}