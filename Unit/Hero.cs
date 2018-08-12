using Genjiworlds;
using System;
using System.Collections.Generic;
using System.IO;

namespace Genjiworlds
{
    public class Hero
    {
        public int id;
        public string name;
        public int level, exp, exp_need;
        public int str, dex, end;
        public int hp, hpmax;
        public int weapon, armor;
        public int gold;
        public int potions;
        public int age, kills;
        public bool inside_city;
        public bool controlled;

        public const int max_potions = 5;
        private static List<string> bought_items = new List<string>();

        public string Name
        {
            get { return controlled ? "You" : name; }
        }
        public string NameMid
        {
            get { return controlled ? "you" : name; }
        }

        public Hero()
        {
        }

        public Hero(int id, bool custom = false)
        {
            this.id = id;
            level = 1;
            exp_need = 100;
            gold = Utils.Random(30, 60);
            if (!custom)
            {
                name = NameGenerator.GetName();
                // start with 1 point in single attribute
                switch (Utils.Rand() % 3)
                {
                    case 0:
                        str = 1;
                        break;
                    case 1:
                        dex = 1;
                        break;
                    case 2:
                        end = 1;
                        break;
                }
                BuyItems(false);
            }
            inside_city = true;
            hp = hpmax = CalculateMaxHp();
        }

        float Hpp { get { return ((float)hp) / hpmax; } }

        public bool ShouldRest()
        {
            if (Hpp <= 0.5f)
                return true;
            else if (hp != hpmax)
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

        public bool ShouldDrinkPotion()
        {
            if (hp <= 5)
                return true;
            else if (hp <= 7)
                return Utils.Rand() % 3 == 0;
            else
                return false;
        }

        public void Save(BinaryWriter f)
        {
            f.Write(id);
            f.Write(name);
            f.Write(level);
            f.Write(exp);
            f.Write(str);
            f.Write(dex);
            f.Write(end);
            f.Write(hp);
            f.Write(weapon);
            f.Write(armor);
            f.Write(gold);
            f.Write(potions);
            f.Write(age);
            f.Write(kills);
            f.Write(inside_city);
        }

        public void Load(BinaryReader f)
        {
            id = f.ReadInt32();
            name = f.ReadString();
            level = f.ReadInt32();
            exp = f.ReadInt32();
            exp_need = 100 * level;
            str = f.ReadInt32();
            dex = f.ReadInt32();
            end = f.ReadInt32();
            hp = f.ReadInt32();
            hpmax = CalculateMaxHp();
            weapon = f.ReadInt32();
            armor = f.ReadInt32();
            gold = f.ReadInt32();
            potions = f.ReadInt32();
            age = f.ReadInt32();
            kills = f.ReadInt32();
            inside_city = f.ReadBoolean();
        }

        public void ShowInfo()
        {
            Console.WriteLine($"{name} [{id}] - inside {(inside_city ? "city" : "dungeon")}, level:{level}, exp:{exp}/{exp_need}\n"
                + $"Hp:{hp}/{hpmax}, str:{str}, dex:{dex}, end:{end}\n"
                + $"Gold:{gold}, weapon:{Item.weapon_names[weapon]}, armor:{Item.armor_names[armor]}, potions:{potions}\n"
                + $"Age:{age}, kills:{kills}");
        }

        public bool BuyItems(bool show)
        {
            if (potions == max_potions && weapon == Item.max_item_level && armor == Item.max_item_level)
                return false;

            bought_items.Clear();

            // first potion if none
            if(potions == 0 && gold >= Item.potion_price && Utils.Rand() % 2 == 0)
            {
                bought_items.Add("potion");
                gold -= Item.potion_price;
            }

            // weapon & armor
            bool check_weapon = Utils.Rand() % 2 == 0;
            for(int i=0; i<2; ++i)
            {
                if (check_weapon)
                {
                    if (weapon < Item.max_item_level)
                    {
                        int price = Item.item_price[weapon];
                        if(gold >= price)
                        {
                            ++weapon;
                            bought_items.Add(Item.weapon_names[weapon]);
                            gold -= price;
                        }
                    }
                }
                else
                {
                    if(armor < Item.max_item_level)
                    {
                        int price = Item.item_price[armor];
                        if(gold >= price)
                        {
                            ++armor;
                            bought_items.Add(Item.armor_names[armor]);
                            gold -= price;
                        }
                    }
                }
                check_weapon = !check_weapon;
            }

            // more potions
            if(potions != max_potions && gold >= Item.potion_price)
            {
                int to_buy = Math.Min(gold / Item.potion_price, 5 - potions);
                if(to_buy > 0)
                {
                    potions += to_buy;
                    gold -= to_buy * Item.potion_price;
                    if (bought_items.Count > 0 && bought_items[0] == "potion")
                    {
                        bought_items.RemoveAt(0);
                        to_buy++;
                    }
                    if(to_buy > 1)
                        bought_items.Add($"{to_buy} potions");
                    else
                        bought_items.Add("potion");
                }
            }

            if (show && bought_items.Count > 0)
                Console.WriteLine($"{name} bought {Utils.FormatList(bought_items)}.");

            return bought_items.Count > 0;
        }

        private int CalculateMaxHp()
        {
            return 9 + level + (end + 1) / 2 * level;
        }

        private void RecalculateHp()
        {
            int prevhp = hpmax;
            hpmax = CalculateMaxHp();
            hp += hpmax - prevhp;
        }

        public void AddExp(int e, bool show)
        {
            exp += e;
            if (exp < exp_need)
                return;
            ++level;
            exp -= exp_need;
            exp_need += 100;
            if (show)
                Console.WriteLine($"{Name} gained {level} level.");
            if (controlled)
                PickAttribute();
            else
            {
                switch (Utils.Rand() % 3)
                {
                    case 0:
                        ++str;
                        break;
                    case 1:
                        ++dex;
                        break;
                    case 2:
                        ++end;
                        break;
                }
            }
            RecalculateHp();
        }

        public void PickAttribute()
        {
            Console.WriteLine("Pick attribute (str, dex, end): ");
            char c = Utils.ReadKey("sde");
            switch(c)
            {
                case 's':
                    ++str;
                    break;
                case 'd':
                    ++dex;
                    break;
                case 'e':
                    ++end;
                    break;
            }
            RecalculateHp();
        }
    }
}
