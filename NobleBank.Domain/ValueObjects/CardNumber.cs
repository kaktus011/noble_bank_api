using NobleBank.Domain.Common;
using NobleBank.Domain.Interfaces;
using System.Linq;

namespace NobleBank.Domain.ValueObjects
{
    public sealed class CardNumber : IEquatable<CardNumber>
    {
        private readonly string _encrypted;
        private readonly string _last4;

        private CardNumber(string encrypted, string last4)
        {
            _encrypted = encrypted;
            _last4 = last4;
        }

        public static CardNumber Create(string plainNumber, IEncryptionService encryption)
        {
            if (string.IsNullOrEmpty(plainNumber) ||
                plainNumber.Length < 4 ||
                !plainNumber.All(char.IsDigit) ||
                !IsValidLuhn(plainNumber))
            {
                throw new DomainException("Invalid card number.");
            }

            string encrypted = encryption.Encrypt(plainNumber);
            string last4 = plainNumber[^4..];  // последните 4 цифри

            return new CardNumber(encrypted, last4);
        }

        private static bool IsValidLuhn(string number)
        {
            if (string.IsNullOrEmpty(number) || !number.All(char.IsDigit))
            {
                return false;
            }

            int[] digits = number.Select(c => c - '0').ToArray();
            int sum = 0;
            bool isSecond = false;

            for (int i = digits.Length - 1; i >= 0; i--)
            {
                int digit = digits[i];
                if (isSecond)
                {
                    digit *= 2;

                    if (digit > 9)
                    {
                        digit -= 9;
                    }
                }

                sum += digit;
                isSecond = !isSecond;
            }

            return sum % 10 == 0;
        }

        public string Last4 => _last4;
        public string Masked => String.Format(Constants.Card.NumberMask, Last4);

        public bool Equals(CardNumber? other) => _encrypted == other?._encrypted;
        public override bool Equals(object? obj) => obj is CardNumber cn && Equals(cn);
        public override int GetHashCode() => _encrypted.GetHashCode();
    }
}
