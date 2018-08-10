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
        public bool inside_city;

        public Hero()
        {
            inside_city = true;
            hp = 10;
            age = 0;
            name = NameGenerator.GetName();
        }
    }
}
