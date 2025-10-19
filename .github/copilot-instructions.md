# ThniksTimer - C# WPF Pomodoro Timer Application

This is a modern desktop Pomodoro timer application built with C# and WPF for Windows.

## Project Structure
- **MainWindow.xaml** - Main UI window with modern design
- **MainWindow.xaml.cs** - Code-behind for UI interactions  
- **PomodoroTimer.cs** - Core timer logic and state management
- **App.xaml** - Application resources and styling
- **App.xaml.cs** - Application startup logic
- **ThniksTimer.csproj** - Project configuration

## Features
- 25-minute work timer with 5-minute break timer (configurable)
- Modern, expert-designed UI with pleasant aesthetics
- Sound notifications when timers complete
- Desktop application that compiles to .exe

## Development Notes
- Target Framework: .NET 6.0+ (Windows)
- UI Framework: WPF with modern styling
- Audio: System.Media.SoundPlayer for notifications
- Timer: System.Windows.Threading.DispatcherTimer for UI-safe updates