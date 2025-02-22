using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartVault.Program
{
    public class DataGenerator
    {
        public string SolutionDirectory { get; set; }

        public DataGenerator()
        {
            SolutionDirectory = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.Parent?.FullName;
        }

        public async Task RunDataGeneration()
        {
            if (string.IsNullOrWhiteSpace(SolutionDirectory) || !Directory.Exists(SolutionDirectory))
            {
                throw new DirectoryNotFoundException($"Solution directory not found: {SolutionDirectory}");
            }

            var dataGenerationPath = Path.Combine(SolutionDirectory, "SmartVault.DataGeneration");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{dataGenerationPath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
        }
    }
}
