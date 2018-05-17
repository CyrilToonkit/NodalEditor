using System;
using System.Collections.Generic;
using System.Text;

namespace TK.NodalEditor
{
    /// <summary>
    /// Stores port modifications that occur when updating nodes
    /// </summary>
    public class PortModifications
    {
        #region MEMBERS
        /// <summary>
        /// The old ports, they don't exist in the "New" Node
        /// </summary>
        List<Port> mOldPorts = new List<Port>();

        /// <summary>
        /// The new ports, they didn't exist in the "Old" Node
        /// </summary>
        List<Port> mNewPorts = new List<Port>();

        #endregion
        
        #region PROPERTIES

        /// <summary>
        /// The old ports, they don't exist in the "New" Node
        /// </summary>
        public List<Port> OldPorts
        {
            get { return mOldPorts; }
            set { mOldPorts = value; }
        }

        /// <summary>
        /// The new ports, they didn't exist in the "Old" Node
        /// </summary>
        public List<Port> NewPorts
        {
            get { return mNewPorts; }
            set { mNewPorts = value; }
        }

        #endregion
        
        #region METHODS
        /// <summary>
        /// Tells if there was any modifications in Ports
        /// </summary>
        /// <returns>true if there is at least one modification, false otherwise</returns>
        public bool IsEmpty()
        {
            return (mNewPorts.Count + mOldPorts.Count) == 0;
        }

        #endregion
        
    }
}
