using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Grpc.Core;
using Notes2022.Proto;
using Notes2022.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Notes2022.Server.Data;

namespace Notes2022.Server.Services
{
    public class AuthService : Auth.AuthBase
    {

        private readonly ILogger<AuthService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager; 
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(ILogger<AuthService> logger,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager
          )
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public override async Task<AuthReply> Register(RegisterRequest request, ServerCallContext context)
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
                return new AuthReply() {Status = StatusCodes.Status500InternalServerError, Message = "User already exists!" };

            userExists = await _userManager.FindByNameAsync(request.Username.Replace(" ", "_"));
            if (userExists != null)
                return new AuthReply() { Status = StatusCodes.Status500InternalServerError, Message = "User already exists!" };

            ApplicationUser user = new()
            {
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = request.Username.Replace(" ", "_"),
                DisplayName = request.Username
            };

            try
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return new AuthReply() { Status = StatusCodes.Status500InternalServerError, Message = "User creation failed! Please check user details and try again." };
            }
            catch(Exception ex) 
            { 
                string message = ex.Message;
                return new AuthReply() { Status = StatusCodes.Status500InternalServerError, Message = "User creation failed! Please check user details and try again.  " + ex.InnerException?.Message };

            }

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.User))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.User);
            }

            if (_userManager.Users.Count() == 1)
            {
                if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Admin);
                }
            }

            return new AuthReply() { Status = StatusCodes.Status200OK, Message = "User created!" };
        }

        public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(request.Email.Replace(" ", "_"));
            }

            if (user != null && await _signInManager.CanSignInAsync(user))
            {
                var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: user.AccessFailedCount > _signInManager.Options.Lockout.MaxFailedAccessAttempts);

                if (!(result.Succeeded))
                {
                    await _userManager.AccessFailedAsync(user);
                    //if ( user.AccessFailedCount > _signInManager.Options.Lockout.MaxFailedAccessAttempts )
                    //{
                    //    user.LockoutEnabled = true;
                    //}
                    await _userManager.UpdateAsync(user);
                    return new LoginReply() { Status = StatusCodes.Status500InternalServerError, Message = "User Login failed! Please check user details and try again." };
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                
                if (user.DisplayName == null)
                    user.DisplayName = String.Empty;

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email)
                };

                List<string> roles = new List<string>();
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    roles.Add(userRole);
                }

                var token = GetToken(authClaims);

                JwtSecurityTokenHandler hand = new JwtSecurityTokenHandler();
                string stoken = hand.WriteToken(token);

                UserInfo userInfo = new UserInfo() { Displayname = user.DisplayName, Email = user.Email, Subject = user.Id};
                userInfo.Roles.Add(roles);

                return new LoginReply() { Status = StatusCodes.Status200OK, Message = "Login successful.", Info = userInfo, Jwt = stoken };
            }

            return new LoginReply() { Status = StatusCodes.Status500InternalServerError, Message = "User Login failed! Please check user details and try again." };
        }

        public override async Task<AuthReply> Logout(LogoutRequest request, ServerCallContext context)
        {
            await _signInManager.SignOutAsync();
            return new AuthReply() { Status = StatusCodes.Status200OK, Message = "User logged out!" };
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTAuth:SecretKey"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWTAuth:ValidIssuerURL"],
                audience: _configuration["JWTAuth:ValidAudienceURL"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
