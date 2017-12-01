using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static fctServer.CardEffects;
using static fctServer.Server;

namespace fctServer
{
    public class Fighter
    {
        public ServerClient clientIdentity;
        public Card[] deck;
        public int maxHP;
        public int oldHP;
        public int currentHP;
        public int protection;
        public int numberOfCards;
        public Card cardToDrawOnFocus;
        public List<Card> cardsToDrawPreFight;
        public List<Card> cardsToDrawAfterFight;
        public List<Card> tokensToDraw;
        public int[] currentCardDamage;
        public int[] damageModifier;
        public bool[] blockedCards;
        public bool[] nullifiedCards;
        public bool[] cardsToNullify;
        public bool[] cardsToDiscard;
        public Card[] cardsInPlay;
        public string action;
        public int parry;
        public int punchParry;
        public int kickParry;
        public bool jinToMutate;
        public bool jinMutated;

        public Fighter()
        {
            cardsToDrawPreFight = new List<Card>();
            cardsToDrawAfterFight = new List<Card>();
            tokensToDraw = new List<Card>();
            currentCardDamage = new int[5];
            damageModifier = new int[5];
            blockedCards = new bool[5];
            nullifiedCards = new bool[5];
            cardsToNullify = new bool[5];
            cardsToDiscard = new bool[5];
            cardsInPlay = new Card[5];
        }
    }


    public class Battle
    {
        public int battleTimer;
        public Fighter fighter1 = new Fighter();
        public Fighter fighter2 = new Fighter();

        private Server _server = Server.Instance;
        private CardEffects _cardEffects = new CardEffects();

        public Battle(ServerClient client1, ServerClient client2)
        {
            client1.currentBattle = this;
            client1.battleIdentity = 1;
            fighter1.clientIdentity = client1;
            //make deck from ids
            fighter1.deck = new Card[client1.playerData.deckCardIds.Length];
            for (int i = 0; i < client1.playerData.deckCardIds.Length; i++) {
                fighter1.deck[i] = _cardEffects.GetCardById(client1.playerData.deckCardIds[i]);
            }
            fighter1.maxHP = 185;
            fighter1.oldHP = fighter1.maxHP;
            fighter1.currentHP = fighter1.maxHP;

            client2.currentBattle = this;
            client2.battleIdentity = 2;
            fighter2.clientIdentity = client2;
            //make deck from ids
            fighter2.deck = new Card[client2.playerData.deckCardIds.Length];
            for (int i = 0; i < client2.playerData.deckCardIds.Length; i++) {
                fighter2.deck[i] = _cardEffects.GetCardById(client2.playerData.deckCardIds[i]);
            }
            fighter2.maxHP = 185;
            fighter2.oldHP = fighter2.maxHP;
            fighter2.currentHP = fighter2.maxHP;
        }

        public void PlayerAction(string action, int identity)
        {
            Fighter player, opponent;
            if (identity == 1) {
                player = fighter1;
                opponent = fighter2;
            }
            else {
                player = fighter2;
                opponent = fighter1;
            }

            switch (action) {
                case "Focus":
                    player.action = "Focus";
                    break;
                case "Strike":
                    player.action = "Strike";
                    break;
                case "Block":
                    player.action = "Block";
                    break;
            }
            if (opponent.action != null) {
                battleTimer = 0;
                PreFight();
            }
        }

