using BuilderGenerator.Runtime;

namespace BuilderGenerator.Sample
{
    [GenerateBuilder]
    public class Person
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
        public Person? Person { get; set; }
    }
}