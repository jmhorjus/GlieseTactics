using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gliese581g
{
    class OriginalMapClass 
	{
		public static int _leftHexagonWidth = 76;
		public static int _topHexagonHeight = 44;
		public static int _hexagonHeight = 88;
		public static int _hexagonSlopHeight = 25;
		public static float _hexagonSlope;
		public static int _MapWidth = 6;
		public static int _MapHight = 8;

		public static void DrawMap(SpriteBatch spriteBatch, Texture2D hexTexture, SpriteFont font)
		{

			for (int i = 0; i < _MapWidth; i++)
			{
				for (int j = 0; j < _MapHight; j++)
				{
					var _alternate = i % 2;
					var _pos_x = (_hexagonHeight * j) + (_alternate * _topHexagonHeight);
					var _pos_y = (_leftHexagonWidth * i);
					var _position = new Vector2(_pos_x, _pos_y);
                    spriteBatch.Draw(hexTexture, _position, Color.White);
                    spriteBatch.DrawString(font, j + ":" + i, _position + new Vector2(0, 20), Color.Green);
				}
			}
		}


	}

}
