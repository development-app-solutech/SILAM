using System.ComponentModel.DataAnnotations;

namespace ProlabWeb.Api.Models;

public class LabResult
{
    [Key]
    public int Id { get; set; }
    
    public string RawMessage { get; set; } = string.Empty;
    
    // Patient Info
    public string? PatientId { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Sex { get; set; }
    
    // Order Info
    public string? SpecimenId { get; set; }
    public string? OrderingPhysician { get; set; }
    
    public DateTime ReceivedAt { get; set; }
    
    // Navigation properties
    public List<TestResult> TestResults { get; set; } = new();
}

public class TestResult
{
    [Key]
    public int Id { get; set; }
    
    public int LabResultId { get; set; }
    public LabResult LabResult { get; set; } = null!;
    
    public string SpecimenId { get; set; } = string.Empty;
    public string UniversalTestId { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public string DataMeasurementValue { get; set; } = string.Empty;
    public string? Units { get; set; }
    public string? ReferenceRanges { get; set; }
    public string? ResultAbnormalFlags { get; set; }
    public double? NumericValue { get; set; }
    public bool IsNumeric { get; set; }
    public string? ResultStatus { get; set; }
}