namespace DayLog.Models
{
    public class UserSecurity
    {
        public int Id { get; set; }
        public string PinHash { get; set; }
        public string PasswordHash { get; set; }
    }
}
