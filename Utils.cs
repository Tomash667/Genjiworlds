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
            return rnd.Next(a, b + 1);
        }

        public static int Random(Tuple<int, int> t)
        {
            return rnd.Next(t.Item1, t.Item2 + 1);
        }

        public static float Random(float a, float b)
        {
            return ((float)((b - a) * rnd.NextDouble())) + a;
        }

        public static char ReadKey(string allowed)
        {
            while (true)
            {
                char c = Console.ReadKey(true).KeyChar;
                if (allowed.IndexOf(c) != -1)
                    return c;
            }
        }

        public static void Ok()
        {
            Console.WriteLine("\n\n(ok)");
            Console.ReadKey(true);
        }

        public static string FormatList(List<string> l)
        {
            if (l.Count == 1)
                return l[0];
            StringBuilder sb = new StringBuilder(l[0]);
            for (int i = 1; i < l.Count; ++i)
            {
                if (i + 1 == l.Count)
                    sb.Append(" and ");
                else
                    sb.Append(", ");
                sb.Append(l[i]);
            }
            return sb.ToString();
        }

        public static int Min(params int[] values)
        {
            if (values.Length == 0)
                return 0;
            int min = values[0];
            for (int i = 1; i < values.Length; ++i)
            {
                if (values[i] < min)
                    min = values[i];
            }
            return min;
        }
    }
}
