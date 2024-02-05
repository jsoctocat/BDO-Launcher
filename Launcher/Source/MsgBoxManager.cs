using System;
using Launcher.ViewModels;
using Launcher.Views;

namespace Launcher.Source;

public static class MsgBoxManager
{
    public static MsgBox GetMessageBox(string title, string content,
        bool isOkShowed = false, bool isYesShowed = false, bool isNoShowed = false,
        Action? okAction = null , Action? yesAction = null)
    {
        var msgBoxViewModel = new MsgBoxViewModel
        {
            ContentTitle = title,
            ContentMessage = content,
            IsOkShowed = isOkShowed,
            IsYesShowed = isYesShowed,
            IsNoShowed = isNoShowed,
            CustomOkAction = okAction,
            CustomYesAction = yesAction
        };
        var msgBoxView = new MsgBoxView
        {
            DataContext = msgBoxViewModel
        };

        return new MsgBox(msgBoxView, msgBoxViewModel);
    }
}