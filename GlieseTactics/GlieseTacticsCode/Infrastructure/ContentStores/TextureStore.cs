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
    public enum TexId
    {
        cursor_default,
        cursor_target,
        cursor_target_sniper,
        cursor_move,
        cursor_recharge,

        // Intro Screen
        team_logo,
        team_logo_swish,

        //Main Menu
        mainmenu_background,
        button_exit_dim,
        button_exit_lit,
        button_loadgame_dim,
        button_loadgame_lit,
        button_newgame_dim,
        button_newgame_lit,
        button_options_dim,
        button_options_lit,

        // Options Menu
        button_apply_dim,
        button_apply_lit,
        button_cancel_dim,
        button_cancel_lit,
        slider_knob_dim,
        slider_knob_lit,
        slider_bar,
        chkbox_false,
        chkbox_true,

        //Game Setup Screen
        default_submenu_frame,
        gamesetup_cancel_dim,
        gamesetup_cancel_lit,
        gamesetup_start_dim,
        gamesetup_start_lit,
        player_stats_frame,
        player_stats_frame_skill_panel,

        button_frame_lit,
        button_frame_dim,
        button_army_small,
        button_army_medium,
        button_army_large,
        button_map_small,
        button_map_medium,
        button_map_large,
        button_map_random,
        button_map_symmetrical,
        button_victory_elimination,
        button_victory_assassination,
        
        button_vs_human,
        button_vs_easy_computer,
        button_vs_hard_computer,

        button_arrow_up,
        button_arrow_down,
        button_arrow_up_lit,
        button_arrow_down_lit,


        //Escape Menu
        //buttons w/g in title represent new green themed buttons
        button_mm_lit,            
        button_mm_dim,            
        button_g_mm_lit,
        button_g_mm_dim,
        button_g_cancel_lit,
        button_g_cancel_dim,
        button_g_option_lit,
        button_g_option_dim,
        button_g_load_lit,
        button_g_load_dim,
        button_g_surrender_lit,
        button_g_surrender_dim,
        button_g_concede_lit, 
        button_g_concede_dim,



        // Map Screen.
        map_end_placement_dim,
        map_end_placement_lit,
        map_end_turn_dim,
        map_end_turn_lit,
        unit_stats_frame,

        background_mountain_river,
        background_river_snow,
        background_rocky_river,
        background_swamp,
        background_waterfall,

        hex_rock,
        hex_sand,
        hex_snow,
        hex_swamp,
        hex_trees,
        hex_trees_2,
        hex_water,
        hex_water_blue,
        hex_grass,
        hex_explosion,


        esc_menu_background_left,
        esc_menu_background_middle,
        esc_menu_background_right,

                
        portrait_empty,
        portrait_newplayer,
        portrait_1,
        portrait_2,
        portrait_3,
        portrait_4,
        portrait_5,
        portrait_6,
        player_trash_dim,
        player_trash_lit,

        textbox_name,
        rdbutton_true,
        rdbutton_false,


        hp_bar_white,
        recharge_numbers,
        commander_star,

        //Units
        unit_infantry,
        unit_tank,
        unit_commander,
        unit_artillery,
        unit_scout,
        unit_roughrider,
        unit_mech,
        

        //Skills
        skill_ace_pilot_red,
        skill_ace_pilot_blue,
        skill_commander_armor_red,
        skill_commander_armor_blue,
        skill_commander_gun_red,
        skill_commander_gun_blue,
        skill_napalm ,
        skill_self_destruct,
        skill_power_boost,
        skill_power_boost_purple,
        skill_power_drain,
        skill_power_drain_purple,
        skill_healing,
        skill_healing_purple,
        skill_repair_unit,
        skill_empty_socket,




        graphic_victory,
        confetti_1,
        confetti_2,
        confetti_3,
        confetti_4,
        confetti_5,
        confetti_6
        
    }



    // * Provides a single place for information about texture files and their paths, so they can be changed in only one place.
    // * Allows textures to be referred to by enum, rather than string.  
    public class TextureStore
    {
        /// <summary>
        /// Static stuff first.
        /// </summary>
        private static TextureStore s_publicStore = null;

        public static void InitPublicStore(ContentManager content)
        {
            s_publicStore = new TextureStore(content);
        }

        public static void PreloadAll()
        {   
            for (TexId tex = TexId.cursor_default; tex < TexId.confetti_6; tex++)
                Store.Preload(tex);
        }

        public static TextureStore Store
        {
            get
            {
                if (s_publicStore == null)
                    throw new Exception("PublicStore has not been initialized!");
                return s_publicStore;
            }
        }

        public static Texture2D Get(TexId Key)
        {
            return Store.Load(Key);
        }

        //public static Texture2D FilePathGet(string filePathKey)
        //{
        //    return Store.CustomLoad(filePathKey);
        //}
        
        /// <summary>
        /// Non-static functions/members.
        /// </summary>

        private ContentManager m_content;
        private Dictionary<TexId, Texture2D> m_store = new Dictionary<TexId, Texture2D>();

        private Dictionary<String, Texture2D> m_customStore = new Dictionary<String, Texture2D>();

        private TextureStore(ContentManager content)
        {
            m_content = content;
        }




        private string pathStrFromTexId(TexId id)
        {
            switch (id)
            {
                case TexId.cursor_default:
                    return "Cursors/default";
                case TexId.cursor_target:
                    return "Cursors/target";
                case TexId.cursor_target_sniper:
                    return "Cursors/target_sniper";
                case TexId.cursor_move:
                    return "Cursors/move";
                case TexId.cursor_recharge:
                    return "Cursors/Recharge_Cursor";


                case TexId.team_logo:
                    return "IntroScreen/g2s_logo_plain";
                case TexId.team_logo_swish:
                    return "IntroScreen/g2s_logo_swish";


                case TexId.mainmenu_background:
                    return "MainMenuScreen/background_2";
                case TexId.button_exit_dim:
                    return "Buttons/exit_dim";
                case TexId.button_exit_lit:
                    return "Buttons/exit_lit";
                case TexId.button_loadgame_dim:
                    return "Buttons/loadgame_dim";
                case TexId.button_loadgame_lit:
                    return "Buttons/loadgame_lit";
                case TexId.button_newgame_dim:
                    return "Buttons/newgame_dim";
                case TexId.button_newgame_lit:
                    return "Buttons/newgame_lit";
                case TexId.button_options_dim:
                    return "Buttons/options_dim";
                case TexId.button_options_lit:
                    return "Buttons/options_lit";

                case TexId.button_apply_dim:
                    return "Buttons/apply_dim";
                case TexId.button_apply_lit:
                    return "Buttons/apply_lit";
                case TexId.button_cancel_dim:
                    return "Buttons/cancel_dim";
                case TexId.button_cancel_lit:
                    return "Buttons/cancel_lit";

                case TexId.slider_knob_dim:
                    return "OptionsScreen/slider_knob_dim";
                case TexId.slider_knob_lit:
                    return "OptionsScreen/slider_knob_lit";
                case TexId.slider_bar:
                    return "OptionsScreen/slider_bar";

                case TexId.chkbox_false:
                    return "OptionsScreen/chkbox_false";
                case TexId.chkbox_true:
                    return "OptionsScreen/chkbox_true";


                case TexId.default_submenu_frame:
                    return "default_submenu_frame";
                case TexId.gamesetup_cancel_dim:
                    return "GameSetupScreen/cancel_dim";
                case TexId.gamesetup_cancel_lit:
                    return "GameSetupScreen/cancel_lit";
                case TexId.gamesetup_start_dim:
                    return "GameSetupScreen/start_dim";
                case TexId.gamesetup_start_lit:
                    return "GameSetupScreen/start_lit";
                case TexId.player_stats_frame:
                    return "GameSetupScreen/player_stats_frame";
                case TexId.player_stats_frame_skill_panel:
                    return "GameSetupScreen/player_stats_frame_skills";

                case TexId.button_frame_dim:
                    return "GameSetupScreen/button_frame_dim";
                case TexId.button_frame_lit:
                    return "GameSetupScreen/button_frame_lit";
                case TexId.button_army_small:
                    return "GameSetupScreen/button_army_small";
                case TexId.button_army_medium:
                    return "GameSetupScreen/button_army_medium";
                case TexId.button_army_large:
                    return "GameSetupScreen/button_army_large";

                case TexId.button_map_small:
                    return "GameSetupScreen/button_map_small";
                case TexId.button_map_medium:
                    return "GameSetupScreen/button_map_medium";
                case TexId.button_map_large:
                    return "GameSetupScreen/button_map_large";

                case TexId.button_map_random:
                    return "GameSetupScreen/button_map_random";
                case TexId.button_map_symmetrical:
                    return "GameSetupScreen/button_map_symmetrical";

                case TexId.button_victory_assassination:
                    return "GameSetupScreen/button_victory_assassination";
                case TexId.button_victory_elimination:
                    return "GameSetupScreen/button_victory_elimination";

                case TexId.button_vs_human:
                    return "GameSetupScreen/button_vs_human";
                case TexId.button_vs_easy_computer:
                    return "GameSetupScreen/button_vs_easy_computer";
                case TexId.button_vs_hard_computer:
                    return "GameSetupScreen/button_vs_hard_computer";

                case TexId.button_arrow_down:
                    return "GameSetupScreen/button_arrow_down";
                case TexId.button_arrow_up:
                    return "GameSetupScreen/button_arrow_up";
                case TexId.button_arrow_down_lit:
                    return "GameSetupScreen/button_arrow_down_lit";
                case TexId.button_arrow_up_lit:
                    return "GameSetupScreen/button_arrow_up_lit";

                case TexId.button_mm_dim:
                    return "Buttons/mm_dim";
                case TexId.button_mm_lit:
                    return "Buttons/mm_lit";           
                case TexId.button_g_mm_dim:
                    return "Buttons/g_mm_dim";
                case TexId.button_g_mm_lit:
                    return "Buttons/g_mm_lit";
                case TexId.button_g_cancel_dim:
                    return "Buttons/g_return_dim";
                case TexId.button_g_cancel_lit:
                    return "Buttons/g_return_lit";
                case TexId.button_g_option_dim:
                    return "Buttons/g_option_dim";
                case TexId.button_g_option_lit:
                    return "Buttons/g_option_lit";
                case TexId.button_g_load_dim:
                    return "Buttons/g_load_dim";
                case TexId.button_g_load_lit:
                    return "Buttons/g_load_lit";
                case TexId.button_g_surrender_dim:
                    return "Buttons/g_surrender_dim";
                case TexId.button_g_surrender_lit:
                    return "Buttons/g_surrender_lit";
                case TexId.button_g_concede_dim:
                    return "Buttons/g_concede_dim";
                case TexId.button_g_concede_lit:
                    return "Buttons/g_concede_lit";


                case TexId.map_end_placement_dim:
                    return "MapScreen/endplacement_dim";
                case TexId.map_end_placement_lit:
                    return "MapScreen/endplacement_lit";
                case TexId.map_end_turn_dim:
                    return "MapScreen/endturn_dim";
                case TexId.map_end_turn_lit:
                    return "MapScreen/endturn_lit";
                case TexId.unit_stats_frame:
                    return "MapScreen/unit_stats_frame";


                case TexId.background_mountain_river:
                    return "Terrain/background_mountain_river";
                case TexId.background_river_snow:
                    return "Terrain/background_river_snow";
                case TexId.background_rocky_river:
                    return "Terrain/background_rocky_river";
                case TexId.background_swamp:
                    return "Terrain/background_swamp";
                case TexId.background_waterfall:
                    return "Terrain/background_waterfall";

                case TexId.hex_rock:
                    return "Terrain/hex_rock";
                case TexId.hex_sand:
                    return "Terrain/hex_sand";
                case TexId.hex_snow:
                    return "Terrain/hex_snow";
                case TexId.hex_swamp:
                    return "Terrain/hex_swamp";
                case TexId.hex_trees:
                    return "Terrain/hex_trees";
                case TexId.hex_trees_2:
                    return "Terrain/hex_trees_2";
                case TexId.hex_water:
                    return "Terrain/hex_water_grey";
                case TexId.hex_water_blue:
                    return "Terrain/hex_water_blue";
                case TexId.hex_grass:
                    return "Terrain/hex_grass";
                case TexId.hex_explosion:
                    return "Terrain/hex_explosion";


                case TexId.esc_menu_background_left:
                    return "EscMenuScreen/menu_brace_left";
                case TexId.esc_menu_background_middle:
                    return "EscMenuScreen/menu_brace_middle";
                case TexId.esc_menu_background_right:
                    return "EscMenuScreen/menu_brace_right";

                case TexId.portrait_1:
                    return "Portraits/face1";
                case TexId.portrait_2:
                    return "Portraits/face2";
                case TexId.portrait_3:
                    return "Portraits/face3";
                case TexId.portrait_4:
                    return "Portraits/face4";
                case TexId.portrait_5:
                    return "Portraits/face5";
                case TexId.portrait_6:
                    return "Portraits/face6";
                case TexId.portrait_empty:
                    return "Portraits/empty_portrait";
                case TexId.portrait_newplayer:
                    return "Portraits/empty_newplayer";
                case TexId.player_trash_dim:
                    return "GameSetupScreen/player_trash_dim";
                case TexId.player_trash_lit:
                    return "GameSetupScreen/player_trash_lit";

                case TexId.textbox_name:
                    return "Buttons/apply_dim"; //TO DO: Apply the right texture
                case TexId.rdbutton_false:
                    return "OptionsScreen/chkbox_false"; //TO DO: Apply the right texture
                case TexId.rdbutton_true:
                    return "OptionsScreen/chkbox_false"; //TO DO: Apply the right texture

                case TexId.hp_bar_white:
                    return "Units/hp_bar_white";
                case TexId.recharge_numbers:
                    return "Units/recharge_numbers";
                case TexId.commander_star:
                    return "Units/commander_star";


                //Units:
                case TexId.unit_infantry:
                    return "Units/Infantry_big";
                case TexId.unit_tank:
                    return "Units/Tank_big";
                case TexId.unit_commander:
                    return "Units/Commander_big";
                case TexId.unit_artillery:
                    return "Units/Artillery_big";
                case TexId.unit_scout:
                    return "Units/Scout_big";
                case TexId.unit_roughrider:
                    return "Units/RoughRider_big";
                case TexId.unit_mech:
                    return "Units/Mech";

               
                //Skills:
                case TexId.skill_ace_pilot_red:
                    return "SkillsScreen/Ace Pilot";
                case TexId.skill_ace_pilot_blue:
                    return "SkillsScreen/Ace Pilot(blue)";
                case TexId.skill_commander_armor_red:
                    return "SkillsScreen/Commander Armor";
                case TexId.skill_commander_armor_blue:
                    return "SkillsScreen/Commander Armor(blue)";
                case TexId.skill_commander_gun_red:
                    return "SkillsScreen/Commander Gun";
                case TexId.skill_commander_gun_blue:
                    return "SkillsScreen/Commander Gun(blue)";
                case TexId.skill_napalm :
                    return "SkillsScreen/Damage Unit, Napalm";
                case TexId.skill_self_destruct:
                    return "SkillsScreen/Damage Unit, Self Destruct";
                case TexId.skill_power_boost:
                    return "SkillsScreen/PowerBoost";
                case TexId.skill_power_boost_purple:
                    return "SkillsScreen/PowerBoost(Purple)";
                case TexId.skill_power_drain:
                    return "SkillsScreen/PowerDrain";
                case TexId.skill_power_drain_purple:
                    return "SkillsScreen/PowerDrane(Purple)";
                case TexId.skill_healing:
                    return "SkillsScreen/RechargeHeal";
                case TexId.skill_healing_purple:
                    return "SkillsScreen/RechargeHeal(Purple)";
                case TexId.skill_repair_unit:
                    return "SkillsScreen/Single Target Heal";

                case TexId.skill_empty_socket:
                    return "SkillsScreen/empty_skill_socket";


                case TexId.graphic_victory:
                    return "VictoryGraphics/VICTORY";
                case TexId.confetti_1:
                    return "VictoryGraphics/confetti_1";
                case TexId.confetti_2:
                    return "VictoryGraphics/confetti_2";
                case TexId.confetti_3:
                    return "VictoryGraphics/confetti_3";
                case TexId.confetti_4:
                    return "VictoryGraphics/confetti_4";
                case TexId.confetti_5:
                    return "VictoryGraphics/confetti_5";
                case TexId.confetti_6:
                    return "VictoryGraphics/confetti_6";


                default:
                    throw new Exception("No entry for this TexId value. This function may need to be updated!");
            }
        }


        public void Preload(TexId Key)
        {
            Load(Key);
        }

        public Texture2D Load(TexId Key)
        {
            Texture2D retVal;
            try
            {
                retVal = m_store[Key];
            }
            catch
            {
                m_store[Key] = m_content.Load<Texture2D>( pathStrFromTexId(Key) );

                retVal = m_store[Key];
            }

            return retVal;
        }

        //public Texture2D CustomLoad(string filePathKey)
        //{
        //    Texture2D retVal;
        //    try
        //    {
        //        retVal = m_customStore[filePathKey];
        //    }
        //    catch
        //    {
        //        m_customStore[filePathKey] = m_content.Load<Texture2D>(filePathKey);
        //        retVal = m_customStore[filePathKey];
        //    }

        //    return retVal;
        //}


    }
}
