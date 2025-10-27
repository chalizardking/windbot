using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Monarch", "AI_GOAT_Monarch", "Advanced")]
    public class GOATMonarchExecutor : DefaultExecutor
    {
        // Monsters
        public const int Sinister = 71413901;
        public const int Airknight = 74713516;
        public const int Dekoichi = 87621407;
        public const int DekoichiOrAirknight = 31560081;
        public const int Mobius = 04929256;
        public const int SinisterSerpent = 26202165;
        public const int DDAssailant = 08131171;
        public const int Thestalos = 26205777;
        public const int HeavyMech = 14087893;

        // Spells
        public const int HeavyStorm = 19613556;
        public const int Metamorphosis = 46411259;
        public const int NobleMansOfCrossout = 71044499;
        public const int PotOfGreed = 55144522;
        public const int Scapegoat = 73915051;
        public const int PrematureBurial = 45986603;
        public const int SoulExchange = 68005187;
        public const int SnatchSteal = 44095762;

        // Traps
        public const int RingOfDestruction = 83555666;
        public const int SakuretsuArmor = 56120475;
        public const int CallOfTheHaunted = 41420027;

        public GOATMonarchExecutor(GameAI ai, Duel duel) : base(ai, duel)
        {
            // Priority: Tribute summon Monarchs
            AddExecutor(ExecutorType.Activate, SoulExchange, ActivateSoulExchange);
            AddExecutor(ExecutorType.Summon, Thestalos, TributeSummonMonarch);
            AddExecutor(ExecutorType.Summon, Mobius, TributeSummonMonarch);

            // Monster effects
            AddExecutor(ExecutorType.Activate, Thestalos, ThestalosEffect);
            AddExecutor(ExecutorType.Activate, Mobius, MobiusEffect);
            AddExecutor(ExecutorType.Activate, Airknight);
            AddExecutor(ExecutorType.Activate, Dekoichi);

            // Key combo pieces
            AddExecutor(ExecutorType.Activate, Scapegoat, DefaultScapegoat);
            AddExecutor(ExecutorType.Activate, Metamorphosis, ActivateMetamorphosis);

            // Spells
            AddExecutor(ExecutorType.Activate, PotOfGreed);
            AddExecutor(ExecutorType.Activate, HeavyStorm, DefaultHeavyStorm);
            AddExecutor(ExecutorType.Activate, SnatchSteal, DefaultSnatchSteal);
            AddExecutor(ExecutorType.Activate, PrematureBurial, DefaultPrematureBurial);
            AddExecutor(ExecutorType.Activate, NobleMansOfCrossout, DefaultBookOfMoon);

            // Traps
            AddExecutor(ExecutorType.Activate, CallOfTheHaunted, CallOfTheHauntedForTribute);
            AddExecutor(ExecutorType.Activate, RingOfDestruction, DefaultRingOfDestruction);
            AddExecutor(ExecutorType.Activate, SakuretsuArmor, DefaultSakuretsuArmor);

            // General summons
            AddExecutor(ExecutorType.Summon, Airknight);
            AddExecutor(ExecutorType.Summon, Dekoichi);
            AddExecutor(ExecutorType.Summon, Sinister);
            AddExecutor(ExecutorType.SpellSet, DefaultSpellSet);
            AddExecutor(ExecutorType.MonsterSet, DefaultMonsterSet);
            AddExecutor(ExecutorType.Repos, DefaultMonsterRepos);
        }

        private bool ActivateSoulExchange()
        {
            // Use Soul Exchange to tribute opponent's monster for Monarch
            if (Enemy.GetMonsterCount() > 0 && (Bot.HasInHand(Thestalos) || Bot.HasInHand(Mobius)))
            {
                AI.SelectCard(Enemy.GetMonsters().OrderByDescending(c => c.Attack).First());
                return true;
            }
            return false;
        }

        private bool TributeSummonMonarch()
        {
            // Tribute summon if we have a monster to tribute
            if (Bot.GetMonsterCount() > 0)
                return true;
            // Or if we just used Soul Exchange
            return Duel.LastChainPlayer == 0 && Duel.CurrentChain.Any(c => c.IsCode(SoulExchange));
        }

        private bool ThestalosEffect()
        {
            // Thestalos makes opponent discard random card
            return Enemy.Hand.Count > 0;
        }

        private bool MobiusEffect()
        {
            // Mobius destroys up to 2 spell/traps
            return Enemy.GetSpellCount() > 0;
        }

        private bool ActivateMetamorphosis()
        {
            // Use on Goat Tokens to summon Thousand-Eyes Restrict
            int goatTokenId = 73414375;
            if (Bot.HasInMonstersZone(goatTokenId))
                return true;
            // Or use on level 6 monsters for higher level fusion
            return Bot.GetMonsters().Any(c => c.Level == 1);
        }

        private bool CallOfTheHauntedForTribute()
        {
            // Revive monster to use as tribute fodder
            if (Bot.HasInGraveyard(Dekoichi))
            {
                AI.SelectCard(Dekoichi);
                return true;
            }
            if (Bot.HasInGraveyard(Airknight))
            {
                AI.SelectCard(Airknight);
                return true;
            }
            return Bot.Graveyard.Any(c => c.HasType(CardType.Monster));
        }
    }
}
