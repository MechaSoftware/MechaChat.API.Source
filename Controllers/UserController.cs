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
using MechaChat.API.Handlers;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;

namespace MechaChat.API.Controllers
{
    [RoutePrefix("v1/user")]
    [EnableCors(origins: "*", headers: "*", methods: "*", SupportsCredentials = false, PreflightMaxAge = 86400)]
    public class UserController : ApiController
    {
        public static Functions Functions = new Functions();

        /// <summary>
        /// Updates the users status with the info they provided.
        /// </summary>
        /// <response code="200">The account is valid and is the real user, Update their status.</response>
        /// <response code="403">The account is not found.</response>
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status200OK)]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Route("update/status")]
        [Authorize(Roles = "system")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateUserStatusEndpoint(ChangeStatusModel userChangeStatusForm)
        {
            var userAccessToken = Functions.EncodeStringBase58(userChangeStatusForm.UserAccessToken);

            var userObj = await SelectSingleQuery<UserAccountsModel>($"SELECT * FROM `user_accounts` WHERE `user_email` = '{userChangeStatusForm.UserEmail}' AND `user_id` = '{userChangeStatusForm.UserId}' AND `access_token` = '{userAccessToken}' LIMIT 1");

            if (userObj != null)
            {
                if (userChangeStatusForm.UserStatusMsg.Length > 0)
                {
                    var newMessage = userChangeStatusForm.UserStatusMsg.Replace("'", "\\\'");

                    await ExecuteQuery($"UPDATE `user_accounts` SET user_status = '{int.Parse(userChangeStatusForm.UserStatus)}' WHERE `user_id` = '{userChangeStatusForm.UserId}'");
                    await ExecuteQuery($"UPDATE `user_accounts` SET user_statusmsg = '{newMessage}' WHERE `user_id` = '{userChangeStatusForm.UserId}'");
                    await ExecuteQuery($"UPDATE `user_accounts` SET user_mood = '{userChangeStatusForm.UserMood}' WHERE `user_id` = '{userChangeStatusForm.UserId}'");
                }
                else
                {
                    await ExecuteQuery($"UPDATE `user_accounts` SET user_status = '{int.Parse(userChangeStatusForm.UserStatus)}' WHERE `user_id` = '{userChangeStatusForm.UserId}'");
                    await ExecuteQuery($"UPDATE `user_accounts` SET user_statusmsg = NULL WHERE `user_id` = '{userChangeStatusForm.UserId}'");
                    await ExecuteQuery($"UPDATE `user_accounts` SET user_mood = '{userChangeStatusForm.UserMood}' WHERE `user_id` = '{userChangeStatusForm.UserId}'");
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Status Changed");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, "No Account Found");
            }
        }

        /// <summary>
        /// Updates the users status with the info they provided.
        /// </summary>
        /// <response code="200">The account is valid and is the real user, Update their status.</response>
        /// <response code="403">The account is not found.</response>
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status200OK)]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Route("update/theme/default")]
        [Authorize(Roles = "system")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateUserThemeEndpoint(ChangeThemeModel userChangeThemeForm)
        {
            var userAccessToken = Functions.EncodeStringBase58(userChangeThemeForm.UserAccessToken);

            var userObj = await SelectSingleQuery<UserAccountsModel>($"SELECT * FROM `user_accounts` WHERE `user_email` = '{userChangeThemeForm.UserEmail}' AND `user_id` = '{userChangeThemeForm.UserId}' AND `access_token` = '{userAccessToken}' LIMIT 1");

            if (userObj != null)
            {
                await ExecuteQuery($"UPDATE `user_accounts` SET user_systemtheme = 'default_{userChangeThemeForm.UserTheme}' WHERE `user_id` = '{userChangeThemeForm.UserId}'");

                return Request.CreateResponse(HttpStatusCode.OK, "Theme Changed");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden, "No Account Found");
            }
        }

        /// <summary>
        /// Fetches the use they requested by their ID.
        /// </summary>
        /// <response code="404">The account with the is you provided is not in the database, Cannot get the info.</response>
        /// <response code="200">The account you are trying to get is in the database, Send account info to client.</response>
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status404NotFound)]
        [Microsoft.AspNetCore.Mvc.ProducesResponseType(StatusCodes.Status200OK)]
        [Route("fetch/{userid?}")]
        [Authorize(Roles = "bot,system")]
        [HttpGet]
        public async Task<HttpResponseMessage> FetchSingle(string userid = "0")
        {
            var userObj = await SelectSingleQuery<UserAccountsModel>($"SELECT * FROM `user_accounts` WHERE `user_id` = '{userid}' LIMIT 1");

            if (userObj == null) return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User Not Found!");

            var userData = new
            {
                userId = userObj.UserId,
                userName = userObj.UserUsername,
                userDiscriminator = userObj.UserDiscriminator,
                userStatus = userObj.UserStatus,
                userStatusMessage = userObj.UserStatusMsg,
                userProfilePicHash = userObj.UserProfilePic,
                userSystemRoles = userObj.UserRoles,
                userIsBot = userObj.UserIsBot,
                userIsDeleted = userObj.UserIsDeleted,
                userIsBanned = userObj.UserIsBanned
            };

            return Request.CreateResponse(HttpStatusCode.OK, userData);
        }
    }
}
