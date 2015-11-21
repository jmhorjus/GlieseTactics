using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Gliese581g
{
    public enum UnitType
    {
        EmptyUnit = 0,
        Commander = 1,
        Infantry = 2,
        Scout = 3,
        RoughRider = 4,
        Tank = 5,
        Artillery = 6,
        Mech = 7
    }


    // Unit needs to inherit from ClickableSprite especially for Animation support.
    [Serializable]
    public class Unit : ClickableSprite
    {
        public class UnitFactory
        {
            public static Unit MakeUnit(UnitType type)
            {
                switch (type)
                {
                    case UnitType.EmptyUnit:
                        return MakeEmptyUnit();
                    case UnitType.Commander:
                        return MakeCommander("");
                    case UnitType.Infantry:
                        return MakeInfantry();
                    case UnitType.Scout:
                        return MakeScout();
                    case UnitType.RoughRider:
                        return MakeRoughRider();
                    case UnitType.Tank:
                        return MakeTank();
                    case UnitType.Artillery:
                        return MakeArtillery();
                    case UnitType.Mech:
                        return MakeMech();
                    default:
                        throw new Exception("invalid unit type");
                }
            }

            public static Unit MakeEmptyUnit()
            {
                return new Unit();
            }

            static int infantry_id = 0;
            public static Unit MakeInfantry()
            {
                Unit infantry = new Unit(
                    UnitType.Infantry,
                    "Infantry Battalion " + infantry_id++, //name
                    TextureStore.Get(TexId.unit_infantry), //texture
                    new RangeTemplate(4, false, false, true, null, true),  //move
                    new RangeTemplate(1, true, true, false, null, true), //target template
                    new LineTemplate(1, false, false, true),
                    new UnitDamageEffect(40, false), //damage
                    false,
                    70, //hp
                    Armor.ArmorFactory.MakeArmor(ArmorType.Light),
                    3, //recharge
                    SfxStore.Get(SfxId.infantry_selected_1),
                    SfxStore.Get(SfxId.infantry_fire));
                infantry.SfxSelected.Add(SfxStore.Get(SfxId.infantry_selected_2));
                infantry.SfxMove.Add(SfxStore.Get(SfxId.infantry_move_1));
                infantry.SfxMove.Add(SfxStore.Get(SfxId.infantry_move_2));
                infantry.SfxKilled.Add(SfxStore.Get(SfxId.infantry_killed));
                return infantry;
            }

            static int scout_id = 0;
            public static Unit MakeScout()
            {
                Unit scout = new Unit(
                    UnitType.Scout,
                    "Scout Division " + scout_id++, //name
                    TextureStore.Get(TexId.unit_scout), //texture
                    new RangeTemplate(5, false, false, true, null, true),  //move
                    new RangeTemplate(1, true, true, false, null, true),  //attack range
                    new LineTemplate(1, false, false, true),
                    new UnitDamageEffect(30, false), //damage
                    false,
                    70, //hp
                    Armor.ArmorFactory.MakeArmor(ArmorType.None),
                    2, //recharge
                    SfxStore.Get(SfxId.scout_selected_1),
                    SfxStore.Get(SfxId.infantry_fire));
                scout.SfxSelected.Add(SfxStore.Get(SfxId.scout_selected_2));
                scout.SfxMove.Add(SfxStore.Get(SfxId.scout_move_1));
                scout.SfxMove.Add(SfxStore.Get(SfxId.scout_move_2));
                scout.SfxKilled.Add(SfxStore.Get(SfxId.scout_killed));
                return scout;
            }

            static int roughrider_id = 0;
            public static Unit MakeRoughRider()
            {
                Unit roughrider = new Unit(
                    UnitType.RoughRider,
                    "Rough Rider Division " + roughrider_id++, //name
                    TextureStore.Get(TexId.unit_roughrider), //texture
                    new RangeTemplate(4, false, false, true, null, true),  //move
                    new RangeTemplate(3, true, true, false, null, true),  //attack range
                    new LineTemplate(1, false, false, true),
                    new UnitDamageEffect(35, false), //damage
                    false,
                    65, //hp
                    Armor.ArmorFactory.MakeArmor(ArmorType.Medium),
                    3, //recharge
                    SfxStore.Get(SfxId.roughrider_selected_1),
                    SfxStore.Get(SfxId.rocket_boom));
                roughrider.SfxSelected.Add(SfxStore.Get(SfxId.roughrider_selected_2));
                roughrider.SfxMove.Add(SfxStore.Get(SfxId.roughrider_move_1));
                roughrider.SfxMove.Add(SfxStore.Get(SfxId.roughrider_move_2));
                roughrider.SfxKilled.Add(SfxStore.Get(SfxId.roughrider_killed));
                return roughrider;
            }


            static int tank_id = 0;
            public static Unit MakeTank()
            {
                Unit tank = new Unit(
                    UnitType.Tank,
                    "Armor Battalion " + tank_id++, //name
                    TextureStore.Get(TexId.unit_tank), //texture
                    new RangeTemplate(3, false, false, true, null, true),  //move
                    new LineTemplate(4, true, true, false, true),  //attack range
                    new LineTemplate(1, false, false, true),
                    new UnitDamageEffect(75, false), //damage
                    false,
                    100, //hp
                    Armor.ArmorFactory.MakeArmor(ArmorType.Heavy),
                    5, //recharge
                    SfxStore.Get(SfxId.tank_selected_1),
                    SfxStore.Get(SfxId.tank_fire));
                tank.SfxSelected.Add(SfxStore.Get(SfxId.tank_selected_2));
                tank.SfxMove.Add(SfxStore.Get(SfxId.tank_move_1));
                tank.SfxMove.Add(SfxStore.Get(SfxId.tank_move_2));
                tank.SfxKilled.Add(SfxStore.Get(SfxId.tank_killed));
                return tank;
            }

   
            public static Unit MakeCommander(string playerName)
            {
                Unit commander = new Unit(
                    UnitType.Commander,
                    "Commander " + playerName, //name
                    TextureStore.Get(TexId.unit_commander), //texture
                    new RangeTemplate(3, false, false, true, null, true),  //move
                    new LineTemplate(2, false, false, false, true), //target template
                    new LineTemplate(4, false, false, true),
                    new UnitDamageEffect(65, true), //damage
                    true,
                    140, //hp
                    Armor.ArmorFactory.MakeArmor(ArmorType.Medium),
                    5,//recharge
                    SfxStore.Get(SfxId.commander_selected_1),
                    SfxStore.Get(SfxId.mech_fire));
                commander.SfxSelected.Add(SfxStore.Get(SfxId.commander_selected_2));
                commander.SfxMove.Add(SfxStore.Get(SfxId.commander_move_1));
                commander.SfxMove.Add(SfxStore.Get(SfxId.commander_move_2));
                commander.SfxKilled.Add(SfxStore.Get(SfxId.commander_killed));

                commander.IsCommander = true; // need to set the commander flag
                return commander;
            }

            public static Unit MakeMech()
            {
                Unit mech = new Unit(
                    UnitType.Mech,
                    "Techno-mech Guardian", //name
                    TextureStore.Get(TexId.unit_mech), //texture
                    new RangeTemplate(3, false, false, true, null, true),  //move
                    new LineTemplate(2, false, false, false, true), //target template
                    new LineTemplate(4, false, false, true),
                    new UnitDamageEffect(70, true), //damage
                    true,
                    160, //hp
                    Armor.ArmorFactory.MakeArmor(ArmorType.Heavy),
                    6, //recharge
                    SfxStore.Get(SfxId.mech_selected_1),
                    SfxStore.Get(SfxId.mech_fire));
                mech.SfxSelected.Add(SfxStore.Get(SfxId.mech_selected_2));
                mech.SfxMove.Add(SfxStore.Get(SfxId.mech_move_1));
                mech.SfxMove.Add(SfxStore.Get(SfxId.mech_move_2));
                mech.SfxKilled.Add(SfxStore.Get(SfxId.mech_killed));
                return mech;
            }


            static int artillery_id = 0;
            public static Unit MakeArtillery()
            {
                Unit artillery = new Unit(
                    UnitType.Artillery,
                    "Artillery Battalion " + artillery_id++, //name
                    TextureStore.Get(TexId.unit_artillery), //texture
                    new RangeTemplate(2, false, false, true, null, true),  //move
                    new RangeTemplate(4, true, true, false, null, true),  //attack range
                    new RangeTemplate(1, true, true, true),
                    new UnitDamageEffect(45, true), //damage
                    true,
                    80, //hp
                    Armor.ArmorFactory.MakeArmor(ArmorType.Medium),
                    5,//recharge
                    SfxStore.Get(SfxId.artillery_selected_1),
                    SfxStore.Get(SfxId.artillery_fire));
                artillery.SfxSelected.Add(SfxStore.Get(SfxId.artillery_selected_2));
                artillery.SfxMove.Add(SfxStore.Get(SfxId.artillery_move_1));
                artillery.SfxMove.Add(SfxStore.Get(SfxId.artillery_move_2));
                artillery.SfxKilled.Add(SfxStore.Get(SfxId.artillery_killed));
                return artillery;
            }

        }
        //-------------------------------------------------------------------
        


        protected Commander m_owner;
        public Commander Owner
        {
            get { return m_owner; }
            set
            {
                m_owner = value;
                Tint = value.UnitColor;
            }
        }


        protected Hex m_currentHex = null;
        public Hex CurrentHex { get { return m_currentHex; } }
        public void ClearCurrentHex() 
        {
            Hex tempHex = m_currentHex;
            m_currentHex = null;
            if (tempHex != null && tempHex.Unit == this)
                tempHex.ClearUnit();
        }

        public MapLocation MapLocation
        {
            get 
            {
                if (m_currentHex == null)
                    return null;
                return new MapLocation(m_currentHex.MapPosition,FacingDirection);
            }
        }


        public UnitType TypeOfUnit;
        public string Name;
        public MapTemplate MoveTemplate;

        // Things that define the attack.   
        public MapTemplate TargetTemplate;
        public MapTemplate AttackTemplate;
        public UnitDamageEffect AttackEffect;
        public bool CanTargetEmptyHex;

        public int MaxHP;
        public int CurrentHP;
        public Armor Armor;
        public bool IsCommander;

        public int MaxRechargeTime;
        public int CurrentRechargeTime;

        public bool AliveAndReady()
        {
            return (CurrentHP > 0) && (CurrentRechargeTime <= 0);
        }

        // Set the cooldown and healing associated with a recharge.
        public void PerformRecharge()
        {
            // The clicked hex is the units own hex: command it to recharge instead of firing.
            this.CurrentRechargeTime = (this.CurrentRechargeTime + 1) / 2;
            // Recharging also heals a damaged unit slightly (20% of max).
            this.CurrentHP = Math.Min(this.MaxHP, this.CurrentHP + (this.MaxHP / 5));        
        }

        //Sounds
        [NonSerialized]
        List<SoundEffect> SfxSelected = new List<SoundEffect>();
        [NonSerialized]
        List<SoundEffect> SfxMove = new List<SoundEffect>();
        [NonSerialized]
        List<SoundEffect> SfxFire = new List<SoundEffect>();
        [NonSerialized]
        List<SoundEffect> SfxKilled = new List<SoundEffect>();

        public TimeSpan PlaySfxSelected()
        { return PlayRandomSoundFromListIfVoiceEnabled(SfxSelected); }

        public TimeSpan PlaySfxMove()
        { return PlayRandomSoundFromListIfVoiceEnabled(SfxMove); }

        public TimeSpan PlaySfxFire()
        { return PlayRandomSoundFromListIfVoiceEnabled(SfxFire); }

        public TimeSpan PlaySfxKilled()
        { return PlayRandomSoundFromListIfVoiceEnabled(SfxKilled); } 


        public static TimeSpan PlayRandomSoundFromListIfVoiceEnabled(List<SoundEffect> list)
        {
            if (list.Count <= 0)
                return TimeSpan.Zero;

            if (!ConfigManager.GlobalManager.UnitVoicesEnabled)
            {
                SfxStore.Play(SfxId.thump);
                return SfxStore.Get(SfxId.thump).Duration;
            }

            SoundEffect soundToPlay = list[new Random().Next(list.Count)];
            SfxStore.Play(soundToPlay);
            return soundToPlay.Duration;
        } 

        protected Direction m_facingDirection;
        public Direction FacingDirection
        { 
            get { return m_facingDirection; }
            set 
            {
                m_facingDirection = value;
                RotationAngle = DirectionRotationAngles[(int)value];
            }
        }

        public static readonly float[] DirectionRotationAngles = new float[(int)Direction.ValueType.NumDirections] { 
            (float)Math.PI / 2f,
            (float)Math.PI * 5f / 6f,
            (float)Math.PI *-5f / 6f,
            (float)-Math.PI / 2f,
            (float)-Math.PI / 6f,   
            (float)Math.PI / 6f
        };



        private Unit(): base(null, Rectangle.Empty, Color.White, 0f,0f,Vector2.Zero,0f) { } // needed in order to be serializable

        /// When Created, the unit is not visible or clickable until placed on a Map.
        /// That way it can be part of an army before the game starts.  
        protected Unit(
            UnitType type,
            String name, 
            Texture2D texture,
            MapTemplate moveTemplate, 
            MapTemplate targetTemplate, 
            MapTemplate attackTemplate,
            UnitDamageEffect attackEffect,
            bool canTargetEmptyHex,
            int HP,
            Armor armor, 
            int maxRecharge,
            SoundEffect sfxSelected,
            SoundEffect sfxFire) 
            : base(texture, Rectangle.Empty, Color.White, 1f, 0f, Vector2.Zero, 0f)
        {
            TypeOfUnit = type;
            Name = name;
            MoveTemplate = moveTemplate;

            // Allow movement through allied units.
            if (MoveTemplate as RangeTemplate != null)
            { (MoveTemplate as RangeTemplate).UnitWhoseAlliesDontBlockMovement = this; }


            TargetTemplate = targetTemplate;
            AttackTemplate = attackTemplate;
            AttackEffect = attackEffect;
            AttackEffect.OwningUnit = this;
            CanTargetEmptyHex = canTargetEmptyHex;


            MaxHP = HP;
            CurrentHP = HP;
            Armor = armor;
            MaxRechargeTime = maxRecharge;
            CurrentRechargeTime = 0;

            IsCommander = false;

            DrawOrigin = new Vector2(texture.Height / 2, texture.Width / 2);
            Visible = false;
            Enabled = false;

            SfxSelected.Add(sfxSelected);
            SfxFire.Add(sfxFire);
        }

        // Used in deep-copy of game state.
        // Copies all non-gui realted attribudes from source 
        public void CopyFrom(Unit source)
        {
            this.IsCommander = source.IsCommander;
            this.TypeOfUnit = source.TypeOfUnit;
            this.Armor = source.Armor;
            this.AttackEffect = source.AttackEffect;
            this.AttackTemplate = source.AttackTemplate;
            this.CanTargetEmptyHex = source.CanTargetEmptyHex;
            this.MaxHP = source.MaxHP;
            this.MaxRechargeTime = source.MaxRechargeTime;
            this.MoveTemplate = source.MoveTemplate;
            this.Name = source.Name;
            this.TargetTemplate = source.TargetTemplate;


            this.CurrentHP = source.CurrentHP;
            this.CurrentRechargeTime = source.CurrentRechargeTime;
            this.FacingDirection = source.FacingDirection;
            
            // Owner and current hex shouldn't be the same - they have to be handled separately during deep-copy.
            this.m_owner = source.m_owner;
            //this.m_currentHex //actually skip currenthex, since we don't want it to be the same.
        }


        /// <summary>
        /// PlaceOnMap places the unit on a certain hex on a certain map and 
        /// sets up the display properties of the unit reletive to that hex.
        /// </summary>
        public bool PlaceOnMap(Hex hex)
        {
            return PlaceOnMap(hex, this.FacingDirection);
        }
        public bool PlaceOnMap(Map map, Point mapCoordinates, Direction facingDirection)
        {
            Hex destination = map.GetHex(mapCoordinates);
            return PlaceOnMap(destination, facingDirection);
        }
        public bool PlaceOnMap(Hex destination, Direction facingDirection)
        {
            if (destination == null || !destination.IsValidDestination)
                return false;

            if (Texture != null)
                DrawOrigin = new Vector2(Texture.Height / 2, Texture.Width / 2);

            // Gotta clear before placing.
            if(m_currentHex != null)
                m_currentHex.ClearUnit();

            destination.PlaceUnit(this);
            m_currentHex = destination;

            if (Texture != null)
            {
                Visible = true;
                Enabled = true;
                LocationRect = destination.DisplayRect;
            }

            FacingDirection = facingDirection;
            
            return true;
        }


        Rectangle getPortionOfDisplayArea(float X, float Y, float Width, float Height)
        {
            return new Rectangle(
                        (int)(Position.X - (DrawOrigin.X * Scale.X) + (Texture.Width * Scale.X * X)),
                        (int)(Position.Y - (DrawOrigin.Y * Scale.Y) + (Texture.Height * Scale.Y * Y)),
                        (int)(Texture.Width * Scale.X * Width),
                        (int)(Texture.Height * Scale.Y * Height));
        }


        const int RECHARGE_NUM_PIXELS = 25;

        ///override the draw function in order to also show HP and recharge data on the unit itself.
        public override void Draw(SpriteBatch spriteBatch, GameTime time)
        {
            base.Draw(spriteBatch, time);

            // Draw the HP Bar
            Rectangle MaxBar = getPortionOfDisplayArea(.2f, .83f, .6f, .07f);
            Rectangle FrameBar = MaxBar; FrameBar.Inflate(1, 1);
            Rectangle CurrentBar = MaxBar;
            CurrentBar.Width = (CurrentBar.Width * CurrentHP) / MaxHP;
            spriteBatch.Draw(TextureStore.Get(TexId.hp_bar_white), FrameBar, Color.Black);
            spriteBatch.Draw(TextureStore.Get(TexId.hp_bar_white), MaxBar, Color.Red);
            spriteBatch.Draw(TextureStore.Get(TexId.hp_bar_white), CurrentBar, Color.Green);

            // Draw the Commander Star
            if (IsCommander)
            {
                Rectangle StarRect = LocationRect;
                spriteBatch.Draw(TextureStore.Get(TexId.commander_star),
                    new Rectangle(StarRect.Center.X - StarRect.Width / 6,
                        StarRect.Top + (StarRect.Height * 2) / 3,
                        StarRect.Width / 3, StarRect.Height / 3),
                    Color.White);
            }

            // Draw the recharge number (if any)
            if (CurrentRechargeTime > 0)
            {
                Rectangle rechargeDisplayRect = getPortionOfDisplayArea(.55f, .05f, .5f, .5f);
                Rectangle sourceRect = new Rectangle(
                    RECHARGE_NUM_PIXELS * (CurrentRechargeTime-1),
                    0,
                    RECHARGE_NUM_PIXELS,
                    TextureStore.Get(TexId.recharge_numbers).Height);

                spriteBatch.Draw(TextureStore.Get(TexId.recharge_numbers),
                    rechargeDisplayRect, sourceRect, new Color(0,255,0));
            }
        }
    }
}
