using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBeg
{
    class Tuple
    {
        public int one;
        public int two;

        public Tuple(int one, int two)
        {
            this.one = one;
            this.two = two;
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Tuple p = obj as Tuple;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (one == p.one) && (two == p.two);
        }

        public bool Equals(Tuple p)
        {
            // If parameter is null return false:
            if ((object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (one == p.one) && (two == p.two);
        }


        public override int GetHashCode()
        {
            return one ^ two;
        }
    }

   

}
