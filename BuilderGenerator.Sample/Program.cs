namespace BuilderGenerator.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var person = PersonBuilder.Create()
               .Name("John Doe")
               .Age(30)
               .Address(address => address
                   .Street("Main St")
                   .City("Springfield"))
               .Build();
     
            Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
            Console.WriteLine($"Address: Street: {person.Address?.Street}, City: {person.Address?.City}");
            Console.WriteLine($"Address.Parent is set correctly: {(person.Address?.Person == person ? "Yes" : "No")}");
        }
    }
}