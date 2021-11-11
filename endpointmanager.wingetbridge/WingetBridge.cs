using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SQLite;
using SQLite.CodeFirst;
using System.Data.Common;
using System.Configuration;
using System.Data.SQLite.EF6;
using System.Data.Entity.Core.Common;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure;
using System.Net;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Security.Cryptography;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace endpointmanager.wingetbridge
{
    class WingetBridge
    {
        public static readonly string DefaultUserAgent = "wingetbridge/1.2";
        public static readonly int DefaultTimeout = 60000;
        public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), typeof(WingetBridge).Namespace);
        public static readonly string DatabasePath = Path.Combine(RootPath, "Database");
        public static readonly string MSIXPath = Path.Combine(DatabasePath, "MSIX");
        public static readonly string MSIXDatabasePath = Path.Combine(MSIXPath, "index.db");
        public static readonly string CacheDatabasePath = Path.Combine(DatabasePath, "wingetbridge.db");
        public static readonly string MSIXSource = "https://winget.azureedge.net/cache/source.msix";
        public static readonly string MSIXTempSource = Path.Combine(MSIXPath, "source.msix");
        public static readonly string WingetCacheUrl = "https://winget.azureedge.net/cache/";

        public static string GetSHA256FromFile(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // ComputeHash - returns byte array
                    byte[] bytes = sha256Hash.ComputeHash(stream);

                    // Convert byte array to a string
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
        }

        public static bool SHA256FromFileVerified(string filename, string expectedSHA256Hash)
        {
            string TargetHash = WingetBridge.GetSHA256FromFile(filename);
            if (TargetHash.ToLower() == expectedSHA256Hash.ToLower())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public class LocalCacheContext : DbContext
        {
            public LocalCacheContext(string filename)
                : base(new SQLiteConnection()
                {
                    ConnectionString =
                        new SQLiteConnectionStringBuilder()
                        { DataSource = filename, ForeignKeys = true }
                        .ConnectionString
                }, true)
            {
                DbProviderFactory factory = System.Data.SQLite.SQLiteFactory.Instance;
                Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS 'manifests' ( 'Id' INTEGER NOT NULL CONSTRAINT 'PK_manifests' PRIMARY KEY AUTOINCREMENT, 'PackageId' TEXT NULL, 'PublisherId' TEXT NULL, 'Name' TEXT NULL, 'Version' TEXT NULL, 'Moniker' TEXT NULL, 'YamlUri' TEXT NULL)");
                Database.ExecuteSqlCommand("CREATE TABLE IF NOT EXISTS 'dbinfo' ('Property' TEXT NOT NULL CONSTRAINT 'PK_dbinfo' PRIMARY KEY, 'Value' TEXT NULL)");
            }
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            public DbSet<ManifestTable> ManifestTable { get; set; }
            public DbSet<dbinfo> dbinfo { get; set; }
        }

        public class MSIXContext : DbContext
        {

            public MSIXContext(string filename)
                : base(new SQLiteConnection()
                {
                    ConnectionString =
                        new SQLiteConnectionStringBuilder()
                        { DataSource = filename, ForeignKeys = true }
                        .ConnectionString
                }, true)
            {
                DbProviderFactory factory = System.Data.SQLite.SQLiteFactory.Instance;
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
                modelBuilder.Entity<ManifestTable>().HasKey(x => x.Id);
            }

            private const string CteQuery = @"WITH cte_paths AS (
                SELECT       
		            1 AS [Level],
                    rowid,
                    parent,
                    CAST(([pathpart]) as TEXT) AS [path]
                FROM pathparts t1
                UNION ALL
                SELECT
		            M.[Level] + 1 As [Level],
		            M.[rowid] as rowid,
                    e.parent,
                    CAST((e.pathpart || '\' || M.[path]) AS TEXT) AS [path]
                FROM pathparts e
                    JOIN cte_paths M ON e.rowid = M.parent
            )
            SELECT
                manifest.pathpart as Id,
                ids.id as PackageId,
                ids.id as PublisherId,
                manifest.rowid,
                names.name As Name,
                versions.version,
                monikers.moniker as Moniker,
                cte_paths.path As YamlUri
            FROM ids JOIN manifest on manifest.id=ids.rowid
                JOIN cte_paths on cte_paths.rowid=manifest.pathpart
                JOIN names on names.rowid=manifest.name
                JOIN versions on versions.rowid=manifest.version
                JOIN monikers on monikers.rowid = manifest.moniker
            WHERE cte_paths.parent IS NULL";

            public virtual DbSqlQuery<ManifestTable> ManifestTable => Set<ManifestTable>().SqlQuery(CteQuery, new object[0]);
        }

        public static async Task GenerateDatabaseAsync()
        {
            var msixDB = new MSIXContext(MSIXDatabasePath);
            var localcacheDB = new LocalCacheContext(CacheDatabasePath);
            localcacheDB.ManifestTable.RemoveRange(localcacheDB.ManifestTable); //fails if Table-Schema does not match

            var query =
                from manifest in msixDB.ManifestTable
                select new ManifestTable
                {
                    PackageId = manifest.PackageId,
                    PublisherId = manifest.PublisherId.Substring(0, manifest.PublisherId.IndexOf(".")),
                    Name = manifest.Name,
                    Moniker = manifest.Moniker,
                    YamlUri = manifest.YamlUri,
                    Version = manifest.Version
                };

            var data = query.ToArray();
            localcacheDB.ManifestTable.AddRange(data);
            localcacheDB.Database.ExecuteSqlCommand("REPLACE INTO dbinfo (Property, Value) VALUES ('LastUpdate','" + DateTime.Now.Ticks.ToString() + "')");
            localcacheDB.Database.ExecuteSqlCommand("REPLACE INTO dbinfo (Property, Value) VALUES ('SchemaVersion','1.3')");
            await localcacheDB.SaveChangesAsync();
            localcacheDB.Dispose();
            GC.Collect(); //Unlock SQLite Files
        }

        public static DateTime? GetLastCacheUpdate()
        {
            try
            {
                using (var localcacheDB = new LocalCacheContext(CacheDatabasePath))
                {
                    var dbQuery = localcacheDB.dbinfo.Where(c => c.Property == "LastUpdate");
                    var data = dbQuery.ToList();
                    if (Int64.TryParse(data.First().Value, out long LastTicks))
                    {
                        DateTime LastUpdate = new DateTime(LastTicks);
                        return LastUpdate;
                    }
                    else
                    { return null; }
                }
            }
            catch { return null; }
        }

        public static String GetCacheSchemaVersion()
        {
            try
            {
                using (var localcacheDB = new LocalCacheContext(CacheDatabasePath))
                {
                    var dbQuery = localcacheDB.dbinfo.Where(c => c.Property == "SchemaVersion").AsEnumerable();
                    var data = dbQuery.ToList();
                    var CurrentSchemaVersion = data.First().Value;
                    return CurrentSchemaVersion;
                    
                }
            }
            catch { return "0"; }
        }

        public static int MinutesBetweenLastCacheUpdate()
        {
            try
            {
                using (var localcacheDB = new LocalCacheContext(CacheDatabasePath))
                {
                    var dbQuery = localcacheDB.dbinfo.Where(c => c.Property == "LastUpdate");
                    var data = dbQuery.ToList();
                    string ValueFromDB = data.First().Value;
                    if (Int64.TryParse(ValueFromDB, out long LastTicks))
                    {
                        DateTime LastUpdate = new DateTime(LastTicks);
                        TimeSpan timebetween = DateTime.Now.Subtract(LastUpdate);
                        return ((int)timebetween.TotalMinutes);
                    }
                    else { return -1; }
                }
            }
            catch
            { return -1; }
        }

        public static int GetMonikerCount()
        {
            try
            {
                using (var localcacheDB = new LocalCacheContext(CacheDatabasePath))
                {
                    int PackageCount = localcacheDB.ManifestTable.GroupBy(e => e.Moniker).Count();
                    return PackageCount;
                }
            }
            catch { return 0; }
        }

        public static int GetPublisherCount()
        {
            try
            {
                using (var localcacheDB = new LocalCacheContext(CacheDatabasePath))
                {
                    int PackageCount = localcacheDB.ManifestTable.GroupBy(e => e.PublisherId).Count();
                    return PackageCount;
                }
            }
            catch { return 0; }
        }

        public static int GetPackageCount()
        {
            try
            {
                using (var localcacheDB = new LocalCacheContext(CacheDatabasePath))
                {
                    int PackageCount = localcacheDB.ManifestTable.GroupBy(e => e.PackageId).Count();
                    return PackageCount;
                }
            }
            catch { return 0; }
        }

        public static int GetPackageVersionsCount()
        {
            try
            {
                using (var localcacheDB = new LocalCacheContext(CacheDatabasePath))
                {
                    int VersionsCount = localcacheDB.ManifestTable.Count();
                    return VersionsCount;
                }
            }
            catch { return 0; }
        }

        public static Version StringToVersion(string strVersion)
        {
            Version tmpVersion;
            if (Version.TryParse(strVersion, out tmpVersion))
            {
                return tmpVersion;
            }
            else { return Version.Parse("0.0.0"); }
        }

        public async static Task<IEnumerable<WingetPackage>> GetPackagesByNameAsync(string PackageName)
        {
            var localcacheDB = new LocalCacheContext(CacheDatabasePath);

            var dbQuery = localcacheDB.ManifestTable.Select(x => new
            {
                x.PackageId,
                x.PublisherId,
                x.YamlUri,
                x.Version,
                x.Moniker,
                x.Name
            }).Where(c => c.Name.ToLower().Contains(PackageName.ToLower()));

            var data = await dbQuery.ToListAsync();

            return data
                .GroupBy(x => x.PackageId)
                .OrderBy(x => x.Key)
                .Select(g => new WingetPackage
                {
                    PackageId = g.Key,
                    PublisherId = g.Select(x => x.PublisherId).First(),
                    LatestVersion = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderBy(e => StringToVersion(e.Version)).Last(),
                    Versions = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderBy(e => StringToVersion(e.Version)).ToList(),
                    Name = g.Select(x => x.Name).First(),
                    Moniker = g.Select(x => x.Moniker).First()
                });
        }

        public async static Task<IEnumerable<WingetPackage>> GetPackagesByMonikerAsync(string Moniker)
        {
            var localcacheDB = new LocalCacheContext(CacheDatabasePath);

            var dbQuery = localcacheDB.ManifestTable.Select(x => new
            {
                x.PackageId,
                x.PublisherId,
                x.YamlUri,
                x.Version,
                x.Moniker,
                x.Name
            }).Where(c => c.Moniker.ToLower().Equals(Moniker.ToLower()));

            var data = await dbQuery.ToListAsync();

            return data
                .GroupBy(x => x.PackageId)
                .OrderBy(x => x.Key)
                .Select(g => new WingetPackage
                {
                    PackageId = g.Key,
                    PublisherId = g.Select(x => x.PublisherId).First(),
                    LatestVersion = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderBy(e => StringToVersion(e.Version)).Last(),
                    Versions = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderBy(e => StringToVersion(e.Version)).ToList(),
                    Name = g.Select(x => x.Name).First(),
                    Moniker = g.Select(x => x.Moniker).First()
                });
        }

        public async static Task<IEnumerable<WingetPackage>> GetPackagesByIdAsync(string PackageId)
        {
            
            var localcacheDB = new LocalCacheContext(CacheDatabasePath);
            var dbQuery = localcacheDB.ManifestTable.Select(x => new
            {
                x.PackageId,
                x.PublisherId,
                x.YamlUri,
                x.Version,
                x.Moniker,
                x.Name
            }).Where(c => (c.PackageId.ToLower().Equals(PackageId.ToLower()) || (PackageId == "*")));

            var data = await dbQuery.ToListAsync();

            return data
                .GroupBy(x => x.PackageId)
                .OrderBy(x => x.Key)
                .Select(g => new WingetPackage
                {
                    PackageId = g.Key,
                    PublisherId = g.Select(x => x.PublisherId).First(),
                    LatestVersion = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderBy(e => StringToVersion(e.Version)).Last(),
                    Versions = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderBy(e => StringToVersion(e.Version)).ToList(),
                    Name = g.Select(x => x.Name).First(),
                    Moniker = g.Select(x => x.Moniker).First()
                });
        }

        public async static Task<IEnumerable<WingetPackage>> GetPackagesByPublisherIdAsync(string PublisherId)
        {
            var localcacheDB = new LocalCacheContext(CacheDatabasePath);
            var dbQuery = localcacheDB.ManifestTable.Select(x => new
            {
                x.PackageId,
                x.PublisherId,
                x.YamlUri,
                x.Version,
                x.Moniker,
                x.Name
            }).Where(c => (c.PublisherId.ToLower().Equals(PublisherId.ToLower()) || (PublisherId == "*")));

            var data = await dbQuery.ToListAsync();

            return data
                .GroupBy(x => x.PackageId)
                .OrderBy(x => x.Key)
                .Select(g => new WingetPackage
                {
                    PackageId = g.Key,
                    PublisherId = g.Select(x => x.PublisherId).First(),
                    LatestVersion = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderBy(e => StringToVersion(e.Version)).Last(),
                    Versions = g.Select(x => new PackageVersion { Version = x.Version, YamlUri = x.YamlUri }).OrderBy(e => StringToVersion(e.Version)).ToList(),
                    Name = g.Select(x => x.Name).First(),
                    Moniker = g.Select(x => x.Moniker).First()
                });
        }

        public static async Task<YamlManifest> GetManifestAsync(string yamlLink, bool UseDefaultWebProxy)
        {
            try
            {
                var client = new WebClient();
                if (UseDefaultWebProxy)
                {
                    IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                    defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                    client.Proxy = defaultWebProxy;
                }
                client.Headers["User-Agent"] = WingetBridge.DefaultUserAgent;

                var responseString = await client.DownloadStringTaskAsync(new System.Uri(WingetCacheUrl+yamlLink));

                if (!string.IsNullOrEmpty(responseString))
                {
                    var deserializer = new DeserializerBuilder()
                                                            .WithNamingConvention(PascalCaseNamingConvention.Instance)
                                                            .IgnoreUnmatchedProperties()
                                                            .Build();
                    var result = deserializer.Deserialize<YamlManifest>(responseString);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            return null;
        }

        /// <summary>
        /// Configure EntityFramework to use SQLite
        /// </summary>
        /// 
        #region SQLite Configuration for EntityFramework
        class SQLiteProviderInvariantName : IProviderInvariantName
        {
            public static readonly SQLiteProviderInvariantName Instance = new SQLiteProviderInvariantName();
            private SQLiteProviderInvariantName() { }
            public const string ProviderName = "System.Data.SQLite.EF6";
            public string Name { get { return ProviderName; } }
        }

        class SQLiteDbDependencyResolver : IDbDependencyResolver
        {
            public object GetService(Type type, object key)
            {
                if (type == typeof(IProviderInvariantName))
                {
                    if (key is SQLiteProviderFactory || key is SQLiteFactory)
                        return SQLiteProviderInvariantName.Instance;
                }
                return null;
            }

            public IEnumerable<object> GetServices(Type type, object key)
            {
                var service = GetService(type, key);
                if (service != null) yield return service;
            }
        }

        class MyDbConfiguration : DbConfiguration
        {
            public MyDbConfiguration()
            {
                SetProviderFactory(SQLiteProviderInvariantName.ProviderName, SQLiteProviderFactory.Instance);
                SetProviderServices(SQLiteProviderInvariantName.ProviderName, (DbProviderServices)SQLiteProviderFactory.Instance.GetService(typeof(DbProviderServices)));
                AddDependencyResolver(new SQLiteDbDependencyResolver());
            }
        }
        #endregion
    }
}
