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

    public interface IUnitController
    {
        Order Think(Hero h);
        void Notify(string str);
        bool CombatDetails { get; }
    }
}
