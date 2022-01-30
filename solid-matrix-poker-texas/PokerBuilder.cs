using SolidMatrix.Poker.Core;
using System.Threading.Channels;

namespace SolidMatrix.Poker.Texas;

public class PokerBuilder
{
    public PokerCard[] AllCards = null!;

    private int _remove = 0;

    public PokerBuilder(int remove = 0)
    {
        AllCards = new PokerCard[(13 - remove) * 4];

        _remove = remove;
        for (int n = remove; n < 13; n++)
            for (int s = 0; s < 4; s++)
            {
                AllCards[(n - remove) * 4 + s] = new PokerCard { Number = n, Suit = s };
            }
    }

    public async Task ComputeAllCombinations(Channel<PokerCard[]> channel)
    {
        int count = AllCards.Length;
        for (int p1 = 0; p1 < count; p1++)
            for (int p2 = p1 + 1; p2 < count; p2++)
                for (int p3 = p2 + 1; p3 < count; p3++)
                    for (int p4 = p3 + 1; p4 < count; p4++)
                        for (int p5 = p4 + 1; p5 < count; p5++)
                            for (int p6 = p5 + 1; p6 < count; p6++)
                                for (int p7 = p6 + 1; p7 < count; p7++)
                                    await channel.Writer.WriteAsync(new PokerCard[] { AllCards[p1], AllCards[p2], AllCards[p3], AllCards[p4], AllCards[p5], AllCards[p6], AllCards[p7] });
        channel.Writer.Complete();
    }

    public bool IsStraight(int[] cards) =>

            cards[0] + 1 == cards[1] &&
            cards[1] + 1 == cards[2] &&
            cards[2] + 1 == cards[3] &&
            cards[3] + 1 == cards[4]
         ||
            cards[0] == _remove &&
            cards[1] == _remove + 1 &&
            cards[2] == _remove + 2 &&
            cards[3] == _remove + 3 &&
            cards[4] == 12
        ;

    public bool IsRoyalStraight(int[] cards) =>
        cards[0] == 8 &&
        cards[1] == 9 &&
        cards[2] == 10 &&
        cards[3] == 11 &&
        cards[4] == 12;

    public bool IsFour(int[] cards) =>
        cards[0] == cards[1] && cards[1] == cards[2] && cards[2] == cards[3] ||
        cards[1] == cards[2] && cards[2] == cards[3] && cards[3] == cards[4];

    public bool IsFullHouse(int[] cards) =>
        cards[0] == cards[1] && cards[2] == cards[3] && cards[3] == cards[4] ||
        cards[0] == cards[1] && cards[1] == cards[2] && cards[3] == cards[4];

    public bool IsThree(int[] cards) =>
        cards[0] == cards[1] && cards[1] == cards[2] ||
        cards[1] == cards[2] && cards[2] == cards[3] ||
        cards[2] == cards[3] && cards[3] == cards[4];

    public bool IsTwoPairs(int[] cards) =>
        cards[0] == cards[1] && cards[2] == cards[3] ||
        cards[0] == cards[1] && cards[3] == cards[4] ||
        cards[1] == cards[2] && cards[3] == cards[4];

    public bool IsOnePair(int[] cards) =>
        cards[0] == cards[1] ||
        cards[1] == cards[2] ||
        cards[2] == cards[3] ||
        cards[3] == cards[4];

    public bool IsFlush(int[] cards) =>
        cards[0] == cards[1] &&
        cards[1] == cards[2] &&
        cards[2] == cards[3] &&
        cards[3] == cards[4];

    public PokerCombination ComputeBestPokerCombination(PokerCard[] all)
    {
        int best = (int)PokerCombination.NoPair;

        for (int p1 = 0; p1 < 7; p1++)
            for (int p2 = p1 + 1; p2 < 7; p2++)
                for (int p3 = p2 + 1; p3 < 7; p3++)
                    for (int p4 = p3 + 1; p4 < 7; p4++)
                        for (int p5 = p4 + 1; p5 < 7; p5++)
                        {
                            var numbers = new int[] { all[p1].Number, all[p2].Number, all[p3].Number, all[p4].Number, all[p5].Number };
                            var suits = new int[] { all[p1].Suit, all[p2].Suit, all[p3].Suit, all[p4].Suit, all[p5].Suit };

                            var isFlush = IsFlush(suits);

                            if (isFlush)
                            {
                                best = (int)PokerCombination.Flush > best ? (int)PokerCombination.Flush : best;
                                if (IsRoyalStraight(numbers)) return PokerCombination.RoyalFlush;
                            }
                            if (IsStraight(numbers))
                            {
                                best = (int)PokerCombination.Straight > best ? (int)PokerCombination.Straight : best;
                                if (isFlush)
                                    best = (int)PokerCombination.StraightFlush > best ? (int)PokerCombination.StraightFlush : best;
                            }
                            if (IsFour(numbers))
                                best = (int)PokerCombination.Four > best ? (int)PokerCombination.Four : best;
                            if (IsFullHouse(numbers))
                                best = (int)PokerCombination.FullHouse > best ? (int)PokerCombination.FullHouse : best;
                            if (IsThree(numbers))
                                best = (int)PokerCombination.Three > best ? (int)PokerCombination.Three : best;
                            if (IsTwoPairs(numbers))
                                best = (int)PokerCombination.TwoPairs > best ? (int)PokerCombination.TwoPairs : best;
                            if (IsOnePair(numbers))
                                best = (int)PokerCombination.OnePair > best ? (int)PokerCombination.OnePair : best;
                        }

        return (PokerCombination)best;
    }
}
