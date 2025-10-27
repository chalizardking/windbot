using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Burn Stall", "AI_GOAT_BurnStall", "Advanced")]
    public class GOATBurnStallExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int MorphingJar = 33508719;
            public const int Sangan = 26202165;
            public const int SpiritReaper = 23205979;
            public const int StealthBird = 02638770;
            public const int MagicianOfFaith = 31560081;

            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int HeavyStorm = 19613556;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int MessengerOfPeace = 44656491;
            public const int SwordsOfRevealingLight = 72302403;
            public const int WaveMotionCannon = 38992735;
            public const int LevelLimitAreaB = 03136426;
            public const int BookOfMoon = 14087893;
            public const int Scapegoat = 73915051;

            public const int MirrorForce = 44095762;
            public const int TorrentialTribute = 53582587;
            public const int RingOfDestruction = 83555666;
            public const int Ceasefire = 36468556;
            public const int GravityBind = 85742772;
            public const int SecretBarrel = 27053506;
            public const int JustDesserts = 24068492;
        }

        public GOATBurnStallExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Stall cards - activate immediately
            AddExecutor(ExecutorType.Activate, CardId.MessengerOfPeace, ActivateMessengerOfPeace);
            AddExecutor(ExecutorType.Activate, CardId.SwordsOfRevealingLight, ActivateSwordsOfRevealingLight);
            AddExecutor(ExecutorType.Activate, CardId.LevelLimitAreaB);
            AddExecutor(ExecutorType.Activate, CardId.GravityBind);

            // Wave-Motion Cannon - win condition
            AddExecutor(ExecutorType.Activate, CardId.WaveMotionCannon, ActivateWaveMotionCannon);

            // Draw spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity);

            // Removal - only when necessary
            AddExecutor(ExecutorType.Activate, CardId.HeavyStorm, ActivateHeavyStorm);
            AddExecutor(ExecutorType.Activate, CardId.MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, CardId.BookOfMoon, ActivateBookOfMoon);
            AddExecutor(ExecutorType.SpellSet, CardId.Scapegoat);
            AddExecutor(ExecutorType.Activate, CardId.Scapegoat, ActivateScapegoat);

            // Monster summons
            AddExecutor(ExecutorType.Summon, CardId.StealthBird);
            AddExecutor(ExecutorType.Activate, CardId.StealthBird, ActivateStealthBird);
            AddExecutor(ExecutorType.Summon, CardId.SpiritReaper);
            AddExecutor(ExecutorType.Summon, CardId.MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfFaith, ActivateMagicianOfFaith);
            AddExecutor(ExecutorType.Summon, CardId.MorphingJar);
            AddExecutor(ExecutorType.Summon, CardId.Sangan);

            // Flip effects
            AddExecutor(ExecutorType.Activate, CardId.MorphingJar);
            AddExecutor(ExecutorType.Activate, CardId.Sangan, ActivateSangan);

            // Burn traps
            AddExecutor(ExecutorType.Activate, CardId.SecretBarrel, ActivateSecretBarrel);
            AddExecutor(ExecutorType.Activate, CardId.JustDesserts, ActivateJustDesserts);
            AddExecutor(ExecutorType.Activate, CardId.Ceasefire, ActivateCeasefire);
            AddExecutor(ExecutorType.Activate, CardId.RingOfDestruction, ActivateRingOfDestruction);

            // Defense traps
            AddExecutor(ExecutorType.Activate, CardId.TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, CardId.MirrorForce, DefaultMirrorForce);

            // Default repositions and sets
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        public override bool OnSelectHand()
        {
            // Always go first to set up stall
            return true;
        }

        private bool ActivateMessengerOfPeace()
        {
            // Activate to stall opponent's big monsters
            if (Enemy.GetMonsters().Any(card => card.Attack >= 1500))
                return true;
            if (Duel.Turn <= 3)
                return true;
            return false;
        }

        private bool ActivateSwordsOfRevealingLight()
        {
            // Activate when opponent has monsters
            return Enemy.GetMonsterCount() > 0;
        }

        private bool ActivateWaveMotionCannon()
        {
            // Set Wave-Motion Cannon early
            // Don't send to grave unless it will win the game
            if (Card.Location == CardLocation.SpellZone)
            {
                // Calculate damage (1000 per turn it's been on field)
                int turnsActive = Card.Counter; // Assuming counter tracks turns
                int damage = turnsActive * 1000;

                // Only send to grave if it will win or if we're at 4+ turns
                if (damage >= Enemy.LifePoints || turnsActive >= 4)
                {
                    return true;
                }
                return false;
            }
            return true; // Activate/set initially
        }

        private bool ActivateHeavyStorm()
        {
            // Only use if opponent has much more backrow than us
            return Enemy.GetSpellCount() >= Bot.GetSpellCount() + 2;
        }

        private bool ActivateBookOfMoon()
        {
            // Stop opponent's attack or flip Magician of Faith
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

        private bool ActivateScapegoat()
        {
            // Activate for defense
            if (Duel.Phase == DuelPhase.End && Duel.Player != 0)
                return true;
            if (Duel.Player == 1 && Duel.Phase == DuelPhase.Battle && Bot.GetMonsterCount() == 0)
                return true;
            return false;
        }

        private bool ActivateStealthBird()
        {
            // Flip for 1000 burn damage, return to hand, repeat
            if (Card.IsFaceup())
            {
                // Return to hand
                return true;
            }
            return false;
        }

        private bool ActivateMagicianOfFaith()
        {
            // Recover Wave-Motion Cannon or stall cards
            if (Bot.Graveyard.Any(card => card.Id == CardId.WaveMotionCannon))
            {
                AI.SelectCard(CardId.WaveMotionCannon);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.MessengerOfPeace))
            {
                AI.SelectCard(CardId.MessengerOfPeace);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.SwordsOfRevealingLight))
            {
                AI.SelectCard(CardId.SwordsOfRevealingLight);
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
            // Search for Stealth Bird or Magician of Faith
            if (Bot.Deck.Any(card => card.Id == CardId.StealthBird))
            {
                AI.SelectCard(CardId.StealthBird);
                return true;
            }
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

        private bool ActivateSecretBarrel()
        {
            // Burn for number of cards opponent controls (cards in hand + field)
            int damage = (Enemy.Hand.Count + Enemy.GetMonsterCount() + Enemy.GetSpellCount()) * 200;
            return damage >= 1000 || damage >= Enemy.LifePoints;
        }

        private bool ActivateJustDesserts()
        {
            // Burn for monsters opponent controls (300 per monster)
            int damage = Enemy.GetMonsterCount() * 500;
            return damage >= 1000 || damage >= Enemy.LifePoints;
        }

        private bool ActivateCeasefire()
        {
            // Burn for effect monsters (500 per effect monster)
            int botMonsters = Bot.GetMonsterCount();
            int enemyMonsters = Enemy.GetMonsterCount();
            int damage = (botMonsters + enemyMonsters) * 500;

            return damage >= Enemy.LifePoints || (Enemy.LifePoints <= 2000 && damage > 0);
        }

        private bool ActivateRingOfDestruction()
        {
            // Use on high-ATK monsters for burn damage
            if (Enemy.GetMonsters().Any(card => card.Attack >= 1500))
            {
                ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                if (target.Attack >= Enemy.LifePoints)
                {
                    AI.SelectCard(target);
                    return true;
                }
            }
            return false;
        }

        public override CardPosition OnSelectPosition(int cardId, IList<CardPosition> positions)
        {
            // Set Stealth Bird face-down to flip again
            if (cardId == CardId.StealthBird)
                return CardPosition.FaceDownDefence;

            return base.OnSelectPosition(cardId, positions);
        }

        public override bool OnSelectYesNo(long desc)
        {
            // Pay 100 LP for Messenger of Peace each turn
            if (desc == Util.GetStringId(CardId.MessengerOfPeace, 0))
                return Bot.LifePoints > 500;

            return base.OnSelectYesNo(desc);
        }
    }
}
