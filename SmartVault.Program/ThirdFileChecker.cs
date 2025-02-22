using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace SmartVault.Program
{
    public class ThirdFileChecker
    {
        private readonly SQLiteConnection _connection;

        public ThirdFileChecker(SQLiteConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        public void WriteEveryThirdFileToFile(string accountId)
        {
            if (_connection.State != System.Data.ConnectionState.Open)
                _connection.Open();

            var files = _connection.Query<string>("SELECT FilePath FROM Document WHERE AccountId = @AccountId",
                new { AccountId = accountId }).ToList();

            if (files.Count == 0)
            {
                throw new FileNotFoundException($"No files found for account {accountId}.");
            }

            var outputContent = new List<string>();

            for (int i = 2; i < files.Count; i += 3)
            {
                string filePath = files[i];

                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);

                    if (content.Contains("Smith Property"))
                    {
                        outputContent.Add($"---- File content from {filePath} ----\n{content}\n");
                    }
                }
            }

            if (outputContent.Any())
            {
                Console.WriteLine("Matching files found:\n");
                Console.WriteLine(string.Join("\n", outputContent));
            }
            else
            {
                Console.WriteLine("No valid files found containing 'Smith Property'.");
            }
        }
    }
}
