namespace NobleBank.Domain.Common
{
    public class Result<T>
    {
        public bool IsSucccess { get; }

        public bool IsFail { get; }

        public T? Value { get; }

        public string Error { get; }

        private Result(T value)
        {
            IsSucccess = true;
            Value = value;
            Error = string.Empty;
        }

        private Result(string error)
        {
            IsFail = true;
            Error = error;
            Value = default;
        }

        public static Result<T> Success(T value) => new(value);

        public static Result<T> Failure(string error) => new(error);
    }
}