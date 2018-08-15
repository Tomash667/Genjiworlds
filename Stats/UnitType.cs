using Genjiworlds.Helpers;
using System.Linq;

namespace Genjiworlds.Stats
{
    public class UnitType
    {
        public string name;
        public int hp, attack, defense, protection, ini, exp;
        public Range damage, gold;
        public bool killed;

        public string Name => name.Capitalize();

        static readonly UnitType[] types = new UnitType[]
        {
            new UnitType
            {
                name = "goblin",
                hp = 5,
                ini = 1,
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
                ini = 0,
                attack = 2,
                damage = new Range(2, 4),
                defense = 0,
                protection = 0,
                gold = new Range(20, 40),
                exp = 30
            },
            new UnitType
            {
                name = "skeleton",
                hp = 13,
                ini = 1,
                attack = 3,
                damage = new Range(3, 6),
                defense = 2,
                protection = 1,
                gold = new Range(20, 50),
                exp = 50
            },
            new UnitType
            {
                name = "zombie",
                hp = 20,
                ini = -1,
                attack = 2,
                damage = new Range(4, 8),
                defense = 0,
                protection = 4,
                gold = new Range(20, 50),
                exp = 80
            },
            new UnitType
            {
                name = "orc warrior",
                hp = 18,
                ini = 0,
                attack = 4,
                damage = new Range(5, 8),
                defense = 2,
                protection = 1,
                gold = new Range(40, 60),
                exp = 120
            },
            new UnitType
            {
                name = "demon",
                hp = 24,
                ini = 1,
                attack = 5,
                damage = new Range(5, 9),
                defense = 5,
                protection = 2,
                gold = new Range(70, 120),
                exp = 150
            }
        };

        public static UnitType Get(string name)
        {
            return types.Single(x => x.name == name);
        }
    }
}
