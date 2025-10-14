# Jimodoro - Modern Pomodoro Timer

A beautiful, modern desktop Pomodoro timer application built with C# and WPF for Windows.

## Features

🍅 **Classic Pomodoro Technique**
- 25-minute work sessions with 5-minute short breaks
- 15-minute long break after 4 completed pomodoros
- Fully configurable timer durations

🎨 **Modern, Expert-Designed UI**
- Clean, minimalist interface with pleasing aesthetics
- Beautiful color schemes that change with timer state
- Smooth progress visualization
- Elegant typography and spacing

🔊 **Smart Notifications**
- Pleasant sound notifications when timers complete
- Visual window flashing to grab attention
- Popup messages for session transitions
- Window activation for visibility

⚙️ **Configurable Settings**
- Adjustable work duration (15-60 minutes)
- Customizable short break (3-15 minutes)
- Configurable long break (10-30 minutes)
- Settings persist during session

📊 **Progress Tracking**
- Visual progress bar with dynamic colors
- Completed pomodoro counter
- Current session status display

## Requirements

- Windows 10 or later
- .NET 8.0 Runtime

## Getting Started

### Building from Source

1. Clone this repository
2. Ensure you have .NET 8.0 SDK installed
3. Navigate to the project directory
4. Build the application:
   ```
   dotnet build
   ```
5. Run the application:
   ```
   dotnet run
   ```

### Creating an Executable

To create a standalone .exe file:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

The executable will be available in `bin/Release/net8.0-windows/win-x64/publish/`

## How to Use

1. **Start a Session**: Click the "Start" button to begin a 25-minute focus session
2. **Take Breaks**: When the timer completes, take the suggested break
3. **Customize Duration**: Expand the Settings section to adjust timer lengths
4. **Track Progress**: Watch your completed pomodoros accumulate

## Timer States

- **🔴 Focus Time**: Work session (default: 25 minutes)
- **🟢 Short Break**: Rest period between work sessions (default: 5 minutes)  
- **🔵 Long Break**: Extended rest after 4 completed pomodoros (default: 15 minutes)

## Project Structure

- `MainWindow.xaml` - Main UI layout and styling
- `MainWindow.xaml.cs` - UI interaction logic
- `PomodoroTimer.cs` - Core timer logic and state management
- `App.xaml` - Application resources and color themes
- `App.xaml.cs` - Application startup logic

## Technology Stack

- **Framework**: .NET 8.0
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Language**: C#
- **Audio**: System.Media.SoundPlayer for notifications
- **Timer**: System.Windows.Threading.DispatcherTimer

## Contributing

Feel free to submit issues and enhancement requests!

## License

This project is open source and available under the [MIT License](LICENSE).