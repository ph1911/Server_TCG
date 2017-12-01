using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace fctServer
{
    public class CardEffects
    {
        public Fighter player = new Fighter();
        public Fighter opponent = new Fighter();
        public int currentCardPosition;

        private Server _server = Server.Instance;

        public struct Card
        {
            public string id;                   //"#001", "#002"...
            public string name;                 //"Oni Stomp", "Twin Pistons"...
            public int damage;                  //e.g. 50
            public int priority;                //set to 1 for nullification effects
            public string activation;           //"onAppear", "Pre", "After", "Nothing"
            public List<Action> effect;         //CardEffects.Method(), null
            public string rarity;               //"Bronze", "Silver", "Gold", "SR"
            public string type;                 //"Punch", "Kick"
            public bool limited;
            public bool unique;
            public bool sr;

            //only for cards in play
            public bool inPlay;
            public int posInDeck;
            public bool tenacity;
        }

        //soon to be used
        public enum PlayerAction
        {
            Focus, Strike, Block
        };

        public Card GetCardById(string id)
        {
            Card card = new Card();
            if (id == "") {
                return card;
            }
            for (int i = 1; i < cardsInDatabase.Length; i++) {
                if (cardsInDatabase[i].id == id) {
                    card = cardsInDatabase[i];
                    break;
                }
            }
            return card;
        }

        private Random random = new Random();
        private Card GetRandomCardFromDeck(Fighter player)
        {
            Card card = new Card();
            int index;
            do {
                index = random.Next(0, player.deck.Length);
                card = player.deck[index];
            } while (card.inPlay);
            card.posInDeck = index;
            player.deck[index].inPlay = true;
            return card;
        }

        private void DrawCard(Fighter player, [Optional]int value)
        {
            if (value == 0) {
                value = 1;
            }
            for (int i = 0; i < value; i++) {
                Card card = GetRandomCardFromDeck(player);
                if (player.cardsInPlay[currentCardPosition].activation == "Pre") {
                    player.cardsToDrawPreFight.Add(card);
                }
                else {
                    player.cardsToDrawAfterFight.Add(card);
                }
            }
        }
        public void DrawToken(Fighter player, int damage, string type)
        {
            Card token = new Card();
            token.id = "Token";
            token.name = "Token";
            token.damage = damage;
            token.type = type;
            player.tokensToDraw.Add(token);
        }
        public void PutTokensInPlay(Fighter player)
        {
            foreach (Card token in player.tokensToDraw) {
                player.currentCardDamage[player.numberOfCards] = token.damage;
                player.cardsInPlay[player.numberOfCards] = token;
                player.numberOfCards++;
            }
        }
        public void FocusDraw(Fighter player)
        {
            player.cardToDrawOnFocus = GetRandomCardFromDeck(player);
        }
        public void PutFocusCardInPlay(Fighter player)
        {
            Card card = player.cardToDrawOnFocus;
            if (player.numberOfCards > 4) {
                //unmark the card as in play
                player.deck[card.posInDeck].inPlay = false;
                player.cardToDrawOnFocus = new Card();
                return;
            }
            player.cardsInPlay[player.numberOfCards] = card;
            player.currentCardDamage[player.numberOfCards] = card.damage;
            player.numberOfCards++;
        }
        public void PutCardsInPlay(Fighter player, string activation)
        {
            if (activation == "Pre") {
                List<Card> cardsToUnmark = player.cardsToDrawPreFight.ToList();
                foreach (Card card in player.cardsToDrawPreFight.ToList()) {
                    if (player.numberOfCards > 4) {
                        //unmark all cards that won't be drawn as in play
                        foreach (Card cardToUnmark in cardsToUnmark.ToList()) {
                            player.deck[cardToUnmark.posInDeck].inPlay = false;
                            player.cardsToDrawPreFight.Remove(cardToUnmark);
                        }
                        return;
                    }
                    player.cardsInPlay[player.numberOfCards] = card;
                    player.currentCardDamage[player.numberOfCards] = card.damage;
                    player.numberOfCards++;
                    cardsToUnmark.Remove(card);
                }
            }
            else {
                List<Card> cardsToUnmark = player.cardsToDrawAfterFight.ToList();
                foreach (Card card in player.cardsToDrawAfterFight.ToList()) {
                    if (player.numberOfCards > 4) {
                        //unmark all cards that won't be drawn as in play
                        foreach (Card cardToUnmark in cardsToUnmark.ToList()) {
                            player.deck[cardToUnmark.posInDeck].inPlay = false;
                            player.cardsToDrawAfterFight.Remove(card);
                        }
                        return;
                    }
                    player.cardsInPlay[player.numberOfCards] = card;
                    player.currentCardDamage[player.numberOfCards] = card.damage;
                    player.numberOfCards++;
                    cardsToUnmark.Remove(card);
                }
            }
        }

        public void Discard(Fighter player, int from, [Optional]int to)
        {
            for (int i = from; i <= to; i++) {
                player.cardsToDiscard[i] = true;
            }
        }
        public void FocusBreak(Fighter player)
        {
            player.cardsToDiscard[0] = true;
            StartDiscarding(player);
        }
        public void StartDiscarding(Fighter player)
        {
            if (player.numberOfCards == 0 || !player.cardsToDiscard.Contains(true))
                return;
            //discard the given cards
            int cardsDiscarded = 0;
            for (int i = 0; i < player.numberOfCards; i++) {
                if (player.cardsToDiscard[i] && !player.cardsInPlay[i].tenacity) {
                    player.currentCardDamage[i] = 0;
                    //mark the corresponding card in the deck as not in play
                    player.deck[player.cardsInPlay[i].posInDeck].inPlay = false;
                    player.cardsInPlay[i] = default(Card);
                    cardsDiscarded++;
                }
                else if (player.cardsToDiscard[i] && player.cardsInPlay[i].tenacity) {
                    player.cardsInPlay[i].tenacity = false;
                }
            }
            //check if there are cards to move after the discard
            if (cardsDiscarded < player.numberOfCards) {
                for (int n = 0; n < 4; n++) {
                    for (int i = player.numberOfCards - 1; i > 0; i--) {
                        if (player.cardsInPlay[i - 1].Equals(default(Card))) {
                            player.cardsInPlay[i - 1] = player.cardsInPlay[i];
                            player.cardsInPlay[i] = default(Card);
                            player.currentCardDamage[i - 1] = player.currentCardDamage[i];
                            player.currentCardDamage[i] = 0;
                        }
                    }
                }
            }
            player.numberOfCards -= cardsDiscarded;
            player.cardsToDiscard = new bool[5];
        }

        public void DamagePlayer(Fighter player, int damage)
        {
            int originalDamage = damage;
            int newDamage = originalDamage - player.protection;
            //if protection not broken, remove protection equivalent from the original damage
            if (newDamage <= 0) {
                newDamage = 0;
                AddProtection(player, -originalDamage);
            }
            //else if protection broken, put it to 0
            else if (player.protection > 0) {
                RemoveProtection(player);
            }

            player.currentHP -= newDamage;
            //no hp over maxHP
            if (player.currentHP > player.maxHP)
                player.currentHP = player.maxHP;
            //no hp under 0
            if (player.currentHP < 0) {
                player.currentHP = 0;
            }
            player.oldHP = player.currentHP;
        }

        private void AddProtection(Fighter player, int value)
        {
            player.protection += value;
            if (player.protection < 0) {
                player.protection = 0;
            }
        }
        public void RemoveProtection(Fighter player)
        {
            player.protection = 0;
        }

        private void Nullify(Fighter player, int from, [Optional]int to)
        {
            if (to == 0) {
                to = from;
            }
            for (int i = from; i <= to; i++) {
                player.cardsToNullify[i] = true;
            }
        }
        public void StartNullify(Fighter player)
        {
            for (int i = 0; i < player.numberOfCards; i++) {
                if (player.cardsToNullify[i]) {
                    player.nullifiedCards[i] = true;
                }
            }
        }
        public void RemoveNullify(Fighter player)
        {
            player.cardsToNullify = new bool[5];
            player.nullifiedCards = new bool[5];
        }

        private void ChangeDamage(Fighter player, int value, int from, [Optional]int to)
        {
            if (to == 0) {
                to = from;
            }
            if (to >= player.numberOfCards) {
                to = player.numberOfCards - 1;
            }
            for (int i = from; i <= to; i++) {
                player.damageModifier[i] += value;
            }
        }
        public void ApplyDamageModifier(Fighter player)
        {
            for (int i = 0; i < player.numberOfCards; i++) {
                player.currentCardDamage[i] = player.cardsInPlay[i].damage + player.damageModifier[i];
            }
        }
        public void ResetDamageModifier(Fighter player)
        {
            player.damageModifier = new int[5];
            ApplyDamageModifier(player);
        }

        public void JinMutation(Fighter player)
        {
            if (!player.jinToMutate) {
                return;
            }
            player.jinToMutate = false;
            if (player.jinMutated) {
                player.jinMutated = false;
            }
            else {
                player.jinMutated = true;
            }
        }

        /*  /////////////   */
        /*  CARD DATABASE   */
        /* //////////////   */
        private Card[] cardsInDatabase = new Card[100];
        public CardEffects()
        {
            cardsInDatabase[1].id = "#001";
            cardsInDatabase[1].name = "Thrusting Uppercut";
            cardsInDatabase[1].damage = 20;
            cardsInDatabase[1].activation = "Pre";
            cardsInDatabase[1].effect = new List<Action>();
            cardsInDatabase[1].effect.Add(() => JinThrustingUppercut());
            cardsInDatabase[1].rarity = "Gold";
            cardsInDatabase[1].type = "Punch";
            cardsInDatabase[1].unique = true;

            cardsInDatabase[2].id = "#002";
            cardsInDatabase[2].name = "Revolving Hands";
            cardsInDatabase[2].damage = 20;
            cardsInDatabase[2].activation = "Pre";
            cardsInDatabase[2].effect = new List<Action>();
            cardsInDatabase[2].effect.Add(() => JinRevolvingHands());
            cardsInDatabase[2].rarity = "Gold";
            cardsInDatabase[2].type = "Kick";
            cardsInDatabase[2].unique = true;

            cardsInDatabase[3].id = "#003";
            cardsInDatabase[3].name = "Stinger";
            cardsInDatabase[3].damage = 17;
            cardsInDatabase[3].activation = "After";
            cardsInDatabase[3].effect = new List<Action>();
            cardsInDatabase[3].effect.Add(() => JinStinger());
            cardsInDatabase[3].rarity = "Gold";
            cardsInDatabase[3].type = "Kick";
            cardsInDatabase[3].limited = true;
            cardsInDatabase[3].unique = true;

            cardsInDatabase[4].id = "#004";
            cardsInDatabase[4].name = "Spinning Side Kick";
            cardsInDatabase[4].damage = 13;
            cardsInDatabase[4].activation = "Pre";
            cardsInDatabase[4].effect = new List<Action>();
            cardsInDatabase[4].effect.Add(() => JinSpinningSideKick());
            cardsInDatabase[4].rarity = "Gold";
            cardsInDatabase[4].type = "Kick";
            cardsInDatabase[4].unique = true;

            cardsInDatabase[5].id = "#005";
            cardsInDatabase[5].name = "Power Stance";
            cardsInDatabase[5].damage = 14;
            cardsInDatabase[5].activation = "After";
            cardsInDatabase[5].effect = new List<Action>();
            cardsInDatabase[5].effect.Add(() => JinPowerStance());
            cardsInDatabase[5].rarity = "Gold";
            cardsInDatabase[5].type = "Punch";

            cardsInDatabase[6].id = "#006";
            cardsInDatabase[6].name = "Median Line Destruction";
            cardsInDatabase[6].damage = 17;
            cardsInDatabase[6].activation = "After";
            cardsInDatabase[6].effect = new List<Action>();
            cardsInDatabase[6].effect.Add(() => JinMedianLineDestruction());
            cardsInDatabase[6].rarity = "Gold";
            cardsInDatabase[6].type = "Punch";
            cardsInDatabase[6].unique = true;

            cardsInDatabase[7].id = "#007";
            cardsInDatabase[7].name = "Right Spinning Axe Kick";
            cardsInDatabase[7].damage = 35;
            cardsInDatabase[7].activation = "After";
            cardsInDatabase[7].effect = new List<Action>();
            cardsInDatabase[7].effect.Add(() => JinRightSpinningAxeKick());
            cardsInDatabase[7].rarity = "Gold";
            cardsInDatabase[7].type = "Kick";
            cardsInDatabase[7].unique = true;

            cardsInDatabase[8].id = "#008";
            cardsInDatabase[8].name = "Left Knee";
            cardsInDatabase[8].damage = 18;
            cardsInDatabase[8].activation = "After";
            cardsInDatabase[8].effect = new List<Action>();
            cardsInDatabase[8].effect.Add(() => JinLeftKnee());
            cardsInDatabase[8].rarity = "Gold";
            cardsInDatabase[8].type = "Kick";

            cardsInDatabase[9].id = "#009";
            cardsInDatabase[9].name = "Left Jab to Double Low";
            cardsInDatabase[9].damage = 15;
            cardsInDatabase[9].activation = "After";
            cardsInDatabase[9].effect = new List<Action>();
            cardsInDatabase[9].effect.Add(() => JinLeftJabToDoubleLow());
            cardsInDatabase[9].rarity = "Gold";
            cardsInDatabase[9].type = "Kick";
            cardsInDatabase[9].unique = true;

            cardsInDatabase[10].id = "#010";
            cardsInDatabase[10].name = "Left Drill Punch";
            cardsInDatabase[10].damage = 17;
            cardsInDatabase[10].activation = "After";
            cardsInDatabase[10].effect = new List<Action>();
            //cardsInDatabase[10].effect.Add(() => JinLeftDrillPunch());
            cardsInDatabase[10].rarity = "Gold";
            cardsInDatabase[10].type = "Punch";
            cardsInDatabase[10].unique = true;

            cardsInDatabase[11].id = "#011";
            cardsInDatabase[11].name = "Knee Popper to Sidekick";
            cardsInDatabase[11].damage = 13;
            cardsInDatabase[11].activation = "After";
            cardsInDatabase[11].effect = new List<Action>();
            cardsInDatabase[11].effect.Add(() => JinKneePopperToSidekick());
            cardsInDatabase[11].rarity = "Gold";
            cardsInDatabase[11].type = "Kick";
            cardsInDatabase[11].limited = true;
            cardsInDatabase[11].unique = true;

            cardsInDatabase[12].id = "#012";
            cardsInDatabase[12].name = "Double Thrust Roundhouse";
            cardsInDatabase[12].damage = 17;
            cardsInDatabase[12].activation = "After";
            cardsInDatabase[12].effect = new List<Action>();
            cardsInDatabase[12].effect.Add(() => JinDoubleThrustRoundhouse());
            cardsInDatabase[12].rarity = "Gold";
            cardsInDatabase[12].type = "Kick";
            cardsInDatabase[12].unique = true;

            cardsInDatabase[13].id = "#013";
            cardsInDatabase[13].name = "Double Lift Kick";
            cardsInDatabase[13].damage = 18;
            cardsInDatabase[13].activation = "After";
            cardsInDatabase[13].effect = new List<Action>();
            cardsInDatabase[13].effect.Add(() => JinDoubleLiftKick());
            cardsInDatabase[13].rarity = "Gold";
            cardsInDatabase[13].type = "Kick";

            cardsInDatabase[14].id = "#014";
            cardsInDatabase[14].name = "Double Chamber Punch";
            cardsInDatabase[14].damage = 25;
            cardsInDatabase[14].activation = "Pre";
            cardsInDatabase[14].effect = new List<Action>();
            cardsInDatabase[14].effect.Add(() => JinDoubleChamberPunch());
            cardsInDatabase[14].rarity = "SR";
            cardsInDatabase[14].type = "Punch";

            cardsInDatabase[15].id = "#015";
            cardsInDatabase[15].name = "Corpse Thrust";
            cardsInDatabase[15].damage = 12;
            cardsInDatabase[15].activation = "After";
            cardsInDatabase[15].effect = new List<Action>();
            cardsInDatabase[15].effect.Add(() => JinCorpseThrust());
            cardsInDatabase[15].rarity = "Gold";
            cardsInDatabase[15].type = "Punch";
            cardsInDatabase[15].limited = true;
            cardsInDatabase[15].unique = true;
        }

        /*  /////////////   */
        /*      CARDS       */
        /* //////////////   */
        public void JinThrustingUppercut()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action == "Focus") {
                    player.kickParry++;
                    if (player.jinMutated) {
                        player.punchParry++;
                    }
                }
            }
        }
        public void JinRevolvingHands()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action == "Strike") {
                    AddProtection(player, 25);
                    if (player.jinMutated) {
                        Nullify(opponent, 0, 4);
                    }
                }
            }
        }
        public void JinStinger()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action == "Focus" && opponent.action == "Focus") {
                    player.jinToMutate = true;
                    AddProtection(player, 18);
                }
            }
        }
        public void JinSpinningSideKick()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action != opponent.action) {
                    if (player.jinMutated) {
                        ChangeDamage(player, 7, 0, 4);
                        ChangeDamage(opponent, -7, 0, 4);
                    }
                    else {
                        ChangeDamage(player, 5, 0, 4);
                        ChangeDamage(opponent, -5, 0, 4);
                    }
                }
            }
        }
        public void JinPowerStance()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action == "Strike" && opponent.action == "Block") {
                    DrawCard(player);
                    if (player.jinMutated) {
                        DrawCard(player);
                        player.jinToMutate = true;
                    }
                }
            }
        }
        public void JinMedianLineDestruction()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if ((player.jinMutated && player.action == "Strike")
                        || (player.action == "Strike" && opponent.action == "Strike")) {
                    DrawCard(player);
                    DrawToken(player, 15, "Punch");
                }
            }
        }
        public void JinRightSpinningAxeKick()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (opponent.action == "Block" && player.action != "Strike") {
                    DamagePlayer(player, -5);
                    if (player.jinMutated) {
                        DrawCard(player);
                        player.jinToMutate = true;
                    }
                }
            }
        }
        public void JinLeftKnee()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action == "Strike" && player.jinMutated) {
                    player.cardsInPlay[0].tenacity = true;
                    Discard(player, currentCardPosition);
                }
                else if (player.action == "Strike") {
                    player.cardsInPlay[3].tenacity = true;
                    Discard(player, currentCardPosition);
                }
            }
        }
        public void JinLeftJabToDoubleLow()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if ((player.action == "Strike" && opponent.action == "Block")
                        || (player.action == "Strike" && player.jinMutated)) {
                    DrawCard(player);
                }
            }
        }
        public void JinKneePopperToSidekick()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action == "Strike") {
                    if (player.jinMutated) {
                        DrawCard(player, 2);
                        player.jinToMutate = true;
                    }
                    else
                        player.jinToMutate = true;
                }
            }
        }
        public void JinDoubleThrustRoundhouse()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action == "Strike" && player.jinMutated) {
                    if (opponent.numberOfCards >= 2) {
                        DrawCard(player);
                    }
                }
                else if (player.action == "Strike") {
                    if (opponent.numberOfCards >= 3) {
                        DrawCard(player);
                    }
                }
            }
        }
        public void JinDoubleLiftKick()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action != opponent.action) {
                    AddProtection(player, 8);
                    player.jinToMutate = true;
                }
            }
        }
        public void JinDoubleChamberPunch()
        {
            if (player.action == "Strike"
                    && player.jinMutated) {
                ChangeDamage(player, 14, 0, 4);
            }
            else if (player.action == "Strike") {
                ChangeDamage(player, 7, 0, 4);
            }
        }
        public void JinCorpseThrust()
        {
            if (player.cardsInPlay[currentCardPosition].rarity == "Gold") {
                if (player.action == "Strike") {
                    player.maxHP -= 6;
                    DrawCard(player);
                }
            }
        }
    }
}
