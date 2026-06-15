// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProlabWeb.Data;

namespace ProlabWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ProlabIdentityUser> _userManager;
        private readonly SignInManager<ProlabIdentityUser> _signInManager;

        public IndexModel(
            UserManager<ProlabIdentityUser> userManager,
            SignInManager<ProlabIdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Nom d'utilisateur")]
        public string NomUtilisateur { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        [Display(Name = "Numéro de téléphone")]
        [Phone(ErrorMessage = "Format de numéro invalide.")]
        public string NumeroTelephone { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        private async Task LoadAsync(ProlabIdentityUser user)
        {
            NomUtilisateur = user.Email;
            NumeroTelephone = user.PhoneNumber;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Utilisateur introuvable.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Utilisateur introuvable.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (NumeroTelephone != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, NumeroTelephone);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Erreur lors de la modification du numéro de téléphone.";
                    return Page();
                }
                StatusMessage = "Numéro de téléphone modifié avec succès.";
                await _signInManager.RefreshSignInAsync(user);
            }
            else
            {
                StatusMessage = "Aucune modification détectée.";
            }
            return RedirectToPage();
        }
    }
}
