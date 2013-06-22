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
        
        private Commander m_playerProfile = null;
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
                    m_portrait = Commander.EncodeJpgBytes(b);
                    b.Dispose();
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
            

            m_playerProfile = CreateCommanderProfile();

            //serializes the profile 
            bool profileConflictFound = false;

            //Check if the unit color or name already exists
            if (Directory.Exists(m_playerProfileDirectory))
            {
                foreach (string file in Directory.EnumerateFiles(m_playerProfileDirectory, "*.xml"))
                {
                    //Deserialize the xml file
                    Commander playerProfile = Commander.LoadXmlFile(m_playerProfileDirectory + Path.GetFileNameWithoutExtension(file) + ".xml", graphics);

                    if (m_playerProfile.UnitColor == playerProfile.UnitColor || playerProfile.Name.ToLower() == m_playerProfile.Name.ToLower())
                    {
                        profileConflictFound = true;
                        break;
                    }
                }
            }
            
            if (!profileConflictFound)
            {
                // Make sure both directories exist
                if (!Directory.Exists(m_playerProfileDirectory))
                    Directory.CreateDirectory(m_playerProfileDirectory);

                string fullXmlFilePath = m_playerProfileDirectory + m_playerProfile.Name + ".xml";

                // Save the file
                m_playerProfile.SaveXmlFile(fullXmlFilePath);
                
                callbackGameSetupScreen.EnableKeysAndMouse();
                callbackPlayerSocket.Player = Commander.LoadXmlFile(fullXmlFilePath, graphics);
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
        protected Commander CreateCommanderProfile()
        {
            Commander playerProfile = new Commander(txtBoxName.Text,
                m_portrait,
                m_unitColor.ToArgb(),
                m_playerProfileDirectory);
            return playerProfile;

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
