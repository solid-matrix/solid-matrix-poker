using SolidMatrix.Poker.Core;
using System.Threading.Channels;

namespace SolidMatrix.Poker.Texas;

class Program
{
    static readonly string outputFilename = "output.txt";
    static readonly int NumbersToRemove = 4;
    static readonly int ThreadNumber = 16;
    static Channel<PokerCard[]> combChannel = Channel.CreateBounded<PokerCard[]>(100000);
    static int[] Count = new int[11];
    static PokerBuilder pokerBuilder = new PokerBuilder(NumbersToRemove);

    static async Task Worker()
    {
        while (true)
        {
            try
            {
                PokerCard[] cards = await combChannel.Reader.ReadAsync();
                var bestType = pokerBuilder.ComputeBestPokerCombination(cards);

                Interlocked.Increment(ref Count[(int)bestType]);
            }
            catch
            {
                break;
            }
        }
    }

    public static void Main()
    {
        Task[] waits = new Task[ThreadNumber];

        // produce data
        waits[0] = Task.Run(() => pokerBuilder.ComputeAllCombinations(combChannel));

        // consume data
        for (int i = 1; i < ThreadNumber; i++) waits[i] = Task.Run(async () => await Worker());

        // wait
        for (int i = 0; i < ThreadNumber; i++) waits[i].Wait();

        // output
        using TextWriter tw = new StreamWriter(outputFilename);
        for (int i = 10; i >= 1; i--)
            tw.WriteLine($"{Enum.GetName((PokerCombination)i)}={Count[i]}");

    }
}
