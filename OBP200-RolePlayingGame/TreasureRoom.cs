namespace OBP200_RolePlayingGame;

public class TreasureRoom : Room
{
    public TreasureRoom(string label) : base(label) { }

    public override bool Enter()
    {
        return Program.DoTreasure();
    }
}