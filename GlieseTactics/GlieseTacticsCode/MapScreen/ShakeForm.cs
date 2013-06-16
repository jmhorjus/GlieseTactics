using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gliese581g
{
    public class ShakeForm
    {
        public static MainApplication s_MainApp; 
        public static System.Windows.Forms.Form s_MainForm; //MainForm
        public static System.Drawing.Point s_CurrentFormLocation; // MainForm Current Location

        /// <summary>
        /// Make the Form shaking
        /// </summary>
        /// 
        public static void Shake()
        {
            Random r = new Random();

            /// Get the MainForm Location
            ShakeForm.s_MainForm = (System.Windows.Forms.Form)System.Windows.Forms.Control.FromHandle(s_MainApp.Window.Handle);
            ShakeForm.s_CurrentFormLocation = ShakeForm.s_MainForm.Location;

            for (int i = 0; i < 30; i++)
            {
                System.Drawing.Point NewFormLocation = new System.Drawing.Point(
                    s_CurrentFormLocation.X + r.Next(-10, 10),
                    s_CurrentFormLocation.Y + r.Next(-10, 10));

                s_MainForm.Location = NewFormLocation;
                System.Threading.Thread.Sleep(20);
                s_MainForm.Location = s_CurrentFormLocation;
            }
        }

    }
}
