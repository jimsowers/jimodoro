using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace ThniksTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PomodoroTimer _pomodoroTimer = null!;
        private System.Windows.Threading.DispatcherTimer? _celebrationCountdownTimer;
        private System.Windows.Threading.DispatcherTimer? _confettiTimer;
        private int _celebrationCountdown;
        private Random _random = new Random();

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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // Force immersive dark title bar regardless of system theme
            TryEnableImmersiveDarkTitleBar(forceDark:true);
            TrySetRoundedCorners();
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

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void MaxRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            if (e.ClickCount == 2)
            {
                // toggle maximize/restore
                MaxRestore_Click(sender, new RoutedEventArgs());
                return;
            }

            try
            {
                DragMove();
            }
            catch
            {
                // ignore
            }
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

        private void CelebrationOverlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Allow user to dismiss celebration early by clicking
            _celebrationCountdownTimer?.Stop();
            StopConfetti();
            HideCelebrationAnimation();
            _pomodoroTimer.Start(); // Auto-start next phase
        }

        private void StartConfetti()
        {
            // Clear any existing confetti
            ConfettiCanvas.Children.Clear();

            // Create confetti timer that adds new particles
            _confettiTimer?.Stop();
            _confettiTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            int confettiCount = 0;
            _confettiTimer.Tick += (s, e) =>
            {
                if (confettiCount < 50) // Limit total confetti pieces
                {
                    CreateConfettiPiece();
                    confettiCount++;
                }
                else
                {
                    _confettiTimer?.Stop();
                }
            };
            _confettiTimer.Start();
        }

        private void CreateConfettiPiece()
        {
            // Random colors for confetti
            var colors = new[] 
            { 
                Colors.Orange, 
                Colors.Gold, 
                Colors.LightBlue, 
                Colors.LightGreen, 
                Colors.Pink, 
                Colors.Purple,
                Colors.Red,
                Colors.Yellow
            };

            // Create confetti piece
            var confetti = new System.Windows.Shapes.Rectangle
            {
                Width = _random.Next(8, 15),
                Height = _random.Next(8, 15),
                Fill = new SolidColorBrush(colors[_random.Next(colors.Length)]),
                Opacity = 0.8,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // Random starting position at top
            double startX = _random.NextDouble() * ActualWidth;
            double startY = -20;

            Canvas.SetLeft(confetti, startX);
            Canvas.SetTop(confetti, startY);
            Canvas.SetZIndex(confetti, 1000);

            ConfettiCanvas.Children.Add(confetti);

            // Create falling and rotating animation
            var storyboard = new Storyboard();

            // Fall animation
            var fallAnimation = new DoubleAnimation
            {
                From = startY,
                To = ActualHeight + 20,
                Duration = TimeSpan.FromMilliseconds(_random.Next(2000, 4000)),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(fallAnimation, confetti);
            Storyboard.SetTargetProperty(fallAnimation, new PropertyPath("(Canvas.Top)"));
            storyboard.Children.Add(fallAnimation);

            // Horizontal drift
            var driftAnimation = new DoubleAnimation
            {
                From = startX,
                To = startX + _random.Next(-100, 100),
                Duration = TimeSpan.FromMilliseconds(_random.Next(2000, 4000)),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(driftAnimation, confetti);
            Storyboard.SetTargetProperty(driftAnimation, new PropertyPath("(Canvas.Left)"));
            storyboard.Children.Add(driftAnimation);

            // Rotation
            var rotateTransform = new RotateTransform(0);
            confetti.RenderTransform = rotateTransform;

            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = _random.Next(360, 720) * (_random.Next(2) == 0 ? 1 : -1),
                Duration = TimeSpan.FromMilliseconds(_random.Next(1500, 3000))
            };
            Storyboard.SetTarget(rotateAnimation, rotateTransform);
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath("Angle"));
            storyboard.Children.Add(rotateAnimation);

            // Remove confetti when animation completes
            storyboard.Completed += (s, e) =>
            {
                ConfettiCanvas.Children.Remove(confetti);
            };

            storyboard.Begin();
        }

        private void StopConfetti()
        {
            _confettiTimer?.Stop();
            ConfettiCanvas.Children.Clear();
        }

        private void OnTimerCompleted(object? sender, TimerCompletedEventArgs e)
        {
            // Show celebration animation instead of modal
            ShowCelebrationAnimation(e.CompletedState, e.NextState);
        }

        private void ShowCelebrationAnimation(TimerState completedState, TimerState nextState)
        {
            // Bring window to focus if minimized
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();

            // Set celebration message based on completed state
            var (message, subMessage) = completedState switch
            {
                TimerState.Work => ("Work Session Complete! ??", "Great focus! Time for a well-deserved break."),
                TimerState.ShortBreak => ("Break Complete!", "Feeling refreshed? Let's get back to work!"),
                TimerState.LongBreak => ("Long Break Complete!", "Recharged and ready to conquer more!"),
                _ => ("Timer Complete!", "Ready for the next session?")
            };

            CelebrationMessage.Text = message;
            CelebrationSubMessage.Text = subMessage;

            // Show overlay
            CelebrationOverlay.Visibility = Visibility.Visible;

            // Create storyboard for all animations
            var storyboard = new Storyboard();

            // 1. Fade in overlay
            var fadeInOverlay = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(fadeInOverlay, CelebrationOverlay);
            Storyboard.SetTargetProperty(fadeInOverlay, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeInOverlay);

            // 2. Scale up celebration card
            var scaleUpX = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
            };
            Storyboard.SetTarget(scaleUpX, CelebrationScaleTransform);
            Storyboard.SetTargetProperty(scaleUpX, new PropertyPath("ScaleX"));
            storyboard.Children.Add(scaleUpX);

            var scaleUpY = new DoubleAnimation
            {
                From = 0.8,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.5 }
            };
            Storyboard.SetTarget(scaleUpY, CelebrationScaleTransform);
            Storyboard.SetTargetProperty(scaleUpY, new PropertyPath("ScaleY"));
            storyboard.Children.Add(scaleUpY);

            // 3. Pulse the timer display in background
            var pulseAnimation = new DoubleAnimationUsingKeyFrames();
            pulseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            pulseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.05, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            pulseAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400))));
            pulseAnimation.Duration = TimeSpan.FromMilliseconds(400);
            
            Storyboard.SetTarget(pulseAnimation, TimerScaleTransform);
            Storyboard.SetTargetProperty(pulseAnimation, new PropertyPath("ScaleX"));
            storyboard.Children.Add(pulseAnimation);

            var pulseAnimationY = pulseAnimation.Clone();
            Storyboard.SetTarget(pulseAnimationY, TimerScaleTransform);
            Storyboard.SetTargetProperty(pulseAnimationY, new PropertyPath("ScaleY"));
            storyboard.Children.Add(pulseAnimationY);

            // 4. Enhanced glow effect on timer border
            var glowAnimation = new ColorAnimation
            {
                From = Color.FromRgb(226, 232, 240), // Light gray
                To = completedState == TimerState.Work 
                    ? Color.FromRgb(249, 115, 22)  // Orange for work
                    : Color.FromRgb(16, 185, 129),  // Green for break
                Duration = TimeSpan.FromMilliseconds(600),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(glowAnimation, TimerShadow);
            Storyboard.SetTargetProperty(glowAnimation, new PropertyPath("Color"));
            storyboard.Children.Add(glowAnimation);

            var glowBlurAnimation = new DoubleAnimation
            {
                From = 10,
                To = 30,
                Duration = TimeSpan.FromMilliseconds(600),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(glowBlurAnimation, TimerShadow);
            Storyboard.SetTargetProperty(glowBlurAnimation, new PropertyPath("BlurRadius"));
            storyboard.Children.Add(glowBlurAnimation);

            // Start animations
            storyboard.Begin();

            // Start confetti animation
            StartConfetti();

            // Start countdown timer (15 seconds)
            _celebrationCountdown = 15;
            CountdownText.Text = _celebrationCountdown.ToString();

            _celebrationCountdownTimer?.Stop();
            _celebrationCountdownTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _celebrationCountdownTimer.Tick += (s, e) =>
            {
                _celebrationCountdown--;
                if (_celebrationCountdown > 0)
                {
                    CountdownText.Text = _celebrationCountdown.ToString();
                }
                else
                {
                    _celebrationCountdownTimer?.Stop();
                    HideCelebrationAnimation();
                    _pomodoroTimer.Start(); // Auto-start next phase
                }
            };
            _celebrationCountdownTimer.Start();
        }

        private void HideCelebrationAnimation()
        {
            StopConfetti();
            
            var storyboard = new Storyboard();

            // Fade out overlay
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(fadeOut, CelebrationOverlay);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));

            // Scale down celebration card
            var scaleDownX = new DoubleAnimation
            {
                From = 1.0,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(scaleDownX, CelebrationScaleTransform);
            Storyboard.SetTargetProperty(scaleDownX, new PropertyPath("ScaleX"));

            var scaleDownY = new DoubleAnimation
            {
                From = 1.0,
                To = 0.8,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(scaleDownY, CelebrationScaleTransform);
            Storyboard.SetTargetProperty(scaleDownY, new PropertyPath("ScaleY"));

            storyboard.Children.Add(fadeOut);
            storyboard.Children.Add(scaleDownX);
            storyboard.Children.Add(scaleDownY);

            storyboard.Completed += (s, e) =>
            {
                CelebrationOverlay.Visibility = Visibility.Collapsed;
            };

            storyboard.Begin();
        }

        // Enable Windows 11 dark title bar (immersive dark mode).
        // Uses DWM attributes: value 20 on 1903+, fallback to 19 for 1809.
        // Try to enable immersive dark title bar. If forceDark is true, set the DWM attribute to dark mode
        private void TryEnableImmersiveDarkTitleBar(bool forceDark = false)
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero)
                    return;

                int useDark = forceDark ? 1 : 0;
                // DWMWA_USE_IMMERSIVE_DARK_MODE (20) for modern builds
                DwmSetWindowAttribute(hwnd, 20 /* DWMWA_USE_IMMERSIVE_DARK_MODE */ , ref useDark, sizeof(int));
                // Fallback (19) for older builds
                DwmSetWindowAttribute(hwnd, 19 /* DWMWA_USE_IMMERSIVE_DARK_MODE (legacy) */ , ref useDark, sizeof(int));
            }
            catch
            {
                // Ignore if not supported on this OS
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        // Prefer more rounded window corners on Windows 11 while keeping the native title bar.
        // DWMWA_WINDOW_CORNER_PREFERENCE = 33
        // DWM_WINDOW_CORNER_PREFERENCE: 0=Default, 1=DoNotRound, 2=Round, 3=RoundSmall
        private void TrySetRoundedCorners()
        {
            try
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero)
                    return;

                int round = 2; // Round (more pronounced)
                DwmSetWindowAttribute(hwnd, 33 /* DWMWA_WINDOW_CORNER_PREFERENCE */, ref round, sizeof(int));
            }
            catch
            {
                // Ignore if not supported
            }
        }
    }
}