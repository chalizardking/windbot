using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Monarchs", "AI_GOAT_Monarchs", "Advanced")]
    public class GOATMonarchsExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int SinisterSerpent = 08131171;
            public const int Sangan = 26202165;
            public const int MorphingJar = 33508719;
            public const int Thestalos = 26205777;
            public const int Zaborg = 51945556;
            public const int Mobius = 04929256;
            public const int MysticTomato = 83011277;
            public const int GravekeepersSpy = 24317029;
            public const int MagicianOfFaith = 31560081;
            public const int SpiritReaper = 23205979;
            public const int ApprenticeMagician = 09156135;
            public const int Breaker = 71413901;
            public const int DDWarriorLady = 07572887;

            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int DelinquentDuo = 44763025;
            public const int SnatchSteal = 45986603;
            public const int HeavyStorm = 19613556;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int PrematureBurial = 70828912;
            public const int NoblemanOfCrossout = 71044499;
            public const int EnemyController = 98045062;
            public const int BookOfMoon = 14087893;
            public const int Scapegoat = 73915051;

            public const int CallOfTheHaunted = 97077563;
            public const int TorrentialTribute = 53582587;
            public const int MirrorForce = 44095762;
            public const int RingOfDestruction = 83555666;
            public const int SakuretsuArmor = 56120475;
            public const int DustTornado = 60082869;
        }

        public GOATMonarchsExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Draw spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity);
            AddExecutor(ExecutorType.Activate, CardId.DelinquentDuo);

            // Removal spells
            AddExecutor(ExecutorType.Activate, CardId.HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, CardId.MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, CardId.SnatchSteal, ActivateSnatchSteal);
            AddExecutor(ExecutorType.Activate, CardId.NoblemanOfCrossout, DefaultNoblemanOfCrossout);
            AddExecutor(ExecutorType.Activate, CardId.BookOfMoon, ActivateBookOfMoon);

            // Monarch support cards
            AddExecutor(ExecutorType.Activate, CardId.EnemyController, ActivateEnemyController);
            AddExecutor(ExecutorType.Activate, CardId.PrematureBurial, ActivatePrematureBurial);
            AddExecutor(ExecutorType.SpellSet, CardId.Scapegoat);
            AddExecutor(ExecutorType.Activate, CardId.Scapegoat, ActivateScapegoat);

            // Monarch summons - Priority based on situation
            AddExecutor(ExecutorType.Summon, CardId.Mobius, SummonMobius);
            AddExecutor(ExecutorType.Activate, CardId.Mobius, ActivateMobius);
            AddExecutor(ExecutorType.Summon, CardId.Zaborg, SummonZaborg);
            AddExecutor(ExecutorType.Activate, CardId.Zaborg, ActivateZaborg);
            AddExecutor(ExecutorType.Summon, CardId.Thestalos, SummonThestalos);
            AddExecutor(ExecutorType.Activate, CardId.Thestalos);

            // Tribute fodder summons
            AddExecutor(ExecutorType.Summon, CardId.MysticTomato);
            AddExecutor(ExecutorType.Activate, CardId.MysticTomato, ActivateMysticTomato);
            AddExecutor(ExecutorType.Summon, CardId.GravekeepersSpy);
            AddExecutor(ExecutorType.Activate, CardId.GravekeepersSpy, ActivateGravekeepersSpy);
            AddExecutor(ExecutorType.Summon, CardId.ApprenticeMagician);
            AddExecutor(ExecutorType.Activate, CardId.ApprenticeMagician, ActivateApprenticeMagician);
            AddExecutor(ExecutorType.Summon, CardId.SpiritReaper);
            AddExecutor(ExecutorType.Summon, CardId.MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfFaith, ActivateMagicianOfFaith);
            AddExecutor(ExecutorType.Summon, CardId.Breaker, SummonBreaker);
            AddExecutor(ExecutorType.Activate, CardId.Breaker);
            AddExecutor(ExecutorType.Summon, CardId.DDWarriorLady);
            AddExecutor(ExecutorType.Activate, CardId.DDWarriorLady, DefaultDDWarriorLady);
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

        private bool ActivateSnatchSteal()
        {
            // Steal monster then tribute for Monarch
            if (Enemy.GetMonsterCount() > 0 && Bot.Hand.Any(card => card.Id == CardId.Thestalos || card.Id == CardId.Zaborg || card.Id == CardId.Mobius))
            {
                ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                AI.SelectCard(target);
                return true;
            }
            return DefaultSnatchSteal();
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

        private bool ActivateEnemyController()
        {
            // Steal opponent's monster then tribute for Monarch
            if (Bot.Hand.Any(card => card.Id == CardId.Thestalos || card.Id == CardId.Zaborg || card.Id == CardId.Mobius))
            {
                if (Enemy.GetMonsterCount() > 0 && Bot.GetMonsterCount() > 0)
                {
                    // Tribute mode - steal and tribute
                    AI.SelectCard(Bot.GetMonsters().OrderBy(card => card.Attack).First());
                    AI.SelectNextCard(Enemy.GetMonsters().OrderByDescending(card => card.Attack).ToArray());
                    return true;
                }
            }
            return false;
        }

        private bool ActivatePrematureBurial()
        {
            // Revive Monarchs or tribute fodder
            if (Bot.Graveyard.Any(card => card.Id == CardId.Mobius))
            {
                AI.SelectCard(CardId.Mobius);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.Zaborg))
            {
                AI.SelectCard(CardId.Zaborg);
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
            // Activate for tribute fodder or defense
            if (Duel.Phase == DuelPhase.End && Duel.Player != 0)
                return true;
            if (Duel.Player == 1 && Duel.Phase == DuelPhase.Battle && Bot.GetMonsterCount() == 0)
                return true;
            return false;
        }

        private bool SummonMobius()
        {
            // Summon when opponent has multiple backrow
            return Bot.GetMonsterCount() > 0 && Enemy.GetSpellCount() >= 2;
        }

        private bool ActivateMobius()
        {
            // Destroy up to 2 spell/traps
            if (Enemy.GetSpells().Count >= 2)
            {
                AI.SelectCard(Enemy.GetSpells().OrderByDescending(card => card.IsFacedown() ? 1 : 0).Take(2).ToArray());
                return true;
            }
            return false;
        }

        private bool SummonZaborg()
        {
            // Summon to remove opponent's biggest threat
            return Bot.GetMonsterCount() > 0 && Enemy.GetMonsterCount() > 0;
        }

        private bool ActivateZaborg()
        {
            // Destroy opponent's monster
            if (Enemy.GetMonsterCount() > 0)
            {
                ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool SummonThestalos()
        {
            // Summon when opponent has cards in hand
            return Bot.GetMonsterCount() > 0 && Enemy.Hand.Count > 0;
        }

        private bool ActivateMysticTomato()
        {
            // Search for more tribute fodder
            if (Bot.Deck.Any(card => card.Id == CardId.SpiritReaper))
            {
                AI.SelectCard(CardId.SpiritReaper);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.SinisterSerpent))
            {
                AI.SelectCard(CardId.SinisterSerpent);
                return true;
            }
            return false;
        }

        private bool ActivateGravekeepersSpy()
        {
            // Search another Spy or Apprentice Magician
            if (Bot.Deck.Any(card => card.Id == CardId.GravekeepersSpy))
            {
                AI.SelectCard(CardId.GravekeepersSpy);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.ApprenticeMagician))
            {
                AI.SelectCard(CardId.ApprenticeMagician);
                return true;
            }
            return false;
        }

        private bool ActivateApprenticeMagician()
        {
            // Search another Apprentice or Magician of Faith
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

        private bool ActivateMagicianOfFaith()
        {
            // Recover tribute-enabling spells
            if (Bot.Graveyard.Any(card => card.Id == CardId.EnemyController))
            {
                AI.SelectCard(CardId.EnemyController);
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

        private bool SummonBreaker()
        {
            // Summon when opponent has backrow
            return Enemy.GetSpellCount() > 0;
        }

        private bool ActivateSangan()
        {
            // Search for tribute fodder
            if (Bot.Deck.Any(card => card.Id == CardId.MagicianOfFaith))
            {
                AI.SelectCard(CardId.MagicianOfFaith);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.SpiritReaper))
            {
                AI.SelectCard(CardId.SpiritReaper);
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
            // Revive Monarchs or tribute fodder
            if (Bot.Graveyard.Any(card => card.Id == CardId.Mobius))
            {
                AI.SelectCard(CardId.Mobius);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.Zaborg))
            {
                AI.SelectCard(CardId.Zaborg);
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
