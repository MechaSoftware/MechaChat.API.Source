using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechaChat.API.Models
{
    public class UserCreateModel
    {
        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string DateOfBirth { get; set; } // MM/DD/YYYY //
    }

    public class UserLoginModel
    {
        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class ChangeStatusModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string UserEmail { get; set; }

        [Required]
        public string UserAccessToken { get; set; }

        [Required]
        public string UserStatus { get; set; }

        [Required]
        public string UserStatusMsg { get; set; }

        [Required]
        public string UserMood { get; set; }
    }

    public class ChangeThemeModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string UserEmail { get; set; }

        [Required]
        public string UserAccessToken { get; set; }

        [Required]
        public string UserTheme { get; set; }
    }

    public class UserRefreshModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        [Required]
        public string UserAccessToken { get; set; }
    }

    public class UserAccountsModel
    {
        [Description("user_id")]
        public string UserId { get; set; }

        [Description("user_email")]
        public string UserEmail { get; set; }

        [Description("user_username")]
        public string UserUsername { get; set; }

        [Description("user_discriminator")]
        public string UserDiscriminator { get; set; }

        [Description("user_password")]
        public string UserPassword { get; set; }

        [Description("user_dateofbirth")]
        public string UserDateOfBirth { get; set; }

        [Description("user_status")]
        public int UserStatus { get; set; }

        [Description("user_statusmsg")]
        public string UserStatusMsg { get; set; }

        [Description("user_mood")]
        public string UserMood { get; set; }

        [Description("user_roles")]
        public string UserRoles { get; set; }

        [Description("user_profilepic")]
        public string UserProfilePic { get; set; }

        [Description("user_systemtheme")]
        public string UserSystemTheme { get; set; }

        [Description("user_isbot")]
        public string UserIsBot { get; set; }

        [Description("user_isbanned")]
        public string UserIsBanned { get; set; }

        [Description("user_isdeleted")]
        public string UserIsDeleted { get; set; }

        [Description("access_token")]
        public string UserAccessToken { get; set; }
    }
}
