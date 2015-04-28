using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DaBlackjack
{
    /// <summary>
    /// A class for a playing card
    /// </summary>
    public class Card
    {
        #region Fields

        Rank rank;
        Suit suit;
        bool faceUp;

        // drawing support
        Texture2D faceUpSprite;
        Texture2D faceDownSprite;
        Rectangle drawRectangle = new Rectangle();

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a card with the given rank and suit centered on the given x and y
        /// </summary>
        /// <param name="contentManager">the content manager for loading content</param>
        /// <param name="rank">the rank</param>
        /// <param name="suit">the suit</param>
        /// <param name="x">the x location</param>
        /// <param name="y">the y location</param>
        public Card(ContentManager contentManager, Rank rank, Suit suit, int x, int y)
        {
            this.rank = rank;
            this.suit = suit;
            faceUp = false;

            // load content and set draw rectangle location
            LoadContent(contentManager);
            drawRectangle.X = x - drawRectangle.Width / 2;
            drawRectangle.Y = y - drawRectangle.Height / 2;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the rank of the card
        /// </summary>
        public Rank Rank
        {
            get { return rank; }
        }
        
        /// <summary>
        /// Gets the suit of the card
        /// </summary>
        public Suit Suit
        {
            get { return suit; }
        }

        /// <summary>
        /// Sets the centered x location of the card
        /// </summary>
        public int X
        {
            set { drawRectangle.X = value - drawRectangle.Width / 2; }
        }

        /// <summary>
        /// Sets the centered y location of the card
        /// </summary>
        public int Y
        {
            set { drawRectangle.Y = value - drawRectangle.Height / 2; }
        }

        /// <summary>
        /// Gets the width of the card
        /// </summary>
        public int Width
        {
            get { return drawRectangle.Width; }
        }

        /// <summary>
        /// Gets whether the card is face up
        /// </summary>
        public bool FaceUp
        {
            get { return faceUp; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Draws the card
        /// </summary>
        /// <param name="spriteBatch">the sprite batch</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (FaceUp)
            {
                spriteBatch.Draw(faceUpSprite, drawRectangle, Color.White);
            }
            else
            {
                spriteBatch.Draw(faceDownSprite, drawRectangle, Color.White);
            }
        }

        /// <summary>
        /// Flips the card over
        /// </summary>
        public void FlipOver()
        {
            faceUp = !faceUp;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Loads the content for the card and sets draw rectangle size
        /// </summary>
        /// <param name="contentManager">the content manager to use</param>
        private void LoadContent(ContentManager contentManager)
        {
            // load content and set draw rectangle size
            string deckName = "xpat2-nox";
            string spriteName = "decks\\" + deckName + "\\" + suit.ToString() + "/" + rank.ToString();
            faceUpSprite = contentManager.Load<Texture2D>(spriteName);
            drawRectangle.Width = faceUpSprite.Width;
            drawRectangle.Height = faceUpSprite.Height;
            faceDownSprite = contentManager.Load<Texture2D>("decks\\" + deckName +"\\Backs\\Back");
        }

        #endregion
    }
}
