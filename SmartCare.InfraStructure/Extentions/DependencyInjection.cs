
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Helpers;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.InfraStructure.Repositories;
using SmartCare.Domain.IRepositories;
using SmartCare.Application.Services;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Application.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.InfraStructure.ExternalServices;
using SmartCare.Application.Mappers;
using SmartCare.Application.Handlers.ResponseHandler;


namespace SmartCare.InfraStructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Repositories
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IClientRepository, ClientRepository>();
            services.AddTransient<IAdressRepository, AdressRepository>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<ICompanyRepository, CompanyRepository>();
            services.AddTransient<IStoreRepository, StoreRepository>();
            services.AddTransient<IRateRepository, RateRepository>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddTransient<IFavouriteRepository, FavouriteRepository>();

            // Configure Identity
            services.AddIdentity<Client, IdentityRole>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;

                // Sign-in settings
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDBContext>()
            .AddDefaultTokenProviders();

            // Register Services
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<ICompanyService, CompanyService>();
            services.AddTransient<IClientService, ClientService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IStoreService, StoreService>();
            services.AddTransient<IRateService, RateService>();
            services.AddTransient<IFavouriteService, FavouriteService>();

            // Register External Services 

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IImageUploaderService, ImageUploaderService>();
            services.AddTransient<IMapService, MapService>();
            //services.AddTransient<IPaymentService, PaymentService>();

            // Register Automapper
            services.AddAutoMapper(typeof(ClientMappingProfile));


            // Register CloudinaryService
            var cloudinary = new CloudinarySettings();
            configuration.GetSection("cloudinary").Bind(cloudinary);
            services.AddSingleton(cloudinary);

            // Some Classes
            services.AddTransient<IResponseHandler , ResponseHandler>();

            // Email
            var emailSettings = new EmailSettings();
            configuration.GetSection(nameof(emailSettings)).Bind(emailSettings);
            services.AddSingleton(emailSettings);

            // JWT Settings
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.AddSingleton(jwtSettings);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
               x.RequireHttpsMetadata = false;
               x.SaveToken = true;
               x.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = jwtSettings.ValidateIssuer,
                   ValidIssuers = new[] { jwtSettings.Issuer },
                   ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                   ValidAudience = jwtSettings.Audience,
                   ValidateAudience = jwtSettings.ValidateAudience,
                   ValidateLifetime = jwtSettings.ValidateLifeTime,
               };
            });

            return services;
        }
    }
}
