using System;
using System.Windows.Forms;
using TK.NodalEditor;
using TK.NodalEditor.NodesLayout;
using System.IO;
using System.Threading;
using DevExpress.XtraRichEdit.API.Native;
using System.Drawing;
using DevExpress.XtraRichEdit.Services;
using RichEditSyntax;

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

            InitializeCodeEditors();

            NodesSerializer.GetInstance().AddSerializer(NodeElement.Node, "CustomNode", typeof(CustomNode));
            ManagerCompanion comp = new ManagerCompanion();
            manager = new NodesManager(comp);

            NodalDirector.Get(manager, tK_NodalEditorUCtrl1.Layout);

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

        private void InitializeCodeEditors()
        {
            //CS
            csEditControl.ReplaceService<ISyntaxHighlightService>(new CSharpSyntaxHighlightService(csEditControl));

            csEditControl.Views.SimpleView.Padding = new Padding(60, 4, 4, 0);
            csEditControl.Views.SimpleView.AllowDisplayLineNumbers = true;

            csEditControl.Document.Sections[0].LineNumbering.Start = 1;
            csEditControl.Document.Sections[0].LineNumbering.CountBy = 1;
            csEditControl.Document.Sections[0].LineNumbering.Distance = 75f;
            csEditControl.Document.Sections[0].LineNumbering.RestartType = LineNumberingRestart.Continuous;

            csEditControl.Document.CharacterStyles["Line Number"].FontName = "Courier";
            csEditControl.Document.CharacterStyles["Line Number"].FontSize = 10;
            csEditControl.Document.CharacterStyles["Line Number"].ForeColor = Color.DarkGray;

            //Python
            pyEditControl.ReplaceService<ISyntaxHighlightService>(new PythonSyntaxHighlightService(pyEditControl.Document));

            pyEditControl.Views.SimpleView.Padding = new Padding(60, 4, 4, 0);
            pyEditControl.Views.SimpleView.AllowDisplayLineNumbers = true;

            pyEditControl.Document.Sections[0].LineNumbering.Start = 1;
            pyEditControl.Document.Sections[0].LineNumbering.CountBy = 1;
            pyEditControl.Document.Sections[0].LineNumbering.Distance = 75f;
            pyEditControl.Document.Sections[0].LineNumbering.RestartType = LineNumberingRestart.Continuous;

            pyEditControl.Document.CharacterStyles["Line Number"].FontName = "Courier";
            pyEditControl.Document.CharacterStyles["Line Number"].FontSize = 10;
            pyEditControl.Document.CharacterStyles["Line Number"].ForeColor = Color.DarkGray;
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
            try
            {
                NodalDirector.Get().verbose = false;

                if (tabControl1.SelectedIndex == 0)
                {
                    NodalDirector.Evaluate(csEditControl.Text.Replace("\f", "\n"));
                }
                else
                {
                    NodalDirector.EvaluatePython(pyEditControl.Text.Replace("\f", "\n"));
                }
            }
            catch
            {
                NodalDirector.Get().verbose = true;
                throw;
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(NodalDirector.CanUndo())
            {
                NodalDirector.Undo();
            }
            else
            {
                NodalDirector.Error("Nothing to undo !");
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NodalDirector.CanRedo())
            {
                NodalDirector.Redo();
            }
            else
            {
                NodalDirector.Error("Nothing to redo !");
            }
        }

        private void sillyMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NodalDirector.AddNode("testNode", "Coco", 10, 10);
        }
    }
}
