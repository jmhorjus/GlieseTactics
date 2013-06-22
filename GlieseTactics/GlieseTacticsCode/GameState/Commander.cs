using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;



namespace Gliese581g        
{

    [Serializable]
    public class Commander
    {
        // Static functions / utilities. 
        public void SaveXmlFile(string xmlFilePath)
        {
            FileStream fs = GetStream(xmlFilePath);
            XmlSerializer xs = new XmlSerializer(this.GetType());
            xs.Serialize(fs, this);
            fs.Close();
        }

        public static Commander LoadXmlFile(string xmlFilePath, GraphicsDevice graphicsDevice)
        {
            FileStream fs = File.Open(xmlFilePath, FileMode.Open);
            Commander commander = null;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(Commander));
                commander = (Commander)xs.Deserialize(fs);

                using (MemoryStream rawimage = new MemoryStream(commander.PortraitRawData))
                {   // Load the profile image into a Texture2D object.
                    commander.Portrait = Texture2D.FromStream(graphicsDevice, rawimage);
                }
                commander.m_profileFileName = xmlFilePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                fs.Close();
            }
            return commander;
        }

        //Used only the first time the image resolution is transformed and the image is re-saved.
        public const int PORTRAIT_WIDTH = 100;  // pixels
        public const int PORTRAIT_HEIGHT = 134; // pixels
        public static byte[] ResizeImageInMemory(byte[] buffer)
        {
            System.Drawing.Image originalImage = System.Drawing.Image.FromStream(new MemoryStream(buffer), true, false);
            //Resizes uploaded image
            ImageResize ir = new ImageResize(originalImage); // is a Bitmap or Image
            ir.Height = Commander.PORTRAIT_HEIGHT;
            ir.Width = Commander.PORTRAIT_WIDTH;
            System.Drawing.Image resizedImage = ir.GetThumbnail();   // this is a Bitmap at the correct dimensions

            byte[] retVal = EncodeJpgBytes(resizedImage);
            
            originalImage.Dispose();
            resizedImage.Dispose();
            return retVal;
        }

        public static byte[] EncodeJpgBytes(System.Drawing.Image bmp)
        {
            MemoryStream tempStream = new MemoryStream();
            // Save to memory using the Jpeg format
            bmp.Save(tempStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            // read to end
            return tempStream.GetBuffer();
        }

        public static FileStream GetStream(string fileName)
        {
            string parentDir = Directory.GetParent(fileName).FullName;
            if (!Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);
            FileStream fs = File.Create(fileName);
            return fs;
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
                throw ex;
            }
        }
        
        
        
        //


        // Class member variables

        private bool m_surrender;
        public bool Surrender
        {
            get { return m_surrender; }
            set { m_surrender = value; }
        }


        public byte[] PortraitRawData;
        [XmlIgnore]
        public Texture2D Portrait;

        public string Name;

        string m_profileFileName;
        string m_portraitFileName;
        
        [NonSerialized]
        protected Color m_unitColor;
        [XmlElement("UnitColorArgb")]
        public int UnitColorAsArgb
        {
            get {
                System.Drawing.Color tempcolor = System.Drawing.Color.FromArgb(
                    UnitColor.A, UnitColor.R, UnitColor.G, UnitColor.B);
                return tempcolor.ToArgb(); 
            }
            set 
            {
                System.Drawing.Color tempcolor = System.Drawing.Color.FromArgb(value);                 
                UnitColor = new Microsoft.Xna.Framework.Color(    
                    tempcolor.R, tempcolor.G, tempcolor.B, tempcolor.A);
            }
        }
        [XmlIgnore]
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


        public List<Unit> MyUnits = new List<Unit>();
        //Other stats can be added later.




        public Commander()
        { }

        public Commander(string name, byte[] portraitRawData, int unitColorAsArgb, string portraitPath)
        {
            Name = name;
            PortraitRawData = portraitRawData;
            UnitColorAsArgb = unitColorAsArgb;
            m_portraitFileName = portraitPath + "\\" + Name + ".jpg";
            m_profileFileName = portraitPath + "\\" + Name + ".xml";            
        }


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
