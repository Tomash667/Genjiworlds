using System;

namespace Genjiworlds.Unit
{
    public class AiController : IUnitController
    {
        public bool notify;
        public bool watched;

        public Order Think(Hero h)
        {
            if (h.InsideCity)
            {
                if (h.BuyItems(notify))
                    return Order.Buy;
                else if (h.ShouldRest())
                    return Order.Rest;
                else
                    return Order.GoDown;
            }
            else
            {
                if (h.potions > 0 && h.ShouldDrinkPotion())
                    return Order.UsePotion;
                else if (h.ai_order == AiOrder.ReturnToCity || h.ShouldExitDungeon())
                {
                    h.ai_order = AiOrder.ReturnToCity;
                    return Order.GoUp;
                }
                else if ((h.ai_order == AiOrder.GotoTarget || (h.level > h.dungeon_level && Utils.Rand() % 4 == 0))
                    && (h.dungeon_level != h.lowest_level || h.know_down_stairs))
                    return Order.GoDown;
                else
                    return Order.Explore;
            }
        }

        public void Notify(string str)
        {
            if (notify)
                Console.WriteLine(str);
        }

        public bool CombatDetails { get { return watched; } }
    }
}
