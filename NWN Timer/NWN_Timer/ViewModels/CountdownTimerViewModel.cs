using System;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NWN_Timer.ViewModels;

public enum TimerUnit
{
    Rounds,
    Turns,
    Hours
}

public partial class CountdownTimerViewModel : ViewModelBase
{
    private readonly DispatcherTimer _timer;
    private DateTimeOffset _endTime;
    private bool _warningTriggered;
    private int _flashPhase;

    public string Name { get; }

    public Array UnitOptions { get; } = Enum.GetValues<TimerUnit>();

    [ObservableProperty]
    private string amount;

    [ObservableProperty]
    private string soundFile = "None";

    [ObservableProperty]
    private string warningAmount = "2";

    [ObservableProperty]
    private string timeRemaining = "00:00";

    [ObservableProperty]
    private TimerUnit selectedUnit;

    [ObservableProperty]
    private bool isWarningActive = false;

    [ObservableProperty]
    private double countdownOpacity = 1.0;

    [ObservableProperty]
    private bool isCountdownRed = false;

    public event Action<string>? WarningSoundRequested;
    public string UnitLabel => SelectedUnit switch
    {
        TimerUnit.Rounds => "rounds",
        TimerUnit.Turns => "turns",
        TimerUnit.Hours => "hours",
        _ => ""
    };

    private int SecondsPerUnit => SelectedUnit switch
    {
        TimerUnit.Rounds => 6,
        TimerUnit.Turns => 60,
        TimerUnit.Hours => 120,
        _ => 1
    };

    public CountdownTimerViewModel(
        string name,
        TimerUnit unit,
        string defaultAmount)
    {
        Name = name;
        amount = defaultAmount;
        selectedUnit = unit;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };

        _timer.Tick += Timer_Tick;

        SetDisplayFromAmount();
    }

    partial void OnSelectedUnitChanged(TimerUnit value)
    {
        OnPropertyChanged(nameof(UnitLabel));
        ResetTimer();
    }

    [RelayCommand]
    public void StopAndResetTimer()
    {
        _timer.Stop();
        ClearWarning();
        SetDisplayFromAmount();
    }

    [RelayCommand]
    public void StartTimer()
    {
        if (!int.TryParse(Amount, out int timerAmount))
            return;

        if (timerAmount <= 0)
            return;

        ClearWarning();

        int seconds = timerAmount * SecondsPerUnit;

        _endTime = DateTimeOffset.UtcNow.AddSeconds(seconds);

        UpdateDisplay();
        _timer.Start();
    }

    [RelayCommand]
    public void StopTimer()
    {
        _timer.Stop();
        ClearWarning();
    }

    [RelayCommand]
    public void ResetTimer()
    {
        _timer.Stop();
        ClearWarning();
        SetDisplayFromAmount();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        TimeSpan remaining = _endTime - DateTimeOffset.UtcNow;

        if (remaining <= TimeSpan.Zero)
        {
            TimeRemaining = "00:00";
            _timer.Stop();
            ClearWarning();
            return;
        }

        int secondsRemaining =
            (int)Math.Ceiling(remaining.TotalSeconds);

        SetDisplay(secondsRemaining);
        CheckWarning(secondsRemaining);
        UpdateFlashState();
    }

    private void CheckWarning(int secondsRemaining)
    {
        if (_warningTriggered)
            return;

        if (!int.TryParse(WarningAmount, out int warningUnits))
            return;

        // 0 or less disables warning
        if (warningUnits <= 0)
            return;

        int warningSeconds = warningUnits * SecondsPerUnit;

        if (secondsRemaining <= warningSeconds)
        {
            _warningTriggered = true;
            IsWarningActive = true;
            CountdownOpacity = 1.0;
            IsCountdownRed = false;
            _flashPhase = 0;

            if (!string.Equals(
                SoundFile,
                "None",
                StringComparison.OrdinalIgnoreCase))
            {
                WarningSoundRequested?.Invoke(SoundFile);
            }
        }
    }

    private void UpdateFlashState()
    {
        if (!IsWarningActive)
        {
            CountdownOpacity = 1.0;
            IsCountdownRed = false;
            return;
        }

        _flashPhase++;

        if (_flashPhase > 2)
            _flashPhase = 0;

        switch (_flashPhase)
        {
            // Normal
            case 0:
                CountdownOpacity = 1.0;
                IsCountdownRed = false;
                break;

            // Red
            case 1:
                CountdownOpacity = 1.0;
                IsCountdownRed = true;
                break;

            // Invisible
            case 2:
                CountdownOpacity = 0.0;
                IsCountdownRed = false;
                break;
        }
    }

    private void ClearWarning()
    {
        _warningTriggered = false;
        IsWarningActive = false;
        CountdownOpacity = 1.0;
        IsCountdownRed = false;
        _flashPhase = 0;
    }

    private void SetDisplayFromAmount()
    {
        if (!int.TryParse(Amount, out int timerAmount) ||
            timerAmount <= 0)
        {
            TimeRemaining = "00:00";
            return;
        }

        SetDisplay(timerAmount * SecondsPerUnit);
    }

    private void SetDisplay(int totalSeconds)
    {
        TimeSpan displayTime =
            TimeSpan.FromSeconds(totalSeconds);

        TimeRemaining =
            $"{(int)displayTime.TotalMinutes:00}:{displayTime.Seconds:00}";
    }
}