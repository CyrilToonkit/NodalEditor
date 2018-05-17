using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

/// <summary>
/// Simple system to print information on a control through a "Graphics" instance.
/// </summary>
namespace TK.NodalEditor.Log
{
    /// <summary>
    /// Describe an information that have to be printed on screen through a "Graphics" instance. It's included in a list in the LogSystem Class. 
    /// </summary>
    class Log
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        /// <param name="inDuration">Duration the message stays on screen</param>
        /// <param name="inSeverity">Type of the message (0 : Log (gray), 1 : Warning (orange), 2 : Error (red))</param>
        public Log(string inMessage, double inDuration, int inSeverity)
        {
            Message = inMessage;
            duration = inDuration;
            Severity = inSeverity;
        }

        #endregion
        
        #region MEMBERS

        /// <summary>
        /// Font used to print
        /// </summary>
        Font _logFont = new Font("Verdana", 9f, FontStyle.Bold);
        /// <summary>
        /// Brush used to print the background
        /// </summary>
        Brush _brush = new SolidBrush(Color.FromArgb(220, 255, 255, 255));

        /// <summary>
        /// Time when the print began
        /// </summary>
        public DateTime StartTime = DateTime.Now;

        /// <summary>
        /// The message to print
        /// </summary>
        public string Message;

        /// <summary>
        /// The duration the message must last
        /// </summary>
        public double duration;

        /// <summary>
        /// The type of the message (Verbose, Warning, Error)
        /// </summary>
        public int Severity;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Brush used to print the text
        /// </summary>
        public Brush curBrush
        {
            get
            {
                switch (Severity)
                {
                    case 1:
                        return Brushes.Orange;

                    case 2:
                        return Brushes.Red;

                    default:
                        return Brushes.Gray;
                }
            }
        }

        #endregion   
     
        #region METHODS

        /// <summary>
        /// Draw the log on screen through a "Graphics" instance
        /// </summary>
        /// <param name="inGraphics">The "Graphics" instance used to draw</param>
        /// <param name="Location">The location (center) where the text should be printed</param>
        public void Draw(Graphics inGraphics, PointF Location)
        {
            SizeF f = inGraphics.MeasureString(Message, _logFont);
            PointF textLoc = new PointF(Location.X - (f.Width / 2), Location.Y - 25);

            inGraphics.FillRectangle(_brush, (int)textLoc.X, (int)textLoc.Y, (int)f.Width, (int)f.Height);
            inGraphics.DrawRectangle(Pens.Black, (int)textLoc.X, (int)textLoc.Y, (int)f.Width, (int)f.Height);
            inGraphics.DrawString(Message, _logFont, curBrush, textLoc);
        }

        #endregion
        
    }
}
