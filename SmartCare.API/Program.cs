
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartCare.API.EventHandlers;
using SmartCare.API.Helpers;
using SmartCare.API.Hubs;
using SmartCare.API.InMemoryEventsHandlers;
using SmartCare.API.Middlewares;
using SmartCare.API.Services;
using SmartCare.Application.CQRs.Payment.Extensions;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.Application.InMemoryEventsHandlers;
using SmartCare.Application.IServices;
using SmartCare.Application.Messaging;
using SmartCare.Application.Notifications;
using SmartCare.Domain.Enums;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.InfraStructure.Extensions;
using SmartCare.InfraStructure.Messaging;
using SmartCare.InfraStructure.Seed;
using SmartCare.InfraStructure.Services;
using StackExchange.Redis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();


#region Swagger_Gn
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartCare_Application_API ", Version = "v1" });
        options.EnableAnnotations();

        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = JwtBearerDefaults.AuthenticationScheme

        });
        options.UseInlineDefinitionsForEnums(); // optional but cleaner

        options.MapType<OrderStatus>(() => new Microsoft.OpenApi.Models.OpenApiSchema
        {
            Type = "string",
            Enum = Enum.GetNames(typeof(OrderStatus))
                       .Select(name => new Microsoft.OpenApi.Any.OpenApiString(name))
                       .Cast<Microsoft.OpenApi.Any.IOpenApiAny>()
                       .ToList()
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                     {
                     new OpenApiSecurityScheme
                     {
                         Reference = new OpenApiReference
                         {
                             Type = ReferenceType.SecurityScheme,
                             Id = JwtBearerDefaults.AuthenticationScheme
                         }
                     },
                     Array.Empty<string>()
                     }
                   });
    });

// To Convert Enum From 0,1,.. To string In Swagger
//builder.Services.AddControllers().AddJsonOptions(options =>
//{
//    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
//});
         
#endregion

#region Connection To SQL SERVER

var connectionString = builder.Configuration.GetConnectionString("Cloud");

builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.UseNetTopologySuite();
    });
});


#endregion

#region Connection To RedisCaching

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");


if (redisConnectionString != null && redisConnectionString.StartsWith("redis://"))
{
    redisConnectionString = redisConnectionString.Replace("redis://", "");
}

builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration = redisConnectionString;
    option.InstanceName = "SmartCare_";
});

#endregion

#region Dependency injections
builder.Services.AddInfrastructureDependencies(builder.Configuration);
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUrlHelper>(x =>
    x.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext));
builder.Services.AddScoped<HtmlTemplateService>();

#region MediatR Registration
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(SmartCare.Application.CQRs.Favourite.Commands.CreateFavouriteAsyncCommand).Assembly);
});
#endregion

#endregion

#region AllowCORS
var CORS = "_DefaultCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CORS,
                      policy =>
                      {
                          policy.AllowAnyHeader();
                          policy.AllowAnyMethod();
                          policy.AllowAnyOrigin();
                      });
});

#endregion

#region  Register-FluentValidation
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining(typeof(ChangePasswordRequestDto));
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
            );

        var apiResponse = ControllersHelperMethods.FinalResponse(
         new Response<bool>
         {
             StatusCode = System.Net.HttpStatusCode.BadRequest,
             ErrorsBag = errors,
             Data = false,
             Message = "Validation Errors !"
         }
        );

        return apiResponse;
    };
});
#endregion

#region Logging Configuration
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day,
                  outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));
#endregion

#region  SignalR
builder.Services.AddSignalR();
#endregion

#region Event Bus Registration

// Register Event Bus
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<IEventPublisherService, EventPublisherService>();
builder.Services.AddScoped<INotificationSender, SignalRNotificationSender>();
builder.Services.AddScoped<IOrderNotificationService, OrderNotificationService>();
builder.Services.AddScoped<PaymentExtensions>();
// Add  events handlers
builder.Services.AddScoped<PaymentStatusChangedHandler>();
builder.Services.AddScoped<ProductStockStatusChangedHandler>();
builder.Services.AddScoped<ReservationExpiredEventHandler>();
builder.Services.AddScoped<OrderExpireAlertHandler>();

#endregion

builder.Services.Configure<InputSanitizationMiddleware>(
    builder.Configuration.GetSection("InputSanitization"));

var app = builder.Build();


#region Seeding Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await RoleSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles.");
    }
}
#endregion
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<PaymentStatusChangedHandler>();
    scope.ServiceProvider.GetRequiredService<ProductStockStatusChangedHandler>();
    scope.ServiceProvider.GetRequiredService<ReservationExpiredEventHandler>();
    scope.ServiceProvider.GetRequiredService<OrderExpireAlertHandler>();
}




app.UseMiddleware<RateLimitingMiddleware>();


app.UseHttpsRedirection();



app.UseSwagger();
app.UseSwaggerUI();
app.UseHangfireDashboard("/hangfire");


app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(CORS);

app.MapControllers();

app.MapHub<PaymentsHub>("/hubs/payments");
app.MapHub<ProductsHub>("/hubs/products");
app.MapHub<CartHub>("/hubs/cart");
app.MapHub<OrderHub>("/hubs/orders");
app.MapHub<UserNotificationHub>("/hubs/users");
app.MapHub<PharmacistHub>("/hubs/pharmacist");

app.Run();
