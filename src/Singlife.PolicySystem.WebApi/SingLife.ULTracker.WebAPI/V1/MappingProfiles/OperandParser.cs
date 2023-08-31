using System;
using System.Globalization;
using System.Linq;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public static class OperandParser
    {
        private static readonly string[] trueTexts = { "True", "Yes", "1" };
        private static readonly string[] falseTexts = { "False", "No", "0" };

        private static readonly string[] dateTimeFormats =
        {
            "yyyy'-'MM'-'dd",
            "yyyy'-'MM'-'dd' 'HH':'mm':'ss",
            "yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'FFFFFFF"
        };

        public static bool ParseBoolean(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            if (trueTexts.Contains(text, StringComparer.OrdinalIgnoreCase))
                return true;

            if (falseTexts.Contains(text, StringComparer.OrdinalIgnoreCase))
                return false;

            throw new FormatException("String was not recognized as a valid Boolean.");
        }

        public static DateTime ParseDateTime(string text)
        {
            foreach (var format in dateTimeFormats)
            {
                if (TryParseExact(text, format, out var result))
                    return result;
            }

            throw new FormatException("String was not recognized as a valid DateTime.");

            bool TryParseExact(string value, string format, out DateTime result)
            {
                return DateTime.TryParseExact(
                    value,
                    format,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                    out result);
            }
        }

        public static decimal ParseDecimal(string text) =>
            decimal.Parse(
                text,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign,
                NumberFormatInfo.InvariantInfo);

        public static int ParseInt32(string text) =>
            int.Parse(
                text,
                NumberStyles.Integer | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign,
                NumberFormatInfo.InvariantInfo);
    }
}