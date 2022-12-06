
namespace Demo.GraphQLC.Users 
{
  public class UserQuery 
  {
    public User GetUser() => 
      new User
      {
        Name = "Prabhjot Singh",
        Email = "prabhjot@insonix.com",
        Password = "*********"
      };
  }
}