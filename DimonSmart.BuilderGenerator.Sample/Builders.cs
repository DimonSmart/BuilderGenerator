using DimonSmart.BuilderGenerator.Runtime;

namespace DimonSmart.BuilderGenerator.Sample
{
    public interface IPerson
    { }


    [GenerateBuilder]
    public class Person : IPerson
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public Address? Address { get; set; }
    }

    [GenerateBuilder]
    public class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public IPerson? Person { get; set; }
    }
}