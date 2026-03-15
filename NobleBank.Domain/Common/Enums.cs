namespace NobleBank.Domain.Common
{
    public class Card
    {
        public enum CardBrand
        {
            Visa = 1,
            Mastercard = 2,
            AmericanExpress = 3,
            Maestro = 4
        }

        public enum CardType
        {
            Debit = 0,
            Credit = 1,
            Virtual = 2,
        }

        public enum CardStatus
        {
            Unknown = 0,
            Pending = 1,
            Active = 2,
            Inactive = 3,
            Blocked = 4,
        }
    }

    public class Loans
    {
        public enum LoanStatus
        {
            Active = 0,
            Pending = 1,
            Closed = 2,
        }
    }

    public class Transactions
    {
        public enum TransactionType
        {
            Income = 0,
            Expense = 1,
            Transfer = 2,
        }
    }
}
