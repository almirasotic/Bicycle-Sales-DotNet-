using bicycleBackend.Data;
using bicycleBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ? Konfiguracija CORS-a
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// ? Dodaj kontrolere
builder.Services.AddControllers();

// ? Swagger konfiguracija
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BikeShop API", Version = "v1" });

    // ? JWT autentifikacija u Swagger-u
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Unesi JWT token u formatu: Bearer {tvoj_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
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
});

// ? Konekcija sa bazom podataka (SQL Server)
builder.Services.AddDbContext<DataContext>(op =>
    op.UseSqlServer(builder.Configuration.GetConnectionString("default")));

// ? Registracija AuthService-a
builder.Services.AddScoped<AuthService>();

// ? JWT autentifikacija
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// ? Omogu?i autorizaciju
builder.Services.AddAuthorization();

var app = builder.Build();

// ? Omogu?i Swagger u development modu
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ? Omogu?i CORS (Angular može komunicirati sa backendom)
app.UseCors("AllowAngularApp");

// ? HTTPS redirekcija
app.UseHttpsRedirection();

// ? Omogu?i autentifikaciju i autorizaciju
app.UseAuthentication();
app.UseAuthorization();

// ? Loguj greške u konzoli (za debugging)
//app.UseExceptionHandler("/error");

// ? Mapiranje kontrolera
app.MapControllers();
app.UseStaticFiles();

app.Run();
