using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    public class Card
    {
        public const int kSuitsPerCard = 4;

        public CardValue Value { get; private set;}
        public int NumberDealt { get; private set; }
        public int NumberLeft
        {
            get
            {
                return kSuitsPerCard - NumberDealt;
            }
        }

        private CardDeck deck;

        public Card(CardValue value)
        {
            this.Value = value;
            this.NumberDealt = 0;
        }

        public Card(CardValue value, CardDeck deck)
        {
            this.Value = value;
            this.NumberDealt = 0;
            this.deck = deck;
        }

        public void Reset()
        {
            this.NumberDealt = 0;
        }

        public void CardDrawn() {
            this.NumberDealt++;
            if(NumberDealt > kSuitsPerCard) {
                throw new InvalidOperationException();
            }
            if (deck != null)
            {
                deck.NotifyCardDrawn();
            }
        }

        public void UndoDraw()
        {
            this.NumberDealt--;
            if (NumberDealt < 0)
            {
                throw new InvalidOperationException();
            }
            if (deck != null)
            {
                deck.NotifyUndoDraw();
            }
        }
    }
}
