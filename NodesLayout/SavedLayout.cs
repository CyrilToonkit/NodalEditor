using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using TK.BaseLib;

namespace TK.NodalEditor.NodesLayout
{
    public class SavedLayout
    {
        public SavedLayout()
        {

        }

        public SavedLayout(string curCompound, int layoutX, int layoutY, double layoutSize, Compound root)
        {
            exploredCompound = curCompound;
            layoutPosition[0] = layoutX;
            layoutPosition[1] = layoutY;

            size = layoutSize;

            if (root != null)
            {
                nodesPositions.Clear();

                List<Node> nodes = root.GetChildren(false);
                foreach (Node node in nodes)
                {
                    nodesPositions.Add(node.FullName, new float[] { node.UIx, node.UIy });
                }
            }

            DumpDictionary();
        }

        private void DumpDictionary()
        {
            nodesNames = new List<string>(nodesPositions.Keys);
            positions = new List<float[]>(nodesPositions.Values);
        }

        string exploredCompound = "";
        public string ExploredCompound
        {
            get { return exploredCompound; }
            set { exploredCompound = value; }
        }

        int[] layoutPosition = new int[] { 0, 0 };
        public int[] LayoutPosition
        {
            get { return layoutPosition; }
            set { layoutPosition = value; }
        }

        double size = 1.0;
        public double Size
        {
            get { return size; }
            set { size = value; }
        }

        Dictionary<string, float[]> nodesPositions = new Dictionary<string, float[]>();

        [XmlIgnore]
        public Dictionary<string, float[]> NodesPositions
        {
            get { return nodesPositions; }
            set { nodesPositions = value; DumpDictionary(); }
        }

        List<string> nodesNames = new List<string>();
        public List<string> NodesNames
        {
            get { return nodesNames; }
            set
            {
                nodesNames = value;
            }
        }

        List<float[]> positions = new List<float[]>();
        public List<float[]> Positions
        {
            get { return positions; }
            set
            {
                positions = value;
            }
        }

        internal void RebuildDictionary()
        {
            nodesPositions.Clear();
            int counter = 0;
            foreach (string key in nodesNames)
            {
                nodesPositions.Add(key, positions[counter]);
                counter++;
            }
        }
    }
}
