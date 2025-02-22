using SmartVault.Program;

namespace SmartVault.Tests
{
    public class DataGenerationUnitTests
    {
        [Fact]
        public async Task RunDataGeneration_ShouldCompleteWithoutErrors()
        {
            var dataGenerator = new DataGenerator();
            var exception = await Record.ExceptionAsync(() => dataGenerator.RunDataGeneration());

            Assert.Null(exception);
        }

        [Fact]
        public async Task RunDataGeneration_ShouldThrowException_WhenDirectoryNotFound()
        {
            var dataGenerator = new DataGenerator(); // 🔹 Cada teste tem sua instância
            dataGenerator.SolutionDirectory = "C:\\Invalid\\Path\\That\\Does\\Not\\Exist";

            var exception = await Record.ExceptionAsync(() => dataGenerator.RunDataGeneration());

            Assert.NotNull(exception);
            Assert.IsType<DirectoryNotFoundException>(exception);
        }
    }
}
