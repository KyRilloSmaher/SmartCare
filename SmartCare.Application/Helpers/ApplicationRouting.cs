namespace SmartCare.API.Helpers
{
    /// <summary>
    /// Defines the API route constants used throughout the application.
    /// Provides a single source of truth for all endpoint paths.
    /// </summary>
    public static class ApplicationRouting
    {
        public const string Root = "api";
        public const string SingleRoute = "/{id:guid}";
        public const string Rule = Root + "/";

        #region Authentication Endpoints
        public static class Authentication
        {
            private const string Prefix = Rule + "auth/";

            public const string Login = Prefix + "login";
            public const string SignUp = Prefix + "sign-up";
            public const string ConfirmEmail = Prefix + "confirm-email";
            public const string ResendConfirmationEmail = Prefix + "resend-confirmation-email";
            public const string ForgotPassword = Prefix + "forgot-password";
            public const string ConfirmResetPasswordCode = Prefix + "confirm-reset-password-code";
            public const string ChangePassword = Prefix + "change-password";
            public const string SendResetCode = Prefix + "send-reset-code";
            public const string ResendResetCode = Prefix + "resend-reset-code";
            public const string ResetPassword = Prefix + "reset-password";
            public const string RefreshToken = Prefix + "refresh-token";
        }
        #endregion

        #region Category Endpoints
        public static class Category
        {
            private const string Prefix = Rule + "categories";
            private const string AdminPrefix = Rule + "admin/categories";

            public const string GetById = Prefix + SingleRoute;
            public const string SearchByName = Prefix + "/search";
            public const string GetAll = Prefix;
            public const string GetAllPaginated = Prefix + "/paginated";
            public const string GetAllForAdmin = AdminPrefix;
            public const string Create = AdminPrefix + "/create";
            public const string Update = AdminPrefix + "/update";
            public const string ChangeImage = AdminPrefix + "/change-image";
            public const string Delete = AdminPrefix + "/delete" + SingleRoute;
        }
        #endregion

        #region Company Endpoints
        public static class Company
        {
            private const string Prefix = Rule + "companies";
            private const string AdminPrefix = Rule + "admin/companies";

            public const string GetById = Prefix + SingleRoute;
            public const string SearchByName = Prefix + "/search";
            public const string GetAll = Prefix;
            public const string GetAllPaginated = Prefix + "/paginated";
            public const string GetAllForAdmin = AdminPrefix;
            public const string Create = AdminPrefix + "/create";
            public const string Update = AdminPrefix + "/update";
            public const string ChangeImage = AdminPrefix + "/change-logo";
            public const string Delete = AdminPrefix + "/delete" + SingleRoute;
        }
        #endregion

        #region Client Endpoints
        public static class Client
        {
            private const string Prefix = Rule + "users/clients";

            public const string GetById = Prefix + SingleRoute;
            public const string GetByEmail = Prefix + "/{email}";
            public const string GetAll = Prefix;
            public const string UpdateProfile = Prefix + "/me/update-profile";
            public const string ChangeProfileImage = Prefix + "/me/change-profile-image";
            public const string Delete = Prefix + "/delete" + SingleRoute;
        }
        #endregion

        #region Store Endpoints
        public static class Store
        {
            private const string Prefix = Rule + "stores";
            private const string AdminPrefix = Rule + "admin/stores";

            public const string GetById = Prefix + SingleRoute;
            public const string GetNearest = Prefix + "/nearest";
            public const string SearchByName = Prefix + "/search";
            public const string GetAll = Prefix;
            public const string GetAllForAdmin = AdminPrefix;
            public const string Create = AdminPrefix + "/create";
            public const string Update = AdminPrefix + "/update";
            public const string Delete = AdminPrefix + "/delete" + SingleRoute;
        }
        #endregion

        #region Rate Endpoints
        public static class Rate
        {
            private const string Prefix = Rule + "rates";

            public const string GetById = Prefix + SingleRoute;
            public const string GetAllForUser = Rule + "me/rates";
            public const string GetAllForProduct = Rule + "products" + SingleRoute + "/rates";
            public const string Create = Prefix + "/create";
            public const string Update = Prefix + "/update";
            public const string Delete = Prefix + "/delete" + SingleRoute;
        }
        #endregion

        #region Favourite
        public static class Favourite
        {
            private const string Prefix = Rule + "favourites";

            public const string GetAllForUser = Rule + "me/favourites";
            public const string Create = Prefix + "/add-to-my-favourites"+ "/{productId}";
            public const string Delete = Prefix + "/remove-from-my-favourites" + "/{productId}";
        }
        #endregion
    }
}
