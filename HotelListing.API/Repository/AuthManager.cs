﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames =
  Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace HotelListing.API.Repository;

public class AuthManager : IAuthManager
{
  private const string _loginProvider = "HotelListingApi";
  private const string _refreshToken = "RefreshToken";
  private readonly IConfiguration _configuration;
  private readonly IMapper _mapper;
  private readonly UserManager<ApiUser> _userManager;

  private ApiUser _user;

  public AuthManager(
    IMapper mapper,
    UserManager<ApiUser> userManager,
    IConfiguration configuration
  )
  {
    _mapper = mapper;
    _userManager = userManager;
    _configuration = configuration;
  }

  public async Task<AuthResponseDto> Login(LoginDto loginDto)
  {
    var _user = await _userManager.FindByEmailAsync(loginDto.Email);
    var isValidUser =
      await _userManager.CheckPasswordAsync(_user, loginDto.Password);

    if (_user == null || isValidUser == false) return null;

    var token = await GenerateToken();

    return new AuthResponseDto
    {
      Token = token,
      UserId = _user.Id,
      RefreshToken = await CreateRefreshToken()
    };
  }

  public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
  {
    var _user = _mapper.Map<ApiUser>(userDto);
    _user.UserName = userDto.Email;

    var result = await _userManager.CreateAsync(_user, userDto.Password);

    if (result.Succeeded) await _userManager.AddToRoleAsync(_user, "User");

    return result.Errors;
  }

  public async Task<string> CreateRefreshToken()
  {
    await _userManager.RemoveAuthenticationTokenAsync(
      _user,
      _loginProvider,
      _refreshToken
    );
    var newRefreshToken = await _userManager.GenerateUserTokenAsync(
      _user,
      _loginProvider,
      _refreshToken
    );
    var result = await _userManager.SetAuthenticationTokenAsync(
      _user,
      _loginProvider,
      _refreshToken,
      newRefreshToken
    );

    return newRefreshToken;
  }

  public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
  {
    var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

    var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
    var username = tokenContent.Claims.ToList()
      .FirstOrDefault(c => c.Type == ClaimTypes.Email)?
      .Value;
    _user = await _userManager.FindByNameAsync(username);

    if (_user == null || _user.Id != request.UserId)
      return null;

    var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(
      _user,
      _loginProvider,
      _refreshToken,
      request.RefreshToken
    );

    if (isValidRefreshToken)
    {
      var token = await GenerateToken();
      return new AuthResponseDto
      {
        Token = token,
        UserId = _user.Id,
        RefreshToken = await CreateRefreshToken()
      };
    }

    await _userManager.UpdateSecurityStampAsync(_user);
    return null;
  }

  private async Task<string> GenerateToken()
  {
    var securityKey = new SymmetricSecurityKey(
      Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"])
    );

    var credentials = new SigningCredentials(
      securityKey,
      SecurityAlgorithms.HmacSha256
    );

    var roles = await _userManager.GetRolesAsync(_user);
    var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();
    var userClaims = await _userManager.GetClaimsAsync(_user);

    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, _user.Email),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.Email, _user.Email),
      new("uid", _user.Id)
    }.Union(userClaims).Union(roleClaims);

    var token = new JwtSecurityToken(
      _configuration["JwtSettings:Issuer"],
      _configuration["JwtSettings:Audience"],
      claims,
      expires: DateTime.Now.AddMinutes(
        Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])
      ),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}