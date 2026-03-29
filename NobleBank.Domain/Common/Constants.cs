namespace NobleBank.Domain.Common
{
    public class Constants
    {
        public class Card
        {
            public const string DefaultCurrency = "EUR";
            public const string NumberMask = "**** **** **** {0}";
        }

        public class Requirements
        {
            public const string PasswordLength = "Password must be at least 8 characters.";
            public const string PasswordUppercase = "Password must contain an uppercase letter.";
            public const string PasswordNumber = "Password must contain a number.";
            public const string PasswordSpecialChar = "Password must contain a special character.";

            public const string EmailRequired = "Email is required.";
            public const string FirstNameRequired = "First name is required.";
            public const string LastNameRequired = "Last name is required.";
            public const string PasswordRequired = "Password is required.";
        }

        public class ErrrorMessages
        {
            public const string IvalidEmailFormat = "Invalid email format.";
        }
    }
}
