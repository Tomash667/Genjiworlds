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
