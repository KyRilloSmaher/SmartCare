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

        #region EndPoints-Category
        public static class Category
        {
            public const string Prefix = Rule + "categories";
            public const string GetCategoryById = Prefix + SignleRoute;
            public const string SearchCategoryByName = Prefix+ "/search";
            public const string GetAllCategories = Prefix ;
            public const string GetAllCategoriesForAdmin = Rule+ "admin/"+Prefix;
            public const string UpdateCategory = Prefix + "/update-category";
            public const string ChangeCategoryImage = Prefix + "/change-category-logo";
            public const string CreateCategory = Prefix + "/create-category";
            public const string DeleteCategory = Prefix + "/Delete" + SignleRoute;

        }
        #endregion

        #region EndPoints-Company
        public static class Company
        {
            public const string Prefix = Rule + "Companies";
            public const string GetCompanyById = Prefix + SignleRoute;
            public const string SearchCompanyByName = Prefix;
            public const string GetAllCompanies = Prefix + "/search";
            public const string GetAllCompaniesForAdmin = Rule + "admin/" + Prefix;
            public const string UpdateCompany = Prefix + "/update-company";
            public const string ChangeCompanyImage = Prefix + "/change-company-logo";
            public const string CreateCompany = Prefix + "/create-company";
            public const string DeleteCompany = Prefix + "/Delete" + SignleRoute;

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

        #region EndPoints-Store
        public static class Store
        {
            public const string Prefix = Rule + "Stores";

            public const string GetStoreById = Prefix + SignleRoute;
            public const string GetNearestStore = Prefix + "/nearest";
            public const string SearchStoresByName = Prefix;
            public const string GetAllStores = Prefix + "/search";
            public const string GetAllStoresForAdmin = Rule + "admin/" + "Stores";

            public const string CreateStore = Prefix + "/create-store";
            public const string UpdateStore = Prefix + "/update-store";
            public const string DeleteStore = Prefix + "/Delete" + SignleRoute;
        }
        #endregion

    }
}