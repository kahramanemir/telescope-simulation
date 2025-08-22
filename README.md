# Telescope Simulation Project

A Unity-based telescope simulation that provides real-time astronomical observations and control through integration with Python astronomy libraries. This project simulates telescope movement, celestial object tracking, and provides a realistic telescope control interface.

## 🌟 Features

- **Real-time Celestial Tracking**: Track planets, stars, and other celestial objects using accurate astronomical data
- **Telescope Control Simulation**: Realistic azimuth and altitude motor control with stepper motor calculations
- **3D Solar System Visualization**: Interactive 3D representation of planets with accurate positioning
- **Camera Control**: Telescope camera simulation with smooth movement and target tracking
- **Python Integration**: Uses Astropy for astronomical calculations and real-time position updates
- **Network Communication**: UDP/TCP communication between Unity and Python scripts
- **Interactive UI**: Dropdown selection for celestial objects with real-time status updates

## 🏗️ Project Structure

```
TelescopeSimulation/
├── Assets/
│   ├── Scenes/
│   │   └── SampleScene.unity          # Main simulation scene
│   ├── Scripts/
│   │   ├── TelescopeController.cs     # Main telescope control and movement
│   │   ├── TelescopeCameraCont.cs     # Camera positioning and target tracking
│   │   ├── PythonPlanetRunner.cs      # TCP client for planet position updates
│   │   ├── PlanetRotation.cs          # Individual planet rotation simulation
│   │   ├── MiniJSON.cs                # JSON parsing utility
│   │   ├── planet_positions.py        # Python server for celestial positions
│   │   └── step_motor.py              # Python telescope control server
│   ├── Materials/                     # 3D materials for planets and objects
│   ├── Planets of the Solar System 3D/ # 3D models and textures
│   └── Settings/                      # Unity project settings
├── ProjectSettings/                   # Unity project configuration
└── readme.md                         # This file
```

## 🚀 Getting Started

### Prerequisites

#### Unity Requirements
- Unity 2022.3 LTS or later
- TextMeshPro package
- Input System package

