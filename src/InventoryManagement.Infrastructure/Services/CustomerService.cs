using InventoryManagement.Application.DTOs.Customer;
using InventoryManagement.Application.Interfaces;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly ApplicationDbContext _context;

    public CustomerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                FullName = c.FullName,
                DocumentNumber = c.DocumentNumber,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address
            })
            .ToListAsync();
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var c = await _context.Customers.FindAsync(id);
        if (c == null) return null;

        return new CustomerDto
        {
            Id = c.Id,
            FullName = c.FullName,
            DocumentNumber = c.DocumentNumber,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address
        };
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var exists = await _context.Customers.AnyAsync(c => c.DocumentNumber == dto.DocumentNumber);
        if (exists)
            throw new DomainException($"Ya existe un cliente registrado con el documento '{dto.DocumentNumber}'.");

        var customer = new Customer
        {
            FullName = dto.FullName,
            DocumentNumber = dto.DocumentNumber,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            DocumentNumber = customer.DocumentNumber,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address
        };
    }
}
