using System.Text;
using System.Text.Json.Serialization;
using InventoryManagement.Api.Middleware;
using InventoryManagement.Infrastructure;
using InventoryManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configuración de Autenticación con JWT Bearer
var jwtSecret = builder.Configuration["Jwt:SecretKey"] ?? "ClaveSecretaSuperSeguraParaInventoryManagement2026!Minimo32CaracteresRequeridos";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "InventoryManagement";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "InventoryManagementUsers";

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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// Configuración de Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Sistema de Gestión de Inventario y Ventas API",
        Version = "v1",
        Description = "API RESTful empresarial con Clean Architecture, JWT y SQL Server"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Description = "Ingresa el token JWT de la siguiente manera: Bearer {tu_token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Type = Microsoft.OpenApi.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    var schemeRef = new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer");
    c.AddSecurityRequirement(doc => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        {
            schemeRef,
            new List<string>()
        }
    });
});

// Registro de dependencias de la capa de Infraestructura
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Inicializar la base de datos y sembrar datos de prueba
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(context);
}

// Configuración del pipeline HTTP
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Management API v1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
