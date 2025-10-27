using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Chaos Control", "AI_GOAT_ChaosControl", "Advanced")]
    public class GOATChaosControlExecutor : DefaultExecutor
    {
        // Monsters
        public const int AsuraPriest = 02134346;
        public const int BlackLusterSoldier = 72989439;
        public const int Sinister = 71413901;
        public const int ChaosSorcerer = 09596126;
        public const int DDWarriorLady = 07572887;
        public const int Airknight = 74713516;
        public const int MagicalMerchant = 32362575;
        public const int Dekoichi = 31560081;
        public const int SinisterSerpent = 26202165;
        public const int DDAssailant = 08131171;
        public const int GoatToken = 73414375;
        public const int Tsukuyomi = 34100279;

        // Spells
        public const int DustTornado = 14087893;
        public const int HeavyStorm = 19613556;
        public const int Metamorphosis = 46411259;
        public const int MysticalSpaceTyphoon = 05318639;
        public const int NobleMansOfCrossout = 71044499;
        public const int PotOfGreed = 55144522;
        public const int Scapegoat = 73915051;
        public const int PrematureBurial = 45986603;
        public const int SnatchSteal = 44095762;
        public const int BrainControl = 60082869;

        // Traps
        public const int RingOfDestruction = 83555666;
        public const int SakuretsuArmor = 56120475;
        public const int TorrentialTribute = 53582587;

        public GOATChaosControlExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Priority: Chaos monsters
            AddExecutor(ExecutorType.Summon, BlackLusterSoldier);
            AddExecutor(ExecutorType.Summon, ChaosSorcerer, ChaosSummon);
            AddExecutor(ExecutorType.Summon, AsuraPriest, AsuraSummon);
            AddExecutor(ExecutorType.Summon, DDWarriorLady);
            AddExecutor(ExecutorType.Summon, Airknight);

            // Monster effects
            AddExecutor(ExecutorType.Activate, BlackLusterSoldier, BLSEffect);
            AddExecutor(ExecutorType.Activate, ChaosSorcerer, ChaosSorcererEffect);
            AddExecutor(ExecutorType.Activate, MagicalMerchant);
            AddExecutor(ExecutorType.Activate, Tsukuyomi);
            AddExecutor(ExecutorType.Activate, AsuraPriest);

            // Key combo pieces
            AddExecutor(ExecutorType.Activate, Scapegoat, DefaultScapegoat);
            AddExecutor(ExecutorType.Activate, Metamorphosis, ActivateMetamorphosis);

            // Spells
            AddExecutor(ExecutorType.Activate, PotOfGreed);
            AddExecutor(ExecutorType.Activate, HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, SnatchSteal, DefaultSnatchSteal);
            AddExecutor(ExecutorType.Activate, BrainControl, DefaultBrainControl);
            AddExecutor(ExecutorType.Activate, PrematureBurial, DefaultPrematureBurial);
            AddExecutor(ExecutorType.Activate, NobleMansOfCrossout, DefaultBookOfMoon);

            // Traps
            AddExecutor(ExecutorType.Activate, TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, RingOfDestruction, DefaultRingOfDestruction);
            AddExecutor(ExecutorType.Activate, SakuretsuArmor, DefaultSakuretsuArmor);
            AddExecutor(ExecutorType.Activate, DustTornado, DefaultMysticalSpaceTyphoon);

            // General summons
            AddExecutor(ExecutorType.Summon, Tsukuyomi);
            AddExecutor(ExecutorType.Summon, Dekoichi);
            AddExecutor(ExecutorType.Summon, MagicalMerchant);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.MonsterSet, DefaultMonsterSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool ChaosSummon()
        {
            // Can summon Chaos Sorcerer if we have light and dark in grave
            int lightCount = Bot.Graveyard.Count(c => c.HasAttribute(CardAttribute.Light));
            int darkCount = Bot.Graveyard.Count(c => c.HasAttribute(CardAttribute.Dark));
            return lightCount > 0 && darkCount > 0;
        }

        private bool BLSEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                // Remove opponent's monster or attack twice
                if (Enemy.GetMonsterCount() > 0 && !Card.IsDisabled())
                    return true;
            }
            return false;
        }

        private bool ChaosSorcererEffect()
        {
            // Banish opponent's face-up monster
            return Enemy.GetMonsters().Any(c => c.IsFaceup());
        }

        private bool AsuraSummon()
        {
            // Summon Asura to attack directly multiple times
            return Enemy.GetMonsterCount() == 0 && Bot.HasInMonstersZone(GoatToken);
        }

        private bool ActivateMetamorphosis()
        {
            // Use on Goat Tokens to summon Thousand-Eyes Restrict
            if (Bot.HasInMonstersZone(GoatToken))
                return true;
            // Or use on level 1 monsters
            return Bot.GetMonsters().Any(c => c.Level == 1);
        }
    }
}
