# VcEngine-Automation
[![Build status](https://travis-ci.org/redsolo/vcengine-automation.svg?branch=develop)](https://travis-ci.org/redsolo/vcengine-automation),  [![NuGet VcEngine](http://flauschig.ch/nubadge.php?id=VcEngine.Automation)](https://www.nuget.org/packages/VcEngine.Automation/) 

# VcEngine-Automation Library
Automation library for Visual Components engine in C#. With the library it is possible to control the engine through a C# application instead of manual using in the through the UI. 

Currently the library can:
 - Load/Save layouts
 - Start/Pause/Reset simulation
 - Record a simulation to a video or [Experience](http://www.visualcomponents.com/insights/blog/introducing-visual-components-experience/) file 
 - Changing properties on selected components
 - Translating or rotating selected components
 - Interact with the camera such as Fill all or Fill on selected components
 - Copy text from the Output panel
 - Load components through VCID
 - Click on any action in the ribbon or File menu
 - Interact with the action panel

*Notes*
 - The library cannot interact with the components in the 3D world as the 3D area is a black box for the automation API.
 - The main motivation behind the library is to automate testing of the FlexLink Design Tool, but the library is not limited to only using the tool

# VcEngine-Runner
Console application that can control the Visual Components simulation tool to automate certain long term running tasks. One possibility is to run several simulations after each other and not having to monitor the result. 

Currently the application can:
 - Run a simulation simulation for a specified duration *run-simulation* 
 - Record a simulation to a video  or the VC Experience format *record-simulation*
 
## Run Simulation
This command runs a simulation for a duration at maximum speed and saves the output in the Output panel into a file. If the simulation is stopped/paused before the simulation has run for the full duration, it will re-run the simulation and record it to a video file.

After the simulation has run, it will create a file with the same name as the layout but with suffix *.txt*.
 - If the simulation ran for the full duration, it will contain *Success* and the text in the output panel
 - If the simulation did not run for the full duration, ie paused for some reason, it will contain *Failure* and the text in the output panel

The following command will load the **MyLayout.vcmx** layout and run the simulation for 12 hours at the maximum speed.
```
VcEngineRunner run-simulation -f MyLayout.vcmx --duration 12:0:0 --speed 100
```
## Record Simulation
This command records a simulation to a Video or Experience file.

The following command will load *robot-palletizing.vcmx* and record the simulation for 10 seconds into the *robot.avi* file.
```
VcEngineRunner record-simulation -f robot-palletizing.vcmx --duration 00:00:10 -o robot.avi -t Video
```
[![Recording simulation to video file](http://img.youtube.com/vi/sq4lEuiQ9qA/0.jpg)](http://www.youtube.com/watch?v=sq4lEuiQ9qA)
