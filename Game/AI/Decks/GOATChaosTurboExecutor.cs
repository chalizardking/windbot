using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Chaos Turbo", "AI_GOAT_ChaosTurbo", "Advanced")]
    public class GOATChaosTurboExecutor : DefaultExecutor
    {
        // Monsters
        public const int BlackLusterSoldier = 72989439;
        public const int Sinister = 71413901;
        public const int ChaosSorcerer = 09596126;
        public const int Dekoichi = 87621407;
        public const int MagicianOfFaith = 24317029;
        public const int Kycoo = 88240808;
        public const int DekoichiOrAirknight = 31560081;
        public const int NightAssailant = 83011277;
        public const int ShiningAngel = 25005816;
        public const int SinisterSerpent = 26202165;
        public const int DDAssailant = 08131171;
        public const int ThunderDragon = 31786629;
        public const int HeavyMech = 14087893;
        public const int UfoBall = 72892473;

        // Spells
        public const int HeavyStorm = 19613556;
        public const int NobleMansOfCrossout = 71044499;
        public const int PotOfGreed = 55144522;
        public const int PrematureBurial = 45986603;
        public const int RaigeikiBreak = 98247752;

        // Traps
        public const int RingOfDestruction = 83555666;
        public const int TorrentialTribute = 53582587;
        public const int TrapDustshoot = 64697231;

        public GOATChaosTurboExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Thunder Dragon to fill grave fast
            AddExecutor(ExecutorType.Activate, ThunderDragon, ActivateThunderDragon);

            // Priority: Chaos monsters
            AddExecutor(ExecutorType.Summon, BlackLusterSoldier);
            AddExecutor(ExecutorType.Summon, ChaosSorcerer, ChaosSummon);

            // Monster effects
            AddExecutor(ExecutorType.Activate, BlackLusterSoldier, BLSEffect);
            AddExecutor(ExecutorType.Activate, ChaosSorcerer, ChaosSorcererEffect);
            AddExecutor(ExecutorType.Activate, MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, Dekoichi);
            AddExecutor(ExecutorType.Activate, ShiningAngel, ShiningAngelEffect);

            // Spells
            AddExecutor(ExecutorType.Activate, PotOfGreed);
            AddExecutor(ExecutorType.Activate, HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, PrematureBurial, DefaultPrematureBurial);
            AddExecutor(ExecutorType.Activate, NobleMansOfCrossout, DefaultBookOfMoon);

            // Traps
            AddExecutor(ExecutorType.Activate, RaigeikiBreak, ActivateRaigekiBreak);
            AddExecutor(ExecutorType.Activate, TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, RingOfDestruction, DefaultRingOfDestruction);
            AddExecutor(ExecutorType.Activate, TrapDustshoot, DefaultTrapDustshoot);

            // General summons
            AddExecutor(ExecutorType.Summon, Kycoo);
            AddExecutor(ExecutorType.Summon, Dekoichi);
            AddExecutor(ExecutorType.Summon, MagicianOfFaith, MagicianSummon);
            AddExecutor(ExecutorType.Summon, NightAssailant);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.MonsterSet, DefaultMonsterSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool ActivateThunderDragon()
        {
            // Discard Thunder Dragon to search 2 more and fill grave
            return true;
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

        private bool ShiningAngelEffect()
        {
            // Search another light fairy from deck
            if (Bot.HasInDeck(ShiningAngel))
            {
                AI.SelectCard(ShiningAngel);
                return true;
            }
            return false;
        }

        private bool ActivateRaigekiBreak()
        {
            // Destroy opponent's card by discarding
            if (Enemy.GetMonsterCount() > 0 || Enemy.GetSpellCount() > 0)
            {
                // Discard Night Assailant or other expendable cards
                AI.SelectCard(NightAssailant, SinisterSerpent, ThunderDragon);
                return true;
            }
            return false;
        }

        private bool MagicianSummon()
        {
            return Bot.HasInGraveyard(PotOfGreed) || Bot.HasInGraveyard(PrematureBurial);
        }
    }
}
