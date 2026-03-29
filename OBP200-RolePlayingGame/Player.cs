namespace OBP200_RolePlayingGame;

public class Player
{
    public string Name { get; set;}
    public string ClassName { get; set;}
    public int HP { get; set;}
    public int MaxHP { get; set;}
    public int Attack { get; set;}
    public int Defense { get; set;}
    public int Gold { get; set;}
    public int XP { get; set;}
    public int Level { get; set;}
    public int Potions { get; set; }
    public List<string> Inventory { get; set; } = new List<string>();
    
    public bool IsDead
    {
        get { return HP <= 0; }
    }
    
}