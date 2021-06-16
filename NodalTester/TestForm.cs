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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.Controls;
using System.ComponentModel.Design;

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

            tK_NodalEditorUCtrl1.Layout.SelectionChangedEvent += Layout_SelectionChangedEvent;
            propertyGridControl1.CellValueChanged += PropertyGridControl1_CellValueChanged;

            TK.GraphComponents.CustomData.MyCollectionEditor.MyFormClosed += new TK.GraphComponents.CustomData.MyCollectionEditor.MyFormClosedEventHandler(MyCollectionEditor_MyFormClosed);

            //SCRIPT MENU ITEM -------------------------------------------------------------
            //string path = @"\\NHAMDS\ToonKit\ToonKit\Rnd\OSCAR_Scripts";

            string path = @"C:\\Users\amandinep\Documents\Toonkit\Maya2018\scripts\tkMenu";
            NodesLayout.CreateScriptsMenu(scriptstoolStripMenuItem, path, MenuItemClickHandlerAll);
            //------------------------------------------------------------------------------

        }

        private void MenuItemClickHandlerAll(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            string tag = (string)clickedItem.Tag;

            if (tag == "ReloadMenuItemClickHandler")
            {
                scriptstoolStripMenuItem.DropDownItems.Clear();
                string path = (string)scriptstoolStripMenuItem.Tag;
                NodesLayout.CreateScriptsMenu(scriptstoolStripMenuItem, path, MenuItemClickHandlerAll);
            }
            else
            {
                string code = System.IO.File.ReadAllText(tag);
                //Console.WriteLine(code);

                if (tag.EndsWith(".py"))
                {
                    NodalDirector.EvaluatePython(code);
                }
                if (tag.EndsWith(".cs"))
                {
                    NodalDirector.Evaluate(code);
                }
            }
        }

        private void PropertyGridControl1_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            char[] rmv = { 'r', 'o', 'w' };
            string propertyName = e.Row.Name.TrimStart(rmv);

            List<NodeBase> nodes = tK_NodalEditorUCtrl1.Layout.Selection.Selection;
            foreach(Node node in nodes)
            {
                NodalDirector.SetProperty(node.FullName, propertyName, propertyGridControl1.GetCellValue(e.Row, e.CellIndex));
            }
            Console.WriteLine("properties "+ e.Row.Name + " " + e.Value +" "+ propertyGridControl1.GetCellValue(e.Row, e.CellIndex));
        }

        private void Layout_SelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            object[] obj = new object[e.Selection.Count];
            
            List<string> types = new List<string>();
            int i = 0;

            foreach (Node nd in e.Selection)
            {
                if (!types.Contains(nd.NativeName))
                {
                    types.Add(nd.NativeName);
                }

                Node node = (Node)Activator.CreateInstance(nd.GetType(), new object[0]);

                node.Copy(nd, false);

                //if (node is Compound)
                //{
                //    (node as RigCompound).Active = false;
                //}
                //else
                //{
                //    (node as RigNode).Active = false;
                //}

                obj[i] = node;
                i++;
            }
            
            propertyGridControl1.SelectedObjects = obj;
            //propertyGridControl1.SelectedObjects = new object[] { e.Selection };
        }

        void MyCollectionEditor_MyFormClosed(object sender, FormClosedEventArgs e)
        {


            Console.WriteLine("FORM CLOSED");
            TK.GraphComponents.CustomData.MyCollectionEditor closedCollection = sender as TK.GraphComponents.CustomData.MyCollectionEditor;
            Console.WriteLine("Properties "+ closedCollection.propertyLabel);
            List<NodeBase> nodes = tK_NodalEditorUCtrl1.Layout.Selection.Selection;
            int k = 0;
            
            foreach (Node node in nodes)
            {
                Console.WriteLine("Node name " + node.FullName);
                Port portCopy;
                PortObj portObjCopy;
                switch (closedCollection.propertyLabel)
                {
                    case "Inputs":
                        int i = 0;
                        node.Inputs.Sort(new PortsSorter((propertyGridControl1.SelectedObjects[k] as Node).Inputs));
                        foreach (Port port in node.Inputs)
                        {
                            portCopy = (propertyGridControl1.SelectedObjects[k] as Node).Inputs[i];

                            //Visible
                            if (portCopy.Visible != port.Visible)
                            {
                                NodalDirector.SetPortProperty(node.FullName, port.FullName, false, "Visible", portCopy.Visible);
                                //port.Visible = portCopy.Visible;
                            }
                            i++;
                        }
                        node.RefreshPortsIndices();
                        node.Parent.RefreshPorts();
                        tK_NodalEditorUCtrl1.Layout.InvalidateAll();
                        break;
                    case "Outputs":
                        i = 0;
                        node.Outputs.Sort(new PortsSorter((propertyGridControl1.SelectedObjects[k] as Node).Outputs));
                        foreach (Port port in node.Outputs)
                        {
                            portCopy = (propertyGridControl1.SelectedObjects[k] as Node).Outputs[i];

                            //Visible
                            if (portCopy.Visible != port.Visible)
                            {
                                NodalDirector.SetPortProperty(node.FullName, port.FullName, true, "Visible", portCopy.Visible);
                                //port.Visible = portCopy.Visible;
                            }
                            i++;
                        }
                        node.RefreshPortsIndices();
                        node.Parent.RefreshPorts();
                        tK_NodalEditorUCtrl1.Layout.InvalidateAll();
                        break;
                    case "Elements":
                        //Re-order
                        node.Elements.Sort(new ElementsSorter((propertyGridControl1.SelectedObjects[k] as Node).Elements));

                        //Update changed Values
                        i = 0;
                        foreach (PortObj port in node.Elements)
                        {
                            portObjCopy = (propertyGridControl1.SelectedObjects[k] as Node).Elements[i];
                        }
                        break;
                }
                k++;
            }

            //RigNode edited = null;

            //bool updateDisplays = false;
            //bool updateSizes = false;
            //bool updateRig = false;
            //bool updateBehaviour = true;

            //if (storedRig.rig != null && storedRig.inspecting)
            //{
            //    if (!closedCollection.Cancelled)
            //    {
            //        edited = storedRig.rig;
            //        updateDisplays = closedCollection.propertyLabel == "Elements";
            //        storedRig.inspecting = false;
            //        SavedRig.Copy(storedRig.rig, true);
            //    }
            //    else
            //    {
            //        storedRig.rig.Copy(SavedRig, true);
            //    }
            //}
            //else
            //{
            //    if (!closedCollection.Cancelled && inspector.Inspected != null && inspector.Inspected.Count > 0 && inspector.Inspected[0].NodeType == "Node" && inspector.Inspected[0].Parent != null)
            //    {
            //        if (closedCollection.propertyLabel == "Helpers")
            //        {
            //            foreach (Node node in inspector.Inspected)
            //            {
            //                edited = node as RigNode;
            //                if (edited != null)
            //                {
            //                    edited.GuideHelpers = (inspector.Mediator as RigNode).GuideHelpers.Copy();
            //                    if (edited.CurrentState == ApparatusState.Guide)
            //                    {
            //                        edited.Update();
            //                    }
            //                }
            //            }
            //            return;
            //        }

            //        edited = inspector.Inspected[0] as RigNode;
            //        edited.ReadDisplays();

            //        Port curPort;
            //        RigElement curPortObj;
            //        CG_PortParam curPortParam;
            //        int counter = 0;

            //        switch (closedCollection.propertyLabel)
            //        {
            //            case "Inputs":
            //                //Re-order
            //                edited.Inputs.Sort(new PortsSorter(inspector.Mediator.Inputs));

            //                //Update changed Values
            //                counter = 0;
            //                foreach (Port port in edited.Inputs)
            //                {
            //                    //Name
            //                    curPort = inspector.Mediator.Inputs[counter];
            //                    if (curPort.Name != port.Name)
            //                    {
            //                        port.Name = curPort.Name;
            //                    }

            //                    //Visible
            //                    if (curPort.Visible != port.Visible)
            //                    {
            //                        port.Visible = curPort.Visible;
            //                    }

            //                    counter++;
            //                }

            //                edited.RefreshPortsIndices();
            //                edited.Parent.RefreshPorts();

            //                RigsLayout.InvalidateAll();
            //                break;
            //            case "Outputs":
            //                //Re-order
            //                edited.Outputs.Sort(new PortsSorter(inspector.Mediator.Outputs));

            //                //Update changed Values
            //                counter = 0;
            //                foreach (Port port in edited.Outputs)
            //                {
            //                    //Name
            //                    curPort = inspector.Mediator.Outputs[counter];
            //                    if (curPort.Name != port.Name)
            //                    {
            //                        port.Name = curPort.Name;
            //                    }

            //                    //Visible
            //                    if (curPort.Visible != port.Visible)
            //                    {
            //                        port.Visible = curPort.Visible;
            //                    }

            //                    counter++;
            //                }

            //                edited.RefreshPortsIndices();
            //                edited.Parent.RefreshPorts();

            //                RigsLayout.InvalidateAll();
            //                break;
            //            case "Elements":
            //                //Re-order
            //                edited.Elements.Sort(new ElementsSorter(inspector.Mediator.Elements));

            //                //Update changed Values
            //                counter = 0;
            //                foreach (PortObj port in edited.Elements)
            //                {
            //                    RigElement portElem = port as RigElement;
            //                    if (portElem != null)
            //                    {
            //                        curPortObj = inspector.Mediator.Elements[counter] as RigElement;

            //                        //GuideSize
            //                        if (curPortObj.GuideSize != portElem.GuideSize)
            //                        {
            //                            portElem.GuideSize = curPortObj.GuideSize;
            //                            updateSizes = true;
            //                        }

            //                        //LOD
            //                        if (curPortObj.LOD != portElem.LOD)
            //                        {
            //                            portElem.LOD = curPortObj.LOD;
            //                            updateDisplays = true;
            //                        }

            //                        //Mirror
            //                        if (curPortObj.Mirror != portElem.Mirror)
            //                        {
            //                            portElem.Mirror = curPortObj.Mirror;
            //                            if (edited.CurrentState == ApparatusState.Rig && edited.HorizontalLocation == HorizontalLocations.Right || edited.VerticaLocation == VerticalLocations.Bottom || edited.DepthLocation == DepthLocations.Back)
            //                            {
            //                                updateRig = true;
            //                            }
            //                        }

            //                        //MirrorRefObject
            //                        if (curPortObj.MirrorRefObject != portElem.MirrorRefObject)
            //                        {
            //                            portElem.MirrorRefObject = curPortObj.MirrorRefObject;
            //                        }

            //                        //MirrorStrategy
            //                        if (curPortObj.MirrorStrategy != portElem.MirrorStrategy)
            //                        {
            //                            portElem.MirrorStrategy = curPortObj.MirrorStrategy;
            //                            if (edited.CurrentState == ApparatusState.Rig && edited.HorizontalLocation == HorizontalLocations.Right || edited.VerticaLocation == VerticalLocations.Bottom || edited.DepthLocation == DepthLocations.Back)
            //                            {
            //                                updateRig = true;
            //                            }
            //                        }

            //                        //Size
            //                        if (curPortObj.Size != portElem.Size)
            //                        {
            //                            portElem.Size = curPortObj.Size;
            //                            updateSizes = true;
            //                        }

            //                        //CustomColor
            //                        if (curPortObj.CustomColor != portElem.CustomColor)
            //                        {
            //                            portElem.CustomColor = curPortObj.CustomColor;
            //                            updateDisplays = true;
            //                        }

            //                        //GuideVis
            //                        if (curPortObj.OverrideGuideVis != portElem.OverrideGuideVis)
            //                        {
            //                            portElem.OverrideGuideVis = curPortObj.OverrideGuideVis;
            //                        }

            //                        //OverrideDisplay
            //                        if (curPortObj.OverrideDisplay != portElem.OverrideDisplay)
            //                        {
            //                            portElem.OverrideDisplay = curPortObj.OverrideDisplay;
            //                        }

            //                        //CustomDisplay
            //                        if (curPortObj.CustomDisplay != portElem.CustomDisplay)
            //                        {
            //                            portElem.CustomDisplay = curPortObj.CustomDisplay;
            //                            updateDisplays = true;
            //                        }

            //                        //SpecialBehaviour
            //                        if (curPortObj.SpecialBehavior != portElem.SpecialBehavior)
            //                        {
            //                            portElem.SpecialBehavior = curPortObj.SpecialBehavior;
            //                            updateBehaviour = true;
            //                        }

            //                        //AbsoluteOffset
            //                        if (!curPortObj.AbsoluteOffset.FuzzyEquals(portElem.AbsoluteOffset))
            //                        {
            //                            portElem.AbsoluteOffset = curPortObj.AbsoluteOffset.Copy();
            //                            if (edited.CurrentState == ApparatusState.Rig)
            //                            {
            //                                updateRig = true;
            //                            }
            //                        }

            //                        //AbsoluteOffsetMirror
            //                        if (curPortObj.AbsoluteOffsetMirror != portElem.AbsoluteOffsetMirror)
            //                        {
            //                            portElem.AbsoluteOffsetMirror = curPortObj.AbsoluteOffsetMirror;
            //                        }

            //                        //AbsoluteOffsetCompensation
            //                        if (curPortObj.AbsoluteOffsetCompensation != portElem.AbsoluteOffsetCompensation)
            //                        {
            //                            portElem.AbsoluteOffsetCompensation = curPortObj.AbsoluteOffsetCompensation;
            //                            if (edited.CurrentState == ApparatusState.Rig)
            //                            {
            //                                updateRig = true;
            //                            }
            //                        }

            //                        //Deepness
            //                        if (curPortObj.Deepness != portElem.Deepness)
            //                        {
            //                            portElem.Deepness = curPortObj.Deepness;
            //                            if (edited.CurrentState == ApparatusState.Rig)
            //                            {
            //                                updateRig = true;
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        CG_PortParam portParam = port as CG_PortParam;
            //                        if (portParam != null)
            //                        {
            //                            curPortParam = inspector.Mediator.Elements[counter] as CG_PortParam;

            //                            //Value
            //                            if (portParam.Value != curPortParam.Value)
            //                            {
            //                                portParam.Value = curPortParam.Value;
            //                                portParam.OwnerRig.ValueChanged(portParam);
            //                            }

            //                            //ValuesLabels
            //                            if (TypesHelper.Join(portParam.ValuesLabels) != TypesHelper.Join(curPortParam.ValuesLabels))
            //                            {
            //                                portParam.ValuesLabels = curPortParam.ValuesLabels;
            //                                if (edited.CurrentState == ApparatusState.Rig)
            //                                {
            //                                    updateRig = true;
            //                                }
            //                                //portParam.OwnerRig.DefinitionChanged(portParam);
            //                            }
            //                        }
            //                    }
            //                    counter++;
            //                }
            //                break;
            //        }
            //    }
            //}

            ////What about : Name ? DisplayOffset ? Trans ?
            //if (edited != null)
            //{
            //    if (updateDisplays)
            //    {
            //        edited.UpdateDisplays(true);
            //    }

            //    if (updateSizes)
            //    {
            //        edited.ResizeElements();
            //    }

            //    if (updateRig)
            //    {
            //        edited.Update();
            //    }
            //    else if (updateBehaviour)
            //    {
            //        edited.UpdateElements();
            //    }
            //}
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
            NodalDirector.New(true);
        }

        //private void openToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    DialogResult rslt = openFileDialog1.ShowDialog();

        //    if (rslt == DialogResult.OK)
        //    {
        //        Compound openedComp = null;

        //        using (FileStream fileStream = new FileStream(openFileDialog1.FileName, FileMode.Open))
        //        {
        //            openedComp = NodesSerializer.GetInstance().CompoundSerializers["Default"].Deserialize(fileStream) as Compound;
        //        }

        //        if (openedComp != null)
        //        {
        //            Manager.NewLayout(openedComp, false);
        //            NodalLayout.ChangeFocus(true);
        //            NodalLayout.Frame(Manager.CurCompound.Nodes);
        //            NodalLayout.Invalidate();
        //        }
        //    }
        //}

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult rslt = openFileDialog1.ShowDialog();

            if (rslt == DialogResult.OK)
            {
                string inPath = openFileDialog1.FileName;
                NodalDirector.Open(inPath, true);
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
            NodalDirector.Get().verbose = false;

            try
            {
                if (tabControl1.SelectedIndex == 0)
                {
                    NodalDirector.Get().history.BeginCompoundDo();
                    NodalDirector.Evaluate(csEditControl.Text.Replace("\f", "\n"));
                    NodalDirector.Get().history.EndCompoundDo();
                }
                else
                {
                    NodalDirector.Get().history.BeginCompoundDo();
                    NodalDirector.EvaluatePython(pyEditControl.Text.Replace("\f", "\n"));
                    NodalDirector.Get().history.EndCompoundDo();
                }
            }
            catch
            {
                
                throw;
            }

            NodalDirector.Get().verbose = true;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NodalDirector.CanUndo())
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

        private void scriptstoolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
