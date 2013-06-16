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
using System.IO;

namespace Gliese581g
{
    class ConfettiFountain : IDrawnObject
    {
        public bool Enabled;
        public bool Visible;

        Vector2 m_position;
        float m_timePeriod;

        List<ConfettiBit> m_bits = new List<ConfettiBit>();

        public ConfettiFountain(Vector2 position, float bitsPerSec = 20f)
        {
            m_position = position;
            m_timePeriod = 1 / bitsPerSec;
            Enabled = false;
            Visible = false;

            ConfettiBit.InitTextures();
        }


        public void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            if (!Enabled)
                return;

            float elapsedTime = (float)time.ElapsedGameTime.TotalSeconds;
            float totalTime = (float)time.TotalGameTime.TotalSeconds;
            if ((totalTime % m_timePeriod) > ((totalTime + elapsedTime) % m_timePeriod))
                m_bits.Add(new ConfettiBit(m_position));

            //foreach (ConfettiBit bit in m_bits)
            for (int ii = 0; ii < m_bits.Count; ii++)
            {
                if (m_bits[ii].LlifetimeRemaining <= TimeSpan.Zero)
                {
                    m_bits.RemoveAt(ii--);
                    continue;
                }
                if (Visible)
                    m_bits[ii].Draw(spriteBatch, time);
            }

        }




        protected class ConfettiBit : IDrawnObject
        {
            Texture2D m_texture;
            Vector2 m_origin;
            Vector2 m_position;
            Vector2 m_velocity;
            Vector2 m_acceleration;

            float m_scale;
            float m_rotation;
            float m_spin;
            public TimeSpan LlifetimeRemaining;

            static Texture2D[] s_texList;
            const int TEXLIST_SIZE = 6;

            public static void InitTextures()
            {
                if (s_texList != null)
                    return;

                s_texList = new Texture2D[TEXLIST_SIZE] { 
                    TextureStore.Get(TexId.confetti_1),
                    TextureStore.Get(TexId.confetti_2),
                    TextureStore.Get(TexId.confetti_3),
                    TextureStore.Get(TexId.confetti_4),
                    TextureStore.Get(TexId.confetti_5),
                    TextureStore.Get(TexId.confetti_6)
                };
            }

            public ConfettiBit(Vector2 position)
            {
                Random rand = new Random();
                m_texture = s_texList[rand.Next(TEXLIST_SIZE)];
                m_origin = new Vector2(m_texture.Width/2, m_texture.Height/2);
                m_position = position;
                m_velocity = new Vector2(rand.Next(-100,100), rand.Next(-240,-80));
                m_acceleration = new Vector2(0,70f);

                m_scale = rand.Next(2,8) * 0.04f;
                m_rotation = 0f;
                m_spin = rand.Next(3,9) * .75f;
                LlifetimeRemaining = new TimeSpan(0, 0, 5);
            }

            public void Draw(SpriteBatch spriteBatch, GameTime time)
            {
                spriteBatch.Draw(m_texture, m_position, null, Color.White, m_rotation, m_origin, m_scale, SpriteEffects.None, 0f);

                float elapsedTime = (float)time.ElapsedGameTime.TotalSeconds;

                m_position += m_velocity * elapsedTime;
                m_velocity += m_acceleration * elapsedTime;
                m_rotation += m_spin * elapsedTime;

                LlifetimeRemaining -= time.ElapsedGameTime;
            }
        }

    }
}
