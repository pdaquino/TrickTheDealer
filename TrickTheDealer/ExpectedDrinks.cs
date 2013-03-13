using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    class ExpectedDrinks
    {
        public double UserDrinks { get; set; }
        public double DealerDrinks { get; set; }

        public double UserDrinksIfLower { get; set; }
        public double DealerDrinksIfLower { get; set; }

        public double UserDrinksIfHigher { get; set; }
        public double DealerDrinksIfHigher { get; set; }

        public double NormalizedUserDrinks { get; private set; }
        public double NormalizedDealerDrinks { get; private set; }

        public ExpectedDrinks(double userDrinks, double dealerDrinks)
        {
            this.UserDrinks = userDrinks;
            this.DealerDrinks = dealerDrinks;

            this.NormalizedUserDrinks = (11 - UserDrinks) / 11;
            this.NormalizedDealerDrinks = DealerDrinks / 10;
        }

        public void ComputeConditionalDrinks(Strategy strategy, Dictionary<CardValue, double> likelihoods)
        {
            ExpectedDrinks drinksIfLower = ComputeDrinksIfLower(strategy, strategy.middleChoice, likelihoods);
            ExpectedDrinks drinksIfHigher = ComputeDrinksIfHigher(strategy, strategy.middleChoice, likelihoods);

            UserDrinksIfHigher = drinksIfHigher.UserDrinks;
            DealerDrinksIfHigher = drinksIfHigher.DealerDrinks;

            UserDrinksIfLower = drinksIfLower.UserDrinks;
            DealerDrinksIfLower = drinksIfLower.DealerDrinks;
        }

        public static ExpectedDrinks ComputeDrinksIfLower(Strategy strategy, CardValue lowerThan, Dictionary<CardValue, double> likelihoods)
        {
            Dictionary<CardValue, double> normalizedLowerLikelihoods = GetNormalizedLikelihoods(
                likelihoods.Where(x => x.Key.Value < lowerThan.Value));

            return new ExpectedDrinks(strategy.GetExpectedUserDrinks(normalizedLowerLikelihoods),
                                strategy.GetExpectedDealerDrinks(normalizedLowerLikelihoods));
        }

        public static ExpectedDrinks ComputeDrinksIfHigher(Strategy strategy, CardValue higherThan, Dictionary<CardValue, double> likelihoods)
        {
            Dictionary<CardValue, double> normalizedHigherLikelihoods = GetNormalizedLikelihoods(
                likelihoods.Where(x => x.Key.Value > higherThan.Value));

            return new ExpectedDrinks(strategy.GetExpectedUserDrinks(normalizedHigherLikelihoods),
                                strategy.GetExpectedDealerDrinks(normalizedHigherLikelihoods));
        }

        private static Dictionary<CardValue, double> GetNormalizedLikelihoods(IEnumerable<KeyValuePair<CardValue, double>> likelihoods)
        {
            double sum = likelihoods.Sum( x => x.Value);

            Dictionary<CardValue, double> normalizedLikelihoods = new Dictionary<CardValue,double>();
            likelihoods.ToList().ForEach(x => normalizedLikelihoods.Add(x.Key, x.Value / sum));
            
            for(int value = CardDeck.kLowestCardValue; value <= CardDeck.kHighestCardValue; value++)
            {
                if(!normalizedLikelihoods.ContainsKey(new CardValue(value)))
                {
                    normalizedLikelihoods.Add(new CardValue(value), 0);
                }
            }

            return normalizedLikelihoods;
        }
    }
}
