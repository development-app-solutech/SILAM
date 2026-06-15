namespace ProlabWeb.Api.Models.Dto;

public class LabResultDto
{
    public string RawMessage { get; set; } = string.Empty;
    public object? Header { get; set; }
    public PatientDto Patient { get; set; } = new();
    public List<OrderDto> Orders { get; set; } = new();
    public List<ResultDto> Results { get; set; } = new();
    public object? Terminator { get; set; }
    public DateTime ReceivedAt { get; set; }
}

public class PatientDto
{
    public string? PatientId { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Sex { get; set; }
    public string? Race { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AttendingPhysician { get; set; }
    // ... autres propriétés selon besoin
}

public class OrderDto
{
    public string? SpecimenId { get; set; }
    public string? InstrumentSpecimenId { get; set; }
    public string? UniversalTestId { get; set; }
    public string? OrderingPhysician { get; set; }
    // ... autres propriétés selon besoin
}

public class ResultDto
{
    public string? SpecimenId { get; set; }
    public string? UniversalTestId { get; set; }
    public string? TestName { get; set; }
    public string? DataMeasurementValue { get; set; }
    public string? Units { get; set; }
    public string? ReferenceRanges { get; set; }
    public string? ResultAbnormalFlags { get; set; }
    public double? NumericValue { get; set; }
    public bool IsNumeric { get; set; }
    public string? ResultStatus { get; set; }
}