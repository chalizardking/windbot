using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Aggro Chaos", "AI_GOAT_AggroChaos", "Advanced")]
    public class GOATAggroChaosExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int BlackLusterSoldier = 72989439;
            public const int ChaosSorcerer = 09596126;
            public const int SinisterSerpent = 08131171;
            public const int Sangan = 26202165;
            public const int Breaker = 71413901;
            public const int DDAssailant = 70074904;
            public const int DDWarriorLady = 07572887;
            public const int BladeKnight = 39507162;
            public const int MysticTomato = 83011277;
            public const int NightAssailant = 25005816;
            public const int AsuraPriest = 02134346;
            public const int AirknightParshath = 18036057;
            public const int MagicianOfFaith = 31560081;

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
            public const int Scapegoat = 73915051;

            public const int CallOfTheHaunted = 97077563;
            public const int TorrentialTribute = 53582587;
            public const int MirrorForce = 44095762;
            public const int RingOfDestruction = 83555666;
            public const int SakuretsuArmor = 56120475;
        }

        public GOATAggroChaosExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Draw spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity);
            AddExecutor(ExecutorType.Activate, CardId.DelinquentDuo);

            // Removal spells - clear path for attacks
            AddExecutor(ExecutorType.Activate, CardId.HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, CardId.MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, CardId.SnatchSteal, DefaultSnatchSteal);
            AddExecutor(ExecutorType.Activate, CardId.SmashingGround, DefaultSmashingGround);
            AddExecutor(ExecutorType.Activate, CardId.NoblemanOfCrossout, DefaultNoblemanOfCrossout);
            AddExecutor(ExecutorType.Activate, CardId.BookOfMoon, ActivateBookOfMoon);

            // Support spells
            AddExecutor(ExecutorType.Activate, CardId.PrematureBurial, ActivatePrematureBurial);
            AddExecutor(ExecutorType.SpellSet, CardId.Scapegoat);
            AddExecutor(ExecutorType.Activate, CardId.Scapegoat, ActivateScapegoat);

            // Monster summons - Aggressive priority
            AddExecutor(ExecutorType.SpSummon, CardId.BlackLusterSoldier, SummonBLS);
            AddExecutor(ExecutorType.Activate, CardId.BlackLusterSoldier, ActivateBLS);
            AddExecutor(ExecutorType.SpSummon, CardId.ChaosSorcerer, SummonChaosSorcerer);
            AddExecutor(ExecutorType.Activate, CardId.ChaosSorcerer, ActivateChaosSorcerer);

            // Aggressive beaters
            AddExecutor(ExecutorType.Summon, CardId.AsuraPriest);
            AddExecutor(ExecutorType.Summon, CardId.BladeKnight, SummonBladeKnight);
            AddExecutor(ExecutorType.Summon, CardId.AirknightParshath);
            AddExecutor(ExecutorType.Summon, CardId.Breaker, SummonBreaker);
            AddExecutor(ExecutorType.Activate, CardId.Breaker);
            AddExecutor(ExecutorType.Summon, CardId.MysticTomato);
            AddExecutor(ExecutorType.Activate, CardId.MysticTomato, ActivateMysticTomato);
            AddExecutor(ExecutorType.Summon, CardId.MagicianOfFaith);
            AddExecutor(ExecutorType.Activate, CardId.MagicianOfFaith, ActivateMagicianOfFaith);
            AddExecutor(ExecutorType.Summon, CardId.DDWarriorLady);
            AddExecutor(ExecutorType.Activate, CardId.DDWarriorLady, DefaultDDWarriorLady);
            AddExecutor(ExecutorType.Summon, CardId.DDAssailant);
            AddExecutor(ExecutorType.Summon, CardId.Sangan);
            AddExecutor(ExecutorType.Summon, CardId.SinisterSerpent);

            // Flip effects
            AddExecutor(ExecutorType.Activate, CardId.Sangan, ActivateSangan);

            // Minimal traps - aggressive build
            AddExecutor(ExecutorType.Activate, CardId.TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, CardId.MirrorForce, DefaultMirrorForce);
            AddExecutor(ExecutorType.Activate, CardId.RingOfDestruction, ActivateRingOfDestruction);
            AddExecutor(ExecutorType.Activate, CardId.SakuretsuArmor, DefaultSakuretsuArmor);
            AddExecutor(ExecutorType.Activate, CardId.CallOfTheHaunted, ActivateCallOfTheHaunted);

            // Default repositions and sets
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
        }

        private bool ActivateScapegoat()
        {
            // Only use defensively when needed
            if (Duel.Player == 1 && Duel.Phase == DuelPhase.Battle && Bot.GetMonsterCount() == 0)
                return true;
            return false;
        }

        private bool ActivateBookOfMoon()
        {
            // Stop opponent's attack or flip for direct damage
            if (Enemy.BattlingMonster != null && Enemy.BattlingMonster.IsAttack())
            {
                AI.SelectCard(Enemy.BattlingMonster);
                return true;
            }

            // Flip Magician of Faith for reuse only if needed
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
            // Revive aggressive beaters
            if (Bot.Graveyard.Any(card => card.Id == CardId.BlackLusterSoldier))
            {
                AI.SelectCard(CardId.BlackLusterSoldier);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.AsuraPriest))
            {
                AI.SelectCard(CardId.AsuraPriest);
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

        private bool SummonBLS()
        {
            // Check if we can summon BLS
            int light = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
            int dark = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));
            return light > 0 && dark > 0;
        }

        private bool ActivateBLS()
        {
            // Aggressive lethal damage checking
            if (Card.Location == CardLocation.MonsterZone && !Card.IsDisabled())
            {
                if (Enemy.GetMonsterCount() > 0)
                {
                    // Calculate lethal damage scenarios
                    int blsAttack = Card.Attack;
                    int otherMonstersAttack = CalculateOtherMonstersAttack();
                    int enemyLP = Enemy.LifePoints;

                    // Scenario 1: Check if double attack (no effect) is lethal
                    int doubleAttackDamage = (blsAttack * 2) + otherMonstersAttack;
                    if (doubleAttackDamage >= enemyLP)
                    {
                        // DON'T use effect - double attack wins game
                        return false;
                    }

                    // Scenario 2: Check if banish + attack is lethal
                    int banishAttackDamage = blsAttack + otherMonstersAttack;
                    if (banishAttackDamage >= enemyLP)
                    {
                        // USE effect - removes blocker and wins game
                        ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                        AI.SelectCard(target);
                        return true;
                    }

                    // Scenario 3: Aggressive tempo - use effect more freely
                    ClientCard strongestEnemy = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();

                    // Aggro deck: Lower threshold (1500+ ATK = significant)
                    if (strongestEnemy.Attack >= 1500)
                    {
                        AI.SelectCard(strongestEnemy);
                        return true;
                    }

                    // If can beat it in battle, save effect
                    if (blsAttack > strongestEnemy.Attack)
                    {
                        return false;
                    }

                    // Can't beat it, use effect
                    AI.SelectCard(strongestEnemy);
                    return true;
                }
            }
            return false;
        }

        private int CalculateOtherMonstersAttack()
        {
            // Sum ATK of all Bot monsters except the current card
            int totalAttack = 0;
            foreach (ClientCard monster in Bot.GetMonsters())
            {
                if (monster != Card && monster.IsAttack())
                {
                    totalAttack += monster.Attack;
                }
            }
            return totalAttack;
        }

        private bool SummonChaosSorcerer()
        {
            // Check if we can summon Chaos Sorcerer
            int light = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
            int dark = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));
            return light > 0 && dark > 0;
        }

        private bool ActivateChaosSorcerer()
        {
            // Aggressive Chaos Sorcerer optimization
            if (Enemy.GetMonsters().Any(card => card.IsFaceup()))
            {
                int chaosAttack = Card.Attack; // 2300 ATK
                int otherMonstersAttack = CalculateOtherMonstersAttack();
                int enemyLP = Enemy.LifePoints;

                // Check if attacking without using effect is lethal
                int attackDamage = chaosAttack + otherMonstersAttack;
                if (attackDamage >= enemyLP)
                {
                    // Don't use effect - attacking wins game
                    return false;
                }

                // Check if using effect clears path for lethal
                int banishDamage = otherMonstersAttack;
                if (banishDamage >= enemyLP)
                {
                    // USE effect - clears blocker and other monsters win game
                    ClientCard target = Enemy.GetMonsters().Where(card => card.IsFaceup()).OrderByDescending(card => card.Attack).First();
                    AI.SelectCard(target);
                    return true;
                }

                // Aggressive Chaos deck: Lower threshold (1500+ ATK = significant for tempo)
                ClientCard strongestEnemy = Enemy.GetMonsters().Where(card => card.IsFaceup()).OrderByDescending(card => card.Attack).First();
                if (strongestEnemy.Attack >= 1500)
                {
                    AI.SelectCard(strongestEnemy);
                    return true;
                }

                // If Chaos Sorcerer can beat it in battle, save effect
                if (chaosAttack > strongestEnemy.Attack)
                {
                    return false;
                }

                // Can't beat it, use effect
                AI.SelectCard(strongestEnemy);
                return true;
            }
            return false;
        }

        private bool SummonBladeKnight()
        {
            // Summon when opponent has low hand for 1900 ATK and piercing
            return Enemy.Hand.Count <= 2 || Enemy.GetMonsterCount() == 0;
        }

        private bool SummonBreaker()
        {
            // Summon to pop backrow and clear path for attacks
            return Enemy.GetSpellCount() > 0;
        }

        private bool ActivateMysticTomato()
        {
            // Search for more aggression
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

        private bool ActivateMagicianOfFaith()
        {
            // Recover aggressive spells
            if (Bot.Graveyard.Any(card => card.Id == CardId.PotOfGreed))
            {
                AI.SelectCard(CardId.PotOfGreed);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.SmashingGround))
            {
                AI.SelectCard(CardId.SmashingGround);
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
            // Search for aggressive monsters
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
            // Revive aggressive beaters
            if (Bot.Graveyard.Any(card => card.Id == CardId.BlackLusterSoldier))
            {
                AI.SelectCard(CardId.BlackLusterSoldier);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.AsuraPriest))
            {
                AI.SelectCard(CardId.AsuraPriest);
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
