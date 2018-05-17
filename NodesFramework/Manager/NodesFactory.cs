using System;
using System.Collections.Generic;
using System.Text;

namespace TK.NodalEditor
{
    /// <summary>
    /// Static library to create node related objects
    /// </summary>
    public static class NodesFactory
    {
        #region METHODS

        /// <summary>
        /// Add a port object
        /// </summary>
        /// <param name="inNode">Node on which to add the port</param>
        /// <param name="inObj">Port object</param>
        /// <returns>The port(s) created by adding the port Object</returns>
        public static List<Port> AddPortObj(Node inNode, PortObj inObj)
        {
            return AddPortObj(inNode, inObj, inNode.Inputs.Count);
        }

        /// <summary>
        /// Inserts a port object
        /// </summary>
        /// <param name="inNode">Node on which to add the port</param>
        /// <param name="inObj">Port object</param>
        /// <param name="index">Index of where to insert the object</param>
        /// <returns></returns>
        internal static List<Port> AddPortObj(Node inNode, PortObj inObj, int index)
        {
            List<Port> ports = new List<Port>();

            inObj.Owner = inNode;
            inNode.Elements.Insert(index, inObj);

            if (inObj.IsInput)
            {
                Port newInput = new Port(inNode, inObj, inNode.Inputs.Count, false);
                inNode.Inputs.Insert(index, newInput);

                if (!inObj.ExposeInput)
                {
                    newInput.Visible = false;
                }

                ports.Add(newInput);
            }

            if (inObj.IsOutput)
            {
                Port newOutput = new Port(inNode, inObj, inNode.Outputs.Count, true);
                inNode.Outputs.Insert(index, newOutput);

                if (!inObj.ExposeOutput)
                {
                    newOutput.Visible = false;
                }

                ports.Add(newOutput);
            }

            return ports;
        }

        #endregion

    }
}
