using Microsoft.AspNetCore.Mvc;
using ProlabWeb.Api.Models.Dto;
using ProlabWeb.Api.Services;

namespace ProlabWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LabResultsController : ControllerBase
{
    private readonly ILabResultService _labResultService;
    private readonly ILogger<LabResultsController> _logger;

    public LabResultsController(ILabResultService labResultService, ILogger<LabResultsController> logger)
    {
        _labResultService = labResultService;
        _logger = logger;
    }

    // api/LabResults
    [HttpPost]
    public async Task<IActionResult> ProcessLabResults([FromBody] List<LabResultDto> labResults)
    {
        try
        {
            if (labResults == null || !labResults.Any())
            {
                return BadRequest("Aucun résultat de laboratoire fourni");
            }

            var list = await _labResultService.ProcessLabResultsAsync(labResults);
            
            _logger.LogInformation($"{list.Count} résultats de laboratoire traités avec succès");
            
            return Ok(new { 
                Message = "Résultats traités avec succès", 
                Data = list 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du traitement des résultats de laboratoire");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    // api/LabResults
    [HttpGet]
    public async Task<IActionResult> GetLabResults([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var results = await _labResultService.GetLabResultsAsync(page, pageSize);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des résultats");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLabResult(int id)
    {
        try
        {
            var result = await _labResultService.GetLabResultByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération du résultat {id}");
            return StatusCode(500, "Erreur interne du serveur");
        }
    }
}