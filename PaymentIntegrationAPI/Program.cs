using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentIntegrationAPI.Configuration;
using PaymentIntegrationAPI.Data;
using PaymentIntegrationAPI.Services.Implementations;
using PaymentIntegrationAPI.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// 1. DATABASE CONFIGURATION
// ============================================
builder.Services.AddDbContext<PaymentIntegrationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ============================================
// 2. CONFIGURATION BINDING
// ============================================
builder.Services.Configure<ESewaConfig>(
    builder.Configuration.GetSection("ESewa"));

// ============================================
// 3. HTTP CLIENT
// ============================================
builder.Services.AddHttpClient();

// ============================================
// 4. DEPENDENCY INJECTION - REPOSITORIES & SERVICES
// ============================================
// Repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Services
builder.Services.AddScoped<IESewaPaymentService, ESewaPaymentService>();

// ============================================
// 5. JWT AUTHENTICATION
// ============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JWT Secret key is not configured in secrets.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

// ============================================
// 6. AUTHORIZATION (if needed)
// ============================================
builder.Services.AddAuthorization();

// ============================================
// 7. CONTROLLERS & API EXPLORER
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ============================================
// 8. SWAGGER CONFIGURATION
// ============================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Payment Integration API",
        Version = "v1",
        Description = "API for eSewa Payment Gateway Integration"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// ============================================
// 9. CORS (Optional - Add if needed for frontend)
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://yourfrontend.com", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ============================================
// BUILD APPLICATION
// ============================================
var app = builder.Build();

// ============================================
// 10. MIDDLEWARE PIPELINE CONFIGURATION
// ============================================

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment API v1");
    });
}

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS (if configured above)
// app.UseCors("AllowFrontend");

// Authentication & Authorization (ORDER IS CRITICAL!)
app.UseAuthentication(); // First: Who are you?
app.UseAuthorization();  // Second: What can you do?

// Map Controllers
app.MapControllers();

// ============================================
// 11. RUN APPLICATION
// ============================================
app.Run();