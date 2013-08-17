using System; 
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gliese581g
{
    public enum SkillId
    {
        knight_commander_armor,
        knight_commander_gun,
        knight_ace,

        engineer_power_boost,
        engineer_repair,
        engineer_recharge_heal,

        invoker_power_drain,
        invoker_overdrive,
        invoker_destruction,

        NONE
    }


    [Serializable]
    public class Skill
    {
        /// <summary>
        /// Properties of a specific skill 
        /// </summary>
        public SkillId Id;
        public string Name;
        public Texture2D Icon_dim;
        public Texture2D Icon_lit;
        public List<SkillId> Dependancies;


        ///Constructors
        public Skill() { } // For serialization
        public Skill(SkillId id, string name, 
            Texture2D icon_dim, 
            Texture2D icon_lit = null, 
            SkillId dependancy1 = SkillId.NONE, 
            SkillId dependancy2 = SkillId.NONE)
        {   
            Id = id;
            Name = name;
            Icon_dim = icon_dim;
            Icon_lit = (icon_lit != null) ? icon_lit : icon_dim;

            if (dependancy1 != SkillId.NONE) Dependancies.Add(dependancy1);
            if (dependancy2 != SkillId.NONE) Dependancies.Add(dependancy2);
        }



        /// <summary>
        /// Static Values/Functions Related to the skill matrix defining what the game's skill actaully are. 
        /// </summary>
        protected static Dictionary<SkillId, Skill> m_skillMatrix;
        public static Dictionary<SkillId, Skill> SkillMatrix { get { return m_skillMatrix; } }

        public static void InitSkillMatrix()
        {
            m_skillMatrix = new Dictionary<SkillId,Skill>();

            // Knight
            m_skillMatrix.Add(SkillId.knight_commander_armor,
                new Skill(SkillId.knight_commander_armor, "Defensive Positioning", TextureStore.Get(TexId.skill_power_boost)));
            m_skillMatrix.Add(SkillId.knight_commander_gun,
                new Skill(SkillId.knight_commander_gun, "Expert Marksman", TextureStore.Get(TexId.skill_power_boost)));
            m_skillMatrix.Add(SkillId.knight_ace,
                new Skill(SkillId.knight_ace, "Ace Pilot", TextureStore.Get(TexId.skill_ace_pilot_blue), TextureStore.Get(TexId.skill_ace_pilot_blue),
                    //Dependancies
                    SkillId.knight_commander_gun,
                    SkillId.knight_commander_armor));


            // Engineer
            m_skillMatrix.Add(SkillId.engineer_power_boost,
                new Skill(SkillId.engineer_power_boost, "Power Boost", TextureStore.Get(TexId.skill_power_boost)));
            m_skillMatrix.Add(SkillId.engineer_repair,
                new Skill(SkillId.engineer_repair, "Emergency Repair", TextureStore.Get(TexId.skill_repair_unit)));
            m_skillMatrix.Add(SkillId.engineer_recharge_heal,
                new Skill(SkillId.engineer_recharge_heal, "Boosted Recovery (passive)", TextureStore.Get(TexId.skill_healing), TextureStore.Get(TexId.skill_healing),
                   //Dependancies
                   SkillId.engineer_power_boost,
                   SkillId.engineer_repair));


            // Invoker
            m_skillMatrix.Add(SkillId.invoker_power_drain,
                new Skill(SkillId.invoker_power_drain, "Power Boost", TextureStore.Get(TexId.skill_power_boost)));
            m_skillMatrix.Add(SkillId.invoker_overdrive,
                new Skill(SkillId.invoker_overdrive, "Emergency Repair", TextureStore.Get(TexId.skill_repair_unit)));
            m_skillMatrix.Add(SkillId.invoker_destruction,
                new Skill(SkillId.invoker_destruction, "Boosted Recovery (passive)", TextureStore.Get(TexId.skill_healing), TextureStore.Get(TexId.skill_healing),
                //Dependancies
                   SkillId.invoker_power_drain,
                   SkillId.invoker_overdrive));

        }
    }

    [Serializable]
    public class SkillSet
    {
        Dictionary<SkillId, int> m_skillLevels;

        /// <summary>
        /// Return true if all skills in this skillset meet their prerequitates.  
        /// </summary>
        public bool IsValid()
        {
            bool retVal = true;
            foreach(SkillId sk in m_skillLevels.Keys)
            {
                foreach (SkillId dependancy in Skill.SkillMatrix[sk].Dependancies)
                {
                    if (!m_skillLevels.ContainsKey(dependancy))
                        retVal = false;
                }
            }
            return retVal;
        }
    }


}
