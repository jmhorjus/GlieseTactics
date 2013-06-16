using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Web;
using System.Drawing;
using System.IO;

namespace Gliese581g
{
    #region ImageResize 
    /// <summary>
    /// ImageResize is a class that is based on an article that was obtained from
    /// the URL http://www.devx.com/dotnet/Article/22079/0/page/3. I had to make
    /// some minor changes to a couple of the properties, but otherwise it is very
    /// much like the original article.
    /// </summary>
    public class ImageResize
    {
        #region Instance Fields
        //instance fields
        private double m_width, m_height;
        private bool m_use_aspect = true;
        private bool m_use_percentage = false;
        private System.Drawing.Image m_src_image = null;   // source image
        private Graphics m_graphics;
        #endregion

        #region Public properties


        /// <summary>
        /// gets of sets the image member
        /// byte[] imageData = m_logoSessionWrapper.Record.LogoImage.ToArray();
        /// MemoryStream ms = new MemoryStream(imageData);
        /// ImageResize ir = new ImageResize(System.Drawing.Image.FromStream(ms, true, false));
        /// </summary>
        public System.Drawing.Image Image
        {
            get { return m_src_image; }
            set { m_src_image = value; }
        }
        /// <summary>
        /// gets of sets the PreserveAspectRatio
        /// </summary>
        public bool PreserveAspectRatio
        {
            get { return m_use_aspect; }
            set { m_use_aspect = value; }
        }
        /// <summary>
        /// gets of sets the UsePercentages
        /// </summary>
        public bool UsePercentages
        {
            get { return m_use_percentage; }
            set { m_use_percentage = value; }
        }
        /// <summary>
        /// gets of sets the Width
        /// </summary>
        public double Width
        {
            get { return m_width; }
            set { m_width = value; }
        }
        /// <summary>
        /// gets of sets the Height
        /// </summary>
        public double Height
        {
            get { return m_height; }
            set { m_height = value; }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// ImageResize - constructor takes an image.
        /// </summary>
        /// <param name="sourceImage"></param>
        public ImageResize(System.Drawing.Image sourceImage)
        {
            m_src_image = sourceImage;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ImageResize()
        {

        }

        /// <summary>
        /// Returns a Image which represents a rezised Image
        /// </summary>
        /// <returns>A Image which represents a rezised Image, using the 
        /// proprerty settings provided</returns>
        public virtual System.Drawing.Image GetThumbnail()
        {
            System.Drawing.Image destImage = null;

            // Flag whether a new image is required
            bool recalculate = false;
            double new_width = Width;
            double new_height = Height;
            // Load via stream rather than Image.FromFile to release the file
            // handle immediately

            try
            {

                // If you opted to specify width and height as percentages of the 
                // original image's width and height, compute these now
                if (UsePercentages)
                {
                    if (Width != 0)
                    {
                        new_width = (double)m_src_image.Width * Width / 100;

                        if (PreserveAspectRatio)
                        {
                            new_height = new_width * m_src_image.Height /
                                (double)m_src_image.Width;
                        }
                    }
                    if (Height != 0)
                    {
                        new_height = (double)m_src_image.Height * Height / 100;

                        if (PreserveAspectRatio)
                        {
                            new_width = new_height * m_src_image.Width /
                                (double)m_src_image.Height;
                        }
                    }
                }
                else
                {
                    // If you specified an aspect ratio and absolute width or height,
                    // then calculate this now; if you accidentally specified both a 
                    // width and height, ignore the PreserveAspectRatio flag

                    if (PreserveAspectRatio)
                    {
                        if (Width != 0 && Height == 0)
                        {
                            new_height = (Width / (
                                double)m_src_image.Width) * m_src_image.Height;
                        }
                        else if (Height != 0 && Width == 0)
                        {
                            new_width = (Height / (
                                double)m_src_image.Height) * m_src_image.Width;
                        }
                    }
                }
                if (m_src_image.Height != new_height || m_src_image.Width != new_width)
                    recalculate = true;

                if (recalculate)
                {
                    // Calculate the new image
                    Bitmap bitmap = new Bitmap((int)new_width,
                        (int)new_height,
                        m_src_image.PixelFormat);
                    m_graphics = Graphics.FromImage(bitmap);
                    m_graphics.SmoothingMode =
                        System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    m_graphics.InterpolationMode =
                        System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    m_graphics.DrawImage(m_src_image, 0, 0, bitmap.Width, bitmap.Height);   // draw into the bitmap with new width and height

                    destImage = bitmap;
                }
                else
                {
                    destImage = m_src_image;
                }
            }
            catch (Exception ex)
            {
                throw ex; 
            }
            finally
            {
                if (m_graphics != null)
                    m_graphics.Dispose();
                m_graphics = null;
            }
            return destImage;
        }
        #endregion

        #region Deconstructor

        /// <summary>
        /// Frees all held resources, such as Graphics and Image handles
        /// </summary>
        ~ImageResize()
        {
            // Free resources

            if (m_graphics != null)
                m_graphics.Dispose();

            //if (m_src_image != null)
            //    m_src_image.Dispose();
        }
        #endregion
    }
    #endregion
}
