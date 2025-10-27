using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Earth Aggro", "AI_GOAT_EarthAggro", "Advanced")]
    public class GOATEarthAggroExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int Breaker = 71413901;
            public const int DDAssailant = 70074904;
            public const int DDWarriorLady = 07572887;
            public const int ExiledForce = 74131780;
            public const int GiantRat = 45121025;
            public const int Gigantes = 53776525;
            public const int GoblinAttackForce = 78658564;
            public const int InjectionFairyLily = 05818798;
            public const int MysticSwordsmanLV2 = 47507260;
            public const int TheRockSpirit = 68815132;

            public const int DelinquentDuo = 44763025;
            public const int GracefulCharity = 79571449;
            public const int HeavyStorm = 19613556;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int NoblemanOfCrossout = 71044499;
            public const int PotOfGreed = 55144522;
            public const int PrematureBurial = 70828912;
            public const int ReinforcementOfTheArmy = 32807846;
            public const int SmashingGround = 97169186;
            public const int SnatchSteal = 45986603;

            public const int CallOfTheHaunted = 97077563;
            public const int DustTornado = 60082869;
            public const int MirrorForce = 44095762;
            public const int RingOfDestruction = 83555666;
            public const int TorrentialTribute = 53582587;
            public const int TrapDustshoot = 64697231;
        }

        public GOATEarthAggroExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Draw spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity);
            AddExecutor(ExecutorType.Activate, CardId.DelinquentDuo);

            // Removal
            AddExecutor(ExecutorType.Activate, CardId.HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, CardId.MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, CardId.SnatchSteal, DefaultSnatchSteal);
            AddExecutor(ExecutorType.Activate, CardId.SmashingGround, DefaultSmashingGround);
            AddExecutor(ExecutorType.Activate, CardId.NoblemanOfCrossout, DefaultNoblemanOfCrossout);
            AddExecutor(ExecutorType.Activate, CardId.PrematureBurial, ActivatePrematureBurial);

            // Searcher
            AddExecutor(ExecutorType.Activate, CardId.ReinforcementOfTheArmy, ActivateReinforcement);

            // Aggressive summons
            AddExecutor(ExecutorType.Summon, CardId.Gigantes);
            AddExecutor(ExecutorType.Summon, CardId.GoblinAttackForce);
            AddExecutor(ExecutorType.Summon, CardId.InjectionFairyLily);
            AddExecutor(ExecutorType.Summon, CardId.MysticSwordsmanLV2);
            AddExecutor(ExecutorType.Summon, CardId.DDAssailant);
            AddExecutor(ExecutorType.Summon, CardId.GiantRat);
            AddExecutor(ExecutorType.Activate, CardId.GiantRat, ActivateGiantRat);
            AddExecutor(ExecutorType.Summon, CardId.DDWarriorLady);
            AddExecutor(ExecutorType.Activate, CardId.DDWarriorLady, DefaultDDWarriorLady);
            AddExecutor(ExecutorType.Summon, CardId.ExiledForce);
            AddExecutor(ExecutorType.Activate, CardId.ExiledForce, ActivateExiledForce);
            AddExecutor(ExecutorType.Summon, CardId.TheRockSpirit);
            AddExecutor(ExecutorType.Summon, CardId.Breaker);
            AddExecutor(ExecutorType.Activate, CardId.Breaker);

            // Traps
            AddExecutor(ExecutorType.Activate, CardId.TrapDustshoot, DefaultTrapDustshoot);
            AddExecutor(ExecutorType.Activate, CardId.TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, CardId.MirrorForce, DefaultMirrorForce);
            AddExecutor(ExecutorType.Activate, CardId.RingOfDestruction, DefaultRingOfDestruction);
            AddExecutor(ExecutorType.Activate, CardId.DustTornado, DefaultDustTornado);
            AddExecutor(ExecutorType.Activate, CardId.CallOfTheHaunted, ActivateCallOfTheHaunted);

            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        private bool ActivateReinforcement()
        {
            // Search for D.D. Warrior Lady or Exiled Force
            if (Enemy.GetMonsterCount() > 0 && Bot.Deck.Any(card => card.Id == CardId.DDWarriorLady))
            {
                AI.SelectCard(CardId.DDWarriorLady);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.ExiledForce))
            {
                AI.SelectCard(CardId.ExiledForce);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.MysticSwordsmanLV2))
            {
                AI.SelectCard(CardId.MysticSwordsmanLV2);
                return true;
            }
            return false;
        }

        private bool ActivateGiantRat()
        {
            // Search for Gigantes or other Earth monsters
            if (Bot.Deck.Any(card => card.Id == CardId.Gigantes))
            {
                AI.SelectCard(CardId.Gigantes);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.TheRockSpirit))
            {
                AI.SelectCard(CardId.TheRockSpirit);
                return true;
            }
            return false;
        }

        private bool ActivateExiledForce()
        {
            if (Enemy.GetMonsterCount() > 0)
            {
                ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool ActivatePrematureBurial()
        {
            ClientCard target = Bot.Graveyard.GetMonsters().OrderByDescending(card => card.Attack).FirstOrDefault();
            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool ActivateCallOfTheHaunted()
        {
            ClientCard target = Bot.Graveyard.GetMonsters().OrderByDescending(card => card.Attack).FirstOrDefault();
            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }
            return false;
        }
    }
}
