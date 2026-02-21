public interface IPool
{
    IPoolable Get();
    void Release(IPoolable poolable);
    void Clear();
}