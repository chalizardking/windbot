using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Ben Kei OTK", "AI_GOAT_BenKeiOTK", "Advanced")]
    public class GOATBenKeiOTKExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int MatazaTheZapper = 22609617;
            public const int GoblinAttackForce = 78658564;
            public const int SasukeSamurai3 = 77379481;

            public const int PotOfGreed = 55144522;
            public const int GracefulCharity = 79571449;
            public const int HeavyStorm = 19613556;
            public const int MysticalSpaceTyphoon = 05318639;
            public const int UnitedWeStand = 56747793;
            public const int MagePower = 83746708;
            public const int AxeOfDespair = 40619825;
            public const int FairyMeteorCrush = 97687912;
            public const int BigBangShot = 61127349;
            public const int KishidoSpirit = 60519422;
            public const int LightningBlade = 55226821;
            public const int ReinforcementOfTheArmy = 32807846;
            public const int FusionSwordMurasameBlade = 37580756;

            public const int RoyalDecree = 51452091;
            public const int ReadyForIntercepting = 31785398;
        }

        public GOATBenKeiOTKExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Draw spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity);

            // Removal - clear path for OTK
            AddExecutor(ExecutorType.Activate, CardId.HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, CardId.MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);

            // Trap lockdown
            AddExecutor(ExecutorType.Activate, CardId.RoyalDecree, ActivateRoyalDecree);

            // Search for attackers
            AddExecutor(ExecutorType.Activate, CardId.ReinforcementOfTheArmy, ActivateReinforcement);

            // Equip spells - activate when we have an attacker
            AddExecutor(ExecutorType.Activate, CardId.UnitedWeStand, ActivateEquipSpell);
            AddExecutor(ExecutorType.Activate, CardId.MagePower, ActivateEquipSpell);
            AddExecutor(ExecutorType.Activate, CardId.AxeOfDespair, ActivateEquipSpell);
            AddExecutor(ExecutorType.Activate, CardId.FairyMeteorCrush, ActivateEquipSpell);
            AddExecutor(ExecutorType.Activate, CardId.BigBangShot, ActivateEquipSpell);
            AddExecutor(ExecutorType.Activate, CardId.KishidoSpirit, ActivateEquipSpell);
            AddExecutor(ExecutorType.Activate, CardId.LightningBlade, ActivateEquipSpell);
            AddExecutor(ExecutorType.Activate, CardId.FusionSwordMurasameBlade, ActivateEquipSpell);

            // Monster summons
            AddExecutor(ExecutorType.Summon, CardId.MatazaTheZapper);
            AddExecutor(ExecutorType.Summon, CardId.GoblinAttackForce);
            AddExecutor(ExecutorType.Summon, CardId.SasukeSamurai3, SummonSasukeSamurai);

            // Protection trap
            AddExecutor(ExecutorType.Activate, CardId.ReadyForIntercepting);

            // Default repositions
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool ActivateRoyalDecree()
        {
            // Activate before attack phase to shut down opponent traps
            if (Bot.GetMonsterCount() > 0 && Bot.Hand.Any(card => card.IsSpell()))
            {
                return true;
            }
            return false;
        }

        private bool ActivateReinforcement()
        {
            // Search Mataza (can attack twice) for OTK
            if (Bot.Deck.Any(card => card.Id == CardId.MatazaTheZapper))
            {
                AI.SelectCard(CardId.MatazaTheZapper);
                return true;
            }
            if (Bot.Deck.Any(card => card.Id == CardId.SasukeSamurai3))
            {
                AI.SelectCard(CardId.SasukeSamurai3);
                return true;
            }
            return false;
        }

        private bool ActivateEquipSpell()
        {
            // Smart equip: Only equip if helps with OTK or board control
            if (Bot.GetMonsterCount() == 0)
                return false;

            // Priority to Mataza (can attack twice)
            ClientCard mataza = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MatazaTheZapper);
            if (mataza != null)
            {
                // Check if we can already OTK without this equip
                int currentOtkDamage = CalculateOTKDamage();
                if (currentOtkDamage >= Enemy.LifePoints)
                {
                    // Already have lethal - don't waste equips unless it's protection
                    if (Card.Id == CardId.ReadyForIntercepting || Card.Id == CardId.KishidoSpirit)
                    {
                        AI.SelectCard(mataza);
                        return true;
                    }
                    return false;
                }

                // We need more damage - equip to Mataza
                AI.SelectCard(mataza);
                return true;
            }

            // Otherwise equip to strongest attacker
            ClientCard attacker = Bot.GetMonsters().OrderByDescending(card => card.Attack).FirstOrDefault();
            if (attacker != null)
            {
                int currentOtkDamage = CalculateOTKDamage();
                if (currentOtkDamage >= Enemy.LifePoints)
                {
                    // Already lethal - only equip protection
                    if (Card.Id == CardId.ReadyForIntercepting || Card.Id == CardId.KishidoSpirit)
                    {
                        AI.SelectCard(attacker);
                        return true;
                    }
                    return false;
                }

                AI.SelectCard(attacker);
                return true;
            }

            return false;
        }

        private int CalculateOTKDamage()
        {
            // Calculate total damage we can deal this turn
            int totalDamage = 0;

            foreach (var monster in Bot.GetMonsters())
            {
                if (!monster.IsAttack()) continue;

                int monsterDamage = monster.Attack;

                // Check if monster has Fairy Meteor Crush (piercing)
                bool hasPiercing = false;
                if (monster.EquipCards != null)
                {
                    foreach (var equip in monster.EquipCards)
                    {
                        if (equip.Id == CardId.FairyMeteorCrush)
                        {
                            hasPiercing = true;
                            break;
                        }
                    }
                }

                // Calculate damage against enemy field
                if (Enemy.GetMonsterCount() == 0)
                {
                    // Direct attack
                    totalDamage += monsterDamage;
                }
                else
                {
                    // Estimate damage (assume we can attack over/through weakest monsters)
                    var weakestEnemy = Enemy.GetMonsters().OrderBy(card => card.GetDefensePower()).FirstOrDefault();
                    if (weakestEnemy != null)
                    {
                        if (hasPiercing)
                        {
                            // Piercing damage
                            int piercingDamage = monsterDamage;
                            if (weakestEnemy.IsDefense())
                                piercingDamage += (monsterDamage - weakestEnemy.Defense);
                            totalDamage += Math.Max(monsterDamage, piercingDamage);
                        }
                        else if (monsterDamage > weakestEnemy.Attack)
                        {
                            // Can attack over
                            totalDamage += monsterDamage;
                        }
                    }
                }

                // Mataza can attack twice
                if (monster.Id == CardId.MatazaTheZapper)
                {
                    totalDamage += monsterDamage;
                }
            }

            return totalDamage;
        }

        private bool SummonSasukeSamurai()
        {
            // Summon when opponent has face-down monsters (can attack directly)
            return Enemy.GetMonsters().Any(card => card.IsFacedown());
        }

        public override bool OnPreBattleBetween(ClientCard attacker, ClientCard defender)
        {
            // Calculate if we can OTK
            if (attacker.Controller == 0 && defender != null)
            {
                // If attacker can kill in one hit, go for it
                int damage = attacker.Attack - (defender.IsAttack() ? defender.Attack : 0);
                if (damage >= Enemy.LifePoints)
                    return true;
            }
            return base.OnPreBattleBetween(attacker, defender);
        }
    }
}
