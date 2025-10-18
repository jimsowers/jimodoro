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

        private void WorkDurationTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_pomodoroTimer != null && sender is System.Windows.Controls.TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value) && value > 0 && value <= 120)
                {
                    _pomodoroTimer.WorkDuration = value;
                    textBox.Background = new SolidColorBrush(Colors.White);
                }
                else if (!string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242)); // Light red for invalid input
                }
            }
        }

        private void ShortBreakTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_pomodoroTimer != null && sender is System.Windows.Controls.TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value) && value > 0 && value <= 60)
                {
                    _pomodoroTimer.ShortBreakDuration = value;
                    textBox.Background = new SolidColorBrush(Colors.White);
                }
                else if (!string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242)); // Light red for invalid input
                }
            }
        }

        private void LongBreakTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_pomodoroTimer != null && sender is System.Windows.Controls.TextBox textBox)
            {
                if (int.TryParse(textBox.Text, out int value) && value > 0 && value <= 120)
                {
                    _pomodoroTimer.LongBreakDuration = value;
                    textBox.Background = new SolidColorBrush(Colors.White);
                }
                else if (!string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242)); // Light red for invalid input
                }
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
            Background = new SolidColorBrush(Color.FromRgb(249, 115, 22)); // AccentColor (Orange)
            
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