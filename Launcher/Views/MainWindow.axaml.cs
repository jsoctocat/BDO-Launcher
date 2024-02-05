using System;
using System.ComponentModel;
using Avalonia.Controls;

namespace Launcher.Views;

public partial class MainWindow : Window
{
    public static MainWindow Instance;
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
    }
}