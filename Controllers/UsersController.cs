using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Net;
using static MechaChat.API.Handlers.MySQLHandler;
using System.Net.Http;
using CryptSharp;
using System;
using Newtonsoft.Json;
using MechaChat.API.Models;
using MechaChat.API.Attributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;

namespace MechaChat.API.Controllers
{
    [RoutePrefix("v1/users")]
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = false, PreflightMaxAge = 86400)]
    public class UsersController : ApiController
    {
        public static Functions Functions = new Functions();

        /// <summary>
        /// Refreshes the users session token so they stay logged in to the website.
        /// </summary>
        /// <response code="404">The account is not found, Cannot refresh the session token.</response>
        /// <response code="403">The account is banned, Cannot refresh the session token, and sends a request to log the user out.</response>
        /// <response code="200">The account has been found, Refreshes the session token.</response>
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status404NotFound)]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status200OK)]
        [Route("refreshtoken")]
        [Authorize(Roles = "system")]
        [HttpPost]
        public async Task<HttpResponseMessage> RefreshCurrentJWTokenAsync(UserRefreshModel refreshData)
        {
            var jwtokengen = Functions.GenerateJSONWebToken();

            var userAccessToken = Functions.EncodeStringBase58(refreshData.UserAccessToken);

            var userObj = await SelectSingleQuery<UserAccountsModel>($"SELECT * FROM `user_accounts` WHERE `user_email` = '{refreshData.EmailAddress}' AND `user_id` = '{refreshData.UserId}' AND `access_token` = '{userAccessToken}' LIMIT 1");

            if (userObj == null || bool.Parse(userObj.UserIsDeleted)) return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Account not found!");

            if (bool.Parse(userObj.UserIsBanned)) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Account is banned!");

            var decodedAccessToken = Functions.DecodeStringBase58(userObj.UserAccessToken);

            var jsonUserData = new
            {
                userid = userObj.UserId,
                avatar = userObj.UserProfilePic,
                username = userObj.UserUsername,
                emailaddress = userObj.UserEmail,
                systemroles = userObj.UserRoles,
                systemtheme = userObj.UserSystemTheme,
                discriminator = userObj.UserDiscriminator,
                status = userObj.UserStatus,
                statusMsg = userObj.UserStatusMsg,
                mood = userObj.UserMood
            };

            var jsonData = new
            {
                accountStore = jsonUserData,
                tokenStore = new
                {
                    accesstoken = decodedAccessToken,
                    jwtoken = jwtokengen
                }
            };

            return Request.CreateResponse(HttpStatusCode.OK, jsonData);
        }

        /// <summary>
        /// Creates a new user with the data provided.
        /// </summary>
        /// <response code="201">All info provided is valid, Will create a new user with the info provided.</response>
        /// <response code="400">One of the inputs where not valid, Cannot create the account.</response>
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status201Created)]
        [Route("create")]
        [Authorize(Roles = "system")]
        [HttpPost]
        public async Task<HttpResponseMessage> CreateUserAccount(UserCreateModel userCreationForm)
        {
            if (userCreationForm.EmailAddress == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Please enter an email!");
            if (userCreationForm.Username == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Please enter a username!");
            if (userCreationForm.Password == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Please enter a password!");
            if (userCreationForm.Password.Length < 6) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Password must be 6 or more characters!");
            if (userCreationForm.DateOfBirth == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Please enter your date of birth!");

            var userCreationData = new UserCreateModel
            {
                EmailAddress = userCreationForm.EmailAddress,
                Username = userCreationForm.Username,
                DateOfBirth = userCreationForm.DateOfBirth
            };

            var userAccountId = Functions.IdentifierGen();

            var IsEmailInUse = await SelectSingleQuery<UserAccountsModel>($"SELECT * FROM `user_accounts` WHERE `user_email` = '{userCreationData.EmailAddress}' LIMIT 1");

            if (IsEmailInUse != null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Email address is already in use.");

            //var userDiscriminator = Functions.();

            var newUserCreateData = new
            {
                newUserId = userAccountId,
                newUserData = new
                {
                    EmailAddress = userCreationForm.EmailAddress,
                    Username = userCreationForm.Username,
                    DateOfBirth = userCreationForm.DateOfBirth
                }
            };

            return Request.CreateResponse(HttpStatusCode.Created, newUserCreateData);
        }

        /// <summary>
        /// Sign the user in with the data they provided.
        /// </summary>
        /// <response code="200">All info provided is correct, Will sign the user in.</response>
        /// <response code="404">The user cannot be found with the info provided, Cannot sign the user in.</response>
        /// <response code="403">The password the user provided does not match, Or the user has been banned, Cannot sign the user in.</response>
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status200OK)]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status404NotFound)]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Route("login")]
        [Authorize(Roles = "system")]
        [HttpPost]
        public async Task<HttpResponseMessage> CheckAuthenticationAsync(UserLoginModel userLoginForm)
        {
            if (userLoginForm.EmailAddress == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Please enter your email!");
            if (userLoginForm.Password == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Please enter your password!");

            var userObj = await SelectSingleQuery<UserAccountsModel>($"SELECT * FROM `user_accounts` WHERE `user_email` = '{userLoginForm.EmailAddress}' LIMIT 1");

            if (userObj == null || bool.Parse(userObj.UserIsDeleted)) return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Email or Password is incorrect!");

            if (!Crypter.CheckPassword(userLoginForm.Password, userObj.UserPassword)) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Email or Password is incorrect");

            if (bool.Parse(userObj.UserIsBanned)) return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "This account has been banned!");

            var jwtokengen = Functions.GenerateJSONWebToken();

            var decodedAccessToken = Functions.DecodeStringBase58(userObj.UserAccessToken);

            var jsonUserData = new {
                userid = userObj.UserId,
                avatar = userObj.UserProfilePic,
                username = userObj.UserUsername,
                emailaddress = userObj.UserEmail,
                discriminator = userObj.UserDiscriminator,
                systemroles = userObj.UserRoles,
                systemtheme = userObj.UserSystemTheme,
                status = userObj.UserStatus,
                statusMsg = userObj.UserStatusMsg,
                mood = userObj.UserMood
            };

            var jsonData = new
            {
                accountStore = jsonUserData,
                tokenStore = new
                {
                    accesstoken = decodedAccessToken,
                    jwtoken = jwtokengen
                }
            };

            return Request.CreateResponse(HttpStatusCode.OK, jsonData);
        }
    }
}
