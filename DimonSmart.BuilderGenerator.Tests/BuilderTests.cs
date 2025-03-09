namespace DimonSmart.BuilderGenerator.Tests
{
    public class BuilderTests
    {
        [Fact]
        public void CreatePerson_WithAllPropertiesAndAddress_SetsValuesCorrectly()
        {
            var person = PersonBuilder.Create()
                .Age(30)
                .Name("John Doe")
                .Address(address => address
                    .Street("Main St")
                    .City("Springfield")
                    .BuildAndSetParent(i => i.Person))
                .Build();

            Assert.Equal("John Doe", person.Name);
            Assert.Equal(30, person.Age);
            Assert.NotNull(person.Address);
            Assert.Equal("Main St", person.Address.Street);
            Assert.Equal("Springfield", person.Address.City);
            Assert.Same(person, person.Address.Person);
        }

        [Fact]
        public void CreatePerson_WithoutAddress_AddressIsNull()
        {
            var person = PersonBuilder.Create()
                .Age(25)
                .Name("Alice")
                .Build();

            Assert.Equal("Alice", person.Name);
            Assert.Equal(25, person.Age);
            Assert.Null(person.Address);
        }

        [Fact]
        public void CreateAddress_WithoutPerson_PersonIsNull()
        {
            var address = AddressBuilder.Create()
                .Street("Elm St")
                .City("Gotham")
                .Build();

            Assert.Equal("Elm St", address.Street);
            Assert.Equal("Gotham", address.City);
            Assert.Null(address.Person);
        }

        [Fact]
        public void CreatePerson_WithAddressButWithoutSettingParent_PersonInAddressIsNull()
        {
            var person = PersonBuilder.Create()
                .Name("Bob")
                .Age(40)
                .Address(address => address
                    .Street("Pine St")
                    .City("Metropolis"))
                .Build();

            Assert.Equal("Bob", person.Name);
            Assert.Equal(40, person.Age);
            Assert.NotNull(person.Address);
            Assert.Equal("Pine St", person.Address.Street);
            Assert.Equal("Metropolis", person.Address.City);
            Assert.Null(person.Address.Person);
        }

        [Fact]
        public void CreatePerson_PropertyOrderDoesNotAffectResult()
        {
            var person = PersonBuilder.Create()
                .Address(address => address
                    .City("Star City")
                    .Street("Oak St")
                    .BuildAndSetParent(i => i.Person))
                .Name("Oliver Queen")
                .Age(35)
                .Build();

            Assert.Equal("Oliver Queen", person.Name);
            Assert.Equal(35, person.Age);
            Assert.NotNull(person.Address);
            Assert.Equal("Oak St", person.Address.Street);
            Assert.Equal("Star City", person.Address.City);
            Assert.Same(person, person.Address.Person);
        }
    }
}
