﻿namespace Finalproject230;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(View.MainPage), typeof(View.MainPage));
    }

    private void btnOpenClose_Clicked(object sender, EventArgs e)
    {

    }
}