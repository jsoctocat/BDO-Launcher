using System;
using Avalonia.Controls;

namespace Launcher.Views;

public partial class MsgBoxView : UserControl
{
    private Action _closeAction;

    public MsgBoxView()
    {
        InitializeComponent();
    }

    private void Close()
    {
        _closeAction?.Invoke();
    }

    public void SetCloseAction(Action closeAction)
    {
        _closeAction = closeAction;
    }

    public void CloseWindow(object? sender, EventArgs eventArgs)
    {
        this.Close();
    }
}