using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Genjiworlds
{
    class Game
    {
        int turn, next_id;
        List<Hero> heroes = new List<Hero>();
        List<Hero> to_remove = new List<Hero>();
        Hero watched;
        bool quit, first_turn;
        static readonly int[] spawn_rate = { 20, 10, 5 };
        static readonly byte[] save_sign = { (byte)'G', (byte)'E', (byte)'N', (byte)'J' };
        const byte version = 2;
        const byte file_end_sign = 0xE3;

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
                        Console.WriteLine($"Turn {turn}, hero {watched.name} - inside {(watched.inside_city ? "city" : "dungeon")} (hp {watched.hp}/{Hero.max_hp}, age {watched.age}, kills {watched.kills})");
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
                        Console.WriteLine("h-help, r-restart, w-watch hero, v-view hero, s-save, l-load, q-quit");
                        break;
                    case 'r':
                        Init();
                        return;
                    case 'w':
                        {
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
                        }
                        break;
                    case 'v':
                        if (watched == null)
                        {
                            Console.Write("Hero name: ");
                            string name = Console.ReadLine();
                            Hero h = heroes.FirstOrDefault(x => x.name == name);
                            if (h == null)
                                Console.WriteLine($"No hero with name {name}.");
                            else
                                h.ShowInfo();
                        }
                        else
                            watched.ShowInfo();
                        break;
                    case 's':
                        Save();
                        break;
                    case 'l':
                        while (true)
                        {
                            LoadResult result = Load();
                            if (result == LoadResult.Ok)
                                return;
                            else if (result == LoadResult.NoFile)
                            {
                                Console.WriteLine("No save file.");
                                break;
                            }
                            else
                            {
                                Console.WriteLine("What to do? (r-retry, n-new game, q-quit)");
                                c = Utils.ReadKey("rnq");
                                if (c == 'r')
                                    continue;
                                else if(c == 'q')
                                {
                                    quit = true;
                                    break;
                                }
                                else
                                {
                                    Init();
                                    return;
                                }
                            }
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        void Init()
        {
            next_id = 0;
            turn = 1;
            first_turn = true;
            watched = null;
            heroes.Clear();
            int count = Utils.Random(10, 12);
            for(int i=0; i<count; ++i)
            {
                Hero h = new Hero(next_id++);
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
                    if(h.BuyItems(watched == null || watched == h))
                    {
                        // bought something
                    }
                    else if(h.ShouldRest())
                    {
                        h.hp = Math.Min(Hero.max_hp, h.hp + Utils.Random(2, 3));
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
                    if(h.potions > 0 && h.ShouldDrinkPotion())
                    {
                        int heal = Utils.Random(2, 5);
                        if (watched == null || watched == h)
                            Console.WriteLine($"{h.name} drinks potion and is healed for {heal} points.");
                        h.hp = Math.Min(Hero.max_hp, h.hp + heal);
                        --h.potions;
                        continue;
                    }

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
                        int dmg = Utils.Random(2, 8) - h.armor;
                        if (dmg < 1)
                            dmg = 1;
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
                        if(win)
                        {
                            int reward = Utils.Random(20, 40);
                            h.gold += reward;
                            if (watched == null || watched == h)
                                Console.WriteLine($"{h.name} takes {reward} gold from corpse.");
                        }
                    }
                    else
                    {
                        e = Utils.Random(0, 4);
                        string what;
                        if ((e == 1 && h.weapon == Item.max_item_level)
                            || (e == 2 && h.armor == Item.max_item_level)
                            || (e == 3 && h.potions == Hero.max_potions && h.hp == Hero.max_hp))
                            e = 0;
                        switch (e)
                        {
                            default:
                            case 0:
                                {
                                    int count = Utils.Random(10, 20);
                                    what = $"{count} gold pile";
                                    h.gold += count;
                                }
                                break;
                            case 1:
                                h.weapon++;
                                what = Item.weapon_names[h.weapon];
                                break;
                            case 2:
                                h.armor++;
                                what = $"{Item.armor_names[h.armor]} armor";
                                break;
                            case 3:
                                h.potions += 2;
                                if(h.potions > Hero.max_potions)
                                {
                                    h.hp = Math.Min(Hero.max_hp, Utils.Random(2, 5) * (h.potions - Hero.max_potions));
                                    h.potions = Hero.max_potions;
                                }
                                what = "potions";
                                break;
                            case 4:
                                {
                                    int count = Utils.Random(50, 100);
                                    what = $"{count} gold treasure";
                                    h.gold += count;
                                }
                                break;
                        }
                        if (watched == null || watched == h)
                            Console.WriteLine($"{h.name} finds {what}.");
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
                    Hero h = new Hero(next_id++);
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
                        int dmg = Utils.Random(1, 4) + h.weapon;
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
                        int dmg = Utils.Random(1, 4) - h.armor;
                        if (dmg < 1)
                            dmg = 1;
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

        void Save()
        {
            try
            {
                using (FileStream fs = new FileStream("save", FileMode.Create))
                using(BinaryWriter f = new BinaryWriter(fs))
                {
                    f.Write(save_sign);
                    f.Write(version);
                    f.Write(turn);
                    f.Write(next_id);
                    f.Write(heroes.Count);
                    foreach (Hero h in heroes)
                        h.Save(f);
                    f.Write(watched?.id ?? -1);
                    f.Write(file_end_sign);
                }
                Console.WriteLine("Save completed.");
            }
            catch(Exception e)
            {
                Console.WriteLine($"Save failed: {e}");
            }
        }

        enum LoadResult
        {
            NoFile,
            Failed,
            Ok
        }

        LoadResult Load()
        {
            try
            {
                using (FileStream fs = new FileStream("save", FileMode.Open))
                using (BinaryReader f = new BinaryReader(fs))
                {
                    byte[] sign = f.ReadBytes(4);
                    if (!sign.SequenceEqual(save_sign))
                        throw new Exception("Invalid file signature.");
                    byte ver = f.ReadByte();
                    if (ver != version)
                        throw new Exception($"Unsupported version {ver} (current {version}).");
                    turn = f.ReadInt32();
                    next_id = f.ReadInt32();
                    int count = f.ReadInt32();
                    heroes.Clear();
                    for(int i=0; i<count; ++i)
                    {
                        Hero h = new Hero();
                        h.Load(f);
                        heroes.Add(h);
                    }
                    int watched_id = f.ReadInt32();
                    if (watched_id == -1)
                        watched = null;
                    else
                        watched = heroes.Single(x => x.id == watched_id);
                    first_turn = true;
                    return LoadResult.Ok;
                }
            }
            catch(FileNotFoundException)
            {
                return LoadResult.NoFile;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Load failed: {e}");
                return LoadResult.Failed;
            }
        }
    }
}
