using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Strike Ninja", "AI_GOAT_StrikeNinja", "Advanced")]
    public class GOATStrikeNinjaExecutor : DefaultExecutor
    {
        // Monsters
        public const int BlackLusterSoldier = 71413901;
        public const int Dekoichi = 87621407;
        public const int Sinister = 74131780;
        public const int MagicianOfFaith = 24317029;
        public const int NightAssailant = 83011277;
        public const int EvilHero = 07913069;
        public const int SinisterSerpent = 26202165;
        public const int StrikeNinja = 41006930;
        public const int Tsukuyomi = 34100279;

        // Spells
        public const int DustTornado = 14087893;
        public const int Scapegoat = 73915051;
        public const int HeavyStorm = 19613556;
        public const int MysticalSpaceTyphoon = 05318639;
        public const int NobleMansOfCrossout = 71044499;
        public const int PotOfGreed = 55144522;
        public const int Raigeki = 32807846;
        public const int PrematureBurial = 45986603;
        public const int SnatchSteal = 44095762;

        // Traps
        public const int ReturnFromDD = 27174286;
        public const int RingOfDestruction = 83555666;
        public const int SakuretsuArmor = 56120475;
        public const int CallOfTheHaunted = 41420027;

        public GOATStrikeNinjaExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Priority monsters to summon
            AddExecutor(ExecutorType.Summon, StrikeNinja, StrikeNinjaSummon);
            AddExecutor(ExecutorType.Summon, BlackLusterSoldier);
            AddExecutor(ExecutorType.Summon, Sinister);
            AddExecutor(ExecutorType.Summon, MagicianOfFaith, MagicianSummon);

            // Monster effects
            AddExecutor(ExecutorType.Activate, StrikeNinja, StrikeNinjaEffect);
            AddExecutor(ExecutorType.Activate, BlackLusterSoldier, BLSEffect);
            AddExecutor(ExecutorType.Activate, MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, Tsukuyomi);

            // Key spells
            AddExecutor(ExecutorType.Activate, PotOfGreed);
            AddExecutor(ExecutorType.Activate, HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, SnatchSteal, DefaultSnatchSteal);
            AddExecutor(ExecutorType.Activate, PrematureBurial, DefaultPrematureBurial);
            AddExecutor(ExecutorType.Activate, NobleMansOfCrossout, DefaultBookOfMoon);
            AddExecutor(ExecutorType.Activate, Scapegoat, DefaultScapegoat);

            // Traps
            AddExecutor(ExecutorType.Activate, ReturnFromDD, ActivateReturnFromDD);
            AddExecutor(ExecutorType.Activate, CallOfTheHaunted, DefaultCallOfTheHaunted);
            AddExecutor(ExecutorType.Activate, RingOfDestruction, DefaultRingOfDestruction);
            AddExecutor(ExecutorType.Activate, SakuretsuArmor, DefaultSakuretsuArmor);
            AddExecutor(ExecutorType.Activate, DustTornado, DefaultMysticalSpaceTyphoon);

            // General summons and sets
            AddExecutor(ExecutorType.Summon, Tsukuyomi);
            AddExecutor(ExecutorType.Summon, Dekoichi);
            AddExecutor(ExecutorType.Summon, NightAssailant);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.MonsterSet, DefaultMonsterSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool StrikeNinjaSummon()
        {
            // Summon Strike Ninja if we have dark monsters to banish
            int darkCount = Bot.Hand.Count(c => c != null && c.HasRace(CardRace.Fiend) && !c.IsCode(StrikeNinja));
            return darkCount >= 2;
        }

        private bool StrikeNinjaEffect()
        {
            // Use Strike Ninja's effect to dodge removal
            if (Card.IsAttack() && Enemy.HasInMonstersZone(CardType.Monster))
                return true;
            if (Duel.CurrentChain.Any(c => c.Controller == 1))
                return true;
            return false;
        }

        private bool BLSEffect()
        {
            // BLS removal or double attack
            if (Card.Location == CardLocation.MonsterZone)
            {
                if (Enemy.GetMonsterCount() > 0 && !Card.IsDisabled())
                    return true;
            }
            return false;
        }

        private bool MagicianSummon()
        {
            return Bot.HasInGraveyard(PotOfGreed) || Bot.HasInGraveyard(PrematureBurial);
        }

        private bool ActivateReturnFromDD()
        {
            // Calculate if we can OTK with banished monsters
            int banishedAttack = 0;
            foreach (ClientCard card in Bot.Banished)
            {
                if (card.HasType(CardType.Monster))
                    banishedAttack += card.Attack;
            }

            // If we can deal enough damage for OTK
            if (banishedAttack + Bot.GetMonsterAttack() >= Enemy.LifePoints)
                return true;

            // Or if we have Strike Ninja and need to recover resources
            return Bot.HasInBanished(StrikeNinja) && Bot.LifePoints < 4000;
        }
    }
}
