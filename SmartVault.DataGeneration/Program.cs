using Dapper;
using Microsoft.Extensions.Configuration;
using SmartVault.Library;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SmartVault.DataGeneration
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var baseDir = Directory.GetParent(AppContext.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
            var configPath = Path.Combine(baseDir, "SmartVault.DataGeneration", "appsettings.json");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(configPath, optional: false, reloadOnChange: true)
                .Build();

            SQLiteConnection.CreateFile(configuration["DatabaseFileName"]);

            var content = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                content.AppendLine("This is my test document");
            }

            File.WriteAllText("TestDoc.txt", content.ToString());

            using (var connection = new SQLiteConnection(string.Format(configuration?["ConnectionStrings:DefaultConnection"] ?? "", configuration?["DatabaseFileName"])))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    var files = Directory.GetFiles(@"..\..\..\..\BusinessObjectSchema");
                    var serializer = new XmlSerializer(typeof(BusinessObject));

                    foreach (var file in files.Take(4))
                    {
                        var businessObject = serializer.Deserialize(new StreamReader(file)) as BusinessObject;
                        connection.Execute(businessObject?.Script, transaction: transaction);
                    }

                    var userCmd = connection.CreateCommand();
                    var accountCmd = connection.CreateCommand();
                    var documentCmd = connection.CreateCommand();

                    userCmd.CommandText = "INSERT INTO User (Id, FirstName, LastName, DateOfBirth, AccountId, Username, Password, CreatedOn) VALUES(@Id, @FirstName, @LastName, @DateOfBirth, @AccountId, @Username, @Password, @CreatedOn)";
                    accountCmd.CommandText = "INSERT INTO Account (Id, Name, CreatedOn) VALUES(@Id, @Name, @CreatedOn)";
                    documentCmd.CommandText = "INSERT INTO Document (Id, Name, FilePath, Length, AccountId, CreatedOn) VALUES(@Id, @Name, @FilePath, @Length, @AccountId, @CreatedOn)";

                    var documentPath = new FileInfo("TestDoc.txt").FullName;
                    var fileLength = new FileInfo(documentPath).Length;


                    const int batchSize = 1000;
                    var documentNumber = 0;

                    for (int i = 0; i < 100; i++)
                    {
                        userCmd.Parameters.AddWithValue("@Id", i);
                        userCmd.Parameters.AddWithValue("@FirstName", $"FName{i}");
                        userCmd.Parameters.AddWithValue("@LastName", $"LName{i}");
                        userCmd.Parameters.AddWithValue("@DateOfBirth", RandomDateOfBirth());
                        userCmd.Parameters.AddWithValue("@AccountId", i);
                        userCmd.Parameters.AddWithValue("@Username", $"UserName-{i}");
                        userCmd.Parameters.AddWithValue("@Password", "e10adc3949ba59abbe56e057f20f883e");
                        userCmd.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow);
                        userCmd.ExecuteNonQuery();

                        accountCmd.Parameters.AddWithValue("@Id", i);
                        accountCmd.Parameters.AddWithValue("@Name", $"Account{i}");
                        accountCmd.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow);
                        accountCmd.ExecuteNonQuery();

                        for (int d = 0; d < 10000; d += batchSize)
                        {
                            var batch = new List<string>();
                            for (int b = 0; b < batchSize && d + b < 10000; b++)
                            {
                                documentCmd.Parameters.Clear();
                                documentCmd.Parameters.AddWithValue("@Id", documentNumber);
                                documentCmd.Parameters.AddWithValue("@Name", $"Document{i}-{d + b}.txt");
                                documentCmd.Parameters.AddWithValue("@FilePath", documentPath);
                                documentCmd.Parameters.AddWithValue("@Length", fileLength);
                                documentCmd.Parameters.AddWithValue("@AccountId", i);
                                documentCmd.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow);
                                documentCmd.ExecuteNonQuery();
                                documentNumber++;
                            }
                        }
                    }

                    var oauthCmd = connection.CreateCommand();
                    oauthCmd.CommandText = "INSERT INTO OAuth (Provider, ClientId, ClientSecret, AccessToken, RefreshToken, ExpiresAt) VALUES (@Provider, @ClientId, @ClientSecret, @AccessToken, @RefreshToken, @ExpiresAt)";

                    oauthCmd.Parameters.AddWithValue("@Provider", "Microsoft");
                    oauthCmd.Parameters.AddWithValue("@ClientId", Guid.NewGuid().ToString());
                    oauthCmd.Parameters.AddWithValue("@ClientSecret", Guid.NewGuid().ToString());
                    oauthCmd.Parameters.AddWithValue("@AccessToken", "access-token");
                    oauthCmd.Parameters.AddWithValue("@RefreshToken", "refresh-token");
                    oauthCmd.Parameters.AddWithValue("@ExpiresAt", DateTime.UtcNow.AddHours(1));
                    oauthCmd.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow);

                    oauthCmd.ExecuteNonQuery();


                    transaction.Commit();
                }

                var counts = connection.Query<int>(@"
                    SELECT COUNT(*) FROM Account
                    UNION ALL
                    SELECT COUNT(*) FROM Document
                    UNION ALL
                    SELECT COUNT(*) FROM User;
                ").ToList();

                Console.WriteLine($"AccountCount: {counts[0]}");
                Console.WriteLine($"DocumentCount: {counts[1]}");
                Console.WriteLine($"UserCount: {counts[2]}");
            }
        }

        private static string RandomDateOfBirth()
        {
            var random = new Random();
            var start = new DateTime(1985, 1, 1);
            var range = (DateTime.Today - start).Days;

            var dateOfBirth = start.AddDays(random.Next(range));

            return dateOfBirth.ToString("yyyy-MM-dd");
        }
    }
}