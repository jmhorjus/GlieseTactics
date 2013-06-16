using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    public class UnitStatsDisplayPanel : ClickableSprite ///It's just easier to inherit from clickablesprite.
    {
        /// Static fonts - initialized once and used for all UnitStatsDisplayPanel objects.
        static SpriteFont s_unitNameFont;
        static SpriteFont s_unitStatsFont;
        public static void InitStaticFonts(SpriteFont unitNameFont, SpriteFont unitStatsFont)
        {
            s_unitNameFont = unitNameFont;
            s_unitStatsFont = unitStatsFont;
        }


        TextLabel UnitNameLabel;
        TextLabel UnitStatsLabel;

        protected Unit m_unitToDisplay = null;
        protected float m_unitRotationInDisplay = 0;
        public Unit UnitToDisplay
        {
            get { return m_unitToDisplay; }
            set 
            {
                m_unitToDisplay = value;
                UnitNameLabel.Text = value.Name;
                UnitStatsLabel.Text = "HP: " + value.CurrentHP + " / " + value.MaxHP;
                
                if (value.CurrentRechargeTime == 0)
                    UnitStatsLabel.Text += "\nRecharge: " + value.MaxRechargeTime + " turns";
                else
                    UnitStatsLabel.Text += "\nRecharge: " + value.CurrentRechargeTime + " / " + value.MaxRechargeTime;

                UnitStatsLabel.Text += "\nMove Speed: " + (value.MoveTemplate as RangeTemplate).Range;

                RangeTemplate attackRange = value.TargetTemplate as RangeTemplate;
                if (value.TypeOfUnit == UnitType.Artillery)
                    UnitStatsLabel.Text += "\nAttack: Area Effect, 4";
                else if (value.TypeOfUnit == UnitType.Commander)
                    UnitStatsLabel.Text += "\nAttack: All in Line, 5";
                else if (value.TypeOfUnit == UnitType.Mech)
                    UnitStatsLabel.Text += "\nAttack: All in Line, 5";
                else if (value.TypeOfUnit == UnitType.Tank)
                    UnitStatsLabel.Text += "\nAttack: First in Line, 3";
                else 
                    UnitStatsLabel.Text += "\nAttack: Any in Range, " + attackRange.Range;

                UnitDamageEffect attackEffect = value.AttackEffect as UnitDamageEffect;
                if (attackEffect != null)
                    UnitStatsLabel.Text += "\nDamage: " + attackEffect.BaseDamage;
                else
                    UnitStatsLabel.Text += "\nDamage: SPECIAL";
            }
        }

        public UnitStatsDisplayPanel(Vector2 position)
            : base(TextureStore.Get(TexId.unit_stats_frame), position, Color.White, 1f, Vector2.One, 0f, Vector2.Zero, 1f )
        {
            UnitNameLabel = new TextLabel("Unit Name", s_unitNameFont, Position + new Vector2(12f, 8f), Color.Black);
            UnitStatsLabel = new TextLabel(" HP: - / - \n Recharge: - / - \n Move Speed: - \n Attack: - \n Damage: -",
                s_unitStatsFont, Position + new Vector2(115f, 30f), Color.Black);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            // The base texture of this ClickableSprite object is the 
            base.Draw(spriteBatch, time);

            UnitNameLabel.Draw(spriteBatch, time);
            UnitStatsLabel.Draw(spriteBatch, time);

            // We draw the texture used by the unit, not the unit itself (which draws on the map).
            if (m_unitToDisplay != null)
            {
                spriteBatch.Draw(m_unitToDisplay.Texture,
                    Position + new Vector2(15f, 59f) + new Vector2(40f), 
                    null,
                    m_unitToDisplay.Owner.UnitColor,
                    m_unitRotationInDisplay,
                    new Vector2(m_unitToDisplay.Texture.Height/2),
                    (float)(Hex.HEX_SIZE) / m_unitToDisplay.Texture.Height,
                    SpriteEffects.None, 0f);
                // Slowly rotate the unit in the display.
                m_unitRotationInDisplay += (float)time.ElapsedGameTime.TotalSeconds;
            }
        }

    }
}
