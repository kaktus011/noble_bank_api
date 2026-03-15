namespace NobleBank.Domain.Common
{
    public class Card
    {
        public enum Brand
        {
            Visa = 1,
            Mastercard = 2,
            AmericanExpress = 3,
            Maestro = 4
        }

        public enum Type
        {
            Debit = 0,
            Credit = 1,
            Virtual = 2,
        }

        public enum Status
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
        public enum Status
        {
            Active = 0,
            Pending = 1,
            Closed = 2,
        }
    }

    public class Transactions
    {
        public enum Type
        {
            Income = 0,
            Expense = 1,
            Transfer = 2,
        }
    }
}
