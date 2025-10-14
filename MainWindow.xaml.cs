using System;
using System.Windows;
using System.Windows.Media;

namespace Jimodoro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PomodoroTimer _pomodoroTimer = null!;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                _pomodoroTimer = new PomodoroTimer();
                DataContext = _pomodoroTimer;
                
                // Subscribe to timer completion event
                _pomodoroTimer.TimerCompleted += OnTimerCompleted;
                
                // Initialize sliders with current values after the window is loaded
                Loaded += MainWindow_Loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Slider values will be set from XAML defaults
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            _pomodoroTimer.StartPause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _pomodoroTimer.Stop();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            _pomodoroTimer.Skip();
        }

        private void WorkDurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_pomodoroTimer != null)
            {
                _pomodoroTimer.WorkDuration = (int)e.NewValue;
            }
        }

        private void ShortBreakSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_pomodoroTimer != null)
            {
                _pomodoroTimer.ShortBreakDuration = (int)e.NewValue;
            }
        }

        private void LongBreakSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_pomodoroTimer != null)
            {
                _pomodoroTimer.LongBreakDuration = (int)e.NewValue;
            }
        }

        private void OnTimerCompleted(object? sender, EventArgs e)
        {
            // Flash the window to get user's attention
            FlashWindow();
            
            // Show notification message
            var state = _pomodoroTimer.CurrentState;
            var message = state switch
            {
                TimerState.Work => "Focus session completed! Time for a break. 🎉",
                TimerState.ShortBreak => "Break time over! Ready to focus again? 💪",
                TimerState.LongBreak => "Long break finished! Let's get back to work! 🚀",
                _ => "Timer completed!"
            };
            
            MessageBox.Show(message, "Jimodoro", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void FlashWindow()
        {
            // Change window background color briefly to create a flash effect
            var originalBrush = Background;
            
            // Flash with accent color
            Background = new SolidColorBrush(Color.FromRgb(72, 187, 120)); // AccentColor
            
            // Create a timer to restore original background
            var flashTimer = new System.Windows.Threading.DispatcherTimer();
            flashTimer.Interval = TimeSpan.FromMilliseconds(200);
            flashTimer.Tick += (s, e) =>
            {
                Background = originalBrush;
                flashTimer.Stop();
            };
            flashTimer.Start();
            
            // Also bring window to front if minimized
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            
            Activate();
            Topmost = true;
            Topmost = false;
        }
    }
}