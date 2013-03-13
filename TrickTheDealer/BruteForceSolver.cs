using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    public class BruteForceSolver
    {
        public List<Strategy> GetAllStrategies()
        {
            List<Strategy> strategies = new List<Strategy>();
            for (int middle = CardDeck.kLowestCardValue; middle <= CardDeck.kHighestCardValue; middle++)
            {
                for (int lower = CardDeck.kLowestCardValue; lower <= middle; lower++)
                {
                    for (int higher = middle; higher <= CardDeck.kHighestCardValue; higher++)
                    {
                        strategies.Add(new Strategy(new CardValue(middle), new CardValue(lower), new CardValue(higher)));
                    }
                }
            }
            return strategies;
        }
    }
}
