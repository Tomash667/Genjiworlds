﻿using Genjiworlds.Stats;
using System;

namespace Genjiworlds.Unit
{
    class PlayerController : IUnitController
    {
        private Hero h;
        private int last_msg = 0;
        const int msg_count = 20;
        string[] msgs = new string[msg_count];

        public void Clear()
        {
            last_msg = 0;
        }

        public Order Think(Hero h)
        {
            this.h = h;
            if (h.gained_level)
            {
                h.gained_level = false;
                Console.Clear();
                WriteHeader();
                WriteMessages();
                h.PickAttribute();
            }
            Order order;
            if (h.InsideCity)
                order = HandleCity();
            else
                order = HandleDungeon();
            last_msg = 0;
            return order;
        }

        public void Notify(string str)
        {
            if (last_msg == msg_count)
            {
                for (int i = 0; i < msg_count - 1; ++i)
                    msgs[i] = msgs[i + 1];
                msgs[msg_count - 1] = str;
            }
            else
            {
                msgs[last_msg] = str;
                ++last_msg;
            }
        }

        public bool CombatDetails { get { return true; } }

        public void Die()
        {
            Console.Clear();
            WriteHeader();
            WriteMessages();
            Console.WriteLine("You died...");
            Utils.Ok();
        }

        private void WriteHeader()
        {
            Console.WriteLine($"{h.name} (level:{h.level}, exp:{h.exp}/{h.exp_need}, hp:{h.hp}/{h.hpmax})");
        }

        private void WriteMessages()
        {
            for (int i = 0; i < last_msg; ++i)
            {
                if (msgs[i] == null)
                    break;
                Console.WriteLine(msgs[i]);
            }
        }

        private Order HandleCity()
        {
            while (true)
            {
                Console.Clear();
                WriteHeader();
                Console.WriteLine($"You are inside city, gold:{h.gold}. (b-buy, r-rest, v-view, g-go to dungeon, C-use command)");
                WriteMessages();
                Console.Write(">");
                char c = Utils.ReadKey("brvgXC");
                switch (c)
                {
                    case 'b':
                        if (HandleShop())
                            return Order.Buy;
                        break;
                    case 'r':
                        return Order.Rest;
                    case 'g':
                        return Order.GoDown;
                    case 'v':
                        h.ShowInfo();
                        Utils.Ok();
                        break;
                    case 'C':
                        Console.Write("\nCommand: ");
                        if (Game.instance.ParseCommand())
                            return Order.None;
                        break;
                }
            }
        }

        private bool HandleShop()
        {
            bool bought = false;
            while (true)
            {
                Console.Clear();
                WriteHeader();
                Console.WriteLine($"You are inside shop, gold:{h.gold}, potions:{h.potions}.");
                Console.WriteLine("What to buy?");
                if (h.weapon != Item.max_item_level)
                    Console.WriteLine($"w - {Item.weapon_names[h.weapon + 1]} ({Item.item_price[h.weapon]} gold)");
                if (h.armor != Item.max_item_level)
                    Console.WriteLine($"a - {Item.armor_names[h.armor + 1]} ({Item.item_price[h.armor]} gold)");
                if (h.bow != Item.max_item_level)
                    Console.WriteLine($"b - {Item.bow_names[h.bow + 1]} ({Item.item_price[h.bow]} gold)");
                if (h.potions != Hero.max_potions)
                    Console.WriteLine($"p - potion ({Item.potion_price} gold)");
                Console.WriteLine("x - exit shop");
                char c = Utils.ReadKey("wabpx");
                switch (c)
                {
                    case 'w':
                        if (h.weapon != Item.max_item_level)
                        {
                            int price = Item.item_price[h.weapon];
                            if (h.gold >= price)
                            {
                                bought = true;
                                h.gold -= price;
                                ++h.weapon;
                                Console.WriteLine($"You bought {Item.weapon_names[h.weapon]}.");
                                Utils.Ok();
                            }
                            else
                            {
                                Console.WriteLine("Not enought gold.");
                                Utils.Ok();
                            }
                        }
                        break;
                    case 'a':
                        if (h.armor != Item.max_item_level)
                        {
                            int price = Item.item_price[h.armor];
                            if (h.gold >= price)
                            {
                                bought = true;
                                h.gold -= price;
                                ++h.armor;
                                Console.WriteLine($"You bought {Item.armor_names[h.armor]}.");
                                Utils.Ok();
                            }
                            else
                            {
                                Console.WriteLine("Not enought gold.");
                                Utils.Ok();
                            }
                        }
                        break;
                    case 'b':
                        if (h.bow != Item.max_item_level)
                        {
                            int price = Item.item_price[h.bow];
                            if (h.gold >= price)
                            {
                                bought = true;
                                h.gold -= price;
                                ++h.bow;
                                Console.WriteLine($"You bought {Item.bow_names[h.bow]}.");
                                Utils.Ok();
                            }
                            else
                            {
                                Console.WriteLine("Not enought gold.");
                                Utils.Ok();
                            }
                        }
                        break;
                    case 'p':
                        if (h.potions != Hero.max_potions)
                        {
                            Console.WriteLine("How many: ");
                            string str = Console.ReadLine();
                            if (int.TryParse(str, out int count) && count > 0)
                            {
                                count = Utils.Min(Hero.max_potions - h.potions, h.gold / Item.potion_price, count);
                                if (count == 0)
                                {
                                    Console.WriteLine("Not enought gold.");
                                    Utils.Ok();
                                }
                                else
                                {
                                    bought = true;
                                    h.gold -= count * Item.potion_price;
                                    h.potions += count;
                                    if (count == 1)
                                        Console.WriteLine("You bought potion.");
                                    else
                                        Console.WriteLine($"You bought {count} potions.");
                                    Utils.Ok();
                                }
                            }
                        }
                        break;
                    case 'x':
                        return bought;
                }
            }
        }

        private Order HandleDungeon()
        {
            while (true)
            {
                Console.Clear();
                WriteHeader();
                string go;
                if (h.InsideCity)
                    go = "u-go to city";
                else
                    go = "u-go upper level";
                if (h.dungeon_level != h.lowest_level || h.know_down_stairs)
                    go += ", d-go lower level";
                Console.WriteLine($"You are inside dungeon {h.dungeon_level}, potions {h.potions}. (e-explore, {go}, p-use potion, v-view, C-use command)");
                WriteMessages();
                Console.Write(">");
                char c = Utils.ReadKey("eudpC");
                switch (c)
                {
                    case 'e':
                        return Order.Explore;
                    case 'u':
                        return Order.GoUp;
                    case 'd':
                        if (h.dungeon_level != h.lowest_level || h.know_down_stairs)
                            return Order.GoDown;
                        break;
                    case 'p':
                        if (h.potions > 0)
                            return Order.UsePotion;
                        else
                        {
                            Console.WriteLine("You don't have any potions.");
                            Utils.Ok();
                        }
                        break;
                    case 'v':
                        h.ShowInfo();
                        Utils.Ok();
                        break;
                    case 'C':
                        Console.Write("\nCommand: ");
                        if (Game.instance.ParseCommand())
                            return Order.None;
                        break;
                }
            }
        }
    }
}
