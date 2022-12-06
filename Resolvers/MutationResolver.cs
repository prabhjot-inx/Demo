using Demo.Logics;
using Demo.InputTypes;
using HotChocolate;

namespace Demo.Resolvers
{
  public class MutationResolver
  {
    public string Register([Service] IAuthLogic authLogic, RegisterInputType registerInput)
    {
      return authLogic.Register(registerInput);
    }
  }
}