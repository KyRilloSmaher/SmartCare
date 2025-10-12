namespace SmartCare.API.Helpers
{
    public static class ApplicationRouting
    {
        public const string SignleRoute = "/{Id:GUID}";
        public const string root = "SmartCare/api";
        public const string Rule = root + "/";

        #region   EndPoints-Authentication

        public static class Authentication
        {
            public const string Prefix = Rule + "auth";
            public const string Login = Prefix + "/login";
            public const string SignUp = Prefix + "/sign-up";
            public const string ConfirmEmail = Prefix + "/confirm-email";
            public const string ForgotPassword = Prefix + "/forgot-password";
            public const string ConfirmResetPasswordCode = Prefix + "/confirm-reset-password-code";
            public const string ChangePassword = Prefix + "/change-password";
            public const string SendResetCode = Prefix + "/send-reset-code";
            public const string ResetPassword = Prefix + "/reset-password";
            public const string RefreshToken = Prefix + "/Refresh-Token";
        }
        #endregion
    }
}
