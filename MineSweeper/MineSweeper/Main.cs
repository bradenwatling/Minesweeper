using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MineSweeper
{
    public class Main : Microsoft.Xna.Framework.Game
    {
        const int GridWidth = 20;
        const int GridHeight = 20;
        const int NumMines = 50;
        const int TileWidth = 16;
        const int MenuHeight = 0;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D tileTexture;
        Texture2D flagTexture;
        Texture2D mineTexture;
        Texture2D noneTexture;
        Texture2D oneTexture;
        Texture2D twoTexture;
        Texture2D threeTexture;
        Texture2D fourTexture;
        Texture2D fiveTexture;
        Texture2D sixTexture;
        Texture2D sevenTexture;
        Texture2D eightTexture;

        public class Tile
        {
            public Texture2D tex;
            public Rectangle rect;
            public bool clicked = false;
            public bool flagged = false;
            public bool mine;
        };

        List<Tile> tiles;

        MouseState lastMouseState = new MouseState();

        int numFlags;
        bool win;
        bool lose;


        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        public void init()
        {
            tiles = new List<Tile>();
            List<Vector2> mines = new List<Vector2>();

            numFlags = 0;
            win = false;
            lose = false;

            for (int i = 0; i < NumMines; ++i)
            {
                Random rand = new Random();
                Vector2 pos = new Vector2(rand.Next() % GridWidth, rand.Next() % GridHeight);
                if (mines.Contains(pos))
                    --i;
                else
                    mines.Add(pos);
            }

            for (int x = 0; x < GridWidth; ++x)
            {
                for (int y = 0; y < GridHeight; ++y)
                {
                    Tile tile = new Tile();
                    int numMines = 0;
                    for (int a = x - 1; a <= x + 1; ++a)
                    {
                        for (int b = y - 1; b <= y + 1; ++b)
                        {
                            if (mines.Contains(new Vector2(a, b)))
                            {
                                ++numMines;
                            }
                        }
                    }
                    switch (numMines)
                    {
                        case 0:
                            tile.tex = noneTexture;
                            break;
                        case 1:
                            tile.tex = oneTexture;
                            break;
                        case 2:
                            tile.tex = twoTexture;
                            break;
                        case 3:
                            tile.tex = threeTexture;
                            break;
                        case 4:
                            tile.tex = fourTexture;
                            break;
                        case 5:
                            tile.tex = fiveTexture;
                            break;
                        case 6:
                            tile.tex = sixTexture;
                            break;
                        case 7:
                            tile.tex = sevenTexture;
                            break;
                        case 8:
                            tile.tex = eightTexture;
                            break;
                    }
                    tile.rect = new Rectangle(x * TileWidth, MenuHeight + y * TileWidth, TileWidth, TileWidth);
                    tile.mine = mines.Contains(new Vector2(x, y));
                    if (tile.mine)
                        tile.tex = mineTexture;
                    tiles.Add(tile);
                }
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = GridWidth * TileWidth;
            graphics.PreferredBackBufferHeight = MenuHeight + GridHeight * TileWidth;
            graphics.ApplyChanges();

            init();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            tileTexture = Content.Load<Texture2D>("tile");
            flagTexture = Content.Load<Texture2D>("flag");
            mineTexture = Content.Load<Texture2D>("mine");
            noneTexture = Content.Load<Texture2D>("none");
            oneTexture = Content.Load<Texture2D>("one");
            twoTexture = Content.Load<Texture2D>("two");
            threeTexture = Content.Load<Texture2D>("three");
            fourTexture = Content.Load<Texture2D>("four");
            fiveTexture = Content.Load<Texture2D>("five");
            sixTexture = Content.Load<Texture2D>("six");
            sevenTexture = Content.Load<Texture2D>("seven");
            eightTexture = Content.Load<Texture2D>("eight");
        }

        protected override void UnloadContent()
        {
        }

        public void checkTile(Tile tile)
        {
            if(tile.clicked)
                return;

            if (tile.flagged)
                --numFlags;

            tile.clicked = true;

            if(tile.tex == noneTexture)
                checkAdjacentTiles(tile);
        }

        public void checkAdjacentTiles(Tile tile)
        {
            foreach (Tile t in tiles)
            {
                if (Vector2.Distance(new Vector2(tile.rect.X, tile.rect.Y), new Vector2(t.rect.X, t.rect.Y)) <= TileWidth * Math.Sqrt(2))
                    checkTile(t);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            MouseState mouseState = Mouse.GetState();

            win = true;

            for (int i = 0; i < tiles.Count; ++i)
            {
                Tile tile = tiles.ElementAt(i);
                bool inTile = tile.rect.Contains(mouseState.X, mouseState.Y);
                if (inTile && lastMouseState.LeftButton == ButtonState.Released && mouseState.LeftButton == ButtonState.Pressed)
                {
                    tile.clicked = true;
                    if (tile.flagged)
                    {
                        tile.flagged = !tile.flagged;
                        --numFlags;
                    }

                    if(tile.tex == noneTexture)
                        checkAdjacentTiles(tile);
                }
                if (inTile && (tile.flagged || numFlags < NumMines) && lastMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed)
                {
                    tile.flagged = !tile.flagged;
                    numFlags += tile.flagged ? 1 : -1;
                }

                if (tile.mine && tile.clicked)
                    lose = true;

                win = tile.mine && !tile.flagged ? false : win;
            }

            lastMouseState = mouseState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            spriteBatch.Begin();
            foreach (Tile tile in tiles)
            {
                Texture2D tex = tileTexture;

                if (tile.clicked)
                    tex = tile.tex;
                else if (tile.flagged)
                    tex = flagTexture;

                if (!tile.mine && win)
                    tex = tile.tex;

                if (lose)
                    tex = tile.tex;

                spriteBatch.Draw(tex, tile.rect, Color.White);
            }
            spriteBatch.End();

            if (win || lose)
            {
                //init();
            }

            base.Draw(gameTime);
        }
    }
}
