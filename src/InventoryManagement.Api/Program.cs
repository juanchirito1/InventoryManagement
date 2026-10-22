using System.Text.Json.Serialization;
using InventoryManagement.Api.Middleware;
using InventoryManagement.Infrastructure;
using InventoryManagement.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializar Enums como strings legibles en JSON (ej. "Pending", "Completed")
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configuración de Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Swagger habilitado tanto en desarrollo como pruebas
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Management API v1");
    c.RoutePrefix = string.Empty; // Cargar Swagger directamente en la raíz "/"
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
