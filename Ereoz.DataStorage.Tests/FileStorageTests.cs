using Ereoz.DataStorage.Tests.Sources;
using Ereoz.Serialization.Json;

namespace Ereoz.DataStorage.Tests
{
    public class FileStorageTests
    {
        private readonly string _tempFolder = "Temp";
        private readonly string _someClassFile = "SomeClassFile.json";

        [Fact]
        public void FileStorage_SaveAndLoad_ShouldEqualObject()
        {
            var fileFullName = Path.Combine(_tempFolder, _someClassFile);

            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);

            Directory.CreateDirectory(_tempFolder);

            var fileStorage = new FileStorage(new SimpleJson());

            var someClass = new SomeClass()
            {
                Name = "John",
                Age = 100
            };

            Assert.False(File.Exists(fileFullName));

            var saveResult = fileStorage.Save(fileFullName, someClass);

            Assert.True(saveResult);
            Assert.True(File.Exists(fileFullName));

            var someClassLoaded = fileStorage.Load<SomeClass>(fileFullName);

            Assert.Equal(someClass.Name, someClassLoaded.Name);
            Assert.Equal(someClass.Age, someClassLoaded.Age);

            Assert.Null(fileStorage.Load<SomeClass>("nonCorrectFile"));

            Directory.Delete(_tempFolder, true);
        }
    }
}