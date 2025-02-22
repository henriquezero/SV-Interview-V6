using Dapper;
using SmartVault.Program;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartVault.Tests
{
    public class FileSizeCalculatorUnitTests
    {
        [Fact]
        public void WriteEveryThirdFileToFile_ShouldSuccess()
        {

            var connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            connection.Open();
            connection.Execute("CREATE TABLE Document (FilePath TEXT, AccountId INTEGER)");

            connection.Execute("INSERT INTO Document (FilePath, AccountId) VALUES ('file1.txt', 1)");
            connection.Execute("INSERT INTO Document (FilePath, AccountId) VALUES ('file2.txt', 1)");
            connection.Execute("INSERT INTO Document (FilePath, AccountId) VALUES ('file3.txt', 1)");

            var sizeCalculator = new FileSizeCalculator(connection);
            var exception = Record.Exception(() => sizeCalculator.GetAllFileSizes());

            Assert.Null(exception);
        }

        [Fact]
        public void WriteEveryThirdFileToFile_ShouldFail()
        {
            var connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            connection.Open();
            connection.Execute("CREATE TABLE Document (FilePath TEXT, AccountId INTEGER)");

            var sizeCalculator = new FileSizeCalculator(connection);

            Assert.Throws<FileNotFoundException>(() => sizeCalculator.GetAllFileSizes());
        }
    }
}
