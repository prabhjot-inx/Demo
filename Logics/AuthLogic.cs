using System;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Demo.InputTypes;
using Demo.Data.Entities;
namespace Demo.Logics
{
  public class AuthLogic: IAuthLogic
  {
    private readonly AuthContext _authContext;

    public AuthLogic(AuthContext authContext)
    {
      _authContext = authContext;
    }
    private string RegistrationValidations(RegisterInputType registerInput) 
    {

      if (string.IsNullOrEmpty(registerInput.EmailAddress)) 
      {
        return "Email is required!";
      }

      if (string.IsNullOrEmpty(registerInput.Password) || string.IsNullOrEmpty(registerInput.ConfirmPassword))
      {
        return "Password and Confirm Password are required!";
      }

      if (registerInput.Password != registerInput.ConfirmPassword)
      {
        return "Password doesn't match with confirm password";
      }

      string emailRules = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
      if (!Regex.IsMatch(registerInput.EmailAddress, emailRules)) {
        return "Email address is invalid!";
      }

      // atleast one lower case letter
      // atleast one upper case letter
      // atleast one special character
      // atleast one number
      // atleast 8 character length
      string passwordRules = @"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$";
      if (!Regex.IsMatch(registerInput.Password, passwordRules))
      {
        return "Weak password! Password should contain atleast one lower case letter, upper case letter, special character, number and should not less than 8.";
      }

      return string.Empty;
    }

    private string PasswordHash(string password)
    {
      byte[] salt;
      new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
      var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
      byte[] hash = pbkdf2.GetBytes(20);

      byte[] hashBytes = new byte[36];
      Array.Copy(salt, 0, hashBytes, 0, 16);
      Array.Copy(hash, 0, hashBytes,16, 20);
      return Convert.ToBase64String(hashBytes);
    }

    public string Register(RegisterInputType registerInput)
    {
      string errorMessage = this.RegistrationValidations(registerInput);

      if (!string.IsNullOrEmpty(errorMessage)) {
        return errorMessage;
      }
      
      var newUser = new User
      {
        FirstName = registerInput.FirstName,
        LastName = registerInput.LastName,
        EmailAddress = registerInput.EmailAddress,
        Password = registerInput.Password
      };

      _authContext.User.Add(newUser);
      _authContext.SaveChanges();

      var newUserRole = new UserRoles
      {
        Name = "admin",
        UserId = newUser.UserId
      };

      _authContext.UserRoles.Add(newUserRole);
      _authContext.SaveChanges();

      return "Registration Success";
    }
  
  }
}
