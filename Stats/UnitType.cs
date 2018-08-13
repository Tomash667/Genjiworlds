using Genjiworlds.Helpers;

namespace Genjiworlds.Stats
{
    public class UnitType
    {
        public string name;
        public int hp, attack, defense, protection, exp;
        public Range damage, gold;

        public static readonly UnitType[] types = new UnitType[]
        {
            new UnitType
            {
                name = "goblin",
                hp = 5,
                attack = 1,
                damage = new Range(1, 3),
                defense = 1,
                protection = 0,
                gold = new Range(5, 10),
                exp = 10
            },
            new UnitType
            {
                name = "orc",
                hp = 12,
                attack = 2,
                damage = new Range(2, 4),
                defense = 0,
                protection = 0,
                gold = new Range(20, 40),
                exp = 30
            },
            new UnitType
            {
                name = "demon",
                hp = 20,
                attack = 3,
                damage = new Range(3, 6),
                defense = 1,
                protection = 1,
                gold = new Range(70, 120),
                exp = 75
            }
        };
    }
}
