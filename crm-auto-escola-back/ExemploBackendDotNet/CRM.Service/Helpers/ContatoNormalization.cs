using System;
using System.Collections.Generic;
using System.Linq;

namespace Exemplo.Service.Helpers
{
    public static class ContatoNormalization
    {
        public static HashSet<string> BuildPhoneVariants(string? input)
        {
            var digits = NormalizeDigits(input);
            var variants = new HashSet<string>(StringComparer.Ordinal);

            if (string.IsNullOrWhiteSpace(digits))
            {
                return variants;
            }

            AddLocalAndCountryVariants(RemoveCountryCode(digits), variants);

            return variants;
        }

        public static string NormalizeDigits(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            return new string(input.Where(char.IsDigit).ToArray());
        }

        public static string BuildComparisonKey(string? input)
        {
            var digits = NormalizeDigits(input);
            if (string.IsNullOrWhiteSpace(digits))
            {
                return string.Empty;
            }

            return RemoveNinthDigitAfterDDD(RemoveCountryCode(digits));
        }

        public static bool AreEquivalent(string? left, string? right)
        {
            var leftKey = BuildComparisonKey(left);
            var rightKey = BuildComparisonKey(right);

            return !string.IsNullOrWhiteSpace(leftKey) &&
                   string.Equals(leftKey, rightKey, StringComparison.Ordinal);
        }

        private static void AddLocalAndCountryVariants(string localDigits, ISet<string> variants)
        {
            if (string.IsNullOrWhiteSpace(localDigits))
            {
                return;
            }

            void AddVariant(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                variants.Add(value);
            }

            var withoutNine = RemoveNinthDigitAfterDDD(localDigits);
            var withNine = AddNinthDigitAfterDDD(localDigits);

            AddVariant(localDigits);
            AddVariant(withoutNine);
            AddVariant(withNine);
            AddVariant(EnsureCountryCode(localDigits));
            AddVariant(EnsureCountryCode(withoutNine));
            AddVariant(EnsureCountryCode(withNine));
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

        private static string AddNinthDigitAfterDDD(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return phone;
            }

            var localDigits = RemoveCountryCode(phone);
            if (localDigits.Length == 10)
            {
                return $"{localDigits.Substring(0, 2)}9{localDigits.Substring(2)}";
            }

            return localDigits;
        }

        private static string RemoveCountryCode(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return phone;
            }

            return phone.StartsWith("55", StringComparison.Ordinal)
                ? phone.Substring(2)
                : phone;
        }

        private static string EnsureCountryCode(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return phone;
            }

            return phone.StartsWith("55", StringComparison.Ordinal)
                ? phone
                : $"55{phone}";
        }
    }
}
