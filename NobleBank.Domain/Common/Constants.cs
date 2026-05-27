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
            public const string UserIdRequired = "User ID is required.";

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
            public const string LoanIdRequired = "Loan ID is required.";

            public const string CardIdRequired = "Card ID is required.";
            public const string TransactionAmountPositive = "Transaction amount must be greater than 0.";
            public const string TransactionDescriptionRequired = "Transaction description is required.";
            public const string TransactionDescriptionMaxLength = "Transaction description cannot exceed 250 characters.";

            public const string PostTitleRequired = "Post title is required.";
            public const string PostTitleMaxLength = "Post title cannot exceed 200 characters.";
            public const string PostBodyRequired = "Post body is required.";
            public const string PostBodyMaxLength = "Post body cannot exceed 5000 characters.";

            public const string RejectionReasonRequired = "Rejection reason is required.";
            public const string RejectionReasonMaxLength = "Rejection reason cannot exceed 500 characters.";
        }

        public class Exceptions
        {
            public const string LoanTypeInvalid = "Invalid loan type.";
            public const string InvalidEmailFormat = "Invalid email format.";
            public const string EmailAlreadyRegistered = "Email is already registered.";
            public const string InvalidCredentials = "Invalid email or password.";
            public const string InvalidTransactionType = "Invalid transaction type.";

            public const string AccountLocked = "Account is locked. Please try again later or contact support.";
            public const string PostNotFound = "Post not found or does not belong to user.";

            public const string CannotSeedDatabase = "An error occurred while seeding the database.";
            public const string CardNumberGenerationFailed = "Failed to generate a unique card number after multiple attempts.";

            public const string UserNotFound = "Could not find a user with the specified ID.";
            public const string CardNotFound = "Could not find a card with the specified ID.";
            public const string LoanNotFound = "Could not find a loan with the specified ID.";
        }
    }
}
