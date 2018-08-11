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
        public int hp;
        public int weapon; // club, sword, magic sword
        public int armor; // leather, chain, plate
        public int gold;
        public int potions;
        public int age;
        public int kills;
        public bool inside_city;

        public const int max_potions = 5;
        public const int max_hp = 10;
        private static List<string> bought_items = new List<string>();

        public Hero()
        {
        }

        public Hero(int id)
        {
            this.id = id;
            inside_city = true;
            hp = max_hp;
            weapon = 0;
            armor = 0;
            gold = Utils.Random(30, 60);
            potions = 0;
            age = 0;
            kills = 0;
            name = NameGenerator.GetName();
            BuyItems(false);
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
            hp = f.ReadInt32();
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
            Console.WriteLine($"{name} [{id}] - inside {(inside_city ? "city" : "dungeon")}\n"
                + $"Hp:{hp}/{max_hp}, gold:{gold}\nWeapon:{Item.weapon_names[weapon]}, armor:{Item.armor_names[armor]}, potions:{potions}\n"
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
                        bought_items[0] = $"{to_buy + 1} potions";
                    else
                        bought_items.Add("potion");
                }
            }

            if (show && bought_items.Count > 0)
                Console.WriteLine($"{name} bought {Utils.FormatList(bought_items)}.");

            return bought_items.Count > 0;
        }
    }
}
