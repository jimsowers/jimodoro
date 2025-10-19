using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Jimodoro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PomodoroTimer _pomodoroTimer = null!;

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

        // Prevent non-numeric characters from being entered
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Validate paste operations to ensure only valid numeric content is pasted
        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pastedText = (string)e.DataObject.GetData(typeof(string));
                
                // Check if the pasted text contains only digits
                if (!Regex.IsMatch(pastedText, @"^\d+$"))
                {
                    e.CancelCommand();
                    return;
                }
                
                // Check if the pasted number is within valid range
                if (int.TryParse(pastedText, out int value))
                {
                    if (value < 1 || value > 120)
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // Validate and ensure values are within safe range (1-120)
        private bool ValidateAndApplyValue(TextBox textBox, Action<int> setValue)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                // Empty field - set light red background
                textBox.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242));
                return false;
            }

            if (int.TryParse(textBox.Text, out int value))
            {
                if (value >= 1 && value <= 120)
                {
                    // Valid value - apply it and set white background
                    setValue(value);
                    textBox.Background = new SolidColorBrush(Colors.White);
                    return true;
                }
                else if (value < 1)
                {
                    // Value too small - correct it to 1
                    textBox.Text = "1";
                    return false; // Will trigger another TextChanged event
                }
                else // value > 120
                {
                    // Value too large - correct it to 120
                    textBox.Text = "120";
                    return false; // Will trigger another TextChanged event
                }
            }
            else
            {
                // Invalid number format - set light red background
                textBox.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242));
                return false;
            }
        }

        // Restore default value if field is empty when focus is lost
        private void RestoreDefaultIfEmpty(TextBox textBox, string defaultValue)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = defaultValue;
                textBox.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void WorkDurationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_pomodoroTimer != null && sender is TextBox textBox)
            {
                ValidateAndApplyValue(textBox, value => _pomodoroTimer.WorkDuration = value);
            }
        }

        private void ShortBreakTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_pomodoroTimer != null && sender is TextBox textBox)
            {
                ValidateAndApplyValue(textBox, value => _pomodoroTimer.ShortBreakDuration = value);
            }
        }

        private void LongBreakTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_pomodoroTimer != null && sender is TextBox textBox)
            {
                ValidateAndApplyValue(textBox, value => _pomodoroTimer.LongBreakDuration = value);
            }
        }

        private void WorkDurationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                RestoreDefaultIfEmpty(textBox, "25");
            }
        }

        private void ShortBreakTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                RestoreDefaultIfEmpty(textBox, "5");
            }
        }

        private void LongBreakTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                RestoreDefaultIfEmpty(textBox, "15");
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