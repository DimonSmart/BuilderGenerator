using BuilderGenerator.Runtime;

namespace DimonSmart.BuilderGenerator.Tests
{
    [GenerateBuilder]
    public class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public IPerson? Person { get; set; }
    }
}