        private void PreFight()
        {
            if (fighter1.action == "Block")
                fighter1.parry += 2;
            if (fighter2.action == "Block")
                fighter2.parry += 2;

            ActivateEffects("Pre");

            if (fighter1.action == "Strike") {
                //calculate blocked cards for fighter1
                for (int i = 0; i < fighter1.numberOfCards; i++) {
                    if (fighter2.punchParry > 0
                            && fighter1.cardsInPlay[i].type == "Punch") {
                        fighter1.blockedCards[i] = true;
                        fighter2.punchParry--;
                    }
                }
                for (int i = 0; i < fighter1.numberOfCards; i++) {
                    if (fighter2.kickParry > 0
                            && fighter1.cardsInPlay[i].type == "Kick") {
                        fighter1.blockedCards[i] = true;
                        fighter2.kickParry--;
                    }
                }
                for (int i = 0; i < fighter1.numberOfCards; i++) {
                    if (fighter2.parry > 0 && fighter1.blockedCards[i] == false) {
                        fighter1.blockedCards[i] = true;
                        fighter2.parry--;
                    }
                }
            }
            if (fighter2.action == "Strike") {
                //calculate blocked cards for fighter2
                for (int i = 0; i < fighter2.numberOfCards; i++) {
                    if (fighter1.punchParry > 0
                            && fighter2.cardsInPlay[i].type == "Punch") {
                        fighter2.blockedCards[i] = true;
                        fighter1.punchParry--;
                    }
                }
                for (int i = 0; i < fighter2.numberOfCards; i++) {
                    if (fighter1.kickParry > 0
                            && fighter2.cardsInPlay[i].type == "Kick") {
                        fighter2.blockedCards[i] = true;
                        fighter1.kickParry--;
                    }
                }
                for (int i = 0; i < fighter2.numberOfCards; i++) {
                    if (fighter1.parry > 0 && fighter2.blockedCards[i] == false) {
                        fighter2.blockedCards[i] = true;
                        fighter1.parry--;
                    }
                }
            }

            //discard first card on focus break
            if (fighter1.action == "Strike" && fighter2.action == "Focus")
                _cardEffects.FocusBreak(fighter2);
            else if (fighter2.action == "Strike" && fighter1.action == "Focus")
                _cardEffects.FocusBreak(fighter1);

            if (fighter1.action == "Strike") {
                for (int i = 0; i < fighter1.numberOfCards; i++)
                    //if card not blocked
                    if (!fighter1.blockedCards[i])
                        //damage opponent
                        _cardEffects.DamagePlayer(fighter2, fighter1.currentCardDamage[i]);
            }
            if (fighter2.action == "Strike") {
                for (int i = 0; i < fighter2.numberOfCards; i++)
                    if (!fighter2.blockedCards[i])
                        _cardEffects.DamagePlayer(fighter1, fighter2.currentCardDamage[i]);
            }

            //MIDFIGHT RESET
            _cardEffects.RemoveProtection(fighter1);
            _cardEffects.RemoveProtection(fighter2);
            _cardEffects.RemoveNullify(fighter1);
            _cardEffects.RemoveNullify(fighter2);
            _cardEffects.ResetDamageModifier(fighter1);
            _cardEffects.ResetDamageModifier(fighter2);
            ActivateEffects("After");
            AfterFight();
        }

        private void ActivateEffects(string activation)
        {
            _cardEffects.player = fighter1;
            _cardEffects.opponent = fighter2;
            for (int i = 0; i < fighter1.numberOfCards; i++)
                if (fighter1.cardsInPlay[i].activation == activation
                    && !fighter1.nullifiedCards[i]
                    && fighter1.cardsInPlay[i].priority == 1) {
                    _cardEffects.currentCardPosition = i;
                    foreach (Action effect in fighter1.cardsInPlay[i].effect)
                        effect();
                }
            _cardEffects.player = fighter2;
            _cardEffects.opponent = fighter1;
            for (int i = 0; i < fighter2.numberOfCards; i++)
                if (fighter2.cardsInPlay[i].activation == activation
                    && !fighter2.nullifiedCards[i]
                    && fighter2.cardsInPlay[i].priority == 1) {
                    _cardEffects.currentCardPosition = i;
                    foreach (Action effect in fighter2.cardsInPlay[i].effect)
                        effect();
                }

            _cardEffects.StartNullify(fighter1);
            _cardEffects.StartNullify(fighter2);

            _cardEffects.player = fighter1;
            _cardEffects.opponent = fighter2;
            for (int i = 0; i < fighter1.numberOfCards; i++)
                if (fighter1.cardsInPlay[i].activation == activation
                  && !fighter1.nullifiedCards[i]
                  && fighter1.cardsInPlay[i].priority == 0) {
                    _cardEffects.currentCardPosition = i;
                    foreach (Action effect in fighter1.cardsInPlay[i].effect)
                        effect();
                }
            _cardEffects.player = fighter2;
            _cardEffects.opponent = fighter1;
            for (int i = 0; i < fighter2.numberOfCards; i++)
                if (fighter2.cardsInPlay[i].activation == activation
                    && !fighter2.nullifiedCards[i]
                    && fighter2.cardsInPlay[i].priority == 0) {
                    _cardEffects.currentCardPosition = i;
                    foreach (Action effect in fighter2.cardsInPlay[i].effect)
                        effect();
                }

            _cardEffects.ApplyDamageModifier(fighter1);
            _cardEffects.ApplyDamageModifier(fighter2);

            _cardEffects.JinMutation(fighter1);
            _cardEffects.JinMutation(fighter2);

            //discard in the after phase if player striked
            if (activation == "After") {
                if (fighter1.action == "Strike") {
                    _cardEffects.Discard(fighter1, 0, 4);
                }
                if (fighter2.action == "Strike") {
                    _cardEffects.Discard(fighter2, 0, 4);
                }
            }
            //activate discard effects
            _cardEffects.StartDiscarding(fighter1);
            _cardEffects.StartDiscarding(fighter2);

            //put drawn cards in play
            _cardEffects.PutCardsInPlay(fighter1, activation);
            _cardEffects.PutCardsInPlay(fighter2, activation);

            _cardEffects.PutTokensInPlay(fighter1);
            _cardEffects.PutTokensInPlay(fighter2);

            //send drawn cards
            string cardIdsToSend = "";
            if (activation == "Pre") {
                foreach (Card card in fighter1.cardsToDrawPreFight) {
                    cardIdsToSend += "|" + card.id;
                }
                _server.Send("CardDraw|Pre" + cardIdsToSend, fighter1.clientIdentity);
                _server.Send("OpDraw|Pre" + cardIdsToSend, fighter2.clientIdentity);
                cardIdsToSend = "";
                foreach (Card card in fighter2.cardsToDrawPreFight) {
                    cardIdsToSend += "|" + card.id;
                }
                _server.Send("CardDraw|Pre" + cardIdsToSend, fighter2.clientIdentity);
                _server.Send("OpDraw|Pre" + cardIdsToSend, fighter1.clientIdentity);
            }
            else {
                foreach (Card card in fighter1.cardsToDrawAfterFight) {
                    cardIdsToSend += "|" + card.id;
                }
                _server.Send("CardDraw|After" + cardIdsToSend, fighter1.clientIdentity);
                _server.Send("OpDraw|After" + cardIdsToSend, fighter2.clientIdentity);
                cardIdsToSend = "";
                foreach (Card card in fighter2.cardsToDrawAfterFight) {
                    cardIdsToSend += "|" + card.id;
                }
                _server.Send("CardDraw|After" + cardIdsToSend, fighter2.clientIdentity);
                _server.Send("OpDraw|After" + cardIdsToSend, fighter1.clientIdentity);
            }
        }

