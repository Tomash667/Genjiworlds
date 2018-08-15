using System.Text;

namespace Genjiworlds
{
    public static class NameGenerator
    {
        private static string[] pre = new string[]
        {
            "ad",
            "ce",
            "do",
            "kon",
            "ma",
            "mar",
            "mi",
            "pa",
            "pio",
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
            "te",
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
            "tryk",
            "trek",
            "usz",
            "weł",
            "wek"
        };

        public static string GetName()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(pre[Utils.Rand() % pre.Length]);
            if (Utils.Rand() % 2 == 0)
                sb.Append(mid[Utils.Rand() % mid.Length]);
            sb.Append(end[Utils.Rand() % end.Length]);
            sb[0] = char.ToUpper(sb[0]);
            return sb.ToString();
        }
    }
}
