using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Configuration;
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.IO.Compression;
using System.IO;
using ByteSizeLib;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Drawing;

namespace endpointmanager.wingetbridge
{
    [Cmdlet("Start", "WingetSearch")]
    [OutputType(typeof(PSObject))]
    public class StartWingetSearchCommand : AsyncCmdlet
    {
        [Parameter(Mandatory = false)]
        public String SearchById
        {
            get { return searchbyid; }
            set { searchbyid = value; }
        }
        private String searchbyid;

        [Parameter(Mandatory = false)]
        public String SearchByPublisherId
        {
            get { return searchbypublisherid; }
            set { searchbypublisherid = value; }
        }
        private String searchbypublisherid;

        [Parameter(Mandatory = false)]
        public String SearchByName
        {
            get { return searchbyname; }
            set { searchbyname = value; }
        }
        private String searchbyname;

        [Parameter(Mandatory = false)]
        public String SearchByMoniker
        {
            get { return searchbymoniker; }
            set { searchbymoniker = value; }
        }
        private String searchbymoniker;

        [Parameter(Mandatory = false)]
        public int? MaxCacheAge
        {
            get { return maxCacheAge; }
            set { maxCacheAge = value; }
        }
        private int? maxCacheAge;
        [Parameter(Mandatory = false)]
        public SwitchParameter UseDefaultWebProxy
        { get; set; } = true;

