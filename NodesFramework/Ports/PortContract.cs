using System;
using System.Collections.Generic;
using System.Text;

namespace TK.NodalEditor
{
    /// <summary>
    /// NOT USED YET : Defines a contract, can't remember exactly what was the idea here, but should be something like interfaces in programming so ports that Nodes can share and that can be switched or reconnected automatically
    /// </summary>
    public class PortContract
    {
        #region MEMBERS

        /// <summary>
        /// A list of matchs in ports names
        /// </summary>
        List<List<string>> contracts = new List<List<string>>();

        #endregion

        #region PROPERTIES
        /// <summary>
        /// A list of matchs in ports names
        /// </summary>
        public List<List<string>> Contracts
        {
            get { return contracts; }
            set { contracts = value; }
        }

        #endregion

    }
}
