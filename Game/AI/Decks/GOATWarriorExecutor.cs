using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Warrior", "AI_GOAT_Warrior", "Advanced")]
    public class GOATWarriorExecutor : DefaultExecutor
    {
        // Monsters
        public const int BlackLusterSoldier = 72989439;
        public const int BladeKnight = 39507162;
        public const int Sinister = 71413901;
        public const int ChaosSorcerer = 09596126;
        public const int DDWarriorLady = 07572887;
        public const int DonZaloog = 76922029;
        public const int Exiled = 74131780;
        public const int Kycoo = 88240808;
        public const int Reflect = 47507260;
        public const int Sanagan = 04041838;
        public const int SpiritReaper = 73752131;
        public const int Regeneration = 33184167;

        // Spells
        public const int HeavyStorm = 19613556;
        public const int MysticalSpaceTyphoon = 05318639;
        public const int NobleMansOfCrossout = 71044499;
        public const int PotOfGreed = 55144522;
        public const int Raigeki = 70828912;
        public const int Reinforcement = 32807846;
        public const int PrematureBurial = 45986603;
        public const int SnatchSteal = 44095762;
        public const int BrainControl = 60082869;

        // Traps
        public const int CallOfTheHaunted = 41420027;
        public const int RingOfDestruction = 83555666;
        public const int SakuretsuArmor = 56120475;
        public const int TorrentialTribute = 53582587;
        public const int TrapDustshoot = 64697231;
        public const int Solemn = 97077563;

        public GOATWarriorExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Priority: Chaos monsters
            AddExecutor(ExecutorType.Summon, BlackLusterSoldier);
            AddExecutor(ExecutorType.Summon, ChaosSorcerer, ChaosSummon);
            AddExecutor(ExecutorType.Summon, BladeKnight);
            AddExecutor(ExecutorType.Summon, DonZaloog, DonZaloogSummon);
            AddExecutor(ExecutorType.Summon, DDWarriorLady);
            AddExecutor(ExecutorType.Summon, Kycoo);

            // Monster effects
            AddExecutor(ExecutorType.Activate, BlackLusterSoldier, BLSEffect);
            AddExecutor(ExecutorType.Activate, ChaosSorcerer, ChaosSorcererEffect);
            AddExecutor(ExecutorType.Activate, DonZaloog);
            AddExecutor(ExecutorType.Activate, Sanagan);

            // Key spells
            AddExecutor(ExecutorType.Activate, Reinforcement, ActivateReinforcement);
            AddExecutor(ExecutorType.Activate, PotOfGreed);
            AddExecutor(ExecutorType.Activate, Raigeki, DefaultRaigeki);
            AddExecutor(ExecutorType.Activate, HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, MysticalSpaceTyphoon, DefaultMysticalSpaceTyphoon);
            AddExecutor(ExecutorType.Activate, SnatchSteal, DefaultSnatchSteal);
            AddExecutor(ExecutorType.Activate, BrainControl, DefaultBrainControl);
            AddExecutor(ExecutorType.Activate, PrematureBurial, DefaultPrematureBurial);
            AddExecutor(ExecutorType.Activate, NobleMansOfCrossout, DefaultBookOfMoon);

            // Traps
            AddExecutor(ExecutorType.Activate, TrapDustshoot, DefaultTrapDustshoot);
            AddExecutor(ExecutorType.Activate, TorrentialTribute, DefaultTorrentialTribute);
            AddExecutor(ExecutorType.Activate, CallOfTheHaunted, DefaultCallOfTheHaunted);
            AddExecutor(ExecutorType.Activate, RingOfDestruction, DefaultRingOfDestruction);
            AddExecutor(ExecutorType.Activate, SakuretsuArmor, DefaultSakuretsuArmor);
            AddExecutor(ExecutorType.Activate, Solemn, DefaultSolemnJudgment);

            // General summons
            AddExecutor(ExecutorType.Summon, Sinister);
            AddExecutor(ExecutorType.MonsterSet, SpiritReaper);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool ChaosSummon()
        {
            // Can summon Chaos Sorcerer if we have light and dark in grave
            int lightCount = Bot.Graveyard.Count(c => c.HasAttribute(CardAttribute.Light));
            int darkCount = Bot.Graveyard.Count(c => c.HasAttribute(CardAttribute.Dark));
            return lightCount > 0 && darkCount > 0;
        }

        private bool BLSEffect()
        {
            if (Card.Location == CardLocation.MonsterZone)
            {
                // Remove opponent's monster or attack twice
                if (Enemy.GetMonsterCount() > 0 && !Card.IsDisabled())
                    return true;
            }
            return false;
        }

        private bool ChaosSorcererEffect()
        {
            // Banish opponent's face-up monster
            return Enemy.GetMonsters().Any(c => c.IsFaceup());
        }

        private bool DonZaloogSummon()
        {
            // Summon Don Zaloog when field is clear for discard effect
            return Enemy.GetMonsterCount() == 0 && Enemy.Hand.Count > 0;
        }

        private bool ActivateReinforcement()
        {
            // Search for best warrior depending on situation
            if (Bot.HasInDeck(DDWarriorLady) && Enemy.GetMonsters().Any(c => c.Attack >= 2000))
            {
                AI.SelectCard(DDWarriorLady);
                return true;
            }
            if (Bot.HasInDeck(BladeKnight))
            {
                AI.SelectCard(BladeKnight);
                return true;
            }
            if (Bot.HasInDeck(DonZaloog) && Enemy.GetMonsterCount() == 0)
            {
                AI.SelectCard(DonZaloog);
                return true;
            }
            return Bot.HasInDeck(Kycoo);
        }
    }
}
