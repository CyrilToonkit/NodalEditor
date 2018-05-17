using System;
using System.Collections.Generic;
using System.Text;

namespace TK.NodalEditor
{
    /// <summary>
    /// Base class to determine if a port is valid inside a given context.
    /// This class can be inherited to add custom tests, overriding "isContextConsistent(Port port)".
    /// See "TK_OSCAR_Manager\ContextMenu" for examples
    /// </summary>
    public class PortContextTag
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Default constructor
        /// </summary>
        public PortContextTag()
        {

        }

        #endregion

        #region MEMBERS

        /// <summary>
        /// Port on which to test the consistency
        /// </summary>
        public Port port;

        #endregion

        #region METHODS

        /// <summary>
        /// Method that is called on the Port
        /// </summary>
        /// <param name="port">Port on which to test the consistency</param>
        /// <returns>True if the port is consistent, False otherwise</returns>
        public virtual bool isContextConsistent(Port port)
        {
            return true;
        }

        #endregion

    }
}
