using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genjiworlds2
{
    public class Utils
    {
        static Random rnd = new Random();

        public static int Rand()
        {
            return rnd.Next();
        }

        public static int Random(int a, int b)
        {
            return rnd.Next(a, b);
        }
    }
}
