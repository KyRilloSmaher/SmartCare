using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.IServices;
using SmartCare.Application.Mappers;
using SmartCare.Application.Services;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.IRepositories;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.InfraStructure.BackgroundJobImplemantations;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.InfraStructure.ExternalServices;
using SmartCare.InfraStructure.Repositories;
using System.Security.Claims;
using System.Text;
using Hangfire;
using SmartCare.Application.Handlers.ResponsesHandler;

namespace SmartCare.InfraStructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // ---------- Repositories ----------
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IAdressRepository, AdressRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IRateRepository, RateRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IFavouriteRepository, FavouriteRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IReservationRepository , ReservationRepository>();


            services.AddScoped<IPaymentRepository, PaymentRepository>();
            // ---------- Identity ----------
            services.AddIdentity<Client, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDBContext>()
            .AddDefaultTokenProviders();

            // ---------- Application Services ----------
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IRateService, RateService>();
            services.AddScoped<IFavouriteService, FavouriteService>();
            services.AddScoped<ICartService, CartService > ();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
            services.AddScoped<IResponseHandler, ResponseHandler>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
            services.AddScoped<IResponseHandler, ResponseHandler>();
            services.AddScoped<IPaymentService, PaymentService>();

            // ---------- External Services ----------
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IImageUploaderService, ImageUploaderService>();
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IPaymentGetway, StripeService>();                

            // ---------- Configurations ----------
            services.Configure<StripeSettings>(configuration.GetSection("StripeSettings"));
            services.Configure<CloudinarySettings>(configuration.GetSection("cloudinary"));
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            var jwtSettings = new JwtSettings();
            configuration.GetSection("JwtSettings").Bind(jwtSettings);
            services.AddSingleton(jwtSettings);

            // ---------- Hangfire ----------
            services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("Cloud")));
            services.AddHangfireServer();

            // ---------- AutoMapper ----------
            services.AddAutoMapper(typeof(ClientMappingProfile));

            // ---------- JWT Authentication ----------
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateLifetime = jwtSettings.ValidateLifeTime,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                // -------- Custom JWT validation + SignalR support --------
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var securityStamp = context.Principal?.FindFirst("security_stamp")?.Value;

                        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(securityStamp))
                        {
                            context.Fail("Invalid token claims.");
                            return;
                        }

                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDBContext>();
                        var user = await dbContext.Users
                            .AsNoTracking()
                            .FirstOrDefaultAsync(u => u.Id == userId);

                        if (user == null)
                        {
                            context.Fail("User not found.");
                            return;
                        }

                        if (!string.Equals(user.SecurityStamp, securityStamp, StringComparison.Ordinal))
                        {
                            context.Fail("Security stamp mismatch - token revoked.");
                        }
                    },

                    OnMessageReceived = context =>
                    {
                        // Allow JWT in SignalR WebSocket requests
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        // ✅ No NameIdentifierUserIdProvider needed
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/payment"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}
