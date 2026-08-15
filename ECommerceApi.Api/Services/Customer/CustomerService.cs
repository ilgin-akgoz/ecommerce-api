using AutoMapper;
using ECommerceApi.Api.Data;
using ECommerceApi.Api.Dtos;
using ECommerceApi.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ECommerceDbContext _db;
    private readonly IMapper _mapper;

    public CustomerService(ECommerceDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var customers = await _db.Customers.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        return customer is null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var emailTaken = await _db.Customers.AnyAsync(c => c.Email == dto.Email);
        if (emailTaken)
            throw new InvalidOperationException($"A customer with email '{dto.Email}' already exists.");

        var customer = _mapper.Map<Customer>(dto);
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return _mapper.Map<CustomerDto>(customer);
    }
}