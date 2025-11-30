using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FMMS.ViewModels;

/// <summary>
/// Wrapper class for TimeSpan to enable two-way binding with TimePicker.
/// TimePicker requires a property that implements INotifyPropertyChanged for proper two-way binding.
/// </summary>
public class ScheduleTimeViewModel : INotifyPropertyChanged
{
    private TimeSpan _time;

    public TimeSpan Time
    {
        get => _time;
        set
        {
            if (_time != value)
            {
                _time = value;
                OnPropertyChanged();
            }
        }
    }

    public ScheduleTimeViewModel(TimeSpan time)
    {
        _time = time;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}