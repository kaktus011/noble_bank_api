namespace NobleBank.Domain.Common
{
    public class CardEnum
    {
        public enum Brand
        {
            Visa = 0,
            Mastercard = 1,
            AmericanExpress = 2,
            Maestro = 3
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

    public class LoansEnum
    {
        public enum Status
        {
            Active = 0,
            Pending = 1,
            Closed = 2,
            Rejected = 3
        }

        public enum Type
        {
            Personal = 0,
            Mortgage = 1,
            Auto = 2,
            Student = 3
        }
    }

    public class TransactionsEnum
    {
        public enum Type
        {
            Income = 0,
            Expense = 1,
            Transfer = 2,
        }
    }
}
