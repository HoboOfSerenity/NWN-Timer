using Avalonia.Controls;
using Avalonia.Interactivity;
using NWN_Timer.ViewModels;

namespace NWN_Timer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SaveAsButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        SaveProfileDialog dialog =
            new SaveProfileDialog();

        string? profileName =
            await dialog.ShowDialog<string?>(this);

        if (string.IsNullOrWhiteSpace(profileName))
            return;

        if (DataContext is MainViewModel viewModel)
        {
            viewModel.SaveAsNewProfile(profileName);
        }
    }
    private async void CustomizeButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        CustomizeDialog dialog =
            new CustomizeDialog(viewModel);

        bool accepted =
            await dialog.ShowDialog<bool>(this);

        if (accepted)
        {
            viewModel.SaveSelectedProfile();
        }
    }
}