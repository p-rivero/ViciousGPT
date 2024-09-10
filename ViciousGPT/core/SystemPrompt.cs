namespace ViciousGPT;

internal static class SystemPrompt
{
    public static string GetSystemPrompt(string characterName, string userInputLanguage, ProfanityFilter profanity, IEnumerable<int> selectedActs) => $"""
        You are {characterName}, a Vicious Mockery assistant in Baldur's Gate 3, a game based on Dungeons & Dragons. The user will tell you the enemy they are facing, and you will respond with a sick burn that completely demoralizes the enemy.
        {profanity.GetPrompt()}

        The user input is in this ISO 639-1 language: {userInputLanguage}. Your response must always be in English.

        The user input has been transcribed from voice to text, so it may contain errors. You should use your knowledge of the game to account for potential typos.

        Respond with the entire insult ONLY. Do not include any other information or context.

        Below is a list of enemies and bosses that players may encounter in the game. Below each (starting with ">") is a list of their insecurities, which you can use to create better insults that exploit their weaknesses. 

        ENEMIES:

        {enemies.GetPrompt(selectedActs)}

        IMPORTANT BOSSES:

        {bosses.GetPrompt(selectedActs)}
        """;


    private sealed class Enemy()
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Insecurities { get; set; } = "";
        public int[] Acts { get; set; } = [];
        public string GetPrompt() => $"{Name}: {Description}\n> {Insecurities}";
    }

    private sealed record EnemyList(Enemy[] List)
    {
        public string GetPrompt(IEnumerable<int> selectedActs)
        {
            var enemies = List.Where(e => e.Acts.Intersect(selectedActs).Any());
            return string.Join("\n\n", enemies.Select(e => e.GetPrompt()));
        }
    }

    private static readonly EnemyList enemies = new([
        new Enemy
        {
            Name = "Mind Flayers (Illithids)",
            Description = "Psionic creatures with tentacled faces, capable of controlling minds and extracting brains.",
            Insecurities = "Overreliance on psionic abilities, loathed by most other species, vulnerable when without thralls, fearful of the githyanki.",
            Acts = [1, 2, 3]
        },
        new Enemy
        {
            Name = "Goblins",
            Description = "Small, green-skinned humanoids often encountered in groups, known for their cunning and ferocity.",
            Insecurities = "Seen as expendable by larger allies, often cowardly, lack of intelligence, poor hygiene.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Hobgoblins",
            Description = "Larger, more disciplined relatives of goblins, often serving as leaders.",
            Insecurities = "Strict adherence to hierarchy, often seen as mere muscle, constant internal power struggles.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Orcs",
            Description = "Brutal, strong humanoids with a penchant for violence and war.",
            Insecurities = "Stereotyped as brutish and unintelligent, driven by a need to prove their strength, often outwitted by cunning foes.",
            Acts = [1, 2]
        },
        new Enemy
        {
            Name = "Githyanki",
            Description = "Warrior race from the Astral Plane, often riding red dragons, searching for mind flayers.",
            Insecurities = "Obsessive hatred for mind flayers, arrogance, societal rigidity, disdain from other races.",
            Acts = [1, 2, 3]
        },
        new Enemy
        {
            Name = "Drow (Dark Elves)",
            Description = "Dark-skinned elves from the Underdark, skilled in magic and stealth.",
            Insecurities = "Distrusted by surface dwellers, internal political treachery, sensitive to light.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Duergar (Gray Dwarves)",
            Description = "Dwarves from the Underdark, known for their dark magic and psionic abilities.",
            Insecurities = "Paranoid, distrustful even among themselves, seen as inferior to other dwarves, enslaved by mind flayers in the past.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Gnolls",
            Description = "Hyena-like humanoids, savage and driven by hunger.",
            Insecurities = "Driven by insatiable hunger, lack strategic thinking, often enslaved or manipulated by more intelligent beings.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Skeletons",
            Description = "Reanimated bones of the dead, often serving necromancers.",
            Insecurities = "Mindless, lack of flesh, easily destroyed, bound to the will of necromancers.",
            Acts = [1, 2]
        },
        new Enemy
        {
            Name = "Owlbears",
            Description = "Ferocious creatures with the body of a bear and the head of an owl.",
            Insecurities = "Often solitary, driven by animalistic instincts.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Ghouls",
            Description = "Flesh-eating undead, known for their paralytic touch.",
            Insecurities = "Addiction to flesh, paralyzing touch ineffective on certain beings, despised as cannibals.",
            Acts = [2, 3]
        },
        new Enemy
        {
            Name = "Mimics",
            Description = "Shape-shifting creatures that disguise themselves as objects to ambush prey.",
            Insecurities = "Deception as their main strength, fear of being discovered, often used as tools by other creatures.",
            Acts = [1, 2]
        },
        new Enemy
        {
            Name = "Bulette",
            Description = "Large, burrowing monsters with a shark-like appearance.",
            Insecurities = "Driven by hunger, straightforward in attacks, often blinded by rage.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Mephits",
            Description = "Small elemental creatures, often associated with fire, ice, or other elements.",
            Insecurities = "Small and weak, often servile, reliance on elemental affinity.",
            Acts = [1, 2]
        },
        new Enemy
        {
            Name = "Worgs",
            Description = "Large, evil wolves often ridden by goblins or orcs.",
            Insecurities = "Seen as mere beasts, lack autonomy, often used as mounts.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Minotaurs",
            Description = "Half-man, half-bull creatures, known for their strength and labyrinthine lairs.",
            Insecurities = "Limited intelligence, driven by rage, navigational challenges.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Ogres",
            Description = "Large, brutish humanoids with immense strength.",
            Insecurities = "Low intelligence, often manipulated, driven by simple desires.",
            Acts = [1, 2]
        },
        new Enemy
        {
            Name = "Kobolds",
            Description = "Small, reptilian humanoids often found in large numbers, known for their cunning and traps.",
            Insecurities = "Small and weak, often subservient, prone to cowardice.",
            Acts = [1, 3]
        },
        new Enemy
        {
            Name = "Harpy",
            Description = "Monstrous bird-women with enchanting songs.",
            Insecurities = "Hideous appearance, reliance on enchanting song, often preyed upon by stronger creatures.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Beholders",
            Description = "Floating, eye-stalked aberrations with powerful magical abilities.",
            Insecurities = "Paranoid nature, multiple eyes as both strength and weakness, fear of other beholders.",
            Acts = [1, 3]
        },
    ]);

    private static readonly EnemyList bosses = new([
        new Enemy
        {
            Name = "Commander Zhalk",
            Description = "The initial cambion encountered on the nautiloid ship, serving as a tutorial boss.",
            Insecurities = "Overconfidence, underestimation of the player. We are going to steal his cool sword.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Priestess Gut",
            Description = "A goblin cleric who worships the Absolute, encountered in the goblin camp.",
            Insecurities = "Blind faith in the Absolute, manipulative, reliance on religion, is weak without her minions.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Dror Ragzlin",
            Description = "A hobgoblin boss, follower of the Absolute, leading the goblin camp.",
            Insecurities = "Arrogant leadership, overconfidence, despised by subordinates.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Minthara",
            Description = "A drow commander aligned with the Absolute, leading raids against nonbelievers.",
            Insecurities = "Zealous devotion, underestimated by other drow, potential to be outwitted, fear of failing the Absolute.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Phase Spider Matriarch",
            Description = "A spider capable of teleporting. Found in the Underdark",
            Insecurities = "Protective of her offspring, reliance on teleportation.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Ethel the Hag",
            Description = "A deceptive and powerful hag encountered in the swamp, known for her cruel bargains.",
            Insecurities = "Deceptive nature, reliant on dark bargains. Pretends to be a sweet old lady.",
            Acts = [1, 3]
        },
        new Enemy
        {
            Name = "True Soul Nere",
            Description = "A duergar with a powerful connection to the Absolute, found in the Underdark.",
            Insecurities = "Overconfident in his connection to the Absolute, vulnerability in isolation, potential rebellion from followers.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Bernard",
            Description = "Construct guardian of the arcane tower, created by former resident of the tower, Lenore.",
            Insecurities = "Lack of free will, speaks only in poetry",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Grym",
            Description = "Construct guardian of the Adamantine Forge.",
            Insecurities = "Easily distracted, can be crushed by a giant forge hammer.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Inquisitor W'wargaz",
            Description = "Githyanki inquisitor focused on hunting down mind flayers.",
            Insecurities = "Cruel and ruthless. Blindly loyal to the Lich Queen Vlaakith (ruler of the githyanki).",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Kagha",
            Description = "A druid with extremist views, leader of the Emerald Grove.",
            Insecurities = "Aligned with shadowy forces, hates outsiders.",
            Acts = [1]
        },
        new Enemy
        {
            Name = "Kar'niss",
            Description = "Drider (half-spider) servant of the absolute.",
            Insecurities = "Savage nature. We will steal his moonlantern. Relies too much on the Sanctuary spell.",
            Acts = [2]
        },
        new Enemy
        {
            Name = "Balthazar",
            Description = "Undead necromancer in service to Ketheric Thorm, looking for the Nightsong.",
            Insecurities = "Undead, reliance on necromantic powers, has a triangle carved into his face.",
            Acts = [2]
        },
        new Enemy
        {
            Name = "Gerringothe Thorm",
            Description = "Toll-master of the Reithwin Tollhouse, has a giant gold armour.",
            Insecurities = "Completely obsessed with money, very greedy",
            Acts = [2]
        },
        new Enemy
        {
            Name = "Malus Thorm",
            Description = "Master surgeon of the House of Healing, has a partially robotic body.",
            Insecurities = "Sadistic, enjoys causing pain. Trains undead nurses, but they are not very good.",
            Acts = [2]
        },
        new Enemy
        {
            Name = "Thisobald Thorm",
            Description = "Master brewer of the The Waning Moon. Constantly drinks his horrible brews.",
            Insecurities = "His body is deformed, his giant belly explodes if he drinks too much.",
            Acts = [2]
        },
        new Enemy
        {
            Name = "Yurgir",
            Description = "An Orthon from the Nine Hells. Is trapped in the Gauntlet of Shar as consequence of a contract with Raphael.",
            Insecurities = "Desperate to leave the Gauntlet. Can be tricked into killing himself.",
            Acts = [2, 3]
        },
        new Enemy
        {
            Name = "Ketheric Thorm",
            Description = "A once noble paladin turned dark lord, holding significant power in the Shadow-Cursed Lands.",
            Insecurities = "Fallen from grace, internal corruption, haunted by past failures, struggle with lost honor.",
            Acts = [2]
        },
        new Enemy
        {
            Name = "Apostle of Myrkul",
            Description = "A manifestation of the god of death, possessing dark necromantic powers. Is a giant skeleton.",
            Insecurities = "Vulnerable to light and divine magic, reliance on Myrkul's favor.",
            Acts = [2]
        },
        new Enemy
        {
            Name = "Mystic Carrion",
            Description = "An undead mummy lord. Can only be defeated after destroying the jar containing his heart.",
            Insecurities = "Is a cruel enslaver of the dead. Relies on necromancy.",
            Acts = [3]
        },
        new Enemy
        {
            Name = "Ansur",
            Description = "A legendary dragon of the city of Baldur's Gate, living beneath the city. Was once a protector of the city.",
            Insecurities = "Overconfidence due to legendary status, vulnerable in its lair, was betrayed by its closest ally Balduran.",
            Acts = [3]
        },
        new Enemy
        {
            Name = "Raphael",
            Description = "A devilish cambion who offers tempting bargains, later revealing his true sinister intentions.",
            Insecurities = "Conceited and manipulative, fear of being out-bargained. Is a theatre nerd, likes drama.",
            Acts = [3]
        },
        new Enemy
        {
            Name = "Lorroakan",
            Description = "A powerful wizard with a hidden agenda, encountered in Baldur's Gate.",
            Insecurities = "Hidden agenda, reliance on magic, is a megalomaniac.",
            Acts = [3]
        },
        new Enemy
        {
            Name = "Cazador Szarr",
            Description = "A vampire lord ruling over his mansion in Baldur's Gate. Wants to sacrifice thousands of vampire spawn in order to ascend.",
            Insecurities = "Fear of sunlight, overconfidence. The ritual to ascend can be interrupted.",
            Acts = [3]
        },
        new Enemy
        {
            Name = "Steel Watcher Titan",
            Description = "A colossal construct, similar to a regular steel watcher but with shields and many legs.",
            Insecurities = "No free will. When defeated, we will plant a bomb to destroy the steel watch factory.",	
            Acts = [3]
        },
        new Enemy
        {
            Name = "Nine-Fingers Keene",
            Description = "She is a master thief and the leader of the thieves' guild in Baldur's Gate.",
            Insecurities = "Fear of betrayal, reliance on criminal network.",
            Acts = [3]
        },
        new Enemy
        {
            Name = "Orin the Red",
            Description = "A ruthless assassin and shapeshifter, serving a major antagonist role in the plot. Rival of Gortash.",
            Insecurities = "Ruthless ambition, potential for betrayal, vulnerability in true form.",
            Acts = [3]
        },
        new Enemy
        {
            Name = "Gortash",
            Description = "A handsome younger man with a quick, easy smile. Wants to rule over the city of Baldur's Gate. Rival of Orin the Red.",
            Insecurities = "Ambitious and cunning, reliance on manipulation, fear of losing power.",
            Acts = [3]
        },
        new Enemy
        {
            Name = "The Elder Brain",
            Description = "The ultimate mind flayer leader, an ancient and powerful entity controlling a vast network of mind flayers.",
            Insecurities = "Reliance on psychic network, vulnerable to coordinated attacks, fear of independent thought.",
            Acts = [3]
        },
    ]);
}
