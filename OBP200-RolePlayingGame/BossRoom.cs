namespace OBP200_RolePlayingGame;

public class BossRoom : Room
{
    public BossRoom(string label) : base(label) { }

    public override bool Enter()
    {
        return Program.DoBattle(true);
    }
}