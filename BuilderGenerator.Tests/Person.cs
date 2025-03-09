namespace BuilderGenerator.Sample.Tests
{

    namespace BuilderGenerator.Sample
    {
        [GenerateBuilder]
        public class Person : IPerson
        {
            public string? Name { get; set; }
            public int Age { get; set; }
            public Address? Address { get; set; }
        }
    }
}
