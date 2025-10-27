using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Empty Jar", "AI_GOAT_EmptyJar", "Advanced")]
    public class GOATEmptyJarExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int MorphingJar = 33508719;
            public const int CyberJar = 34124316;
            public const int NeedleWorm = 81843628;
            public const int CyberStein = 69015963;
            public const int MagicianOfFaith = 31560081;

            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int CardDestruction = 72892473;
            public const int Reload = 22589918;
            public const int HandDestruction = 74848038;
            public const int Metamorphosis = 46411259;
            public const int BookOfMoon = 14087893;
            public const int BookOfTaiyou = 38699854;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int HeavyStorm = 19613556;
            public const int MessengerOfPeace = 44656491;
            public const int SwordsOfRevealingLight = 72302403;
            public const int LevelLimitAreaB = 03136426;

            public const int GoodGoblinHousekeeping = 09744376;
            public const int JarOfGreed = 83968380;
            public const int LegacyOfYataGarasu = 74860293;
            public const int Ceasefire = 36468556;
            public const int GravityBind = 85742772;
            public const int MagicJammer = 77414722;

            public const int BlueEyesUltimateDragon = 23995346;
            public const int LastWarrior = 86099788;
            public const int ReaperOnTheNightmare = 85684223;
            public const int DarkBalter = 89111398;
            public const int FiendSkullDragon = 32593080;
        }

        public GOATEmptyJarExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Stall cards - activate immediately
            AddExecutor(ExecutorType.Activate, CardId.MessengerOfPeace, ActivateMessengerOfPeace);
            AddExecutor(ExecutorType.Activate, CardId.SwordsOfRevealingLight, ActivateSwordsOfRevealingLight);
            AddExecutor(ExecutorType.Activate, CardId.LevelLimitAreaB);
            AddExecutor(ExecutorType.Activate, CardId.GravityBind);

            // Draw and mill spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity);
            AddExecutor(ExecutorType.Activate, CardId.CardDestruction, ActivateCardDestruction);
            AddExecutor(ExecutorType.Activate, CardId.Reload);
            AddExecutor(ExecutorType.Activate, CardId.HandDestruction);

            // Book spells for Morphing Jar loops
            AddExecutor(ExecutorType.Activate, CardId.BookOfMoon, ActivateBookOfMoon);
            AddExecutor(ExecutorType.Activate, CardId.BookOfTaiyou, ActivateBookOfTaiyou);
            AddExecutor(ExecutorType.Activate, CardId.Metamorphosis, ActivateMetamorphosis);

            // Removal
            AddExecutor(ExecutorType.Activate, CardId.HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, CardId.MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);

            // Emergency beater
            AddExecutor(ExecutorType.Activate, CardId.CyberStein, ActivateCyberStein);

            // Monster summons
            AddExecutor(ExecutorType.Summon, CardId.MorphingJar);
            AddExecutor(ExecutorType.Summon, CardId.MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfFaith, ActivateMagicianOfFaith);
            AddExecutor(ExecutorType.Summon, CardId.NeedleWorm);
            AddExecutor(ExecutorType.Summon, CardId.CyberStein);
            AddExecutor(ExecutorType.Summon, CardId.CyberJar);

            // Flip effects
            AddExecutor(ExecutorType.Activate, CardId.MorphingJar);
            AddExecutor(ExecutorType.Activate, CardId.CyberJar);
            AddExecutor(ExecutorType.Activate, CardId.NeedleWorm);

            // Draw traps
            AddExecutor(ExecutorType.Activate, CardId.GoodGoblinHousekeeping, ActivateGoodGoblinHousekeeping);
            AddExecutor(ExecutorType.Activate, CardId.JarOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.LegacyOfYataGarasu);

            // Burn and protection
            AddExecutor(ExecutorType.Activate, CardId.Ceasefire, ActivateCeasefire);
            AddExecutor(ExecutorType.Activate, CardId.MagicJammer, DefaultMagicJammer);

            // Default repositions and sets
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        private bool ActivateMessengerOfPeace()
        {
            // Activate to stall
            return Enemy.GetMonsterCount() > 0 || Duel.Turn <= 3;
        }

        private bool ActivateSwordsOfRevealingLight()
        {
            // Activate when opponent has monsters
            return Enemy.GetMonsterCount() > 0;
        }

        private bool ActivateCardDestruction()
        {
            // Mill deck and cycle for combo pieces
            // But check if we'll deck out ourselves
            int cardsToDiscard = Bot.Hand.Count;
            int cardsWeDraw = cardsToDiscard;

            if (Bot.Deck.Count < cardsWeDraw)
            {
                // We'll deck out - don't do it unless opponent decks out first
                int enemyCardsToDiscard = Enemy.Hand.Count;
                if (Enemy.Deck.Count < enemyCardsToDiscard && Enemy.Deck.Count <= Bot.Deck.Count)
                {
                    // Opponent decks out first - go for it!
                    return true;
                }
                return false;
            }

            // Check if this brings opponent close to deck-out
            int enemyCardsToDiscard = Enemy.Hand.Count;
            if (Enemy.Deck.Count <= enemyCardsToDiscard + 5)
            {
                // Opponent is close to decking out - prioritize this
                return true;
            }

            return Bot.Hand.Count > 0;
        }

        private bool ActivateBookOfMoon()
        {
            // Check deck-out win condition proximity first
            bool opponentNearDeckOut = Enemy.Deck.Count <= 10;
            bool weCanSurvive = Bot.Deck.Count > 5;

            // Flip Morphing Jar face-down to reuse it for deck-out strategy
            ClientCard morphingJar = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MorphingJar && card.IsFaceup());
            if (morphingJar != null)
            {
                // Only flip if opponent has enough deck to draw from
                // (Morphing Jar makes both players discard hand and draw 5)
                if (Enemy.Deck.Count >= 5 && Bot.Deck.Count >= 5)
                {
                    AI.SelectCard(morphingJar);
                    return true;
                }
                // If opponent will deck out from this, do it even if we're close
                else if (Enemy.Deck.Count < 5 && Enemy.Deck.Count < Bot.Deck.Count)
                {
                    AI.SelectCard(morphingJar);
                    return true;
                }
            }

            // Flip Magician of Faith for reuse
            ClientCard magician = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MagicianOfFaith && card.IsFaceup());
            if (magician != null && Bot.Graveyard.Any(card => card.IsSpell()))
            {
                // Prioritize recovering Book of Moon/Taiyou when close to winning
                if (opponentNearDeckOut && weCanSurvive)
                {
                    AI.SelectCard(magician);
                    return true;
                }
                // Otherwise flip if safe
                else if (Bot.Deck.Count > 10)
                {
                    AI.SelectCard(magician);
                    return true;
                }
            }

            // Stop opponent's attack
            if (Enemy.BattlingMonster != null)
            {
                AI.SelectCard(Enemy.BattlingMonster);
                return true;
            }

            return false;
        }

        private bool ActivateBookOfTaiyou()
        {
            // Flip Morphing Jar face-up to activate it again
            ClientCard morphingJar = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MorphingJar && card.IsFacedown());
            if (morphingJar != null)
            {
                AI.SelectCard(morphingJar);
                return true;
            }

            // Flip Magician of Faith for effect
            ClientCard magician = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MagicianOfFaith && card.IsFacedown());
            if (magician != null && Bot.Graveyard.Any(card => card.IsSpell()))
            {
                AI.SelectCard(magician);
                return true;
            }

            return false;
        }

        private bool ActivateMetamorphosis()
        {
            // Use on Morphing Jar or Cyber Jar to summon Last Warrior (lock)
            ClientCard jar = Bot.GetMonsters().FirstOrDefault(card =>
                (card.Id == CardId.MorphingJar || card.Id == CardId.CyberJar) && card.IsFaceup());

            if (jar != null && Bot.ExtraDeck.Any(card => card.Id == CardId.LastWarrior))
            {
                AI.SelectCard(CardId.LastWarrior);
                AI.SelectMaterials(jar);
                return true;
            }

            return false;
        }

        private bool ActivateCyberStein()
        {
            // Emergency beater: Pay 5000 LP to summon Blue-Eyes Ultimate Dragon
            if (Bot.LifePoints > 5000 && Enemy.LifePoints <= 4500)
            {
                AI.SelectCard(CardId.BlueEyesUltimateDragon);
                return true;
            }
            return false;
        }

        private bool ActivateMagicianOfFaith()
        {
            // Recover Book of Moon, Book of Taiyou, or draw spells
            if (Bot.Graveyard.Any(card => card.Id == CardId.BookOfMoon))
            {
                AI.SelectCard(CardId.BookOfMoon);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.BookOfTaiyou))
            {
                AI.SelectCard(CardId.BookOfTaiyou);
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

        private bool ActivateGoodGoblinHousekeeping()
        {
            // Draw and recycle deck
            return true;
        }

        private bool ActivateCeasefire()
        {
            // Burn damage when close to winning
            int damage = (Bot.GetMonsterCount() + Enemy.GetMonsterCount()) * 500;
            return damage >= Enemy.LifePoints || (Enemy.LifePoints <= 2000 && damage > 0);
        }
    }
}
