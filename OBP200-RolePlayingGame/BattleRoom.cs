namespace OBP200_RolePlayingGame;

public class BattleRoom : Room
{
    public BattleRoom(string label) : base(label)
    {
    }

    public override bool Enter()
    {
        return Program.DoBattle(false);
    }
}