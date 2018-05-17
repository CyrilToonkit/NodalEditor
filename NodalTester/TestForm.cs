using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TK.NodalEditor;
using TK.BaseLib;
using TK.BaseLib.CustomData;

namespace NodalTester
{
    public partial class TestForm : Form
    {
        NodesManager manager;

        public TestForm()
        {
            InitializeComponent();
            
            NodesSerializer.GetInstance().AddSerializer(NodeElement.Node, "Default", typeof(CustomNode));
            manager = new NodesManager();
            manager.AvailableCompound = new Compound();

            CustomNode node = new CustomNode();
            node.DynamicInputs = true;
            node.DynamicOutputs = true;
            node.Name = node.NativeName = "testNode";

            PortObj obj = new PortObj();
            obj.Name = "TestObj";
            obj.NativeName = "TestObj";

            PortObj obj1 = new PortObj();
            obj1.Name = "TestObj1";
            obj1.NativeName = "TestObj1";

            PortObj obj2 = new PortObj();
            obj2.Name = "TestObj2";
            obj2.NativeName = "TestObj2";

            NodesFactory.AddPortObj(node, obj);
            NodesFactory.AddPortObj(node, obj1);
            NodesFactory.AddPortObj(node, obj2);

            node.CustomText = "Zobi la mouche";

            manager.AvailableNodes.Add(node);
            manager.NewLayout();
            manager.AddNode(0, manager.Root, 50, 50);
            tK_NodalEditorUCtrl1.Init("C:\\Rigs\\NodesLayout", manager);

            stringNode categs = new stringNode("Available Nodes");

            categs.AddNode("Chain", "Rigs that can be attached to same instances to create chains");
            categs.AddNode("Control", "Rigs that create mainly Controls");
            categs.AddNode("Behaviour", "Rigs used to add pecific behaviours between ports (switchs, constraints...)");
            categs.AddNode("Deformation", "Rigs that create mainly Defomers");
            categs.AddNode("Parameters", "Rigs that manages parameters");

            stringNode BodyParts = categs.AddNode("BodyParts", "Compounds used to generate characters");
            BodyParts.AddNode("Leg", "Compounds used as legs or limbs");
            BodyParts.AddNode("Spine", "Compounds used as spines");

            categs.AddNode("Mechanical", "Rigs that stands for mechanical elements");
            categs.AddNode("Debug", "Nodes with special behaviour used to Debug the graph");

            stringNodesTreeView1.Set(categs, false);
        }
    }
}
