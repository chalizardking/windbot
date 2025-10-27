using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Zombie", "AI_GOAT_Zombie", "Advanced")]
    public class GOATZombieExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int SinisterSerpent = 08131171;
            public const int Sangan = 26202165;
            public const int MorphingJar = 33508719;
            public const int PyramidTurtle = 77044671;
            public const int VampireLord = 53839837;
            public const int RyuKokki = 87255382;
            public const int RegeneratingMummy = 70821187;
            public const int SpiritReaper = 23205979;
            public const int DespairFromTheDark = 71645242;
            public const int MagicianOfFaith = 31560081;
            public const int NoblemanEaterBug = 50098496;
            public const int MysticTomato = 83011277;

            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int DelinquentDuo = 44763025;
            public const int SnatchSteal = 45986603;
            public const int HeavyStorm = 19613556;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int PrematureBurial = 70828912;
            public const int NoblemanOfCrossout = 71044499;
            public const int BookOfLife = 02204140;
            public const int BookOfMoon = 14087893;
            public const int Scapegoat = 73915051;

            public const int CallOfTheHaunted = 97077563;
            public const int TorrentialTribute = 53582587;
            public const int MirrorForce = 44095762;
            public const int RingOfDestruction = 83555666;
            public const int SakuretsuArmor = 56120475;
            public const int DustTornado = 60082869;
        }

        public GOATZombieExecutor(GameAI ai, Duel duel)
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
            AddExecutor(ExecutorType.Activate, CardId.NoblemanOfCrossout, DefaultNoblemanOfCrossout);

            // Zombie-specific spells
            AddExecutor(ExecutorType.Activate, CardId.BookOfLife, ActivateBookOfLife);
            AddExecutor(ExecutorType.Activate, CardId.BookOfMoon, ActivateBookOfMoon);
            AddExecutor(ExecutorType.Activate, CardId.PrematureBurial, ActivatePrematureBurial);
            AddExecutor(ExecutorType.SpellSet, CardId.Scapegoat);
            AddExecutor(ExecutorType.Activate, CardId.Scapegoat, ActivateScapegoat);

            // Monster summons
            AddExecutor(ExecutorType.Summon, CardId.VampireLord);
            AddExecutor(ExecutorType.Summon, CardId.RyuKokki);
            AddExecutor(ExecutorType.Summon, CardId.PyramidTurtle);
            AddExecutor(ExecutorType.Activate, CardId.PyramidTurtle, ActivatePyramidTurtle);
            AddExecutor(ExecutorType.Summon, CardId.SpiritReaper);
            AddExecutor(ExecutorType.Summon, CardId.RegeneratingMummy);
            AddExecutor(ExecutorType.Summon, CardId.MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfFaith, ActivateMagicianOfFaith);
            AddExecutor(ExecutorType.Summon, CardId.NoblemanEaterBug);
            AddExecutor(ExecutorType.Activate, CardId.NoblemanEaterBug, ActivateNoblemanEaterBug);
            AddExecutor(ExecutorType.Summon, CardId.MysticTomato);
            AddExecutor(ExecutorType.Activate, CardId.MysticTomato, ActivateMysticTomato);
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

        private bool ActivateBookOfLife()
        {
            // Revive zombie + banish opponent's monster
            ClientCard zombie = Bot.Graveyard.GetMonsters().Where(card => card.HasRace(CardRace.Zombie))
                                             .OrderByDescending(card => card.Attack).FirstOrDefault();
            if (zombie != null && Enemy.Graveyard.GetMonsterCount() > 0)
            {
                AI.SelectCard(zombie);
                AI.SelectNextCard(Enemy.Graveyard.GetMonsters().OrderByDescending(card => card.Attack).ToArray());
                return true;
            }
            return false;
        }

        private bool ActivateBookOfMoon()
        {
            // Flip Magician of Faith for reuse, or stop opponent's attack
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
            // Revive Vampire Lord or Ryu Kokki
            if (Bot.Graveyard.Any(card => card.Id == CardId.VampireLord))
            {
                AI.SelectCard(CardId.VampireLord);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.RyuKokki))
            {
                AI.SelectCard(CardId.RyuKokki);
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
            // Activate during opponent's End Phase or when under attack
            if (Duel.Phase == DuelPhase.End && Duel.Player != 0)
                return true;
            if (Duel.Player == 1 && Duel.Phase == DuelPhase.Battle && Bot.GetMonsterCount() == 0)
                return true;
            return false;
        }

        private bool ActivatePyramidTurtle()
        {
            // Search Vampire Lord or Ryu Kokki from deck when destroyed
            if (Bot.Deck.Any(card => card.Id == CardId.VampireLord))
            {
                AI.SelectCard(CardId.VampireLord);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.RyuKokki))
            {
                AI.SelectCard(CardId.RyuKokki);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.RegeneratingMummy))
            {
                AI.SelectCard(CardId.RegeneratingMummy);
                return true;
            }
            return false;
        }

        private bool ActivateMagicianOfFaith()
        {
            // Recover Book of Life or other key spells
            if (Bot.Graveyard.Any(card => card.Id == CardId.BookOfLife))
            {
                AI.SelectCard(CardId.BookOfLife);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.PotOfGreed))
            {
                AI.SelectCard(CardId.PotOfGreed);
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

        private bool ActivateNoblemanEaterBug()
        {
            // Flip to destroy monsters
            if (Enemy.GetMonsterCount() > 0)
            {
                ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool ActivateMysticTomato()
        {
            // Search for Spirit Reaper or Sinister Serpent
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

        private bool ActivateSangan()
        {
            // Search for utility monsters
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
            // Revive Vampire Lord or best zombie
            if (Bot.Graveyard.Any(card => card.Id == CardId.VampireLord))
            {
                AI.SelectCard(CardId.VampireLord);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.RyuKokki))
            {
                AI.SelectCard(CardId.RyuKokki);
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
