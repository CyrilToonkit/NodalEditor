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
using TK.NodalEditor.NodesLayout;
using System.IO;

namespace NodalTester
{
    public partial class TestForm : Form
    {
        NodesManager manager;

        public NodesManager Manager { get => manager; set => manager = value; }

        public NodesLayout NodalLayout { get => tK_NodalEditorUCtrl1.Layout; }
        
        public TestForm()
        {
            InitializeComponent();
            
            NodesSerializer.GetInstance().AddSerializer(NodeElement.Node, "CustomNode", typeof(CustomNode));
            ManagerCompanion comp = new ManagerCompanion();
            manager = new NodesManager(comp);

            NodalDirector.RegisterManager(manager);
            NodalDirector.RegisterLayout(tK_NodalEditorUCtrl1.Layout);

            manager.AvailableCompound = new Compound();

            CustomNode node = new CustomNode();
            node.DynamicInputs = true;
            node.DynamicOutputs = true;
            node.Name = node.NativeName = "testNode";

            PortObj obj = new PortObj();
            obj.Name = "testNode_TestObj";
            obj.NativeName = "testNode_TestObj";

            PortObj obj1 = new PortObj();
            obj1.Name = "testNode_TestObj1";
            obj1.NativeName = "testNode_TestObj1";

            PortObj obj2 = new PortObj();
            obj2.Name = "testNode_TestObj2";
            obj2.NativeName = "testNode_TestObj2";

            NodesFactory.AddPortObj(node, obj);
            NodesFactory.AddPortObj(node, obj1);
            NodesFactory.AddPortObj(node, obj2);

            node.CustomText = "Custom Text";

            CustomNode tetNode = new CustomNode();
            tetNode.Copy(node, true);
            tetNode.AllowAddPorts = true;
            tetNode.NativeName = tetNode.Name = "tetNode";

            CustomNode otherNode = new CustomNode();
            otherNode.Copy(node, true);
            otherNode.NativeName = otherNode.Name = "otherNode";

            manager.AvailableNodes.Add(node);
            manager.AvailableNodes.Add(tetNode);
            manager.AvailableNodes.Add(otherNode);

            manager.NewLayout();
            manager.AddNode(0, manager.Root, 50, 50);
            tK_NodalEditorUCtrl1.Init("C:\\Rigs\\NodesLayout", manager);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Manager.NewLayout();
            NodalLayout.Invalidate();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult rslt = openFileDialog1.ShowDialog();

            if(rslt == DialogResult.OK)
            {
                Compound openedComp = null;

                using (FileStream fileStream = new FileStream(openFileDialog1.FileName, FileMode.Open))
                {
                    openedComp = NodesSerializer.GetInstance().CompoundSerializers["Default"].Deserialize(fileStream) as Compound;
                }

                if (openedComp != null)
                {
                    Manager.NewLayout(openedComp, false);
                    NodalLayout.ChangeFocus(true);
                    NodalLayout.Frame(Manager.CurCompound.Nodes);
                    NodalLayout.Invalidate(); 
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult rslt = saveFileDialog1.ShowDialog();

            if (rslt == DialogResult.OK)
            {
                StreamWriter myWriter = null;

                try
                {
                    myWriter = new StreamWriter(saveFileDialog1.FileName);

                    NodesSerializer.GetInstance().CompoundSerializers["Default"].Serialize(myWriter, Manager.Root);
                }
                finally
                {
                    if(myWriter != null)
                        myWriter.Close();
                }
                /*
                using (StreamWriter myWriter = new StreamWriter(saveFileDialog1.FileName))
                {
                    
                }*/
            }
        }

        private void nodalExecuteBT_Click(object sender, EventArgs e)
        {
            NodalDirector.Evaluate(scriptEditorTB.Text);
        }
    }
}
