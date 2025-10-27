using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Perfect Circle", "AI_GOAT_PerfectCircle", "Advanced")]
    public class GOATPerfectCircleExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int BlackLusterSoldier = 72989439;
            public const int SinisterSerpent = 08131171;
            public const int Sangan = 26202165;
            public const int MorphingJar = 33508719;
            public const int Breaker = 71413901;
            public const int MagicianOfFaith = 31560081;
            public const int MysticTomato = 83011277;
            public const int NightAssailant = 25005816;
            public const int ApprenticeMagician = 09156135;
            public const int DDAssailant = 70074904;
            public const int DDWarriorLady = 07572887;
            public const int AirknightParshath = 18036057;

            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int DelinquentDuo = 44763025;
            public const int SnatchSteal = 45986603;
            public const int HeavyStorm = 19613556;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int PrematureBurial = 70828912;
            public const int NoblemanOfCrossout = 71044499;
            public const int BookOfMoon = 14087893;
            public const int SmashingGround = 97169186;
            public const int CreatureSwap = 31036355;
            public const int Scapegoat = 73915051;

            public const int CallOfTheHaunted = 97077563;
            public const int TorrentialTribute = 53582587;
            public const int MirrorForce = 44095762;
            public const int RingOfDestruction = 83555666;
            public const int SakuretsuArmor = 56120475;
            public const int DustTornado = 60082869;
        }

        public GOATPerfectCircleExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Draw spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity);
            AddExecutor(ExecutorType.Activate, CardId.DelinquentDuo);

            // Removal spells
            AddExecutor(ExecutorType.Activate, CardId.HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, CardId.MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, CardId.SnatchSteal, DefaultSnatchSteal);
            AddExecutor(ExecutorType.Activate, CardId.SmashingGround, DefaultSmashingGround);
            AddExecutor(ExecutorType.Activate, CardId.NoblemanOfCrossout, DefaultNoblemanOfCrossout);
            AddExecutor(ExecutorType.Activate, CardId.BookOfMoon, ActivateBookOfMoon);

            // Key card - Creature Swap
            AddExecutor(ExecutorType.Activate, CardId.CreatureSwap, ActivateCreatureSwap);

            // Support spells
            AddExecutor(ExecutorType.Activate, CardId.PrematureBurial, ActivatePrematureBurial);
            AddExecutor(ExecutorType.SpellSet, CardId.Scapegoat);
            AddExecutor(ExecutorType.Activate, CardId.Scapegoat, ActivateScapegoat);

            // Monster summons - BLS priority
            AddExecutor(ExecutorType.SpSummon, CardId.BlackLusterSoldier, SummonBLS);
            AddExecutor(ExecutorType.Activate, CardId.BlackLusterSoldier, ActivateBLS);

            // Searcher chain monsters
            AddExecutor(ExecutorType.Summon, CardId.MysticTomato);
            AddExecutor(ExecutorType.Activate, CardId.MysticTomato, ActivateMysticTomato);
            AddExecutor(ExecutorType.Summon, CardId.ApprenticeMagician);
            AddExecutor(ExecutorType.Activate, CardId.ApprenticeMagician, ActivateApprenticeMagician);

            // Other summons
            AddExecutor(ExecutorType.Summon, CardId.AirknightParshath);
            AddExecutor(ExecutorType.Summon, CardId.Breaker, SummonBreaker);
            AddExecutor(ExecutorType.Activate, CardId.Breaker);
            AddExecutor(ExecutorType.Summon, CardId.MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfFaith, ActivateMagicianOfFaith);
            AddExecutor(ExecutorType.Summon, CardId.DDWarriorLady);
            AddExecutor(ExecutorType.Activate, CardId.DDWarriorLady, DefaultDDWarriorLady);
            AddExecutor(ExecutorType.Summon, CardId.DDAssailant);
            AddExecutor(ExecutorType.Summon, CardId.MorphingJar);
            AddExecutor(ExecutorType.Summon, CardId.Sangan);
            AddExecutor(ExecutorType.Summon, CardId.SinisterSerpent);

            // Flip effects
            AddExecutor(ExecutorType.Activate, CardId.MorphingJar);
            AddExecutor(ExecutorType.Activate, CardId.Sangan, ActivateSangan);

            // Traps
            AddExecutor(ExecutorType.Activate, CardId.TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, CardId.MirrorForce, DefaultMirrorForce);
            AddExecutor(ExecutorType.Activate, CardId.RingOfDestruction, ActivateRingOfDestruction);
            AddExecutor(ExecutorType.Activate, CardId.SakuretsuArmor, DefaultSakuretsuArmor);
            AddExecutor(ExecutorType.Activate, CardId.DustTornado, DefaultDustTornado);
            AddExecutor(ExecutorType.Activate, CardId.CallOfTheHaunted, ActivateCallOfTheHaunted);

            // Default repositions and sets
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        private bool ActivateCreatureSwap()
        {
            // Give Scapegoat token or weak monster, take strong monster
            if (Bot.GetMonsters().Any(card => card.IsToken()) && Enemy.GetMonsterCount() > 0)
            {
                // Give token
                AI.SelectCard(Bot.GetMonsters().FirstOrDefault(card => card.IsToken()));
                AI.SelectNextCard(Enemy.GetMonsters().OrderByDescending(card => card.Attack).ToArray());
                return true;
            }

            if (Bot.GetMonsterCount() > 0 && Enemy.GetMonsterCount() > 0)
            {
                // Give weakest, take strongest
                ClientCard weakest = Bot.GetMonsters().OrderBy(card => card.Attack).FirstOrDefault();
                ClientCard strongest = Enemy.GetMonsters().OrderByDescending(card => card.Attack).FirstOrDefault();

                if (weakest != null && strongest != null && strongest.Attack > weakest.Attack)
                {
                    AI.SelectCard(weakest);
                    AI.SelectNextCard(strongest);
                    return true;
                }
            }

            return false;
        }

        private bool ActivateBookOfMoon()
        {
            // Stop opponent's attack or flip for reuse
            if (Enemy.BattlingMonster != null && Enemy.BattlingMonster.IsAttack())
            {
                AI.SelectCard(Enemy.BattlingMonster);
                return true;
            }

            ClientCard magician = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MagicianOfFaith && card.IsFaceup());
            if (magician != null && Bot.Graveyard.Any(card => card.IsSpell()))
            {
                AI.SelectCard(magician);
                return true;
            }

            return false;
        }

        private bool ActivatePrematureBurial()
        {
            // Revive BLS or key monsters
            if (Bot.Graveyard.Any(card => card.Id == CardId.BlackLusterSoldier))
            {
                AI.SelectCard(CardId.BlackLusterSoldier);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.AirknightParshath))
            {
                AI.SelectCard(CardId.AirknightParshath);
                return true;
            }

            ClientCard target = Bot.Graveyard.GetMonsters().OrderByDescending(card => card.Attack).FirstOrDefault();
            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool ActivateScapegoat()
        {
            // Activate for defense or Creature Swap fodder
            if (Duel.Phase == DuelPhase.End && Duel.Player != 0)
                return true;
            if (Duel.Player == 1 && Duel.Phase == DuelPhase.Battle && Bot.GetMonsterCount() == 0)
                return true;
            return false;
        }

        private bool SummonBLS()
        {
            // Check if we can summon BLS
            int light = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
            int dark = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));
            return light > 0 && dark > 0;
        }

        private bool ActivateBLS()
        {
            // Banish key threats or attack twice
            if (Card.Location == CardLocation.MonsterZone && !Card.IsDisabled())
            {
                if (Enemy.GetMonsterCount() > 0)
                {
                    ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                    AI.SelectCard(target);
                    return true;
                }
            }
            return false;
        }

        private bool ActivateMysticTomato()
        {
            // Search Apprentice Magician to continue thinning deck
            if (Bot.Deck.Any(card => card.Id == CardId.ApprenticeMagician))
            {
                AI.SelectCard(CardId.ApprenticeMagician);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.NightAssailant))
            {
                AI.SelectCard(CardId.NightAssailant);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.DDAssailant))
            {
                AI.SelectCard(CardId.DDAssailant);
                return true;
            }
            return false;
        }

        private bool ActivateApprenticeMagician()
        {
            // Search another Apprentice or Magician of Faith for continuous thinning
            if (Bot.Deck.Any(card => card.Id == CardId.ApprenticeMagician))
            {
                AI.SelectCard(CardId.ApprenticeMagician);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.MagicianOfFaith))
            {
                AI.SelectCard(CardId.MagicianOfFaith);
                return true;
            }
            return false;
        }

        private bool SummonBreaker()
        {
            // Summon when opponent has backrow
            return Enemy.GetSpellCount() > 0;
        }

        private bool ActivateMagicianOfFaith()
        {
            // Recover key spells
            if (Bot.Graveyard.Any(card => card.Id == CardId.PotOfGreed))
            {
                AI.SelectCard(CardId.PotOfGreed);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.CreatureSwap))
            {
                AI.SelectCard(CardId.CreatureSwap);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.PrematureBurial))
            {
                AI.SelectCard(CardId.PrematureBurial);
                return true;
            }

            ClientCard spell = Bot.Graveyard.GetSpells().FirstOrDefault();
            if (spell != null)
            {
                AI.SelectCard(spell);
                return true;
            }
            return false;
        }

        private bool ActivateSangan()
        {
            // Search for searchers to continue deck thinning
            if (Bot.Deck.Any(card => card.Id == CardId.MysticTomato))
            {
                AI.SelectCard(CardId.MysticTomato);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.MagicianOfFaith))
            {
                AI.SelectCard(CardId.MagicianOfFaith);
                return true;
            }
            return false;
        }

        private bool ActivateRingOfDestruction()
        {
            // Use on high-ATK threats
            if (Enemy.GetMonsters().Any(card => card.Attack >= 2000))
            {
                ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool ActivateCallOfTheHaunted()
        {
            // Revive BLS or best monster
            if (Bot.Graveyard.Any(card => card.Id == CardId.BlackLusterSoldier))
            {
                AI.SelectCard(CardId.BlackLusterSoldier);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.AirknightParshath))
            {
                AI.SelectCard(CardId.AirknightParshath);
                return true;
            }

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
