using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Gravekeeper", "AI_GOAT_Gravekeeper", "Advanced")]
    public class GOATGravekeeperExecutor : DefaultExecutor
    {
        // Monsters
        public const int BlackLusterSoldier = 71413901;
        public const int GravekeepersAssailant = 25262697;
        public const int GravekeepersGuard = 37101832;
        public const int GravekeepersSpy = 63695531;
        public const int MagicianOfFaith = 24317029;
        public const int Tsukuyomi = 34100279;
        public const int Exiled = 14087893;

        // Spells
        public const int HeavyStorm = 19613556;
        public const int MysticalSpaceTyphoon = 05318639;
        public const int Necrovalley = 47355498;
        public const int NobleMansOfCrossout = 71044499;
        public const int PotOfGreed = 55144522;
        public const int Scapegoat = 73915051;
        public const int SnatchSteal = 44095762;
        public const int PrematureBurial = 45986603;
        public const int GracefulCharity = 79571449;
        public const int RoyalTribute = 94192409;

        // Traps
        public const int CallOfTheHaunted = 41420027;
        public const int RingOfDestruction = 83555666;
        public const int SakuretsuArmor = 60082869;
        public const int TorrentialTribute = 53582587;
        public const int WabokuOrThreatening = 30450531;

        public GOATGravekeeperExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Priority: Activate Necrovalley ASAP
            AddExecutor(ExecutorType.Activate, Necrovalley);
            AddExecutor(ExecutorType.Activate, RoyalTribute, ActivateRoyalTribute);

            // Priority monsters
            AddExecutor(ExecutorType.Summon, BlackLusterSoldier);
            AddExecutor(ExecutorType.Summon, GravekeepersAssailant, SummonAssailant);
            AddExecutor(ExecutorType.Summon, GravekeepersSpy);
            AddExecutor(ExecutorType.Summon, MagicianOfFaith, MagicianSummon);

            // Monster effects
            AddExecutor(ExecutorType.Activate, BlackLusterSoldier, BLSEffect);
            AddExecutor(ExecutorType.Activate, GravekeepersSpy, SpyEffect);
            AddExecutor(ExecutorType.Activate, MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, Tsukuyomi);

            // Spells
            AddExecutor(ExecutorType.Activate, PotOfGreed);
            AddExecutor(ExecutorType.Activate, GracefulCharity, DefaultGracefulCharity);
            AddExecutor(ExecutorType.Activate, HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, SnatchSteal, DefaultSnatchSteal);
            AddExecutor(ExecutorType.Activate, PrematureBurial, DefaultPrematureBurial);
            AddExecutor(ExecutorType.Activate, NobleMansOfCrossout, DefaultBookOfMoon);
            AddExecutor(ExecutorType.Activate, Scapegoat, DefaultScapegoat);

            // Traps
            AddExecutor(ExecutorType.Activate, TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, CallOfTheHaunted, DefaultCallOfTheHaunted);
            AddExecutor(ExecutorType.Activate, RingOfDestruction, DefaultRingOfDestruction);
            AddExecutor(ExecutorType.Activate, SakuretsuArmor, DefaultSakuretsuArmor);

            // General summons
            AddExecutor(ExecutorType.Summon, Tsukuyomi);
            AddExecutor(ExecutorType.Summon, GravekeepersGuard);
            AddExecutor(ExecutorType.MonsterSet, GravekeepersSpy);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool ActivateRoyalTribute()
        {
            // Activate Royal Tribute only if Necrovalley is active and opponent has cards
            return Bot.HasInSpellZone(Necrovalley) && Enemy.Hand.Count > 0;
        }

        private bool SummonAssailant()
        {
            // Summon Assailant if Necrovalley is active for ATK boost
            return Bot.HasInSpellZone(Necrovalley);
        }

        private bool SpyEffect()
        {
            // Spy effect to search another Gravekeeper
            if (Bot.HasInDeck(GravekeepersAssailant) && Bot.HasInSpellZone(Necrovalley))
                return true;
            if (Bot.HasInDeck(GravekeepersGuard) && Enemy.GetMonsterCount() > 0)
                return true;
            return Bot.HasInDeck(GravekeepersSpy);
        }

        private bool BLSEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                if (Enemy.GetMonsterCount() > 0 && !Card.IsDisabled())
                    return true;
            }
            return false;
        }

        private bool MagicianSummon()
        {
            return Bot.HasInGraveyard(Necrovalley) || Bot.HasInGraveyard(PotOfGreed);
        }
    }
}
