namespace SWeetJelly.Pooling.Contract
{
    public interface IPool<T> where T : class
    {
        int Count { get; }
        int ActiveCount { get; }
        int InactiveCount { get; }

        T Rent();
        bool TryRent(out T item);

        void Return(T item);

        bool Contains(T item);
        bool IsActive(T item);

        void Clear();
    }
}