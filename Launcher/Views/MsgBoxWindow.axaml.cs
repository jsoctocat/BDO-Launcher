﻿using Avalonia.Controls;
using Avalonia.Threading;

namespace Launcher.Views;

public partial class MsgBoxWindow : Window
{
    public MsgBoxWindow()
    {
        InitializeComponent();
        ShowInTaskbar = false;
        CanResize = false;
    }

    public async void CloseSafe()
    {
        await Dispatcher.UIThread.InvokeAsync(Close);
    }
}