using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genjiworlds2
{
    class Game
    {
        int turn;
        List<Hero> heroes = new List<Hero>();
        List<Hero> to_remove = new List<Hero>();

        public void Run()
        {
            Init();
            while(true)
            {
                Console.Clear();
                Hero oldest = heroes.MaxBy(x => x.age);
                string oldest_str = "";
                if (oldest != null)
                    oldest_str = $", oldest {oldest.name} (age {oldest.age})";
                Console.WriteLine($"Turn {turn}, heroes {heroes.Count}{oldest_str}");
                Update();
                ++turn;
                var key = Console.ReadKey();
                if (key.KeyChar == 'q')
                    break;
            }
        }

        void Init()
        {
            turn = 1;
            heroes.Clear();
            int count = Utils.Random(10, 12);
            for(int i=0; i<count; ++i)
            {
                Hero h = new Hero();
                heroes.Add(h);
            }
        }

        void Update()
        {
            foreach(Hero h in heroes)
            {
                ++h.age;
                if(h.inside_city)
                {
                    if (Utils.Rand() % 2 == 0)
                        Console.WriteLine($"{h.name} rests inside city.");
                    else
                    {
                        Console.WriteLine($"{h.name} goes into dungeon.");
                        h.inside_city = false;
                    }
                }
                else
                {
                    int e = Utils.Random(0, 3);
                    if (e == 0)
                        Console.WriteLine($"{h.name} explores dungeon.");
                    else if (e == 1)
                    {
                        Console.WriteLine($"{h.name} was killed by a trap.");
                        to_remove.Add(h);
                    }
                    else if(e == 2)
                    {
                        string str;
                        if(Utils.Rand() % 2 == 0)
                        {
                            str = "wins combat";
                        }
                        else
                        {
                            str = "got killed";
                            to_remove.Add(h);
                        }
                        Console.WriteLine($"{h.name} was attacked by orc and {str}.");
                    }
                    else
                    {
                        Console.WriteLine($"{h.name} returns to city.");
                        h.inside_city = true;
                    }
                }
            }

            foreach (Hero h in to_remove)
                heroes.Remove(h);

            if(heroes.Count < 20 && Utils.Rand() % 2 == 0)
            {
                Hero h = new Hero();
                Console.WriteLine($"{h.name} joins the city.");
                heroes.Add(h);
            }
        }
    }
}
