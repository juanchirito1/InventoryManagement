using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Categories.AnyAsync())
        {
            return; // Ya fue inicializada
        }

        var electronics = new Category { Name = "Electrónica", Description = "Dispositivos, gadgets y accesorios" };
        var storage = new Category { Name = "Almacenamiento", Description = "Discos duros, SSDs y memorias" };
        var peripherals = new Category { Name = "Periféricos", Description = "Teclados, ratones y monitores" };

        context.Categories.AddRange(electronics, storage, peripherals);
        await context.SaveChangesAsync();

        var products = new List<Product>
        {
            new Product("TECH-001", "Monitor Gaming 27'' IPS 165Hz", 280.00m, 190.00m, 15, 3, peripherals.Id)
            {
                Description = "Monitor con resolución QHD y tiempo de respuesta de 1ms."
            },
            new Product("TECH-002", "Teclado Mecánico RGB Switch Red", 65.00m, 35.00m, 40, 5, peripherals.Id)
            {
                Description = "Teclado compacto 75% con switches intercambiables."
            },
            new Product("TECH-003", "SSD NVMe M.2 1TB Gen4", 85.00m, 50.00m, 4, 5, storage.Id)
            {
                Description = "Disco de alta velocidad de lectura hasta 7000 MB/s (Alerta stock bajo)."
            },
            new Product("TECH-004", "Mouse Inalámbrico Ergonómico", 45.00m, 22.00m, 25, 5, peripherals.Id)
            {
                Description = "Sensor óptico 16000 DPI con batería recargable."
            },
            new Product("TECH-005", "Hub USB-C 7 en 1 HDMI 4K", 35.00m, 18.00m, 2, 5, electronics.Id)
            {
                Description = "Adaptador multifunción de aluminio (Alerta stock crítico)."
            }
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();

        var customers = new List<Customer>
        {
            new Customer
            {
                FullName = "Carlos Mendoza",
                DocumentNumber = "71829384",
                Email = "carlos.mendoza@email.com",
                Phone = "+51 987654321",
                Address = "Av. Las Palmeras 123, Lima"
            },
            new Customer
            {
                FullName = "Empresa Soluciones Tech S.A.C.",
                DocumentNumber = "20601234567",
                Email = "compras@solucionestech.com",
                Phone = "+51 912345678",
                Address = "Calle Los Robles 450, San Isidro"
            }
        };

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();
    }
}
