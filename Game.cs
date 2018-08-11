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
        static readonly int[] spawn_rate = { 20, 10, 5 };

        public void Run()
        {
            quit = false;
            Init();
            while(!quit)
            {
                Console.Clear();
                if (first_turn)
                {
                    StringBuilder sb = new StringBuilder($"Turn {turn}, heroes({heroes.Count}): ");
                    foreach (Hero h in heroes)
                        sb.Append($"{h.name}, ");
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(".");
                    Console.WriteLine(sb);
                    first_turn = false;
                }
                else
                {
                    if (watched == null)
                    {
                        Hero oldest = heroes.Where(x => x.age > 0).MaxBy(x => x.age);
                        string oldest_str = "";
                        if (oldest != null)
                            oldest_str = $", oldest {oldest.name} (age {oldest.age})";
                        Hero winner = heroes.Where(x => x.kills > 0).MaxBy(x => x.kills);
                        string winner_str = "";
                        if (winner != null)
                            winner_str = $", winner {winner.name} (kills {winner.kills})";
                        Console.WriteLine($"Turn {turn}, heroes {heroes.Count}{oldest_str}{winner_str}");
                    }
                    else
                        Console.WriteLine($"Turn {turn}, hero {watched.name} - inside {(watched.inside_city ? "city" : "dungeon")} (hp {watched.hp}, age {watched.age}, kills {watched.kills})");
                    Update();
                }
                ParseCommand();
                if(!first_turn)
                    ++turn;
            }
        }

        void ParseCommand()
        {
            while (true)
            {
                Console.Write("> ");
                char c = Console.ReadKey(true).KeyChar;
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
                    if(h.ShouldRest())
                    {
                        h.hp = Math.Min(10, h.hp + Utils.Random(2, 3));
                        if(watched == null || watched == h)
                            Console.WriteLine($"{h.name} rests inside city.");
                    }
                    else
                    {
                        if (watched == null || watched == h)
                            Console.WriteLine($"{h.name} goes into dungeon.");
                        h.inside_city = false;
                    }
                }
                else
                {
                    bool exit = h.ShouldExitDungeon();
                    if(exit)
                    {
                        if(Utils.Rand() % 2 == 0)
                        {
                            if (watched == null || watched == h)
                                Console.WriteLine($"{h.name} returns to city.");
                            h.inside_city = true;
                        }
                        else
                        {
                            if (watched == null || watched == h)
                                Console.WriteLine($"{h.name} searches for exit from dungeon but fails.");
                        }
                    }
                    int e = Utils.Random(0, 3);
                    if (e == 0)
                    {
                        if(!exit && (watched == null || watched == h))
                            Console.WriteLine($"{h.name} explores dungeon.");
                    }
                    else if (e == 1)
                    {
                        int dmg = Utils.Random(2, 8);
                        h.hp -= dmg;
                        if(h.hp <= 0)
                        {
                            to_remove.Add(h);
                            if (watched == null || watched == h)
                                Console.WriteLine($"{h.name} was killed by a trap.");
                        }
                        else
                        {
                            if (watched == null || watched == h)
                                Console.WriteLine($"{h.name} takes {dmg} damage from trap.");
                        }
                    }
                    else if (e == 2)
                    {
                        bool win = Combat(h, watched == h);
                        if (watched == null)
                        {
                            string str;
                            if (Utils.Rand() % 2 == 0)
                                str = "wins combat";
                            else
                                str = "got killed";
                            Console.WriteLine($"{h.name} was attacked by orc and {str}.");
                        }
                    }
                    else
                    {
                        h.hp = Math.Min(10, h.hp + 5);
                        if (watched == null || watched == h)
                            Console.WriteLine($"{h.name} finds healing potion.");
                    }
                }
            }

            foreach (Hero h in to_remove)
            {
                if (h == watched)
                    watched = null;
                heroes.Remove(h);
            }

            foreach (int rate in spawn_rate)
            {
                if (heroes.Count < rate && Utils.Rand() % 2 == 0)
                {
                    Hero h = new Hero();
                    if(watched == null)
                        Console.WriteLine($"{h.name} joins the city.");
                    heroes.Add(h);
                }
            }
        }

        bool Combat(Hero h, bool details)
        {
            if(details)
                Console.WriteLine($"{h.name} was attacked by orc.");
            bool hero_turn = Utils.Rand() % 2 == 0;
            int enemy_hp = 10;
            while(true)
            {
                if(hero_turn)
                {
                    if(Utils.Rand() % 3 != 0)
                    {
                        int dmg = Utils.Random(1, 4);
                        enemy_hp -= dmg;
                        if(enemy_hp <= 0)
                        {
                            if (details)
                                Console.WriteLine($"{h.name} attacks orc for {dmg} damage and kills him.");
                            h.kills++;
                            return true;
                        }
                        else
                        {
                            if(details)
                                Console.WriteLine($"{h.name} attacks orc for {dmg} damage.");
                        }
                    }
                    else
                    {
                        if (details)
                            Console.WriteLine($"{h.name} tries to attack orc but misses.");
                    }
                    hero_turn = false;
                }
                else
                {
                    if (Utils.Rand() % 3 != 0)
                    {
                        int dmg = Utils.Random(1, 4);
                        h.hp -= dmg;
                        if (h.hp <= 0)
                        {
                            to_remove.Add(h);
                            if (details)
                                Console.WriteLine($"Orc attacks {h.name} for {dmg} damage and kills him.");
                            return false;
                        }
                        else
                        {
                            if (details)
                                Console.WriteLine($"Orc attacks {h.name} for {dmg} damage.");
                        }
                    }
                    else
                    {
                        if (details)
                            Console.WriteLine($"Orc tries to attack {h.name} but misses.");
                    }
                    hero_turn = true;
                }
            }
        }
    }
}
