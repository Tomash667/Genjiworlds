using System.Text;

namespace Genjiworlds
{
    public static class NameGenerator
    {
        private static string[] pre = new string[]
        {
            "ad",
            "bar",
            "ce",
            "do",
            "ed",
            "fi",
            "grze",
            "hen",
            "i",
            "ja",
            "kon",
            "le",
            "łu",
            "ma",
            "mar",
            "mi",
            "nor",
            "o",
            "pa",
            "pio",
            "prze",
            "ra",
            "sła",
            "to",
            "woj",
            "zbig"
        };

        private static string[] mid = new string[]
        {
            "do",
            "ko",
            "li",
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
            "bert",
            "cek",
            "ciech",
            "gor",
            "gorz",
            "lip",
            "łaj",
            "mir",
            "mek",
            "niew",
            "nik",
            "on",
            "kasz",
            "rad",
            "rian",
            "ry",
            "ryk",
            "sław",
            "szu",
            "tek",
            "tin",
            "tryk",
            "trek",
            "usz",
            "ward",
            "weł",
            "wek",
            "wier"
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
