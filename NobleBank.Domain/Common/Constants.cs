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
            
            public const string LoanMinAmount = "Loan amount must be greater than 0.";
            public const string LoanMaxAmount = "Loan amount cannot exceed 100,000.";
            public const string LoanMinTerm = "Loan term must be at least 1 month.";
            public const string LoanMaxTerm = "Loan term cannot exceed 30 years (360 months).";
            public const string LoanInterestRate = "Loan interest rate cannot be negative.";
        }

        public class ErrorMessages
        {
            public const string LoanTypeInvalid = "Invalid loan type.";
            public const string InvalidEmailFormat = "Invalid email format.";
        }
    }
}
