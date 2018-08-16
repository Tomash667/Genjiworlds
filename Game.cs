using Genjiworlds.Stats;
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
        public static Game instance;
        int turn, next_id, max_hero_level, max_dungeon_level;
        List<Hero> heroes = new List<Hero>();
        List<Hero> to_remove = new List<Hero>();
        List<string> journal = new List<string>();
        Hero watched, chempion;
        bool quit, first_turn, controlled, controlled_quit, controlled_die;
        static readonly int[] spawn_rate = { 20, 10, 5 };
        static readonly byte[] save_sign = { (byte)'G', (byte)'E', (byte)'N', (byte)'J' };
        const byte version = 4;
        const byte file_end_sign = 0xE3;
        PlayerController pc = new PlayerController();
        AiController ai = new AiController();

        public void Run()
        {
            Console.Title = $"Genjiworlds v{version}";
            quit = false;
            instance = this;

            Init();
            while (!quit)
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
                        Hero best = heroes.OrderByDescending(x => x.level).ThenByDescending(x => x.exp).FirstOrDefault();
                        string best_str = "";
                        if (best != null)
                            best_str = $", best hero {best.name} (level {best.level})";
                        Console.WriteLine($"Turn {turn}, heroes {heroes.Count}{best_str}");
                    }
                    else if (!controlled)
                    {
                        Console.WriteLine($"Turn {turn}, hero {watched.name} - inside {watched.Location}\n"
                            + $"(level:{watched.level}, exp:{watched.exp}/{watched.exp_need}, hp:{watched.hp}/{watched.hpmax})");
                    }
                    Update();
                }
                if (!controlled)
                {
                    if (controlled_die)
                        controlled_die = false;
                    else
                        ParseCommand();
                }
                if (!first_turn)
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
                    case 'Q':
                        quit = true;
                        return true;
                    case 'h':
                    case '?':
                        Console.WriteLine("h-help, c-create hero, t-take control, w-watch hero, v-view hero, l-list heroes, j-journal, s-stats, S-save, L-load, R-restart, Q-quit, @-cheats");
                        break;
                    case 'R':
                        if (controlled)
                            controlled_quit = true;
                        Init();
                        return true;
                    case 'w':
                    case 't':
                        {
                            bool watch = c == 'w';
                            Console.Write($"Hero name to {(watch ? "watch" : "controll")}: ");
                            string name = Console.ReadLine();
                            if (name == "null")
                            {
                                bool was_controlled = controlled;
                                if (watched != null)
                                {
                                    watched.controlled = false;
                                    watched = null;
                                    controlled = false;
                                }
                                if (was_controlled)
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
                                    watched = h;
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
                    case 'S':
                        Save();
                        break;
                    case 'L':
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
                                else if (c == 'q')
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
                            Console.WriteLine("Pick race:");
                            int index = 1;
                            StringBuilder choices = new StringBuilder("r");
                            foreach(Race race in Race.races)
                            {
                                Console.WriteLine($"{index}. {race.name} ({race.desc})");
                                ++index;
                                choices.Append($"{index}");
                            }
                            Console.WriteLine("r. random\n>");
                            char rc = Utils.ReadKey(choices.ToString());
                            if (rc == 'r')
                                h.race = Race.GetRandom();
                            else
                                h.race = Race.races[rc - '1'];
                            h.ApplyRaceBonus(null);
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
                    case 'j':
                        Console.WriteLine("World journal:");
                        foreach (string str in journal)
                            Console.WriteLine($"* {str}");
                        if (journal.Count == 0)
                            Console.WriteLine("(empty)");
                        break;
                    case ' ':
                    case '\n':
                    case '\r':
                        return false;
                    case '@':
                        {
                            Console.WriteLine("Cheat command: ");
                            string[] strs = Console.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (strs.Length == 0)
                                break;
                            switch(strs[0])
                            {
                                case "help":
                                    Console.WriteLine("=== Available cheats ===\nhelp - list all\nskip N - fast forward time\ngold N - give gold to watched hero\nimmortal - makes watched hero immortal");
                                    break;
                                case "skip":
                                    if(strs.Length >= 2 && int.TryParse(strs[1], out int turns) && turns > 0)
                                    {
                                        Hero old_watched = watched;
                                        Hero h = new Hero();
                                        watched = h;
                                        for (int i = 0; i < turns; ++i)
                                        {
                                            Update();
                                            ++turn;
                                        }
                                        watched = old_watched;
                                    }
                                    break;
                                case "gold":
                                    if(strs.Length >= 2 && int.TryParse(strs[1], out int gold) && watched != null)
                                    {
                                        watched.gold += gold;
                                        Console.WriteLine($"Added {gold} gold to {watched.name}.");
                                    }
                                    break;
                                case "immortal":
                                    if(watched != null)
                                    {
                                        watched.immortal = !watched.immortal;
                                        Console.WriteLine($"{watched.Name} is now {(watched.immortal ? "" : "not ")}immortal.");
                                    }
                                    break;
                                default:
                                    Console.WriteLine($"Unknown cheat '{strs[0]}'.");
                                    break;
                            }
                        }
                        break;
                    case 's':
                        {
                            Console.WriteLine("=== Stats ===");
                            Hero oldest = heroes.Where(x => x.age > 0).MaxBy(x => x.age);
                            if (oldest != null)
                                Console.WriteLine($"Oldest hero: {oldest.Name} (age {oldest.age})");
                            Hero killer = heroes.Where(x => x.kills > 0).MaxBy(x => x.kills);
                            if (killer != null)
                                Console.WriteLine($"Killer hero: {killer.Name} (kills {killer.kills})");
                            Hero expert = heroes.OrderByDescending(x => x.level).ThenByDescending(x => x.exp).FirstOrDefault();
                            if (expert != null)
                                Console.WriteLine($"Expert hero: {expert.name} (level {expert.level}, exp {expert.ExpP}%)");
                            bool any = false;
                            foreach(UnitType unit in UnitType.types)
                            {
                                if(unit.kills != 0 || unit.killed != 0)
                                {
                                    Console.WriteLine($"{unit.name}, killed {unit.killed}, kills {unit.kills}, win ratio {Utils.Ratio(unit.killed, unit.kills)}%");
                                    any = true;
                                }
                            }
                            if (!any)
                                Console.WriteLine("(empty)");
                        }
                        break;
                    case 'l':
                        Console.WriteLine("=== Heroes ===");
                        foreach (Hero h in heroes)
                            Console.WriteLine($"{h.name} - level {h.level}, {h.Location}, {h.race.name}, age {h.age}");
                        if (heroes.Count == 0)
                            Console.WriteLine("(empty)");
                        break;
                }
            }
        }

        void Init()
        {
            next_id = 0;
            turn = 1;
            max_hero_level = 1;
            max_dungeon_level = 1;
            first_turn = true;
            watched = null;
            controlled = false;
            heroes.Clear();
            journal.Clear();
            int count = Utils.Random(10, 12);
            for (int i = 0; i < count; ++i)
            {
                Hero h = new Hero(next_id++);
                heroes.Add(h);
            }
        }

        void Update()
        {
            foreach (Hero h in heroes)
            {
                ++h.age;

                Order order = Order.None;
                IUnitController controller = null;
                if (controlled && h == watched)
                {
                    controller = pc;
                    order = pc.Think(h);
                    if (controlled_quit)
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

                switch (order)
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
                    case Order.GoDown:
                        if (h.dungeon_level == 0)
                        {
                            controller.Notify($"{h.Name} goes into dungeon.");
                            h.dungeon_level = 1;
                            if (h.lowest_level == 0)
                                h.lowest_level = 1;
                            h.PickTarget();
                        }
                        else
                        {
                            bool ok;
                            if (h.lowest_level == h.dungeon_level)
                                ok = Utils.Rand() % 4 == 0;
                            else if (h.lowest_level - 1 == h.dungeon_level)
                                ok = Utils.Rand() % 4 != 0;
                            else
                                ok = true;
                            if (ok)
                            {
                                ++h.dungeon_level;
                                controller.Notify($"{h.Name} goes to lower dungeon level {h.dungeon_level}.");
                                if (h.lowest_level < h.dungeon_level)
                                {
                                    ++h.lowest_level;
                                    h.know_down_stairs = false;
                                    h.AddExp(10 * h.dungeon_level, controller);
                                    if (h.dungeon_level > max_dungeon_level)
                                    {
                                        max_dungeon_level = h.dungeon_level;
                                        journal.Add($"{turn} - Hero {h.name} reached {h.dungeon_level} dungeon level.");
                                        chempion = h;
                                    }
                                }
                                if (h.target_level == h.dungeon_level && h.ai_order == AiOrder.GotoTarget)
                                    h.ai_order = AiOrder.Explore;
                            }
                            else
                            {
                                controller.Notify($"{h.name} searches for stairs to lower dungeon level but fails.");
                                ExploreDungeon(h, true, controller);
                            }
                        }
                        break;
                    case Order.UsePotion:
                        {
                            int heal = Utils.Random(2, 5) + h.level / 2 + h.end;
                            controller.Notify($"{h.Name} drinked potion and was healed for {heal} points.");
                            h.hp = Math.Min(h.hpmax, h.hp + heal);
                            --h.potions;
                        }
                        break;
                    case Order.GoUp:
                        {
                            bool ok;
                            if (h.dungeon_level == h.lowest_level)
                                ok = Utils.Rand() % 2 == 0;
                            else if (h.dungeon_level + 1 == h.lowest_level)
                                ok = Utils.Rand() % 4 != 0;
                            else
                                ok = true;
                            if (ok)
                            {
                                --h.dungeon_level;
                                if (h.dungeon_level == 0)
                                    controller.Notify($"{h.Name} returns to city.");
                                else
                                    controller.Notify($"{h.Name} goes to upper dungeon level {h.dungeon_level}.");
                            }
                            else
                            {
                                controller.Notify($"{h.Name} searches for exit from dungeon but fails.");
                                ExploreDungeon(h, true, controller);
                            }
                        }
                        break;
                    case Order.Explore:
                        ExploreDungeon(h, false, controller);
                        break;
                }
            }

            foreach (Hero h in to_remove)
            {
                if (h == chempion)
                {
                    journal.Add($"{turn} - Champion {h.name} killed by {(h.killer?.name ?? "trap")} at {h.Location}.");
                    chempion = null;
                }
                if (h == watched)
                {
                    watched.controlled = false;
                    watched = null;
                    controlled = false;
                    controlled_die = true;
                }
                heroes.Remove(h);
            }

            foreach (int rate in spawn_rate)
            {
                if (heroes.Count < rate && Utils.Rand() % 2 == 0)
                {
                    Hero h = new Hero(next_id++);
                    if (watched == null)
                        Console.WriteLine($"{h.name} joins the city.");
                    heroes.Add(h);
                }
            }
        }

        enum ExploreEvent
        {
            Nothing,
            Trap,
            Combat,
            Treasure,
            Stairs,
            Max
        }

        enum Treasure
        {
            GoldPile,
            Weapon,
            Armor,
            Potions,
            GoldTreasure,
            Max
        }

        void ExploreDungeon(Hero h, bool exit, IUnitController controller)
        {
            ExploreEvent e = (ExploreEvent)(Utils.Rand() % (int)ExploreEvent.Max);

            if (e == ExploreEvent.Stairs)
            {
                if (h.dungeon_level != h.lowest_level || h.know_down_stairs)
                    e = (ExploreEvent)(Utils.Rand() % ((int)ExploreEvent.Max - 1));
            }

            if (!exit)
                controller.Notify($"{h.Name} explores dungeon.");

            if (e == ExploreEvent.Nothing)
                h.AddExp(h.dungeon_level / h.level, controller);
            else if (e == ExploreEvent.Trap)
            {
                int attack = Utils.Random(1, 10) - h.dex;
                if (attack >= 3)
                {
                    int dmg = Utils.Random(2, 8) - h.armor;
                    if (dmg < 1)
                        dmg = 1;
                    h.hp -= dmg;
                    if (h.immortal && h.hp < 0)
                        h.hp = 1;
                    if (h.hp <= 0)
                    {
                        to_remove.Add(h);
                        if (watched == h && controlled)
                        {
                            controller.Notify($"You take {dmg} damage from trap.");
                            pc.Die();
                        }
                        else
                            controller.Notify($"{h.Name} takes {dmg} damage and is killed by a trap.");
                    }
                    else
                    {
                        controller.Notify($"{h.Name} takes {dmg} damage from trap.");
                        h.AddExp(5 * h.dungeon_level / h.level, controller);
                    }
                }
                else
                {
                    controller.Notify($"{h.Name} dodged trap.");
                    h.AddExp(5 * h.dungeon_level / h.level, controller);
                }
            }
            else if (e == ExploreEvent.Combat)
            {
                UnitType enemy = GetCombatEnemy(h.dungeon_level);
                bool win = Combat(h, controller, enemy);
                if (watched == null)
                {
                    string str;
                    if (win)
                        str = "won combat";
                    else
                        str = "got killed";
                    controller.Notify($"{h.Name} was attacked by {enemy.name} and {str}.");
                }
                if (win)
                {
                    int reward = enemy.gold.Random();
                    h.gold += reward;
                    controller.Notify($"{h.Name} takes {reward} gold from corpse.");
                    h.AddExp(enemy.CalculateExp(h.level), controller);
                    if (enemy.killed == 0)
                        journal.Add($"{turn} - Hero {h.name} killed first {enemy.name}.");
                    enemy.killed++;
                }
                else
                {
                    h.killer = enemy;
                    enemy.kills++;
                    if (watched == h)
                    {
                        if (controlled)
                            pc.Die();
                        else
                            Utils.Ok();
                    }
                }
            }
            else if (e == ExploreEvent.Treasure)
            {
                Treasure t = (Treasure)(Utils.Rand() % (int)Treasure.Max);
                string what;
                if ((t == Treasure.Weapon && (h.weapon == Item.max_item_level || h.weapon == h.dungeon_level))
                    || (t == Treasure.Armor && (h.armor == Item.max_item_level || h.armor == h.dungeon_level))
                    || (t == Treasure.Potions && h.potions == Hero.max_potions && h.hp == h.hpmax))
                    t = Treasure.GoldPile;
                switch (t)
                {
                    default:
                    case Treasure.GoldPile:
                        {
                            int count = Utils.Random(5, 15) + 5 * h.dungeon_level;
                            what = $"{count} gold pile";
                            h.gold += count;
                        }
                        break;
                    case Treasure.Weapon:
                        h.weapon++;
                        what = Item.weapon_names[h.weapon];
                        break;
                    case Treasure.Armor:
                        h.armor++;
                        what = $"{Item.armor_names[h.armor]} armor";
                        break;
                    case Treasure.Potions:
                        h.potions += 2;
                        if (h.potions > Hero.max_potions)
                        {
                            h.hp = Math.Min(h.hpmax, (Utils.Random(2, 5) + h.level / 2 + h.end) * (h.potions - Hero.max_potions));
                            h.potions = Hero.max_potions;
                        }
                        what = "potions";
                        break;
                    case Treasure.GoldTreasure:
                        {
                            int count = Utils.Random(30, 80) + 20 * h.dungeon_level;
                            what = $"{count} gold treasure";
                            h.gold += count;
                        }
                        break;
                }
                controller.Notify($"{h.Name} finds {what}.");
                h.AddExp(5 * h.dungeon_level / h.level, controller);
            }
            else if (e == ExploreEvent.Stairs)
            {
                h.know_down_stairs = true;
                controller.Notify($"{h.Name} finds stairs to lower level.");
                h.AddExp(5 * h.dungeon_level / h.level, controller);
            }
        }

        UnitType GetCombatEnemy(int dungeon_level)
        {
            UnitType[] targets = UnitType.types.Where(x => x.level <= dungeon_level && x.level + 2 >= dungeon_level).ToArray();
            switch (targets.Length)
            {
                case 0:
                    return UnitType.types.Last();
                case 1:
                    return targets[0];
                case 2:
                    return targets[Utils.Rand() % 5 <= 2 ? 0 : 1];
                default:
                    {
                        int r = Utils.Rand() % 7;
                        if (r == 0)
                            return targets[0];
                        else if (r <= 3)
                            return targets[1];
                        else
                            return targets[2];
                    }
            }
        }

        bool Combat(Hero h, IUnitController controller, UnitType enemy)
        {
            bool details = controller.CombatDetails;
            if (details)
                controller.Notify($"{h.Name} was attacked by {enemy.name}.");
            int player_ini = Utils.Random(1, 10) + h.dex,
                enemy_ini = Utils.Random(1, 10) + enemy.ini;
            bool hero_turn = player_ini >= enemy_ini;
            int enemy_hp = enemy.hp;
            while (true)
            {
                if (hero_turn)
                {
                    int attack = Utils.Random(1, 10) + h.dex;
                    if (attack > 5 + enemy.defense)
                    {
                        int dmg = Item.weapon_dmg[h.weapon].Random() + h.str;
                        dmg -= enemy.protection;
                        if (dmg <= 1)
                            dmg = 1;
                        enemy_hp -= dmg;
                        if (enemy_hp <= 0)
                        {
                            if (details)
                                controller.Notify($"{h.Name} attacks {enemy.name} for {dmg} damage and kills him.");
                            h.kills++;
                            return true;
                        }
                        else
                        {
                            if (details)
                                controller.Notify($"{h.Name} attacks {enemy.name} for {dmg} damage.");
                        }
                    }
                    else
                    {
                        if (details)
                            controller.Notify($"{h.Name} tries to attack {enemy.name} but misses.");
                    }
                    hero_turn = false;
                }
                else
                {
                    int attack = Utils.Random(1, 10) + enemy.attack;
                    if (attack > 5 + h.dex)
                    {
                        int dmg = enemy.damage.Random() - h.armor;
                        if (dmg < 1)
                            dmg = 1;
                        h.hp -= dmg;
                        if (h.immortal && h.hp < 1)
                            h.hp = 1;
                        if (h.hp <= 0)
                        {
                            to_remove.Add(h);
                            if (details)
                                controller.Notify($"{enemy.Name} attacks {h.NameMid} for {dmg} damage and kills him.");
                            return false;
                        }
                        else
                        {
                            if (details)
                                controller.Notify($"{enemy.Name} attacks {h.NameMid} for {dmg} damage.");
                        }
                    }
                    else
                    {
                        if (details)
                            controller.Notify($"{enemy.Name} tries to attack {h.NameMid} but misses.");
                    }
                    hero_turn = true;
                }
            }
        }

        public void LevelUp(Hero h)
        {
            if (h.level > max_hero_level)
            {
                journal.Add($"{turn} - Hero {h.name} gained {h.level} level.");
                max_hero_level = h.level;
                chempion = h;
            }
        }

        void Save()
        {
            try
            {
                using (FileStream fs = new FileStream("save", FileMode.Create))
                using (BinaryWriter f = new BinaryWriter(fs))
                {
                    f.Write(save_sign);
                    f.Write(version);
                    f.Write(turn);
                    f.Write(next_id);
                    f.Write(max_hero_level);
                    f.Write(max_dungeon_level);
                    f.Write(journal.Count);
                    foreach (string s in journal)
                        f.Write(s);
                    f.Write(heroes.Count);
                    foreach (Hero h in heroes)
                        h.Save(f);
                    f.Write(watched?.id ?? -1);
                    f.Write(controlled);
                    foreach(UnitType unit in UnitType.types)
                    {
                        f.Write(unit.killed);
                        f.Write(unit.kills);
                    }
                    f.Write(chempion?.id ?? -1);
                    f.Write(file_end_sign);
                }
                Console.WriteLine("Save completed.");
            }
            catch (Exception e)
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
                    max_hero_level = f.ReadInt32();
                    max_dungeon_level = f.ReadInt32();
                    int count = f.ReadInt32();
                    journal.Clear();
                    for (int i = 0; i < count; ++i)
                        journal.Add(f.ReadString());
                    count = f.ReadInt32();
                    heroes.Clear();
                    for (int i = 0; i < count; ++i)
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
                    if (watched != null && controlled)
                        watched.controlled = true;
                    foreach(UnitType unit in UnitType.types)
                    {
                        unit.killed = f.ReadInt32();
                        unit.kills = f.ReadInt32();
                    }
                    int chempion_id = f.ReadInt32();
                    if (chempion_id == -1)
                        chempion = null;
                    else
                        chempion = heroes.Single(x => x.id == chempion_id);
                    byte eof_sign = f.ReadByte();
                    if (eof_sign != file_end_sign)
                        throw new Exception("Missing end of file signature.");
                    first_turn = true;
                    return LoadResult.Ok;
                }
            }
            catch (FileNotFoundException)
            {
                return LoadResult.NoFile;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Load failed: {e}");
                return LoadResult.Failed;
            }
        }
    }
}
