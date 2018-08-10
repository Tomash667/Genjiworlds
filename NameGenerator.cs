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
            "ad",
            "ce",
            "do",
            "kon",
            "mar",
            "mi",
            "prze",
            "sła",
            "to"
        };

        private static string[] mid = new string[]
        {
            "ko",
            "ma",
            "mek",
            "mi",
            "my",
            "wo",
            "za"
        };

        private static string[] end = new string[]
        {
            "łaj",
            "mir",
            "mek",
            "nik",
            "rad",
            "rian",
            "ry",
            "sław",
            "szu",
            "tin",
            "wek"
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
