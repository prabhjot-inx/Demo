using Microsoft.EntityFrameworkCore;
using Demo.Data;

namespace Demo.Data.Entities
{
  public class AuthContext: DbContext 
  {
    public AuthContext(DbContextOptions<AuthContext> options) : base(options)
    {

    }

    public DbSet<User> User { get; set;}
    public DbSet<UserRoles> UserRoles { get; set;}
  }
}