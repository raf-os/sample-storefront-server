using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using SampleStorefront.Context;
using SampleStorefront.Models;
using SampleStorefront.Services;
using SampleStorefront.Settings;

var builder = WebApplication.CreateBuilder(args);

var JwtApiKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt key is null.");
var JwtValidIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new Exception("Jwt issuer is null.");
var JwtValidAudience = builder.Configuration["Jwt:Audience"] ?? throw new Exception("Jwt audience is null.");

builder.Services.AddControllers().AddNewtonsoftJson(opt =>
{
  opt.SerializerSettings.NullValueHandling =
      Newtonsoft.Json.NullValueHandling.Ignore;
});

builder.Services.AddMemoryCache();

builder.Services.AddOptions<CookieSettings>()
    .Bind(builder.Configuration.GetSection("CookieSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
  options.ShouldInclude = operation => operation.HttpMethod != null;
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
  options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresqlConnection"));
  options.UseSeeding((context, _) =>
  {
    var testUser = context.Set<User>().FirstOrDefault(x => x.Name == "Admin");
    if (testUser == null)
    {
      var hashedPassword = BCrypt.Net.BCrypt.HashPassword("1234");
      var baseAdminUser = new User
      {
        Name = "Admin",
        Email = "admin@internet.com",
        Password = hashedPassword
      };
      context.Set<User>().Add(baseAdminUser);
      context.SaveChanges();
    }
  });
  options.UseAsyncSeeding(async (context, _, cancellationToken) =>
  {
    var testUser = await context.Set<User>().FirstOrDefaultAsync(x => x.Name == "Admin", cancellationToken);
    {
      var hashedPassword = BCrypt.Net.BCrypt.HashPassword("1234");
      var baseAdminUser = new User
      {
        Name = "Admin",
        Email = "admin@internet.com",
        Password = hashedPassword
      };
      context.Set<User>().Add(baseAdminUser);
      await context.SaveChangesAsync(cancellationToken);
    }
  });
});

builder.Services.Configure<FormOptions>(options =>
{
  options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
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


builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddSingleton<GlobalServerSettings>();

JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddHostedService<ServerConfigService>();

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