        protected override async Task ProcessRecordAsync()
        {
            if (MaxCacheAge == null)
            {
                MaxCacheAge = 1440;
                Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "You did not specified '-MaxCacheAge', the default value is 1440 (minutes, = 1 day)");
            }
            if ((WingetBridge.GetPackageCount() == 0) || (WingetBridge.GetCacheSchemaVersion() != "1.3") || ((MaxCacheAge != null) && (WingetBridge.MinutesBetweenLastCacheUpdate() >= MaxCacheAge)))
            {
                try
                {
                    string additionalParameters = " -MaxCacheAge " + MaxCacheAge.ToString();
                    if (!UseDefaultWebProxy.IsPresent)
                    {
                        additionalParameters += " -UseDefaultWebProxy:$false";
                    }
                    ScriptBlock sb = ScriptBlock.Create("Update-WingetBridgeCache"+ additionalParameters);
                    var ret = sb.Invoke();
                }
                catch
                {
                    Host.UI.WriteLine(ConsoleColor.Red, Host.UI.RawUI.BackgroundColor, "Failed to update cache");
                }
                if (WingetBridge.GetPackageCount() == 0)
                {
                    Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "Please rebuild your local cache! (Update-WingetBridgeCache -Force)");
                }
            }
            else
            {
                Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "Search against local cache (Last Update: " + WingetBridge.GetLastCacheUpdate().ToString()+")");
            }
            if ((searchbyid == null) && (searchbyname == null) && (searchbypublisherid == null) && (searchbymoniker == null))
            {
                IEnumerable<WingetPackage> packages = await WingetBridge.GetPackagesByIdAsync("*"); //Return all packages
                List<WingetPackage> asList = packages.ToList();
                foreach (var package in packages)
                {
                    WriteObject(package);
                }
                if (packages.Count() == 0)
                {
                    Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "No packages in local cache found");
                }
            }
            else
            {
                if (searchbyid != null)
                {
                    IEnumerable<WingetPackage> packages = AsyncHelper.RunSync<IEnumerable<WingetPackage>>(() => WingetBridge.GetPackagesByIdAsync(searchbyid));
                    List<WingetPackage> asList = packages.ToList();
                    foreach (var package in packages)
                    {
                        WriteObject(package);
                    }
                    if (packages.Count() == 0)
                    {
                        Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "No match found");
                    }
                }
                if (searchbypublisherid != null)
                {
                    IEnumerable<WingetPackage> packages = AsyncHelper.RunSync<IEnumerable<WingetPackage>>(() => WingetBridge.GetPackagesByPublisherIdAsync(searchbypublisherid));
                    List<WingetPackage> asList = packages.ToList();
                    foreach (var package in packages)
                    {
                        WriteObject(package);
                    }
                    if (packages.Count() == 0)
                    {
                        Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "No match found");
                    }
                }
                if (searchbyname != null)
                {
                    IEnumerable<WingetPackage> packages = AsyncHelper.RunSync<IEnumerable<WingetPackage>>(() => WingetBridge.GetPackagesByNameAsync(searchbyname));
                    List<WingetPackage> asList = packages.ToList();
                    foreach (var package in packages)
                    {
                        WriteObject(package);
                    }
                    if (packages.Count() == 0)
                    {
                        Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "No match found");
                    }
                }
                if (searchbymoniker != null)
                {
                    IEnumerable<WingetPackage> packages = AsyncHelper.RunSync<IEnumerable<WingetPackage>>(() => WingetBridge.GetPackagesByMonikerAsync(searchbymoniker));
                    List<WingetPackage> asList = packages.ToList();
                    foreach (var package in packages)
                    {
                        WriteObject(package);
                    }
                    if (packages.Count() == 0)
                    {
                        Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "No match found");
                    }
                }
            }
        }
    }

    [Cmdlet("Get", "WingetManifest")]
    [OutputType(typeof(PSObject))]
    public class GetWingetManifestCommand : PSCmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public WingetPackage Package
        {
            get { return package; }
            set { package = value; }
        }
        private WingetPackage package;

        [Parameter(Mandatory = false, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public PackageVersion PackageVersion
        {
            get { return packageVersion; }
            set { packageVersion = value; }
        }
        private PackageVersion packageVersion;

        [Parameter(Mandatory = false)]
        public SwitchParameter UseDefaultWebProxy
        { get; set; } = true;
        public SwitchParameter Verbose
        { get; set; } = true;
        protected override void ProcessRecord()
        {
            if ((package == null) && (packageVersion == null))
            {
                throw new ArgumentException("You need to specifiy a package (-Package) or specific version of a package (-PackageVersion)");
            }
            else
            {
                if (package != null)
                {
                    packageVersion = package.LatestVersion;
                }
                if (Verbose.IsPresent)
                {
                    Host.UI.WriteLine(ConsoleColor.Magenta, Host.UI.RawUI.BackgroundColor, "Download Manifest [" + WingetBridge.WingetCacheUrl + packageVersion.YamlUri + "]");
                }
                WriteObject(AsyncHelper.RunSync(() => WingetBridge.GetManifestAsync(packageVersion.YamlUri, UseDefaultWebProxy.IsPresent)));
            }
        }
    }

    [Cmdlet("Start", "WingetInstallerDownload")]
    [OutputType(typeof(PSObject))]
    public class StartWingetInstallerDownloadCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public Installer PackageInstaller
        {
            get { return packageInstaller; }
            set { packageInstaller = value; }
        }
        private Installer packageInstaller;

        [Parameter(Mandatory = false)]
        public String TargetDirectory
        {
            get { return targetDirectory; }
            set { targetDirectory = value; }
        }
        private string targetDirectory;

        [Parameter(Mandatory = false)]
        public SwitchParameter UseDefaultWebProxy
        { get; set; } = true;
        [Parameter(Mandatory = false)]
        public SwitchParameter Force //Overwrites existing files
        { get; set; } = false;

        [Parameter(Mandatory = false)]
        public SwitchParameter AcceptAgreements
        { get; set; } = false;

        protected void ProgressChanged(object sender, DownloadEventArgs e)
        {
            ProgressRecord myprogress = new ProgressRecord(0, "Download in Progress", "-");
            myprogress.PercentComplete = e.PercentDone;
            if (e.TotalFileSize != 0)
            {
                myprogress.StatusDescription = e.PercentDone + "% (" + ByteSize.FromBytes(e.TotalFileSize).ToString() + ")";
            }
            else
            {
                myprogress.StatusDescription = e.PercentDone + "%";
            }
            WriteProgress(myprogress);
        }

        protected void DownloadedComplete(object sender, EventArgs e)
        {
            Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "Download finished");

            ProgressRecord progress = new ProgressRecord(0, "Download in Progress", ".");
            progress.RecordType = ProgressRecordType.Completed; //Removes the Progressbar
            WriteProgress(progress);
        }

        protected override void ProcessRecord()
        {
            if (!AcceptAgreements.IsPresent)
            {
                Host.UI.WriteLine(ConsoleColor.Red, Host.UI.RawUI.BackgroundColor, "When using this cmdlet you need to confirm that the author of this module is not responsible for, nor does it grant any licenses to, third-party packages.");
                Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "Even the author did it's best to develop a stable build, he provides his work \"as-it-is\" and should not be used in a production environment without testing it.");
                Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "Before you install (or deploy) any packages, you need to check and agree it's license agreements on your own.");
                Host.UI.WriteLine(ConsoleColor.Yellow, Host.UI.RawUI.BackgroundColor, "WingetBridge uses the official Winget-Repository to get download informations. However, Microsoft is also not responsible for, nor does it grant any licences to, third-party packages.");
                Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "To accept the agreement and to avoid this prompt, use this cmdlet together with [-AcceptAgreements]");
                Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "");
            }
            else
            {
                var FileToDownload = packageInstaller.InstallerUrl;
                Host.UI.WriteLine(ConsoleColor.Magenta, Host.UI.RawUI.BackgroundColor, "Download Installer [" + FileToDownload + "]");
                if (targetDirectory == null) { targetDirectory = this.SessionState.Path.CurrentFileSystemLocation.ToString(); }
                FileDownloader downloader = new FileDownloader();
                if (UseDefaultWebProxy.IsPresent)
                {
                    IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                    defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                    downloader.Proxy = defaultWebProxy;
                }
                downloader.DeleteOnError = true;
                downloader.OverrideExisting = Force.IsPresent;
                downloader.DownloadComplete += new EventHandler(DownloadedComplete);
                downloader.ProgressChanged += new DownloadProgressHandler(ProgressChanged);
                bool QueuedDownload = true;
                int Retries = 0;

                do
                {
                    DownloadResult downloadedInstaller = downloader.Download(FileToDownload, targetDirectory);
                    QueuedDownload = false;
                    if (downloadedInstaller.FullPath != null)
                    {
                        if (WingetBridge.SHA256FromFileVerified(downloadedInstaller.FullPath, packageInstaller.InstallerSha256))
                        {
                            Host.UI.WriteLine(ConsoleColor.Green, Host.UI.RawUI.BackgroundColor, "Downloaded file, Hash validated successful");
                            downloadedInstaller.HashValidated = true;
                        }
                        else
                        {
                            Host.UI.WriteLine(ConsoleColor.Red, Host.UI.RawUI.BackgroundColor, "Downloaded file, Hash-Validation failed");
                            if ((Retries == 0) && (downloadedInstaller.AlreadyExisted) && (downloadedInstaller.DownloadDone))
                            {
                                Host.UI.WriteLine(ConsoleColor.Magenta, Host.UI.RawUI.BackgroundColor, "Retry Download one more time, content may have changed");
                                downloader.OverrideExisting = true;
                                QueuedDownload = true;
                            }
                        }
                        if (!QueuedDownload) { WriteObject(downloadedInstaller); }
                    }
                    Retries++;
                } while (QueuedDownload);
            }
        }
    }

    [Cmdlet("Update", "WingetBridgeCache")]
    [OutputType(typeof(PSObject))]
    public class UpdateWingetBridgeCacheCommand : AsyncCmdlet
    {
        [Parameter(Mandatory = false)]
        public int? MaxCacheAge
        {
            get { return maxCacheAge; }
            set { maxCacheAge = value; }
        }
        private int? maxCacheAge;
        [Parameter(Mandatory = false)]
        public SwitchParameter UseDefaultWebProxy
        { get; set; } = true;
        [Parameter(Mandatory = false)]
        public SwitchParameter Force
        { get; set; } = false;

        CancellationTokenSource internalTokenSource = new CancellationTokenSource();
        Timer timer;
        WebClient client = new WebClient();

        protected override async Task EndProcessingAsync()
        {
            timer.Dispose();
            await base.EndProcessingAsync();
        }

        protected override async Task StopProcessingAsync()
        {
            await base.StopProcessingAsync();
        }

        void CancelAfterTimeout(object state)
        {
            if (_cancellationSource.IsCancellationRequested)
            {
                internalTokenSource.Cancel();
                timer.Dispose();
                if (client.IsBusy) { client.CancelAsync(); }
            }
        }

        protected override async Task ProcessRecordAsync()
        {
            timer = new Timer(new TimerCallback(CancelAfterTimeout), null, 3000, 3000);
            bool MSIXDownloadFailed = false;

            void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
            {
                try
                {
                    if (e.Error != null)
                    {
                        MSIXDownloadFailed = true;
                        WriteError(new ErrorRecord(e.Error, "Failed to gather Winget Repository", ErrorCategory.InvalidOperation, null));
                    }
                    else
                    {
                        if (e.Cancelled)
                        {
                            Host.UI.WriteLine(ConsoleColor.Red, Host.UI.RawUI.BackgroundColor, "Downloaded was cancelled");
                        }
                        else
                        {
                            lock (e.UserState)
                            {
                                //releases blocked thread
                                Monitor.Pulse(e.UserState);
                            }
                        }
                    }

                    ProgressRecord progress = new ProgressRecord(0, "Gather Winget Repository", ".");
                    progress.RecordType = ProgressRecordType.Completed; //Removes the Progressbar
                    WriteProgress(progress);
                    internalTokenSource.Cancel();
                }
                catch { } //Required to prevent "Pipeline closed Exception" on cancellation

                if (MSIXDownloadFailed)
                {
                    throw new Exception(
                        String.Format(e.Error.Message), e.Error);
                }
            }

            void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                try
                {
                    ProgressRecord progress = new ProgressRecord(0, "Gather Winget Repository", e.ProgressPercentage.ToString() + "%");
                    progress.PercentComplete = e.ProgressPercentage;
                    WriteProgress(progress);
                }
                catch { } //Required to prevent "Pipeline closed Exception" on cancellation
            }

            CacheInfo cacheinfo = new CacheInfo();

            if (MaxCacheAge == null)
            {
                MaxCacheAge = 1440;
                if (!Force.IsPresent) { Host.UI.WriteLine(ConsoleColor.Gray, Host.UI.RawUI.BackgroundColor, "You did not specified '-MaxCacheAge', the default value is 1440 (minutes, = 1 day)"); }
            }
            if ((Force.IsPresent) || (!File.Exists(WingetBridge.CacheDatabasePath)) || (WingetBridge.GetPackageCount() == 0) || (WingetBridge.GetCacheSchemaVersion() != "1.3") || ((MaxCacheAge != null) && (WingetBridge.MinutesBetweenLastCacheUpdate() >= MaxCacheAge)))
            {
                //Initialize folders
                if (!Directory.Exists(WingetBridge.RootPath)) { Directory.CreateDirectory(WingetBridge.RootPath); }
                if (!Directory.Exists(WingetBridge.DatabasePath)) { Directory.CreateDirectory(WingetBridge.DatabasePath); }
                if (!Directory.Exists(WingetBridge.MSIXPath)) { Directory.CreateDirectory(WingetBridge.MSIXPath); }

                if ((Force.IsPresent) || (WingetBridge.GetCacheSchemaVersion() != "1.3"))
                {
                    if (File.Exists(WingetBridge.CacheDatabasePath)) {
                        File.Delete(WingetBridge.CacheDatabasePath);
                    }
                }

                try
                {
                    if (UseDefaultWebProxy.IsPresent)
                    {
                        IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                        defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                        client.Proxy = defaultWebProxy;
                    }

                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);

                    try
                    {
                        await client.DownloadFileTaskAsync(new System.Uri(WingetBridge.MSIXSource), WingetBridge.MSIXTempSource);
                    }
                    catch (WebException wex)
                    {
                        Host.UI.WriteLine(ConsoleColor.Red, Host.UI.RawUI.BackgroundColor, "Web Exception occured:" + wex.Message.ToString());
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, "Failed to gather Winget Repository", ErrorCategory.InvalidOperation, null));
                }

                if (!MSIXDownloadFailed)
                {
                    ZipArchive compressedFiles = ZipFile.Open(WingetBridge.MSIXTempSource, ZipArchiveMode.Read);
                    try
                    {
                        foreach (var compressedfile in compressedFiles.Entries)
                        {
                            if (compressedfile.Name == "index.db")
                            {
                                compressedfile.ExtractToFile(WingetBridge.MSIXDatabasePath, true);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, "", ErrorCategory.InvalidOperation, null));
                    }
                    finally
                    {
                        compressedFiles.Dispose();
                    }

                    ProgressRecord progress = new ProgressRecord(1, "Update local cache", "In Progress");
                    WriteProgress(progress);
                    await WingetBridge.GenerateDatabaseAsync();
                    Host.UI.WriteLine(ConsoleColor.Magenta, Host.UI.RawUI.BackgroundColor, "Succesfully updated local cache");
                    progress.RecordType = ProgressRecordType.Completed; //Removes the Progressbar
                    WriteProgress(progress);
                }
            }
            else
            {
                cacheinfo.SkippedUpdate = true;
            }

            if (!MSIXDownloadFailed)
            {
                cacheinfo.LastCacheUpdate = WingetBridge.GetLastCacheUpdate();
                cacheinfo.SchemaVersion = WingetBridge.GetCacheSchemaVersion();
                cacheinfo.PublisherCount = WingetBridge.GetPublisherCount();
                cacheinfo.MonikerCount = WingetBridge.GetMonikerCount();
                cacheinfo.PackageCount = WingetBridge.GetPackageCount();
                cacheinfo.PackageVersions = WingetBridge.GetPackageVersionsCount();

                WriteObject(cacheinfo);
            }
        }
    }

    [Cmdlet("Save", "WingetBridgeAppIcon")]
    public class SaveWingetBridgeAppIcon : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string SetupFile
        {
            get { return setupFile; }
            set { setupFile = value; }
        }
        private string setupFile;

        [Parameter(Mandatory = true)]
        public string TargetIconFile
        {
            get { return targetIconFile; }
            set { targetIconFile = value; }
        }
        private string targetIconFile;

        protected override void ProcessRecord()
        {
            if (File.Exists(setupFile))
            {
                using (var fileStream = new FileStream(TargetIconFile, FileMode.Create, FileAccess.Write))
                {
                    Icon appicon = IconExtractor.ExtractIconFromExecutable(setupFile);
                    appicon.Save(fileStream);
                    Host.UI.WriteLine(ConsoleColor.Magenta, Host.UI.RawUI.BackgroundColor, "Successfully saved AppIcon to [" + targetIconFile + "]");
                }
            }
            else
            {
                throw new ArgumentException("You must specifiy an executable [-SetupFile] that exists. (File [" + setupFile + "] not found)");
            }
        }
    }
}
