using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace endpointmanager.wingetbridge
{
    public class YamlManifest
    {
        public string Author { get; set; }
        public string PackageIdentifier { get; set; }
        public string PackageFamilyName { get; set; }
        public string PackageName { get; set; }
        public string PackageVersion { get; set; }
        public string Publisher { get; set; }
        public string PublisherUrl { get; set; }
        public string PublisherSupportUrl { get; set; }
        public string License { get; set; }
        public string Copyright { get; set; }
        public string CopyrightUrl { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string LicenseUrl { get; set; }
        public string PrivacyUrl { get; set; }
        public string PackageUrl { get; set; }
        public string ManifestType { get; set; }
        public string ManifestVersion { get; set; }
        public string MinimumOSVersion { get; set; }
        public string PackageLocale { get; set; }
        public string InstallerLocale { get; set; } //... for all installers within the package
        public string InstallerType { get; set; } //... for all installers within the package
        public string Scope { get; set; } //... for all installers within the package
        public string UpgradeBehavior { get; set; }
        public Dependencies Dependencies { get; set; } //... for all installers within the package
        public List<String> InstallModes { get; set; }
        public List<String> FileExtensions { get; set; }
        public List<String> Tags { get; set; }
        public List<String> Platform { get; set; }
        public List<Installer> Installers { get; set; }
        public List<Localization> Localization { get; set; }
    }

    public class InstallerSwitches
    {
        public string Custom { get; set; }
        public string Interactive { get; set; }
        public string Silent { get; set; }
        public string SilentWithProgress { get; set; }
    }

    public class Dependencies
    {
        public List<String> WindowsFeatures { get; set; }
        public List<String> WindowsLibraries { get; set; }
        public List<PackageDependencies> PackageDependencies { get; set; }
        public List<String> ExternalDependencies { get; set; }
    }

    public class PackageDependencies
    {
        public string PackageIdentifier { get; set; }
        public string MinimumVersion { get; set; }
    }

    public class Localization
    {
        public string PackageLocale { get; set; }
        public string Publisher { get; set; }
        public string ShortDescription { get; set; }
    }

    public class Installer
    {
        public string Architecture { get; set; }
        public string InstallerLocale { get; set; } //... only for this installer within the package
        public string InstallerUrl { get; set; }
        public string InstallerType { get; set; } //... only for this installer within the package
        public string InstallerSha256 { get; set; }
        public string SignatureSha256 { get; set; } //... only for MSIX Installers
        public string ProductCode { get; set; }
        public string Scope { get; set; } //... only for this installer within the package
        public Dependencies Dependencies { get; set; } //... only for this installer within the package
        public InstallerSwitches InstallerSwitches { get; set; }
        public bool Selected { get; set; } //.. for simple selection in an automation powershell script
    }

    [Table("manifest")]
    public class ManifestMSIXTable
    {
        [Key]
        public long rowid { get; set; }
        public long id { get; set; }
        public long name { get; set; }
        public long moniker { get; set; }
        public long version { get; set; }
        public long channel { get; set; }
        public long pathpart { get; set; }
        public byte[] hash { get; set; }
    }

    public class CacheInfo
    {
        public string SchemaVersion { get; set; }
        public DateTime? LastCacheUpdate { get; set; }
        public int PublisherCount { get; set; }
        public int MonikerCount { get; set; }
        public int PackageCount { get; set; }
        public int PackageVersions { get; set; }
        public bool SkippedUpdate { get; set; }
    }

    [Table("dbinfo")]
    public class dbinfo
    {
        [Key]
        public string Property { get; set; }
        public string Value { get; set; }
    }

    [Table("manifests")]
    public class ManifestTable //used in local cache db
    {
        [Key]
        public long Id { get; set; }
        public string PackageId { get; set; }
        public string PublisherId { get; set; }
        public string Name { get; set; }
        public string Moniker { get; set; }
        public string Version { get; set; }
        public string YamlUri { get; set; }
    }

    public class WingetPackage //used to reference a wingetpackage-object in powershell
    {
        public string PackageId { get; set; }
        public string PublisherId { get; set; }
        public string Name { get; set; }
        public string Moniker { get; set; }
        public PackageVersion LatestVersion { get; set; }
        public List<PackageVersion> Versions { get; set; }
    }

    public class PackageVersion
    {
        public string Version { get; set; }
        public string YamlUri { get; set; }
    }

}
