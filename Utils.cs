using System;
using System.Collections.Generic;
using System.Text;

namespace Genjiworlds
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

        public static char ReadKey(string allowed)
        {
            while(true)
            {
                char c = Console.ReadKey(true).KeyChar;
                if (allowed.IndexOf(c) != -1)
                    return c;
            }
        }

        public static string FormatList(List<string> l)
        {
            if (l.Count == 1)
                return l[0];
            StringBuilder sb = new StringBuilder(l[0]);
            for(int i=1; i<l.Count; ++i)
            {
                if (i + 1 == l.Count)
                    sb.Append(" and ");
                else
                    sb.Append(", ");
                sb.Append(l[i]);
            }
            return sb.ToString();
        }
    }
}
