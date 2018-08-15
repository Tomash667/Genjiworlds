using System;

namespace Genjiworlds
{
    public static class Item
    {
        public static string[] weapon_names =
        {
            "club",
            "sword",
            "battle axe",
            "magic sword"
        };
        public static Tuple<int, int>[] weapon_dmg =
        {
            new Tuple<int, int>(1, 4),
            new Tuple<int, int>(2, 6),
            new Tuple<int, int>(3, 8),
            new Tuple<int, int>(5, 9)
        };

        public static string[] armor_names =
        {
            "leather",
            "chain",
            "plate",
            "magic"
        };

        public const int max_item_level = 3;
        public static int[] item_price = { 50, 200, 1000 };
        public const int potion_price = 12;
    }
}
