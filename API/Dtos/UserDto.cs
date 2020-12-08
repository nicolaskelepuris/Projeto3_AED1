namespace API.Dtos
{
    public class UserDto{
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsEmployee { get; set; }
    }
}