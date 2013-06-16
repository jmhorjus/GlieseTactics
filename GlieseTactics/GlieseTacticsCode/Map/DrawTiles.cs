using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HexMapsGame
{
	class DrawTiles : HexMapsGame
	{
		public static int _leftHexagonWidth = 76;
		public static int _topHexagonHeight = 44;
		public static int _hexagonHeight = 88;
		public static int _hexagonSlopHeight = 25;
		public static float _hexagonSlope;
		public static int _MapWidth = 6;
		public static int _MapHight = 8;
		public static Vector2 _scrollOffset;

		public static void tiles()
		{

			for (int i = 0; i < _MapWidth; i++)
			{
				for (int j = 0; j < _MapHight; j++)
				{
					var _alternate = i % 2;
					var _pos_x = (_hexagonHeight * j) + (_alternate * _topHexagonHeight) + _scrollOffset.X;
					var _pos_y = (_leftHexagonWidth * i) + _scrollOffset.Y;
					var _position = new Vector2(_pos_x, _pos_y);
					HexMapsGame.spriteBatch.Draw(HexMapsGame._hexagon, _position, Color.White);
					HexMapsGame.spriteBatch.DrawString(HexMapsGame.font, j + ":" + i, _position + new Vector2(0, 20), Color.Green);
				}
			}
		}


	}

}
