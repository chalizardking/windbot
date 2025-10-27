using YGOSharp.OCGWrapper.Enums;
using System.Collections.Generic;
using System.Linq;
using WindBot;
using WindBot.Game;
using WindBot.Game.AI;

namespace WindBot.Game.AI.Decks
{
    [Deck("GOAT Chaos", "AI_GOAT_Chaos", "Advanced")]
    public class GOATChaosExecutor : DefaultExecutor
    {
        public class CardId
        {
            public const int BlackLusterSoldier = 72989439;
            public const int ChaosSorcerer = 09596126;
            public const int SinisterSerpent = 08131171;
            public const int Sangan = 26202165;
            public const int MorphingJar = 33508719;
            public const int Breaker = 71413901;
            public const int TribeInfectingVirus = 33184167;
            public const int DDAssailant = 70074904;
            public const int DDWarriorLady = 07572887;
            public const int AirknightParshath = 18036057;
            public const int MysticTomato = 83011277;
            public const int NightAssailant = 25005816;
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
            public const int DustTornado = 60082869;
        }

        public GOATChaosExecutor(GameAI ai, Duel duel)
            : base(ai, duel)
        {
            // Draw spells
            AddExecutor(ExecutorType.Activate, CardId.PotOfGreed);
            AddExecutor(ExecutorType.Activate, CardId.GracefulCharity, ActivateGracefulCharity);
            AddExecutor(ExecutorType.Activate, CardId.DelinquentDuo);

            // Removal spells
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

            // Monster summons - Chaos monsters priority
            AddExecutor(ExecutorType.SpSummon, CardId.BlackLusterSoldier, SummonBLS);
            AddExecutor(ExecutorType.Activate, CardId.BlackLusterSoldier, ActivateBLS);
            AddExecutor(ExecutorType.SpSummon, CardId.ChaosSorcerer, SummonChaosSorcerer);
            AddExecutor(ExecutorType.Activate, CardId.ChaosSorcerer, ActivateChaosSorcerer);

            // Other summons
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
            AddExecutor(ExecutorType.Summon, CardId.TribeInfectingVirus);
            AddExecutor(ExecutorType.Activate, CardId.TribeInfectingVirus, ActivateTribeInfectingVirus);
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

        private bool ActivateGracefulCharity()
        {
            // Smart discard selection for Chaos strategy
            // After drawing 3, we need to discard 2 cards
            // Priority: Setup graveyard for BLS/Chaos Sorcerer summons

            // This will be handled by OnSelectCard callback for actual discard selection
            // For now, just activate when beneficial
            return Bot.Hand.Count > 0;
        }

        public override IList<ClientCard> OnSelectCard(IList<ClientCard> cards, int min, int max, long hint, bool cancelable)
        {
            // Handle Graceful Charity discards intelligently
            if (Card != null && Card.Id == CardId.GracefulCharity && min == 2 && max == 2)
            {
                List<ClientCard> toDiscard = new List<ClientCard>();

                // Count current LIGHT/DARK in graveyard
                int lightInGrave = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
                int darkInGrave = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));

                // Check if we have Chaos monsters in hand
                bool hasBlsInHand = Bot.Hand.Any(c => c.Id == CardId.BlackLusterSoldier);
                bool hasChaosInHand = Bot.Hand.Any(c => c.Id == CardId.ChaosSorcerer);
                bool needsChaosSetup = hasBlsInHand || hasChaosInHand;

                // Priority 1: Discard Night Assailant (returns to hand)
                var nightAssailants = cards.Where(c => c.Id == CardId.NightAssailant).ToList();
                foreach (var na in nightAssailants)
                {
                    if (toDiscard.Count < 2)
                    {
                        toDiscard.Add(na);
                    }
                }

                if (toDiscard.Count < 2 && needsChaosSetup)
                {
                    // Priority 2: Balance LIGHT/DARK for Chaos summons
                    if (lightInGrave == 0 || darkInGrave < lightInGrave)
                    {
                        // Need more DARK monsters
                        var darkMonster = cards.Where(c => c.IsMonster() && c.HasAttribute(CardAttribute.Dark)
                            && c.Id != CardId.BlackLusterSoldier && c.Id != CardId.ChaosSorcerer
                            && !toDiscard.Contains(c)).FirstOrDefault();
                        if (darkMonster != null)
                            toDiscard.Add(darkMonster);
                    }

                    if (toDiscard.Count < 2 && (darkInGrave == 0 || lightInGrave < darkInGrave))
                    {
                        // Need more LIGHT monsters
                        var lightMonster = cards.Where(c => c.IsMonster() && c.HasAttribute(CardAttribute.Light)
                            && !toDiscard.Contains(c)).FirstOrDefault();
                        if (lightMonster != null)
                            toDiscard.Add(lightMonster);
                    }
                }

                // Priority 3: Discard low-value cards (keep spells/traps, keep high-value monsters)
                while (toDiscard.Count < 2)
                {
                    // Prefer discarding weak monsters over spells
                    var weakMonster = cards.Where(c => c.IsMonster() && !toDiscard.Contains(c)
                        && c.Id != CardId.BlackLusterSoldier && c.Id != CardId.ChaosSorcerer
                        && c.Attack <= 1500).OrderBy(c => c.Attack).FirstOrDefault();

                    if (weakMonster != null)
                    {
                        toDiscard.Add(weakMonster);
                    }
                    else
                    {
                        // Discard any non-essential card
                        var anyCard = cards.Where(c => !toDiscard.Contains(c)
                            && c.Id != CardId.BlackLusterSoldier && c.Id != CardId.ChaosSorcerer
                            && c.Id != CardId.PotOfGreed).FirstOrDefault();
                        if (anyCard != null)
                            toDiscard.Add(anyCard);
                        else
                            break;
                    }
                }

                if (toDiscard.Count == 2)
                    return toDiscard;
            }

