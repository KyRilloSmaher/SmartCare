namespace SmartCare.API.Helpers
{
    public static class ApplicationRouting
    {
        public const string SignleRoute = "/{id:guid}";
        public const string root = "api";
        public const string Rule = root + "/";

        #region EndPoints-Authentication
        public static class Authentication
        {
            public const string Prefix = Rule + "auth/";
            public const string Login = Prefix + "login";
            public const string SignUp = Prefix + "sign-up";
            public const string ConfirmEmail = Prefix + "confirm-email";
            public const string ReSendConfirmationEmail = Prefix + "resend-confirmation-email";
            public const string ForgotPassword = Prefix + "forgot-password";
            public const string ConfirmResetPasswordCode = Prefix + "confirm-reset-password-code";
            public const string ChangePassword = Prefix + "change-password";
            public const string SendResetCode = Prefix + "send-reset-code";
            public const string ReSendResetCode = Prefix + "resend-reset-code";
            public const string ResetPassword = Prefix + "reset-password";
            public const string RefreshToken = Prefix + "refresh-token";  
        }
        #endregion

        #region EndPoints-Client
        public static class Client
        {
            public const string Prefix = Rule + "Users/clients";
            public const string GetClientById = Prefix + SignleRoute;
            public const string GetClientByEmail = Prefix + "/{email}";
            public const string GetAllClients = Prefix;
            public const string UpdateClient = Prefix + "/update-profile";
            public const string ChangeProfileImage = Prefix + "/me/change-profile-image";
            public const string DeleteClient = Prefix + "/Delete"+SignleRoute;

        }
        #endregion   
    }
}