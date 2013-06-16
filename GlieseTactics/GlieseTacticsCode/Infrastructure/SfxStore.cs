using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g
{
    public enum SfxId
    {
        menu_click,
        menu_mouseover,

        thump,
        explode,
        rocket_boom,

        unit_not_ready,
        unit_recharge,

        infantry_selected_1,
        infantry_selected_2,
        infantry_move_1,
        infantry_move_2,
        infantry_fire,
        infantry_killed,

        tank_selected_1,
        tank_selected_2,
        tank_move_1,
        tank_move_2,
        tank_fire,
        tank_killed,

        commander_selected_1,
        commander_selected_2,
        commander_move_1,
        commander_move_2,
        commander_killed,

        artillery_selected_1,
        artillery_selected_2,
        artillery_move_1,
        artillery_move_2,
        artillery_fire,
        artillery_killed,

        scout_selected_1,
        scout_selected_2,
        scout_move_1,
        scout_move_2,
        scout_killed,

        roughrider_killed,
        roughrider_selected_1,
        roughrider_selected_2,
        roughrider_move_1,
        roughrider_move_2,

        mech_killed,
        mech_fire,
        mech_selected_1,
        mech_selected_2,
        mech_move_1,
        mech_move_2

    }

    class SfxStore
    {
        /// <summary>
        /// Static stuff first.
        /// </summary>
        private static SfxStore s_publicStore = null;

        public static void InitPublicStore(ContentManager content)
        {
            s_publicStore = new SfxStore(content);
        }

        public static SfxStore Store
        {
            get
            {
                if (s_publicStore == null)
                    throw new Exception("PublicStore has not been initialized!");
                return s_publicStore;
            }
        }

        public static SoundEffect Get(SfxId Key)
        {
            return Store.Load(Key);
        }

        public static void Play(SfxId key)
        {
            Get(key).Play(ConfigManager.GlobalManager.SfxVolume, 0, 0);
        }
        public static void Play(SoundEffect sound)
        {
            sound.Play(ConfigManager.GlobalManager.SfxVolume, 0, 0);
        }


        /// <summary>
        /// Non-static functions/members.
        /// </summary>
        private ContentManager m_content;
        private Dictionary<SfxId, SoundEffect> m_store = new Dictionary<SfxId, SoundEffect>();

        private SfxStore(ContentManager content)
        {
            m_content = content;
        }

        private string pathStrFromTexId(SfxId id)
        {
            switch (id)
            {
                case SfxId.menu_click:
                    return "Sounds/menu_click";
                case SfxId.menu_mouseover:
                    return "Sounds/menu_mouseover";
                case SfxId.thump:
                    return "Sounds/thump";
                case SfxId.explode:
                    return "Sounds/explode";
                case SfxId.rocket_boom:
                    return "Sounds/rocket_boom";

                case SfxId.unit_not_ready:
                    return "Sounds/Units/unit_not_ready";
                case SfxId.unit_recharge:
                    return "Sounds/Units/unit_recharge";

                case SfxId.infantry_selected_1:
                    return "Sounds/Units/Infantry/infantry_selected_1";
                case SfxId.infantry_selected_2:
                    return "Sounds/Units/Infantry/infantry_selected_2";
                case SfxId.infantry_move_1:
                    return "Sounds/Units/Infantry/infantry_move_1";
                case SfxId.infantry_move_2:
                    return "Sounds/Units/Infantry/infantry_move_2";
                case SfxId.infantry_fire:
                    return "Sounds/Units/Infantry/infantry_fire";
                case SfxId.infantry_killed:
                    return "Sounds/Units/Infantry/infantry_killed";


                case SfxId.tank_selected_1:
                    return "Sounds/Units/Tank/tank_selected_1";
                case SfxId.tank_selected_2:
                    return "Sounds/Units/Tank/tank_selected_2";
                case SfxId.tank_move_1:
                    return "Sounds/Units/Tank/tank_move_1";
                case SfxId.tank_move_2:
                    return "Sounds/Units/Tank/tank_move_2";
                case SfxId.tank_fire:
                    return "Sounds/Units/Tank/tank_fire";
                case SfxId.tank_killed:
                    return "Sounds/Units/Tank/tank_killed";

                case SfxId.commander_selected_1:
                    return "Sounds/Units/Commander/commander_selected_1";
                case SfxId.commander_selected_2:
                    return "Sounds/Units/Commander/commander_selected_2";
                case SfxId.commander_move_1:
                    return "Sounds/Units/Commander/commander_move_1";
                case SfxId.commander_move_2:
                    return "Sounds/Units/Commander/commander_move_2";
                case SfxId.commander_killed:
                    return "Sounds/Units/Commander/commander_killed";

                case SfxId.artillery_selected_1:
                    return "Sounds/Units/Artillery/artillery_selected_1";
                case SfxId.artillery_selected_2:
                    return "Sounds/Units/Artillery/artillery_selected_2";
                case SfxId.artillery_move_1:
                    return "Sounds/Units/Artillery/artillery_move_1";
                case SfxId.artillery_move_2:
                    return "Sounds/Units/Artillery/artillery_move_2";
                case SfxId.artillery_fire:
                    return "Sounds/Units/Artillery/artillery_fire";
                case SfxId.artillery_killed:
                    return "Sounds/Units/Artillery/artillery_killed";


                case SfxId.scout_selected_1:
                    return "Sounds/Units/Scout/scout_selected_1";
                case SfxId.scout_selected_2:
                    return "Sounds/Units/Scout/scout_selected_2";
                case SfxId.scout_move_1:
                    return "Sounds/Units/Scout/scout_move_1";
                case SfxId.scout_move_2:
                    return "Sounds/Units/Scout/scout_move_2";
                case SfxId.scout_killed:
                    return "Sounds/Units/Scout/scout_killed";

                case SfxId.roughrider_selected_1:
                    return "Sounds/Units/RoughRider/roughrider_selected_1";
                case SfxId.roughrider_selected_2:
                    return "Sounds/Units/RoughRider/roughrider_selected_2";
                case SfxId.roughrider_move_1:
                    return "Sounds/Units/RoughRider/roughrider_move_1";
                case SfxId.roughrider_move_2:
                    return "Sounds/Units/RoughRider/roughrider_move_2";
                case SfxId.roughrider_killed:
                    return "Sounds/Units/RoughRider/roughrider_killed";

                case SfxId.mech_selected_1:
                    return "Sounds/Units/Mech/mech_selected_1";
                case SfxId.mech_selected_2:
                    return "Sounds/Units/Mech/mech_selected_2";
                case SfxId.mech_move_1:
                    return "Sounds/Units/Mech/mech_move_1";
                case SfxId.mech_move_2:
                    return "Sounds/Units/Mech/mech_move_2";
                case SfxId.mech_fire:
                    return "Sounds/Units/Mech/mech_fire";
                case SfxId.mech_killed:
                    return "Sounds/Units/Mech/mech_killed";

                default:
                    throw new Exception("No entry for this TexId value. This function may need to be updated!");
            }
        }


        public void Preload(SfxId Key)
        {
            Load(Key);
        }

        public SoundEffect Load(SfxId Key)
        {
            SoundEffect retVal;
            try
            {
                retVal = m_store[Key];
            }
            catch
            {
                m_store[Key] = m_content.Load<SoundEffect>(pathStrFromTexId(Key));

                retVal = m_store[Key];
            }
            return retVal;
        }


    }
}
