using System;
using System.Collections.Generic;
using System.Text;

namespace TK.NodalEditor.Tags
{
    /// <summary>
    /// Base class to determine if a node is valid inside a given context.
    /// This class can be inherited to add custom tests, overriding <c>isContextConsistent(Node node)</c>".
    /// <see cref="RigItemTag">For instance, a context tester for a Node in Rig Mode</see>
    /// <seealso cref="PortContextTag">Same class type for ports</seealso>
    /// </summary>
    public class NodeContextTag
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="inIsNodeConsistent">Is valid for a Node</param>
        /// <param name="inIsCompoundConsistent">Is valid for a Compound</param>
        public NodeContextTag(bool inIsNodeConsistent, bool inIsCompoundConsistent)
        {
            isNodeConsistent = inIsNodeConsistent;
            isCompoundConsistent = inIsCompoundConsistent;
        }

        #endregion

        #region MEMBERS

        /// <summary>
        /// List of nodes for this context
        /// </summary>
        public List<Node> nodes;

        /// <summary>
        /// Is valid for a Node
        /// </summary>
        public bool isNodeConsistent = true;

        /// <summary>
        /// Is valid for a Compound
        /// </summary>
        public bool isCompoundConsistent = true;

        #endregion

        #region METHODS

        /// <summary>
        /// Method which calls <c>isContextConsistent(Node node)</c> on each node in the context
        /// </summary>
        /// <returns>True if all nodes are consistent, False otherwise</returns>
        /// <example>Utilisé comme ceci : <code>if(tag.isContextConsistent()){//DoProcess()}</code> </example>
        public bool isContextConsistent()
        {
            foreach (Node node in nodes)
            {
                if (!isContextConsistent(node))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Method that is called on each Node of the collection
        /// </summary>
        /// <param name="node">Considered Node</param>
        /// <returns>True if the node is consistent, False otherwise</returns>
        public virtual bool isContextConsistent(Node node)
        {
            return true;
        }

        #endregion

    }
}
