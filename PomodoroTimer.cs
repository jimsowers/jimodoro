using System;
using System.ComponentModel;
using System.Media;
using System.Windows.Threading;

namespace Jimodoro
{
    public sealed class TimerCompletedEventArgs : EventArgs
    {
        public TimerState CompletedState { get; }
        public TimerState NextState { get; }

        public TimerCompletedEventArgs(TimerState completedState, TimerState nextState)
        {
            CompletedState = completedState;
            NextState = nextState;
        }
    }

    public enum TimerState
    {
        Work,
        ShortBreak,
        LongBreak,
        Stopped
    }

    public class PomodoroTimer : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private TimeSpan _timeRemaining;
        private TimerState _currentState;
        private bool _isRunning;
        private int _completedPomodoros;
        
        // Configurable durations (in minutes)
        public int WorkDuration { get; set; } = 25;
        public int ShortBreakDuration { get; set; } = 5;
        public int LongBreakDuration { get; set; } = 15;
        public int PomodorosUntilLongBreak { get; set; } = 4;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<TimerCompletedEventArgs>? TimerCompleted;

        public PomodoroTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _currentState = TimerState.Stopped;
            _completedPomodoros = 0;
            ResetTimer();
        }

        public TimeSpan TimeRemaining
        {
            get => _timeRemaining;
            private set
            {
                _timeRemaining = value;
                OnPropertyChanged(nameof(TimeRemaining));
                OnPropertyChanged(nameof(DisplayTime));
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        public TimerState CurrentState
        {
            get => _currentState;
            private set
            {
                _currentState = value;
                OnPropertyChanged(nameof(CurrentState));
                OnPropertyChanged(nameof(StateDisplayText));
                OnPropertyChanged(nameof(StateColor));
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(PlayPauseText));
            }
        }

        public int CompletedPomodoros
        {
            get => _completedPomodoros;
            private set
            {
                _completedPomodoros = value;
                OnPropertyChanged(nameof(CompletedPomodoros));
            }
        }

        public string DisplayTime => _timeRemaining.ToString(@"mm\:ss");

        public string StateDisplayText
        {
            get
            {
                return _currentState switch
                {
                    TimerState.Work => "Getting It done!",
                    TimerState.ShortBreak => "Short Break",
                    TimerState.LongBreak => "Long Break",
                    TimerState.Stopped => "Let's Go!",
                    _ => "Unknown"
                };
            }
        }

        public string StateColor
        {
            get
            {
                return _currentState switch
                {
                    TimerState.Work => "#E53E3E",
                    TimerState.ShortBreak => "#38B2AC",
                    TimerState.LongBreak => "#38B2AC",
                    TimerState.Stopped => "#718096",
                    _ => "#718096"
                };
            }
        }

        public string PlayPauseText => _isRunning ? "Pause" : "Start";

        public double ProgressPercentage
        {
            get
            {
                var totalDuration = GetCurrentStateDuration();
                if (totalDuration.TotalSeconds == 0) return 0;
                
                var elapsed = totalDuration - _timeRemaining;
                return (elapsed.TotalSeconds / totalDuration.TotalSeconds) * 100;
            }
        }

        public void StartPause()
        {
            if (_isRunning)
            {
                Pause();
            }
            else
            {
                Start();
            }
        }

        public void Start()
        {
            if (_currentState == TimerState.Stopped)
            {
                StartWorkSession();
            }
            
            IsRunning = true;
            _timer.Start();
        }

        public void Pause()
        {
            IsRunning = false;
            _timer.Stop();
        }

        public void Stop()
        {
            IsRunning = false;
            _timer.Stop();
            CurrentState = TimerState.Stopped;
            ResetTimer();
        }

        public void Skip()
        {
            CompleteCurrentSession();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (TimeRemaining > TimeSpan.Zero)
            {
                TimeRemaining = TimeRemaining.Subtract(TimeSpan.FromSeconds(1));
            }
            else
            {
                CompleteCurrentSession();
            }
        }

        private void CompleteCurrentSession()
        {
            _timer.Stop();
            IsRunning = false;
            var completedState = _currentState;
            
            // Play completion sound
            PlayCompletionSound();
            
            // Transition to next state
            TransitionToNextState();
            var nextState = _currentState;
            
            // Notify completion
            TimerCompleted?.Invoke(this, new TimerCompletedEventArgs(completedState, nextState));
        }

        private void TransitionToNextState()
        {
            switch (_currentState)
            {
                case TimerState.Work:
                    CompletedPomodoros++;
                    if (_completedPomodoros % PomodorosUntilLongBreak == 0)
                    {
                        StartLongBreak();
                    }
                    else
                    {
                        StartShortBreak();
                    }
                    break;
                    
                case TimerState.ShortBreak:
                case TimerState.LongBreak:
                    StartWorkSession();
                    break;
            }
        }

        private void StartWorkSession()
        {
            CurrentState = TimerState.Work;
            TimeRemaining = TimeSpan.FromMinutes(WorkDuration);
        }

        private void StartShortBreak()
        {
            CurrentState = TimerState.ShortBreak;
            TimeRemaining = TimeSpan.FromMinutes(ShortBreakDuration);
        }

        private void StartLongBreak()
        {
            CurrentState = TimerState.LongBreak;
            TimeRemaining = TimeSpan.FromMinutes(LongBreakDuration);
        }

        private void ResetTimer()
        {
            TimeRemaining = TimeSpan.FromMinutes(WorkDuration);
        }

        private TimeSpan GetCurrentStateDuration()
        {
            return _currentState switch
            {
                TimerState.Work => TimeSpan.FromMinutes(WorkDuration),
                TimerState.ShortBreak => TimeSpan.FromMinutes(ShortBreakDuration),
                TimerState.LongBreak => TimeSpan.FromMinutes(LongBreakDuration),
                _ => TimeSpan.FromMinutes(WorkDuration)
            };
        }

        private void PlayCompletionSound()
        {
            try
            {
                // Play multiple system sounds for better notification
                SystemSounds.Asterisk.Play();
                System.Threading.Thread.Sleep(100);
                SystemSounds.Asterisk.Play();
            }
            catch
            {
                // Silently fail if sound cannot be played
                try
                {
                    // Fallback to beep if system sounds fail
                    Console.Beep(800, 200);
                    System.Threading.Thread.Sleep(100);
                    Console.Beep(800, 200);
                }
                catch
                {
                    // Complete fallback - do nothing
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}