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
            // Equip to our strongest attacker
            if (Bot.GetMonsterCount() == 0)
                return false;

            // Calculate current OTK damage
            int currentDamage = CalculateOTKDamage();

            // If already lethal, don't waste equips (except protection)
            if (currentDamage >= Enemy.LifePoints)
            {
                // Only equip Ready for Intercepting (protection) if already lethal
                if (Card.Id == CardId.ReadyForIntercepting)
                {
                    ClientCard mataza = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MatazaTheZapper);
                    if (mataza != null)
                    {
                        AI.SelectCard(mataza);
                        return true;
                    }
                }
                return false; // Don't waste attack-boosting equips when already lethal
            }

            // Priority to Mataza (can attack twice)
            ClientCard target = Bot.GetMonsters().FirstOrDefault(card => card.Id == CardId.MatazaTheZapper);
            if (target == null)
            {
                // Otherwise equip to strongest attacker
                target = Bot.GetMonsters().OrderByDescending(card => card.Attack).FirstOrDefault();
            }

            if (target != null)
            {
                AI.SelectCard(target);
                return true;
            }

            return false;
        }

        private int CalculateOTKDamage()
        {
            // Calculate total damage from all attack-capable monsters
            int totalDamage = 0;

            foreach (ClientCard monster in Bot.GetMonsters())
            {
                if (monster.IsAttack())
                {
                    int monsterDamage = monster.Attack;

                    // Mataza the Zapper can attack twice
                    if (monster.Id == CardId.MatazaTheZapper)
                    {
                        monsterDamage *= 2;
                    }

                    // Check if monster has Fairy Meteor Crush (piercing damage)
                    bool hasPiercing = monster.EquipCards.Any(equip => equip.Id == CardId.FairyMeteorCrush);

                    // If has piercing and enemy has defense position monsters, guaranteed damage
                    if (hasPiercing && Enemy.GetMonsters().Any(m => m.IsDefense()))
                    {
                        totalDamage += monsterDamage; // Piercing ignores defense
                    }
                    // If enemy has no monsters, direct attack
                    else if (Enemy.GetMonsterCount() == 0)
                    {
                        totalDamage += monsterDamage;
                    }
                    // If enemy has monsters in attack position, calculate battle damage
                    else if (Enemy.GetMonsters().Any(m => m.IsAttack()))
                    {
                        // Assume we can attack over weakest enemy monster
                        ClientCard weakestEnemy = Enemy.GetMonsters().Where(m => m.IsAttack()).OrderBy(m => m.Attack).FirstOrDefault();
                        if (weakestEnemy != null && monster.Attack > weakestEnemy.Attack)
                        {
                            totalDamage += (monster.Attack - weakestEnemy.Attack);
                        }
                    }
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
