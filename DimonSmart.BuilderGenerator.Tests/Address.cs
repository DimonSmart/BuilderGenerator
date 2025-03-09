using BuilderGenerator.Runtime;

namespace BuilderGenerator.Sample.Tests
{

    namespace BuilderGenerator.Sample
    {
        [GenerateBuilder]
        public class Address
        {
            public string? Street { get; set; }
            public string? City { get; set; }
            public IPerson? Person { get; set; }
        }
    }
}
