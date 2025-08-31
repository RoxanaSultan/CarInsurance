using System.Text.Json.Serialization;

namespace CarInsurance.Api.Dtos;

public record CarDto(long Id, string Vin, string? Make, string? Model, int Year, long OwnerId, string OwnerName, string? OwnerEmail);
public record InsuranceValidityResponse(long CarId, string Date, bool Valid);
public record ClaimDto(DateOnly ClaimDate, string Description, decimal Amount);

[JsonDerivedType(typeof(PolicyHistoryDto))]
[JsonDerivedType(typeof(ClaimHistoryDto))]
public abstract record CarHistoryDto(string EventType, [property: JsonIgnore] DateOnly SortDate);

public record PolicyHistoryDto(
    DateOnly StartDate,
    DateOnly EndDate,
    string Provider
) : CarHistoryDto("Policy", StartDate);

public record ClaimHistoryDto(
    DateOnly ClaimDate,
    string Description,
    decimal Amount
) : CarHistoryDto("Claim", ClaimDate);