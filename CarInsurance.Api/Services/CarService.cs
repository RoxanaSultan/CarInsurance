using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            (p.EndDate == null || p.EndDate >= date)
        );
    }

    public async Task FileClaimAsync(long carId, ClaimDto claimDto)
    {
        var car = await _db.Cars.FindAsync(carId);
        if (car == null) throw new KeyNotFoundException($"Car {carId} not found");

        var claim = new Models.Claim
        {
            CarId = carId,
            ClaimDate = claimDto.ClaimDate,
            Description = claimDto.Description,
            Amount = claimDto.Amount
        };

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();
    }

    public async Task<List<CarHistoryDto>> GetCarHistoryAsync(long carId)
    {
        var car = await _db.Cars.FindAsync(carId);
        if (car == null)
            throw new KeyNotFoundException($"Car {carId} not found");

        var policies = await _db.Policies
            .Where(p => p.CarId == carId)
            .Select(p => new PolicyHistoryDto(
                p.StartDate,
                p.EndDate,
                p.Provider
            ))
            .Cast<CarHistoryDto>()
            .ToListAsync();

        var claims = await _db.Claims
            .Where(c => c.CarId == carId)
            .Select(c => new ClaimHistoryDto(
                c.ClaimDate,
                c.Description,
                c.Amount
            ))
            .Cast<CarHistoryDto>()
            .ToListAsync();

        return policies
            .Concat(claims)
            .OrderBy(e => e.SortDate)
            .ToList(); ;
    }
}