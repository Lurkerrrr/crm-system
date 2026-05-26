using System.Windows;
using System.Windows.Controls;

namespace CRMSystem.UI.Views.Controls;

public partial class BusyOverlay : UserControl
{
    public BusyOverlay()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(BusyOverlay),
            new PropertyMetadata("Loading..."));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}