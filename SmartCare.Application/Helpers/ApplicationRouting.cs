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
            public const string Logout = Prefix + "logout";
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

        #region Favourite Endpoints
        public static class Favourite
        {
            private const string Prefix = Rule + "favourites";

            public const string GetAllForUser = Rule + "me/favourites";
            public const string Create = Prefix + "/add-to-my-favourites"+ "/{productId}";
            public const string Delete = Prefix + "/remove-from-my-favourites" + "/{productId}";
        }
        #endregion

        #region Cart Endpoints
        public static class Cart
        {
            private const string Prefix = Rule + "cart";
            private const string AdminPrefix = Rule + "admin/cart";

            public const string GetById = Prefix + SingleRoute;
            public const string GetForUser = Prefix + "/me";
            public const string AddItem = Prefix + "/add-item";
            public const string UpdateItem = Prefix + "/update-item";
            public const string RemoveItem = Prefix + "/remove-item";
            public const string Clear = Prefix + "/clear";
            public const string Delete = Prefix + "/{cartId}";
        }
        #endregion

        #region Product Endpoints
        public static class Product
        {
            private const string Prefix = Rule + "Products";
            private const string AdminPrefix = Rule + "admin/Products";

            public const string GetDetailsForUser = Rule + "user/Products";
            public const string GetDetailsForAdmin = Rule + "Admin/Products";
            public const string SearchBypartialDescription = Prefix + "/Description";
            public const string GetAll = Prefix;
            public const string Create = AdminPrefix + "/create";
            public const string Update = AdminPrefix + "/update";
            public const string GetByCompanyId = Prefix + "/CompanyId";
            public const string SearchByCompanyName = Prefix + "/CompanyName";
            public const string GetByCategoryId = Prefix + "/CategoryId";
            public const string SearchByCategoryName = Prefix + "/CategoryName";
            public const string GetExpired = Prefix + "/Expired";
            public const string GetUnExpired = Prefix + "/UnExpired";
            public const string GetBestSeller = Prefix + "/BestSeller";
            public const string SearchByName = Prefix + "/Name";
            public const string GetByFilter = Prefix + "/Filter";
            public const string Delete = AdminPrefix + "/delete" + SingleRoute;



        }
        #endregion

        #region Order Endpoints
        public static class Order
        {
            private const string Prefix = Rule + "orders";
            private const string AdminPrefix = Rule + "admin/orders";

            public const string GetById = Prefix + SingleRoute;
            public const string GetWithDetailsById = Prefix + "/details" + SingleRoute;
            public const string GetByCustomerId = Prefix + "/by-customer";
            public const string GetOrdersByCustomerAndStatus = Prefix + "/by-customer-and-status";
            public const string GetByStatus = AdminPrefix + "/by-status";
            public const string GetByDateRange = AdminPrefix + "/by-date-range";
            public const string GetAllWithDetails = Prefix + "/with-details";
            public const string GetTopNByValue = AdminPrefix + "/top-value";
            public const string GetRecent = AdminPrefix + "/recent";
            public const string GetTotalCount = AdminPrefix + "/total-count";
            public const string GetTotalRevenue = AdminPrefix + "/total-revenue";
            public const string GetCountByStatus = AdminPrefix + "/count-by-status";

            public const string Create = Prefix + "/create";
            public const string Update = Prefix + "/update";
            public const string UpdateStatus = AdminPrefix + "/update-status" + SingleRoute;
            public const string Delete = AdminPrefix + "/delete" + SingleRoute;
        }
        #endregion
    }
}
