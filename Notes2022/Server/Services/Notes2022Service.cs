// ***********************************************************************
// Assembly         : Notes2022.Server
// Author           : Dale Sinder
// Created          : 04-20-2022
//
// Last Modified By : Dale Sinder
// Last Modified On : 05-10-2022
//
// Copyright © 2022, Dale Sinder
//
// Name: Notes2022Service.cs
//
// Description:
//      gRPC service implementation for the app
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 3 as
// published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License version 3 for more details.
//
//  You should have received a copy of the GNU General Public License
//  version 3 along with this program in file "license-gpl-3.0.txt".
//  If not, see<http://www.gnu.org/licenses/gpl-3.0.txt>.
// ***********************************************************************
// <copyright file="Notes2022Service.cs" company="Notes2022.Server">
//     Copyright (c) Dale Sinder. All rights reserved.
// </copyright>
// ***********************************************************************
// <summary></summary>

using Grpc.Core;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Notes2022.Proto;
using Notes2022.Server.Data;
using Notes2022.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Notes2022.Server.Services
{
    /// <summary>
    /// Class Notes2022Service.  Contains all the Notes2022 gRPC service methods/handlers.
    /// Implements the <see cref="Notes2022.Proto.Notes2022Server.Notes2022ServerBase" />
    /// </summary>
    /// <seealso cref="Notes2022.Proto.Notes2022Server.Notes2022ServerBase" />
    public class Notes2022Service : Notes2022Server.Notes2022ServerBase
    {
        /// <summary>
        /// The logger
        /// </summary>
#pragma warning disable IDE0052 // Remove unread private members
        //private readonly ILogger<Notes2022Service> _logger; // not currently used - here if/when needed
#pragma warning restore IDE0052 // Remove unread private members

        /// <summary>
        /// The database
        /// </summary>
        private readonly NotesDbContext _db;

        /// <summary>
        /// The user manager
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// The role manager
        /// </summary>
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// The sign in manager
        /// </summary>
        private readonly SignInManager<ApplicationUser> _signInManager;

        /// <summary>
        /// The email sender
        /// </summary>
        private readonly IEmailSender _emailSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="Notes2022Service" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="db">The database.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="roleManager">The role manager.</param>
        /// <param name="signInManager">The sign in manager.</param>
        /// <param name="emailSender">The email sender.</param>
        /// <param name="userManager">The user manager.</param>
        public Notes2022Service(/*ILogger<Notes2022Service> logger,*/
            NotesDbContext db,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager
          )
        {
            //_logger = logger;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Does nothing.  Just permits a ping of the server.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<NoRequest> NoOp(NoRequest request, ServerCallContext context)
        {
            return new();
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <summary>
        /// Gets the access token for current user in file/archive
        /// </summary>
        /// <param name="request">fileid and arcid</param>
        /// <param name="context">The context.</param>
        /// <returns>Access Token</returns>
        public override async Task<NoteAccess> GetAccess(NoteFileRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArcId);
            return na;
        }

        /// <summary>
        /// Registers the specified User.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>AuthReply.</returns>
        public override async Task<AuthReply> Register(RegisterRequest request, ServerCallContext context)
        {
            ApplicationUser? userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists is not null)
                return new AuthReply() { Status = StatusCodes.Status500InternalServerError, Message = "User already exists!" };

            userExists = await _userManager.FindByNameAsync(request.Username.Replace(" ", "_"));
            if (userExists != null)
                return new AuthReply() { Status = StatusCodes.Status500InternalServerError, Message = "User already exists!" };

            ApplicationUser user = new()
            {
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = request.Username.Replace(" ", "_").Trim(),
                DisplayName = request.Username,
                Ipref2 = 12     // starting note index page size pref.
            };

            try
            {
                IdentityResult? result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return new AuthReply() { Status = StatusCodes.Status500InternalServerError, Message = "User creation failed! Please check user details and try again." };
            }
            catch (Exception ex)
            {
                return new AuthReply() { Status = StatusCodes.Status500InternalServerError, Message = "User creation failed! Please check user details and try again.  " + ex.InnerException?.Message };
            }

            // add roles if they do not exist
            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            // everyone is a user
            if (await _roleManager.RoleExistsAsync(UserRoles.User))
                await _userManager.AddToRoleAsync(user, UserRoles.User);

            // first user to register is assumed to be the starting admin
            if (_userManager.Users.Count() == 1)        // first user is Admin
            {
                if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Admin);
                }
            }

            // send email for user to confirm email address - not not log in until they do
            string? code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            ConfirmEmailRequest mess = new() { UserId = user.Id, Code = code };
            string payload = Globals.Base64Encode(JsonSerializer.Serialize(mess));

            string target = _configuration["AppUrl"] + "/authentication/confirmemail/" + payload;
            await _emailSender.SendEmailAsync(request.Email, "Confirm your email",
                $"Please confirm your Notes 2022 account email by <a href='{target}'>clicking here</a>.  You cannot login until you do this.");

            return new AuthReply() { Status = StatusCodes.Status200OK, Message = "User created!" };
        }

        /// <summary>
        /// Confirms the email account of the user.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>AuthReply.</returns>
        public override async Task<AuthReply> ConfirmEmail(ConfirmEmailRequest request, ServerCallContext context)
        {
            AuthReply ret = new()
            {
                Status = StatusCodes.Status500InternalServerError
            };

            if (request.Code is null || request.UserId is null)
            {
                ret.Message = "Improper request! (null elements)";
                return ret;
            }

            ApplicationUser? user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                ret.Message = "Failed to find user!";
                return ret;
            }

            IdentityResult? result = await _userManager.ConfirmEmailAsync(user, request.Code);
            if (result.Succeeded)
            {
                ret.Status = StatusCodes.Status200OK;
                ret.Message = "Thank you for confirming your email " + user.DisplayName + ".  You may now log in!";
                return ret;
            }

            ret.Message = "Failed to confirm email address.";
            return ret;
        }

        /// <summary>
        /// Logins the specified User.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>LoginReply.</returns>
        public override async Task<LoginReply> Login(LoginRequest request, ServerCallContext context)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                user = await _userManager.FindByNameAsync(request.Email.Replace(" ", "_").Trim());
            }
            if (user is not null && await _signInManager.CanSignInAsync(user))
            {
                if (user.Pref0) // login turned off
                {
                    return new LoginReply() { Status = StatusCodes.Status500InternalServerError, Message = "User Login failed! Login turned off!" };
                }
                SignInResult? result = await _signInManager.PasswordSignInAsync(user, request.Password, request.Hours > 0, lockoutOnFailure: user.AccessFailedCount > _signInManager.Options.Lockout.MaxFailedAccessAttempts);

                if (!(result.Succeeded))
                {
                    await _userManager.AccessFailedAsync(user);
                    await _userManager.UpdateAsync(user);
                    return new LoginReply() { Status = StatusCodes.Status500InternalServerError, Message = "User Login failed! Please check user details and try again later." };
                }

                IList<string>? userRoles = await _userManager.GetRolesAsync(user);

                if (user.DisplayName is null)
                    user.DisplayName = String.Empty;

                List<Claim>? authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email)
                };

                List<string> roles = new();
                foreach (string? userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    roles.Add(userRole);
                }

                JwtSecurityToken? token = GetToken(authClaims, request.Hours);

                JwtSecurityTokenHandler hand = new();
                string stoken = hand.WriteToken(token);

                UserInfo userInfo = new()
                {
                    Displayname = user.DisplayName,
                    Email = user.Email,
                    Subject = user.Id,
                    IsAdmin = roles.Contains("Admin"),
                    IsUser = roles.Contains("User")
                };

                if (userInfo.IsAdmin)
                    userInfo.Hangfire = Globals.HangfireAddress;

                return new LoginReply() { Status = StatusCodes.Status200OK, Message = "Login successful.", Info = userInfo, Jwt = stoken };
            }

            return new LoginReply() { Status = StatusCodes.Status500InternalServerError, Message = "User Login failed! Please check user details and try again." };
        }

        [Authorize] // IT IS VITAL THAT THIS IS AUTHORIZED HERE!  
        public override async Task<NoRequest> ReLogin(NoRequest request, ServerCallContext context)
        {
            ApplicationUser user = await GetAppUser(context);

            await _signInManager.SignInAsync(user, isPersistent: true);

            return new();
        }

        public override async Task<AuthReply> ResendEmail(AString request, ServerCallContext context)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(request.Val);
            if (user is null)
            {
                user = await _userManager.FindByNameAsync(request.Val);
            }
            if (user is null)
                return new() { Status = 500, Message = "Somthing went wrong!" };

            // send email for user to confirm email address - not not log in until they do
            string? code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            ConfirmEmailRequest mess = new() { UserId = user.Id, Code = code };
            string payload = Globals.Base64Encode(JsonSerializer.Serialize(mess));

            string target = _configuration["AppUrl"] + "/authentication/confirmemail/" + payload;
            await _emailSender.SendEmailAsync(request.Val, "Confirm your email",
                $"Please confirm your Notes 2022 account email by <a href='{target}'>clicking here</a>.  You cannot login until you do this.");

            return new AuthReply() { Status = StatusCodes.Status200OK, Message = "Email sent!" };
        }

        public override async Task<AuthReply> ResetPassword(AString request, ServerCallContext context)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(request.Val);
            if (user is null)
            {
                user = await _userManager.FindByNameAsync(request.Val);
            }
            if (user is null)
                return new() { Status = 500, Message = "Somthing went wrong!" };

            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            ConfirmEmailRequest mess = new() { UserId = user.Id, Code = code };
            string payload = Globals.Base64Encode(JsonSerializer.Serialize(mess));

            string target = _configuration["AppUrl"] + "/authentication/resetpassword/" + payload;
            await _emailSender.SendEmailAsync(request.Val, "Reset your password",
                $"Please <a href='{target}'>click here</a> to reset your Notes2022 password.");

            return new AuthReply() { Status = StatusCodes.Status200OK, Message = "Email sent!" };
        }

        public override async Task<AuthReply> ResetPassword2(ResetPasswordRequest request, ServerCallContext context)
        {
            AuthReply ar = new() { Status = 500, Message = "Something went wrong" };

            ApplicationUser? user = await _userManager.FindByIdAsync(request.UserId);
            if (user is null)
            {
                return ar;
            }

            IdentityResult res = await _userManager.ResetPasswordAsync(user, request.Code, request.NewPassword);

            if (res.Succeeded)
            {
                ar.Message = "Password changed!";
                ar.Status = 200;
            }

            return ar;
        }

        [Authorize]
        public override async Task<AuthReply> ChangePassword(ResetPasswordRequest request, ServerCallContext context)
        {
            AuthReply ar = new() { Status = 500, Message = "Something went wrong" };

            ApplicationUser? user = await GetAppUser(context);
            if (user is null)
            {
                return ar;
            }

            IdentityResult? res = await _userManager.ChangePasswordAsync(user, request.Code, request.NewPassword);

            if (res.Succeeded)
            {
                ar.Status = 200;
                ar.Message = "Password Changed!";
            }

            return ar;
        }

        /// <summary>
        /// Logouts the current User.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>AuthReply.</returns>
        public override async Task<AuthReply> Logout(NoRequest request, ServerCallContext context)
        {
            await _signInManager.SignOutAsync();
            return new AuthReply() { Status = StatusCodes.Status200OK, Message = "User logged out!" };
        }

        /// <summary>
        /// Gets the JWT token.
        /// </summary>
        /// <param name="authClaims">The authentication claims.</param>
        /// <param name="hours">The hours.</param>
        /// <returns>JwtSecurityToken.</returns>
        private JwtSecurityToken GetToken(List<Claim> authClaims, int hours)
        {
            SymmetricSecurityKey? authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTAuth:SecretKey"]));

            JwtSecurityToken? token = new JwtSecurityToken(
                issuer: _configuration["JWTAuth:ValidIssuerURL"],
                audience: _configuration["JWTAuth:ValidAudienceURL"],
                expires: DateTime.Now.AddHours(hours),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        //[Authorize]
        //public override async Task<GAppUser> GetAppUser(AppUserRequest request, ServerCallContext context)
        //{
        //    string Id = request.Subject;
        //    ApplicationUser user = await _userManager.FindByIdAsync(Id);
        //    return user.GetGAppUser();
        //}

        /// <summary>
        /// Gets the list of all Users
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GAppUserList.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<GAppUserList> GetUserList(NoRequest request, ServerCallContext context)
        {
            List<ApplicationUser> list = await _userManager.Users.ToListAsync();
            return ApplicationUser.GetGAppUserList(list);
        }

        /// <summary>
        /// Gets the user roles.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>EditUserViewModel.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<EditUserViewModel> GetUserRoles(AppUserRequest request, ServerCallContext context)
        {
            EditUserViewModel model = new()
            {
                RolesList = new CheckedUserList()
            };
            string Id = request.Subject;
            ApplicationUser user = await _userManager.FindByIdAsync(Id);

            model.UserData = user.GetGAppUser();

            List<IdentityRole>? allRoles = _roleManager.Roles.ToList();

            //var myRoles = await _userManager.GetRolesAsync(user);

            foreach (IdentityRole? role in allRoles)
            {
                CheckedUser cu = new()
                {
                    TheRole = new()
                };
                cu.TheRole.RoleId = role.Id;
                cu.TheRole.RoleName = role.Name;
                cu.IsMember = await _userManager.IsInRoleAsync(user, role.Name);
                model.RolesList.List.Add(cu);
            }

            return model;
        }

        /// <summary>
        /// Updates the user roles.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<NoRequest> UpdateUserRoles(EditUserViewModel model, ServerCallContext context)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(model.UserData.Id);
            IList<string>? myRoles = await _userManager.GetRolesAsync(user);
            foreach (CheckedUser item in model.RolesList.List)
            {
                if (item.IsMember && !myRoles.Contains(item.TheRole.RoleName)) // need to add role
                {
                    await _userManager.AddToRoleAsync(user, item.TheRole.RoleName);
                }
                else if (!item.IsMember && myRoles.Contains(item.TheRole.RoleName)) // need to remove role
                {
                    await _userManager.RemoveFromRoleAsync(user, item.TheRole.RoleName);
                }
            }

            return new NoRequest();
        }

        //[Authorize]
        //public override async Task<GNotefileList> GetAllNotefiles(NoRequest request, ServerCallContext context)
        //{
        //    List<NoteFile> x = _db.NoteFile.ToList();

        //    // should filter out files user has no access to  TODO

        //    return NoteFile.GetGNotefileList(x);
        //}

        /// <summary>
        /// Gets the application User.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>ApplicationUser.</returns>
        private async Task<ApplicationUser> GetAppUser(ServerCallContext context)
        {
            ClaimsPrincipal? user = context.GetHttpContext().User;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            ApplicationUser appUser = await _userManager.FindByIdAsync(user.FindFirst(ClaimTypes.NameIdentifier).Value);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            return appUser;
        }

        /// <summary>
        /// Creates a note file.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>System.Nullable&lt;GNotefile&gt;.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<NoteFile?> CreateNoteFile(NoteFile request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            await NoteDataManager.CreateNoteFile(_db, appUser.Id, request.NoteFileName, request.NoteFileTitle);

            List<NoteFile> x = _db.NoteFile.OrderBy(x => x.Id).ToList();
            NoteFile newfile = x[^1];
            return newfile;
        }

        /// <summary>
        /// Gets the home page model.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>HomePageModel.</returns>
        [Authorize]
        public override async Task<HomePageModel> GetHomePageModel(NoRequest request, ServerCallContext context)
        {
            return await GetBaseHomePageModelAsync(request, context);
        }

        /// <summary>
        /// Gets the admin page model.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>HomePageModel.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<HomePageModel> GetAdminPageModel(NoRequest request, ServerCallContext context)
        {
            HomePageModel homepageModel = await GetBaseHomePageModelAsync(request, context);

            List<ApplicationUser> udl = _db.Users.ToList();
            homepageModel.UserDataList = new GAppUserList();
            foreach (ApplicationUser userx in udl)
            {
                GAppUser ud = userx.GetGAppUser();
                homepageModel.UserDataList.List.Add(ud);
            }

            GAppUser user = homepageModel.UserData;
            homepageModel.NoteAccesses = new NoteAccessList();
            foreach (NoteFile nf in homepageModel.NoteFiles.List)
            {
                NoteAccess na = await AccessManager.GetAccess(_db, user.Id, nf.Id, 0);
                homepageModel.NoteAccesses.List.Add(na);
            }

            return homepageModel;
        }

        /// <summary>
        /// Get base home page model as an asynchronous operation.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>A Task&lt;HomePageModel&gt; representing the asynchronous operation.</returns>
        private async Task<HomePageModel> GetBaseHomePageModelAsync(NoRequest request, ServerCallContext context)
        {
            HomePageModel homepageModel = new();

            NoteFile? hpmf = _db.NoteFile.Where(p => p.NoteFileName == "homepagemessages").FirstOrDefault();
            if (hpmf is not null)
            {
                NoteHeader? hpmh = _db.NoteHeader.Where(p => p.NoteFileId == hpmf.Id && !p.IsDeleted && p.Version == 0)
                    .OrderByDescending(p => p.CreateDate).FirstOrDefault();
                if (hpmh is not null)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    homepageModel.Message = _db.NoteContent.Where(p => p.NoteHeaderId == hpmh.Id).FirstOrDefault().NoteBody;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
            }

            if (context.GetHttpContext().User != null)
            {
                try
                {
                    ClaimsPrincipal user = context.GetHttpContext().User;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (user.FindFirst(ClaimTypes.NameIdentifier) is not null && user.FindFirst(ClaimTypes.NameIdentifier).Value != null)
                    {
                        ApplicationUser appUser;
                        try
                        {

                            appUser = await GetAppUser(context);
                            homepageModel.UserData = appUser.GetGAppUser();
                        }
                        catch (Exception)
                        {
                            return homepageModel;
                        }

                        List<NoteFile> allFiles = _db.NoteFile.ToList().OrderBy(p => p.NoteFileTitle).ToList();
                        List<NoteAccess> myAccesses = _db.NoteAccess.Where(p => p.UserID == appUser.Id).ToList();
                        List<NoteAccess> otherAccesses = _db.NoteAccess.Where(p => p.UserID == Globals.AccessOtherId).ToList();

                        NoteFileList myNoteFiles = new();

                        bool isAdmin = await _userManager.IsInRoleAsync(appUser, UserRoles.Admin);
                        foreach (NoteFile file in allFiles)
                        {
                            NoteAccess? x = myAccesses.SingleOrDefault(p => p.NoteFileId == file.Id);
                            if (x is null)
                                x = otherAccesses.Single(p => p.NoteFileId == file.Id);

                            if (isAdmin || x.ReadAccess || x.Write || x.ViewAccess)
                            {
                                myNoteFiles.List.Add(file);

                            }
                        }
                        homepageModel.NoteFiles = myNoteFiles;
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
                catch (Exception)
                {
                }
            }
            return homepageModel;
        }

        /// <summary>
        /// Updates the note file.
        /// </summary>
        /// <param name="noteFile">The note file.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// NoteFile.
        /// </returns>
        [Authorize(Roles = "Admin")]
        public override async Task<NoteFile> UpdateNoteFile(NoteFile noteFile, ServerCallContext context)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, noteFile.Id);
            nf.NoteFileName = noteFile.NoteFileName;
            nf.NoteFileTitle = noteFile.NoteFileTitle;
            _db.NoteFile.Update(nf);
            await _db.SaveChangesAsync();
            return noteFile;
        }

        /// <summary>
        /// Deletes the note file.
        /// </summary>
        /// <param name="noteFile">The note file.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize(Roles = "Admin")]
        public override async Task<NoRequest> DeleteNoteFile(NoteFile noteFile, ServerCallContext context)
        {
            NoteFile nf = await NoteDataManager.GetFileById(_db, noteFile.Id);

            // remove tags
            //List<Tags> tl = _db.Tags.Where(p => p.NoteFileId == nf.Id).ToList();
            //_db.Tags.RemoveRange(tl);

            // remove content
            //List<NoteHeader> hl = _db.NoteHeader.Where(p => p.NoteFileId == nf.Id).ToList();
            //foreach (NoteHeader h in hl)
            //{
            //    NoteContent? c = _db.NoteContent.SingleOrDefault(p => p.NoteHeaderId == h.Id);
            //    if (c != null)
            //        _db.NoteContent.Remove(c);
            //}

            //// remove headers
            //_db.NoteHeader.RemoveRange(hl);

            //// remove access
            //List<NoteAccess> al = _db.NoteAccess.Where(p => p.NoteFileId == nf.Id).ToList();
            //_db.NoteAccess.RemoveRange(al);

            // remove file
            _db.NoteFile.Remove(nf);
            await _db.SaveChangesAsync();
            return new NoRequest();
        }

        ///// <summary>
        ///// Imports the specified request.
        ///// </summary>
        ///// <param name="request">The request. Points to a file on server</param>
        ///// <param name="context">The context.</param>
        ///// <returns>NoRequest.</returns>
        //[Authorize(Roles = "Admin")]
        //public override async Task<NoRequest> Import(ImportRequest request, ServerCallContext context)
        //{
        //    Importer imp = new();
        //    _ = await imp.Import(_db, Globals.ImportRoot + request.UploadFile, request.NoteFile);
        //    return new NoRequest();
        //}

        ///// <summary>
        ///// Imports the specified request.
        ///// </summary>
        ///// <param name="request">The request. Contains entire contents to import!</param>
        ///// <param name="context">The context.</param>
        ///// <returns>NoRequest.</returns>
        //[Authorize(Roles = "Admin")]
        //public override async Task<NoRequest> Import(ImportRequest request, ServerCallContext context)
        //{
        //    MemoryStream? input = new(request.Payload.ToArray());
        //    StreamReader file = new(input);

        //    Importer? imp = new();
        //    _ = await imp.Import(_db, file, request.NoteFile);

        //    file.DiscardBufferedData();
        //    file.Dispose();
        //    input.Dispose();
        //    GC.Collect();
        //    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        //    GC.Collect();
        //    return new NoRequest();
        //}

        /// <summary>
        /// rpc Import(ImportRequest) returns (NoRequest);  
        /// Runs a Json import given client side file contents
        /// Caller must have Write access to the target note file.
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>NoRequest</returns>
        [Authorize]
        public override async Task<NoRequest> ImportJson(ImportRequest request, ServerCallContext context)
        {
            ApplicationUser appUser;

            try
            {
                NoteFile? nf = await _db.NoteFile.SingleOrDefaultAsync(p => p.NoteFileName == request.NoteFile);
                if (nf is null || string.IsNullOrEmpty(request.Payload))
                    return new NoRequest();

                appUser = await GetAppUser(context);
                NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, nf.Id, 0);
                if (!na.Write)
                    return new NoRequest();
            }
            catch (Exception)
            {
                return new NoRequest();
            }

            JsonData data = new JsonData();
            data.JsonText = request.Payload;
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<JsonData>? ent =_db.JsonData.Add(data);
            await _db.SaveChangesAsync();

