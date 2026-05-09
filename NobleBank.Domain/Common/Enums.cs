namespace NobleBank.Domain.Common
{
    public class CardEnum
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

    public class LoansEnum
    {
        public enum Status
        {
            Pending = 1,
            Active = 2,
            Closed = 3,
            Rejected = 4
        }

        public enum Type
        {
            Personal = 1,
            Mortgage = 2,
            Auto = 3,
            Student = 4
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
