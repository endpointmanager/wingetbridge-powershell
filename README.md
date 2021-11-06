![Image of wingetbridge-powershell](https://repository-images.githubusercontent.com/423620656/cf98f351-dfaa-4d5d-bb2b-363633e639b6)
[![Twitter URL](https://img.shields.io/twitter/url/https/twitter.com/PaulJezek.svg?style=social&label=Follow%20%40Paul%20Jezek%20%23wingetbridge)](https://twitter.com/PaulJezek)
# wingetbridge-powershell

This repository contains a PowerShell module that completes the ability to automatically provide and maintain applications in your software deployment tool by using the Windows Package Manager Repository.

# Understand the goals of this project

I hope that an official powershell module (maintained by Microsoft) will be available soon, which can be used to accomplish the main goals of this project:
* Automatically provide and maintain packages in software deployment tools (like [Microsoft Endpoint Manager Configuration Manager](https://docs.microsoft.com/en-us/mem/configmgr/))
* Automatically download and update installers into [MDT](https://docs.microsoft.com/en-us/windows/deployment/deploy-windows-mdt/get-started-with-the-microsoft-deployment-toolkit) (Which will be used for [Image Factory](https://deploymentbunny.com/2018/10/19/image-factory-4-0-is-available-for-download/) in my case) 

Meanwhile, I built a powershell module (which is Powershell 5 compatible on purpose) to prepare the required powershell-scripts to automatically maintain packages in Microsoft Endpoint Manager and MDT.

## Known limitations

* Only the public winget repository can be searched (There is no implementation to search private repositories or msstore at the moment)
* No software installation through wingetbridge-module

Currently, there are no plans to implement more features, because I think the official powershell support for [winget-cli](https://github.com/microsoft/winget-cli) will be available soon.

## Risk of damage :warning:

I highly recommend not using WingetBridge in a production environment without validating the downloaded installers before you deploy it. (e.g. by validating the certificate of a signed installer)

## Current Version
v1.0.0.0

## Requirements
* Windows 10 or Windows 11
* .NET Framework 4.8 (Powershell 5)
* Internet connection

## Available cmdlets

After the module is loaded (see Setup)...
* Use "Get-Command -Module endpointmanager.wingetbridge" to get a full list of cmdlets provided by this module.
* Please use e.g. "Get-Help Get-wingetbridge -Examples" to get examples of how to use the corresponding cmdlet.

### Start-WingetSearch
Searches packages in the winget-repository without using the winget-cli.
#### Example (Start-WingetSearch)
```ps
Start-WingetSearch -SearchByMoniker "vlc" -MaxCacheAge 15
```  
> Search the winget repository by moniker "vlc"

### Get-WingetManifest
Downloads the package manifest (includes package details), and return it as an object

### Start-WingetInstallerDownload
Downloads a specified package installer.  

```ps
(Start-WingetSearch -SearchByMoniker "vlc" | Get-WingetManifest).Installers | Start-WingetInstallerDownload -AcceptAgreements
```  
> Search the winget repository by moniker "vlc" and download all installers provided in the manifest  

When using [-AcceptAgreements] you agree any licenses required to download packages.  
You can specify a target directory with [-TargetDirectory], otherwise the download will be stored in the current directory.

### Update-WingetBridgeCache
Updates the cache (local database) for WingetBridge.  
It is optional to update the cache before using the cmdlet "Start-WingetSearch", as it updates the cache automatically if required.

```ps
Update-WingetBridgeCache -Force
```  
> Rebuild the local cache WingetBridge uses.

### Save-WingetBridgeAppIcon
Saves the default icon (supports hi-res icons up to 256x256px) that is contained in the specified executable [-SetupFile], to the specified [-TargetIconFile].

```ps
Save-WingetBridgeAppIcon -SetupFile "C:\Windows\Explorer.exe" -TargetIconFile "explorer.ico"
```  


### Setup
#### Online
* The **wingetbridge-PowerShell module** is now listed on **PowerShell Gallery**, therefore it can be installed with:  
    ```ps
    Install-Module endpointmanager.wingetbridge
    ```
#### Offline
* Extract the folder (endpointmanager.wingetbridge) from the Release-zip to %ProgramFiles%\WindowsPowerShell\Modules\
* OR use "**Import**-Module .\endpointmanager.wingetbridge\endpointmanager.wingetbridge.**psd1**" from the root directory of the extracted Release-zip
* Make sure you don't have any security-restrictions to load the Powershell-Module. If so, please remove any NTFS Alternate Data Streams (ADS) from the Release-zip before you extract it, and set the ExecutionPolicy to Unrestricted

    ```ps
    Unblock-File .\endpointmanager.wingetbridge_v1.1.0.0.zip
    Set-ExecutionPolicy -ExecutionPolicy Unrestricted
    ```

## Credits :heart:
WingetBridge only exists because of the beautiful and hard work of the developers and contributors of [Windows Package Manager](https://docs.microsoft.com/en-us/windows/package-manager/).

## License (does not include 3rd party software, which can be downloaded using this module)

See [LICENSE](LICENSE.md) file for licence rights and limitations (MIT)

The author of this module is not responsible for, nor does it grant any licenses to, third-party packages.