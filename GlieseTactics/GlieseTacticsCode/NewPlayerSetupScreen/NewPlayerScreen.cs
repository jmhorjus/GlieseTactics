using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;

namespace Gliese581g
{
    public partial class NewPlayerScreen : Form
    {
        public GameSetupScreen callbackGameSetupScreen;
        public PlayerDisplaySocket callbackPlayerSocket;
        public Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics;
        
        private PlayerProfile m_playerProfile = null;
        private byte[] m_portrait = null;
        private Color m_unitColor;
        private string m_portraitFileName = null;
//        private string m_portraitFilePath = null;
        private string m_fileSource;
        private string m_fileDestination;

        string m_playerProfileDirectory;

        private static NewPlayerScreen newPlayerScreeninstance = null;
        public NewPlayerScreen()
        {
            // Use the directory from the config manager.
            m_playerProfileDirectory = ConfigManager.GlobalManager.PlayerProfileDirectory;

            InitializeComponent();
        }
        //uses singleton dp
        public static NewPlayerScreen GetInstance
        {
            get
            {
                if (newPlayerScreeninstance == null)
                {
                    newPlayerScreeninstance = new NewPlayerScreen();
                }
                return newPlayerScreeninstance;
            }
        }
        #region Event Handlers
        private void OnLoad(object sender, EventArgs e)
        {
            ClearForm();
        }
        
        private void OnCancel(object sender, EventArgs e)
        {
            callbackGameSetupScreen.EnableKeysAndMouse();
            this.Close();
        }

