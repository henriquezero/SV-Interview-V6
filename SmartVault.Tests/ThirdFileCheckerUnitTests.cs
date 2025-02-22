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
    public class ThirdFileCheckerUnitTests
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

            var thirdFileChecker = new ThirdFileChecker(connection);
            var exception = Record.Exception(() => thirdFileChecker.WriteEveryThirdFileToFile("1"));

            Assert.Null(exception);
        }

        [Fact]
        public void WriteEveryThirdFileToFile_ShouldFail()
        {
            var connection = new SQLiteConnection("Data Source=:memory:;Version=3;");
            connection.Open();
            connection.Execute("CREATE TABLE Document (FilePath TEXT, AccountId INTEGER)");

            var thirdFileChecker = new ThirdFileChecker(connection);

            Assert.Throws<FileNotFoundException>(() => thirdFileChecker.WriteEveryThirdFileToFile("1"));
        }
    }
}
