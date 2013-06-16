using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Gliese581g        
{
    public class Player
    {
        private bool m_surrender;

        public bool Surrender
        {
            get { return m_surrender; }
            set { m_surrender = value; }
        }

        public Texture2D Portrait;
        public string Name;
        string m_profileFileName;
        string m_portraitFileName;
        protected Color m_unitColor;
        public Color UnitColor
        {
            get { return m_unitColor; }
            set 
            {
                m_unitColor = value;
                foreach (Unit unit in MyUnits)
                    unit.Tint = value; 
            }
        }

        public void DeleteProfile()
        {
            try
            {
                File.Delete(m_portraitFileName);
                File.Delete(m_profileFileName);
            }
            catch (Exception ex)
            {
                ;
            }
        }

        public Player(string xmlFilePath, GraphicsDevice graphicsDevice)
        {
            PlayerProfile pProfile = PlayerProfile.DeserializeXml(xmlFilePath);

            string jpgFilePath = Directory.GetParent(xmlFilePath) + "\\" + pProfile.Name + ".jpg";
            Texture2D fileTexture;
            using (FileStream fileStream = new FileStream(jpgFilePath, FileMode.Open))
            {   // Load the profile image into a Texture2D object.
                fileTexture = Texture2D.FromStream(graphicsDevice, fileStream);
            }
            // Covert the profile color to a usable XNA format.
            Microsoft.Xna.Framework.Color xnaColor = new Microsoft.Xna.Framework.Color(
                pProfile.UnitColor.R,
                pProfile.UnitColor.G,
                pProfile.UnitColor.B);

            Portrait = fileTexture;
            Name = pProfile.Name;
            UnitColor = xnaColor;
            m_profileFileName = xmlFilePath;
            m_portraitFileName = jpgFilePath;
        }

        public List<Unit> MyUnits = new List<Unit>();
        //Other stats can be added later.

        public void AddUnit(Unit newUnit)
        {
            MyUnits.Add(newUnit);
            newUnit.Owner = this;
        }

        public bool HasLiveCommander
        {
            get
            {
                bool hasCommander = false;
                foreach (Unit unit in MyUnits)
                {
                    if (unit.CurrentHP > 0 && unit.IsCommander == true)
                        hasCommander = true;
                }
                return hasCommander;
            }
        }

        public bool HasLiveUnit
        {
            get
            {
                bool hasLiveUnit = false;
                foreach (Unit unit in MyUnits)
                {
                    if (unit.CurrentHP > 0)
                        hasLiveUnit = true;
                }
                return hasLiveUnit;
            }
        }
    }
}
