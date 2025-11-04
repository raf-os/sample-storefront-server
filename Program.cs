using System.Text;
using Microsoft.IdentityModel.Tokens;

using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Services;

var builder = WebApplication.CreateBuilder(args);

var JwtApiKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt key is null.");
var JwtValidIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new Exception("Jwt issuer is null.");
var JwtValidAudience = builder.Configuration["Jwt:Audience"] ?? throw new Exception("Jwt audience is null.");

builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddMemoryCache();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.ShouldInclude = operation => operation.HttpMethod != null;
});

builder.Services.AddDbContext<AppDbContext>();

builder.Services.AddAuthentication().AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtValidIssuer,
            ValidAudience = JwtValidAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtApiKey)),
            RoleClaimType = "role"
        };
    }
);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ModOnly",
        policy => policy.RequireRole(nameof(UserRole.Operator), nameof(UserRole.Admin)));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<CategoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
    });
}

app.UseExceptionHandler("/error");

app.UseRouting();
app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

