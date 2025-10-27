using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Control", "AI_GOAT_Control", "Advanced")]
    public class GOATControlExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int BlackLusterSoldier = 72989439;
            public const int SinisterSerpent = 08131171;
            public const int Sangan = 26202165;
            public const int MorphingJar = 33508719;
            public const int TribeInfectingVirus = 33184167;
            public const int Breaker = 71413901;
            public const int AbyssSoldier = 18318842;
            public const int MagicianOfFaith = 31560081;
            public const int DDAssailant = 70074904;
            public const int Tsukuyomi = 34100279;

            public const int Scapegoat = 73915051;
            public const int Metamorphosis = 46411259;
            public const int BookOfMoon = 14087893;
            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int DelinquentDuo = 44763025;
            public const int SnatchSteal = 45986603;
            public const int HeavyStorm = 19613556;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int PrematureBurial = 70828912;
            public const int NoblemanOfCrossout = 71044499;

            public const int CallOfTheHaunted = 97077563;
            public const int TorrentialTribute = 53582587;
            public const int MirrorForce = 44095762;
            public const int RingOfDestruction = 83555666;
            public const int SakuretsuArmor = 56120475;
            public const int BottomlessTrapHole = 29401950;
            public const int DustTornado = 60082869;

            public const int ThousandEyesRestrict = 63519819;
            public const int DarkBalter = 89111398;
            public const int ReaperOnTheNightmare = 85684223;
            public const int FiendSkullDragon = 32593080;
            public const int GatlingDragon = 87751584;
            public const int MusicianKing = 56907389;
        }

        public GOATControlExecutor(GameAI ai, Duel duel)
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

            // Key combo pieces
            AddExecutor(ExecutorType.SpellSet, CardId.Scapegoat, SetScapegoat);
            AddExecutor(ExecutorType.Activate, CardId.Scapegoat, ActivateScapegoat);
            AddExecutor(ExecutorType.Activate, CardId.Metamorphosis, ActivateMetamorphosis);
            AddExecutor(ExecutorType.Activate, CardId.BookOfMoon, ActivateBookOfMoon);

            // Support spells
            AddExecutor(ExecutorType.Activate, CardId.PrematureBurial, ActivatePrematureBurial);

            // Monster summons
            AddExecutor(ExecutorType.Summon, CardId.BlackLusterSoldier, SummonBLS);
            AddExecutor(ExecutorType.Activate, CardId.BlackLusterSoldier, ActivateBLS);
            AddExecutor(ExecutorType.Summon, CardId.Breaker, SummonBreaker);
            AddExecutor(ExecutorType.Activate, CardId.Breaker);
            AddExecutor(ExecutorType.Summon, CardId.Tsukuyomi);
            AddExecutor(ExecutorType.Activate, CardId.Tsukuyomi, ActivateTsukuyomi);
            AddExecutor(ExecutorType.Summon, CardId.MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfFaith, ActivateMagicianOfFaith);
            AddExecutor(ExecutorType.Summon, CardId.AbyssSoldier);
            AddExecutor(ExecutorType.Activate, CardId.AbyssSoldier, ActivateAbyssSoldier);
            AddExecutor(ExecutorType.Summon, CardId.TribeInfectingVirus);
            AddExecutor(ExecutorType.Activate, CardId.TribeInfectingVirus, ActivateTribeInfectingVirus);
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
            AddExecutor(ExecutorType.Activate, CardId.BottomlessTrapHole, DefaultBottomlessTrapHole);
            AddExecutor(ExecutorType.Activate, CardId.SakuretsuArmor, DefaultSakuretsuArmor);
            AddExecutor(ExecutorType.Activate, CardId.DustTornado, DefaultDustTornado);
            AddExecutor(ExecutorType.Activate, CardId.CallOfTheHaunted, ActivateCallOfTheHaunted);

            // Fusion monster effects
            AddExecutor(ExecutorType.Activate, CardId.ThousandEyesRestrict, ActivateThousandEyesRestrict);

            // Default repositions and sets
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        private bool SetScapegoat()
        {
            // Set Scapegoat turn 1 for defense and Metamorphosis combo
            if (Duel.Turn == 1)
                return true;
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

        private bool ActivateMetamorphosis()
        {
            // Use on Goat Token (level 1) to summon Thousand-Eyes Restrict (level 1 fusion)
            if (Bot.HasInMonstersZone(CardId.Scapegoat))
            {
                // Check if we have Thousand-Eyes Restrict in extra deck
                if (Bot.ExtraDeck.Any(card => card.Id == CardId.ThousandEyesRestrict))
                {
                    // Select a token (Goat tokens are level 1)
                    ClientCard token = Bot.GetMonsters().FirstOrDefault(card => card.IsToken());
                    if (token != null)
                    {
                        AI.SelectCard(CardId.ThousandEyesRestrict);
                        AI.SelectMaterials(token);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool ActivateBookOfMoon()
        {
            // Flip Magician of Faith or Tsukuyomi for reuse, or stop opponent's attack
            if (Enemy.BattlingMonster != null && Enemy.BattlingMonster.IsAttack())
            {
                AI.SelectCard(Enemy.BattlingMonster);
                return true;
            }

            // Flip our own face-up flip effects for reuse
            ClientCard magician = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MagicianOfFaith && card.IsFaceup());
            if (magician != null && Bot.Graveyard.Any(card => card.IsSpell()))
            {
                AI.SelectCard(magician);
                return true;
            }

            ClientCard tsukuyomi = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.Tsukuyomi && card.IsFaceup());
            if (tsukuyomi != null)
            {
                AI.SelectCard(tsukuyomi);
                return true;
            }

            return false;
        }

        private bool ActivatePrematureBurial()
        {
            // Revive BLS, Breaker, or other key monsters
            ClientCard target = Bot.Graveyard.GetMonsters().OrderByDescending(card => card.Attack).FirstOrDefault();
            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool SummonBLS()
        {
            // Check if we can properly summon BLS (need LIGHT and DARK in grave)
            int light = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
            int dark = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));
            return light > 0 && dark > 0 && light + dark >= 2;
        }

        private bool ActivateBLS()
        {
            // Banish key threats or attack twice for game
            if (Card.Location == CardLocation.MonsterZone)
            {
                // Banish enemy's strongest monster
                if (Enemy.GetMonsterCount() > 0 && !Card.IsDisabled())
                {
                    ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                    AI.SelectCard(target);
                    return true;
                }
            }
            return false;
        }

        private bool SummonBreaker()
        {
            // Summon Breaker when opponent has backrow
            return Enemy.GetSpellCount() > 0;
        }

        private bool ActivateTsukuyomi()
        {
            // Flip our flip effects face-down for reuse, or flip BLS to reset it
            ClientCard magician = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MagicianOfFaith && card.IsFaceup());
            if (magician != null)
            {
                AI.SelectCard(magician);
                return true;
            }

            ClientCard bls = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.BlackLusterSoldier && card.IsFaceup());
            if (bls != null && bls.IsDisabled())
            {
                AI.SelectCard(bls);
                return true;
            }

            return false;
        }

        private bool ActivateMagicianOfFaith()
        {
            // Recover most valuable spell: Metamorphosis > Scapegoat > Pot of Greed
            if (Bot.Graveyard.Any(card => card.Id == CardId.Metamorphosis))
            {
                AI.SelectCard(CardId.Metamorphosis);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.Scapegoat))
            {
                AI.SelectCard(CardId.Scapegoat);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.PotOfGreed))
            {
                AI.SelectCard(CardId.PotOfGreed);
                return true;
            }

            // Otherwise recover any spell
            ClientCard spell = Bot.Graveyard.GetSpells().FirstOrDefault();
            if (spell != null)
            {
                AI.SelectCard(spell);
                return true;
            }
            return false;
        }

        private bool ActivateAbyssSoldier()
        {
            // Discard Sinister Serpent to bounce threats
            if (Bot.Hand.Any(card => card.Id == CardId.SinisterSerpent) && Enemy.GetMonsterCount() > 0)
            {
                AI.SelectCard(CardId.SinisterSerpent);
                AI.SelectNextCard(Enemy.GetMonsters().OrderByDescending(card => card.Attack).ToArray());
                return true;
            }
            return false;
        }

        private bool ActivateTribeInfectingVirus()
        {
            // Clear field when ahead - discard to destroy all monsters of chosen type
            if (Bot.LifePoints > Enemy.LifePoints && Enemy.GetMonsterCount() >= 2 && Bot.Hand.Count > 2)
            {
                // Discard Sinister Serpent if we have it
                if (Bot.Hand.Any(card => card.Id == CardId.SinisterSerpent))
                    AI.SelectCard(CardId.SinisterSerpent);
                else
                    AI.SelectCard(Bot.Hand.OrderBy(card => card.Attack).First());

                // Choose most common type on enemy field
                return true;
            }
            return false;
        }

        private bool ActivateSangan()
        {
            // Search for combo pieces or utility monsters
            if (Bot.Deck.Any(card => card.Id == CardId.MagicianOfFaith) && !Bot.Hand.Any(card => card.Id == CardId.MagicianOfFaith))
            {
                AI.SelectCard(CardId.MagicianOfFaith);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.Tsukuyomi))
            {
                AI.SelectCard(CardId.Tsukuyomi);
                return true;
            }
            return false;
        }

        private bool ActivateRingOfDestruction()
        {
            // Use on BLS or other high-ATK threats for damage + removal
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
            // Revive best monster from grave
            ClientCard target = Bot.Graveyard.GetMonsters().OrderByDescending(card => card.Attack).FirstOrDefault();
            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool ActivateThousandEyesRestrict()
        {
            // Absorb opponent's strongest monster
            if (Enemy.GetMonsterCount() > 0)
            {
                ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                AI.SelectCard(target);
                return true;
            }
            return false;
        }
    }
}
