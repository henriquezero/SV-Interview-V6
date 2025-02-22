using Dapper;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace SmartVault.Program
{
    public class FileSizeCalculator
    {
        private readonly SQLiteConnection _connection;

        public FileSizeCalculator(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public void GetAllFileSizes()
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            var files = _connection.Query<string>("SELECT FilePath FROM Document").ToList();

            if (files.Count == 0)
            {
                throw new FileNotFoundException("No files found in the database.");
            }

            long totalSize = 0;

            foreach (var filePath in files)
            {
                if (File.Exists(filePath))
                {
                    long fileSize = new FileInfo(filePath).Length;
                    totalSize += fileSize;
                }
            }

            Console.WriteLine($"Total actual file size: {totalSize / 1024.0 / 1024.0:F2} MB");
        }
    }
}
