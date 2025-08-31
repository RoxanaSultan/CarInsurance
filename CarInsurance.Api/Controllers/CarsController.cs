using CarInsurance.Api.Dtos;
using CarInsurance.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarInsurance.Api.Controllers;

[ApiController]
[Route("api")]
public class CarsController(CarService service) : ControllerBase
{
    private readonly CarService _service = service;

    [HttpGet("cars")]
    public async Task<ActionResult<List<CarDto>>> GetCars()
        => Ok(await _service.ListCarsAsync());

    [HttpGet("cars/{carId:long}/insurance-valid")]
    public async Task<ActionResult<InsuranceValidityResponse>> IsInsuranceValid(long carId, [FromQuery] string date)
    {
        if (!DateOnly.TryParse(date, out var parsed))
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");

        try
        {
            var valid = await _service.IsInsuranceValidAsync(carId, parsed);
            return Ok(new InsuranceValidityResponse(carId, parsed.ToString("yyyy-MM-dd"), valid));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("cars/{carId}/claims")]
    public async Task<ActionResult> FileClaim(long carId, [FromBody] ClaimDto claimDto)
    {
        try
        {
            await _service.FileClaimAsync(carId, claimDto);
            return Ok($"Claim filed for car with id: {carId}.");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("cars/{carId}/history")]
    public async Task<ActionResult<List<CarHistoryDto>>> GetCarHistory(long carId)
    {
        try
        {
            return Ok(await _service.GetCarHistoryAsync(carId));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
