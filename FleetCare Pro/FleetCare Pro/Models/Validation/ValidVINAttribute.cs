using System.ComponentModel.DataAnnotations;


namespace FleetCare_Pro.Models.Validation
{
    public class ValidVINAttribute : ValidationAttribute
    {
        private static readonly Dictionary<char, int> TransliterationValues =
            new()
            {
            { 'A', 1 }, { 'B', 2 }, { 'C', 3 }, { 'D', 4 },
            { 'E', 5 }, { 'F', 6 }, { 'G', 7 }, { 'H', 8 },
            { 'J', 1 }, { 'K', 2 }, { 'L', 3 }, { 'M', 4 },
            { 'N', 5 }, { 'P', 7 }, { 'R', 9 },
            { 'S', 2 }, { 'T', 3 }, { 'U', 4 }, { 'V', 5 },
            { 'W', 6 }, { 'X', 7 }, { 'Y', 8 }, { 'Z', 9 },

            { '0', 0 }, { '1', 1 }, { '2', 2 }, { '3', 3 },
            { '4', 4 }, { '5', 5 }, { '6', 6 }, { '7', 7 },
            { '8', 8 }, { '9', 9 }
            };

        private static readonly int[] Weights =
        {
          8, 7, 6, 5, 4, 3, 2, 10, 0,
          9, 8, 7, 6, 5, 4, 3, 2
        };

        protected override ValidationResult? IsValid(
            object? value,
            ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("VIN is required.");
            }

            string vin = value.ToString()!.ToUpperInvariant();

            // VIN must contain exactly 17 characters
            if (vin.Length != 17)
            {
                return new ValidationResult(
                    "VIN must contain exactly 17 characters.");
            }

            // I, O and Q are not allowed in VINs
            if (vin.Contains('I') ||
                vin.Contains('O') ||
                vin.Contains('Q'))
            {
                return new ValidationResult(
                    "VIN cannot contain the letters I, O, or Q.");
            }

            // Check that every character is valid
            foreach (char character in vin)
            {
                if (!TransliterationValues.ContainsKey(character))
                {
                    return new ValidationResult(
                        "VIN contains invalid characters.");
                }
            }

            // VIN checksum
            int sum = 0;

            for (int i = 0; i < 17; i++)
            {
                int valueForCharacter =
                    TransliterationValues[vin[i]];

                sum += valueForCharacter * Weights[i];
            }

            int remainder = sum % 11;

            char expectedCheckDigit =
                remainder == 10
                    ? 'X'
                    : remainder.ToString()[0];

            if (vin[8] != expectedCheckDigit)
            {
                return new ValidationResult(
                    "VIN checksum is invalid.");
            }

            return ValidationResult.Success;
        }
    }
    
}