            return base.OnSelectCard(cards, min, max, hint, cancelable);
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
            // Flip Magician of Faith for reuse, or stop opponent's attack
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

        private bool ActivatePrematureBurial()
        {
            // Revive BLS, Chaos Sorcerer, or other key monsters
            if (Bot.Graveyard.Any(card => card.Id == CardId.BlackLusterSoldier))
            {
                AI.SelectCard(CardId.BlackLusterSoldier);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.ChaosSorcerer))
            {
                AI.SelectCard(CardId.ChaosSorcerer);
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
            // Check if we can properly summon BLS (need LIGHT and DARK in grave)
            int light = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
            int dark = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));
            return light > 0 && dark > 0;
        }

        private bool ActivateBLS()
        {
            // Banish key threats or attack twice for game
            if (Card.Location == CardLocation.MonsterZone && !Card.IsDisabled())
            {
                // Banish enemy's strongest monster
                if (Enemy.GetMonsterCount() > 0)
                {
                    ClientCard target = Enemy.GetMonsters().OrderByDescending(card => card.Attack).First();
                    AI.SelectCard(target);
                    return true;
                }
            }
            return false;
        }

        private bool SummonChaosSorcerer()
        {
            // Check if we can properly summon Chaos Sorcerer (need LIGHT and DARK in grave)
            int light = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Light));
            int dark = Bot.Graveyard.Count(card => card.HasAttribute(CardAttribute.Dark));
            return light > 0 && dark > 0;
        }

        private bool ActivateChaosSorcerer()
        {
            // Banish face-up threats repeatedly
            if (Enemy.GetMonsters().Any(card => card.IsFaceup()))
            {
                ClientCard target = Enemy.GetMonsters().Where(card => card.IsFaceup()).OrderByDescending(card => card.Attack).First();
                AI.SelectCard(target);
                return true;
            }
            return false;
        }

        private bool SummonBreaker()
        {
            // Summon Breaker when opponent has backrow
            return Enemy.GetSpellCount() > 0;
        }

        private bool ActivateMysticTomato()
        {
            // Search Night Assailant for recursion, or D.D. Assailant for removal
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
            if (Bot.Deck.Any(card => card.Id == CardId.SinisterSerpent))
            {
                AI.SelectCard(CardId.SinisterSerpent);
                return true;
            }
            return false;
        }

        private bool ActivateMagicianOfFaith()
        {
            // Recover key spells
            if (Bot.Graveyard.Any(card => card.Id == CardId.PotOfGreed))
            {
                AI.SelectCard(CardId.PotOfGreed);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.GracefulCharity))
            {
                AI.SelectCard(CardId.GracefulCharity);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.PrematureBurial))
            {
                AI.SelectCard(CardId.PrematureBurial);
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

        private bool ActivateTribeInfectingVirus()
        {
            // Clear field when ahead
            if (Bot.LifePoints > Enemy.LifePoints && Enemy.GetMonsterCount() >= 2 && Bot.Hand.Count > 2)
            {
                if (Bot.Hand.Any(card => card.Id == CardId.SinisterSerpent))
                    AI.SelectCard(CardId.SinisterSerpent);
                else if (Bot.Hand.Any(card => card.Id == CardId.NightAssailant))
                    AI.SelectCard(CardId.NightAssailant);
                else
                    AI.SelectCard(Bot.Hand.OrderBy(card => card.Attack).First());
                return true;
            }
            return false;
        }

        private bool ActivateSangan()
        {
            // Search for combo pieces or utility monsters
            if (Bot.Deck.Any(card => card.Id == CardId.MagicianOfFaith))
            {
                AI.SelectCard(CardId.MagicianOfFaith);
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
            // Revive best monster from grave
            if (Bot.Graveyard.Any(card => card.Id == CardId.BlackLusterSoldier))
            {
                AI.SelectCard(CardId.BlackLusterSoldier);
                return true;
            }
            if (Bot.Graveyard.Any(card => card.Id == CardId.ChaosSorcerer))
            {
                AI.SelectCard(CardId.ChaosSorcerer);
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
