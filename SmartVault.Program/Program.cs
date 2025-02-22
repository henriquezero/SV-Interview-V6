using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartVault.Program
{
    public partial class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            await new DataGenerator().RunDataGeneration();

            using (var connection = GetDatabaseConnection())
                new ThirdFileChecker(connection).WriteEveryThirdFileToFile(args[0]);

            using (var connection = GetDatabaseConnection())
                new FileSizeCalculator(connection).GetAllFileSizes();
        }

        private static IConfiguration LoadConfiguration()
        {
            try
            {
                var baseDir = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
                if (baseDir == null)
                {
                    throw new Exception("Could not determine base directory.");
                }

                var configPath = Path.Combine(baseDir, "SmartVault.DataGeneration", "appsettings.json");

                return new ConfigurationBuilder()
                    .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                    .Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                return null;
            }
        }

        private static SQLiteConnection GetDatabaseConnection()
        {
            var config = LoadConfiguration();
            if (config == null)
            {
                throw new Exception("Configuration could not be loaded.");
            }

            string databaseFileName = config["DatabaseFileName"];
            string connectionString = string.Format(config["ConnectionStrings:DefaultConnection"] ?? "", databaseFileName);

            if (!File.Exists(databaseFileName))
            {
                throw new FileNotFoundException($"Database file not found: {databaseFileName}");
            }

            return new SQLiteConnection(connectionString);
        }
    }
}