using Demo.Logics;
using Demo.InputTypes;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;

namespace Demo.Resolvers
{
  public class QueryResolver
  {
    [Authorize]
    public string Welcome()
    {
      return "Welcome to the world of magic";
    }
  }
}