#pragma warning disable CS8605 // Unboxing a possibly null value.
            int fileId = (int)ent.Member("Id").CurrentValue;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            Importer imp = new Importer(_db);
            BackgroundJob.Enqueue(() => imp.Import(fileId, request.NoteFile, request.JsonFileName, appUser.Email));

            Thread.Sleep(2000);

            return new NoRequest();
        }


        /// <summary>
        /// Gets the note file index data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoteDisplayIndexModel.</returns>
        [Authorize]
        public override async Task<NoteDisplayIndexModel> GetNoteFileIndexData(NoteFileRequest request, ServerCallContext context)
        {
            ClaimsPrincipal user;
            ApplicationUser appUser;
            NoteDisplayIndexModel idxModel = new();
            bool isAdmin;
            bool isUser;

            int arcId = request.ArcId;

            user = context.GetHttpContext().User;
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (user.FindFirst(ClaimTypes.NameIdentifier) != null && user.FindFirst(ClaimTypes.NameIdentifier).Value != null)
                {
                    try
                    {
                        appUser = await GetAppUser(context);

                        isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");
                        isUser = await _userManager.IsInRoleAsync(appUser, "User");
                        if (!isUser)
                            return idxModel;    // not a User?  You get NOTHING!

                        NoteAccess noteAccess = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, arcId);
                        if (noteAccess is null)
                        {
                            idxModel.Message = "File does not exist";
                            return idxModel;
                        }
                        if (isAdmin)
                        {
                            noteAccess.ViewAccess = true;    // Admins can always view access list
                        }
                        idxModel.MyAccess = noteAccess;

                        idxModel.NoteFile = (await NoteDataManager.GetFileById(_db, request.NoteFileId));

                        if (!idxModel.MyAccess.ReadAccess && !idxModel.MyAccess.Write)
                        {
                            idxModel.Message = "You do not have access to file " + idxModel.NoteFile.NoteFileName;
                            return idxModel;
                        }

                        //List<LinkedFile> linklist = await _db.LinkedFile.Where(p => p.HomeFileId == request.NoteFileId).ToListAsync();
                        //if (linklist is not null && linklist.Count > 0)
                        //    idxModel.LinkedText = " (Linked)";

                        List<NoteHeader> allhead = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == arcId).ToListAsync();
                        idxModel.AllNotes.AddRange(allhead);

                        List<NoteHeader> notes = allhead.FindAll(p => p.ResponseOrdinal == 0).OrderBy(p => p.NoteOrdinal).ToList();
                        idxModel.Notes.AddRange(notes);    // = NoteHeader.GetGNoteHeaderList(notes);

                        idxModel.UserData = appUser.GetGAppUser();

                        List<Tags> tags = await _db.Tags.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == arcId).ToListAsync();
                        if (tags.Count > 0)
                        {
                            //idxModel.Tags = new();
                            idxModel.Tags.AddRange(tags);
                        }

                        idxModel.ArcId = arcId;
                    }
                    catch (Exception ex1)
                    {
                        idxModel.Message = ex1.Message;
                    }
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            catch (Exception ex)
            {
                idxModel.Message = ex.Message;
            }

            return idxModel;
        }

        /// <summary>
        /// Gets the content of the note.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>DisplayModel.</returns>
        [Authorize]
        public override async Task<DisplayModel> GetNoteContent(DisplayModelRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            bool isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");

            NoteHeader nh = await _db.NoteHeader.SingleAsync(p => p.Id == request.NoteId && p.Version == request.Vers);
            NoteContent c = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == nh.Id);
            List<Tags> tags = await _db.Tags.Where(p => p.NoteHeaderId == nh.Id).ToListAsync();
            NoteFile nf = await _db.NoteFile.SingleAsync(p => p.Id == nh.NoteFileId);
            NoteAccess access = await AccessManager.GetAccess(_db, appUser.Id, nh.NoteFileId, nh.ArchiveId);

            bool canEdit = isAdmin;         // admins can always edit a note
            if (appUser.Id == nh.AuthorID)  // otherwise only the author can edit
                canEdit = true;

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, nh.NoteFileId, nh.ArchiveId);

            if (!na.ReadAccess)
                return new DisplayModel();

            DisplayModel model = new()
            {
                Header = nh,
                Content = c,
                //Tags = Tags.GetGTagsList(tags),   // see below
                NoteFile = nf,
                Access = access,
                CanEdit = canEdit,
                IsAdmin = isAdmin
            };

            model.Tags.AddRange(tags);
            return model;
        }

        /// <summary>
        /// Gets the content of the note.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>DisplayModel.</returns>
        [Authorize]
        public override async Task<DisplayModel> GetPartNoteContent(DisplayModelRequest request, ServerCallContext context)
        {
            NoteContent c = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == request.NoteId);
            List<Tags> tags = await _db.Tags.Where(p => p.NoteHeaderId == request.NoteId).ToListAsync();

            DisplayModel model = new()
            {
                Content = c,
                //Tags = Tags.GetGTagsList(tags),   // see below
            };

            model.Tags.AddRange(tags);
            return model;
        }

        /// <summary>
        /// Searches the content of the note.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>ContentSearchResponse.</returns>
        [Authorize]
        public override async Task<ContentSearchResponse> SearchNoteContent(ContentSearchRequest request, ServerCallContext context)
        {
            List<NoteHeader> nhl = await _db.NoteHeader.Where(p => p.NoteFileId == request.FileId 
                    && p.ArchiveId == request.ArcId && !p.IsDeleted && p.Version == 0)
                    //.OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal)
                    .ToListAsync();

            List<long> ids = nhl.Select(p => p.Id).ToList();

            List<NoteContent> ncl = await _db.NoteContent.Where(p => ids.Contains(p.NoteHeaderId)).ToListAsync();

            ContentSearchResponse resp = new();

            foreach(NoteContent nc  in ncl)
            {
                if (nc.NoteBody.Contains(request.Target))
                    resp.List.Add(nc.NoteHeaderId);
            }

            return resp;
        }


        /// <summary>
        /// Gets the access list.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteAccessList.</returns>
        [Authorize]
        public override async Task<NoteAccessList> GetAccessList(AccessAndUserListRequest request, ServerCallContext context)
        {
            NoteAccessList ret = new();
            ret.List.AddRange((await _db.NoteAccess.Where(p => p.NoteFileId == request.FileId && p.ArchiveId == request.ArcId).ToListAsync()));
            return ret;
        }

        /// <summary>
        /// Gets the access and user list.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>AccessAndUserList.</returns>
        [Authorize]
        public override async Task<AccessAndUserList> GetAccessAndUserList(AccessAndUserListRequest request, ServerCallContext context)
        {
            AccessAndUserList accessAndUserList = new()
            {
                //AccessList = NoteAccess.GetGNoteAccessList(_db.NoteAccess.Where(p => p.NoteFileId == request.FileId && p.ArchiveId == request.ArcId).ToList()),
                //AppUsers = ApplicationUser.GetGAppUserList((await _userManager.GetUsersInRoleAsync("User")).ToList()),
                UserAccess = (await AccessManager.GetAccess(_db, request.UserId, request.FileId, request.ArcId))
            };

            accessAndUserList.AccessList.AddRange(_db.NoteAccess.Where(p => p.NoteFileId == request.FileId && p.ArchiveId == request.ArcId).ToList());
            GAppUserList? ul = ApplicationUser.GetGAppUserList((await _userManager.GetUsersInRoleAsync("User")).ToList());

            accessAndUserList.AppUsers = ul;
            return accessAndUserList;
        }

        /// <summary>
        /// Updates the access item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteAccess.</returns>
        [Authorize]
        public override async Task<NoteAccess> UpdateAccessItem(NoteAccess request, ServerCallContext context)
        {
            NoteAccess access = (request);
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, access.NoteFileId, access.ArchiveId);
            if (na.EditAccess)
            {
                _db.NoteAccess.Update(access);
                await _db.SaveChangesAsync();
            }

            return request;
        }

        /// <summary>
        /// Deletes the access item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> DeleteAccessItem(NoteAccess request, ServerCallContext context)
        {
            NoteAccess access = (request);
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, access.NoteFileId, access.ArchiveId);
            if (na.EditAccess)
            {
                _db.NoteAccess.Remove(access);
                await _db.SaveChangesAsync();
            }

            return new NoRequest();
        }

        /// <summary>
        /// Adds an access item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteAccess.</returns>
        [Authorize]
        public override async Task<NoteAccess> AddAccessItem(NoteAccess request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArchiveId);
            if (na.EditAccess)
            {
                await _db.NoteAccess.AddAsync(request);
                await _db.SaveChangesAsync();
            }

            return request;
        }

        /// <summary>
        /// Gets the user data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GAppUser.</returns>
        [Authorize]
        public override async Task<GAppUser> GetUserData(NoRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            return appUser.GetGAppUser();
        }

        /// <summary>
        /// Updates the user data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GAppUser.</returns>
        [Authorize]
        public override async Task<GAppUser> UpdateUserData(GAppUser request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            if (appUser.Id != request.Id)   // can onlt update self
                return request;

            ApplicationUser appUserBase = await _userManager.FindByIdAsync(request.Id);
            ApplicationUser merged = ApplicationUser.MergeApplicationUser(request, appUserBase);

            await _userManager.UpdateAsync(merged);

            return request;
        }

        /// <summary>
        /// Gets the versions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteHeaderList.</returns>
        [Authorize]
        public override async Task<NoteHeaderList> GetVersions(GetVersionsRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.FileId, request.ArcId);
            if (!na.ReadAccess)
                return new NoteHeaderList();

            List<NoteHeader> hl;

            hl = _db.NoteHeader.Where(p => p.NoteFileId == request.FileId && p.Version != 0
                    && p.NoteOrdinal == request.NoteOrdinal && p.ResponseOrdinal == request.ResponseOrdinal && p.ArchiveId == request.ArcId)
                .OrderBy(p => p.Version)
                .ToList();

            NoteHeaderList ret= new();
            ret.List.AddRange(hl);
            return ret;
        }

        /// <summary>
        /// Gets the sequencer list.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GSequencerList.</returns>
        [Authorize]
        public override async Task<SequencerList> GetSequencer(NoRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            // My list
            List<Sequencer> mine = await _db.Sequencer.Where(p => p.UserId == appUser.Id).OrderBy(p => p.Ordinal).ThenBy(p => p.LastTime).ToListAsync();

            if (mine is null)
                mine = new List<Sequencer>();

            List<Sequencer> avail = new();

            foreach (Sequencer m in mine)
            {
                NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, m.NoteFileId, 0);
                if (na.ReadAccess)
                    avail.Add(m);   // ONLY if you have current read access!!
            }

            SequencerList ret = new();
            ret.List.AddRange(avail.OrderBy(p => p.Ordinal).ToList());

            return ret;
        }

        /// <summary>
        /// Creates the sequencer item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> CreateSequencer(SCheckModel request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            List<Sequencer> mine = await _db.Sequencer.Where(p => p.UserId == appUser.Id).OrderByDescending(p => p.Ordinal).ToListAsync();

            int ord;
            if (mine is null || mine.Count == 0)
            {
                ord = 1;
            }
            else
            {
                ord = mine[0].Ordinal + 1;
            }

            Sequencer tracker = new()   // make a starting entry
            {
                Active = true,
                NoteFileId = request.FileId,
                LastTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                UserId = appUser.Id,
                Ordinal = ord,
                StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
            };

            _db.Sequencer.Add(tracker);
            await _db.SaveChangesAsync();

            return new NoRequest();
        }

        /// <summary>
        /// Deletes the sequencer item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> DeleteSequencer(SCheckModel request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Sequencer mine = await _db.Sequencer.SingleOrDefaultAsync(p => p.UserId == appUser.Id && p.NoteFileId == request.FileId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            if (mine is null)
                return new NoRequest();

            _db.Sequencer.Remove(mine);
            await _db.SaveChangesAsync();

            return new NoRequest();
        }

        /// <summary>
        /// Updates the sequencer item ordinal.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> UpdateSequencerOrdinal(Sequencer request, ServerCallContext context)
        {
            Sequencer modified = await _db.Sequencer.SingleAsync(p => p.UserId == request.UserId && p.NoteFileId == request.NoteFileId);

            modified.LastTime = request.LastTime;
            modified.Ordinal = request.Ordinal;

            _db.Entry(modified).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return new NoRequest();
        }

        /// <summary>
        /// Updates the sequencer item.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> UpdateSequencer(Sequencer request, ServerCallContext context)
        {
            Sequencer modified = await _db.Sequencer.SingleAsync(p => p.UserId == request.UserId && p.NoteFileId == request.NoteFileId);

            modified.Active = request.Active;
            if (request.Active)  // starting to seq - set start time
            {
                modified.StartTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            }
            else            // end of a file - copy start time to LastTime so we do not miss notes
            {
                modified.LastTime = modified.StartTime;  //request.StartTime.ToDateTime();
            }

            _db.Entry(modified).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return new NoRequest();
        }

        /// <summary>
        /// Gets the note file.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNotefile.</returns>
        [Authorize]
        public override async Task<NoteFile> GetNoteFile(NoteFileRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, 0);
            if (na.Write || na.ReadAccess || na.EditAccess || na.Respond)      // TODO is this right??
            { }
            else
                return new NoteFile();

            NoteFile nf = _db.NoteFile.Single(p => p.Id == request.NoteFileId);

            return nf;
        }

        /// <summary>
        /// Creates the new note.
        /// </summary>
        /// <param name="TextViewModel">The TVM.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteHeader.</returns>
        [Authorize]
        public override async Task<NoteHeader> CreateNewNote(TextViewModel tvm, ServerCallContext context)
        {
            if (tvm.MyNote is null || tvm.MySubject is null)
                return new NoteHeader();

            ApplicationUser appUser = await GetAppUser(context);
            bool test = await _userManager.IsInRoleAsync(appUser, "User");
            if (!test)  // Must be in a User Role
                return new NoteHeader();

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, tvm.NoteFileID, 0);
            if (na.Write || na.Respond)
            { }
            else
                return new NoteHeader();

            ApplicationUser me = appUser;
            Google.Protobuf.WellKnownTypes.Timestamp now = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            NoteHeader nheader = new()  // construct a new NoteHeader
            {
                LastEdited = now,
                ThreadLastEdited = now,
                CreateDate = now,
                NoteFileId = tvm.NoteFileID,
                AuthorName = me.DisplayName,
                AuthorID = me.Id,
                NoteSubject = tvm.MySubject,
                DirectorMessage = tvm.DirectorMessage,
                ResponseOrdinal = 0,
                ResponseCount = 0
            };

            NoteHeader created;
            using (IDbContextTransaction? dbTran = _db.Database.BeginTransaction())
            {
                if (tvm.BaseNoteHeaderID == 0)  // a base note
                {
                    created = await NoteDataManager.CreateNote(_db, nheader, tvm.MyNote, tvm.TagLine, tvm.DirectorMessage, true, false);
                }
                else        // a response
                {
                    nheader.BaseNoteId = tvm.BaseNoteHeaderID;
                    nheader.RefId = tvm.RefId;
                    created = await NoteDataManager.CreateResponse(_db, nheader, tvm.MyNote, tvm.TagLine, tvm.DirectorMessage, true, false);
                }
                dbTran.Commit();
            }

            //// Process any linked note file
            //await ProcessLinkedNotes();

            //// Send copy to subscribers
            //await SendNewNoteToSubscribers(created);

            return created;
        }

        /// <summary>
        /// Updates the note.
        /// </summary>
        /// <param name="tvm">The TVM.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteHeader.</returns>
        [Authorize]
        public override async Task<NoteHeader> UpdateNote(TextViewModel tvm, ServerCallContext context)
        {
            if (tvm.MyNote is null || tvm.MySubject is null)
                return new NoteHeader();

            NoteHeader nheader = await NoteDataManager.GetBaseNoteHeaderById(_db, tvm.NoteID);

            ApplicationUser appUser = await GetAppUser(context);
            bool isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");
            bool canEdit = isAdmin;         // admins can always edit a note
            if (appUser.Id == nheader.AuthorID)  // otherwise only the author can edit
                canEdit = true;

            if (!canEdit)
                return new NoteHeader();

            // update header
            DateTime now =  DateTime.UtcNow;
            nheader.NoteSubject = tvm.MySubject;
            nheader.DirectorMessage = tvm.DirectorMessage;
            //nheader.LastEdited = now;
            nheader.ThreadLastEdited = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(now);

            NoteContent nc = new()
            {
                NoteHeaderId = tvm.NoteID,
                NoteBody = tvm.MyNote
            };

            NoteHeader newheader = await NoteDataManager.EditNote(_db, _userManager, nheader, nc, tvm.TagLine);

            //await ProcessLinkedNotes();

            return newheader;
        }

        /// <summary>
        /// Gets the header for note identifier.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteHeader.</returns>
        [Authorize]
        public override async Task<NoteHeader> GetHeaderForNoteId(NoteId request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            bool isAdmin = await _userManager.IsInRoleAsync(appUser, "Admin");

            NoteHeader gnh = (await _db.NoteHeader.SingleAsync(p => p.Id == request.Id));

            if (isAdmin)
            { }
            else
            {
                NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, gnh.NoteFileId, gnh.ArchiveId);
                if (!na.ReadAccess)
                {
                    return new NoteHeader();
                }
            }

            return gnh;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <summary>
        /// Gets the about.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>AboutModel.</returns>
        public override async Task<AboutModel> GetAbout(NoRequest request, ServerCallContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            TimeSpan span = DateTime.UtcNow - Globals.StartTime;
            Google.Protobuf.WellKnownTypes.Duration dur = new Google.Protobuf.WellKnownTypes.Duration();
            dur.Seconds = (long)span.TotalSeconds;

            return new AboutModel()
            {
                PrimeAdminEmail = _configuration["PrimeAdminEmail"],
                PrimeAdminName = _configuration["PrimeAdminName"],
                HostUri = _configuration["AppUrl"],
                UpTime = dur
            };
        }


        /// <summary>
        /// Send email authenticated.
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        [Authorize]
        public override async Task<NoRequest> SendEmailAuth(GEmail request, ServerCallContext context)
        {
            await _emailSender.SendEmailAsync(request.Address, request.Subject, request.Body);
            return new NoRequest();
        }

        /// <summary>
        /// Gets the export info for phase 2. (note content)
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNoteContent.</returns>
        [Authorize]
        public override async Task<NoteContent> GetExport2(NoteId request, ServerCallContext context)
        {
            NoteContent nc = _db.NoteContent.Single(p => p.NoteHeaderId == request.Id);

            NoteHeader nh = _db.NoteHeader.Single(p => p.Id == request.Id);
                
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, nh.NoteFileId, nh.ArchiveId);
            if (!na.ReadAccess)
                return new NoteContent();

            return nc;
        }

        /// <summary>
        /// Does the forward of note(s) to an email address.
        /// </summary>
        /// <param name="fv">The fv.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> DoForward(ForwardViewModel fv, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, fv.FileID, fv.ArcID);
            if (!na.ReadAccess)
                return new NoRequest();

