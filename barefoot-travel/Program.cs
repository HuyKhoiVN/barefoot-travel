using AutoMapper;
using barefoot_travel.Common;
using barefoot_travel.Common;
using barefoot_travel.Models;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure routing
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = false;
});

// Database Configuration
builder.Services.AddDbContext<SysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Authorization
builder.Services.AddAuthorization();

// CORS Configuration
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? new[] { "*" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        if (corsOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(corsOrigins);
        }
        policy.AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger/OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Barefoot Travel API", 
        Version = "v1",
        Description = "API for Barefoot Travel Management System"
    });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var actionDescriptor = apiDesc.ActionDescriptor;
        return actionDescriptor.EndpointMetadata.Any(em => em is Microsoft.AspNetCore.Mvc.ApiControllerAttribute);
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // File Upload Support
    c.OperationFilter<FileUploadOperationFilter>();
});

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(Program));

// FluentValidation Configuration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Repository Registration
builder.Services.AddScoped<barefoot_travel.Repositories.IAccountRepository, barefoot_travel.Repositories.AccountRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IRoleRepository, barefoot_travel.Repositories.RoleRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourRepository, barefoot_travel.Repositories.TourRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourImageRepository, barefoot_travel.Repositories.TourImageRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourCategoryRepository, barefoot_travel.Repositories.TourCategoryRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourPriceRepository, barefoot_travel.Repositories.TourPriceRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourPolicyRepository, barefoot_travel.Repositories.TourPolicyRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ICategoryRepository, barefoot_travel.Repositories.CategoryRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IPolicyRepository, barefoot_travel.Repositories.PolicyRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IBookingRepository, barefoot_travel.Repositories.BookingRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IBookingStatusRepository, barefoot_travel.Repositories.BookingStatusRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IPriceTypeRepository, barefoot_travel.Repositories.PriceTypeRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IHomePageFeaturedTourRepository, barefoot_travel.Repositories.HomePageFeaturedTourRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IHomePageSelectedTourRepository, barefoot_travel.Repositories.HomePageSelectedTourRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IHomePageSectionRepository, barefoot_travel.Repositories.HomePageSectionRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IHomePageSectionCategoryRepository, barefoot_travel.Repositories.HomePageSectionCategoryRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IHomePageSectionTourRepository, barefoot_travel.Repositories.HomePageSectionTourRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IDashboardRepository, barefoot_travel.Repositories.DashboardRepository>();

// Service Registration
builder.Services.AddScoped<barefoot_travel.Services.IAuthService, barefoot_travel.Services.AuthService>();
builder.Services.AddScoped<barefoot_travel.Services.IUserService, barefoot_travel.Services.UserService>();
builder.Services.AddScoped<barefoot_travel.Services.IRoleService, barefoot_travel.Services.RoleService>();
builder.Services.AddScoped<barefoot_travel.Services.ITourService, barefoot_travel.Services.TourService>();
builder.Services.AddScoped<barefoot_travel.Services.ICategoryService, barefoot_travel.Services.CategoryService>();
builder.Services.AddScoped<barefoot_travel.Services.IPolicyService, barefoot_travel.Services.PolicyService>();
builder.Services.AddScoped<barefoot_travel.Services.IBookingService, barefoot_travel.Services.BookingService>();
builder.Services.AddScoped<barefoot_travel.Services.IBookingStatusService, barefoot_travel.Services.BookingStatusService>();
builder.Services.AddScoped<barefoot_travel.Services.IPriceTypeService, barefoot_travel.Services.PriceTypeService>();
builder.Services.AddScoped<barefoot_travel.Services.IFileUploadService, barefoot_travel.Services.FileUploadService>();
builder.Services.AddScoped<barefoot_travel.Services.IHomePageService, barefoot_travel.Services.HomePageService>();
builder.Services.AddScoped<barefoot_travel.Services.IHomePageSectionService, barefoot_travel.Services.HomePageSectionService>();
builder.Services.AddScoped<barefoot_travel.Services.IFeaturedDailyToursService, barefoot_travel.Services.FeaturedDailyToursService>();
builder.Services.AddScoped<barefoot_travel.Services.IDashboardService, barefoot_travel.Services.DashboardService>();

// HTML Sanitizer for XSS protection
//builder.Services.AddHtmlSanitizer();

// JwtMiddleware is registered as middleware, not as a service

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<SysDbContext>();
        await DatabaseSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Enable Swagger in Development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Barefoot Travel API v1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// CORS must be before UseRouting
app.UseCors("AllowSpecificOrigins");

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom JWT Middleware
app.UseMiddleware<JwtMiddleware>();

// Slug-based category tours route (NEW - PRIORITY)
// Example: /categories/ha-long-bay
app.MapControllerRoute(
    name: "categoriesBySlug",
    pattern: "categories/{slug}",
    defaults: new { controller = "Categories", action = "Index" },
    constraints: new { 
        slug = @"^[a-z0-9-]+$"  // Only lowercase, numbers, hyphens
    });

// Slug-based tour details route
// Example: /tours/ha-long-bay-2-day-cruise
app.MapControllerRoute(
    name: "toursBySlug",
    pattern: "tours/{slug}",
    defaults: new { controller = "Tours", action = "Index" },
    constraints: new { 
        slug = @"^[a-z0-9-]+$"  // Only lowercase, numbers, hyphens
    });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
