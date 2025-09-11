using Ereoz.Abstractions.Serialization;

namespace Ereoz.DataStorage.Tests.Sources
{
    public class SomeStoredData : StoredState
    {
        public SomeStoredData() { }

        public SomeStoredData(IStringSerializer serializer) : base(serializer) { }
        public SomeStoredData(IBinarySerializer serializer) : base(serializer) { }

        public string? Name { get; set; }
        public string? Age { get; set; }

        internal bool? IsLoadComplete { get; set; } = null;

        protected override void LoadComplete()
        {
            IsLoadComplete = true;
        }

        protected override void LoadFail()
        {
            IsLoadComplete = false;
        }
    }
}