        private void AfterFight()
        {
            switch (fighter1.action) {
                case "Focus":
                    if (fighter1.numberOfCards < 5) {
                        _cardEffects.FocusDraw(fighter1);
                        _cardEffects.PutFocusCardInPlay(fighter1);
                        _server.Send("CardDrawFocus|" + fighter1.cardToDrawOnFocus.id, fighter1.clientIdentity);
                        _server.Send("OpFocus|" + fighter1.cardToDrawOnFocus.id, fighter2.clientIdentity);
                    }
                    else {
                        _server.Send("CardDrawFocus|none", fighter1.clientIdentity);
                        _server.Send("OpFocus|none", fighter2.clientIdentity);
                    }
                    break;
                case "Strike":
                    _server.Send("OpStrike", fighter2.clientIdentity);
                    break;
                case "Block":
                    _server.Send("OpBlock", fighter2.clientIdentity);
                    break;
            }
            switch (fighter2.action) {
                case "Focus":
                    if (fighter2.numberOfCards < 5) {
                        _cardEffects.FocusDraw(fighter2);
                        _cardEffects.PutFocusCardInPlay(fighter2);
                        _server.Send("CardDrawFocus|" + fighter2.cardToDrawOnFocus.id, fighter2.clientIdentity);
                        _server.Send("OpFocus|" + fighter2.cardToDrawOnFocus.id, fighter1.clientIdentity);
                    }
                    else {
                        _server.Send("CardDrawFocus|none", fighter2.clientIdentity);
                        _server.Send("OpFocus|none", fighter1.clientIdentity);
                    }
                    break;
                case "Strike":
                    _server.Send("OpStrike", fighter1.clientIdentity);
                    break;
                case "Block":
                    _server.Send("OpBlock", fighter1.clientIdentity);
                    break;
            }

            _server.Invoke((MethodInvoker)delegate {
                _server.Fighter1HPTB.Text = fighter1.currentHP.ToString();
                _server.Fighter2HPTB.Text = fighter2.currentHP.ToString();
                _server.Fighter1CardsTB.Text = fighter1.numberOfCards.ToString();
                _server.Fighter2CardsTB.Text = fighter2.numberOfCards.ToString();
            });

            if (fighter1.currentHP <= 0 && fighter2.currentHP <= 0) {
                _server.Send("END|TRUCE", fighter1.clientIdentity);
                _server.Send("END|TRUCE", fighter2.clientIdentity);
            }
            else if (fighter1.currentHP <= 0) {
                _server.Send("END|LOSE", fighter1.clientIdentity);
                _server.Send("END|WIN", fighter2.clientIdentity);
            }
            else if (fighter2.currentHP <= 0) {
                _server.Send("END|WIN", fighter1.clientIdentity);
                _server.Send("END|LOSE", fighter2.clientIdentity);
            }

            fighter1.blockedCards = new bool[5];
            fighter2.blockedCards = new bool[5];
            fighter1.cardToDrawOnFocus = new Card();
            fighter2.cardToDrawOnFocus = new Card();
            fighter1.cardsToDrawPreFight = new List<Card>();
            fighter2.cardsToDrawPreFight = new List<Card>();
            fighter1.cardsToDrawAfterFight = new List<Card>();
            fighter2.cardsToDrawAfterFight = new List<Card>();
            fighter1.tokensToDraw = new List<Card>();
            fighter2.tokensToDraw = new List<Card>();
            fighter1.action = null;
            fighter2.action = null;
            fighter1.parry = 0;
            fighter1.punchParry = 0;
            fighter1.kickParry = 0;
            fighter2.parry = 0;
            fighter2.punchParry = 0;
            fighter2.kickParry = 0;
        }
    }
}
