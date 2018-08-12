using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genjiworlds.Unit
{
    public class AiController : IUnitController
    {
        public bool notify;

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
    }
}
