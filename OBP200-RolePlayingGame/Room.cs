namespace OBP200_RolePlayingGame;

public abstract class Room : IRoomAction
{
    public string Label { get; set; }

    protected Room(string label)
    {
        Label = label;
    }

    public abstract bool Enter();
}