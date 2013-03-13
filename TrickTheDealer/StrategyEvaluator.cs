using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    class StrategyEvaluator
    {
        private Dictionary<Strategy, ExpectedDrinks> drinksMap = new Dictionary<Strategy, ExpectedDrinks>();
        private Dictionary<CardValue, double> likelihoods;
        private List<Strategy> strategies;
        public StrategyEvaluator(IEnumerable<Strategy> strategies, Dictionary<CardValue, double> likelihoods)
        {
            this.strategies = new List<Strategy>(strategies);
            this.likelihoods = likelihoods;
            this.strategies.ForEach(s => drinksMap.Add(s, GetStrategyDrinks(s)));
        }

        public Strategy GetBestStrategy(double aggressiveness)
        {
            return GetBestStrategy(strategies, aggressiveness);
        }

        public ExpectedDrinks GetExpectedDrinks(Strategy strategy)
        {
            return drinksMap[strategy];
        }

        private Strategy GetBestStrategy(IEnumerable<Strategy> strategies, double aggressiveness)
        {
            double maxFitness = Double.NegativeInfinity;
            Strategy bestStrategy = null;
            foreach (Strategy strategy in strategies)
            {
                double fitness = GetStrategyFitness(strategy, aggressiveness);
                if (fitness - maxFitness > 0.0000001)
                {
                    maxFitness = fitness;
                    bestStrategy = strategy;
                }
            }
            return bestStrategy;
        }

        public Strategy GetBestStrategyWithConstraints(StrategyConstraint constraint, double aggressiveness)
        {
            IEnumerable<Strategy> validStrategies = strategies;
            if (constraint.MiddleChoice != null)
            {
                validStrategies = validStrategies.Where(s => s.middleChoice.Value.Equals(constraint.MiddleChoice.Value));
            }
            if (constraint.LowerChoice != null)
            {
                validStrategies = validStrategies.Where(s => s.lowerChoice.Value.Equals(constraint.LowerChoice.Value));
            }
            if (constraint.HigherChoice != null)
            {
                validStrategies = validStrategies.Where(s => s.higherChoice.Value.Equals(constraint.HigherChoice.Value));
            }
            return GetBestStrategy(validStrategies, aggressiveness);
        }

        public double GetStrategyFitness(Strategy strategy, double aggressiveness)
        {
            if (aggressiveness < 0 || aggressiveness > 1)
            {
                throw new ArgumentException("The aggressiveness has to be between 0 and 1.");
            }
            //
            // The maximum number of user drinks is 11, and the maximum number of dealer drinks is 10.
            // We normalize user and dealer drinks w.r.t. these values.
            //
            // normalizedUserDrinks == 1 => 0 drinks.
            // normalizedDealerDrinks == 1 => 10 drinks.
            //
            double normalizedUserDrinks = drinksMap[strategy].NormalizedUserDrinks;
            double normalizedDealerDrinks = drinksMap[strategy].NormalizedDealerDrinks;
            return normalizedUserDrinks * (1 - aggressiveness) + normalizedDealerDrinks * aggressiveness;
        }

        private ExpectedDrinks GetStrategyDrinks(Strategy strategy)
        {
           ExpectedDrinks drinks = new ExpectedDrinks(strategy.GetExpectedUserDrinks(likelihoods),
                strategy.GetExpectedDealerDrinks(likelihoods));
           
            drinks.ComputeConditionalDrinks(strategy, likelihoods);

            return drinks;
        }
    }
}
