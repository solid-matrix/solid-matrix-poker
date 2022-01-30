using System.Threading.Channels;

namespace SolidMatrix.Poker.Core;

public class ProgressRate
{
    private long _total;

    private long _count;

    public long Total { get => _total; set => _total = value; }

    public long Count { get => _count; }

    public double Percent => 100.0 * Count / Total;

    public ProgressRate() { }

    public ProgressRate(long total) => Total = total;

    public void Increment() => Interlocked.Increment(ref _count);

    public void Decrement() => Interlocked.Decrement(ref _count);
}
