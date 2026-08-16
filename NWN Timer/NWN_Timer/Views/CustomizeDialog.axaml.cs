using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using NWN_Timer.ViewModels;

namespace NWN_Timer.Views;

public partial class CustomizeDialog : Window
{
    private readonly MainViewModel _viewModel;


    public CustomizeDialog(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;


        // Current warning settings
        RoundWarningBox.Text =
            _viewModel.RoundTimer.WarningAmount;

        TurnWarningBox.Text =
            _viewModel.TurnTimer.WarningAmount;

        HourWarningBox.Text =
            _viewModel.HourTimer.WarningAmount;

        CustomWarningBox.Text =
            _viewModel.CustomTimer.WarningAmount;


        // All WAV files sitting beside the program.
        IReadOnlyList<string> sounds =
            _viewModel.GetAvailableSounds();

        RoundSoundBox.ItemsSource = sounds;
        TurnSoundBox.ItemsSource = sounds;
        HourSoundBox.ItemsSource = sounds;
        CustomSoundBox.ItemsSource = sounds;


        RoundSoundBox.SelectedItem =
            GetSelectedSound(
                sounds,
                _viewModel.RoundTimer.SoundFile);

        TurnSoundBox.SelectedItem =
            GetSelectedSound(
                sounds,
                _viewModel.TurnTimer.SoundFile);

        HourSoundBox.SelectedItem =
            GetSelectedSound(
                sounds,
                _viewModel.HourTimer.SoundFile);

        CustomSoundBox.SelectedItem =
            GetSelectedSound(
                sounds,
                _viewModel.CustomTimer.SoundFile);
    }


    private static string GetSelectedSound(
        IReadOnlyList<string> sounds,
        string currentSound)
    {
        return sounds.Contains(currentSound)
            ? currentSound
            : "None";
    }


    private void OK_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _viewModel.RoundTimer.WarningAmount =
            RoundWarningBox.Text ?? "0";

        _viewModel.TurnTimer.WarningAmount =
            TurnWarningBox.Text ?? "0";

        _viewModel.HourTimer.WarningAmount =
            HourWarningBox.Text ?? "0";

        _viewModel.CustomTimer.WarningAmount =
            CustomWarningBox.Text ?? "0";


        _viewModel.RoundTimer.SoundFile =
            RoundSoundBox.SelectedItem as string
            ?? "None";

        _viewModel.TurnTimer.SoundFile =
            TurnSoundBox.SelectedItem as string
            ?? "None";

        _viewModel.HourTimer.SoundFile =
            HourSoundBox.SelectedItem as string
            ?? "None";

        _viewModel.CustomTimer.SoundFile =
            CustomSoundBox.SelectedItem as string
            ?? "None";


        Close(true);
    }


    private void Cancel_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close(false);
    }


    private void TestRoundSound_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _viewModel.PlaySound(
            RoundSoundBox.SelectedItem as string);
    }


    private void TestTurnSound_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _viewModel.PlaySound(
            TurnSoundBox.SelectedItem as string);
    }


    private void TestHourSound_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _viewModel.PlaySound(
            HourSoundBox.SelectedItem as string);
    }


    private void TestCustomSound_Click(
        object? sender,
        RoutedEventArgs e)
    {
        _viewModel.PlaySound(
            CustomSoundBox.SelectedItem as string);
    }
}