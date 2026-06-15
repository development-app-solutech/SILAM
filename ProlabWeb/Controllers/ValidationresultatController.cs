using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Data;
using ProlabWeb.Db;
using ProlabWeb.ViewModels;
using System.Linq;
using System.Text.RegularExpressions;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class ValidationresultatController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly UserManager<ProlabIdentityUser> _usermanager;
        private readonly ILogger<ValidationresultatController> _logger;

        public ValidationresultatController(ProlabwebContext context, UserManager<ProlabIdentityUser> userManager, ILogger<ValidationresultatController> logger)
        {
            _context = context;
            _usermanager = userManager;
            _logger = logger;
        }

        // GET: Validationresultat
        public async Task<IActionResult> Index()
        {
            // utilisateur connecté
            var user = await _usermanager.GetUserAsync(User);

            Utilisateur utilisateur = null;

            List<int> laboratoires = new List<int>();

            if (user != null)
            {
                var roles = await _usermanager.GetRolesAsync(user);

                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Include(x => x.CodesiteNavigation)
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();

                if (utilisateur != null && roles.Contains(EnumRoles.Biologiste.ToString()))
                {
                    laboratoires = await _context.Utilisateurlaboratoires.AsNoTracking()
                        .Include(x => x.IdlaboratoireNavigation)
                        .Where(x => x.Utilisateurid == utilisateur.Utilisateurid)
                        .Select(x => x.IdlaboratoireNavigation.Idlaboratoire)
                        .ToListAsync();
                }
            }

            var resultats = await _context.Enteteresultats
                .Include(e => e.CodesiteNavigation)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Patient)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Prescripteur)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Policeassurance)
                        .ThenInclude(pa => pa.CodeassuranceNavigation)
                .Include(e => e.IdanalyseNavigation)
                    .ThenInclude(a => a.Categorieanalyses)
                        .ThenInclude(ca => ca.Categorie)
                .Include(e => e.Technicien)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            // filtrer par les laboratoires du biologiste

            if (laboratoires.Any())
            {
                resultats = resultats
                    .Where(x => laboratoires.Contains(x.IdanalyseNavigation.Idlaboratoire))
                    .ToList();
            }

            return View(resultats);
        }

        // GET: Validationresultat/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Enteteresultats == null)
            {
                return NotFound();
            }

            var enteteresultat = await _context.Enteteresultats
                .Include(e => e.CodesiteNavigation)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Patient)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Prescripteur)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Policeassurance)
                        .ThenInclude(pa => pa.CodeassuranceNavigation)
                .Include(e => e.Entetedemande)
                    .ThenInclude(d => d.Detaildemandes)
                        .ThenInclude(dd => dd.Prelevements)
                            .ThenInclude(p => p.IdnatureechantillonNavigation)
                .Include(e => e.IdanalyseNavigation)
                .Include(e => e.Technicien)
                .Include(e => e.Detailresultats)
                    .ThenInclude(d => d.Parametre)
                .Include(e => e.Detailresultats)
                    .ThenInclude(d => d.Parametre.CodeuniteNavigation)
                .Include(e => e.Detailresultats)
                    .ThenInclude(d => d.Parametre.CodeunitesiNavigation)
                .FirstOrDefaultAsync(m => m.Enteteresultatid == id);
            if (enteteresultat == null)
            {
                return NotFound();
            }

            // utilisateur connecté
            var user = await _usermanager.GetUserAsync(User);

            Utilisateur utilisateur = null;

            if (user != null)
            {
                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Include(x => x.CodesiteNavigation)
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();
            }

            // Récupérer la signature du biologiste si elle existe déjà (depuis Signatureutilisateur)
            var biologisteSignature = utilisateur != null ?
                await _context.Signatureutilisateurs
                    .Where(s => s.Utilisateurid == utilisateur.Utilisateurid)
                    .FirstOrDefaultAsync() : null;

            string? signatureBase64 = null;
            bool hasBiologisteSignature = false;

            if (biologisteSignature != null && biologisteSignature.Image != null && biologisteSignature.Image.Length > 0)
            {
                signatureBase64 = $"data:image/{biologisteSignature.Extension};base64,{Convert.ToBase64String(biologisteSignature.Image)}";
                hasBiologisteSignature = true;
            }

            // Récupérer les informations sur les prélèvements et échantillons pour cette analyse
            // Test: récupérer tous les prélèvements de la demande (sans filtrer par analyse)
            var prelevements = enteteresultat.Entetedemande?.Detaildemandes?
                .SelectMany(dd => dd.Prelevements)
                .ToList() ?? new List<Prelevement>();
                
            // Debug: loguer les informations pour diagnostiquer
            _logger.LogInformation($"Nombre de détails demande: {enteteresultat.Entetedemande?.Detaildemandes?.Count ?? 0}");
            _logger.LogInformation($"Nombre de prélèvements trouvés: {prelevements.Count}");
            _logger.LogInformation($"ID Analyse recherchée: {enteteresultat.Idanalyse}");
            
            // Récupérer les natures d'échantillons
            var naturesEchantillons = prelevements
                .Select(p => p.IdnatureechantillonNavigation?.Nom)
                .Where(n => !string.IsNullOrEmpty(n))
                .Distinct()
                .ToList();
            
            // Récupérer la première heure d'arrivée (date de réception)
            var heureArrivee = prelevements
                .Where(p => p.Datereception.HasValue)
                .OrderBy(p => p.Datereception)
                .FirstOrDefault()?.Datereception;
                
            // Récupérer la première date de prélèvement
            var datePrelevement = prelevements
                .OrderBy(p => p.Dateprelevement)
                .FirstOrDefault()?.Dateprelevement;
                
            // Récupérer le lieu de prélèvement
            var lieuPrelevement = prelevements
                .Select(p => p.Lieuprelevement)
                .Where(l => !string.IsNullOrEmpty(l))
                .FirstOrDefault();

            var vm = new ProlabWeb.ViewModels.ValidationresultatVM
            {
                Enteteresultatid = enteteresultat.Enteteresultatid,
                Codesite = enteteresultat.Codesite,
                SiteNom = enteteresultat.CodesiteNavigation?.Name,
                Technicienid = enteteresultat.Technicienid,
                TechnicienNom = enteteresultat.Technicien?.Nom,
                TechnicienPrenom = enteteresultat.Technicien?.Prenom,
                Biologisteid = utilisateur != null ? utilisateur.Utilisateurid : Guid.Empty,
                Entetedemandeid = enteteresultat.Entetedemandeid,
                DemandeNumero = enteteresultat.Entetedemande?.Numero,
                DemandeDate = enteteresultat.Entetedemande?.Date,
                BiologisteNomComplet = utilisateur != null ? $"{utilisateur.Nom} {utilisateur.Prenom}" : "",
                PatientNom = enteteresultat.Entetedemande?.Patient?.Nom,
                PatientPrenom = enteteresultat.Entetedemande?.Patient?.Prenom,
                PatientCode = enteteresultat.Entetedemande?.Patient?.Code,
                PrescripteurNom = enteteresultat.Entetedemande?.Prescripteur?.Nom,
                AssuranceNom = enteteresultat.Entetedemande?.Policeassurance?.CodeassuranceNavigation?.Nom,
                AssuranceTaux = enteteresultat.Entetedemande?.Policeassurance?.Taux,
                Date = enteteresultat.Date,
                Interpretation = StripHtml(enteteresultat.Interpretation ?? ""),
                Idanalyse = enteteresultat.Idanalyse,
                AnalyseNom = enteteresultat.IdanalyseNavigation?.Nom,
                Statut = enteteresultat.Statut,
                Parametres = enteteresultat.Detailresultats.Select(d => new ProlabWeb.ViewModels.ParametreResultatVM
                {
                    Parametreid = d.Parametreid,
                    Nom = d.Parametre?.Nom ?? "",
                    Code = d.Parametre?.Code ?? "",
                    Unite = d.Parametre?.CodeuniteNavigation?.Name ?? "",
                    Resultat = d.Resultat,
                    UniteSI = d.Parametre?.CodeunitesiNavigation?.Name ?? "",
                    Resultatsi = d.Resultatsi,
                    Commentaire = d.Commentaire
                }).ToList(),
                // Colonnes supprimées de la BD - plus de compatibilité nécessaire
                // Nouvelle méthode automatique
                BiologisteSignatureBase64 = signatureBase64,
                HasBiologisteSignature = hasBiologisteSignature,
                
                // Informations sur les échantillons et prélèvements
                HeureArrivee = heureArrivee,
                NatureEchantillons = naturesEchantillons.Any() ? string.Join(", ", naturesEchantillons) : null,
                DatePrelevement = datePrelevement,
                LieuPrelevement = lieuPrelevement
            };

            // Récupérer les biologistes
            //var biologisteProfilNom = ProlabWeb.EnumProfil.biologiste.ToString();
            //var utilisateursBiologistes = _context.Utilisateurprofils
            //    //.Where(up => up.Profil.Nom == biologisteProfilNom)
            //    .Select(up => up.Utilisateur)
            //    .Distinct()
            //    .ToList();
            var users = (await _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString()))
                .Select(x => x.Id)
                .ToList();

            var utilisateursBiologistes = users.Any()
                ? _context.Utilisateurs.AsNoTracking()
                    .Where(x => users.Contains(x.Userid))
                    .ToList()
                : new List<Utilisateur>();
            ViewData["Biologisteid"] = new SelectList(utilisateursBiologistes, "Utilisateurid", "Nom");

            return View(vm);
        }

        // POST: Validationresultat/Details/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(Guid id, ProlabWeb.ViewModels.ValidationresultatVM vm)
        {
            if (id != vm.Enteteresultatid)
                return NotFound();

            // utilisateur connecté pour définir le biologiste
            var user = await _usermanager.GetUserAsync(User);
            Utilisateur utilisateur = null;

            if (user != null)
            {
                utilisateur = await _context.Utilisateurs.AsNoTracking()
                    .Include(x => x.CodesiteNavigation)
                    .Where(x => x.Userid == user.Id)
                    .FirstOrDefaultAsync();
                    
                // Définir le biologiste automatiquement si pas encore défini
                if (utilisateur != null && vm.Biologisteid == Guid.Empty)
                {
                    vm.Biologisteid = utilisateur.Utilisateurid;
                }
            }

            try
            {
                // Remove validation errors for Builder properties since they're not used in validation
                var builderKeys = ModelState.Keys.Where(key => key.Contains("Builder")).ToList();
                foreach (var key in builderKeys)
                {
                    ModelState.Remove(key);
                }
                
                // Debug : afficher les erreurs de validation
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"ModelState invalide pour l'entité {vm.Enteteresultatid}");
                    
                    var errorMessages = new List<string>();
                    foreach (var modelError in ModelState)
                    {
                        var key = modelError.Key;
                        var errors = modelError.Value.Errors;
                        foreach (var error in errors)
                        {
                            var errorMessage = $"Erreur de validation pour {key}: {error.ErrorMessage}";
                            _logger.LogWarning(errorMessage);
                            errorMessages.Add(errorMessage);
                        }
                    }
                    
                    // Ajouter un message d'erreur détaillé
                    TempData["Error"] = "Erreur de validation du formulaire: " + string.Join("; ", errorMessages);

                    // Récupérer les biologistes
                    var users = (await _usermanager
                        .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString()))
                        .Select(x => x.Id)
                        .ToList();

                    var utilisateursBiologistes = users.Any()
                        ? _context.Utilisateurs.AsNoTracking()
                            .Where(x => users.Contains(x.Userid))
                            .ToList()
                        : new List<Utilisateur>();
                    ViewData["Biologisteid"] = new SelectList(utilisateursBiologistes, "Utilisateurid", "Nom", vm.Biologisteid);

                    return View(vm);
                }

                // utilisateur déjà récupéré plus haut

                var entete = await _context.Enteteresultats
                    .Include(e => e.Detailresultats)
                    .FirstOrDefaultAsync(e => e.Enteteresultatid == vm.Enteteresultatid);
                if (entete == null) return NotFound();

                // Resolve biologiste id safely to prevent FK violations on enteteresultat.biologisteid.
                var biologisteIdToSave = vm.Biologisteid != Guid.Empty
                    ? vm.Biologisteid
                    : (utilisateur?.Utilisateurid ?? Guid.Empty);

                if (biologisteIdToSave == Guid.Empty)
                {
                    TempData["Error"] = "Aucun biologiste valide n'a ete selectionne.";

                    var users = (await _usermanager
                        .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString()))
                        .Select(x => x.Id)
                        .ToList();

                    var utilisateursBiologistes = users.Any()
                        ? _context.Utilisateurs.AsNoTracking()
                            .Where(x => users.Contains(x.Userid))
                            .ToList()
                        : new List<Utilisateur>();
                    ViewData["Biologisteid"] = new SelectList(utilisateursBiologistes, "Utilisateurid", "Nom", vm.Biologisteid);

                    return View(vm);
                }

                var biologisteExists = await _context.Utilisateurs
                    .AsNoTracking()
                    .AnyAsync(u => u.Utilisateurid == biologisteIdToSave);

                if (!biologisteExists)
                {
                    TempData["Error"] = "Le biologiste selectionne est introuvable.";

                    var users = (await _usermanager
                        .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString()))
                        .Select(x => x.Id)
                        .ToList();

                    var utilisateursBiologistes = users.Any()
                        ? _context.Utilisateurs.AsNoTracking()
                            .Where(x => users.Contains(x.Userid))
                            .ToList()
                        : new List<Utilisateur>();
                    ViewData["Biologisteid"] = new SelectList(utilisateursBiologistes, "Utilisateurid", "Nom", vm.Biologisteid);

                    return View(vm);
                }

                vm.Biologisteid = biologisteIdToSave;

                // Mettre à jour la validation biologiste et le biologisteid sur l'entête (et non plus sur chaque détail)
                entete.Biologisteid = biologisteIdToSave;
                entete.Validationbiologiste = true;
                entete.Interpretation = vm.Interpretation;
                
                // Récupérer la signature de l'utilisateur depuis Signatureutilisateur
                var signatureUtilisateur = await _context.Signatureutilisateurs
                    .Where(s => s.Utilisateurid == biologisteIdToSave)
                    .FirstOrDefaultAsync();
                
                // Gérer la signature depuis le canvas si pas de signature en base
                if (signatureUtilisateur == null && !string.IsNullOrEmpty(vm.CanvasSignatureData))
                {
                    // Format: data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...
                    var base64Parts = vm.CanvasSignatureData.Split(',');
                    if (base64Parts.Length == 2)
                    {
                        var headerPart = base64Parts[0]; // "data:image/png;base64"
                        var dataPart = base64Parts[1];   // "iVBORw0KGgoAAAANSUhEUgAA..."
                        
                        // Extraire l'extension depuis l'en-tête
                        string extension = "png"; // par défaut
                        if (headerPart.Contains("image/png")) extension = "png";
                        else if (headerPart.Contains("image/jpeg") || headerPart.Contains("image/jpg")) extension = "jpg";
                        
                        try
                        {
                            byte[] signatureBytes = Convert.FromBase64String(dataPart);
                            
                            // Créer une nouvelle signature utilisateur
                            signatureUtilisateur = new Signatureutilisateur
                            {
                                Signatureutilisateurid = Guid.NewGuid(),
                                Utilisateurid = biologisteIdToSave,
                                Image = signatureBytes,
                                Extension = extension
                            };
                            _context.Signatureutilisateurs.Add(signatureUtilisateur);
                            await _context.SaveChangesAsync(); // Sauvegarder immédiatement pour avoir l'ID
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erreur lors de la conversion de la signature canvas");
                        }
                    }
                }
                
                // Les colonnes Biologistesignature et Biologistesignatureextension ont été supprimées de la BD
                // La signature est maintenant uniquement stockée dans Signatureutilisateur
                
                // Le biologisteid est déjà défini plus haut, pas besoin de sauvegarder le nom
                // Le nom sera récupéré via la relation Biologiste
                
                // Mettre à jour le statut de l'entête
                entete.Statut = ProlabWeb.EnumStatut.valide.ToString();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Une erreur est survenue !");
                throw;
            }
            

            TempData["Success"] = "Résultat validé avec succès.";
            
            // Instead of redirecting immediately, reload the same view with success message
            // The JavaScript will handle showing the success dialog and redirecting
            return RedirectToAction("Details", new { id = vm.Enteteresultatid });
        }

        [HttpGet]
        public async Task<IActionResult> ValiderEnLot([FromQuery] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Aucun identifiant fourni.");

            // Récupérer l'id de l'utilisateur connecté (colonne Id de AspNetUsers)
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            // Chercher l'utilisateur dans la table Utilisateur avec ce Userid
            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Userid == userIdStr);
            if (utilisateur == null)
                return Unauthorized();

            // Vérifier que l'utilisateur a le profil biologiste
            //var profilBiologiste = await _context.Utilisateurprofils
            //    .Include(up => up.Profil)
            //    .FirstOrDefaultAsync(up => up.Utilisateurid == utilisateur.Utilisateurid && up.Profil.Nom == ProlabWeb.EnumProfil.biologiste.ToString());
            var users = (await _usermanager
                .GetUsersInRoleAsync(EnumRoles.Biologiste.ToString()))
                .Where(x => x.Id == utilisateur.Userid)
                .ToList();

            var profilBiologiste = users.Any()
                ? users.ElementAt(0)
                : null;

            if (profilBiologiste == null)
                return Forbid("Seuls les biologistes peuvent valider en lot.");

            var entetes = await _context.Enteteresultats
                .Include(e => e.Detailresultats)
                .Where(e => ids.Contains(e.Enteteresultatid))
                .ToListAsync();

            // Récupérer la signature de l'utilisateur pour l'appliquer à toutes les validations
            var signatureUtilisateur = await _context.Signatureutilisateurs
                .Where(s => s.Utilisateurid == utilisateur.Utilisateurid)
                .FirstOrDefaultAsync();

            foreach (var entete in entetes)
            {
                entete.Statut = ProlabWeb.EnumStatut.valide.ToString();
                entete.Biologisteid = utilisateur.Utilisateurid;
                entete.Validationbiologiste = true;
                
                // Les colonnes Biologistesignature et Biologistesignatureextension ont été supprimées de la BD
                // La signature est maintenant uniquement stockée dans Signatureutilisateur
            }
            await _context.SaveChangesAsync();
            return Json(new ProlabWeb.JsonResponseViewModel
            {
                success = true,
                data = null,
                message = $"{entetes.Count} résultats validés."
            });
        }

        /// <summary>
        /// Action pour afficher la signature d'un biologiste depuis un résultat validé
        /// </summary>
        /// <param name="id">ID de l'entête résultat</param>
        /// <returns>Image de signature ou NotFound</returns>
        [HttpGet]
        public async Task<IActionResult> AfficherSignature(Guid id)
        {
            // Récupérer l'entête pour obtenir le biologiste ID
            var entete = await _context.Enteteresultats
                .FirstOrDefaultAsync(e => e.Enteteresultatid == id);
                
            if (entete?.Biologisteid == null)
            {
                return NotFound("Résultat non trouvé ou biologiste non défini.");
            }

            // Récupérer la signature depuis Signatureutilisateur
            var signature = await _context.Signatureutilisateurs
                .FirstOrDefaultAsync(s => s.Utilisateurid == entete.Biologisteid);
                
            if (signature?.Image == null || signature.Image.Length == 0)
            {
                return NotFound("Aucune signature trouvée pour ce biologiste.");
            }

            var extension = signature.Extension ?? "png";
            var contentType = extension switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "bmp" => "image/bmp",
                "webp" => "image/webp",
                _ => "image/png"
            };

            return File(signature.Image, contentType);
        }
        
        /// <summary>
        /// Méthode helper pour nettoyer le HTML d'une chaîne
        /// </summary>
        /// <param name="html">Chaîne contenant du HTML</param>
        /// <returns>Chaîne sans balises HTML</returns>
        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
                
            // Supprimer les balises HTML avec une regex
            return Regex.Replace(html, @"<[^>]*>", string.Empty).Trim();
        }
    }
}
