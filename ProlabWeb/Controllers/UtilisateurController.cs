using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProlabWeb.Data;
using ProlabWeb.Db;
using ProlabWeb.Helpers;
using ProlabWeb.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProlabWeb.Controllers
{
    [AllowAnonymous]
    public class UtilisateurController : Controller
    {
        private readonly ProlabwebContext _context;
        private readonly UserManager<ProlabIdentityUser> _usermanager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UtilisateurController(ProlabwebContext context, UserManager<ProlabIdentityUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
        {
            _context = context;
            _usermanager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        // GET: Utilisateurs
        public async Task<IActionResult> Index()
        {
            var utilisateurs = await _context.Utilisateurs.ToListAsync();

            var indexVMs = utilisateurs.Select(u => new UtilisateurIndexVM
            {
                Utilisateurid = u.Utilisateurid,
                Nom = u.Nom,
                Prenom = u.Prenom,
                Codesexe = u.Codesexe == "M" ? "Masculin" : u.Codesexe == "F" ? "Féminin" : u.Codesexe,
                Datenaissance = u.Datenaissance,
                Nationnalite = _context.Countries.FirstOrDefault(c => c.Countryisocode2 == u.Nationnalite)?.Name ?? "",
                Codesite = _context.Sites.FirstOrDefault(s => s.Codesite == u.Codesite)?.Name ?? ""
            }).ToList();

            return View(indexVMs);
        }

        // GET: Utilisateurs/Details/5
        public async Task<IActionResult> Details(Guid? id, bool showTempPassword = false)
        {
            if (id == null)
                return NotFound();

            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Utilisateurid == id);
            if (utilisateur == null)
                return NotFound();

            // Get temporary password from TempData if available
            var tempPassword = TempData["TemporaryPassword"]?.ToString();
            
            // For newly created users with Mustchangepass = true, show their generated password
            string passwordToShow = null;
            if (!string.IsNullOrEmpty(tempPassword))
            {
                passwordToShow = tempPassword; // From reset password action
            }
            else if (utilisateur.Mustchangepass && !string.IsNullOrEmpty(utilisateur.Password))
            {
                // Check if it's a generated password (not a hash)
                if (!utilisateur.Password.StartsWith("$") && utilisateur.Password.Length < 50)
                {
                    passwordToShow = utilisateur.Password; // Show generated password
                }
            }

            var vm = new UtilisateurDetailsVM
            {
                Utilisateurid = utilisateur.Utilisateurid,
                Nom = utilisateur.Nom,
                Prenom = utilisateur.Prenom,
                Codesexe = utilisateur.Codesexe,
                Datenaissance = utilisateur.Datenaissance,
                Nationnalite = utilisateur.Nationnalite,
                Codesite = utilisateur.Codesite,
                Matricule = utilisateur.Matricule,
                Tel1 = utilisateur.Tel1,
                Tel2 = utilisateur.Tel2,
                Mob1 = utilisateur.Mob1,
                Mob2 = utilisateur.Mob2,
                Email = utilisateur.Email,
                Login = utilisateur.Login,
                PasswordToShow = passwordToShow,
                Isactive = utilisateur.Isactive,
                Mustchangepass = utilisateur.Mustchangepass
            };

            return View(vm);
        }

        // POST: Utilisateurs/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            // Récupérer l'utilisateur
            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Utilisateurid == id);
            if (utilisateur == null) return NotFound();

            if (string.IsNullOrEmpty(utilisateur.Userid))
                return BadRequest("L'utilisateur n'est pas lié à Identity.");

            var identityUser = await _usermanager.FindByIdAsync(utilisateur.Userid);
            if (identityUser == null)
                return NotFound("Utilisateur Identity introuvable.");

            // Générer un mot de passe temporaire sécurisé
            var tempPassword = Utilities.CreatePassword(10); // longueur 10 caractères

            // Générer le token de reset unique
            var token = await _usermanager.GeneratePasswordResetTokenAsync(identityUser);

            // Appliquer le nouveau mot de passe
            var result = await _usermanager.ResetPasswordAsync(identityUser, token, tempPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest($"Erreur lors du reset du mot de passe : {errors}");
            }

            // Mettre à jour le flag pour forcer le changement de mot de passe à la prochaine connexion
            utilisateur.Mustchangepass = true;
            await _context.SaveChangesAsync();

            // IMPORTANT: Also update the Identity user's MustChangePassword flag
            identityUser.MustChangePassword = true;
            await _usermanager.UpdateAsync(identityUser);

            // Optionnel : afficher le mot de passe temporaire à l'administrateur
            TempData["TemporaryPassword"] = tempPassword;

            // Rediriger vers la page détails de l'utilisateur
            return RedirectToAction(nameof(Details), new { id = utilisateur.Utilisateurid });
        }




        // GET: Utilisateurs/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.country = new SelectList(await _context.Countries.ToListAsync(), "Countryisocode2", "Name");
            ViewBag.sites = new SelectList(await _context.Sites.ToListAsync(), "Codesite", "Name");
            return View();
        }

        private async Task<(IdentityResult, string)> CreateUserAsync(string employeeId, string username, string email, string password)
        {
            var newuser = await _usermanager.FindByNameAsync(username);
            if (newuser == null)
            {
                newuser = new ProlabIdentityUser
                {
                    UserName = username,
                    Email = email,
                    TemporaryPassword = password,
                    MustChangePassword = true,
                    EmployeeId = employeeId
                };
                var result = await _usermanager.CreateAsync(newuser, password);
                return (result, newuser.Id);
            }
            else
            {
                var failed = new IdentityError() { Code = "Exist", Description = "Ce nom d'utilisateur est déjà utilisé." };
                return (IdentityResult.Failed(failed), string.Empty);
            }
        }

        // POST: Utilisateurs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UtilisateurCreateVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var utilisateur = _mapper.Map<Utilisateur>(model);
            utilisateur.Utilisateurid = Guid.NewGuid();
            utilisateur.Login = utilisateur.Email;
            utilisateur.Creationdate = utilisateur.Updatedate = DateOnly.FromDateTime(DateTime.UtcNow);
            utilisateur.Createdby = utilisateur.Updateby = User.Identity!.Name!;

            var password = Utilities.CreatePassword(8);
            utilisateur.Password = password;

            var result = await CreateUserAsync(utilisateur.Utilisateurid.ToString(), utilisateur.Login!, utilisateur.Email!, password);
            if (result.Item1.Succeeded)
            {
                utilisateur.Userid = result.Item2;
                
                // Set flags to force password change on first login
                var identityUser = await _usermanager.FindByIdAsync(utilisateur.Userid);
                if (identityUser != null)
                {
                    identityUser.MustChangePassword = true;
                    await _usermanager.UpdateAsync(identityUser);
                }
                
                // Set custom table flag as well
                utilisateur.Mustchangepass = true;
                // Keep the temporary password for display, don't overwrite with hash
                utilisateur.Password = password;

                TempData["UtilisateurCree"] = $"L'utilisateur \"{utilisateur.Nom} {utilisateur.Prenom}\" a été créé ! Username: {utilisateur.Email}, Password: {password}";
            }

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // GET: Utilisateurs/Edit/5
        // GET: Utilisateurs/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null) return NotFound();

            var identityUser = await _usermanager.FindByNameAsync(utilisateur.Email);

            var allRoles = _roleManager.Roles.ToList();
            var assignedRoles = identityUser != null
                ? (await _usermanager.GetRolesAsync(identityUser)).ToList()
                : new List<string>();

            // Récupérer la signature existante si elle existe
            var signature = await _context.Signatureutilisateurs
                .Where(s => s.Utilisateurid == utilisateur.Utilisateurid)
                .FirstOrDefaultAsync();
            
            string? signatureBase64 = null;
            if (signature != null)
            {
                signatureBase64 = $"data:image/{signature.Extension};base64,{Convert.ToBase64String(signature.Image)}";
            }

            var vm = new UtilisateurEditVM
            {
                Utilisateurid = utilisateur.Utilisateurid,
                Matricule = utilisateur.Matricule,
                Nom = utilisateur.Nom,
                Prenom = utilisateur.Prenom,
                Codesexe = utilisateur.Codesexe,
                Datenaissance = utilisateur.Datenaissance,
                Nationnalite = utilisateur.Nationnalite,
                Tel1 = utilisateur.Tel1,
                Tel2 = utilisateur.Tel2,
                Mob1 = utilisateur.Mob1,
                Mob2 = utilisateur.Mob2,
                Email = utilisateur.Email,
                Codesite = utilisateur.Codesite,
                Idinh = utilisateur.Idinh,
                Pays = new SelectList(await _context.Countries.ToListAsync(), "Countryisocode2", "Name"),
                Sites = new SelectList(await _context.Sites.ToListAsync(), "Codesite", "Name"),
                UserId = identityUser?.Id,
                AvailableRoles = allRoles.Select(r => new UtilisateurRoleVM
                {
                    Id = r.Id,
                    Name = r.Name,
                    NormalizedName = r.NormalizedName,
                    IsAssigned = assignedRoles.Contains(r.Name),
                    IsSelected = assignedRoles.Contains(r.Name)
                }).ToList(),
                CurrentSignatureBase64 = signatureBase64
            };

            return View(vm);
        }
        // POST: Utilisateurs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UtilisateurEditVM vm)
        {
            if (id != vm.Utilisateurid) return NotFound();

            // DEBUG: Log what we received
            Console.WriteLine($"DEBUG: SelectedRoleIds count: {vm.SelectedRoleIds?.Count ?? 0}");
            if (vm.SelectedRoleIds != null)
            {
                foreach (var roleId in vm.SelectedRoleIds)
                {
                    Console.WriteLine($"DEBUG: Received RoleId: {roleId}");
                }
            }

            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null) return NotFound();

            var identityUser = !string.IsNullOrEmpty(utilisateur.Userid)
                ? await _usermanager.FindByIdAsync(utilisateur.Userid)
                : null;

            if (!ModelState.IsValid)
            {
                await ReloadVmData(vm, utilisateur);
                return View(vm);
            }

            try
            {
                // mise à jour des champs utilisateur
                utilisateur.Matricule = vm.Matricule;
                utilisateur.Nom = vm.Nom;
                utilisateur.Prenom = vm.Prenom;
                utilisateur.Codesexe = vm.Codesexe;
                utilisateur.Datenaissance = vm.Datenaissance;
                utilisateur.Nationnalite = vm.Nationnalite;
                utilisateur.Tel1 = vm.Tel1;
                utilisateur.Tel2 = vm.Tel2;
                utilisateur.Mob1 = vm.Mob1;
                utilisateur.Mob2 = vm.Mob2;
                utilisateur.Email = vm.Email;
                utilisateur.Codesite = vm.Codesite;
                utilisateur.Idinh = vm.Idinh;

                _context.Update(utilisateur);
                await _context.SaveChangesAsync();

                // mise à jour des rôles si identityUser existe
                if (identityUser != null)
                {
                    var currentRoles = await _usermanager.GetRolesAsync(identityUser);
                    
                    // Get selected role names from SelectedRoleIds
                    var selectedRoleNames = new List<string>();
                    if (vm.SelectedRoleIds != null && vm.SelectedRoleIds.Any())
                    {
                        var allRoles = _roleManager.Roles.ToList();
                        selectedRoleNames = allRoles
                            .Where(r => vm.SelectedRoleIds.Contains(r.Id))
                            .Select(r => r.Name)
                            .ToList();
                    }
                    
                    // retirer les rôles non sélectionnés
                    var toRemove = currentRoles.Except(selectedRoleNames);
                    if (toRemove.Any())
                        await _usermanager.RemoveFromRolesAsync(identityUser, toRemove);

                    // ajouter les nouveaux rôles
                    var toAdd = selectedRoleNames.Except(currentRoles);
                    if (toAdd.Any())
                        await _usermanager.AddToRolesAsync(identityUser, toAdd);
                }

                // Gestion de la signature si l'utilisateur a le rôle Biologiste
                var hasBiologisteRole = identityUser != null && await _usermanager.IsInRoleAsync(identityUser, EnumRoles.Biologiste.ToString());
                if (hasBiologisteRole)
                {
                    await HandleUserSignature(vm, utilisateur.Utilisateurid);
                }
                else
                {
                    // Si l'utilisateur n'est plus biologiste, supprimer sa signature existante
                    var existingSignature = await _context.Signatureutilisateurs
                        .Where(s => s.Utilisateurid == utilisateur.Utilisateurid)
                        .FirstOrDefaultAsync();
                    if (existingSignature != null)
                    {
                        _context.Signatureutilisateurs.Remove(existingSignature);
                        await _context.SaveChangesAsync();
                    }
                }

                TempData["UtilisateurModifie"] = "L'utilisateur a été modifié avec succès.";

                // rester sur la page Edit avec les données mises à jour
                await ReloadVmData(vm, utilisateur);
                return View(vm);
            }
            catch
            {
                TempData["UtilisateurModifie"] = "Erreur lors de la modification.";
                await ReloadVmData(vm, utilisateur);
                return View(vm);
            }
        }

        // Helper pour recharger les listes et rôles dans la VM
        private async Task ReloadVmData(UtilisateurEditVM vm, Utilisateur utilisateur)
        {
            var allRoles = _roleManager.Roles.ToList();
            var identityUser = !string.IsNullOrEmpty(utilisateur.Userid)
                ? await _usermanager.FindByIdAsync(utilisateur.Userid)
                : null;

            var currentRoles = identityUser != null
                ? await _usermanager.GetRolesAsync(identityUser)
                : new List<string>();

            vm.AvailableRoles = allRoles.Select(r => new UtilisateurRoleVM
            {
                Id = r.Id,
                Name = r.Name,
                NormalizedName = r.NormalizedName,
                IsAssigned = currentRoles.Contains(r.Name),
                // on garde IsSelected en fonction des rôles actuels
                IsSelected = identityUser != null && currentRoles.Contains(r.Name)
            }).ToList();

            vm.Pays = new SelectList(await _context.Countries.ToListAsync(), "Countryisocode2", "Name", vm.Nationnalite);
            vm.Sites = new SelectList(await _context.Sites.ToListAsync(), "Codesite", "Name", utilisateur.Codesite);
            
            // Recharger la signature actuelle
            var signature = await _context.Signatureutilisateurs
                .Where(s => s.Utilisateurid == utilisateur.Utilisateurid)
                .FirstOrDefaultAsync();
            
            if (signature != null)
            {
                vm.CurrentSignatureBase64 = $"data:image/{signature.Extension};base64,{Convert.ToBase64String(signature.Image)}";
            }
        }





        // GET: Utilisateurs/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(m => m.Utilisateurid == id);
            if (utilisateur == null) return NotFound();

            return View(utilisateur);
        }

        // POST: Utilisateurs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur != null) _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Suppression AJAX
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(Guid id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null) return Json(new { success = false, message = "Utilisateur non trouvé." });

            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool UtilisateurExists(Guid id) => _context.Utilisateurs.Any(e => e.Utilisateurid == id);

        /// <summary>
        /// Gère la sauvegarde de la signature utilisateur (upload ou canvas)
        /// </summary>
        private async Task HandleUserSignature(UtilisateurEditVM vm, Guid utilisateurId)
        {
            byte[]? signatureBytes = null;
            string? extension = null;

            // Priorité 1: Signature canvas (si fournie)
            if (!string.IsNullOrEmpty(vm.CanvasSignatureData))
            {
                // Format: data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...
                var base64Parts = vm.CanvasSignatureData.Split(',');
                if (base64Parts.Length == 2)
                {
                    var headerPart = base64Parts[0]; // "data:image/png;base64"
                    var dataPart = base64Parts[1];   // "iVBORw0KGgoAAAANSUhEUgAA..."
                    
                    // Extraire l'extension depuis l'en-tête
                    if (headerPart.Contains("image/png")) extension = "png";
                    else if (headerPart.Contains("image/jpeg") || headerPart.Contains("image/jpg")) extension = "jpg";
                    else extension = "png"; // par défaut
                    
                    try
                    {
                        signatureBytes = Convert.FromBase64String(dataPart);
                    }
                    catch
                    {
                        // Ignore si la conversion échoue
                    }
                }
            }
            // Priorité 2: Upload de fichier
            else if (vm.SignatureFile != null && vm.SignatureFile.Length > 0)
            {
                extension = Path.GetExtension(vm.SignatureFile.FileName).TrimStart('.').ToLower();
                using (var memoryStream = new MemoryStream())
                {
                    await vm.SignatureFile.CopyToAsync(memoryStream);
                    signatureBytes = memoryStream.ToArray();
                }
            }

            // Sauvegarder la signature si on en a une
            if (signatureBytes != null && !string.IsNullOrEmpty(extension))
            {
                // Rechercher signature existante
                var existingSignature = await _context.Signatureutilisateurs
                    .Where(s => s.Utilisateurid == utilisateurId)
                    .FirstOrDefaultAsync();

                if (existingSignature != null)
                {
                    // Mettre à jour
                    existingSignature.Image = signatureBytes;
                    existingSignature.Extension = extension;
                    _context.Signatureutilisateurs.Update(existingSignature);
                }
                else
                {
                    // Créer nouvelle signature
                    var newSignature = new Signatureutilisateur
                    {
                        Signatureutilisateurid = Guid.NewGuid(),
                        Utilisateurid = utilisateurId,
                        Image = signatureBytes,
                        Extension = extension
                    };
                    _context.Signatureutilisateurs.Add(newSignature);
                }
                
                await _context.SaveChangesAsync();
            }
        }
    }
}
