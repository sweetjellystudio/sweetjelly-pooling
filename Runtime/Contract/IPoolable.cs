namespace SWeetJelly.Pooling.Contract
{
    public interface IPoolable
    {
        void OnRentFromPool();
        void OnReturnToPool();
    }
}