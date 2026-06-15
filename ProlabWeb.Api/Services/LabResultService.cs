using ProlabWeb.Api.Data;
using ProlabWeb.Api.Models;
using ProlabWeb.Api.Models.Dto;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Api.Db;
using NCalc;

namespace ProlabWeb.Api.Services;

public interface ILabResultService
{
    Task<List<Detailresultat>> ProcessLabResultsAsync(List<LabResultDto> labResultDtos);
    Task<List<LabResult>> GetLabResultsAsync(int page = 1, int pageSize = 10);
    Task<LabResult?> GetLabResultByIdAsync(int id);
}

public class LabResultService : ILabResultService
{
    private readonly AutomateContext _context;

    private readonly ProlabwebContext _prolabwebContext;
    private readonly ILogger<LabResultService> _logger;

    public LabResultService(AutomateContext context, ProlabwebContext prolabwebContext, ILogger<LabResultService> logger)
    {
        _context = context;
        _prolabwebContext = prolabwebContext;
        _logger = logger;
    }

    public async Task<List<Detailresultat>> ProcessLabResultsAsync(List<LabResultDto> labResultDtos)
    {
        var processedCount = 0;
        var allDetailresultatsToUpdate = new List<Detailresultat>();

        foreach (var dto in labResultDtos)
        {
            try
            {
                var labResult = new LabResult
                {
                    RawMessage = dto.RawMessage,
                    PatientId = dto.Patient.PatientId,
                    LastName = dto.Patient.LastName,
                    FirstName = dto.Patient.FirstName,
                    BirthDate = dto.Patient.BirthDate?.ToUniversalTime(),
                    Sex = dto.Patient.Sex,
                    SpecimenId = dto.Orders.FirstOrDefault()?.SpecimenId,
                    OrderingPhysician = dto.Orders.FirstOrDefault()?.OrderingPhysician,
                    ReceivedAt = dto.ReceivedAt.ToUniversalTime()
                };

                // Trouver le patient correspondant
                var patient = await _prolabwebContext.Patients.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Code == dto.Patient.PatientId);

                // Liste temporaire pour les détails résultats à mettre à jour
                var detailresultatsToUpdate = new List<Detailresultat>();

                // Ajouter les résultats de tests
                foreach (var result in dto.Results)
                {
                    // Chercher le paramètre correspondant au UniversalTestId
                    var parametre = await _prolabwebContext.Parametres.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Code == result.UniversalTestId);

                    // Chercher le détail résultat le plus récent pour ce patient et ce paramètre
                    Detailresultat? detailresultat = null;
                    if (patient != null && parametre != null)
                    {
                        detailresultat = await _prolabwebContext.Detailresultats.AsNoTracking()
                            .Include(dr => dr.Enteteresultat)
                                .ThenInclude(er => er.Entetedemande)
                                    .ThenInclude(ed => ed.Patient)
                            .Include(dr => dr.Enteteresultat)
                                .ThenInclude(er => er.IdanalyseNavigation)
                            .Include(dr => dr.Enteteresultat)
                                .ThenInclude(er => er.Technicien)
                            .Include(dr => dr.Enteteresultat)
                                .ThenInclude(er => er.Biologiste)
                            .Include(dr => dr.Parametre)
                            .Where(dr => dr.Enteteresultat.Entetedemande.Patientid == patient.Patientid)
                            .Where(dr => dr.Parametreid == parametre.Parametreid)
                            .OrderByDescending(dr => dr.Date)
                            .FirstOrDefaultAsync();
                    }

                    // Remplir la propriété résultat si détail résultat trouvé
                    if (detailresultat != null && result.NumericValue.HasValue)
                    {
                        // Calculer automatiquement le commentaire basé sur les valeurs de référence
                        int patientAge = 0;
                        string patientSexe = "";
                        if (patient != null)
                        {
                            var dateNaissance = patient.Datenaissance;
                            if (dateNaissance != DateTime.MinValue)
                            {
                                patientAge = DateTime.Now.Year - dateNaissance.Year;
                                if (DateTime.Now.DayOfYear < dateNaissance.DayOfYear)
                                    patientAge--;
                            }
                            patientSexe = patient.Codesexe ?? "";
                        }
                        
                        string commentaireAuto = await CalculerCommentaireAutomatique(
                            detailresultat.Enteteresultat.Idanalyse, 
                            ((decimal)result.NumericValue.Value).ToString(), 
                            patientAge, 
                            patientSexe
                        );
                        
                        var detailresultatToUpdate = new Detailresultat
                        {
                            Detailresultatid = detailresultat.Detailresultatid,
                            Enteteresultatid = detailresultat.Enteteresultatid,
                            Parametreid = detailresultat.Parametreid,
                            Date = detailresultat.Date,
                            Commentaire = commentaireAuto, // Utiliser le commentaire auto-calculé
                            Databuilder = detailresultat.Databuilder,
                            Resultat = ((decimal)result.NumericValue.Value).ToString(),
                            Resultatsi = !string.IsNullOrEmpty(parametre.Formuleautomate) 
                                ? CalculateFormula(parametre.Formuleautomate, (decimal)result.NumericValue.Value).ToString() 
                                : !string.IsNullOrWhiteSpace(detailresultat.Resultatsi) ? detailresultat.Resultatsi.ToString() : null
                        };
                        
                        detailresultatsToUpdate.Add(detailresultatToUpdate);
                    }

                    labResult.TestResults.Add(new TestResult
                    {
                        SpecimenId = result.SpecimenId ?? string.Empty,
                        UniversalTestId = result.UniversalTestId ?? string.Empty,
                        TestName = result.TestName ?? string.Empty,
                        DataMeasurementValue = result.DataMeasurementValue ?? string.Empty,
                        Units = result.Units,
                        ReferenceRanges = result.ReferenceRanges,
                        ResultAbnormalFlags = result.ResultAbnormalFlags,
                        NumericValue = result.NumericValue,
                        IsNumeric = result.IsNumeric,
                        ResultStatus = result.ResultStatus
                    });
                }

                // Ajouter les détails résultats à mettre à jour au contexte
                _prolabwebContext.Detailresultats.UpdateRange(detailresultatsToUpdate);
                allDetailresultatsToUpdate.AddRange(detailresultatsToUpdate);

                _context.LabResults.Add(labResult);
                processedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du résultat de laboratoire");
                return new List<Detailresultat>();
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            await _prolabwebContext.SaveChangesAsync();
            
            return allDetailresultatsToUpdate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde");
            return new List<Detailresultat>();
        }
    }

    public async Task<List<LabResult>> GetLabResultsAsync(int page = 1, int pageSize = 10)
    {
        return await _context.LabResults
            .Include(lr => lr.TestResults)
            .OrderByDescending(lr => lr.ReceivedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<LabResult?> GetLabResultByIdAsync(int id)
    {
        return await _context.LabResults
            .Include(lr => lr.TestResults)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    private decimal? CalculateFormula(string formula, decimal value)
    {
        try
        {
            var expression = new NCalc.Expression(formula.Replace("x", value.ToString()));
            
            // Vérifier si la formule est correcte
            if (expression.HasErrors())
            {
                _logger.LogWarning($"Formule incorrecte: {formula}");
                return null;
            }
            
            var result = expression.Evaluate();
            return Convert.ToDecimal(result);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Erreur lors de l'application de la formule: {formula}");
            return null;
        }
    }

    /// <summary>
    /// Calcule automatiquement le commentaire (Haut/Bas/Normal) basé sur les valeurs de référence
    /// </summary>
    /// <param name="analyseId">ID de l'analyse</param>
    /// <param name="resultatStr">Résultat sous forme de string</param>
    /// <param name="age">Âge du patient</param>
    /// <param name="sexe">Sexe du patient</param>
    /// <returns>Commentaire calculé (Haut/Bas/Normal)</returns>
    private async Task<string> CalculerCommentaireAutomatique(Guid analyseId, string resultatStr, int age, string sexe)
    {
        try
        {
            // Si pas de résultat, retourner vide
            if (string.IsNullOrWhiteSpace(resultatStr))
                return "";
            
            // Essayer de parser le résultat en nombre
            if (!decimal.TryParse(resultatStr.Replace(",", "."), System.Globalization.NumberStyles.Float, 
                System.Globalization.CultureInfo.InvariantCulture, out decimal resultat))
            {
                // Si ce n'est pas un nombre, retourner Normal par défaut
                return "Normal";
            }
            
            // Récupérer les valeurs de référence pour cette analyse, âge et sexe
            var valeurRef = await _prolabwebContext.Valeurreferences
                .Where(vr => vr.Idanalyse == analyseId)
                .Where(vr => 
                    // Filtrer par sexe si spécifié (H/M considérés comme homme, F comme femme, HF tous)
                    string.IsNullOrEmpty(sexe) ||
                    vr.Sexeautorisecode == "HF" ||
                    (vr.Sexeautorisecode == "H" && (sexe == "H" || sexe == "M")) ||
                    (vr.Sexeautorisecode == "M" && (sexe == "H" || sexe == "M")) ||
                    (vr.Sexeautorisecode == "F" && sexe == "F") ||
                    vr.Sexeautorisecode == sexe
                )
                .Where(vr => 
                    // Filtrer par âge si spécifié
                    age == 0 || 
                    (vr.Agedebut == null || age >= vr.Agedebut) &&
                    (vr.Agefin == null || age <= vr.Agefin)
                )
                .FirstOrDefaultAsync();
            
            // Fallbacks intelligents si aucune valeur de référence n'est trouvée
            if (valeurRef == null)
            {
                if (resultat < 0)
                    return "Bas";
                return "Normal";
            }
            
            // Comparer le résultat avec les valeurs de référence
            var minRef = valeurRef.Referencefromvalue;
            var maxRef = valeurRef.Referencetovalue;
            
            // Si aucune borne n'est définie, appliquer une règle simple: les négatifs sont "Bas"
            if (!minRef.HasValue && !maxRef.HasValue)
            {
                return resultat < 0 ? "Bas" : "Normal";
            }
            
            // Si valeur inférieure au minimum de référence
            if (minRef.HasValue && resultat < minRef.Value)
                return "Bas";
            
            // Si valeur supérieure au maximum de référence
            if (maxRef.HasValue && resultat > maxRef.Value)
                return "Haut";
            
            // Sinon, dans la plage normale
            return "Normal";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul automatique du commentaire pour l'analyse {AnalyseId}", analyseId);
            return "Normal"; // Valeur par défaut en cas d'erreur
        }
    }
}


















