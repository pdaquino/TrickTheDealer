using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    class StrategyConstraint
    {
        public CardValue MiddleChoice { get; set; }
        public CardValue LowerChoice { get; set; }
        public CardValue HigherChoice { get; set; }

        public bool AreAllNull()
        {
            return MiddleChoice == null && LowerChoice == null && HigherChoice == null;
        }

        public void Reset()
        {
            this.MiddleChoice = this.LowerChoice = this.HigherChoice = null;
        }

        public bool IsSatisfiedBy(Strategy strategy)
        {
            if(MiddleChoice != null && !MiddleChoice.Equals(strategy.middleChoice))
                return false;
            if (HigherChoice != null && !HigherChoice.Equals(strategy.higherChoice))
                return false;
            if (LowerChoice != null && !LowerChoice.Equals(strategy.lowerChoice))
                return false;
            return true;
        }
    }
}