#pragma warning disable CS8604 // Possible null reference argument.
            string myEmail = await LocalService.MakeNoteForEmail(fv, fv.NoteFile, _db, appUser.Email, appUser.DisplayName);
#pragma warning restore CS8604 // Possible null reference argument.

            await _emailSender.SendEmailAsync(appUser.Email, fv.NoteSubject, myEmail);
            return new NoRequest();
        }

        /// <summary>
        /// Gets the note files ordered by name.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>GNotefileList.</returns>
        [Authorize]
        public override async Task<NoteFileList> GetNoteFilesOrderedByName(NoRequest request, ServerCallContext context)
        {
            List<NoteFile> noteFiles = await _db.NoteFile.OrderBy(p => p.NoteFileName).ToListAsync();

            NoteFileList ret = new();
            ret.List.AddRange(noteFiles);
            return ret;
        }

        /// <summary>
        /// Copies note(s) from one file to another
        /// </summary>
        /// <param name="Model">The model.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> CopyNote(CopyModel Model, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);

            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, Model.Note.NoteFileId, Model.Note.ArchiveId);
            if (!na.ReadAccess)
                return new NoRequest();         // can not read file

            int fileId = Model.FileId;

            // Can I write to the target file?
            string uid = appUser.Id;
            NoteAccess myAccess = await AccessManager.GetAccess(_db, uid, fileId, 0);
            if (!myAccess.Write)
                return new NoRequest();         // can not write to file

            // Prepare to copy
            NoteHeader Header = Model.Note;
            bool whole = Model.WholeString;
            NoteFile noteFile = await _db.NoteFile.SingleAsync(p => p.Id == fileId);

            using (IDbContextTransaction? dbTran = _db.Database.BeginTransaction())
            {
                // Just the note
                if (!whole)
                {
                    NoteContent cont = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == Header.Id);
                    //cont.NoteHeader = null;
                    List<Tags> tags = await _db.Tags.Where(p => p.NoteHeaderId == Header.Id).ToListAsync();

                    string Body = string.Empty;
                    Body = MakeHeader(Header, noteFile);
                    Body += cont.NoteBody;

                    Header = Header.CloneForLink();

                    Header.Id = 0;
                    Header.ArchiveId = 0;
                    Header.LinkGuid = string.Empty;
                    Header.NoteOrdinal = 0;
                    Header.ResponseCount = 0;
                    Header.NoteFileId = fileId;
                    Header.BaseNoteId = 0;
                    //Header.NoteFile = null;
                    Header.AuthorID = appUser.Id;
                    Header.AuthorName = appUser.DisplayName;

                    Header.CreateDate = Header.ThreadLastEdited = Header.LastEdited = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

                    _ = await NoteDataManager.CreateNote(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);
                }
                else    // whole note string
                {
                    // get base note first
                    NoteHeader BaseHeader;
                    BaseHeader = await _db.NoteHeader.SingleAsync(p => p.NoteFileId == Header.NoteFileId
                        && p.ArchiveId == Header.ArchiveId
                        && p.NoteOrdinal == Header.NoteOrdinal
                        && p.ResponseOrdinal == 0);

                    Header = BaseHeader.CloneForLink();

                    NoteContent cont = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == Header.Id);
                    //cont.NoteHeader = null;
                    List<Tags> tags = await _db.Tags.Where(p => p.NoteHeaderId == Header.Id).ToListAsync();

                    string Body = string.Empty;
                    Body = MakeHeader(Header, noteFile);
                    Body += cont.NoteBody;

                    Header.Id = 0;
                    Header.ArchiveId = 0;
                    Header.LinkGuid = string.Empty;
                    Header.NoteOrdinal = 0;
                    Header.ResponseCount = 0;
                    Header.NoteFileId = fileId;
                    Header.BaseNoteId = 0;
                    //Header.NoteFile = null;
                    Header.AuthorID = appUser.Id;
                    Header.AuthorName = appUser.DisplayName;

                    Header.CreateDate = Header.ThreadLastEdited = Header.LastEdited = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

                    Header.Content = null;

                    NoteHeader NewHeader = await NoteDataManager.CreateNote(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);

                    // now deal with any responses
                    for (int i = 1; i <= BaseHeader.ResponseCount; i++)
                    {
                        NoteHeader RHeader = await _db.NoteHeader.SingleAsync(p => p.NoteFileId == BaseHeader.NoteFileId
                            && p.ArchiveId == BaseHeader.ArchiveId
                            && p.NoteOrdinal == BaseHeader.NoteOrdinal
                            && p.ResponseOrdinal == i);

                        Header = RHeader.CloneForLinkR();

                        cont = await _db.NoteContent.SingleAsync(p => p.NoteHeaderId == Header.Id);
                        tags = await _db.Tags.Where(p => p.NoteHeaderId == Header.Id).ToListAsync();

                        Body = string.Empty;
                        Body = MakeHeader(Header, noteFile);
                        Body += cont.NoteBody;

                        Header.Id = 0;
                        Header.ArchiveId = 0;
                        Header.LinkGuid = string.Empty;
                        Header.NoteOrdinal = NewHeader.NoteOrdinal;
                        Header.ResponseCount = 0;
                        Header.NoteFileId = fileId;
                        Header.BaseNoteId = NewHeader.Id;
                        //Header.NoteFile = null;
                        Header.ResponseOrdinal = 0;
                        Header.AuthorID = appUser.Id;
                        Header.AuthorName = appUser.DisplayName;

                        Header.CreateDate = Header.ThreadLastEdited = Header.LastEdited = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

                        _ = await NoteDataManager.CreateResponse(_db, Header, Body, Tags.ListToString(tags), Header.DirectorMessage, true, false);
                    }
                }
                dbTran.Commit();
            }

            return new NoRequest();
        }

        // Utility method - makes a viewable header for the copied note
        /// <summary>
        /// Makes the header.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="noteFile">The note file.</param>
        /// <returns>System.String.</returns>
        private static string MakeHeader(NoteHeader header, NoteFile noteFile)
        {
            StringBuilder sb = new();

            sb.Append("<div class=\"copiednote\">From: ");
            sb.Append(noteFile.NoteFileName);
            sb.Append(" - ");
            sb.Append(header.NoteSubject);
            sb.Append(" - ");
            sb.Append(header.AuthorName);
            sb.Append(" - ");
            sb.Append(header.CreateDate.ToDateTime().ToShortDateString());
            sb.AppendLine("</div>");
            return sb.ToString();
        }

        /// <summary>
        /// Deletes the note.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>NoRequest.</returns>
        [Authorize]
        public override async Task<NoRequest> DeleteNote(NoteId request, ServerCallContext context)
        {
            NoteHeader note = await NoteDataManager.GetNoteByIdWithFile(_db, request.Id);

            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, note.NoteFileId, note.ArchiveId);
            if (!na.DeleteEdit)
            {
                return new();
            }

            await NoteDataManager.DeleteNote(_db, note);

            return new();
        }

        /// <summary>
        /// Gets the export json.  Well it's called json here because it was intended to be used to export a
        /// file as json.  But the fact is this is a good way to grab every thing the file contains 
        /// relevant to the requesting user.  In here you have the file object, all headers including their 
        /// content object and tag objects.  Finally, the users access token.
        /// Grab this and you have all you need to display a file.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="context">The context.</param>
        /// <returns>JsonExport.</returns>
        [Authorize]
        public override async Task<JsonExport> GetExportJson(ExportRequest request, ServerCallContext context)
        {
            JsonExport stuff = new()
            {
                NoteFile = _db.NoteFile.Single(p => p.Id == request.FileId)
            };

            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, stuff.NoteFile.Id, 0);
            if (!na.ReadAccess)
                return new JsonExport();

            stuff.NoteAccess = na;

            NoteHeadersRequest request2 = new()
            {
                NoteFileId = request.FileId,
                ArcId = request.ArcId,
                NoteOrdinal = -1,
                ContentAndTags = true,
                NestResponses = request.NestResponses
            };
            if (request.NoteOrdinal > 0)
                request2.NoteOrdinal = request.NoteOrdinal;

            stuff.NoteHeaders = await GetNoteHeaders(request2, context);

            return stuff;
        }

        /// <summary>
        /// Gets text from server for display in client.
        /// files: about.html | help.html | helpdialog.html | helpdialog2.html | license.html
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<AString> GetTextFile(AString request, ServerCallContext context)
        {
            AString stuff = new()
            {
                Val = string.Empty
            };

            if (request.Val == "syncfusionkey.rsghjjsrsrj43632353")
            {
                stuff.Val = _configuration["SyncfusionKey"];
                return stuff;
            }

            if (request.Val == "CookieName")
            {
                stuff.Val = _configuration["CookieName"];
                return stuff;
            }

            string myFileInput = Globals.ImportRoot + "Text\\" + request.Val;
            // Get the input file
            StreamReader file;
            try
            {
                file = new StreamReader(myFileInput);
            }
            catch
            {
                return stuff;
            }

            StringBuilder sb = new();
            string? line;
            while ((line = await file.ReadLineAsync()) is not null)
            {
                sb.AppendLine(line);
            }

            stuff.Val = sb.ToString();

            return stuff;
        }

        /// <summary>
        /// Gets the current homepage message if any
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task<AString> GetHomePageMessage(NoRequest request, ServerCallContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            AString? message = new();

            NoteFile? hpmf = _db.NoteFile.Where(p => p.NoteFileName == "homepagemessages").FirstOrDefault();
            if (hpmf is not null)
            {
                NoteHeader? hpmh = _db.NoteHeader.Where(p => p.NoteFileId == hpmf.Id && !p.IsDeleted).OrderByDescending(p => p.CreateDate).FirstOrDefault();
                if (hpmh is not null)
                {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    message.Val = _db.NoteContent.Where(p => p.NoteHeaderId == hpmh.Id).FirstOrDefault().NoteBody;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                }
            }

            return message;
        }

        /// <summary>
        /// Get a list of notes w or wo content and tags as specified by the request.
        /// Returns the same stuff as a JsonExport but without the Notefile and Access token.
        /// Also permits filtering to a limted degree.
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        [Authorize]
        public override async Task<NoteHeaderList> GetNoteHeaders(NoteHeadersRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId,request.ArcId);
            if (!na.ReadAccess)
                return new();

            List<NoteHeader> work =  new();

            if (request.NoteOrdinal == -1 && request.MinNote > 0 && request.MaxNote >= request.MinNote)      // base notes and responses
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                    && p.NoteOrdinal >= request.MinNote && p.NoteOrdinal <= request.MaxNote
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToListAsync();
            }
            else if (request.NoteOrdinal == -1) // base notes and responses
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal).ThenBy(p => p.ResponseOrdinal).ToListAsync();
            }
            else if (request.NoteOrdinal == 0 && request.MinNote > 0 && request.MaxNote >= request.MinNote)  // base notes only
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0
                    && p.NoteOrdinal >= request.MinNote && p.NoteOrdinal <= request.MaxNote
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal).ToListAsync();
            }
            else if (request.NoteOrdinal == 0)  // base notes only
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0 
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.NoteOrdinal).ToListAsync();
            }
            else if (request.ResponseOrdinal <= 0) // specifc base note plus all responses
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.NoteOrdinal == request.NoteOrdinal 
                    && !p.IsDeleted && p.Version == 0)
                    .OrderBy(p => p.ResponseOrdinal).ToListAsync();
            }
            else if (request.ResponseOrdinal == 0) // specifc base note 
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId
                    && p.ResponseOrdinal == 0  && p.NoteOrdinal == request.NoteOrdinal 
                    && !p.IsDeleted && p.Version == 0).ToListAsync();
            }
            else    // specific response
            {
                work = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId  && p.ResponseOrdinal == request.ResponseOrdinal 
                    && p.NoteOrdinal == request.NoteOrdinal 
                    && !p.IsDeleted && p.Version == 0).ToListAsync();
            }

            NoteHeaderList returnval = new();
            returnval.List.AddRange(work);

            if (request.ContentAndTags)
            {
                long[] items = work.Select(p => p.Id).ToArray();
                List<NoteContent> cont = await _db.NoteContent.Where(p => items.Contains(p.NoteHeaderId)).ToListAsync();
                List<Tags> tags = await (_db.Tags.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId)).ToListAsync();

                foreach (NoteHeader item in returnval.List)
                {
                    item.Content = cont.Single(p => p.NoteHeaderId == item.Id);
                    List<Tags> x = tags.Where(p => p.NoteHeaderId == item.Id).ToList();
                    item.Tags = Tags.GetGTagsList(x);
                }
            }

            if (request.NestResponses)
            {
                List<NoteHeader> bases = returnval.List.Where(p => p.ResponseOrdinal == 0).ToList();
                foreach (NoteHeader bn in bases)
                {
                    List<NoteHeader> rn = returnval.List.Where(p => p.NoteOrdinal == bn.NoteOrdinal && p.ResponseOrdinal > 0).OrderBy(p => p.ResponseOrdinal).ToList();
                    if (rn.Count == 0)
                        continue;
                    bn.Responses = new();
                    bn.Responses.List.AddRange(rn);
                    foreach (NoteHeader ln in rn)
                    {
                        returnval.List.Remove(ln);
                    }
                }
            }

            return returnval;
        }

        /// <summary>
        /// Gets the number of base notes.
        /// </summary>
        /// <param name="request">Specifies file and archive</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>Number of base notes.</returns>
        [Authorize]
        public override async Task<NoteCount> GetNoteCount(NoteFileRequest request, ServerCallContext context)
        {
            ApplicationUser appUser = await GetAppUser(context);
            NoteAccess na = await AccessManager.GetAccess(_db, appUser.Id, request.NoteFileId, request.ArcId);
            if (!na.ReadAccess)
                return new();

            NoteCount returnval = new()
            {
                Count = await _db.NoteHeader.Where(p => p.NoteFileId == request.NoteFileId && p.ArchiveId == request.ArcId && p.ResponseOrdinal == 0
                   && !p.IsDeleted && p.Version == 0).CountAsync()
            };

            return returnval;
        }

    }
}