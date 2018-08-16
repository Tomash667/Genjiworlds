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
