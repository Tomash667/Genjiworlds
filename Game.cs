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
        Hero watched;
        bool quit, first_turn;

        public void Run()
        {
            quit = false;
            Init();
            while(!quit)
            {
                Console.Clear();
                if (first_turn)
                {
                    StringBuilder sb = new StringBuilder($"Turn {turn}, heroes: ");
                    foreach (Hero h in heroes)
                        sb.Append($"{h.name}, ");
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(".");
                    Console.WriteLine(sb);
                }
                else
                {
                    if (watched == null)
                    {
                        Hero oldest = heroes.MaxBy(x => x.age);
                        string oldest_str = "";
                        if (oldest != null)
                            oldest_str = $", oldest {oldest.name} (age {oldest.age})";
                        Console.WriteLine($"Turn {turn}, heroes {heroes.Count}{oldest_str}");
                    }
                    else
                        Console.WriteLine($"Turn {turn}, hero {watched.name} (hp {watched.hp}, age {watched.age})");
                    Update();
                }
                ParseCommand();
                ++turn;
            }
        }

        void ParseCommand()
        {
            while (true)
            {
                Console.Write("> ");
                char c = Console.ReadKey().KeyChar;
                switch (c)
                {
                    case 'q':
                        quit = true;
                        return;
                    case 'h':
                        Console.WriteLine("h-help, r-restart, w-watch hero, q-quit");
                        break;
                    case 'r':
                        Init();
                        return;
                    case 'w':
                        Console.Write("Hero name to watch: ");
                        string name = Console.ReadLine();
                        if (name == "null")
                            watched = null;
                        else
                        {
                            watched = heroes.FirstOrDefault(x => x.name == name);
                            if (watched == null)
                                Console.WriteLine($"No hero with name {name}.");
                            else
                                Console.WriteLine($"Watching {name}.");
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        void Init()
        {
            turn = 1;
            first_turn = true;
            watched = null;
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
