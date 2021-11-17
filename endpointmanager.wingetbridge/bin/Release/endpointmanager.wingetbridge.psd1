#
# Module manifest for module 'endpointmanager.wingetbridge'
#
# Generated by: Paul Jezek
#
# Generated on: 09/09/2021, Modified on: Nov 17, 2021
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'endpointmanager.wingetbridge.dll'

# Version number of this module.
ModuleVersion = '1.2.0.2'

# Supported PSEditions
# CompatiblePSEditions = @()

# ID used to uniquely identify this module
GUID = '84dd2739-7cca-4291-a004-b80d3b63166d'

# Author of this module
Author = 'Paul Jezek'

# Company or vendor of this module
CompanyName = 'endpointmanager\endpointmanager.wingetbridge'

# Copyright statement for this module
Copyright = '(c) 2021 Paul Jezek. All rights reserved.'

# Description of the functionality provided by this module
Description = 'PowerShell Cmdlets, to retrieve package and installer informations from winget repository. This allows us to automatically maintain application installers in our software deployment tool like Microsoft Endpoint Manager Configuration Manager (ConfigMgr).'

# Minimum version of the PowerShell engine required by this module
# PowerShellVersion = ''

# Name of the PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the PowerShell host required by this module
PowerShellHostVersion = '5.1'

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
DotNetFrameworkVersion = '4.8'

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
RequiredAssemblies = @('endpointmanager.wingetbridge.dll','ByteSize.dll','YamlDotNet.dll','System.Data.SQLite.Linq.dll','SQLite.CodeFirst.dll','EntityFramework.dll','System.Data.SQLite.dll','System.Data.SQLite.EF6.dll','Microsoft.Win32.Primitives.dll','netstandard.dll','System.Drawing.dll','System.AppContext.dll','System.Collections.Concurrent.dll','System.Collections.dll','System.Collections.NonGeneric.dll','System.Collections.Specialized.dll','System.ComponentModel.dll','System.ComponentModel.EventBasedAsync.dll','System.ComponentModel.Primitives.dll','System.ComponentModel.TypeConverter.dll','System.Console.dll','System.Data.Common.dll','System.Diagnostics.Contracts.dll','System.Diagnostics.Debug.dll','System.Diagnostics.FileVersionInfo.dll','System.Diagnostics.Process.dll','System.Diagnostics.StackTrace.dll','System.Diagnostics.TextWriterTraceListener.dll','System.Diagnostics.Tools.dll','System.Diagnostics.TraceSource.dll','System.Diagnostics.Tracing.dll','System.Drawing.Primitives.dll','System.Dynamic.Runtime.dll','System.Globalization.Calendars.dll','System.Globalization.dll','System.Globalization.Extensions.dll','System.IO.Compression.dll','System.IO.Compression.ZipFile.dll','System.IO.dll','System.IO.FileSystem.dll','System.IO.FileSystem.DriveInfo.dll','System.IO.FileSystem.Primitives.dll','System.IO.FileSystem.Watcher.dll','System.IO.IsolatedStorage.dll','System.IO.MemoryMappedFiles.dll','System.IO.Pipes.dll','System.IO.UnmanagedMemoryStream.dll','System.Linq.dll','System.Linq.Expressions.dll','System.Linq.Parallel.dll','System.Linq.Queryable.dll','System.Management.Automation.dll','System.Net.Http.dll','System.Net.NameResolution.dll','System.Net.NetworkInformation.dll','System.Net.Ping.dll','System.Net.Primitives.dll','System.Net.Requests.dll','System.Net.Security.dll','System.Net.Sockets.dll','System.Net.WebHeaderCollection.dll','System.Net.WebSockets.Client.dll','System.Net.WebSockets.dll','System.ObjectModel.dll','System.Reflection.dll','System.Reflection.Extensions.dll','System.Reflection.Primitives.dll','System.Resources.Reader.dll','System.Resources.ResourceManager.dll','System.Resources.Writer.dll','System.Runtime.CompilerServices.VisualC.dll','System.Runtime.dll','System.Runtime.Extensions.dll','System.Runtime.Handles.dll','System.Runtime.InteropServices.dll','System.Runtime.InteropServices.RuntimeInformation.dll','System.Runtime.Numerics.dll','System.Runtime.Serialization.Formatters.dll','System.Runtime.Serialization.Json.dll','System.Runtime.Serialization.Primitives.dll','System.Runtime.Serialization.Xml.dll','System.Security.Claims.dll','System.Security.Cryptography.Algorithms.dll','System.Security.Cryptography.Csp.dll','System.Security.Cryptography.Encoding.dll','System.Security.Cryptography.Primitives.dll','System.Security.Cryptography.X509Certificates.dll','System.Security.Principal.dll','System.Security.SecureString.dll','System.Text.Encoding.dll','System.Text.Encoding.Extensions.dll','System.Text.RegularExpressions.dll','System.Threading.dll','System.Threading.Overlapped.dll','System.Threading.Tasks.dll','System.Threading.Tasks.Parallel.dll','System.Threading.Thread.dll','System.Threading.ThreadPool.dll','System.Threading.Timer.dll','System.ValueTuple.dll','System.Xml.ReaderWriter.dll','System.Xml.XDocument.dll','System.Xml.XmlDocument.dll','System.Xml.XmlSerializer.dll','System.Xml.XPath.dll','System.Xml.XPath.XDocument.dll')

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
# FormatsToProcess = @()

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
# NestedModules = @('')

# Functions to export from this module
FunctionsToExport = '*'

# Cmdlets to export from this module
CmdletsToExport = @('Start-WingetSearch', 'Update-WingetBridgeCache', 'Get-WingetManifest', 'Start-WingetInstallerDownload', "Save-WingetBridgeAppIcon")

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module
AliasesToExport = '*'

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @('endpointmanager.wingetbridge.dll')

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @("wingetbridge", "configmgr", "MEM", "MEMCM", "installer")

        # A URL to the license for this module.
        LicenseUri = 'https://github.com/endpointmanager/wingetbridge-powershell/blob/master/LICENSE'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/endpointmanager/wingetbridge-powershell'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        # ReleaseNotes = ''

        # Prerelease string of this module
        # Prerelease = ''

        # Flag to indicate whether the module requires explicit user acceptance for install/update/save
        RequireLicenseAcceptance = $false

        # External dependent modules of this module
        # ExternalModuleDependencies = @()

    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
HelpInfoURI = 'https://github.com/endpointmanager/wingetbridge-powershell'

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}