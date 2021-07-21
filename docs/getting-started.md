[← Go Back](https://github.com/ewang2002/RealmEyeSharper/blob/master/docs/docs-guide.md)

# Getting Started Guide 
Here, I will briefly explain how you can set this API up for local use. This is ideal if you are planning on hosting an application that relies on this API on the **same device** as the API itself.

If you encounter a problem while setting this up, please submit an Issue.

## The Guide
Before you begin, be sure you are on a [supported operating system](https://github.com/dotnet/core/blob/main/release-notes/5.0/5.0-supported-os.md). 

0. Download the latest version of the .NET Runtime [here](https://dotnet.microsoft.com/download). When asked what you want to install, select the option **Run server apps**. Then, install it.


1. Download the code. You can either click on the Download button or `git clone` it.

2. Go to the project root directory. The root directory is the directory (folder) that contains files like `RealmSharper.sln`, `LICENSE.txt`, and `README.md`.

3. Run the `dotnet publish` command in this directory. For more information on how this works, click [here](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish) and [here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/visual-studio-publish-profiles).
    - If you are going to run this project on a Windows-based system, run the command:
    ```
    dotnet publish -c Release --self-contained true --runtime win-x64 --framework net5.0
    ```
    - If you are going to run this project on a Linux-based system, run the command:
    ```
    dotnet publish -c Release --self-contained true --runtime linux-x64 --framework net 5.0
    ```
   - If you are going to run this project on OSX (Mac), run the command: 
    ```
    dotnet publish -c Release --self-contained true --runtime osx-x64 --framework net5.0
    ```
   ⚠️ As a side note, if you are planning on running this application on a device that already has the .NET Runtime (corresponding to the specified framework that you are building on) installed, then you can set `--self-contained` to `false`.


4. After the above command finishes running, it should tell you the directory where your compiled application exists. 
    ``` 
    Determining projects to restore...
    All projects are up-to-date for restore.
    RealmAspNet -> C:\Users\...\RealmEyeSharper\RealmAspNet\bin\Release\net5.0\win-x64\RealmAspNet.dll
    RealmAspNet -> C:\Users\...\RealmEyeSharper\RealmAspNet\bin\Release\net5.0\win-x64\publish\
    ```
    In this example, the compiled application is located in the `publish` folder (the very last line).


5. Go to the `publish` folder. Copy **everything** from the `publish` folder to a directory of your choice. 

   ⚠️ As a side note, the directory above the `publish` folder (i.e. the directory that contains the `publish` folder) contains the same exact thing as what is in the `publish` folder. Feel free to delete those files.


6. There should be a file called `appsettings.example.json`. Rename this file to `appsettings.json`. Fill in any appropriate API keys. 

   ⚠️You do not need to provide any API keys to use the API's basic functionality. 
   - You only need to provide a **Webshare.io** API key if you want to send many concurrent RealmEye requests per second (usually for getting many profiles).
   - You only need to provide a **OCR.Space** API key if you want to enable parsing functionality (for example, the `parseWhoOnly` endpoint).

7. Run the `RealmAspNet` executable. 

This concludes the Getting Started guide. If you have any questions, please let us know.

## Other Things
One main thing you should look into is making sure the application restarts upon shutting down. This is especially important if you are planning on hosting this application on a device that is dedicated to hosting your applications. 