namespace Genjiworlds
{
    public enum Order
    {
        None,
        Buy,
        Rest,
        GotoDungeon,
        Explore,
        GotoCity,
        UsePotion
    }

    interface IUnitController
    {
        Order Think(Hero h);
    }
}
