using Xunit;
using CarInsurance.Api.Services;
using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using CarInsurance.Api.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Tests;

public class CarServiceTests
{
    private readonly AppDbContext _db;
    private readonly CarService _service;

    public CarServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _service = new CarService(_db);

        SeedData();
    }

    private void SeedData()
    {
        var car = new Car { Id = 1, Vin = "1111", Make = "Test", Model = "X", YearOfManufacture = 2020 };
        _db.Cars.Add(car);

        var policy = new InsurancePolicy
        {
            Id = 1,
            CarId = 1,
            StartDate = new DateOnly(2024, 1, 1),
            EndDate = new DateOnly(2024, 12, 31),
            Provider = "Allianz"
        };
        _db.Policies.Add(policy);

        _db.SaveChanges();
    }

    [Fact]
    public async Task IsInsuranceValid_OnStartDate_ReturnsTrue()
    {
        var result = await _service.IsInsuranceValidAsync(1, new DateOnly(2024, 1, 1));
        Assert.True(result);
    }

    [Fact]
    public async Task IsInsuranceValid_OnEndDate_ReturnsTrue()
    {
        var result = await _service.IsInsuranceValidAsync(1, new DateOnly(2024, 12, 31));
        Assert.True(result);
    }

    [Fact]
    public async Task IsInsuranceValid_BeforeStartDate_ReturnsFalse()
    {
        var result = await _service.IsInsuranceValidAsync(1, new DateOnly(2023, 12, 31));
        Assert.False(result);
    }

    [Fact]
    public async Task IsInsuranceValid_AfterEndDate_ReturnsFalse()
    {
        var result = await _service.IsInsuranceValidAsync(1, new DateOnly(2025, 1, 1));
        Assert.False(result);
    }

    [Fact]
    public async Task IsInsuranceValid_CarNotExist_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.IsInsuranceValidAsync(999, new DateOnly(2024, 1, 1)));
    }

    [Fact]
    public async Task IsInsuranceValid_InvalidDateFormat_ReturnsBadRequest()
    {
        var controller = new CarsController(_service);
        var result = await controller.IsInsuranceValid(1, "2024-13-01");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid date or date format. Use YYYY-MM-DD or a valid calendar date.", badRequest.Value);
    }

    [Fact]
    public async Task IsInsuranceValid_ImpossibleDate_ReturnsBadRequest()
    {
        var controller = new CarsController(_service);
        var result = await controller.IsInsuranceValid(1, "2024-02-30");
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid date or date format. Use YYYY-MM-DD or a valid calendar date.", badRequest.Value);
    }

    [Fact]
    public async Task InsuranceValid_CarNotExist_ReturnsNotFound()
    {
        var controller = new CarsController(_service);
        var result = await controller.IsInsuranceValid(999, "2024-01-01");
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Car 999 not found", notFound.Value);
    }
}
