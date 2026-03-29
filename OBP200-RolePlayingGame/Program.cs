using System.Text;

namespace OBP200_RolePlayingGame;


class Program
{
    
    public static Player player = new Player();
    
    static List<Room> Rooms = new List<Room>();
    
    static List<Enemy> EnemyTemplates = new List<Enemy>();
    
    static int CurrentRoomIndex = 0;
    
    static Random Rng = new Random();

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitEnemyTemplates();

        while (true)
        {
            ShowMainMenu();
            Console.Write("Välj: ");
            var choice = (Console.ReadLine() ?? "").Trim();

            if (choice == "1")
            {
                StartNewGame();
                RunGameLoop();
            }
            else if (choice == "2")
            {
                Console.WriteLine("Avslutar...");
                return;
            }
            else
            {
                Console.WriteLine("Ogiltigt val.");
            }

            Console.WriteLine();
        }
    }
    
    static void ShowMainMenu()
    {
        Console.WriteLine("=== Text-RPG ===");
        Console.WriteLine("1. Nytt spel");
        Console.WriteLine("2. Avsluta");
    }

    static void StartNewGame()
    {
        Console.Write("Ange namn: ");
        var name = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) name = "Namnlös";

        Console.WriteLine("Välj klass: 1) Warrior  2) Mage  3) Rogue");
        Console.Write("Val: ");
        var k = (Console.ReadLine() ?? "").Trim();

        string cls = "Warrior";
        int hp = 0, maxhp = 0, atk = 0, def = 0;
        int potions = 0, gold = 0;
        
        switch (k)
        {
            case "1": // Warrior: tankig
                cls = "Warrior";
                maxhp = 40; hp = 40; atk = 7; def = 5; potions = 2; gold = 15;
                break;
            case "2": // Mage: hög damage, låg def
                cls = "Mage";
                maxhp = 28; hp = 28; atk = 10; def = 2; potions = 2; gold = 15;
                break;
            case "3": // Rogue: krit-chans
                cls = "Rogue";
                maxhp = 32; hp = 32; atk = 8; def = 3; potions = 3; gold = 20;
                break;
            default:
                cls = "Warrior";
                maxhp = 40; hp = 40; atk = 7; def = 5; potions = 2; gold = 15;
                break;
        }
        
        player.Name = name;
        player.ClassName = cls;
        player.HP = hp;
        player.MaxHP = maxhp;
        player.Attack = atk;
        player.Defense = def;
        player.Gold = gold;
        player.XP = 0;   // XP
        player.Level = 1;   // LEVEL
        player.Potions = potions;
        player.Inventory = new List<string>
            { "Wooden Sword", "Cloth Armor" }; // inventory som semicolon-separerad sträng
    
        
        Rooms.Clear();
        Rooms.Add(new BattleRoom( "Skogsstig" ));
        Rooms.Add(new TreasureRoom("Gammal kista"));
        Rooms.Add(new ShopRoom("Vandrande köpman"));
        Rooms.Add(new BattleRoom ("Grottans mynning" ));
        Rooms.Add(new RestRoom ("Lägereld" ));
        Rooms.Add(new BattleRoom( "Grottans djup" ));
        Rooms.Add(new BossRoom("Urdraken"));

        CurrentRoomIndex = 0;

        Console.WriteLine($"Välkommen, {name} the {cls}!");
        ShowStatus();
  }  


    static void RunGameLoop()
    {
        while (true)
        {
            var room = Rooms[CurrentRoomIndex];
            Console.WriteLine($"--- Rum {CurrentRoomIndex + 1}/{Rooms.Count}: {room.Label} ---");

            bool resultat = room.Enter();
            
            if (IsPlayerDead())
            {
                Console.WriteLine("Du har stupat... Spelet över.");
                break;
            }
            
            if (!resultat)
            {
                Console.WriteLine("Du lämnar äventyret för nu.");
                break;
            }

            CurrentRoomIndex++;
            
            if (CurrentRoomIndex >= Rooms.Count)
            {
                Console.WriteLine();
                Console.WriteLine("Du har klarat äventyret!");
                break;
            }
            
            Console.WriteLine();
            Console.WriteLine("[C] Fortsätt     [Q] Avsluta till huvudmeny");
            Console.Write("Val: ");
            var post = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (post == "Q")
            {
                Console.WriteLine("Tillbaka till huvudmenyn.");
                break;
            }

            Console.WriteLine();
        }
    }
    

    public static bool DoBattle(bool isBoss)
    {
        Enemy enemy = GenerateEnemy(isBoss);
        Console.WriteLine($"En {enemy.Name} dyker upp! (HP {enemy.HP}, ATK {enemy.Attack}, DEF {enemy.Defense})");

        int enemyHp = enemy.HP;
        int enemyAtk = enemy.Attack;
        int enemyDef = enemy.Defense;

        while (enemyHp > 0 && !IsPlayerDead())
        {
            Console.WriteLine();
            ShowStatus();
            Console.WriteLine($"Fiende: {enemy.Name} HP={enemyHp}");
            Console.WriteLine("[A] Attack   [X] Special   [P] Dryck   [R] Fly");
            if (isBoss) Console.WriteLine("(Du kan inte fly från en boss!)");
            Console.Write("Val: ");

            var cmd = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

            if (cmd == "A")
            {
                int damage = CalculatePlayerDamage(enemyDef);
                enemyHp -= damage;
                Console.WriteLine($"Du slog {enemy.Name} för {damage} skada.");
            }
            else if (cmd == "X")
            {
                int special = UseClassSpecial(enemyDef, isBoss);
                enemyHp -= special;
                Console.WriteLine($"Special! {enemy.Name} tar {special} skada.");
            }
            else if (cmd == "P")
            {
                UsePotion();
            }
            else if (cmd == "R" && !isBoss)
            {
                if (TryRunAway())
                {
                    Console.WriteLine("Du flydde!");
                    return true; // fortsätt äventyr
                }
                else
                {
                    Console.WriteLine("Misslyckad flykt!");
                }
            }
            else
            {
                Console.WriteLine("Du tvekar...");
            }

            if (enemyHp <= 0) break;

            // Fiendens tur
            int enemyDamage = CalculateEnemyDamage(enemyAtk);
            ApplyDamageToPlayer(enemyDamage);
            Console.WriteLine($"{enemy.Name} anfaller och gör {enemyDamage} skada!");
        }

        if (IsPlayerDead())
        {
            return false; // avsluta äventyr
        }

        // Vinstrapporter, XP, guld, loot
        int xpReward = enemy.XPReward;
        int goldReward = enemy.GoldReward;

        AddPlayerXp(xpReward);
        AddPlayerGold(goldReward);

        Console.WriteLine($"Seger! +{xpReward} XP, +{goldReward} guld.");
        MaybeDropLoot(enemy.Name);

        return true;
    }

    static Enemy GenerateEnemy(bool isBoss)
    {
        if (isBoss)
        {
            // Boss-mall
            return new Enemy 
                {Type = "boss", Name ="Urdraken", HP = 55, Attack = 9, Defense = 4,XPReward = 30,GoldReward = 50 };
        }
        else
        {
            // Slumpa bland templates
            var template= EnemyTemplates[Rng.Next(EnemyTemplates.Count)];
            return new Enemy
            {
                // Slmumpmässig justering av stats
                Type = template.Type, Name = template.Name, HP = template.HP + Rng.Next(-1,3), Attack = template.Attack + Rng.Next(0,2),
                Defense = template.Defense+ Rng.Next(0,2), XPReward = template.XPReward + Rng.Next(0,3), GoldReward = template.GoldReward+ Rng.Next(0,3)

            };
        }
    }

    static void InitEnemyTemplates()
    {
        EnemyTemplates.Clear();
        EnemyTemplates.Add(new Enemy
            { Type = "beast", Name = "Vildsvin", HP = 18, Attack = 4, Defense = 1, XPReward = 6, GoldReward = 4 });
        EnemyTemplates.Add(new Enemy
            { Type = "undead", Name = "Skelett", HP = 20, Attack = 5, Defense = 2, XPReward = 7, GoldReward = 5 });
        EnemyTemplates.Add(new Enemy
            { Type = "bandit", Name = "Bandit", HP = 16, Attack = 6, Defense = 1, XPReward = 8, GoldReward = 6 });
        EnemyTemplates.Add(new Enemy
            { Type = "slime", Name = "Geléslem", HP = 14, Attack = 3, Defense = 0, XPReward = 5, GoldReward = 3});
        
    }

    static int CalculatePlayerDamage(int enemyDef)
    {
        int atk = player.Attack;
        string cls = player.ClassName ?? "Warrior";

        
        int baseDmg = Math.Max(1, atk - (enemyDef / 2));
        int roll = Rng.Next(0, 3); // liten variation

        switch (cls.Trim())
        {
            case "Warrior":
                baseDmg += 1; // warrior buff
                break;
            case "Mage":
                baseDmg += 2; // mage buff
                break;
            case "Rogue":
                baseDmg += (Rng.NextDouble() < 0.2) ? 4 : 0; // rogue crit-chans
                break;
            default:
                baseDmg += 0;
                break;
        }

        return Math.Max(1, baseDmg + roll);
    }

    static int UseClassSpecial(int enemyDef, bool vsBoss)
    {
        string cls = player.ClassName ?? "Warrior";
        int specialDmg = 0;

        // Hantering av specialförmågor
        if (cls == "Warrior")
        {
            // Heavy Strike: hög skada men självskada
            Console.WriteLine("Warrior använder Heavy Strike!");
            int atk = player.Attack;
            specialDmg = Math.Max(2, atk + 3 - enemyDef);
            ApplyDamageToPlayer(2); // självskada
        }
        else if (cls == "Mage")
        {
            // Fireball: stor skada, kostar guld
            int gold = player.Gold;
            if (gold >= 3)
            {
                Console.WriteLine("Mage kastar Fireball!");
                player.Gold = gold - 3;
                int atk = player.Attack;
                specialDmg = Math.Max(3, atk + 5 - (enemyDef / 2));
            }
            else
            {
                Console.WriteLine("Inte tillräckligt med guld för att kasta Fireball (kostar 3).");
                specialDmg = 0;
            }
        }
        else if (cls == "Rogue")
        {
            // Backstab: chans att ignorera försvar, hög risk/hög belöning
            if (Rng.NextDouble() < 0.5)
            {
                Console.WriteLine("Rogue utför en lyckad Backstab!");
                int atk = player.Attack;
                specialDmg = Math.Max(4, atk + 6);
            }
            else
            {
                Console.WriteLine("Backstab misslyckades!");
                specialDmg = 1;
            }
        }
        else
        {
            specialDmg = 0;
        }

        // Dämpa skada mot bossen
        if (vsBoss)
        {
            specialDmg = (int)Math.Round(specialDmg * 0.8);
        }

        return Math.Max(0, specialDmg);
    }

    static int CalculateEnemyDamage(int enemyAtk)
    {
        int def = player.Defense;
        int roll = Rng.Next(0, 3);

        int dmg = Math.Max(1, enemyAtk - (def / 2)) + roll;

        // Liten chans till "glancing blow" (minskad skada)
        if (Rng.NextDouble() < 0.1) dmg = Math.Max(1, dmg - 2);

        return dmg;
    }

    static void ApplyDamageToPlayer(int dmg)
    {
        player.HP -= Math.Max(0, dmg);
        if (player.HP < 0) player.HP = 0;
    }

    static void UsePotion()
    {
        int pot = player.Potions;
        if (pot <= 0)
        {
            Console.WriteLine("Du har inga drycker kvar.");
            return;
        }
        

        // Helning av spelaren
        int heal = 12;
        int oldHp = player.HP;
        int newHp = Math.Min(player.MaxHP, oldHp + heal);
       
        player.HP = newHp;
        player.Potions = pot - 1;
        Console.WriteLine($"Du dricker en dryck och återfår {newHp - oldHp} HP.");
    }

    static bool TryRunAway()
    {
        // Flyktschans baserad på karaktärsklass
        string cls = player.ClassName ?? "Warrior";
        double chance = 0.25;
        if (cls == "Rogue") chance = 0.5;
        if (cls == "Mage") chance = 0.35;
        return Rng.NextDouble() < chance;
    }

    static bool IsPlayerDead()
    {
        return player.IsDead;
    }

    static void AddPlayerXp(int amount)
    {
        player.XP += Math.Max(0, amount);
        MaybeLevelUp();
    }

    static void AddPlayerGold(int amount)
    {
        player.Gold += Math.Max(0, amount);
    }

    static void MaybeLevelUp()
    {
        // Nivåtrösklar
        int xp = player.XP;
        int lvl = player.Level;
        int nextThreshold = lvl == 1 ? 10 : (lvl == 2 ? 25 : (lvl == 3 ? 45 : lvl * 20));

        if (xp >= nextThreshold)
        {
            player.Level = lvl + 1;

            // Uppgradering baserad på karaktärsklass
            string cls = player.ClassName ?? "Warrior";
            int maxhp = player.MaxHP;
            int atk = player.Attack;
            int def = player.Defense;

            switch (cls)
            {
                case "Warrior":
                    maxhp += 6; atk += 2; def += 2;
                    break;
                case "Mage":
                    maxhp += 4; atk += 4; def += 1;
                    break;
                case "Rogue":
                    maxhp += 5; atk += 3; def += 1;
                    break;
                default:
                    maxhp += 4; atk += 3; def += 1;
                    break;
            }

            player.MaxHP = maxhp;
            player.Attack = atk;
            player.Defense = def;
            player.HP = maxhp; // full heal vid level up

            Console.WriteLine($"Du når nivå {lvl + 1}! Värden ökade och HP återställd.");
        }
    }

    static void MaybeDropLoot(string enemyName)
    {
        // Enkel loot-regel
        if (Rng.NextDouble() < 0.35)
        {
            string item = "Minor Gem";
            if (enemyName.Contains("Urdraken")) item = "Dragon Scale";
            
            player.Inventory.Add(item);
            Console.WriteLine($"Föremål hittat: {item} (lagt i din väska)");
        }
    }

    // ======= Rumshändelser =======

    public static bool DoTreasure()
    {
        Console.WriteLine("Du hittar en gammal kista...");
        if (Rng.NextDouble() < 0.5)
        {
            int gold = Rng.Next(8, 15);
            AddPlayerGold(gold);
            Console.WriteLine($"Kistan innehåller {gold} guld!");
        }
        else
        {
            var items = new[] { "Iron Dagger", "Oak Staff", "Leather Vest", "Healing Herb" };
            string found = items[Rng.Next(items.Length)];
            player.Inventory.Add(found);
            Console.WriteLine($"Du plockar upp: {found}");
        }
        return true;
    }

    public static bool DoShop()
    {
        Console.WriteLine("En vandrande köpman erbjuder sina varor:");
        while (true)
        {
            Console.WriteLine($"Guld: {player.Gold} | Drycker: {player.Potions}");
            Console.WriteLine("1) Köp dryck (10 guld)");
            Console.WriteLine("2) Köp vapen (+2 ATK) (25 guld)");
            Console.WriteLine("3) Köp rustning (+2 DEF) (25 guld)");
            Console.WriteLine("4) Sälj alla 'Minor Gem' (+5 guld/st)");
            Console.WriteLine("5) Lämna butiken");
            Console.Write("Val: ");
            var val = (Console.ReadLine() ?? "").Trim();

            if (val == "1")
            {
                TryBuy(10, () => player.Potions += 1, "Du köper en dryck.");
            }
            else if (val == "2")
            {
                TryBuy(25, () => player.Attack += 2, "Du köper ett bättre vapen.");
            }
            else if (val == "3")
            {
                TryBuy(25, () => player.Defense += 2, "Du köper bättre rustning.");
            }
            else if (val == "4")
            {
                SellMinorGems();
            }
            else if (val == "5")
            {
                Console.WriteLine("Du säger adjö till köpmannen.");
                break;
            }
            else
            {
                Console.WriteLine("Köpmannen förstår inte ditt val.");
            }
        }
        return true;
    }

    static void TryBuy(int cost, Action apply, string successMsg)
    {
        int gold = player.Gold;
        if (gold >= cost)
        {
            player.Gold = gold - cost;
            apply();
            Console.WriteLine(successMsg);
        }
        else
        {
            Console.WriteLine("Du har inte råd.");
        }
    }

    static void SellMinorGems()
    {
        var inv = player.Inventory;
        if ( inv.Count == 0) 
        {
            Console.WriteLine("Du har inga föremål att sälja.");
            return;
        }
        
        int count = inv.Count(x => x == "Minor Gem");
        if (count == 0)
        {
            Console.WriteLine("Inga 'Minor Gem' i väskan.");
            return;
        }

        player.Inventory= inv.Where(x => x != "Minor Gem").ToList();

        player.Gold += count * 5;
        Console.WriteLine($"Du säljer {count} st Minor Gem för {count * 5} guld.");
    }
    

    // ======= Status =======

    static void ShowStatus()
    {
        Console.WriteLine($"{player.Name} | {player.ClassName}" + $"Hp {player.HP} | {player.MaxHP} |" + $"Atk {player.Attack}" + $"DEF{player.Defense} " + $"lvl {player.Level}" + $"XP {player.XP}" + $"Guld {player.Gold}" + $"Drycker {player.Potions}");
        if ( player.Inventory.Count > 0)
        {
            Console.WriteLine($"Väska: {string.Join(", ", player.Inventory)}");
        }
    }
    
    // ======= Hjälpmetoder =======

    static int ParseInt(string s, int fallback)
    {
        return int.TryParse(s, out int value) ? value : fallback;
    }
    
}
