using Avalonia.Controls;
using Avalonia.Interactivity;

namespace NWN_Timer.Views;

public partial class SaveProfileDialog : Window
{
    public SaveProfileDialog()
    {
        InitializeComponent();
    }

    private void Save_Click(
        object? sender,
        RoutedEventArgs e)
    {
        string? profileName =
            ProfileNameBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(profileName))
            return;

        Close(profileName);
    }

    private void Cancel_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close(null);
    }
}