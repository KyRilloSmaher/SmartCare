
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
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Hangfire;
using SmartCare.InfraStructure.BackgroundJobImplemantations;


namespace SmartCare.InfraStructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // Register Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IAdressRepository, AdressRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IRateRepository, RateRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IFavouriteRepository, FavouriteRepository>();

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
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IRateService, RateService>();
            services.AddScoped<IFavouriteService, FavouriteService>();

            // Register External Services 
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IImageUploaderService, ImageUploaderService>();
            services.AddScoped<IMapService, MapService>();
            //services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

            // Register Automapper
            services.AddAutoMapper(typeof(ClientMappingProfile));


            // Register CloudinaryService
            var cloudinary = new CloudinarySettings();
            configuration.GetSection("cloudinary").Bind(cloudinary);
            services.AddSingleton(cloudinary);

            // Hangfire setup
            services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("Cloud")));
            services.AddHangfireServer();

            // Some Classes
            services.AddScoped<IResponseHandler , ResponseHandler>();

            // Email
            var emailSettings = new EmailSettings();
            configuration.GetSection(nameof(emailSettings)).Bind(emailSettings);
            services.AddSingleton(emailSettings);

            // JWT Settings
            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.AddSingleton(jwtSettings);

            // Configure Authentication
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
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                x.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                        if (string.IsNullOrEmpty(userId))
                        {
                            context.Fail("No user id in token.");
                            return;
                        }

                        var tokenStamp = context.Principal?.FindFirst("security_stamp")?.Value;
                        if (string.IsNullOrEmpty(tokenStamp))
                        {
                            context.Fail("No security stamp in token.");
                            return;
                        }

                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<Client>>();
                        var user = await userManager.FindByIdAsync(userId);
                        if (user == null)
                        {
                            context.Fail("User not found.");
                            return;
                        }

                        var currentStamp = await userManager.GetSecurityStampAsync(user);
                        if (!string.Equals(tokenStamp, currentStamp, StringComparison.Ordinal))
                        {
                            context.Fail("Security stamp mismatch - token revoked.");
                            return;
                        }

                    }
                };
            });

            return services;
        }
    }
}
