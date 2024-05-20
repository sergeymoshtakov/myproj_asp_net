namespace myproj.Models
{
    public class Person
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public Person() { }

        public Person(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
