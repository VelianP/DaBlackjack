using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using XnaCards;

namespace DaBlackjack
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        const int WINDOW_WIDTH = 800;
        const int WINDOW_HEIGHT = 600;

        // max valid blackjack score for a hand
        const int MAX_HAND_VALUE = 21;

        // deck and hands
        Deck deck;
        List<Card> dealerHand = new List<Card>();
        List<Card> playerHand = new List<Card>();

        // hand placement
        const int TOP_CARD_OFFSET = 100;
        const int HORIZONTAL_CARD_OFFSET = 150;
        const int VERTICAL_CARD_SPACING = 100;

        // messages
        SpriteFont messageFont;
        const string SCORE_MESSAGE_PREFIX = "Score: ";
        Message playerScoreMessage;
        Message playerBustMessage;
        Message WinnerMessage;
        Message FeedbackMessage;
        Message handsWonMessage;
        List<Message> messages = new List<Message>();

        // message placement
        const int SCORE_MESSAGE_TOP_OFFSET = 25;
        const int HORIZONTAL_MESSAGE_OFFSET = HORIZONTAL_CARD_OFFSET;
        Vector2 winnerMessageLocation = new Vector2(WINDOW_WIDTH/2, WINDOW_HEIGHT/2);
        Vector2 loserMessageLocation = new Vector2(WINDOW_WIDTH/2, WINDOW_HEIGHT/3);

        // menu buttons
        Texture2D quitButtonSprite;
        List<MenuButton> menuButtons = new List<MenuButton>();

        private Texture2D hitButtonSprite;
        private Texture2D standButtonSprite;

        // menu button placement
        const int TOP_MENU_BUTTON_OFFSET = TOP_CARD_OFFSET;
        const int QUIT_MENU_BUTTON_OFFSET = WINDOW_HEIGHT - TOP_CARD_OFFSET;
        const int HORIZONTAL_MENU_BUTTON_OFFSET = WINDOW_WIDTH / 2;
        const int VERTICAL_MENU_BUTTON_SPACING = 125;

        // use to detect hand over when player and dealer didn't hit
        bool playerHit = false;
        bool dealerHit = false;

        // game state tracking
        static GameState currentState = GameState.WaitingForPlayer;

        // tmp card
        private Card tmpCard = null;

        //
        MenuButton quitButton = null;
        MenuButton playButton = null;

        //
        private int PlayerHandsWon = 0;
        private int DealerHandsWon = 0;
        private int DealerScore = 0;
        private int PlayerScore = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // set resolution and show mouse
            graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            graphics.PreferredBackBufferWidth = WINDOW_WIDTH;

            IsMouseVisible = true;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            InitNewGame();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // update menu buttons as appropriate
            MouseState mouse = Mouse.GetState();

            if (currentState == GameState.WaitingForPlayer || currentState == GameState.DisplayingHandResults)
            {
                foreach (MenuButton menuButton in menuButtons)
                {
                    menuButton.Update(mouse);
                }
            }

            // game state-specific processing
            DealerScore = GetBlackjackScore(dealerHand);
            PlayerScore = GetBlackjackScore(playerHand);

            switch (currentState)
            {
                case GameState.WaitingForPlayer:
                    {
                        messages[3].Text = "waiting for player (to hit or stand)";
                        if (playerHand.Count == 5 && PlayerScore <= MAX_HAND_VALUE)
                        {
                            messages[2].Text = "PLAYER HAS 5 CARDS!";
                            PlayerHandsWon += 1;
                            ChangeState(GameState.DisplayingHandResults);
                        }
                        else if (PlayerScore == MAX_HAND_VALUE)
                        {
                            ChangeState(GameState.WaitingForDealer);
                        }
                        else if (PlayerScore > MAX_HAND_VALUE)
                        {
                            messages[2].Text = "PLAYER HAS BUSTED!";
                            DealerHandsWon += 1;
                            ChangeState(GameState.DisplayingHandResults);
                        }

                        break;
                    }

                case GameState.PlayerHitting:
                {
                    messages[3].Text = "player hit";
                    dealACard("Player");
                    ChangeState(GameState.WaitingForPlayer);
                    if (PlayerScore > MAX_HAND_VALUE)
                    {
                        messages[2].Text = "PLAYER HAS BUSTED!";
                        DealerHandsWon += 1;
                        ChangeState(GameState.DisplayingHandResults);
                    }

                    break;
                }

                case GameState.DealerHitting:
                {
                    messages[3].Text = "dealer hit";
                    dealACard("Dealer");
                    ChangeState(GameState.WaitingForDealer);

                    break;
                } 


                case GameState.WaitingForDealer:
                {
                    messages[3].Text = "waiting for dealer to hit or stand";
  
                    if (DealerScore <= 16 && dealerHand.Count < 5)
                    {
                        ChangeState(GameState.DealerHitting);
                    }
                    else
                    {
                        ChangeState(GameState.CheckingHandOver);
                    }
                    break;
                }

                case GameState.CheckingHandOver:
                {
                    //messages[3].Text = "deciding";
 
                    if (DealerScore > MAX_HAND_VALUE)
                    {
                        messages[2].Text = "DEALER HAS BUSTED!";
                        PlayerHandsWon += 1;
                        ChangeState(GameState.DisplayingHandResults);
                    }
                    else if (PlayerScore == MAX_HAND_VALUE && DealerScore != MAX_HAND_VALUE)
                    {
                        messages[2].Text = "PLAYER HAS 21!";
                        PlayerHandsWon += 1;
                        ChangeState(GameState.DisplayingHandResults);
                    }
                    else if (PlayerScore > DealerScore)
                    {
                        messages[2].Text = "PLAYER WINS!";
                        PlayerHandsWon += 1;
                        ChangeState(GameState.DisplayingHandResults);
                    }
                    else if (PlayerScore < DealerScore)
                    {
                        messages[2].Text = "DEALER WINS!";
                        DealerHandsWon += 1;
                        ChangeState(GameState.DisplayingHandResults);
                    }
                    else if (PlayerScore == DealerScore)
                    {
                        messages[2].Text = "ITS A DRAW!";
                        ChangeState(GameState.DisplayingHandResults);
                    }
                    messages[4].Text = PlayerHandsWon.ToString() + " : " + DealerHandsWon.ToString();
                    break;
                }

                case GameState.DisplayingHandResults:
                {
                    if (!dealerHand[0].FaceUp)
                    {
                        dealerHand[0].FlipOver();
                        messages[1].Text = SCORE_MESSAGE_PREFIX + GetBlackjackScore(dealerHand).ToString();
                    }

                    menuButtons.Clear();
                    menuButtons.Add(quitButton);

                    foreach (MenuButton button in menuButtons)
                    {
                        button.Update(mouse);
                    }

                    messages[3].Text = "Right click to play again!";
                    menuButtons.Add(quitButton);

                    // restart the game on right click
                    if (mouse.RightButton == ButtonState.Pressed)
                    {
                        InitNewGame();
                    }

                    break;
                }

                case GameState.Exiting:
                {
                    Console.WriteLine("Exiting State");
                    this.Exit();
                    break;
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Goldenrod);

            spriteBatch.Begin();

            // draw hands
            foreach (Card card in playerHand)
            {
                card.Draw(spriteBatch);
            }

            foreach (Card card in dealerHand)
            {
                card.Draw(spriteBatch);
            }


            // draw messages
            foreach (Message message in messages)
            {
                message.Draw(spriteBatch);
            }

            // draw menu buttons
            foreach (MenuButton menuButton in menuButtons)
            {
                menuButton.Draw(spriteBatch);
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Calculates the Blackjack score for the given hand
        /// </summary>
        /// <param name="hand">the hand</param>
        /// <returns>the Blackjack score for the hand</returns>
        private int GetBlackjackScore(List<Card> hand)
        {
            // add up score excluding Aces
            int numAces = 0;
            int score = 0;
            foreach (Card card in hand)
            {
                if (card.Rank != Rank.Ace)
                {
                    score += GetBlackjackCardValue(card);
                }
                else
                {
                    numAces++;
                }
            }

            // if more than one ace, only one should ever be counted as 11
            if (numAces > 1)
            {
                // make all but the first ace count as 1
                score += numAces - 1;
                numAces = 1;
            }

            // if there's an Ace, score it the best way possible
            if (numAces > 0)
            {
                if (score + 11 <= MAX_HAND_VALUE)
                {
                    // counting Ace as 11 doesn't bust
                    score += 11;
                }
                else
                {
                    // count Ace as 1
                    score++;
                }
            }

            return score;
        }

        /// <summary>
        /// Gets the Blackjack value for the given card
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>the Blackjack value for the card</returns>
        private int GetBlackjackCardValue(Card card)
        {
            switch (card.Rank)
            {
                case Rank.Ace:
                    return 11;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    return 10;
                case Rank.Nine:
                    return 9;
                case Rank.Eight:
                    return 8;
                case Rank.Seven:
                    return 7;
                case Rank.Six:
                    return 6;
                case Rank.Five:
                    return 5;
                case Rank.Four:
                    return 4;
                case Rank.Three:
                    return 3;
                case Rank.Two:
                    return 2;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Changes the state of the game
        /// </summary>
        /// <param name="newState">the new game state</param>
        public static void ChangeState(GameState newState)
        {
            currentState = newState;
        }

        private void dealACard(string N = "Player")
        {
            if (N == "Player")
            {
                // player
                tmpCard = deck.TakeTopCard();
                tmpCard.FlipOver();
                tmpCard.X = HORIZONTAL_CARD_OFFSET;
                tmpCard.Y = (playerHand.Count * VERTICAL_CARD_SPACING) + TOP_CARD_OFFSET;
                playerHand.Add(tmpCard);

                messages[0].Text = SCORE_MESSAGE_PREFIX + GetBlackjackScore(playerHand).ToString();

                playerHit = true;

            }
            else
            {
                // dealer
                tmpCard = deck.TakeTopCard();
                tmpCard.FlipOver();
                tmpCard.X = WINDOW_WIDTH - HORIZONTAL_CARD_OFFSET;
                tmpCard.Y = (TOP_CARD_OFFSET) + (dealerHand.Count * VERTICAL_CARD_SPACING);
                dealerHand.Add(tmpCard);

                messages[1].Text = SCORE_MESSAGE_PREFIX + GetBlackjackScore(dealerHand).ToString();

                dealerHit = true;

            }
        }

        private void InitNewGame()
        {
            menuButtons.Clear();
            messages.Clear();

            // create and shuffle deck
            int deckX = 1;
            int deckY = 1;
            deck = new Deck(Content, deckX, deckY);
            deck.Shuffle();

            dealerHand = new List<Card>();
            playerHand = new List<Card>();

            // first player card
            playerHand.Add(deck.TakeTopCard());
            playerHand[0].FlipOver();
            playerHand[0].X = HORIZONTAL_CARD_OFFSET;
            playerHand[0].Y = TOP_CARD_OFFSET;


            // first dealer card
            dealerHand.Add(deck.TakeTopCard());
            dealerHand[0].X = WINDOW_WIDTH - HORIZONTAL_CARD_OFFSET;
            dealerHand[0].Y = TOP_CARD_OFFSET;


            // second player card
            playerHand.Add(deck.TakeTopCard());
            playerHand[1].FlipOver();
            playerHand[1].X = HORIZONTAL_CARD_OFFSET;
            playerHand[1].Y = TOP_CARD_OFFSET + VERTICAL_CARD_SPACING;


            // second dealer card
            dealerHand.Add(deck.TakeTopCard());
            dealerHand[1].FlipOver();
            dealerHand[1].X = WINDOW_WIDTH - HORIZONTAL_CARD_OFFSET;
            dealerHand[1].Y = TOP_CARD_OFFSET + VERTICAL_CARD_SPACING;

            // load sprite font, create message for player score and add to list
            messageFont = Content.Load<SpriteFont>("fonts\\Arial24");

            playerScoreMessage = new Message(SCORE_MESSAGE_PREFIX + GetBlackjackScore(playerHand).ToString(), messageFont, new Vector2(HORIZONTAL_MESSAGE_OFFSET, SCORE_MESSAGE_TOP_OFFSET));
            messages.Add(playerScoreMessage);

            Message dealerScoreMessage = new Message("Score: " + (GetBlackjackScore(dealerHand) - GetBlackjackCardValue(dealerHand[0])).ToString(), messageFont, new Vector2(WINDOW_WIDTH - HORIZONTAL_MESSAGE_OFFSET, SCORE_MESSAGE_TOP_OFFSET));
            messages.Add(dealerScoreMessage);

            WinnerMessage = new Message("", messageFont, winnerMessageLocation);
            messages.Add(WinnerMessage);

            FeedbackMessage = new Message("", messageFont, new Vector2(WINDOW_WIDTH / 2, WINDOW_HEIGHT * (float)0.965));
            messages.Add(FeedbackMessage);

            handsWonMessage = new Message(PlayerHandsWon.ToString() + " : " + DealerHandsWon.ToString(), messageFont, new Vector2(WINDOW_WIDTH/2, SCORE_MESSAGE_TOP_OFFSET));
            messages.Add(handsWonMessage);

            // load quit button sprite for later use
            quitButtonSprite = Content.Load<Texture2D>("buttons\\quitbutton");
            quitButton = new MenuButton(quitButtonSprite, new Vector2(HORIZONTAL_MENU_BUTTON_OFFSET, QUIT_MENU_BUTTON_OFFSET), GameState.Exiting);

            // create hit button and add to list
            hitButtonSprite = Content.Load<Texture2D>("buttons\\hitbutton");
            Vector2 hitButtonCenter = new Vector2(HORIZONTAL_MENU_BUTTON_OFFSET, TOP_MENU_BUTTON_OFFSET);
            menuButtons.Add(new MenuButton(hitButtonSprite, hitButtonCenter, GameState.PlayerHitting));

            // create stand button and add to list
            standButtonSprite = Content.Load<Texture2D>("buttons\\standbutton");
            Vector2 standButtonCenter = new Vector2(HORIZONTAL_MENU_BUTTON_OFFSET, VERTICAL_MENU_BUTTON_SPACING + TOP_MENU_BUTTON_OFFSET);
            menuButtons.Add(new MenuButton(standButtonSprite, standButtonCenter, GameState.WaitingForDealer));

            ChangeState(GameState.WaitingForPlayer);
        }
    }
}
