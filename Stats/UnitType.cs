using Genjiworlds.Helpers;
using System.Linq;

namespace Genjiworlds.Stats
{
    public class UnitType
    {
        public string name;
        public int hp, attack, defense, protection, ini, exp, level;
        public Range damage, gold;
        public int kills, killed;

        public string Name => name.Capitalize();

        public int CalculateExp(int hero_level)
        {
            int dif = level - hero_level;
            switch(dif)
            {
                case -2:
                    return exp / 10;
                case -1:
                    return exp / 2;
                case 0:
                    return exp;
                case 1:
                    return exp * 3 / 2;
                case 2:
                    return exp * 2;
                default:
                    if (dif < -2)
                        return exp / 10;
                    else
                        return exp * 2;
            }
        }

        public static readonly UnitType[] types = new UnitType[]
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
                exp = 20,
                level = 0
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
                exp = 30,
                level = 1
            },
            new UnitType
            {
                name = "skeleton",
                hp = 13,
                ini = 1,
                attack = 2,
                damage = new Range(3, 6),
                defense = 1,
                protection = 1,
                gold = new Range(20, 50),
                exp = 50,
                level = 2
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
                exp = 80,
                level = 3
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
                exp = 120,
                level = 4
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
                exp = 150,
                level = 5
            },
            new UnitType
            {
                name = "golem",
                hp = 35,
                ini = -1,
                attack = 4,
                damage = new Range(5, 10),
                defense = 2,
                protection = 6,
                gold = new Range(100, 120),
                exp = 200,
                level = 6
            },
            new UnitType
            {
                name = "evil knight",
                hp = 32,
                ini = 2,
                attack = 6,
                damage = new Range(6, 12),
                defense = 6,
                protection = 3,
                gold = new Range(110, 150),
                exp = 250,
                level = 7
            }
        };

        public static UnitType Get(string name)
        {
            return types.Single(x => x.name == name);
        }
    }
}
