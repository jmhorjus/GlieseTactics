using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Gliese581g
{
    /// <summary>
    /// Singleton class that manages global configuration settings.  
    /// Should also be in charge of reading/writing those settings from the xml config file on disc. 
    /// </summary>
    class ConfigManager
    {
        /// <summary>
        /// Static stuff first.
        /// </summary>
        private static ConfigManager s_configManager = null;

        public static void InitPublicConfigManager()
        {
            s_configManager = new ConfigManager();
        }

        public static ConfigManager GlobalManager
        {
            get
            {
                if (s_configManager == null)
                    throw new Exception("The Config Manager has not been initialized!");
                return s_configManager;
            }
        }



        /// <summary>
        /// Non-static functions/members.
        /// </summary>

        /// Sound effect volume.  
        float m_sfxVolume = .5f;
        public float SfxVolume
        {
            get { return m_sfxVolume;  }
            set { m_sfxVolume = value; }
        }

        /// Music volume.  
        float m_musicVolume = .5f;
        public float MusicVolume
        {
            get { return m_musicVolume; }
            set { m_musicVolume = value; }
        }


        /// Unit voices enable/disable.  
        bool m_unitVoicesEnabled = true;
        public bool UnitVoicesEnabled
        {
            get { return m_unitVoicesEnabled; }
            set { m_unitVoicesEnabled = value; }
        }

        /// Mouse scrolling enable/disable.  
        bool m_mouseScrollEnabled = true;
        public bool MouseScrollEnabled
        {
            get { return m_mouseScrollEnabled; }
            set { m_mouseScrollEnabled = value; }
        }

        /// Scroll Speed - from 0 to 1 (min to max)
        float m_mapScrollSpeed = 0.5f;
        public float MapScrollSpeed
        {
            get { return m_mapScrollSpeed; }
            set { m_mapScrollSpeed = value; }
        }


        string m_playerProfileDirectory;
        public string PlayerProfileDirectory
        {
            get { return m_playerProfileDirectory; }
            set { m_playerProfileDirectory = value; }
        }


        private ConfigManager()
        {
            // Load initial settings from file!!
            //m_playerProfileDirectory = Directory.GetCurrentDirectory() + "PlayerProfiles\\";
            m_playerProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Gliese581g\\PlayerProfiles\\";
        }

    }
}
