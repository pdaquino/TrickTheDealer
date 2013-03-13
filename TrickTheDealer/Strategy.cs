using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    public class Strategy
    {
        public CardValue middleChoice;
        public CardValue lowerChoice;
        public CardValue higherChoice;

        public Strategy(CardValue middleChoice, CardValue lowerChoice, CardValue higherChoice)
        {
            this.middleChoice = middleChoice;
            this.lowerChoice = lowerChoice;
            this.higherChoice = higherChoice;
        }

        public const int kUserDrinksIfUserGuessesMiddle = 0;
        public const int kUserDrinksIfUserGuessesHigher = 0;
        public const int kUserDrinksIfUserGuessesLower = 0;
        public const int kUserDrinksIfUserMissesPerDistance = 1;

        public const int kDealerDrinksIfUserGuessesMiddle = 10;
        public const int kDealerDrinksIfUserGuessesHigher = 5;
        public const int kDealerDrinksIfUserGuessesLower = 5;
        public const int kDealerDrinksIfUserMissesPerDistance = 0;

        public double GetExpectedUserDrinks(Dictionary<CardValue, double> likelihoods)
        {
            return GetExpectedDrinks(likelihoods,
                kUserDrinksIfUserGuessesMiddle, kUserDrinksIfUserGuessesHigher, kUserDrinksIfUserGuessesLower, kUserDrinksIfUserMissesPerDistance);
        }

        public double GetExpectedDealerDrinks(Dictionary<CardValue, double> likelihoods)
        {
            return GetExpectedDrinks(likelihoods,
                kDealerDrinksIfUserGuessesMiddle, kDealerDrinksIfUserGuessesHigher, kDealerDrinksIfUserGuessesLower, kDealerDrinksIfUserMissesPerDistance);
        }

        private double GetExpectedDrinks(Dictionary<CardValue, double> likelihoods,
            int drinksIfUserGuessesMiddle, int drinksIfUserGuessesHigher, int drinksIfUserGuessesLower, int drinksIfUserMissesPerDistance)
        {
            double expectedDrinks = 0;
            expectedDrinks += likelihoods[middleChoice] * drinksIfUserGuessesMiddle;
            if (higherChoice.Value > middleChoice.Value)
            {
                expectedDrinks += likelihoods[higherChoice] * drinksIfUserGuessesHigher;
            }
            if (lowerChoice.Value < middleChoice.Value)
            {
                expectedDrinks += likelihoods[lowerChoice] * drinksIfUserGuessesLower;
            }

            if (drinksIfUserMissesPerDistance != 0)
            {
                //
                // Go through all the cards whose values are less than the middle choice.
                // Note we don't have to skip the lower choice because the number of drinks will always be 0.
                //
                foreach (CardValue card in likelihoods.Keys.Where(c => c.Value < middleChoice.Value))
                {
                    expectedDrinks += Math.Abs(card.Value - lowerChoice.Value) * likelihoods[card] * drinksIfUserMissesPerDistance;
                }
                //
                // Go through all the cards with values higher than the middle choice.
                //
                foreach (CardValue card in likelihoods.Keys.Where(c => c.Value > middleChoice.Value))
                {
                    expectedDrinks += Math.Abs(card.Value - higherChoice.Value) * likelihoods[card] * drinksIfUserMissesPerDistance;
                }
            }
            return expectedDrinks;
        }

        public override string ToString()
        {
            return "M: " + middleChoice.ToString() + ", L: " + lowerChoice.ToString() + ", H: " + higherChoice.ToString();
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Strategy other = (Strategy)obj;
            return this.middleChoice.Value.Equals(other.middleChoice.Value) &&
                this.lowerChoice.Value.Equals(other.lowerChoice.Value) &&
                this.higherChoice.Value.Equals(other.higherChoice.Value);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.middleChoice.Value.GetHashCode() + this.lowerChoice.Value.GetHashCode() +
                this.higherChoice.Value.GetHashCode();
        }
    }
}
