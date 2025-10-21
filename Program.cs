using System.Text;
using Microsoft.IdentityModel.Tokens;

using SampleStorefront.Context;
using SampleStorefront.Services;

var builder = WebApplication.CreateBuilder(args);

var JwtApiKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt key is null.");

builder.Services.AddControllers().AddNewtonsoftJson();

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
            ValidIssuer = "sampleStorefrontServer",
            ValidAudience = "sampleStorefrontClient",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtApiKey))
        };
    }
);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<PasswordService>();

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

