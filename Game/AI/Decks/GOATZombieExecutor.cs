using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Zombie", "AI_GOAT_Zombie", "Advanced")]
    public class GOATZombieExecutor : DefaultExecutor
    {
        // Monsters
        public const int DesLacooda = 39168895;
        public const int GiantRat = 45121025;
        public const int Kycoo = 88240808;
        public const int PyramidTurtle = 77044671;
        public const int RyuKokki = 87255382;
        public const int Sinister = 71413901;
        public const int SpiritReaper = 74131780;
        public const int VampireLord = 05818798;
        public const int Regeneration = 33184167;

        // Spells
        public const int BookOfLife = 02204140;
        public const int CreatureSwap = 31036355;
        public const int DoubleSpell = 79759861;
        public const int GracefulCharity = 79571449;
        public const int HeavyStorm = 19613556;
        public const int MysticalSpaceTyphoon = 05318639;
        public const int NobleMansOfCrossout = 71044499;
        public const int PotOfGreed = 55144522;
        public const int Raigeki = 70828912;
        public const int PrematureBurial = 45986603;
        public const int Reload = 53839837;

        // Traps
        public const int RoyalDecree = 51452091;
        public const int Solemn = 97077563;

        public GOATZombieExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Priority summons
            AddExecutor(ExecutorType.Summon, RyuKokki, SummonRyuKokki);
            AddExecutor(ExecutorType.Summon, VampireLord);
            AddExecutor(ExecutorType.Summon, Kycoo);
            AddExecutor(ExecutorType.Summon, SpiritReaper, SummonReaper);

            // Monster effects
            AddExecutor(ExecutorType.Activate, PyramidTurtle, PyramidTurtleEffect);
            AddExecutor(ExecutorType.Activate, GiantRat, GiantRatEffect);
            AddExecutor(ExecutorType.Activate, VampireLord);
            AddExecutor(ExecutorType.Activate, DesLacooda);

            // Key spells
            AddExecutor(ExecutorType.Activate, BookOfLife, ActivateBookOfLife);
            AddExecutor(ExecutorType.Activate, PotOfGreed);
            AddExecutor(ExecutorType.Activate, GracefulCharity, DefaultGracefulCharity);
            AddExecutor(ExecutorType.Activate, CreatureSwap, DefaultCreatureSwap);
            AddExecutor(ExecutorType.Activate, HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, PrematureBurial, DefaultPrematureBurial);
            AddExecutor(ExecutorType.Activate, NobleMansOfCrossout, DefaultBookOfMoon);
            AddExecutor(ExecutorType.Activate, DoubleSpell);
            AddExecutor(ExecutorType.Activate, Raigeki, DefaultRaigeki);

            // Traps
            AddExecutor(ExecutorType.Activate, RoyalDecree);
            AddExecutor(ExecutorType.Activate, Solemn, DefaultSolemnJudgment);

            // General summons
            AddExecutor(ExecutorType.MonsterSet, PyramidTurtle);
            AddExecutor(ExecutorType.MonsterSet, GiantRat);
            AddExecutor(ExecutorType.MonsterSet, DesLacooda);
            AddExecutor(ExecutorType.Summon, Sinister);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool SummonRyuKokki()
        {
            // Summon Ryu Kokki to beat over big monsters
            return Enemy.GetMonsters().Any(c => c.Attack >= 2000);
        }

        private bool SummonReaper()
        {
            // Spirit Reaper to stall and discard
            return Enemy.Hand.Count > 2;
        }

        private bool PyramidTurtleEffect()
        {
            // Search Ryu Kokki or Vampire Lord from deck
            if (Bot.HasInDeck(RyuKokki))
                return true;
            return Bot.HasInDeck(VampireLord);
        }

        private bool GiantRatEffect()
        {
            // Search another rat or Kycoo
            if (Bot.HasInDeck(Kycoo))
                return true;
            return Bot.HasInDeck(GiantRat);
        }

        private bool ActivateBookOfLife()
        {
            // Revive zombie from grave and banish opponent's monster
            if (Bot.HasInGraveyard(RyuKokki))
            {
                AI.SelectCard(RyuKokki);
                return true;
            }
            if (Bot.HasInGraveyard(VampireLord))
            {
                AI.SelectCard(VampireLord);
                return true;
            }
            return Bot.Graveyard.Any(c => c.HasRace(CardRace.Zombie));
        }
    }
}
