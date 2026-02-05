using System;
using System.Collections.Generic;
using System.Linq;

namespace Exemplo.Service.Helpers
{
    public static class ContatoNormalization
    {
        public static HashSet<string> BuildPhoneVariants(string input)
        {
            var digits = Normalize(input);
            var variants = new HashSet<string>(StringComparer.Ordinal);

            if (string.IsNullOrWhiteSpace(digits))
            {
                return variants;
            }

            void AddVariants(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                variants.Add(value);
                variants.Add(RemoveNinthDigitAfterDDD(value));
            }

            AddVariants(digits);

            if (digits.StartsWith("55", StringComparison.Ordinal))
            {
                AddVariants(digits.Substring(2));
            }

            return variants;
        }

        private static string Normalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return new string(input.Where(char.IsDigit).ToArray());
        }

        private static string RemoveNinthDigitAfterDDD(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return phone;
            }

            if (phone.StartsWith("55", StringComparison.Ordinal) &&
                phone.Length >= 13 &&
                phone[4] == '9')
            {
                return phone.Remove(4, 1);
            }

            if (phone.Length >= 11 && phone[2] == '9')
            {
                return phone.Remove(2, 1);
            }

            return phone;
        }
    }
}
