using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Demo.InputTypes;
using Demo.Data.Entities;
using Demo.Shared;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace Demo.Logics
{
  public class AuthLogic: IAuthLogic
  {
    private readonly AuthContext _authContext;
    private readonly TokenSettings _tokenSettings;

    public AuthLogic(AuthContext authContext, IOptions<TokenSettings> tokenSettings)
    {
      _authContext = authContext;
      _tokenSettings = tokenSettings.Value;
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

    private bool ValidatePasswordHash(string password, string dbPassword) {
      byte[] hashBytes = Convert.FromBase64String(dbPassword);

      byte[] salt = new Byte[16];
      Array.Copy(hashBytes, 0, salt, 0, 16 );

      var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);

      byte[] hash = pbkdf2.GetBytes(20);

      for (int i = 0; i<20; i++) 
      {
        if (hash[i] != hashBytes[i + 16])
        {
          return false;
        }
      }
      return true;
    }

    private string getJWTAuthKey(User user, List<UserRoles> roles) 
    {
      var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Key));
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var claims = new List<Claim>();

      claims.Add(new Claim("Email", user.EmailAddress));
      claims.Add(new Claim("LastName", user.LastName));

      if ((roles?.Count ?? 0) > 0) 
      {
        foreach (var role in roles) 
        {
          claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }
      }

      var jwtSecurityToken = new JwtSecurityToken(
        issuer: _tokenSettings.Issuer,
        audience: _tokenSettings.Audience,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: credentials,
        claims: claims
      );

      return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

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
        Password = this.PasswordHash(registerInput.Password)
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
    public string Login(LoginInputType loginInput) {
      if (string.IsNullOrEmpty(loginInput.Email) || string.IsNullOrEmpty(loginInput.Password))
      {
        return "Invalid Credentials";
      }

      var user = _authContext.User.Where(_ => _.EmailAddress == loginInput.Email).FirstOrDefault();

      if (user == null) {
        return "Invalid Credentials";
      }

      if (!ValidatePasswordHash(loginInput.Password, user.Password))
      {
        return "Invalid Credentials";
      }

      var roles = _authContext.UserRoles.Where(_ => _.UserId == user.UserId).ToList();
      
      return getJWTAuthKey(user, roles);

    }
  }
}
