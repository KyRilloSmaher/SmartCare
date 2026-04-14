using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCare.Domain.Constants
{
    public static class CacheConstants
    {
        public const string Products = "Products_tag";
        public const string Categories = "categories_tag";
        public const string Companies = "companies_tag";
        public const string Stories = "Stories_tag";
        public const string Addresses = "Adresses_tag";
        public const string Client = "Client_tag";
        public const string Pharmacist = "pharmacist_tag";
        public const string Favourite = "Favourites_tag";
        public const string Rates = "Rates_tag";
        public const string Orders = "orders_tag";


        public const string CategoriesAllAdmin = "categories_all_admin";
        public const string CategoriesClient = "categories_all_client";
        public const string Category = "category";

        public const string CompanyiesAllAdmin = "companies_list_admin";
        public const string CompaniesClient = "companies_list_client";
        public const string Company= "company";

    }

    public static class Time
    {
        public static readonly TimeSpan Default = TimeSpan.FromMinutes(60);
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan Long = TimeSpan.FromHours(24);
    }
}
