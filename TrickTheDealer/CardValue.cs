using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrickTheDealer
{
    public class CardValue
    {
        public int Value { get; private set; }
        public CardValue(int value)
        {
            this.Value = value;
        }

        public string Name
        {
            get
            {
                if (Value >= 2 && Value <= 10)
                {
                    return Value.ToString();
                }
                else if (Value == 11)
                    return "J";
                else if (Value == 12)
                    return "Q";
                else if (Value == 13)
                    return "K";
                else if (Value == 14)
                    return "A";
                else if (Value == -1)
                    return "  ";
                else
                    throw new Exception();
            }
        }

        public override string ToString()
        {
            return Name;
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

            return ((CardValue)obj).Value == this.Value;
        }

        //public bool operator==(object obj)
        //{
        //    return this.Equals(obj);
        //}

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return 31 * this.Value;
        }
    }
}
