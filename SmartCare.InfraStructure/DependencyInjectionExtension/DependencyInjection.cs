using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SmartCare.Application.commens;
using SmartCare.Application.ExternalServiceInterfaces;
using SmartCare.Application.ExternalServiceInterfaces.AI;
using SmartCare.Application.ExternalServiceInterfaces.Payments;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.Handlers.ResponsesHandler;
using SmartCare.Application.IServices;
using SmartCare.Application.Mappings;
using SmartCare.Domain.Entities;
using SmartCare.Domain.Helpers;
using SmartCare.Domain.Interfaces.IServices;
using SmartCare.Domain.IRepositories;
using SmartCare.Infrastructure.Data;
using SmartCare.Infrastructure.Repositories;
using SmartCare.InfraStructure.BackgroundJobImplemantations;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.InfraStructure.ExternalServices;
using SmartCare.InfraStructure.ExternalServices.Payments;
using SmartCare.InfraStructure.Repositories;
using SmartCare.InfraStructure.Services;
using System.Security.Claims;
using System.Text;
using Polly.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;

namespace SmartCare.InfraStructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            // ---------- Repositories ----------
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<IPharmacistRepository, PharmacistRepository>();
            services.AddScoped<IRateRepository, RateRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IFavouriteRepository, FavouriteRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IReservationRepository , ReservationRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

            services.AddScoped<ISalesRepository, SalesRepository>();

            // ---------- Identity ----------
            services.AddIdentity<ApplictionUser, IdentityRole>(options =>
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
            //services.AddScoped<IAuthenticationService, AuthenticationService>();
            //services.AddScoped<IAddressService, AddressService>();
            //services.AddScoped<ICategoryService, CategoryService>();
            //services.AddScoped<ICompanyService, CompanyService>();
            //services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ITokenService, TokenService>();
            //services.AddScoped<IStoreService, StoreService>();
            //services.AddScoped<IRateService, RateService>();
            //services.AddScoped<IFavouriteService, FavouriteService>();
            //services.AddScoped<ICartService, CartService > ();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
            services.AddScoped<IResponseHandler, ResponseHandler>();
           // services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
            services.AddScoped<IResponseHandler, ResponseHandler>();
            //services.AddScoped<IPaymentService, PaymentService>();
            //services.AddScoped<IOrderService, OrderService>();
            //services.AddScoped<IinventoryService, InventoryService>();
            services.AddScoped<ISqlLockManager, SqlLockManager>();

            // ---------- External Services ----------
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IImageUploaderService, ImageUploaderService>();
            services.AddScoped<IMapService, MapService>();
            services.AddScoped<IPaymentGetway, StripeService>();
            services.AddScoped<IPaymentGetway, PaymobService>();
            services.Configure<PaymobSettings>(configuration.GetSection("Paymob"));
            services.AddHttpClient<PaymobService>();
            // AI Services
            var baseUrl = configuration["AiCore:BaseUrl"]?? throw new InvalidOperationException("AiCore:BaseUrl is missing from appsettings.");

            services
                .AddHttpClient<IAiServices, AiCoreService>(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = TimeSpan.FromSeconds(
                        configuration.GetValue("AiCore:TimeoutSeconds", 30));
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                })
                // Retry 3x: waits 2s → 4s → 8s
                .AddPolicyHandler((sp, _) =>
                {
                    var logger = sp.GetRequiredService<ILogger<AiCoreService>>();
                    return HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(3,
                            attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                            (outcome, delay, attempt, _) => logger.LogWarning(
                                "AiCore retry {Attempt}/3 after {Delay}s — {Reason}",
                                attempt, delay.TotalSeconds,
                                outcome.Exception?.Message ?? outcome.Result.ReasonPhrase));
                })
                // Circuit breaker: opens after 5 failures, resets after 30s
                .AddPolicyHandler((sp, _) =>
                {
                    var logger = sp.GetRequiredService<ILogger<AiCoreService>>();
                    return HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                            onBreak: (o, d) => logger.LogError(
                                                      "AiCore circuit OPEN for {D}s", d.TotalSeconds),
                            onReset: () => logger.LogInformation("AiCore circuit CLOSED"),
                            onHalfOpen: () => logger.LogInformation("AiCore circuit HALF-OPEN"));
                });
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
            services.AddAutoMapper(typeof(CartMappingProfile));

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
