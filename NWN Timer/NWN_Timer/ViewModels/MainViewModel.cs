using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NWN_Timer.Models;
using NWN_Timer.Services;

namespace NWN_Timer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ProfileService _profileService = new();
    private readonly AudioService _audioService = new();


    // -------------------------
    // TIMERS
    // -------------------------

    public CountdownTimerViewModel RoundTimer { get; } =
        new("Rounds", TimerUnit.Rounds, "25");

    public CountdownTimerViewModel TurnTimer { get; } =
        new("Turns", TimerUnit.Turns, "25");

    public CountdownTimerViewModel HourTimer { get; } =
        new("Hours", TimerUnit.Hours, "5");

    public CountdownTimerViewModel CustomTimer { get; } =
        new("Custom", TimerUnit.Turns, "10");


    // -------------------------
    // PROFILES
    // -------------------------

    public ObservableCollection<string> Profiles { get; } =
        new();


    [ObservableProperty]
    private string? selectedProfile;
    partial void OnSelectedProfileChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        LoadSelectedProfile();
    }

    public MainViewModel()
    {
        RoundTimer.WarningSoundRequested += PlaySound;
        TurnTimer.WarningSoundRequested += PlaySound;
        HourTimer.WarningSoundRequested += PlaySound;
        CustomTimer.WarningSoundRequested += PlaySound;

        // If this is the first launch and no Default exists,
        // create one using the built-in starting values.
        if (!_profileService.Exists("Default"))
        {
            _profileService.Save(
                "Default",
                CreateProfileFromCurrentSettings());
        }

        RefreshProfiles();

        // Default always loads when the program starts.
        SelectedProfile = "Default";

    }
    public IReadOnlyList<string> GetAvailableSounds()
    {
        return _audioService.GetAvailableSoundFiles();
    }

    public void PlaySound(string? soundFile)
    {
        _audioService.Play(soundFile);
    }


    // =========================
    // TIMER COMMANDS
    // =========================

    [RelayCommand]
    private void StartAll()
    {
        RoundTimer.StartTimer();
        TurnTimer.StartTimer();
        HourTimer.StartTimer();
        CustomTimer.StartTimer();
    }


    [RelayCommand]
    private void StopAndResetAll()
    {
        RoundTimer.StopAndResetTimer();
        TurnTimer.StopAndResetTimer();
        HourTimer.StopAndResetTimer();
        CustomTimer.StopAndResetTimer();
    }


    // =========================
    // PROFILE COMMANDS
    // =========================

   private void LoadSelectedProfile()
    {
        if (string.IsNullOrWhiteSpace(SelectedProfile))
            return;

        TimerProfile? profile =
            _profileService.Load(SelectedProfile);

        if (profile == null)
            return;

        ApplyProfile(profile);
    }


    [RelayCommand]
    public void SaveSelectedProfile()
    {
        if (string.IsNullOrWhiteSpace(SelectedProfile))
            return;

        string savedName =
            _profileService.Save(
                SelectedProfile,
                CreateProfileFromCurrentSettings());

        RefreshProfiles();

        SelectedProfile = savedName;
    }


    [RelayCommand]
    public void SaveAsNewProfile(string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
            return;

        string savedName =
            _profileService.Save(
                profileName,
                CreateProfileFromCurrentSettings());

        RefreshProfiles();

        SelectedProfile = savedName;
    }


    // =========================
    // PROFILE DATA
    // =========================

    private TimerProfile CreateProfileFromCurrentSettings()
    {
        return new TimerProfile
        {
            RoundAmount = RoundTimer.Amount,
            RoundWarning = RoundTimer.WarningAmount,
            RoundSound = RoundTimer.SoundFile,

            TurnAmount = TurnTimer.Amount,
            TurnWarning = TurnTimer.WarningAmount,
            TurnSound = TurnTimer.SoundFile,

            HourAmount = HourTimer.Amount,
            HourWarning = HourTimer.WarningAmount,
            HourSound = HourTimer.SoundFile,

            CustomAmount = CustomTimer.Amount,
            CustomWarning = CustomTimer.WarningAmount,
            CustomSound = CustomTimer.SoundFile,

            CustomUnit =
                CustomTimer.SelectedUnit.ToString()
        };
    }


    private void ApplyProfile(TimerProfile profile)
    {
        // A character/profile change should not leave
        // timers from the previous character running.
        StopAndResetAll();


        RoundTimer.Amount =
            profile.RoundAmount;

        RoundTimer.WarningAmount =
            profile.RoundWarning;

        RoundTimer.SoundFile =
            profile.RoundSound;


        TurnTimer.Amount =
            profile.TurnAmount;

        TurnTimer.WarningAmount =
            profile.TurnWarning;
        TurnTimer.SoundFile =
            profile.TurnSound;

        HourTimer.Amount =
            profile.HourAmount;

        HourTimer.WarningAmount =
            profile.HourWarning;
        HourTimer.SoundFile =
            profile.HourSound;

        CustomTimer.Amount =
            profile.CustomAmount;

        CustomTimer.WarningAmount =
            profile.CustomWarning;
        CustomTimer.SoundFile =
            profile.CustomSound;


        TimerUnit? customUnit = profile.CustomUnit switch
        {
            "R" => TimerUnit.Rounds,
            "T" => TimerUnit.Turns,
            "H" => TimerUnit.Hours,

            _ => Enum.TryParse<TimerUnit>(
                profile.CustomUnit,
                true,
                out TimerUnit parsedUnit)
                    ? parsedUnit
                    : null
        };

        if (customUnit.HasValue)
        {
            CustomTimer.SelectedUnit = customUnit.Value;
        }


        // Refresh the displayed countdown values
        // using the newly-loaded profile.
        RoundTimer.ResetTimer();
        TurnTimer.ResetTimer();
        HourTimer.ResetTimer();
        CustomTimer.ResetTimer();
    }


    private void RefreshProfiles()
    {
        string? oldSelection =
            SelectedProfile;

        Profiles.Clear();

        foreach (string profileName
                 in _profileService.GetProfileNames())
        {
            Profiles.Add(profileName);
        }

        if (oldSelection != null &&
            Profiles.Contains(oldSelection))
        {
            SelectedProfile = oldSelection;
        }
    }
}