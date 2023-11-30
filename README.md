# Quick Start Guide for UIA Driver Server with Selenium Grid

This guide provides a quick start on deploying a UIA Driver Server on Windows OS and registering it with a Selenium Grid.

## Prerequisites

Ensure that the following prerequisites are met before proceeding:

1. Windows OS 7 or above.
2. [Git for Windows](https://gitforwindows.org/) installed.
3. [Visual Studio 2022 Community](https://visualstudio.microsoft.com/vs/) or a later version installed.

## Install .NET 8 SDK

1. [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Run the installer and follow the provided instructions.

## Clone & Build the Driver Server

1. Create a new folder named `UiaDriverServer`.
2. Open a command line and navigate to the `UiaDriverServer` folder.
3. Run the following command to clone the code (from the `observeit` branch) without requiring credentials:

   ```cmd
   git clone https://github.com/gravity-api/gravity-uia-server.git -b observeit
   ```

### Build the Driver Server

1. Using File Explorer, navigate to the `gravity-uia-server\src` folder under the `UiaDriverServer` directory.
2. Double-click on the `UiaDriverServer.sln` file. If prompted, choose to open the file with `Visual Studio 2022`.
3. In Visual Studio Solution Explorer, right-click on the solution and select `Build`.
4. In Visual Studio Solution Explorer, right-click on the `UiaDriverServer` project and choose `Open Folder in File Explorer`.
5. Navigate to `bin\Debug`.
6. Copy the `net8.0-windows` folder and save it to any location on your hard drive.

## Download UIA Driver Server

You can download the UIA Driver Server directly from the [Releases](https://github.com/gravity-api/gravity-uia-server/releases) page. Look for the latest release and download the `UiaDriverServer.zip` file.

1. Extract the contents of the downloaded zip file to a location of your choice.

## Run the Driver Server

1. Navigate to the `net8.0-windows` folder.
2. Double-click on the `UiaDriverServer.exe` file.
3. Approve any popups or alerts that appear.

## Register Driver Server with a Selenium Grid

Register the driver server from your localhost to a remote grid. If the host has a static IP, use it instead of `localhost`.

1. Navigate to the `net8.0-windows` folder.
2. Open a command line under the `net8.0-windows` folder.
3. Run the following command:

   ```cmd
   UiaDriverServer.exe --register --host:localhost --hub:HubIpAddress
   ```

Feel free to reach out if you have any questions or encounter issues during the setup process!
