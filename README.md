# Quick Start

A quick start guide of how to deploy a UIA Driver Server on Windows OS and how to register it with a Selenium Grid.  

## Prerequisites

1. Windows OS 7 or above
2. [Git for windows installed](https://gitforwindows.org/)
3. [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/) or above installed

## Install .NET 5 & 6 SDK

1. [Download .NET5 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-5.0.408-windows-x64-installer).
2. [Download .NET6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.300-windows-x64-installer).
3. Run each of these files and follow the the installation instructions.

## Clone & Build the Driver Server

1. Create a new folder `UiaDriverServer`.
2. Open a command line and navigate to `UiaDriverServer` folder.
3. Run the clone command (from `observeit` branch) to clone the code - no credentials needed.

```cmd
git clone https://github.com/gravity-api/gravity-uia-server.git -b observeit
```

## Build the Driver Server

1. Using file explorer navigate to `gravity-uia-server\src` folder under `UiaDriverServer` folder.
2. Double click on the the file `UiaDriverServer.sln` - if a Visual Studio version selection appears, choose to open the file with `Visual Studio 2022`.
3. Under `Visual Studio Solution Explorer` right click on the solution > `Build`.
4. Under `Visual Studio Solution Explorer` right click on the `UiaDriverServer` project > `Open Folder in File Explorer`.
5. Under `File Explorer` navigate to `bin\Debug`.
6. Copy the folder `net5.0-windows` and save it anywhere on your hard drive.

## Run the Driver Server

1. Navigate to `net5.0-windows` folder.
2. Double Click on `UiaDriverServer.exe` file.
3. Approve any popup or alert.

## Register Driver Server with a Selenium Grid

This will register the driver server from your localhost to a remote grid. If the host has a static IP, use it instead of `localhost`.

1. Navigate to `net5.0-windows` folder.
2. Open a command line under `net5.0-windows` folder.
3. Run the following command

```cmd
UiaDriverServer.exe --register --host:localhost --hub:HubIpAddress
```
