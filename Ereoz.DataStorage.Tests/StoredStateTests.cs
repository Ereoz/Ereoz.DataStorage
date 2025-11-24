using Ereoz.Abstractions.Serialization;
using Ereoz.DataStorage.Tests.Sources;
using Ereoz.Serialization.Json;

namespace Ereoz.DataStorage.Tests
{
    public class StoredStateTests
    {
        private readonly string _fileName = "SomeStoredData.json";

        [Theory]
        [InlineData(typeof(SimpleJson))]
        [InlineData(typeof(NewtonsoftJsonSerializer))]
        [InlineData(typeof(SystemTextJsonSerializer))]
        public void StoredState_SaveAndLoad_ShouldEqualObject(Type serializerType)
        {
            if (File.Exists(_fileName))
                File.Delete(_fileName);

            IStringSerializer serializer = (IStringSerializer)Activator.CreateInstance(serializerType)!;

            var someStoredData1 = new SomeStoredData(serializer!);
            someStoredData1.LoadState();

            Assert.False(someStoredData1.IsLoadComplete);

            someStoredData1.Name = "Tom";
            someStoredData1.Age = "99";

            someStoredData1.SaveState();

            var someStoredData2 = new SomeStoredData(serializer!);
            someStoredData2.LoadState();

            Assert.True(someStoredData2.IsLoadComplete);
            Assert.Equal(someStoredData1.Name, someStoredData2.Name);
            Assert.Equal(someStoredData1.Age, someStoredData2.Age);

            File.Delete(_fileName);
        }
    }
}
