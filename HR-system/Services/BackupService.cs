using HR_system.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System.IO.Compression;

namespace HR_system.Services
{
    public class BackupService : IBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupService> _logger;
        private readonly string _backupFolder;

        public BackupService(IConfiguration configuration, ILogger<BackupService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            if (OperatingSystem.IsWindows())
            {
                _backupFolder = @"D:\HR-System-Backup";
                if (!Directory.Exists(_backupFolder))
                    Directory.CreateDirectory(_backupFolder);
            }
            else
            {
                _backupFolder = "/var/opt/mssql/data";
            }
        }

        public string GetBackupFolder() => _backupFolder;

        public async Task<BackupResult> CreateBackupAsync()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return new BackupResult { Success = false, Message = "Connection string not found" };

            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFileName = $"HR_Backup_{timestamp}.bak";
            var backupPath = Path.Combine(_backupFolder, backupFileName);

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var backupQuery = $@"BACKUP DATABASE [{databaseName}] TO DISK = N'{backupPath}' 
                    WITH FORMAT, INIT, COMPRESSION, NAME = N'HR System Backup - {timestamp}'";

                using var command = new SqlCommand(backupQuery, connection);
                command.CommandTimeout = 600;
                await command.ExecuteNonQueryAsync();

                string finalPath = backupPath;
                if (OperatingSystem.IsWindows())
                    finalPath = await CompressBackupAsync(backupPath);

                return new BackupResult { Success = true, Message = "تم إنشاء النسخة الاحتياطية بنجاح", FilePath = finalPath };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                return new BackupResult { Success = false, Message = $"خطأ في إنشاء النسخة الاحتياطية: {ex.Message}" };
            }
        }

        public async Task<BackupResult> RestoreBackupAsync(string backupPath)
        {
            if (!File.Exists(backupPath))
                return new BackupResult { Success = false, Message = "ملف النسخة الاحتياطية غير موجود" };

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            string actualBackupPath = backupPath;
            bool isCompressed = backupPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase);

            try
            {
                if (isCompressed)
                    actualBackupPath = await DecompressBackupAsync(backupPath);

                builder.InitialCatalog = "master";
                using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync();

                using (var cmd = new SqlCommand($@"DECLARE @kill varchar(8000) = '';
                    SELECT @kill = @kill + 'KILL ' + CONVERT(varchar(5), session_id) + ';'
                    FROM sys.dm_exec_sessions WHERE database_id = DB_ID('{databaseName}') AND session_id <> @@SPID;
                    EXEC(@kill);", connection)) { await cmd.ExecuteNonQueryAsync(); }

                using (var cmd = new SqlCommand($"ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", connection)) 
                { await cmd.ExecuteNonQueryAsync(); }

                using (var cmd = new SqlCommand($"RESTORE DATABASE [{databaseName}] FROM DISK = N'{actualBackupPath}' WITH REPLACE", connection))
                { cmd.CommandTimeout = 1200; await cmd.ExecuteNonQueryAsync(); }

                using (var cmd = new SqlCommand($"ALTER DATABASE [{databaseName}] SET MULTI_USER;", connection)) 
                { await cmd.ExecuteNonQueryAsync(); }

                if (isCompressed && actualBackupPath != backupPath && File.Exists(actualBackupPath))
                    File.Delete(actualBackupPath);

                return new BackupResult { Success = true, Message = "تم استعادة قاعدة البيانات بنجاح" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup");
                return new BackupResult { Success = false, Message = $"خطأ في استعادة قاعدة البيانات: {ex.Message}" };
            }
        }

        public Task<List<BackupInfo>> GetBackupsAsync()
        {
            var backups = new List<BackupInfo>();
            if (!Directory.Exists(_backupFolder)) return Task.FromResult(backups);

            var files = Directory.GetFiles(_backupFolder, "HR_Backup_*.zip")
                .Concat(Directory.GetFiles(_backupFolder, "HR_Backup_*.bak"));

            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    backups.Add(new BackupInfo
                    {
                        FileName = fileInfo.Name,
                        FilePath = fileInfo.FullName,
                        CreatedAt = fileInfo.CreationTime,
                        SizeInBytes = fileInfo.Length
                    });
                }
                catch { }
            }
            return Task.FromResult(backups.OrderByDescending(b => b.CreatedAt).ToList());
        }

        public Task<bool> DeleteBackupAsync(string backupPath)
        {
            try
            {
                if (File.Exists(backupPath)) { File.Delete(backupPath); return Task.FromResult(true); }
                return Task.FromResult(false);
            }
            catch { return Task.FromResult(false); }
        }

        private async Task<string> CompressBackupAsync(string backupPath)
        {
            var compressedPath = Path.ChangeExtension(backupPath, ".zip");
            try
            {
                await Task.Run(() =>
                {
                    using var archive = ZipFile.Open(compressedPath, ZipArchiveMode.Create);
                    archive.CreateEntryFromFile(backupPath, Path.GetFileName(backupPath), CompressionLevel.Optimal);
                });
                File.Delete(backupPath);
                return compressedPath;
            }
            catch { return backupPath; }
        }

        private async Task<string> DecompressBackupAsync(string zipPath)
        {
            var extractFolder = Path.GetDirectoryName(zipPath) ?? _backupFolder;
            var tempFolder = Path.Combine(extractFolder, $"temp_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempFolder);
            await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, tempFolder));
            var bakFile = Directory.GetFiles(tempFolder, "*.bak").FirstOrDefault() 
                ?? throw new Exception("No .bak file found");
            var destinationPath = Path.Combine(extractFolder, $"temp_restore_{Guid.NewGuid():N}.bak");
            File.Move(bakFile, destinationPath);
            Directory.Delete(tempFolder, true);
            return destinationPath;
        }
    }
}
