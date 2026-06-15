namespace ProlabWeb.Mappers
{
    public static class MappingHelpers
    {
        public static int? ParseInt(string? value)
        {
            return int.TryParse(value, out var result) ? result : (int?)null;
        }

        public static decimal? ParseDecimal(string? value)
        {
            return decimal.TryParse(value, out var result) ? result : (decimal?)null;
        }
    }

}
