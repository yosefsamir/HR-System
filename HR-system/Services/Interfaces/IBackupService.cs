namespace HR_system.Services.Interfaces
{
    public interface IBackupService
    {
        Task<BackupResult> CreateBackupAsync();
        Task<BackupResult> RestoreBackupAsync(string backupPath);
        Task<List<BackupInfo>> GetBackupsAsync();
        Task<bool> DeleteBackupAsync(string backupPath);
        string GetBackupFolder();
    }

    public class BackupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? FilePath { get; set; }
    }

    public class BackupInfo
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public long SizeInBytes { get; set; }
        public string SizeDisplay => FormatSize(SizeInBytes);

        private static string FormatSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }
    }
}
