using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Bazoo Return", "AI_GOAT_BazooReturn", "Advanced")]
    public class GOATBazooReturnExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int BazooTheSoulEater = 40133511;
            public const int BlackLusterSoldier = 72989439;
            public const int Breaker = 71413901;
            public const int CyberJar = 34124316;
            public const int DDWarriorLady = 07572887;
            public const int Dekoichi = 87621407;
            public const int KingTigerWanghu = 83986578;
            public const int MagicalMerchant = 32362575;
            public const int MorphingJar = 33508719;
            public const int SkilledDarkMagician = 73752131;
            public const int ThunderDragon = 31786629;
            public const int TribeInfectingVirus = 33184167;

            public const int BookOfMoon = 14087893;
            public const int CardDestruction = 72892473;
            public const int DelinquentDuo = 44763025;
            public const int GracefulCharity = 79571449;
            public const int HeavyStorm = 19613556;
            public const int PotOfGreed = 55144522;
            public const int NoblemanOfCrossout = 71044499;

            public const int MirrorForce = 44095762;
            public const int ReturnFromTheDifferentDimension = 27174286;
            public const int RingOfDestruction = 83555666;
            public const int SkullLair = 06733059;
            public const int TorrentialTribute = 53582587;
            public const int TrapDustshoot = 64697231;
        }

        public GOATBazooReturnExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Draw and setup spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity);
            AddExecutor(ExecutorType.Activate, CardId.CardDestruction, ActivateCardDestruction);
            AddExecutor(ExecutorType.Activate, CardId.DelinquentDuo);
            AddExecutor(ExecutorType.Activate, CardId.ThunderDragon, ActivateThunderDragon);

            // Removal
            AddExecutor(ExecutorType.Activate, CardId.HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, CardId.BookOfMoon, ActivateBookOfMoon);
            AddExecutor(ExecutorType.Activate, CardId.NoblemanOfCrossout, DefaultNoblemanOfCrossout);

            // Monster summons
            AddExecutor(ExecutorType.SpSummon, CardId.BlackLusterSoldier, SummonBLS);
            AddExecutor(ExecutorType.Activate, CardId.BlackLusterSoldier, ActivateBLS);
            AddExecutor(ExecutorType.Summon, CardId.BazooTheSoulEater, SummonBazoo);
            AddExecutor(ExecutorType.Activate, CardId.BazooTheSoulEater, ActivateBazoo);
            AddExecutor(ExecutorType.Summon, CardId.Breaker);
            AddExecutor(ExecutorType.Activate, CardId.Breaker);
            AddExecutor(ExecutorType.Summon, CardId.KingTigerWanghu);
            AddExecutor(ExecutorType.Summon, CardId.MagicalMerchant);
            AddExecutor(ExecutorType.Activate, CardId.MagicalMerchant);
            AddExecutor(ExecutorType.Summon, CardId.Dekoichi);
            AddExecutor(ExecutorType.Activate, CardId.Dekoichi);
            AddExecutor(ExecutorType.Summon, CardId.DDWarriorLady);
            AddExecutor(ExecutorType.Activate, CardId.DDWarriorLady, DefaultDDWarriorLady);
            AddExecutor(ExecutorType.Summon, CardId.TribeInfectingVirus);
            AddExecutor(ExecutorType.Activate, CardId.TribeInfectingVirus, ActivateTribeInfectingVirus);
            AddExecutor(ExecutorType.Summon, CardId.MorphingJar);
            AddExecutor(ExecutorType.Summon, CardId.CyberJar);
            AddExecutor(ExecutorType.Summon, CardId.SkilledDarkMagician);

            // Flip effects
            AddExecutor(ExecutorType.Activate, CardId.MorphingJar);
            AddExecutor(ExecutorType.Activate, CardId.CyberJar);

            // Traps - Return from DD is the win condition
            AddExecutor(ExecutorType.Activate, CardId.ReturnFromTheDifferentDimension, ActivateReturnFromDD);
            AddExecutor(ExecutorType.Activate, CardId.TrapDustshoot, DefaultTrapDustshoot);
            AddExecutor(ExecutorType.Activate, CardId.SkullLair, ActivateSkullLair);
            AddExecutor(ExecutorType.Activate, CardId.TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, CardId.MirrorForce, DefaultMirrorForce);
            AddExecutor(ExecutorType.Activate, CardId.RingOfDestruction, ActivateRingOfDestruction);

            // Default
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        private bool ActivateThunderDragon()
        {
            // Discard Thunder Dragon to search 2 more, gives fuel for Bazoo banishing
            return true;
        }

        private bool ActivateCardDestruction()
        {
            // Mill deck to set up graveyard for Bazoo
            return Bot.Hand.Count > 0;
        }

        private bool ActivateBookOfMoon()
        {
            // Stop attacks or flip problematic monsters
            if (Enemy.BattlingMonster != null)
            {
                AI.SelectCard(Enemy.BattlingMonster);
                return true;
            }
            return false;
        }

        private bool SummonBLS()
        {
            int light = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
            int dark = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));
            return light > 0 && dark > 0;
        }

        private bool ActivateBLS()
        {
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

        private bool SummonBazoo()
        {
            // Summon Bazoo when we have monsters to banish
            return Bot.Graveyard.GetMonsterCount() >= 3;
        }

        private bool ActivateBazoo()
        {
            // Banish up to 3 monsters from grave to power up Bazoo
            if (Bot.Graveyard.GetMonsterCount() >= 3)
            {
                AI.SelectCard(Bot.Graveyard.GetMonsters().Take(3).ToArray());
                return true;
            }
            return false;
        }

        private bool ActivateTribeInfectingVirus()
        {
            // Use when opponent has multiple monsters
            if (Enemy.GetMonsterCount() >= 2 && Bot.Hand.Count > 1)
            {
                AI.SelectCard(Bot.Hand.OrderBy(card => card.Attack).First());
                return true;
            }
            return false;
        }

        private bool ActivateReturnFromDD()
        {
            // OTK condition: Summon all banished monsters for game
            int banishedCount = Bot.Banished.GetMonsterCount();

            // Calculate if we can OTK
            if (banishedCount >= 3)
            {
                int totalAttack = Bot.Banished.GetMonsters().Sum(card => card.Attack);

                // If we can deal lethal damage or close to it
                if (totalAttack >= Enemy.LifePoints || (totalAttack >= Enemy.LifePoints - 2000 && Enemy.GetMonsterCount() == 0))
                {
                    AI.SelectCard(Bot.Banished.GetMonsters().OrderByDescending(card => card.Attack).ToArray());
                    return true;
                }
            }
            return false;
        }

        private bool ActivateSkullLair()
        {
            // Remove opponent monsters by banishing our grave
            if (Enemy.GetMonsterCount() > 0 && Bot.Graveyard.GetMonsterCount() >= 2)
            {
                AI.SelectCard(Enemy.GetMonsters().OrderByDescending(card => card.Attack).First());
                AI.SelectNextCard(Bot.Graveyard.GetMonsters().OrderBy(card => card.Attack).Take(2).ToArray());
                return true;
            }
            return false;
        }

        private bool ActivateRingOfDestruction()
        {
            if (Enemy.GetMonsters().Any(card => card.Attack >= 1500))
            {
                ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                AI.SelectCard(target);
                return true;
            }
            return false;
        }
    }
}
