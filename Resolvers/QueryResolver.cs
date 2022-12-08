using Demo.Logics;
using Demo.InputTypes;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;

namespace Demo.Resolvers
{
  public class QueryResolver
  {
    // [Authorize(Policy="claim-policy-1")]
    // [Authorize(Roles = new[] {"admin"})]
    // [Authorize(Policy="roles-policy")]
    [Authorize]
    public string Welcome()
    {
      return "Welcome to the world of magic";
    }
  }
}