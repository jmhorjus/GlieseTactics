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

namespace Gliese581g
{
    public class MapEnvironment
    {
        public static readonly MapEnvironment[] Environments = new MapEnvironment[3] 
        { 
            // Waterfall, sand, water
            //new MapEnvironment(TextureStore.Get(TexId.background_waterfall), Color.Gray, 
            //TextureStore.Get(TexId.hex_sand), TextureStore.Get(TexId.hex_water_blue)),
            // Rocky river, sand, rocks
            new MapEnvironment(TextureStore.Get(TexId.background_rocky_river), Color.LightGray, 
            TextureStore.Get(TexId.hex_sand), TextureStore.Get(TexId.hex_rock)),
            // Snow river, snow, rocks
            new MapEnvironment(TextureStore.Get(TexId.background_river_snow), Color.Gray, 
            TextureStore.Get(TexId.hex_snow), TextureStore.Get(TexId.hex_rock)),
            // Swamp!
            //new MapEnvironment(TextureStore.Get(TexId.background_swamp), Color.White, 
            //TextureStore.Get(TexId.hex_swamp), TextureStore.Get(TexId.hex_water)),
            // trees/water?
            new MapEnvironment(TextureStore.Get(TexId.background_mountain_river), Color.Gray, 
            TextureStore.Get(TexId.hex_trees_2), TextureStore.Get(TexId.hex_water_blue)),

        };
        public const int Count = 3;

        public static MapEnvironment GetRandomEnvironment()
        { return Environments[new Random().Next(Count)]; }


        // The background image and tint that the map screen will use.  
        public Texture2D BackgroundTexture;// = TextureStore.Get(TexId.background_waterfall);
        public Color BackgroundTint;// = Color.Gray;

        // Textures for blocking and non-blocking hexes.
        public Texture2D DefaultHexTexture;// = TextureStore.Get(TexId.hex_snow);
        public Texture2D BlockingHexTexture;// = TextureStore.Get(TexId.hex_rock);

        // Used if the environment has special music.
        public Song Music;// = Content.Load<Song>("music/battle1");


        MapEnvironment(Texture2D background, Color backgroundTint, Texture2D defaultHexTexture, Texture2D blockingHexTexture, Song music = null)
        {
            BackgroundTexture = background;
            BackgroundTint = backgroundTint;
            DefaultHexTexture = defaultHexTexture;
            BlockingHexTexture = blockingHexTexture;
            Music = music;
        }

    }
}
