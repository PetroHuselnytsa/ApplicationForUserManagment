namespace TestFirstProject.Models
{
    public class Person
    {
        protected Person() { }
        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Age { get; set; }
    }
}
