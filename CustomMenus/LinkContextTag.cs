using System;
using System.Collections.Generic;
using System.Text;

namespace TK.NodalEditor.Tags
{
    /// <summary>
    /// Base class to determine if a link is valid inside a given context.
    /// This class can be inherited to add custom tests, overriding <c>isContextConsistent(Link link)</c>".
    /// </summary>
    public class LinkContextTag
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Default constructor
        /// </summary>
        public LinkContextTag()
        {

        }

        #endregion

        #region MEMBERS

        /// <summary>
        /// List of nodes for this context
        /// </summary>
        public List<Link> links;

        #endregion

        #region METHODS

        /// <summary>
        /// Method which calls <c>isContextConsistent(Node node)</c> on each node in the context
        /// </summary>
        /// <returns>True if all nodes are consistent, False otherwise</returns>
        /// <example>Utilisé comme ceci : <code>if(tag.isContextConsistent()){//DoProcess()}</code> </example>
        public bool isContextConsistent()
        {
            foreach (Link link in links)
            {
                if (!isContextConsistent(link))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Method that is called on each Node of the collection
        /// </summary>
        /// <param name="link">Considered Link</param>
        /// <returns>True if the node is consistent, False otherwise</returns>
        public virtual bool isContextConsistent(Link link)
        {
            return true;
        }

        #endregion

    }
}
