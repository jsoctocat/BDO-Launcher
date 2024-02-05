using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Launcher.ViewModels;
using Launcher.Views;

namespace Launcher.Source;

public class MsgBox(MsgBoxView msgBoxView, MsgBoxViewModel msgBoxViewModel) : UserControl
{
    public Task ShowAsync()
    {
        if (Application.Current != null &&
            Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
            return ShowWindowAsync(tcs);
        }

        throw new NotSupportedException("ApplicationLifetime is not supported");
    }
    
    private async Task ShowWindowAsync(TaskCompletionSource<bool> tcs)
    {
        var window = new MsgBoxWindow
        {
            Content = msgBoxView,
            DataContext = msgBoxViewModel
        };
        window.Closed += msgBoxView.CloseWindow;

        msgBoxViewModel.CustomNoAction = () => {
            window.Close();
        };
        msgBoxView.SetCloseAction(() =>
        {
            window.Close();
            tcs?.TrySetResult(true);
        });

        window.Show();
        await tcs.Task;
    }
}