using System.Threading.Channels;

namespace SolidMatrix.Poker.Core;

public class DeckBuilder
{
    public PokerCard[] Cards { get; set; }
    public int[] Numbers { get; set; }

    public DeckBuilder(int[] numbers, int[] suits)
    {
        List<PokerCard> list = new();
        Numbers = numbers.ToArray();
        foreach (var number in numbers)
            foreach (var suit in suits)
                list.Add(new PokerCard { Number = number, Suit = suit });
        Cards = list.ToArray();
    }

    public DeckBuilder(int[] numbers) : this(numbers, new int[] { 0, 1, 2, 3 }) { }

    public DeckBuilder() : this(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) { }

    public long NumOfCombinations()
    {
        long res = 1;
        for (int i = 0; i < 8; i++) res *= Cards.Length - i;
        for (int i = 0; i < 8; i++) res /= i + 1;
        return res;
    }

    public async Task GenerateCombinations(int cardNumber, Channel<PokerCard[]> channel)
    {
        if (cardNumber > Cards.Length) throw new Exception("card number more than deck");

        int[] stack = new int[cardNumber];
        int p = 0;
        stack[p++] = 0;

        while (true)
        {
            if (stack[p - 1] == Cards.Length)
            {
                if (--p == 0) break;
                stack[p - 1]++;
                continue;
            }

            if (p < cardNumber)
            {
                stack[p] = stack[p - 1] + 1;
                p++;
                continue;
            }

            await channel.Writer.WriteAsync(stack.Select(j => Cards[j]).ToArray());
            stack[p - 1]++;
        }

        channel.Writer.Complete();
    }
}
