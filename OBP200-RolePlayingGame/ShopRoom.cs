namespace OBP200_RolePlayingGame;

public class ShopRoom : Room
{
    public ShopRoom(string label) : base(label) { }

    public override bool Enter()
    {
        return Program.DoShop();
    }
}