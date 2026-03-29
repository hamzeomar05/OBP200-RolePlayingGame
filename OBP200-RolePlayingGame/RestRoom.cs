namespace OBP200_RolePlayingGame;

public class RestRoom : Room
{
    public RestRoom(string label) : base(label) {}

    public override bool Enter()
    {
        Console.WriteLine("Du slår läger och vilar.");
        Program.player.HP = Program.player.MaxHP;
        Console.WriteLine("HP återställs till max.");
        return true;
    }
}