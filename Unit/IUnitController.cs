namespace Genjiworlds
{
    public enum Order
    {
        None,
        Buy,
        Rest,
        GoDown,
        GoUp,
        Explore,
        UsePotion
    }

    public interface IUnitController
    {
        Order Think(Hero h);
        void Notify(string str);
        bool CombatDetails { get; }
    }
}
