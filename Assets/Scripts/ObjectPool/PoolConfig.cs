public class PoolConfig
{
    public bool AutoPrewarm { get; set; } = true;
    public int InitialSize { get; set; } = 10;
    public int MaxSize { get; set; }
}