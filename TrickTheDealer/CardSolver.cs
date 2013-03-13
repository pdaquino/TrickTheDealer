using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    public class CardSolver
    {
        public double ExpectedCardValue(IEnumerable<Card> cardsCollection)
        {
            List<Card> cards = new List<Card>(cardsCollection);
            double valueSum = 0;
            int numberOfCardsLeft = 0;
            cards.ForEach(card => valueSum += card.NumberLeft * card.Value.Value);
            cards.ForEach(card => numberOfCardsLeft += card.NumberLeft);
            return valueSum / numberOfCardsLeft;
        }

        public Strategy FindOptimalStrategy(CardDeck deck)
        {
            double expectedCardValue = ExpectedCardValue(deck.Cards);
            return FindOptimalStrategy(FindClosestExistingCard(expectedCardValue, deck.Cards), deck);
        }

        private Strategy FindOptimalStrategy(CardValue middleChoice, CardDeck deck)
        {
            IEnumerable<Card> cardsLower = deck.Cards.Where(card => card.Value.Value < middleChoice.Value);
            IEnumerable<Card> cardsHigher = deck.Cards.Where(card => card.Value.Value > middleChoice.Value);
            Strategy optimalStrategy = new Strategy(
                middleChoice,
                FindClosestExistingCard(ExpectedCardValue(cardsLower), cardsLower),
                FindClosestExistingCard(ExpectedCardValue(cardsHigher), cardsHigher));
            return optimalStrategy;
        }

        private CardValue FindClosestExistingCard(double d, IEnumerable<Card> cards)
        {
            double minimumDistance = Double.PositiveInfinity;
            CardValue closestCard = new CardValue(-1);
            foreach (Card card in cards.Where(c => c.NumberLeft > 0))
            {
                double distance = Math.Abs(d - card.Value.Value);
                if (distance < minimumDistance)
                {
                    closestCard = card.Value;
                    minimumDistance = distance;
                }
            }
            return closestCard;
        }
    }
}
