using SolidMatrix.Poker.Core;
using System.Threading.Channels;

namespace SolidMatrix.Poker.Xiaojiu;


public class Program
{
    private static int ThreadNumber = 8;
    private static Channel<PokerCard[]> cardChannel = Channel.CreateBounded<PokerCard[]>(100000);
    private static Channel<int> countChannel = Channel.CreateBounded<int>(100000);

    private static int?[] records = new int?[100000000];

    private static int[][] permu = new int[][]
    {
        new int[]{0,1,2,3,4,5},
        new int[]{0,1,2,4,3,5},
        new int[]{0,1,2,5,3,4},
        new int[]{0,2,1,3,4,5},
        new int[]{0,2,1,4,3,5},
        new int[]{0,2,1,5,3,4},
        new int[]{0,3,2,1,4,5},
        new int[]{0,3,2,4,1,5},
        new int[]{0,3,2,5,1,4},
        new int[]{0,4,2,3,1,5},
        new int[]{0,4,2,1,3,5},
        new int[]{0,4,2,5,3,1},
        new int[]{0,5,2,3,4,1},
        new int[]{0,5,2,4,3,1},
        new int[]{0,5,2,1,3,4},
    };

    private static ProgressRate progressRate = new();

    private static int ComputeGain(int a1, int a2, int b1, int b2)
    {
        int c = a1 == a2 ? (b1 == b2 ? (a1 >= b1 ? 1 : -2) : 1) : (b1 == b2 ? -2 : ((a1 + a2 + 2) % 10 >= (b1 + b2 + 2) % 10 ? 1 : -1));
        return c;
    }

    private static int ComputeComposition(int[] nums)
    {
        var code = Encode(nums);
        if (records[code] != null) return (int)(records[code])!;

        int count = 0;

        for (int i = 0; i < 8; i++)
            for (int j = i + 1; j < 8; j++)
            {
                int[] b = new int[6];
                int tmp = 0;
                for (int k = 0; k < 8; k++) if (k != i && k != j) b[tmp++] = k;

                for (int k = 0; k < 15; k++)
                {
                    count += ComputeGain(nums[i], nums[j], nums[b[permu[k][0]]], nums[b[permu[k][1]]]);
                    count += ComputeGain(nums[i], nums[j], nums[b[permu[k][2]]], nums[b[permu[k][3]]]);
                    count += ComputeGain(nums[i], nums[j], nums[b[permu[k][4]]], nums[b[permu[k][5]]]);
                }
            }

        records[code] = count;
        return count;
    }

    private static int Encode(int[] nums)
    {
        int code = 0;
        for (int i = 0, timer = 1; i < 8; i++, timer *= 10) code += timer * nums[i];
        return code;
    }

    private static async Task Compute()
    {
        while (true)
        {
            try
            {
                PokerCard[] cards = await cardChannel.Reader.ReadAsync();
                int count = ComputeComposition(cards.Select(c => c.Number).ToArray());
                await countChannel.Writer.WriteAsync(count);
            }
            catch
            {
                break;
            }
        }
    }

    private static async Task<long> Count()
    {
        long sum = 0;
        int percent = 0;

        while (true)
        {
            try
            {
                int count = await countChannel.Reader.ReadAsync();
                sum += count;
                progressRate.Increment();

                if ((int)progressRate.Percent > percent)
                {
                    percent = (int)progressRate.Percent;
                    Console.WriteLine($"{percent}%\t Gain/Game={100.0 * sum / progressRate.Total / 7 / 8 / 15:0.000000}%");
                }
            }
            catch
            {
                break;
            }
        }

        return sum;
    }

    public static async Task Main()
    {
        var deckBuilder = new DeckBuilder(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        progressRate.Total = deckBuilder.NumOfCombinations();

        // produce data
        Task producer = deckBuilder.GenerateCombinations(8, cardChannel);

        // compute data
        Task[] computes = new Task[ThreadNumber];
        for (int i = 0; i < ThreadNumber; i++) computes[i] = Compute();

        // count data
        var count = Count();

        // await all
        await producer;

        for (int i = 0; i < ThreadNumber; i++) await computes[i];

        countChannel.Writer.Complete();

        long sum = await count;


        Console.WriteLine($"Total Gain \t{sum}");
        Console.WriteLine($"Total Games\t{progressRate.Total * 7 * 8 * 15}");
    }
}
