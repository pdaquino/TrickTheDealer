using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    public class CardDeck
    {
        public const int kLowestCardValue = 2;
        public const int kHighestCardValue = 14;

        private int totalNumberCardsLeft = (kHighestCardValue - kLowestCardValue + 1) * Card.kSuitsPerCard;

        public List<Card> Cards { get; private set; }

        public CardDeck()
        {
            this.Cards = new List<Card>();
            for (int value = kLowestCardValue; value <= kHighestCardValue; value++)
            {
                Cards.Add(new Card(new CardValue(value), this));
            }
        }

        public void Shuffle()
        {
            foreach(Card card in Cards) {
                card.Reset();
            }
            totalNumberCardsLeft = (kHighestCardValue - kLowestCardValue + 1) * Card.kSuitsPerCard;
        }

        public void NotifyCardDrawn()
        {
            totalNumberCardsLeft--;
        }

        public void NotifyUndoDraw()
        {
            totalNumberCardsLeft++;
        }

        public double CardLikelihood(CardValue cardValue)
        {
            if (totalNumberCardsLeft == 0) return 0;
            return Cards.Where(c => c.Value.Equals(cardValue)).First().NumberLeft / (double)totalNumberCardsLeft;
        }

        public Dictionary<CardValue, double> CardsLikelihoods()
        {
            Dictionary<CardValue, double> likelihoods = new Dictionary<CardValue, double>();
            Cards.ForEach(c => likelihoods.Add(c.Value, CardLikelihood(c.Value)));
            return likelihoods;
        }

        public CardValue HighestRemainingCard()
        {
            for (int value = kHighestCardValue; value >= kLowestCardValue; value--)
            {
                if (findCardByValue(new CardValue(value)).NumberLeft > 0)
                {
                    return new CardValue(value);
                }
            }
            return null;
        }

        public CardValue LowestRemainingCard()
        {
            for (int value = kLowestCardValue; value <= kHighestCardValue; value++)
            {
                if (findCardByValue(new CardValue(value)).NumberLeft > 0)
                {
                    return new CardValue(value);
              }
            }
            return null;
        }

        protected Card findCardByValue(CardValue value)
        {
            foreach (Card card in Cards)
            {
                if (card.Value.Equals(value))
                {
                    return card;
                }
            }

            // there was no such card in the deck
            throw new InvalidOperationException("No such card: " + value.Value);
        }
    }
}
