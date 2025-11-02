
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartCare.InfraStructure.DbContexts;
using SmartCare.InfraStructure.Extensions;
using FluentValidation;
using SmartCare.Application.DTOs.Auth.Requests;
using SmartCare.API.Helpers;
using SmartCare.Application.Handlers.ResponseHandler;
using SmartCare.API.Middlewares;
using SmartCare.InfraStructure.Seed;
using Hangfire;
using SmartCare.API.Hubs;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//Swagger Gn
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartCare_Application_API ", Version = "v1" });
    c.EnableAnnotations();

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme

    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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


builder.Services.AddLogging();
builder.Services.AddHttpClient();

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

#region Dependency injections
builder.Services.AddInfrastructureDependencies(builder.Configuration);
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUrlHelper>(x =>
    x.GetRequiredService<IUrlHelperFactory>().GetUrlHelper(x.GetRequiredService<IActionContextAccessor>().ActionContext));


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
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}
app.MapHub<PaymentsHub>("/hubs/payments");
app.UseSwagger();
app.UseSwaggerUI();
app.UseHangfireDashboard("/hangfire");
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();