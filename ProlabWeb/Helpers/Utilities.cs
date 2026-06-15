using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using ProlabWeb.Db;
using System.IO;
using System.Security.Cryptography;

namespace ProlabWeb.Helpers
{
    public class Utilities
    {
        public static (decimal, decimal, decimal, decimal, decimal) CalculerDifferentPrixDetailDemande(Policeassurance policeassurance, decimal prixdebase, decimal tarif)
        {
            // prixdebase = prix public de l'acte (ex: 23 000)
            // tarif = base de remboursement assurance (ex: 20 000)
            // base = tarif si disponible, sinon prix public
            var baseFacturable = policeassurance != null
                ? (tarif > 0 ? tarif : prixdebase)
                : prixdebase;

            var partassurance = policeassurance != null
                ? baseFacturable * policeassurance.Taux / 100
                : 0;
            var partpatient = policeassurance != null
                ? baseFacturable * (100 - policeassurance.Taux) / 100
                : baseFacturable;

            // Le complément est l'écart entre prix public et base facturable assurance.
            // On borne à 0 pour éviter les montants négatifs si un tarif dépasse le prix public.
            var complement = policeassurance != null
                ? Math.Max(prixdebase - baseFacturable, 0)
                : 0;

            var net = partpatient + complement;

            return (baseFacturable, partassurance, partpatient, complement, net);
        }

        public static string ConvertAmountToWords(double amount, string devise = "francs cfa", string centimes = "")
        {
            if (amount == 0) return "zéro";

            string words = "";
            int intPart = (int)amount;
            int decimalPart = (int)((amount - intPart) * 100);

            words += ConvertToWords(intPart) + " " + devise;

            //if (decimalPart > 0)
            //{
            //    words += " et " + ConvertToWords(decimalPart) + " " + centimes;
            //}

            return words;
        }

        private static string ConvertToWords(int number)
        {
            string[] Units = { "", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf" };
            string[] Tens = { "", "dix", "vingt", "trente", "quarante", "cinquante", "soixante", "soixante-dix", "quatre-vingt", "quatre-vingt-dix" };
            string words = "";

            if (number == 0)
            {
                words += "";
            }
            if (number < 10)
            {
                words += Units[number];
            }
            if (number < 20)
            {
                words += "dix-" + Units[number - 10];
            }
            if (number < 100)
            {
                int tens = number / 10;
                int units = number % 10;

                string tensWord = Tens[tens];
                if (tens == 7 || tens == 9) // Special cases for 70-79 and 90-99
                {
                    tensWord = Tens[tens - 1] + "-dix";
                    units += 10;
                }

                words += tensWord + (units > 0 ? "-" + Units[units] : "");
            }

            if (number < 1000)
            {
                int hundreds = number / 100;
                int remainder = number % 100;

                string hundredsWord = (hundreds > 1 ? Units[hundreds] + "-cent" : "cent");
                words += hundredsWord + (remainder > 0 ? " " + ConvertToWords(remainder) : "");
            }

            return words;
        }

        public static int CalculerAgeEnAnnee(DateTime birthDate)
        {
            DateTime today = DateTime.Today;

            int years = today.Year - birthDate.Year;
            int months = today.Month - birthDate.Month;
            int days = today.Day - birthDate.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(today.Year, today.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return years;
        }

        public static string CalculerAgeEnAnneeMoisJour(DateTime birthDate)
        {
            DateTime today = DateTime.Today;

            int years = today.Year - birthDate.Year;
            int months = today.Month - birthDate.Month;
            int days = today.Day - birthDate.Day;

            if (days < 0)
            {
                months--;
                days += DateTime.DaysInMonth(today.Year, today.Month - 1);
            }

            if (months < 0)
            {
                years--;
                months += 12;
            }

            return $"{years} an(s), {months} mois, {days} jour(s)";
        }

        public static string FormatNumeroRecu(string code, int ordre)
        {
            string annee = DateTime.UtcNow.ToString("yy");
            string identifiant = !string.IsNullOrWhiteSpace(code) ? code : "";
            string ordreformate = $"{ordre:D6}";

            var str = $"{annee}{identifiant}{ordreformate}";

            return str;
        }

public static string CreatePassword(int length)
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+[]{}|;:,.<>?";
        char[] res = new char[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] uintBuffer = new byte[sizeof(uint)];

            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                res[i] = valid[(int)(num % (uint)valid.Length)];
            }
        }

        return new string(res);
    }
    public static string ConvertByteArrayToBase64(byte[] bytes, string extension)
        {
            string result = "";

            if (bytes != null && bytes.Length > 0 && !string.IsNullOrWhiteSpace(extension))
            {
                string base64 = Convert.ToBase64String(bytes);
                result = $"data:image/{extension};base64,{base64}";
            }
            return result;
        }

        /// <summary>
        /// Calculate the status of a demande based on requested analyses and available results
        /// </summary>
        /// <param name="requestedAnalyseIds">All analyses requested (from categories and direct analyses)</param>
        /// <param name="completedAnalyseIds">Analyses that have completed results</param>
        /// <returns>Status of the demande</returns>
        public static EnumStatutDemande CalculerStatutDemande(List<Guid> requestedAnalyseIds, List<Guid> completedAnalyseIds)
        {
            if (requestedAnalyseIds == null || requestedAnalyseIds.Count == 0)
                return EnumStatutDemande.NonTraite;

            if (completedAnalyseIds == null || completedAnalyseIds.Count == 0)
                return EnumStatutDemande.NonTraite;

            // Remove duplicates from requested analyses
            var distinctRequested = requestedAnalyseIds.Distinct().ToList();
            var distinctCompleted = completedAnalyseIds.Distinct().ToList();

            var completedCount = distinctRequested.Count(id => distinctCompleted.Contains(id));

            if (completedCount == 0)
                return EnumStatutDemande.NonTraite;
            else if (completedCount == distinctRequested.Count)
                return EnumStatutDemande.Traite;
            else
                return EnumStatutDemande.EnCours;
        }
    }
}
