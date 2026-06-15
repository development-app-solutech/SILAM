namespace ProlabWeb.Helpers
{
    using System;
    using System.Globalization;

    public class NumberToWordsConverter
    {
        private static readonly string[] UnitsMap = { "zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf", "dix",
                                                  "onze", "douze", "treize", "quatorze", "quinze", "seize" };
        private static readonly string[] TensMap = { "", "", "vingt", "trente", "quarante", "cinquante", "soixante", "soixante", "quatre-vingt", "quatre-vingt" };

        public static string Convert(decimal number)
        {
            if (number == 0)
                return "zéro francs cfa";

            long integralPart = (long)Math.Floor(number);
            int decimalPart = (int)((number - integralPart) * 100);

            string words = NumberToWords(integralPart) + (integralPart > 1 ? " francs cfa" : " francs cfa");

            if (decimalPart > 0)
            {
                words += " et " + NumberToWords(decimalPart) + (decimalPart > 1 ? " centimes" : " centime");
            }

            return words;
        }

        private static string NumberToWords(long number)
        {
            if (number == 0)
                return "zéro";

            if (number < 0)
                return "moins " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million";
                if ((number / 1000000) > 1)
                    words += "s";
                number %= 1000000;
                if (number > 0) words += " ";
            }

            if ((number / 1000) > 0)
            {
                if ((number / 1000) == 1)
                    words += "mille";
                else
                    words += NumberToWords(number / 1000) + " mille";

                number %= 1000;
                if (number > 0) words += " ";
            }

            if ((number / 100) > 0)
            {
                if ((number / 100) == 1)
                    words += "cent";
                else
                    words += UnitsMap[number / 100] + " cent";

                if (number % 100 == 0 && (number / 100) > 1)
                    words += "s";

                number %= 100;
                if (number > 0) words += " ";
            }

            if (number > 0)
            {
                if (number < 17)
                    words += UnitsMap[number];
                else if (number < 20)
                    words += "dix-" + UnitsMap[number - 10];
                else if (number < 70)
                {
                    words += TensMap[number / 10];
                    if ((number % 10) == 1 && (number / 10) != 8)
                        words += "-et-un";
                    else if ((number % 10) > 0)
                        words += "-" + UnitsMap[number % 10];
                }
                else if (number < 80)
                {
                    words += "soixante";
                    long rest = number - 60;
                    if (rest == 1)
                        words += "-et-onze";
                    else if (rest <= 16)
                        words += "-" + UnitsMap[rest];
                    else
                        words += "-" + NumberToWords(rest);
                }
                else if (number < 100)
                {
                    words += "quatre-vingt";
                    long rest = number - 80;
                    if (rest > 0)
                    {
                        if (rest == 1)
                            words += "-un";
                        else
                            words += "-" + UnitsMap[rest];
                    }
                }
            }

            return words;
        }
    }

}
