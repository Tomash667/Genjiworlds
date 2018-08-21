using System.Linq;

namespace Genjiworlds.Stats
{
    public enum RaceId
    {
        Human,
        HalfOrc,
        Elf,
        Dwarf
    }

    public class Race
    {
        public enum Bonus
        {
            Str,
            Dex,
            End,
            Random
        }

        public RaceId id;
        public string name, desc;
        public Bonus bonus;

        public Attribute GetAttribute()
        {
            switch(bonus)
            {
                case Bonus.Str:
                    return Attribute.Strength;
                case Bonus.Dex:
                    return Attribute.Dexterity;
                case Bonus.End:
                    return Attribute.Endurance;
                case Bonus.Random:
                default:
                    return (Attribute)(Utils.Rand() % 3);
            }
        }

        public static Race[] races = new Race[]
        {
            new Race
            {
                id = RaceId.Human,
                name = "human",
                desc = "+random attribute",
                bonus = Bonus.Random
            },
            new Race
            {
                id = RaceId.HalfOrc,
                name = "half-orc",
                desc = "+strength",
                bonus = Bonus.Str
            },
            new Race
            {
                id = RaceId.Elf,
                name = "elf",
                desc = "+dexterity",
                bonus = Bonus.Dex
            },
            new Race
            {
                id = RaceId.Dwarf,
                name = "dwarf",
                desc = "+endurance",
                bonus = Bonus.End
            }
        };

        public static Race GetRandom()
        {
            return races[Utils.Rand() % races.Length];
        }

        public static Race Get(RaceId id)
        {
            return races.Single(x => x.id == id);
        }
    }
}
