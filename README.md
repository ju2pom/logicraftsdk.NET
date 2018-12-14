# logicraftsdk.NET
This is a .NET SDK to control the Logitech Craft keyboard "Crown" (the knob on the picture below)
![enter image description here](https://github.com/Logitech/logi_craft_sdk/blob/master/documentation/assets/craft.png)


# Intro
If you are a happy owner of this beautiful keyboard you are probably 
* Willing to get the most out of the Crown as a user
* Willing to add support for to your application as a developer

I started this project because the only useful resource I found for this keyboard is the following repo: [logi_craft_sdk](https://github.com/Logitech/logi_craft_sdk)
This repo contains  enough information to understand how it works, but does not provides a library ready to use.

## Usage (in code)
As soon as you have cloned and compile the SDK:
* Add a reference to the assembly
* Use this piece of code:
```csharp
var sdk = new CraftDevice();
sdk.Connect(Process.GetCurrentProcess(), ApplicationId).Wait();
sdk.CrownTouched += OnCrownTouched;
sdk.CrownTurned += OnCrownTurned;
```
The two events will tell you when the crown is touched or turned.
* If you want to change the current tool simply call:
```csharp
sdk.ChangeTool("Id_Of_The_Tool")
```
* Remember to disconnect when done:
```csharp
sdk.Disconnect();
``` 
## Register your plugin
In order to let _Logitech Option_ to know about your plugin/application you need to follow the instructions from official logitech documentation [here](https://github.com/Logitech/logi_craft_sdk/blob/master/samples/WinFormsCrownSample/README.md)

I would advise you to use the sample console application in this repository as a starting point to understand more easily the relationship between the tools.json file and the application.
You''l notice that the sample application I made has a post-build event to copy the necessary files at the right place. **But** at least you'll need to uninstall and reinstall your plugin in the _Logitech Options_ software.  My experience with this process told me that you'd better restart the computer each time you change the manifest files.
