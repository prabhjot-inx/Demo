using Demo.InputTypes;

namespace Demo.Logics
{
  public interface IAuthLogic
  {
    string Register(RegisterInputType registerInput);
    string Login(LoginInputType loginInput);
  }
}