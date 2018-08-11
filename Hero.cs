using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genjiworlds2
{
    public class Hero
    {
        public string name;
        public int hp;
        public int age;
        public int kills;
        public bool inside_city;

        public Hero()
        {
            inside_city = true;
            hp = 10;
            age = 0;
            kills = 0;
            name = NameGenerator.GetName();
        }

        public bool ShouldRest()
        {
            if (hp <= 5)
                return true;
            else if (hp < 10)
                return Utils.Rand() % 3 != 0;
            else
                return Utils.Rand() % 3 == 0;
        }

        public bool ShouldExitDungeon()
        {
            if (hp <= 2)
                return true;
            else if (hp <= 5)
                return Utils.Rand() % 3 != 0;
            else if (hp <= 9)
                return Utils.Rand() % 3 == 0;
            else
                return Utils.Rand() % 10 == 0;
        }
    }
}
