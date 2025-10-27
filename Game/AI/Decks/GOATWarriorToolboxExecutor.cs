using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Warrior Toolbox", "AI_GOAT_WarriorToolbox", "Advanced")]
    public class GOATWarriorToolboxExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int BlackLusterSoldier = 72989439;
            public const int SinisterSerpent = 08131171;
            public const int Sangan = 26202165;
            public const int MorphingJar = 33508719;
            public const int ExiledForce = 74131780;
            public const int DDWarriorLady = 07572887;
            public const int DDAssailant = 70074904;
            public const int MysticSwordsmanLV2 = 47507260;
            public const int BladeKnight = 39507162;
            public const int ZombyratheDark = 88472456;
            public const int GoblinAttackForce = 78658564;
            public const int CommandKnight = 10375182;
            public const int MaraudingCaptain = 02460565;
            public const int MysticTomato = 83011277;

            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int DelinquentDuo = 44763025;
            public const int SnatchSteal = 45986603;
            public const int HeavyStorm = 19613556;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int PrematureBurial = 70828912;
            public const int NoblemanOfCrossout = 71044499;
            public const int ReinforcementOfTheArmy = 32807846;
            public const int BookOfMoon = 14087893;
            public const int SmashingGround = 97169186;
            public const int Scapegoat = 73915051;

            public const int CallOfTheHaunted = 97077563;
            public const int TorrentialTribute = 53582587;
            public const int MirrorForce = 44095762;
            public const int RingOfDestruction = 83555666;
            public const int SakuretsuArmor = 56120475;
            public const int DustTornado = 60082869;
        }

        public GOATWarriorToolboxExecutor(GameAI ai, Duel duel)
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

            // Key card - Reinforcement of the Army
            AddExecutor(ExecutorType.Activate, CardId.ReinforcementOfTheArmy, ActivateReinforcement);

            // Support spells
            AddExecutor(ExecutorType.Activate, CardId.PrematureBurial, ActivatePrematureBurial);
            AddExecutor(ExecutorType.SpellSet, CardId.Scapegoat);
            AddExecutor(ExecutorType.Activate, CardId.Scapegoat, ActivateScapegoat);

            // Monster summons - Special summons first
            AddExecutor(ExecutorType.SpSummon, CardId.BlackLusterSoldier, SummonBLS);
            AddExecutor(ExecutorType.Activate, CardId.BlackLusterSoldier, ActivateBLS);

            // Marauding Captain combo
            AddExecutor(ExecutorType.Summon, CardId.MaraudingCaptain, SummonMaraudingCaptain);
            AddExecutor(ExecutorType.Activate, CardId.MaraudingCaptain, ActivateMaraudingCaptain);

            // Other warrior summons
            AddExecutor(ExecutorType.Summon, CardId.CommandKnight, SummonCommandKnight);
            AddExecutor(ExecutorType.Summon, CardId.BladeKnight, SummonBladeKnight);
            AddExecutor(ExecutorType.Summon, CardId.MysticSwordsmanLV2);
            AddExecutor(ExecutorType.Summon, CardId.ExiledForce, SummonExiledForce);
            AddExecutor(ExecutorType.Activate, CardId.ExiledForce, ActivateExiledForce);
            AddExecutor(ExecutorType.Summon, CardId.DDWarriorLady);
            AddExecutor(ExecutorType.Activate, CardId.DDWarriorLady, DefaultDDWarriorLady);
            AddExecutor(ExecutorType.Summon, CardId.DDAssailant);
            AddExecutor(ExecutorType.Summon, CardId.GoblinAttackForce);
            AddExecutor(ExecutorType.Summon, CardId.ZombyratheDark);
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

        private bool ActivateReinforcement()
        {
            // Search best warrior for situation
            // Priority: Exiled Force for removal > D.D. Warrior Lady vs big monsters > Blade Knight for aggro

            if (Enemy.GetMonsterCount() > 0 && Bot.Deck.Any(card => card.Id == CardId.ExiledForce))
            {
                ClientCard threat = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                if (threat.Attack >= 2000 && !Bot.Hand.Any(card => card.Id == CardId.ExiledForce))
                {
                    AI.SelectCard(CardId.ExiledForce);
                    return true;
                }
            }

            if (Enemy.GetMonsters().Any(card => card.Attack >= 2000) && Bot.Deck.Any(card => card.Id == CardId.DDWarriorLady))
            {
                if (!Bot.Hand.Any(card => card.Id == CardId.DDWarriorLady))
                {
                    AI.SelectCard(CardId.DDWarriorLady);
                    return true;
                }
            }

            if (Bot.Deck.Any(card => card.Id == CardId.BladeKnight) && !Bot.Hand.Any(card => card.Id == CardId.BladeKnight))
            {
                if (Enemy.Hand.Count <= 2)
                {
                    AI.SelectCard(CardId.BladeKnight);
                    return true;
                }
            }

            if (Bot.Deck.Any(card => card.Id == CardId.MaraudingCaptain) && !Bot.Hand.Any(card => card.Id == CardId.MaraudingCaptain))
            {
                AI.SelectCard(CardId.MaraudingCaptain);
                return true;
            }

            // Default to searching any warrior we don't have
            if (Bot.Deck.Any(card => card.Id == CardId.CommandKnight))
            {
                AI.SelectCard(CardId.CommandKnight);
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

        private bool ActivateBookOfMoon()
        {
            // Stop opponent's attack
            if (Enemy.BattlingMonster != null && Enemy.BattlingMonster.IsAttack())
            {
                AI.SelectCard(Enemy.BattlingMonster);
                return true;
            }
            return false;
        }

        private bool ActivatePrematureBurial()
        {
            // Revive BLS or warriors
            if (Bot.Graveyard.Any(card => card.Id == CardId.BlackLusterSoldier))
            {
                AI.SelectCard(CardId.BlackLusterSoldier);
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
            // Check if we can properly summon BLS
            int light = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
            int dark = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));
            return light > 0 && dark > 0;
        }

        private bool ActivateBLS()
        {
            // Banish key threats or attack twice for game
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

                    // Scenario 3: Not lethal - make smart board control decision
                    ClientCard strongestEnemy = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();

                    // Only banish significant threats (2000+ ATK)
                    if (strongestEnemy.Attack >= 2000)
                    {
                        AI.SelectCard(strongestEnemy);
                        return true;
                    }

                    // If threat is weak, check if we can beat it in battle
                    if (blsAttack > strongestEnemy.Attack)
                    {
                        // Can beat it by attacking, save effect for later
                        return false;
                    }

                    // Can't beat it by attacking, use effect
                    AI.SelectCard(strongestEnemy);
                    return true;
                }
            }
            return false;
        }

        private int CalculateOtherMonstersAttack()
        {
            // Sum ATK of all Bot monsters except BLS
            int totalAttack = 0;
            foreach (ClientCard monster in Bot.GetMonsters())
            {
                if (monster.Id != CardId.BlackLusterSoldier && monster.IsAttack())
                {
                    totalAttack += monster.Attack;
                }
            }
            return totalAttack;
        }

        private bool SummonMaraudingCaptain()
        {
            // Summon when we have another warrior to special summon
            return Bot.Hand.Count(card => card.IsMonster() && card.HasType(CardType.Warrior) && card.Level <= 4) >= 2;
        }

        private bool ActivateMaraudingCaptain()
        {
            // Special summon another warrior
            ClientCard warrior = Bot.Hand.Where(card => card.IsMonster() && card.HasType(CardType.Warrior) && card.Level <= 4 && card.Id != CardId.MaraudingCaptain)
                                         .OrderByDescending(card => card.Attack).FirstOrDefault();
            if (warrior != null)
            {
                AI.SelectCard(warrior);
                return true;
            }
            return false;
        }

        private bool SummonCommandKnight()
        {
            // Summon when we have other warriors
            return Bot.GetMonsters().Any(card => card.HasType(CardType.Warrior));
        }

        private bool SummonBladeKnight()
        {
            // Summon when opponent has low hand for 1900 ATK
            return Enemy.Hand.Count <= 2;
        }

        private bool SummonExiledForce()
        {
            // Summon when we need to remove a threat
            return Enemy.GetMonsters().Any(card => card.Attack >= 2000);
        }

        private bool ActivateExiledForce()
        {
            // Tribute to destroy problem monsters
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
            // Search for D.D. Assailant or Sinister Serpent
            if (Bot.Deck.Any(card => card.Id == CardId.DDAssailant))
            {
                AI.SelectCard(CardId.DDAssailant);
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
            if (Bot.Deck.Any(card => card.Id == CardId.ExiledForce))
            {
                AI.SelectCard(CardId.ExiledForce);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.MysticTomato))
            {
                AI.SelectCard(CardId.MysticTomato);
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
            // Revive best warrior
            if (Bot.Graveyard.Any(card => card.Id == CardId.BlackLusterSoldier))
            {
                AI.SelectCard(CardId.BlackLusterSoldier);
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
