using System;

namespace Genjiworlds.Unit
{
    class PlayerController : IUnitController
    {
        public Game game;
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
            if(h.gained_level)
            {
                h.gained_level = false;
                Console.Clear();
                WriteHeader();
                WriteMessages();
                h.PickAttribute();
            }
            Order order;
            if (h.inside_city)
                order = HandleCity();
            else
                order = HandleDungeon();
            last_msg = 0;
            return order;
        }

        public void Notify(string str)
        {
            if(last_msg == msg_count)
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
                        return Order.GotoDungeon;
                    case 'v':
                        h.ShowInfo();
                        Utils.Ok();
                        break;
                    case 'C':
                        Console.Write("\nCommand: ");
                        if (game.ParseCommand())
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
                if (h.potions != Hero.max_potions)
                    Console.WriteLine($"p - potion ({Item.potion_price} gold)");
                Console.WriteLine("x - exit shop");
                char c = Utils.ReadKey("wapx");
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
                                    if(count == 1)
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
            while(true)
            {
                Console.Clear();
                WriteHeader();
                Console.WriteLine($"You are inside dungeon, potions {h.potions}. (e-explore, g-go to city, p-use potion, v-view, C-use command)");
                WriteMessages();
                Console.Write(">");
                char c = Utils.ReadKey("egpC");
                switch(c)
                {
                    case 'e':
                        return Order.Explore;
                    case 'g':
                        return Order.GotoCity;
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
                        if (game.ParseCommand())
                            return Order.None;
                        break;
                }
            }
        }
    }
}
