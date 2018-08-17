using Genjiworlds.Helpers;

namespace Genjiworlds.Stats
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Bow
    }

    public static class Item
    {
        public static string[] weapon_names =
        {
            "club",
            "sword",
            "battle axe",
            "magic sword",
            "flame axe",
            "ultimate sword"
        };
        public static Range[] weapon_dmg =
        {
            new Range(1,4),
            new Range(2,6),
            new Range(3,8),
            new Range(5,9),
            new Range(6,11),
            new Range(8,14)
        };

        public static string[] armor_names =
        {
            "leather",
            "chain",
            "plate",
            "magic",
            "dragon",
            "ultimate"
        };

        public static string[] bow_names =
        {
            "(none)",
            "short bow",
            "long bow",
            "composite bow",
            "elven bow",
            "ultimate bow"
        };
        public static Range[] bow_dmg =
        {
            new Range(1,4),
            new Range(2,5),
            new Range(3,6),
            new Range(4,7),
            new Range(5,8),
            new Range(6,9)
        };

        public const int max_item_level = 5;
        public static int[] item_price = { 50, 200, 800, 2500, 10000 };
        public const int potion_price = 12;
    }
}
