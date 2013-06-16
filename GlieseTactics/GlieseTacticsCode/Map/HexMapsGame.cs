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

namespace HexMapsGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class HexMapsGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		public static SpriteBatch spriteBatch;
		public static Texture2D _hexagon;
		public static SpriteFont font;

        Camera _camera;

		public HexMapsGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
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
			_hexagon = Content.Load<Texture2D>(@"Images\Hexagon");
			DrawTiles._hexagonSlope = (float)DrawTiles._topHexagonHeight / (float)DrawTiles._hexagonSlopHeight;
			DrawTiles._scrollOffset = new Vector2(20, 0);
			IsMouseVisible = true;
			font = Content.Load<SpriteFont>("text");
			//-----------------
            _camera = new Camera(GraphicsDevice.Viewport);
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

			// TODO: Add your update logic here
			var _mouse = Mouse.GetState();
			var _pointer = new Point(_mouse.X, _mouse.Y);
            _camera.Update(new Vector2 (GraphicsDevice.Viewport.Width /2, GraphicsDevice.Viewport.Height /2));

            if (Keyboard.GetState().IsKeyDown(Keys.W))
                _camera.Zoom += 0.01f;
            else if (Keyboard.GetState().IsKeyDown(Keys.S))
                _camera.Zoom -= 0.01f;

            if (Keyboard.GetState().IsKeyDown(Keys.A))
                _camera.Rotation += 0.01f;
            else if (Keyboard.GetState().IsKeyDown(Keys.D))
                _camera.Rotation -= 0.01f;

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{

			GraphicsDevice.Clear(Color.CornflowerBlue);
			spriteBatch.Begin(SpriteSortMode.Deferred,
                              BlendState.AlphaBlend,
                              null,null,null,null,
                              _camera.Transform
                              );
			DrawTiles.tiles();
			spriteBatch.End();
			// TODO: Add your drawing code here

			base.Draw(gameTime);
		}




	}
}
