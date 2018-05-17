using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TK.NodalEditor.Log
{
    /// <summary>
    /// Class used to manage a list of Log instances that need to be printed on screen through a "Graphics" instance
    /// </summary>
    class LogSystem
    {
        #region MEMBERS
        /// <summary>
        /// The list of logs to print
        /// </summary>
        public List<Log> Logs = new List<Log>();

        #region METHODS


        #endregion

        /// <summary>
        /// Draw the logs on screen
        /// </summary>
        /// <param name="inGraphics">The "Graphics" instance used to draw</param>
        /// <param name="Location">The position the logs should be displayed</param>
        /// <param name="Area">The maximum area the text should occupy</param>
        public void DrawLogs(Graphics inGraphics, PointF Location, Rectangle Area)
        {
            PointF CroppedLoc = new PointF(Math.Max(Math.Min(Location.X, Area.Width - 100), Area.X + 100), Math.Max(0, Math.Min(Location.Y, Area.Height)));

            DateTime now = DateTime.Now;
            List<Log> expiredLogs = new List<Log>();
            int counter = 0;

            foreach (Log log in Logs)
            {
                double duration = (double)(now.Ticks - log.StartTime.Ticks) / 1000000.0;
                if (duration > log.duration)
                {
                    expiredLogs.Add(log);
                }
                else
                {
                    log.Draw(inGraphics, new PointF(CroppedLoc.X, CroppedLoc.Y + 15 * counter));
                }
                counter++;
            }

            foreach (Log log in expiredLogs)
            {
                Logs.Remove(log);
            }
        }

        /// <summary>
        /// Add a log to be printed on screen
        /// </summary>
        /// <param name="inMessage">Message to log</param>
        /// <param name="inDuration">Duration the message stays on screen</param>
        /// <param name="inSeverity">Type of the message (0 : Log (gray), 1 : Warning (orange), 2 : Error (red))</param>
        internal void AddLog(string inMessage, double inDuration, int inSeverity)
        {
            Logs.Add(new Log(inMessage, inDuration, inSeverity));
        }

        #endregion
    }
}