#### Python Requirements
- Python 3.7 or later
- Required Python packages:
  ```bash
  pip install astropy numpy
  ```

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd TelescopeSimulation
   ```

2. **Open in Unity**
   - Open Unity Hub
   - Click "Add" and select the project folder
   - Open the project with Unity 2022.3 LTS or later

3. **Install Python Dependencies**
   ```bash
   pip install astropy numpy
   ```

### Running the Simulation

1. **Start Python Servers**
   
   Open two terminal windows and run:
   
   **Terminal 1 - Planet Position Server:**
   ```bash
   cd Assets/Scripts
   python planet_positions.py
   ```
   
   **Terminal 2 - Telescope Control Server:**
   ```bash
   cd Assets/Scripts
   python step_motor.py
   ```

2. **Run Unity Scene**
   - Open `Assets/Scenes/SampleScene.unity`
   - Press Play button in Unity Editor
   - Use the dropdown menu to select celestial objects

## 🎮 How to Use

1. **Object Selection**: Use the dropdown menu in the Unity interface to select a celestial object (Sun, Mercury, Venus, Earth, Moon, Mars, Jupiter, Saturn, Uranus, Neptune, or stars like Polaris, Antares, Capella, Spica)

2. **Telescope Movement**: The telescope will automatically calculate and move to the selected target's azimuth and altitude coordinates

3. **Camera View**: The telescope camera will smoothly move to provide an optimal viewing angle of the selected object

4. **Status Monitoring**: Real-time status display shows:
   - Current azimuth and altitude angles
   - Target coordinates
   - Required stepper motor steps and turns
   - Error messages for objects below horizon

## 🔧 System Architecture

### Unity Components

#### TelescopeController.cs
- Receives azimuth/altitude coordinates via UDP (port 5006)
- Controls telescope 3D model rotation with realistic stepper motor simulation
- Calculates required motor steps and gear ratios
- Displays real-time status information

#### TelescopeCameraCont.cs
- Manages telescope camera positioning and movement
- Receives target selection via UDP (port 5008)
- Sends target names to Python server (port 5005)
- Provides smooth camera transitions and target tracking

#### PythonPlanetRunner.cs (PlanetSocketUpdater)
- TCP client connecting to planet position server (port 65432)
- Updates 3D positions of all celestial objects in real-time
- Handles JSON parsing of position data

#### PlanetRotation.cs
- Simulates individual planet rotations
- Configurable rotation periods and directions
- Supports retrograde rotation for appropriate planets

### Python Components

#### planet_positions.py
- TCP server providing real-time celestial object positions
- Uses Astropy library for accurate astronomical calculations
- Includes solar system bodies and fixed stars
- Updates positions based on current time with configurable time steps

#### step_motor.py
- UDP server for telescope control calculations
- Converts celestial coordinates to telescope azimuth/altitude
- Uses Astropy for coordinate transformations
- Location-specific calculations (configured for Ankara, Turkey)
- Handles object visibility and horizon limitations

### Network Communication

| Component | Protocol | Port | Purpose |
|-----------|----------|------|---------|
| Planet Positions | TCP | 65432 | Stream celestial object positions |
| Telescope Control In | UDP | 5006 | Receive az/alt coordinates |
| Telescope Control Out | UDP | 5007 | Send az/alt coordinates |
| Camera Control | UDP | 5005/5008 | Target selection communication |

## ⚙️ Configuration

### Location Settings
The telescope is configured for Ankara, Turkey by default. To change location, modify these values in `step_motor.py`:

```python
latitude = 39.9334   # Ankara latitude
longitude = 32.8597  # Ankara longitude
elevation = 890      # Elevation in meters
```

### Network Settings
All network ports can be configured in the respective script files:

- `TelescopeController.cs`: `receivePort = 5006`
- `TelescopeCameraCont.cs`: `receivePort = 5008`, `cameraSendPort = 5005`
- `PythonPlanetRunner.cs`: `port = 65432`

### Simulation Parameters
Adjust simulation speed and accuracy in the Python scripts:

```python
# In planet_positions.py
TIME_STEP_SECONDS = 3600 * 0.005  # 18 seconds per update (5 hours simulation per real hour)
UPDATE_INTERVAL = 0.01            # 100 Hz update rate

# In step_motor.py
UPDATE_INTERVAL = 0.5             # 2 Hz update rate for telescope control
```

## 🎯 Supported Celestial Objects

### Solar System Bodies
- Sun
- Mercury, Venus, Earth, Mars
- Moon
- Jupiter, Saturn, Uranus, Neptune

### Stars
- Polaris (North Star)
- Antares (Alpha Scorpii)
- Capella (Alpha Aurigae)
- Spica (Alpha Virginis)

## 🔍 Troubleshooting

### Common Issues

1. **Python servers not connecting**
   - Ensure Python servers are running before starting Unity
   - Check firewall settings for UDP/TCP ports
   - Verify Python dependencies are installed

2. **Objects not visible**
   - Check if object is above horizon for your location
   - Error messages will display for objects below horizon
   - Some objects may not be visible at certain times

3. **Telescope not moving**
   - Verify UDP communication is working
   - Check console for network errors
   - Ensure telescope GameObjects are properly assigned in Unity

4. **Performance issues**
   - Adjust `UPDATE_INTERVAL` in Python scripts
   - Reduce `TIME_STEP_SECONDS` for more accurate but slower simulation

## 🚧 Development Notes

### Coordinate Systems
- Unity uses left-handed coordinate system
- Astronomical coordinates are converted appropriately
- Azimuth: 0° = North, 90° = East, 180° = South, 270° = West
- Altitude: 0° = Horizon, 90° = Zenith

### Stepper Motor Simulation
The telescope control includes realistic stepper motor calculations:
- 200 steps per revolution (typical stepper motor)
- 10:1 gear ratio
- 16x microstepping
- Total: 32,000 steps per full rotation

## 📄 License

This project is licensed under the MIT License.

**Authors:**
- Emir Kahraman
- Ramazan Sefa Kurtuluş

```
MIT License

Copyright (c) 2025 Emir Kahraman, Ramazan Sefa Kurtuluş

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

**Created with Unity 2022.3 LTS and Python 3.x**