using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genjiworlds2
{
    public static class NameGenerator
    {
        private static string[] pre = new string[]
        {
            "to",
            "do",
            "ce",
            "mar",
            "ad",
            "sła",
            "prze",
            "kon"
        };

        private static string[] mid = new string[]
        {
            "ma",
            "mek",
            "mi",
            "za",
            "wo",
            "my"
        };

        private static string[] end = new string[]
        {
            "szu",
            "nik",
            "ry",
            "tin",
            "rian",
            "wek",
            "mir",
            "mek",
            "sław",
            "rad"
        };

        public static string GetName()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(pre[Utils.Rand() % pre.Length]);
            if(Utils.Rand() % 2 == 0)
                sb.Append(mid[Utils.Rand() % mid.Length]);
            sb.Append(end[Utils.Rand() % end.Length]);
            sb[0] = char.ToUpper(sb[0]);
            return sb.ToString();
        }
    }
}
