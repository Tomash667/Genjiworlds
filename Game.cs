using Genjiworlds.Unit;
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
        bool quit, first_turn, controlled, controlled_quit;
        static readonly int[] spawn_rate = { 20, 10, 5 };
        static readonly byte[] save_sign = { (byte)'G', (byte)'E', (byte)'N', (byte)'J' };
        const byte version = 3;
        const byte file_end_sign = 0xE3;
        PlayerController pc = new PlayerController();
        AiController ai = new AiController();

        public void Run()
        {
            Console.Title = $"Genjiworlds v{version}";
            quit = false;
            pc.game = this;

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
                    else if(!controlled)
                    {
                        Console.WriteLine($"Turn {turn}, hero {watched.name} - inside {(watched.inside_city ? "city" : "dungeon")}\n"
                            + $"(level:{watched.level}, exp:{watched.exp}/{watched.exp_need}, hp:{watched.hp}/{watched.hpmax})");
                    }
                    Update();
                }
                if (!controlled)
                    ParseCommand();
                if(!first_turn)
                    ++turn;
            }
        }

        public bool ParseCommand()
        {
            while (true)
            {
                Console.Write("> ");
                char c = Console.ReadKey(true).KeyChar;
                switch (c)
                {
                    case 'q':
                        quit = true;
                        return true;
                    case 'h':
                        Console.WriteLine("h-help, c-create hero, t-take control, w-watch hero, v-view hero, s-save, l-load, r-restart, q-quit");
                        return false;
                    case 'r':
                        if (controlled)
                            controlled_quit = true;
                        Init();
                        return true;
                    case 'w':
                    case 't':
                        {
                            bool watch = c == 'w';
                            Console.Write($"Hero name to {(watch?"watch":"controll")}: ");
                            string name = Console.ReadLine();
                            if (name == "null")
                            {
                                if (watched != null)
                                {
                                    watched.controlled = false;
                                    watched = null;
                                }
                                if (controlled)
                                    return true;
                            }
                            else
                            {
                                Hero h = heroes.FirstOrDefault(x => x.name == name);
                                if (h == null)
                                    Console.WriteLine($"No hero with name {name}.");
                                else
                                {
                                    bool prev_controlled = controlled;
                                    if (watched != null)
                                        watched.controlled = false;
                                    controlled = !watch;
                                    pc.Clear();
                                    Console.WriteLine($"{(watch ? "Watching" : "Controlling")} {name}.");
                                    if (prev_controlled)
                                        return true;
                                }
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
                        if (controlled)
                            controlled_quit = true;
                        while (true)
                        {
                            LoadResult result = Load();
                            if (result == LoadResult.Ok)
                                return true;
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
                                    return true;
                                }
                            }
                        }
                        break;
                    case 'c':
                        {
                            if (controlled)
                                controlled_quit = true;
                            Console.Write("Hero name: ");
                            string name = Console.ReadLine();
                            Hero h = new Hero(next_id++, true)
                            {
                                name = name,
                                controlled = true
                            };
                            h.PickAttribute();
                            heroes.Add(h);
                            watched = h;
                            controlled = true;
                            pc.Clear();
                            Console.WriteLine($"Controlling {name}.");
                            if (controlled_quit)
                                return true;
                        }
                        break;
                    default:
                        return false;
                }
            }
        }

        void Init()
        {
            next_id = 0;
            turn = 1;
            first_turn = true;
            watched = null;
            controlled = false;
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

                Order order = Order.None;
                IUnitController controller = null;
                if(controlled && h == watched)
                {
                    controller = pc;
                    order = pc.Think(h);
                    if(controlled_quit)
                    {
                        controlled_quit = false;
                        return;
                    }
                }

                if (order == Order.None)
                {
                    controller = ai;
                    ai.notify = watched == null || watched == h;
                    ai.watched = watched == h;
                    order = ai.Think(h);
                }

                switch(order)
                {
                    case Order.Buy:
                        break;
                    case Order.Rest:
                        {
                            int healed = (int)(Utils.Random(0.15f, 0.25f) * h.hpmax);
                            h.hp = Math.Min(h.hpmax, h.hp + healed);
                            controller.Notify($"{h.Name} rests inside city.");
                        }
                        break;
                    case Order.GotoDungeon:
                        controller.Notify($"{h.Name} goes into dungeon.");
                        h.inside_city = false;
                        break;
                    case Order.UsePotion:
                        {
                            int heal = Utils.Random(2, 5) + h.level / 2;
                            controller.Notify($"{h.Name} drinked potion and was healed for {heal} points.");
                            h.hp = Math.Min(h.hpmax, h.hp + heal);
                            --h.potions;
                        }
                        break;
                    case Order.GotoCity:
                        if (Utils.Rand() % 2 == 0)
                        {
                            controller.Notify($"{h.Name} returns to city.");
                            h.inside_city = true;
                        }
                        else
                        {
                            controller.Notify($"{h.Name} searches for exit from dungeon but fails.");
                            ExploreDungeon(h, true, controller);
                        }
                        break;
                    case Order.Explore:
                        ExploreDungeon(h, false, controller);
                        break;
                }
            }

            foreach (Hero h in to_remove)
            {
                if (h == watched)
                {
                    watched.controlled = false;
                    watched = null;
                }
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

        void ExploreDungeon(Hero h, bool exit, IUnitController controller)
        {
            int e = Utils.Random(0, 3);
            if (e == 0)
            {
                if (!exit)
                    controller.Notify($"{h.Name} explores dungeon.");
                h.AddExp(1, controller);
            }
            else if (e == 1)
            {
                int dmg = Utils.Random(2, 8) - h.armor;
                if (dmg < 1)
                    dmg = 1;
                h.hp -= dmg;
                if (h.hp <= 0)
                {
                    to_remove.Add(h);
                    controller.Notify($"{h.Name} was killed by a trap.");
                }
                else
                {
                    controller.Notify($"{h.Name} takes {dmg} damage from trap.");
                    h.AddExp(5, controller);
                }
            }
            else if (e == 2)
            {
                bool win = Combat(h, controller);
                if (watched == null)
                {
                    string str;
                    if (Utils.Rand() % 2 == 0)
                        str = "won combat";
                    else
                        str = "got killed";
                    controller.Notify($"{h.Name} was attacked by orc and {str}.");
                }
                if (win)
                {
                    int reward = Utils.Random(20, 40);
                    h.gold += reward;
                    controller.Notify($"{h.Name} takes {reward} gold from corpse.");
                    h.AddExp(30, controller);
                }
                else
                {
                    // TODO
                }
            }
            else
            {
                e = Utils.Random(0, 4);
                string what;
                if ((e == 1 && h.weapon == Item.max_item_level)
                    || (e == 2 && h.armor == Item.max_item_level)
                    || (e == 3 && h.potions == Hero.max_potions && h.hp == h.hpmax))
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
                        if (h.potions > Hero.max_potions)
                        {
                            h.hp = Math.Min(h.hpmax, (Utils.Random(2, 5) + h.level / 2) * (h.potions - Hero.max_potions));
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
                controller.Notify($"{h.Name} finds {what}.");
                h.AddExp(5, controller);
            }
        }

        bool Combat(Hero h, IUnitController controller)
        {
            bool details = controller.CombatDetails;
            if(details)
                controller.Notify($"{h.Name} was attacked by orc.");
            int player_ini = Utils.Random(1, 10) + h.dex,
                enemy_ini = Utils.Random(1, 10);
            bool hero_turn = player_ini >= enemy_ini;
            int enemy_hp = 10;
            while(true)
            {
                if(hero_turn)
                {
                    int attack = Utils.Random(1, 10) + h.dex;
                    if(attack > 5)
                    {
                        int dmg = Utils.Random(Item.weapon_dmg[h.weapon]) + h.str;
                        enemy_hp -= dmg;
                        if(enemy_hp <= 0)
                        {
                            if (details)
                                controller.Notify($"{h.Name} attacks orc for {dmg} damage and kills him.");
                            h.kills++;
                            return true;
                        }
                        else
                        {
                            if(details)
                                controller.Notify($"{h.Name} attacks orc for {dmg} damage.");
                        }
                    }
                    else
                    {
                        if (details)
                            controller.Notify($"{h.Name} tries to attack orc but misses.");
                    }
                    hero_turn = false;
                }
                else
                {
                    int attack = Utils.Random(1, 10);
                    if (attack > 5 + h.dex)
                    {
                        int dmg = Utils.Random(1, 4) - h.armor;
                        if (dmg < 1)
                            dmg = 1;
                        h.hp -= dmg;
                        if (h.hp <= 0)
                        {
                            to_remove.Add(h);
                            if (details)
                                controller.Notify($"Orc attacks {h.NameMid} for {dmg} damage and kills him.");
                            return false;
                        }
                        else
                        {
                            if (details)
                                controller.Notify($"Orc attacks {h.NameMid} for {dmg} damage.");
                        }
                    }
                    else
                    {
                        if (details)
                            controller.Notify($"Orc tries to attack {h.NameMid} but misses.");
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
                    f.Write(controlled);
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
                    controlled = f.ReadBoolean();
                    if (watched != null)
                        watched.controlled = true;
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
