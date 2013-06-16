using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;

namespace Gliese581g
{
    [Serializable]
    public class PlayerProfile
    {
        public string Name { get; set; }
        public byte[] Portrait { get; set; }
        public string PortraitPath { get; set; }
        public string PortraitFileName { get; set; }
        [XmlIgnore]
        public Color UnitColor { get; set; }

        [XmlElement("UnitColor")]
        public int UnitColorAsArgb
        {
            get { return UnitColor.ToArgb(); }
            set { UnitColor = Color.FromArgb(value); }
        }
        public void SerializeXml(FileStream fs)
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(this.GetType());
                xs.Serialize(fs, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.Message, ex.Message);
            }
            finally
            {
                fs.Close();
            }
        }
        public static PlayerProfile DeserializeXml(string filename)
        {
            FileStream fs = File.Open(filename, FileMode.Open);
            PlayerProfile pProfile = null;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(PlayerProfile));
                pProfile = (PlayerProfile)xs.Deserialize(fs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.Message, ex.Message);
            }
            finally
            {
                fs.Close();
            }
            return pProfile;
        }

        public const int PORTRAIT_WIDTH = 100;  // pixels
        public const int PORTRAIT_HEIGHT = 134; // pixels
        public Bitmap BuildPortraitImage(byte[] imageData)
        {
            Bitmap theJpeg = null;
            MemoryStream ms = null;
            System.Drawing.Image fullsizeImage = null;
            try
            {
                // create an image from the byte array
                ms = new MemoryStream(imageData);
                fullsizeImage = System.Drawing.Image.FromStream(ms, true, false);
                theJpeg = new Bitmap(new Bitmap(fullsizeImage, PORTRAIT_WIDTH, PORTRAIT_HEIGHT));
            }
            catch (Exception ex)
            {
                throw ex; //Something
            }
            return theJpeg;
        }
    }
}   