        private void OnPickUnitColor(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            // Set the initial color
            dlg.AnyColor = true;
            dlg.AllowFullOpen = false;
            dlg.FullOpen = false;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            m_unitColor = dlg.Color;
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = @"Jpeg Files(*.jpg)|*.jpg;*.jpeg|Bitmap Files(*.bmp)|*.bmp|Png Files(*.png)|*.png|Gif Files(*.gif)|*.gif|All Files(*.*)|*.*";
                DialogResult ret = STAShowDialog(dlg);
                if (ret != DialogResult.OK)
                    return;
                txtBoxImagePath.Text = dlg.FileName;
                m_portraitFileName = Path.GetFileName(dlg.FileName);
                m_fileSource = dlg.FileName;
                m_fileDestination = Path.Combine(m_playerProfileDirectory, m_portraitFileName);
                string fileName;
                fileName = dlg.FileName;

                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    System.Drawing.Bitmap b = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(fs);
                    m_portrait = BmpToBytes(b);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.Message, ex.Message);
            }
        }
        [STAThread]
        private void OnProfileSubmit(object sender, EventArgs e)
        {
            if (!ValidatePlayerProfile())
                return;
            if (m_playerProfile == null)
                m_playerProfile = new PlayerProfile();
            MemoryStream ms = new MemoryStream(m_portrait);
            System.Drawing.Image theImage = System.Drawing.Image.FromStream(ms, true, false);
            //Resizes uploaded image
            ImageResize ir = new ImageResize(theImage); // is a Bitmap or Image
            ImageResize resize = new ImageResize(theImage);
            ir.Height = 100.0;
            ir.Width = 130.0;
            System.Drawing.Image theJpeg = ir.GetThumbnail();   // this is a Bitmap at the correct dimensions

            MemoryStream outstream = new MemoryStream();
            theJpeg.Save(outstream, System.Drawing.Imaging.ImageFormat.Jpeg);   // save in jpeg format in outstream
            theImage = System.Drawing.Image.FromStream(outstream, true, false);
            m_portrait = BmpToBytes(theImage);

            PopulatePlayerProfile(m_playerProfile);

            //serializes the profile 
            bool profileConflictFound = false;
            Microsoft.Xna.Framework.Color xnaColor = new Microsoft.Xna.Framework.Color(
                m_playerProfile.UnitColor.R, 
                m_playerProfile.UnitColor.G, 
                m_playerProfile.UnitColor.B);
            //Check if the unit color or name already exists
            if (Directory.Exists(m_playerProfileDirectory))
            {
                foreach (string file in Directory.EnumerateFiles(m_playerProfileDirectory, "*.xml"))
                {
                    //Deserialize the xml file
                    PlayerProfile playerProfile = PlayerProfile.DeserializeXml(m_playerProfileDirectory + Path.GetFileNameWithoutExtension(file) + ".xml");
                    using (FileStream fileStream = new FileStream(m_playerProfileDirectory + playerProfile.Name + ".jpg", FileMode.Open))
                    {
                        Microsoft.Xna.Framework.Color xnaSavedColor = new Microsoft.Xna.Framework.Color(
                            playerProfile.UnitColor.R,
                            playerProfile.UnitColor.G,
                            playerProfile.UnitColor.B);
                        if (xnaColor == xnaSavedColor || playerProfile.Name.ToLower() == m_playerProfile.Name.ToLower())
                        {
                            profileConflictFound = true;
                            break;
                        }
                    }
                }
            }
            
            if (!profileConflictFound)
            {
                // Make sure both directories exist
                if (!Directory.Exists(m_playerProfileDirectory))
                    Directory.CreateDirectory(m_playerProfileDirectory);

                string fullXmlFilePath = m_playerProfileDirectory + m_playerProfile.Name + ".xml";
                string fullJpgFilePath = m_playerProfileDirectory + m_playerProfile.Name + ".jpg";

                // Save the files: xml first.
                m_playerProfile.SerializeXml(GetStream(fullXmlFilePath));
                // then jpg.
                Bitmap bMap = m_playerProfile.BuildPortraitImage(m_playerProfile.Portrait);
                bMap.Save(fullJpgFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                
                callbackGameSetupScreen.EnableKeysAndMouse();
                callbackPlayerSocket.Player = new Player(fullXmlFilePath, graphics);
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("A player with this name or color already exists!");
                return;
            }
            Close();
        }

        #endregion

        #region Helpers
        protected void PopulatePlayerProfile(PlayerProfile playerProfile)
        {
            playerProfile.Name = txtBoxName.Text;
            playerProfile.Portrait = m_portrait;
            playerProfile.UnitColorAsArgb = m_unitColor.ToArgb();
            playerProfile.PortraitPath = m_playerProfileDirectory;
            playerProfile.PortraitFileName = m_portraitFileName;
        }

        private void ThreadMethod()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = @"Jpeg Files(*.jpg)|*.jpg;*.jpeg|Bitmap Files(*.bmp)|*.bmp|Png Files(*.png)|*.png|Gif Files(*.gif)|*.gif";

            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            txtBoxImagePath.Text = dlg.FileName;
        }
        private DialogResult STAShowDialog(FileDialog dialog)
        {
            DialogState state = new DialogState();
            state.dialog = dialog;
            System.Threading.Thread t = new System.Threading.Thread(state.ThreadProcShowDialog);
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            t.Join();
            return state.result;
        }
        public bool ValidatePlayerProfile()
        {
            if (txtBoxName.Text.Length == 0)
            {
                MessageBox.Show("Name cannot be blank");
                return false;
            }
            if (txtBoxImagePath.Text.Length == 0)
            {
                MessageBox.Show("Portrait Cannot be blank");
                return false;
            }
            if (!Regex.IsMatch(txtBoxName.Text, @"^[a-zA-Z0-9]+$"))
            {
                MessageBox.Show("Please enter a valid name.");
                return false;
            }
            return true;
        }
        public void ClearForm()
        {
            txtBoxImagePath.Text = string.Empty;
            txtBoxName.Text = string.Empty;
        }
        private FileStream GetStream(string fileName)
        {
            string parentDir = Directory.GetParent(fileName).FullName;
            if (!Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);
            FileStream fs = File.Create(fileName);
            return fs;
        }
        private byte[] BmpToBytes(System.Drawing.Image bmp)
        {
            MemoryStream ms = null;
            byte[] bmpBytes = null;
            try
            {
                ms = new MemoryStream();
                // Save to memory using the Jpeg format
                // what format?

                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                // read to end
                bmpBytes = ms.GetBuffer();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                bmp.Dispose();
                if (ms != null)
                {
                    ms.Close();
                }
            }
            return bmpBytes;
        }
        //Hides the minimize, maximize and close menu
        //Online resource obtained from http://stackoverflow.com/questions/7301825/windows-forms-how-to-hide-close-x-button
        private const int WS_SYSMENU = 0x80000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~WS_SYSMENU;
                return cp;
            }
        }

        public bool ConfirmationDialog(string dialogString)
        {
            // confirm to the user there deletion.
            DialogResult res = MessageBox.Show(dialogString,
                Application.ProductName, MessageBoxButtons.YesNo);
            if (res != DialogResult.Yes)
                return false;
            return true;
        }
        #endregion
    }
    public class DialogState
    {
        public DialogResult result;
        public FileDialog dialog;
        public void ThreadProcShowDialog()
        {
            result = dialog.ShowDialog();
        }
    }
}
