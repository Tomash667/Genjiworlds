using System;

namespace Genjiworlds
{
    public static class Item
    {
        public static string[] weapon_names =
        {
            "club",
            "sword",
            "magic sword"
        };
        public static Tuple<int, int>[] weapon_dmg =
        {
            new Tuple<int, int>(1, 4),
            new Tuple<int, int>(2, 6),
            new Tuple<int, int>(3, 8)
        };

        public static string[] armor_names =
        {
            "leather",
            "chain",
            "plate"
        };

        public const int max_item_level = 2;
        public static int[] item_price = { 50, 200 };
        public const int potion_price = 10;
    }
}
