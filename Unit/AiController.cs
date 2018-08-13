using System;

namespace Genjiworlds.Unit
{
    public class AiController : IUnitController
    {
        public bool notify;
        public bool watched;

        public Order Think(Hero h)
        {
            if (h.inside_city)
            {
                if (h.BuyItems(notify))
                    return Order.Buy;
                else if (h.ShouldRest())
                    return Order.Rest;
                else
                    return Order.GotoCity;
            }
            else
            {
                if (h.potions > 0 && h.ShouldDrinkPotion())
                    return Order.UsePotion;
                else if (h.ShouldExitDungeon())
                    return Order.GotoCity;
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
