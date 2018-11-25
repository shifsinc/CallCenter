﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebApi.Models;

namespace WebApi.Services
{
    public interface IAuthService
    {
        TokenDto GetToken(AuthDto authDto);
        TokenDto RefreshToken(Guid refreshToken);
    }
    public class AuthService: IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public TokenDto GetToken(AuthDto authDto)
        {
                var user = RequestService.WebLogin(authDto.Login, authDto.Password);
                if (user == null)
                {
                    return null;
                }
                var now = DateTime.UtcNow;
                return new TokenDto
                {
                    Access = CreateAccessToken(user, now),
                    Refresh = CreateRefreshToken(user, now)
                };
        }

        public TokenDto RefreshToken(Guid refreshToken)
        {
                var userToken = GetUserToken(refreshToken);
                if (userToken == null)
                {
                    return null;
                }
                if (userToken.ExpirationDate < DateTime.UtcNow)
                {
                    DeleteRefreshToken(userToken);
                    return null;
                }
                var now = DateTime.UtcNow;
                return new TokenDto
                {
                    Access = CreateAccessToken(userToken.User, now),
                    Refresh = UpdateRefreshToken(userToken, now)
                };
        }

        private User GetUser(AuthDto authDto)
        {
            var passwordHash = EncodeMD5(authDto.Password);
            try
            {
                var user = new User() {Id = 1, Login = "asdsa", PasswordHash = "sad"};
                return user;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogCritical(e, "Found more than one user by '{Login}' login", authDto.Login);
                return null;
            }
        }

        private UserToken GetUserToken(Guid refreshToken)
        {
            try
            {
                var userToken = new UserToken
                {
                    Token = Guid.NewGuid(),
                    ExpirationDate = DateTime.Now.AddDays(1000),
                   // User = new User() {Id = 1, Login = "asdsa",PasswordHash = "sad"}
                };
                return userToken;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogCritical(e, "Found more than one users's token by '{RefreshToken}'", refreshToken);
                return null;
            }
        }

        private string CreateAccessToken(WebUserDto user, DateTime start)
        {
            //todo: добавить роли
            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim("Login", user.Login??""),
                new Claim("SurName", user.SurName??""),
                new Claim("FirstName", user.FirstName??""),
                new Claim("PatrName", user.PatrName??""),
                new Claim("CanCreateRequestInWeb", user.CanCreateRequestInWeb.ToString()),
                new Claim("AllowStatistics", user.AllowStatistics.ToString()),
                new Claim("CanSetRating", user.CanSetRating.ToString()),
                new Claim("CanCloseRequest", user.CanCloseRequest.ToString()),
                 new Claim("WorkerId", user.WorkerId.ToString()),
                 new Claim("CanChangeExecutors", user.CanChangeExecutors.ToString()),
                 new Claim("ServiceCompanyFilter", user.ServiceCompanyFilter.ToString()),
            };
            var jwt = new JwtSecurityToken(
                issuer: _configuration["Auth:Issuer"],
                notBefore: start,
                claims: claims,
                expires: start.Add(TimeSpan.FromMinutes(_configuration.GetValue<int>("Auth:AccessExpireMinutes"))),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.Default.GetBytes(_configuration["Auth:Key"])), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        
        private void UpdateRefreshTokenAttributes(UserToken userToken, DateTime start)
        {
            userToken.ExpirationDate = start.Add(TimeSpan.FromDays(_configuration.GetValue<int>("Auth:RefreshExpireDays")));
        }

        private Guid CreateRefreshToken(WebUserDto user, DateTime start)
        {
            var userToken = new UserToken
            {
                Token = Guid.NewGuid(),
                User = user
            };
            UpdateRefreshTokenAttributes(userToken, start);
            return userToken.Token;
        }

        private Guid UpdateRefreshToken(UserToken userToken, DateTime start)
        {
            UpdateRefreshTokenAttributes(userToken, start);
            return userToken.Token;
        }

        private void DeleteRefreshToken(UserToken userToken)
        {

        }
        /**/
        private static string EncodeMD5(string value)
        {
            var hash = MD5.Create().ComputeHash(Encoding.GetEncoding(1251).GetBytes(value));
            return string.Concat(Array.ConvertAll(hash, ToHex));
        }

        private static string ToHex(byte value)
        {
            var result = $"{value:X}";
            while (result.Length < 2)
            {
                result = "0" + result;
            }
            return result;
        }
    }
}
