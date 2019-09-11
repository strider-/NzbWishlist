namespace NzbWishlist.Azure.Models
{
    public class Either<TSuccess, TError> where TSuccess : class
                                          where TError : class
    {
        public static implicit operator Either<TSuccess, TError>(TSuccess success) => new Either<TSuccess, TError>(success);

        public static implicit operator Either<TSuccess, TError>(TError error) => new Either<TSuccess, TError>(error);

        public override string ToString() => Successful ? nameof(Successful) : nameof(Failed);

        private Either(TSuccess success) => Success = success;

        private Either(TError error) => Error = error;

        public void Deconstruct(out TSuccess success, out TError error)
        {
            success = Success;
            error = Error;
        }

        public TSuccess Success { get; }

        public TError Error { get; }

        public bool Successful => Success != null;

        public bool Failed => Error != null;
    }
}
