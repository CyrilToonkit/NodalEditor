using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using TK.BaseLib.CustomData;
using TK.NodalEditor.Tags;
using TK.GraphComponents.CustomData;
using TK.NodalEditor.Log;
using TK.GraphComponents.Dialogs;
using TK.BaseLib;
using System.Xml.Serialization;
using System.Drawing.Drawing2D;

namespace TK.NodalEditor.NodesLayout
{
    public enum LinksArrows
    {
        None, SharpArrow, SolidArrow, Lock, Scaling
    }

    public enum Reconnecting
    {
        None, Input, Output
    }

    public partial class NodesLayout : UserControl
    {
        float[] DASHPATTERN = new float[] { 4f, 2f };

        // === Events ==========================================================================

        public event InteractionEventHandler InteractionEvent;
        public event SelectionChangedEventHandler SelectionChangedEvent;
        public event LinkSelectionChangedEventHandler LinkSelectionChangedEvent;
        public event FocusChangedEventHandler FocusChangedEvent;
        public event PortClickEventHandler PortClickEvent;

        public virtual void OnInteraction(InteractionEventArgs e)
        {
            InteractionEvent(this, e);
        }
        public virtual void OnPortClick(PortClickEventArgs e)
        {
            PortClickEvent(this, e);
        }
        public virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectionChangedEvent(this, e);
        }
        public virtual void OnLinkSelectionChanged(LinkSelectionChangedEventArgs e)
        {
            LinkSelectionChangedEvent(this, e);
        }
        public virtual void OnFocusChanged(FocusChangedEventArgs e)
        {
            FocusChangedEvent(this, e);
        }

        // CONSTRUCTORS ================================================================
        private Bitmap mDisplayIcon;

        public NodesLayout()
        {
            InitializeComponent();
            DoubleBuffered = true;

            this.MouseWheel += new MouseEventHandler(this.RigNodesLayout_MouseWheel);
            nodeMenuStrip.Closed += new ToolStripDropDownClosedEventHandler(contextMenuStrip1_Closed);
        }

        public NodesLayout(string inRootPath, NodesManager inManager, CompoundPortPad inputsPad, CompoundPortPad outputsPad, ToolStrip inBreadCrumbs)
        {
            InitializeComponent();
            DoubleBuffered = true;
            //CompoundPorts

            this.MouseWheel += new MouseEventHandler(this.RigNodesLayout_MouseWheel);
            nodeMenuStrip.Closed += new ToolStripDropDownClosedEventHandler(contextMenuStrip1_Closed);
            Init(inRootPath, inManager, inputsPad, outputsPad, inBreadCrumbs);
        }

        public void Init(string inPath, NodesManager inManager, CompoundPortPad inputsPad, CompoundPortPad outputsPad, ToolStrip inBreadCrumbs)
        {
            mDisplayIcon = global::TK.NodalEditor.Properties.Resources.DisplayIcon;
            Inputs = inputsPad;
            Outputs = outputsPad;
            BreadCrumbs = inBreadCrumbs;

            InitializeContextMenu();
            Manager = inManager;
            //Selection = new SelectionManager(inManager);
            RootPath = inPath;

            overlay = new Overlay(this);

            Preferences = new NodalEditorPreferences(RootPath);
            Manager.Preferences = Preferences;

            graphics = CreateGraphics();
            CreateGraphicalElements();

            this.FocusChangedEvent += new FocusChangedEventHandler(NodesLayout_FocusChangedEvent);
            this.SelectionChangedEvent += new SelectionChangedEventHandler(NodesLayout_SelectionChangedEvent);
            this.LinkSelectionChangedEvent += new LinkSelectionChangedEventHandler(NodesLayout_LinkSelectionChangedEvent);
            this.PortClickEvent += new PortClickEventHandler(NodesLayout_PortClickEvent);
            this.InteractionEvent += new InteractionEventHandler(NodesLayout_InteractionEvent);

            if (Parent != null)
            {
                Location = new Point();
                Parent.Resize += new EventHandler(Parent_Resize);
                Inputs.Init(this, false);
                Outputs.Init(this, true);
            }

            ChangeFocus(true);

            IsInitialised = true;
        }

        // === Empty base delegates ==========================================================================

        void NodesLayout_InteractionEvent(object sender, InteractionEventArgs e)
        {

        }

        void NodesLayout_OperationEvent(object sender, InteractionEventArgs e)
        {

        }

        void NodesLayout_PortClickEvent(object sender, PortClickEventArgs e)
        {

        }

        void NodesLayout_LinkSelectionChangedEvent(object sender, LinkSelectionChangedEventArgs e)
        {

        }

        void NodesLayout_SelectionChangedEvent(object sender, SelectionChangedEventArgs e)
        {
            Selection.DeselectLinks();
        }

        void NodesLayout_FocusChangedEvent(object sender, FocusChangedEventArgs e)
        {

        }

        // === Log & Prefs ==========================================================================
        const long DBLCLICKDELTA = 3000000;
        public string RootPath;

        internal LogSystem log = new LogSystem();
        public NodalEditorPreferences Preferences;
        List<Link> links = new List<Link>();
        Dictionary<Link, GraphicsPath> paths = new Dictionary<Link, GraphicsPath>();

        bool IsInitialised = false;

        int ContextX = 0;
        int ContextY = 0;

        // === Nodes ==========================================================================

        public NodesManager Manager;

        long LastClick = 0;
        public bool WasDoubleClicked()
        {
            long click = DateTime.Now.Ticks;
            if ((click - LastClick) < DBLCLICKDELTA)
            {
                LastClick = 0;
                return true;
            }
            else
            {
                LastClick = click;
            }

            return false;
        }

        CompoundPortPad Inputs;
        CompoundPortPad Outputs;
        ToolStrip BreadCrumbs;

        Size zeroSize = new Size(0,0);

        public SelectionManager Selection;

        int portHeight = 18;

        const int NODE_MIN_WIDTH = 110;
        const int NODE_MIN_HEIGHT = 35;
        public int PortHeight
        {
            get { return (int)(portHeight * LayoutSize); }
            set { portHeight = (int)(value / LayoutSize); }
        }

        // === Graphical Elements ==========================================================================

        Graphics graphics;
        
        public Pen GridPen = new Pen(Color.LightGray);
        public Pen FramePen = new Pen(Color.Black);
        public Pen WhitePen = new Pen(Color.White);
        public Pen widenPen = new Pen(Color.Black, 10f);
        public Pen HoverPen = new Pen(Color.LightSteelBlue, 3f);

        public Pen LinkPen = new Pen(Color.Red);
        public Pen FatPen = new Pen(Color.Black, 3f);
        public Brush LinkBrush = Brushes.Red;

        public Brush NodeBrush = Brushes.DodgerBlue;
        public Brush NodeSelectedBrush = Brushes.DeepSkyBlue;
        public Brush CompoundBrush = Brushes.SandyBrown;
        public Brush CompoundSelectedBrush = Brushes.Orange;

        public Font lightFont;
        public Font specialFont;
        public Font HighlightedPortFont = new Font("Tahoma", 8.25f, FontStyle.Bold);

        public Brush lightFontBrush = Brushes.Black;

        public Font strongFont;

        public Brush strongFontBrush = Brushes.Black;

        public Brush CompoundPadBrush = Brushes.Black;

        public Dictionary<string, List<Brush>> NodeCategoryBrushes;

        public Dictionary<string, Brush> LinkCategoryBrushes;
        public Dictionary<string, Pen> LinkCategoryPens;
        public Dictionary<string, LinkState> LinkStates;

        public Dictionary<string, Image> StatesIcons;
        public Dictionary<string, bool> TypesVisible;
        public Dictionary<string, bool> NodesVisible;

        Overlay overlay;

        public int LeftStart = 0;
        public int RightStart = 0;

        int BaseWidth = 400;
        int BaseHeight = 400;

        // === UI Data  ==========================================================================

        public int XLoc
        {
            get
            {
                if (Parent == null)
                    return 0;

                return (int)((-Location.X + LeftStart + (Parent.Width - LeftStart - RightStart) / 2.0) / LayoutSize);
            }
            set
            {
                if (Parent != null)
                    Location = new Point((int)(-value * LayoutSize + (Parent.Width - LeftStart - RightStart) / 2.0) + LeftStart, Location.Y);
            }
        }

        public int YLoc
        {
            get 
            {
                if (Parent == null)
                    return 0;

                return (int)((-Location.Y + Parent.Height / 2.0) / LayoutSize);
            }
            set
            {
                if (Parent != null)
                    Location = new Point(Location.X, (int)(-value * LayoutSize + Parent.Height / 2.0));
            }
        }

        public newPort newPortForm = new newPort();

        int CurConnection = -1;
        int CurConnection2 = -1;
        bool IsDragging = false;
        bool IsPanning = false;
        bool InMiniMap = false;
        bool HasMoved = false;
        Point HitPoint = new Point();
        Point OldPos = new Point();
        Point Hit = new Point();

        //Reconnecing a link
        Link detachLink = null;
        Reconnecting reconnecting = Reconnecting.None;

        Node hitNode = null;
        Node hoverNode = null;
        Port hoverPort = null;

        Node detachNode = null;
        //Accessor for the rigs contextMenu

        public void SetShortCut(string itemName, string shortCut)
        {
            foreach (ToolStripItem item in nodeMenuStrip.Items)
            {
                if (item is ToolStripMenuItem &&  item.Name == itemName)
                {
                    (item as ToolStripMenuItem).ShortcutKeyDisplayString = shortCut;
                }
            }
        }

        // SIZE MEMBERS ================================================================

        private double mLayoutSize = 1;
        public double LayoutSize
        {
            get { return mLayoutSize; }
            set
            {
                mLayoutSize = value;
                PerformMyLayout(new Point(1,1));
            }
        }
        
        void contextMenuStrip1_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            Invalidate();
        }

        private void CreateGraphicalElements()
        {
            CreateLinksTools();
            CreateNodesTools();
            CreateLayoutTools();
            CreateFonts();
            CreateStates();
        }

        // Nodes Management ====================================================================

        //MOUSE EVENTS =========================================================================

        private void RigNodesLayout_MouseWheel(object sender, MouseEventArgs e)
        {
            if (new Rectangle(new Point( -Location.X, -Location.Y) , Size).Contains(e.Location))
            {
                double delta = (e.Delta / 1000.0);
                double NewSize = LayoutSize + delta;

                if (NewSize < Preferences.MinimumZoom)
                {
                    NewSize = Preferences.MinimumZoom;
                }
                else
                {
                    if (NewSize > Preferences.MaximumZoom)
                    {
                        NewSize = Preferences.MaximumZoom;
                    }
                }

                double NewWidth = (double)BaseWidth * NewSize;
                double Factor = (double)NewWidth / (double)Width;
                Point Loc = new Point((int)Math.Min(0, Location.X + (e.X - e.X * Factor)), (int)Math.Min(0, Location.Y + (e.Y - e.Y * Factor)));
                SetSize(NewSize, Loc);
            }
        }

        private void NodesLayout_MouseDown(object sender, MouseEventArgs e)
        {
            if (IsInitialised)
            {
                hitNode = GetHitNode(e.Location);

                if (hitNode == null)
                {
                    HitPoint = e.Location;
                    Hit = PointToScreen(HitPoint);

                    switch (e.Button)
                    {
                        case MouseButtons.Left: //Classic case, DragSelect

                            detachLink = GetHitLink(e.Location);

                            if (detachLink == null)
                            {
                                //Rectangle selection with mouse left click
                                if (!IsDragging)
                                {
                                    overlay.SelectRectangle.Size = zeroSize;
                                    IsDragging = true;
                                }
                                
                            }
                            break;

                        case MouseButtons.Middle: //Pan Layout
                            if (!IsPanning)
                            {
                                IsPanning = true;
                                OldPos = Location;

                                //Maybe we are panning in the minimap

                                if (Preferences.ShowMap)
                                {
                                    if (e.X > overlay.LeftStart && e.X < overlay.LeftStart + overlay.VisWidth && e.Y > overlay.TopStart && e.Y < overlay.TopStart + overlay.VisHeight)
                                    {
                                        InMiniMap = true;
                                    }
                                }
                            }
                            break;
                    }
                }
                else
                {
                    Node_MouseDown(hitNode, e);
                }
            }
        }

        private void NodesLayout_MouseUp(object sender, MouseEventArgs e)
        {
            
            if (IsInitialised)
            {
                if (hitNode == null)
                {
                    bool doubleClicked = (e.Button == MouseButtons.Left && WasDoubleClicked());

                    if (overlay.DragSelect)
                    {
                        Selection.Select(overlay.SelectRectangle, LayoutSize, ModifierKeys);
                        Invalidate();
                        OnLinkSelectionChanged(new LinkSelectionChangedEventArgs(null));
                        OnSelectionChanged(new SelectionChangedEventArgs(Selection.Selection));
                        overlay.DragSelect = false;
                    }
                    else
                    {
                        //Reconnection 
                        if (CurConnection2 != -1)
                        {
                            Port connectPort = detachNode.GetPort(CurConnection2);
                            Node HitCtrl = GetHitNode(e.Location);
                            if (HitCtrl != null)
                            {
                                Point TransPos = new Point(e.Location.X - (int)(HitCtrl.UIx * LayoutSize), e.Location.Y - (int)(HitCtrl.UIy * LayoutSize));
                                int portIndex = GetPortClick(HitCtrl, TransPos.X, TransPos.Y);
                                if (portIndex > -1)
                                {
                                    //Check
                                    Port depositPort = HitCtrl.GetPort(portIndex);

                                    string Error = "";
                                    if (depositPort != null)
                                    {
                                        //Change display port Compound if Node is in Compound 
                                        if (HitCtrl.IsIn(Manager.CurCompound))
                                        {
                                            if (depositPort.IsOutput)
                                            {
                                                Outputs.GetPort(depositPort).Visible = true;
                                            }
                                            else
                                            {
                                                Inputs.GetPort(depositPort).Visible = true;
                                            }
                                            RefreshPorts();
                                        }

                                        if (!depositPort.IsOutput && connectPort.IsOutput)
                                        {
                                            if (reconnecting == Reconnecting.Input)
                                            {
                                                NodalDirector.ReConnect(detachLink.Target.Owner.FullName, detachLink.Target.FullName, detachNode.FullName, connectPort.FullName,
                                                                        HitCtrl.FullName, depositPort.FullName, detachNode.FullName, connectPort.FullName);
                                            }
                                            else if(reconnecting == Reconnecting.Output)
                                            {
                                                NodalDirector.ReConnect(detachLink.Source.Owner.FullName, detachLink.Source.FullName, detachNode.FullName, connectPort.FullName, 
                                                                        HitCtrl.FullName, depositPort.FullName, detachNode.FullName, connectPort.FullName);

                                            }
                                        }
                                        else
                                        {
                                            if (depositPort.IsOutput && !connectPort.IsOutput)
                                            {
                                                if (reconnecting == Reconnecting.Input)
                                                {
                                                    NodalDirector.ReConnect(detachLink.Target.Owner.FullName, detachLink.Target.FullName, detachNode.FullName, connectPort.FullName, 
                                                                            detachNode.FullName, connectPort.FullName, HitCtrl.FullName, depositPort.FullName);
                                                }
                                                else if (reconnecting == Reconnecting.Output)
                                                {
                                                    NodalDirector.ReConnect(detachNode.FullName, connectPort.FullName, detachLink.Source.Owner.FullName, detachLink.Source.FullName,
                                                                            detachNode.FullName, connectPort.FullName, HitCtrl.FullName, depositPort.FullName);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //plug in new Port
                                        if (HitCtrl.GetPortTypes().Contains(connectPort.NodeElementType))
                                        {
                                            if (connectPort.IsOutput)
                                            {
                                                DialogResult result = newPortForm.ShowDialog(connectPort.Name, HitCtrl, connectPort);
                                                if (result == DialogResult.OK)
                                                {
                                                    depositPort = HitCtrl.NewPort(newPortForm.PortName, newPortForm.PortType, true, newPortForm.CustomParams, newPortForm.TypeMetaData);
                                                    if (depositPort != null)
                                                    {
                                                        if (reconnecting == Reconnecting.Input)
                                                        {
                                                            NodalDirector.ReConnect(detachLink.Target.Owner.FullName, detachLink.Target.FullName, detachNode.FullName, connectPort.FullName,
                                                                                    HitCtrl.FullName, depositPort.FullName, detachNode.FullName, connectPort.FullName);
                                                        }
                                                        else if (reconnecting == Reconnecting.Output)
                                                        {
                                                            NodalDirector.ReConnect(detachLink.Source.Owner.FullName, detachLink.Source.FullName, detachNode.FullName, connectPort.FullName, 
                                                                                    HitCtrl.FullName, depositPort.FullName, detachNode.FullName, connectPort.FullName);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                DialogResult result = newPortForm.ShowDialog(connectPort.Name, HitCtrl, connectPort);
                                                if (result == DialogResult.OK)
                                                {
                                                    depositPort = HitCtrl.NewPort(newPortForm.PortName, newPortForm.PortType, false, newPortForm.CustomParams, newPortForm.TypeMetaData);
                                                    if (depositPort != null)
                                                    {
                                                        if (reconnecting == Reconnecting.Input)
                                                        {
                                                            NodalDirector.ReConnect(detachLink.Target.Owner.FullName, detachLink.Target.FullName, detachNode.FullName, connectPort.FullName, 
                                                                                    detachNode.FullName, connectPort.FullName, HitCtrl.FullName, depositPort.FullName);
                                                        }
                                                        else if (reconnecting == Reconnecting.Output)
                                                        {
                                                            NodalDirector.ReConnect(detachLink.Source.Owner.FullName, detachLink.Source.FullName, detachNode.FullName, connectPort.FullName, 
                                                                                    detachNode.FullName, connectPort.FullName, HitCtrl.FullName, depositPort.FullName);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            log.AddLog(HitCtrl.Name + " disallow adding ports of type " + connectPort.NodeElementType, 15, 2);
                                        }
                                    }

                                    if (Error != "")
                                    {
                                        log.AddLog(Error, 15, 2);
                                    }
                                }
                            }
                            else
                            {
                                //Dropped on empty space
                                overlay.ConnectArrow = new Point[0];
                            }
                            detachNode = null;
                            overlay.ConnectArrow = new Point[0];
                            CurConnection2 = -1;
                            Invalidate();
                        }
                        else
                        {
                            //Select a link with mouse left click up and deselect it with mouse right click
                            //Maybe we hit a link ?
                            Link HitLink = GetHitLink(e.Location);

                            if (HitLink != null)
                            {
                                Selection.DeselectAll();
                                OnLinkSelectionChanged(new LinkSelectionChangedEventArgs(HitLink));

                                Selection.SelectLink(HitLink);
                                Invalidate();

                                if (e.Button == MouseButtons.Right)
                                {
                                    linkMenuStrip.Tag = HitLink;
                                    foreach (ToolStripItem item in linkMenuStrip.Items)
                                    {
                                        LinkContextTag tag = item.Tag as LinkContextTag;
                                        if (tag != null)
                                        {
                                            tag.links = Selection.GetSelectedLinks();
                                            item.Visible = tag.isContextConsistent();
                                        }
                                    }
                                    linkMenuStrip.Show(PointToScreen(e.Location));
                                }
                            }
                            else
                            {
                                if (e.Button == MouseButtons.Right)
                                {
                                    if (Manager.ClipBoard.Count > 0)
                                    {
                                        foreach (ToolStripItem item in rootMenuStrip.Items)
                                        {
                                            NodeContextTag tag = item.Tag as NodeContextTag;
                                            if (tag != null)
                                            {
                                                tag.nodes = Manager.ClipBoard;
                                                item.Visible = tag.isContextConsistent();
                                            }
                                        }

                                        rootMenuStrip.Show(PointToScreen(e.Location));
                                    }
                                }
                                else
                                {
                                    if (doubleClicked)
                                    {
                                        Compound comp = Manager.CurCompound;
                                        Manager.ExitCompound();
                                        ChangeFocus(true);
                                        //Frame(Manager.CurCompound.Nodes);
                                        Frame(new List<Node> { comp });
                                    }
                                }
                            }
                        }
                    }

                    IsDragging = false;
                    IsPanning = false;
                    InMiniMap = false;

                }
                else
                {
                    Node_MouseUp(hitNode, e);
                }

                detachLink = null;
                reconnecting = Reconnecting.None;
            }
        }

        private void NodesLayout_MouseMove(object sender, MouseEventArgs e)
        {
            if (Preferences.ShowNodeTips)
            {
                Node potentialHover = GetHitNode(e.Location);
                if (hoverNode == null)
                {
                    if (potentialHover != null)
                    {
                        //Enter
                        hoverNode = potentialHover;
                        string nodeText = hoverNode.ToString();
                        if (nodeText != "")
                        {
                            customToolTip.Text = nodeText;
                            customToolTip.Show(e.Location, 2);
                        }
                    }
                }
                else
                {
                    if (potentialHover == null)
                    {
                        //Exit
                        hoverNode = null;
                        customToolTip.Hidden();
                    }
                    else
                    {
                        //Update
                        customToolTip.MoveAt(e.Location);
                        Point TransPos = new Point(e.Location.X - (int)(hoverNode.UIx * LayoutSize), e.Location.Y - (int)(hoverNode.UIy * LayoutSize));

                        int PortClick = GetPortClick(hoverNode, TransPos.X, TransPos.Y);
                        Port potentialHoverPort = hoverNode.GetPort(PortClick);

                        if (hoverPort == null || potentialHoverPort != null && customToolTip.Text != potentialHoverPort.Name)
                        {
                            if (potentialHoverPort != null)
                            {
                                //Enter Port
                                hoverPort = potentialHoverPort;
                                string nodeText = hoverPort.ToString();
                                if (nodeText != "")
                                {
                                    customToolTip.Text = nodeText;
                                    customToolTip.Show(e.Location, 2);
                                }
                            }
                        }
                        else
                        {
                            if (potentialHoverPort == null)
                            {
                                //Exit
                                hoverPort = null;
                                customToolTip.Hidden();
                            }
                            else
                            {
                                //Update
                            }
                        }
                    }
                }
            }
            else
            {
                hoverNode = null;
                hoverPort = null;
            }

            if (hitNode == null)
            {
                Point Moved = PointToScreen(e.Location);
                //We "dragged" something
                if (Math.Abs(e.X - HitPoint.X) + Math.Abs(e.Y - HitPoint.Y) > 2)
                {
                    if (IsDragging)
                    {
                        SuspendLayout();

                        overlay.DragSelect = true;

                        overlay.SelectRectangle.Location = new Point(Math.Min(HitPoint.X, e.X), Math.Min(HitPoint.Y, e.Y));
                        overlay.SelectRectangle.Width = e.X > HitPoint.X ? e.X - HitPoint.X : HitPoint.X - e.X;
                        overlay.SelectRectangle.Height = e.Y > HitPoint.Y ? e.Y - HitPoint.Y : HitPoint.Y - e.Y;

                        Invalidate();
                        ResumeLayout();
                    }
                    else
                    {
                        if (IsPanning)
                        {
                            IsInitialised = false;
                            Point Pos = OldPos;

                            if (InMiniMap)
                            {
                                Pos = Point.Add(Pos, new Size((int)((Hit.X - Moved.X) * (1 / overlay.ZoomRatio)), (int)((Hit.Y - Moved.Y) * (1 / overlay.ZoomRatio))));
                            }
                            else
                            {
                                Pos = Point.Add(Pos, new Size(Moved.X - Hit.X, Moved.Y - Hit.Y));
                            }

                            //Clamp new position
                            Pos.X = Math.Max(Pos.X, -Width + Parent.Width - LeftStart);
                            Pos.Y = Math.Max(Pos.Y, -Height + Parent.Height);
                            Pos.X = Math.Min(Pos.X, LeftStart);
                            Pos.Y = Math.Min(Pos.Y, 0);

                            Location = Pos;

                            IsInitialised = true;
                            Invalidate();
                        }
                        //Reconnection link
                        else if(detachLink != null)
                        {

                            if (reconnecting == Reconnecting.None)
                            {
                                double distance1 = -1;
                                double distance2 = -1;

                                Node SourceNode = detachLink.Source.Owner;
                                Node TargetNode = detachLink.Target.Owner;
                                Port foundPort = null;

                                if (NodeIsShowing(SourceNode.NodeElementType) && NodeIsShowing(TargetNode.NodeElementType))
                                {
                                    if (SourceNode.IsIn(Manager.CurCompound))
                                    {
                                        if (detachLink.Target.Owner.IsIn(Manager.CurCompound)) //CASE 1 : Source and Target in Compound
                                        {
                                            distance1 = e.Location.X - (detachLink.Source.Owner.UIx * LayoutSize + detachLink.Source.Owner.UIWidth * LayoutSize);
                                            distance2 = detachLink.Target.Owner.UIx * LayoutSize - e.Location.X;

                                            if (distance2 <= distance1) //Detach the link Target
                                            {
                                                int PortClick = detachLink.Source.DisplayIndex + 1000;
                                                detachNode = detachLink.Source.Owner;
                                                Port port = detachLink.Source;

                                                if (PortClick >= 0)
                                                {
                                                    if (port != null)
                                                    {
                                                        CurConnection2 = PortClick;
                                                        reconnecting = Reconnecting.Input;
                                                    }
                                                }
                                            }
                                            if (distance2 > distance1) //Detach the link Source
                                            {
                                                int PortClick = detachLink.Target.DisplayIndex;
                                                detachNode = detachLink.Target.Owner;
                                                Port port = detachLink.Target;

                                                if (PortClick >= 0)
                                                {
                                                    if (port != null)
                                                    {
                                                        CurConnection2 = PortClick;
                                                        reconnecting = Reconnecting.Output;
                                                    }
                                                }
                                            }

                                        }
                                        else //CASE 2 : Source in Compound, Target outside
                                        {
                                            foundPort = Outputs.GetPort(detachLink.Source);

                                            if (foundPort != null)
                                            {
                                                distance1 = e.Location.X - (detachLink.Source.Owner.UIx * LayoutSize + detachLink.Source.Owner.UIWidth * LayoutSize);
                                                distance2 = GetPortLocation(Manager.CurCompound,foundPort.Index).X - e.Location.X;

                                                if (distance2 <= distance1) //Detach the link Target
                                                {
                                                    int PortClick = detachLink.Source.DisplayIndex + 1000;
                                                    detachNode = detachLink.Source.Owner;
                                                    Port port = detachLink.Source;

                                                    if (PortClick >= 0)
                                                    {
                                                        if (port != null)
                                                        {
                                                            CurConnection2 = PortClick;
                                                            reconnecting = Reconnecting.Input;
                                                        }
                                                    }
                                                }
                                                if (distance2 > distance1) //Detach the link Source
                                                {
                                                    int PortClick = foundPort.Index + 1000;
                                                    detachNode = detachLink.Target.Owner;
                                                    Port port = foundPort;
                                                    
                                                    if (PortClick >= 0)
                                                    {
                                                        if (port != null)
                                                        {
                                                            CurConnection2 = detachLink.Target.DisplayIndex; //PortClick;
                                                            reconnecting = Reconnecting.Output;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                NodalDirector.Error("Cannot get source " + detachLink.Target.Name + " on " + Outputs.node.Name + " !!");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (detachLink.Target.Owner.IsIn(Manager.CurCompound)) //CASE 3 : Source outside, Target in Compound
                                        {
                                            foundPort = Inputs.GetPort(detachLink.Target);

                                            if (foundPort != null)
                                            {
                                                distance1 = e.Location.X - GetPortLocation(Manager.CurCompound, foundPort.Index + 1000).X;
                                                distance2 = detachLink.Target.Owner.UIx * LayoutSize - e.Location.X;

                                                if (distance2 <= distance1) //Detach the link Target
                                                {
                                                    int PortClick = foundPort.Index;
                                                    detachNode = detachLink.Source.Owner;
                                                    Port port = foundPort;

                                                    if (PortClick >= 0)
                                                    {
                                                        if (port != null)
                                                        {
                                                            CurConnection2 = detachLink.Source.DisplayIndex + 1000;//PortClick;
                                                            reconnecting = Reconnecting.Input;
                                                        }
                                                    }
                                                }
                                                if (distance2 > distance1) //Detach the link Source
                                                {
                                                    int PortClick = detachLink.Target.DisplayIndex;
                                                    detachNode = detachLink.Target.Owner;
                                                    Port port = detachLink.Target;

                                                    if (PortClick >= 0)
                                                    {
                                                        if (port != null)
                                                        {
                                                            CurConnection2 = PortClick;
                                                            reconnecting = Reconnecting.Output;
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                NodalDirector.Error("Cannot get Target " + detachLink.Target.Name + " on " + Inputs.node.Name + " !!");
                                            }
                                        }
                                        else //CASE 4 : Source and Target outside
                                        {
                                            distance1 = e.Location.X - (detachLink.Source.Owner.UIx * LayoutSize + detachLink.Source.Owner.UIWidth * LayoutSize);
                                            distance2 = detachLink.Target.Owner.UIx * LayoutSize - e.Location.X;

                                            if (distance2 <= distance1) //Detach the link Target
                                            {
                                                int PortClick = detachLink.Source.DisplayIndex + 1000;
                                                detachNode = detachLink.Source.Owner;
                                                Port port = detachLink.Source;

                                                if (PortClick >= 0)
                                                {
                                                    if (port != null)
                                                    {
                                                        CurConnection2 = PortClick;
                                                        reconnecting = Reconnecting.Input;
                                                    }
                                                }
                                            }
                                            if (distance2 > distance1) //Detach the link Source
                                            {
                                                int PortClick = detachLink.Target.DisplayIndex;
                                                detachNode = detachLink.Target.Owner;
                                                Port port = detachLink.Target;

                                                if (PortClick >= 0)
                                                {
                                                    if (port != null)
                                                    {
                                                        CurConnection2 = PortClick;
                                                        reconnecting = Reconnecting.Output;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if(reconnecting == Reconnecting.Input)
                            {
                                Node SourceNode = detachLink.Source.Owner;
                                Node TargetNode = detachLink.Target.Owner;
                                Port foundPort = null;

                                if (NodeIsShowing(SourceNode.NodeElementType) && NodeIsShowing(TargetNode.NodeElementType))
                                {
                                    if (SourceNode.IsIn(Manager.CurCompound)) 
                                    {
                                        if (detachLink.Target.Owner.IsIn(Manager.CurCompound)) //CASE 1 : Source and Target in Compound
                                        {
                                            overlay.ConnectPen = GetPen(detachLink.Source.NodeElementType);
                                            overlay.ConnectArrow = new Point[] { GetPortLocation(detachLink.Source.Owner, detachLink.Source.Index + 1000), e.Location };
                                        }
                                        else //CASE 2 : Source in Compound, Target outside
                                        {
                                            foundPort = Outputs.GetPort(detachLink.Source);
                                            if (foundPort != null)
                                            {
                                                overlay.ConnectPen = GetPen(detachLink.Source.NodeElementType);
                                                overlay.ConnectArrow = new Point[] { GetPortLocation(detachLink.Source.Owner, detachLink.Source.Index + 1000), e.Location };
                                            }
                                            else
                                            {
                                                NodalDirector.Error("Cannot get source " + detachLink.Source.Name + " on " + Outputs.node.Name + " !!");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (detachLink.Target.Owner.IsIn(Manager.CurCompound)) //CASE 3 : Source outside, Target in Compound
                                        {
                                            foundPort = Inputs.GetPort(detachLink.Target);
                                            if (foundPort != null)
                                            {
                                                overlay.ConnectPen = GetPen(detachLink.Source.NodeElementType);
                                                overlay.ConnectArrow = new Point[] { GetPortLocation(Inputs, foundPort.Index + 1000), e.Location };
                                            }
                                            else
                                            {
                                                NodalDirector.Error("Cannot get Target " + detachLink.Target.Name + " on " + Inputs.node.Name + " !!");
                                            }
                                        }
                                        else
                                        {
                                            //CASE 4 : Source and Target outside
                                            overlay.ConnectPen = GetPen(detachLink.Source.NodeElementType);
                                            overlay.ConnectArrow = new Point[] { GetPortLocation(detachLink.Source.Owner, detachLink.Source.Index + 1000), e.Location };
                                        }
                                    }
                                }

                            }
                            else if (reconnecting == Reconnecting.Output)
                            {
                                Node SourceNode = detachLink.Source.Owner;
                                Node TargetNode = detachLink.Target.Owner;
                                Port foundPort = null;

                                if (NodeIsShowing(SourceNode.NodeElementType) && NodeIsShowing(TargetNode.NodeElementType))
                                {
                                    if (SourceNode.IsIn(Manager.CurCompound))
                                    {
                                        if (detachLink.Target.Owner.IsIn(Manager.CurCompound)) //CASE 1 : Source and Target in Compound
                                        {
                                            overlay.ConnectPen = GetPen(detachLink.Target.NodeElementType);
                                            overlay.ConnectArrow = new Point[] { GetPortLocation(detachLink.Target.Owner, detachLink.Target.Index), e.Location };

                                        }
                                        else //CASE 2 : Source in Compound, Target outside
                                        {
                                            foundPort = Outputs.GetPort(detachLink.Source);
                                            if (foundPort != null)
                                            {
                                                overlay.ConnectPen = GetPen(detachLink.Target.NodeElementType);
                                                overlay.ConnectArrow = new Point[] { GetPortLocation(Outputs, foundPort.Index + 1000), e.Location };
                                            }
                                            else
                                            {
                                                NodalDirector.Error("Cannot get source " + detachLink.Target.Name + " on " + Outputs.node.Name + " !!");
                                            }
                                        }
                                    }
                                    else 
                                    {
                                        if (detachLink.Target.Owner.IsIn(Manager.CurCompound)) //CASE 3 : Source outside, Target in Compound
                                        {
                                            foundPort = Inputs.GetPort(detachLink.Target);
                                            if (foundPort != null)
                                            {
                                                overlay.ConnectPen = GetPen(detachLink.Source.NodeElementType);
                                                overlay.ConnectArrow = new Point[] { GetPortLocation(detachLink.Target.Owner, detachLink.Target.Index), e.Location };
                                            }
                                            else
                                            {
                                                NodalDirector.Error("Cannot get Target " + detachLink.Target.Name + " on " + Inputs.node.Name + " !!");
                                            }
                                        }
                                        else
                                        {
                                            //CASE 4 : Source and Target outside
                                            overlay.ConnectPen = GetPen(detachLink.Target.NodeElementType);
                                            overlay.ConnectArrow = new Point[] { GetPortLocation(detachLink.Target.Owner, detachLink.Target.Index), e.Location };
                                        }
                                    }
                                }
                                
                            }
                            Invalidate();


                            Point Position = PointToScreen(e.Location);
                            Point parentPos = Parent.PointToClient(Position);

                            if (CurConnection2 != -1) // Is Linking
                            {
                                if (overlay.ConnectArrow.Length != 2)
                                {
                                    return;
                                }

                                overlay.ConnectArrow[1] = e.Location;


                                //Pulling links outside the viewport

                                Position = Location;
                                if (parentPos.X < 0)
                                {
                                    Position.X = Math.Min(0, Location.X + 1);
                                }
                                else if (parentPos.X > Parent.Width)
                                {
                                    Position.X = Math.Max(Parent.Width - Width, Location.X - 1);
                                }

                                if (parentPos.Y < 0)
                                {
                                    Position.Y = Math.Min(0, Location.Y + 1);
                                }
                                else if (parentPos.Y > Parent.Height)
                                {
                                    Position.Y = Math.Max(Parent.Height - Height, Location.Y - 1);
                                }

                                Location = Position;
                                Invalidate();
                            }
                        }
                    }
                }

                if (Preferences.ShowHoveredLinks)
                {
                    //Call get hit link just to update hovered link
                    GetHitLink(e.Location);
                }
            }
            else
            {
                Node_MouseMove(hitNode, e);
            }

            if (log.Logs.Count > 0)
            {
                Invalidate();
            }
        }

        private void NodesLayout_KeyDown(object sender, KeyEventArgs e)
        {
                if (e.KeyCode == Keys.T)
                {
                    Console.WriteLine("coucou j'ai appuyé sur TAB");
                }
        }
        private void NodesLayout_KeyUp(object sender, KeyEventArgs e)
        {

        }

        //void Node_MouseUp(object sender, MouseEventArgs e)
        //{
        //    if (hitNode != null)
        //    {
        //        Node Node = sender as Node;
        //        bool doubleClicked = (e.Button == MouseButtons.Left && WasDoubleClicked());

        //        switch (e.Button)
        //        {
        //            case MouseButtons.Left:
        //            if (CurConnection != -1)
        //            {
        //                Port connectPort = connectPort = Node.GetPort(CurConnection);

        //                if (doubleClicked)
        //                {
        //                    PortClickEvent(Node, new PortClickEventArgs(connectPort.Index, connectPort.IsOutput));
        //                }
        //                else  // Connecting
        //                {
        //                    Node HitCtrl = GetHitNode(e.Location);

        //                    if (HitCtrl != null)
        //                    {
        //                        Point TransPos = new Point(e.Location.X - (int)(HitCtrl.UIx * LayoutSize), e.Location.Y - (int)(HitCtrl.UIy * LayoutSize));

        //                        int portIndex = GetPortClick(HitCtrl, TransPos.X, TransPos.Y);
        //                        if (portIndex > -1)
        //                        {
        //                            //Check
        //                            Port depositPort = HitCtrl.GetPort(portIndex);

        //                            string Error = "";
        //                            if (depositPort != null)
        //                            {

        //                                if (!depositPort.IsOutput && connectPort.IsOutput)
        //                                {
        //                                    HitCtrl.Connect(depositPort.Index, Node, connectPort.Index, ModifierKeys.ToString(), out Error, Preferences.CheckCycles);
        //                                }
        //                                else
        //                                {
        //                                    if (depositPort.IsOutput && !connectPort.IsOutput)
        //                                    {
        //                                        Node.Connect(connectPort.Index, HitCtrl, depositPort.Index, ModifierKeys.ToString(), out Error, Preferences.CheckCycles);
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                //plug in new Port
        //                                if (HitCtrl.GetPortTypes().Contains(connectPort.NodeElementType))
        //                                {
        //                                    if (connectPort.IsOutput)
        //                                    {

        //                                        DialogResult result = newPortForm.ShowDialog(connectPort.Name, HitCtrl, connectPort);
        //                                        if (result == DialogResult.OK)
        //                                        {
        //                                            depositPort = HitCtrl.NewPort(newPortForm.PortName, newPortForm.PortType, true, newPortForm.CustomParams, newPortForm.TypeMetaData);
        //                                            if (depositPort != null)
        //                                            {
        //                                                HitCtrl.Connect(depositPort.Index, Node, connectPort.Index, ModifierKeys.ToString(), out Error, Preferences.CheckCycles);
        //                                            }
        //                                        }
        //                                    }
        //                                    else
        //                                    {
        //                                        DialogResult result = newPortForm.ShowDialog(connectPort.Name, HitCtrl, connectPort);
        //                                        if (result == DialogResult.OK)
        //                                        {
        //                                            depositPort = HitCtrl.NewPort(newPortForm.PortName, newPortForm.PortType, false, newPortForm.CustomParams, newPortForm.TypeMetaData);
        //                                            if (depositPort != null)
        //                                            {
        //                                                Node.Connect(connectPort.Index, HitCtrl, depositPort.Index, ModifierKeys.ToString(), out Error, Preferences.CheckCycles);
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    log.AddLog(HitCtrl.Name + " disallow adding ports of type " + connectPort.NodeElementType, 15, 2);
        //                                }
        //                            }                                       

        //                            if (Error != "")
        //                            {
        //                                log.AddLog(Error, 15, 2);
        //                            }
        //                        }
        //                    }
        //                    else //Expose
        //                    {
        //                        CompoundPortPad pad = GetHitPad(e.Location);

        //                        if (pad != null)
        //                        {
        //                            if (CurConnection >= 1000)
        //                            {
        //                                if (pad.righty)
        //                                {
        //                                    pad.ExposePort(Node.GetPort(CurConnection));
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (!pad.righty)
        //                                {
        //                                    pad.ExposePort(Node.GetPort(CurConnection));
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //                overlay.ConnectArrow = new Point[0];
        //                CurConnection = -1;
        //                Invalidate();
        //            }
        //            else
        //            {
        //                if (!HasMoved) // Select
        //                {
        //                    if (HitDisplay(Node, e.Location))
        //                    {
        //                        switch (Node.DisplayState)
        //                        {
        //                            case NodeState.Normal:
        //                                Node.DisplayState = NodeState.Minimal;
        //                                break;
        //                            case NodeState.Minimal:
        //                                Node.DisplayState = NodeState.Collapsed;
        //                                break;
        //                            default:
        //                                Node.DisplayState = NodeState.Normal;
        //                                break;
        //                        }
        //                        Invalidate();
        //                    }
        //                    else
        //                    {

        //                        if (doubleClicked && Node is Compound)
        //                        {

        //                            Manager.EnterCompound(Node as Compound);
        //                            ChangeFocus(true);

        //                            Frame(Manager.CurCompound.Nodes);
        //                        }
        //                        else
        //                        {
        //                            Selection.Modify(Node, ModifierKeys);
        //                        }

        //                        OnLinkSelectionChanged(new LinkSelectionChangedEventArgs(null));
        //                        OnSelectionChanged(new SelectionChangedEventArgs(Selection.Selection));
        //                        Invalidate();

        //                    }
        //                }
        //            }
        //        break;

        //            case MouseButtons.Middle:
        //                //Branch select
        //        List<Node> depend = Manager.GetBranch(Node);

        //        Selection.Modify(depend, ModifierKeys);

        //        OnLinkSelectionChanged(new LinkSelectionChangedEventArgs(null));
        //        OnSelectionChanged(new SelectionChangedEventArgs(Selection.Selection));
        //        Invalidate();

        //        break;

        //        case MouseButtons.Right:  // ContextMenu
        //            bool hitPort = false;

        //            if (!Node.Selected)
        //            {
        //                Selection.Modify(Node, ModifierKeys);
        //                Invalidate();
        //            }

        //            if (Node.DynamicInputs || Node.DynamicOutputs)
        //            {
        //                Point TransPos = new Point(e.Location.X - (int)(Node.UIx * LayoutSize), e.Location.Y - (int)(Node.UIy * LayoutSize));
        //                int click = GetPortClick(Node, TransPos.X, TransPos.Y);

        //                if (click != -1)
        //                {
        //                    Port clickedPort = Node.GetPort(click);
        //                    foreach (ToolStripItem item in customPortMenuStrip.Items)
        //                    {
        //                        item.Visible = false;
        //                    }

        //                    if (clickedPort != null && !clickedPort.Default)
        //                    {
        //                        hitPort = true;

        //                        deletePortToolStripMenuItem.Visible = true;
        //                        deletePortToolStripMenuItem.Tag = clickedPort;
        //                        customPortMenuStrip.Show(PointToScreen(e.Location));
        //                    }
        //                    else
        //                    {
        //                        foreach (ToolStripItem item in customPortMenuStrip.Items)
        //                        {
        //                            if (item != deletePortToolStripMenuItem)
        //                            {
        //                                PortContextTag tag = item.Tag as PortContextTag;
        //                                if (tag != null)
        //                                {
        //                                    if (tag.isContextConsistent(clickedPort))
        //                                    {
        //                                        item.Visible = true;
        //                                        hitPort = true;
        //                                        tag.port = clickedPort;
        //                                    }
        //                                }
        //                            }
        //                        }

        //                        if (hitPort)
        //                        {
        //                            customPortMenuStrip.Show(PointToScreen(e.Location));
        //                        }
        //                    }
        //                }
        //            }

        //            if (!hitPort)
        //            {
        //                ShowNodeContextMenu(Node, PointToScreen(e.Location));
        //            }
        //            break;
        //        }

        //        hitNode = null;
        //    }
        //    IsDragging = false;
        //    HasMoved = false;
        //}

        void Node_MouseUp(object sender, MouseEventArgs e)
        {
            if (hitNode != null)
            {
                Node Node = sender as Node;
                bool doubleClicked = (e.Button == MouseButtons.Left && WasDoubleClicked());

                switch (e.Button)
                {
                    case MouseButtons.Left:

                        if (CurConnection != -1)
                        {
                            Port connectPort = connectPort = Node.GetPort(CurConnection);

                            if (doubleClicked)
                            {
                                PortClickEvent(Node, new PortClickEventArgs(connectPort.Index, connectPort.IsOutput));
                            }
                            else  // Connecting
                            {
                                Node HitCtrl = GetHitNode(e.Location);
                                if (HitCtrl != null)
                                {
                                    Point TransPos = new Point(e.Location.X - (int)(HitCtrl.UIx * LayoutSize), e.Location.Y - (int)(HitCtrl.UIy * LayoutSize));
                                    int portIndex = GetPortClick(HitCtrl, TransPos.X, TransPos.Y);
                                    if (portIndex > -1)
                                    {
                                        //Check
                                        Port depositPort = HitCtrl.GetPort(portIndex);
                                        string Error = "";
                                        if (depositPort != null)
                                        {

                                            if (!depositPort.IsOutput && connectPort.IsOutput)
                                            {
                                                NodalDirector.Connect(HitCtrl.FullName, depositPort.FullName, Node.FullName, connectPort.FullName, ModifierKeys.ToString());
                                            }
                                            else
                                            {
                                                if (depositPort.IsOutput && !connectPort.IsOutput)
                                                {
                                                    NodalDirector.Connect(Node.FullName, connectPort.FullName, HitCtrl.FullName, depositPort.FullName, ModifierKeys.ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //plug in new Port
                                            if (HitCtrl.GetPortTypes().Contains(connectPort.NodeElementType))
                                            {
                                                if (connectPort.IsOutput)
                                                {

                                                    DialogResult result = newPortForm.ShowDialog(connectPort.Name, HitCtrl, connectPort);
                                                    if (result == DialogResult.OK)
                                                    {
                                                        depositPort = HitCtrl.NewPort(newPortForm.PortName, newPortForm.PortType, true, newPortForm.CustomParams, newPortForm.TypeMetaData);
                                                        if (depositPort != null)
                                                        {
                                                            NodalDirector.Connect(HitCtrl.FullName, depositPort.FullName, Node.FullName, connectPort.FullName, ModifierKeys.ToString());
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    DialogResult result = newPortForm.ShowDialog(connectPort.Name, HitCtrl, connectPort);
                                                    if (result == DialogResult.OK)
                                                    {
                                                        depositPort = HitCtrl.NewPort(newPortForm.PortName, newPortForm.PortType, false, newPortForm.CustomParams, newPortForm.TypeMetaData);
                                                        if (depositPort != null)
                                                        {
                                                            NodalDirector.Connect(Node.FullName, connectPort.FullName, HitCtrl.FullName, depositPort.FullName, ModifierKeys.ToString());
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                log.AddLog(HitCtrl.Name + " disallow adding ports of type " + connectPort.NodeElementType, 15, 2);
                                            }
                                        }

                                        if (Error != "")
                                        {
                                            log.AddLog(Error, 15, 2);
                                        }
                                    }
                                }
                                else //Expose
                                {
                                    CompoundPortPad pad = GetHitPad(e.Location);
                                    if (pad != null)
                                    {
                                        if (CurConnection >= 1000)
                                        {
                                            if (pad.righty)
                                            {
                                                pad.ExposePort(Node.GetPort(CurConnection));
                                            }
                                        }
                                        else
                                        {
                                            if (!pad.righty)
                                            {
                                                pad.ExposePort(Node.GetPort(CurConnection));
                                            }
                                        }
                                    }
                                }
                            }
                            overlay.ConnectArrow = new Point[0];
                            CurConnection = -1;
                            Invalidate();
                        }
                        else
                        {
                            if (!HasMoved) // Select
                            {
                                if (HitDisplay(Node, e.Location))
                                {
                                    switch (Node.DisplayState)
                                    {
                                        case NodeState.Normal:
                                            Node.DisplayState = NodeState.Minimal;
                                            break;
                                        case NodeState.Minimal:
                                            Node.DisplayState = NodeState.Collapsed;
                                            break;
                                        default:
                                            Node.DisplayState = NodeState.Normal;
                                            break;
                                    }
                                    Invalidate();
                                }
                                else
                                {

                                    if (doubleClicked && Node is Compound)
                                    {

                                        Manager.EnterCompound(Node as Compound);
                                        ChangeFocus(true);

                                        Frame(Manager.CurCompound.Nodes);
                                    }
                                    else
                                    {
                                        Selection.Modify(Node, ModifierKeys);
                                    }

                                    OnLinkSelectionChanged(new LinkSelectionChangedEventArgs(null));
                                    OnSelectionChanged(new SelectionChangedEventArgs(Selection.Selection));
                                    Invalidate();

                                }
                            }
                        }
                        break;

                    case MouseButtons.Middle:
                        //Branch select
                        List<Node> depend = Manager.GetBranch(Node);

                        Selection.Modify(depend, ModifierKeys);

                        OnLinkSelectionChanged(new LinkSelectionChangedEventArgs(null));
                        OnSelectionChanged(new SelectionChangedEventArgs(Selection.Selection));
                        Invalidate();

                        break;

                    case MouseButtons.Right:  // ContextMenu
                        bool hitPort = false;

                        if (!Node.Selected)
                        {
                            Selection.Modify(Node, ModifierKeys);
                            Invalidate();
                        }

                        if (Node.DynamicInputs || Node.DynamicOutputs)
                        {
                            Point TransPos = new Point(e.Location.X - (int)(Node.UIx * LayoutSize), e.Location.Y - (int)(Node.UIy * LayoutSize));
                            int click = GetPortClick(Node, TransPos.X, TransPos.Y);

                            if (click != -1)
                            {
                                Port clickedPort = Node.GetPort(click);
                                foreach (ToolStripItem item in customPortMenuStrip.Items)
                                {
                                    item.Visible = false;
                                }

                                if (clickedPort != null && !clickedPort.Default)
                                {
                                    hitPort = true;

                                    deletePortToolStripMenuItem.Visible = true;
                                    deletePortToolStripMenuItem.Tag = clickedPort;
                                    customPortMenuStrip.Show(PointToScreen(e.Location));
                                }
                                else
                                {
                                    foreach (ToolStripItem item in customPortMenuStrip.Items)
                                    {
                                        if (item != deletePortToolStripMenuItem)
                                        {
                                            PortContextTag tag = item.Tag as PortContextTag;
                                            if (tag != null)
                                            {
                                                if (tag.isContextConsistent(clickedPort))
                                                {
                                                    item.Visible = true;
                                                    hitPort = true;
                                                    tag.port = clickedPort;
                                                }
                                            }
                                        }
                                    }

                                    if (hitPort)
                                    {
                                        customPortMenuStrip.Show(PointToScreen(e.Location));
                                    }
                                }
                            }
                        }

                        if (!hitPort)
                        {
                            ShowNodeContextMenu(Node, PointToScreen(e.Location));
                        }
                        break;
                }

                hitNode = null;
            }
            IsDragging = false;
            HasMoved = false;
        }

        public void ShowNodeContextMenu(Node inNode, Point point)
        {
            nodeMenuStrip.Tag = inNode;
            List<Node> selectedNodes = Selection.GetSelectedNodes();
            foreach (ToolStripItem item in nodeMenuStrip.Items)
            {
                NodeContextTag tag = item.Tag as NodeContextTag;
                if (tag != null)
                {
                    tag.nodes = selectedNodes;
                    if (tag.isContextConsistent())
                    {
                        if (inNode is Compound)
                        {
                            item.Visible = tag.isCompoundConsistent;
                        }
                        else
                        {
                            item.Visible = tag.isNodeConsistent;
                        }
                    }
                    else
                    {
                        item.Visible = false;
                    }
                }
            }
            nodeMenuStrip.Show(point);
        }

        private bool HitDisplay(Node Node, Point point)
        {
            return (point.X < ((18 + Node.UIx) * LayoutSize)) && (point.Y < ((18 + Node.UIy) * LayoutSize));
        }

        void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (hitNode != null)
            {
                Node Node = sender as Node;
                Point Position = PointToScreen(e.Location);
                Point parentPos = Parent.PointToClient(Position);

                if (CurConnection != -1) // Is Linking
                {
                    if (overlay.ConnectArrow.Length != 2)
                    {
                        return;
                    }

                    if (CurConnection >= 1000)
                    {
                        overlay.ConnectArrow[1] = e.Location;
                    }
                    else
                    {
                        overlay.ConnectArrow[0] = e.Location;
                    }

                    //Pulling links outside the viewport

                    Position = Location;
                    if (parentPos.X < 0)
                    {
                        Position.X = Math.Min(0, Location.X + 1);
                    }
                    else if (parentPos.X > Parent.Width)
                    {
                        Position.X = Math.Max(Parent.Width - Width, Location.X - 1);
                    }

                    if (parentPos.Y < 0)
                    {
                        Position.Y = Math.Min(0, Location.Y + 1);
                    }
                    else if (parentPos.Y > Parent.Height)
                    {
                        Position.Y = Math.Max(Parent.Height - Height, Location.Y - 1);
                    }

                    Location = Position;
                    Invalidate();
                }

                if (IsDragging)
                {
                    Point Translation = new Point(Position.X - HitPoint.X, Position.Y - HitPoint.Y);
                    if (!HasMoved && (Math.Abs(Translation.X) + Math.Abs(Translation.Y) > 3))
                    {
                        HasMoved = true;
                        if (!Node.Selected)
                        {
                            switch (ModifierKeys)
                            {
                                case Keys.Shift:
                                    Selection.AddToSelection(Node);
                                    break;

                                case Keys.Control:
                                    Selection.ToggleSelection(Node);
                                    break;
                                default:
                                    Selection.Select(Node);
                                    break;
                            }

                            OnSelectionChanged(new SelectionChangedEventArgs(Selection.Selection));
                        }
                    }

                    if (HasMoved)
                    {
                        List<Node> selNodes = Selection.GetSelectedNodes();

                        foreach (Node NUctrl in selNodes)
                        {

                            PointF OldPos = new PointF(NUctrl.UIx, NUctrl.UIy);
                            OldPos.X += (float)(Translation.X / LayoutSize);
                            OldPos.Y += (float)(Translation.Y / LayoutSize);

                            NUctrl.UIx = OldPos.X;
                            NUctrl.UIy = OldPos.Y;
                        }

                        ResizeLayout();

                        HitPoint = Position;
                        Invalidate();
                    }
                }
            }

        }

        void Node_MouseDown(object sender, MouseEventArgs e)
        {
            Node Node = sender as Node;

            if (e.Button == MouseButtons.Left)
            {
                if (!IsDragging)
                {
                    Point TransPos = new Point(e.Location.X - (int)(Node.UIx * LayoutSize), e.Location.Y - (int)(Node.UIy * LayoutSize));

                    int PortClick = GetPortClick(Node, TransPos.X, TransPos.Y);
                    Port port = Node.GetPort(PortClick);

                    if (PortClick >= 0)
                    {
                        if (port != null)
                        {
                            CurConnection = PortClick;
                            overlay.ConnectPen = GetPen(port.NodeElementType);
                            if (PortClick >= 1000)
                            {
                                overlay.ConnectArrow = new Point[] { GetPortLocation(Node, port.Index + 1000), e.Location };
                            }
                            else
                            {
                                overlay.ConnectArrow = new Point[] { e.Location, GetPortLocation(Node, port.Index) };
                            }
                        }
                        else //new Port
                        {
                            if (PortClick >= 1000)
                            {
                                DialogResult result = newPortForm.ShowDialog("new_port", Node);
                                if (result == DialogResult.OK)
                                {
                                    Node.NewPort(newPortForm.PortName, newPortForm.PortType, true, newPortForm.CustomParams, newPortForm.TypeMetaData);
                                    Invalidate();
                                }
                            }
                            else
                            {
                                DialogResult result = newPortForm.ShowDialog("new_port", Node);
                                if (result == DialogResult.OK)
                                {
                                    Node.NewPort(newPortForm.PortName, newPortForm.PortType, false, newPortForm.CustomParams, newPortForm.TypeMetaData);
                                    Invalidate();
                                }
                            }
                        }
                    }
                    else
                    {
                        HitPoint = PointToScreen(e.Location);
                        IsDragging = true;
                    }
                }
            }
        }

        private Point GetPortLocation(Node inNode, int realPortIndex)
        {
            bool isOutput = false;
            List<Port> ports = null;

            if (realPortIndex >= 999)
            {
                realPortIndex -= 1000;
                ports = inNode.Outputs;
                isOutput = true;
            }
            else
            {
                ports = inNode.Inputs;
            }

            Point PortLoc = new Point();

            Port port = ports[realPortIndex];

            if (port.Owner.IsIn(Manager.CurCompound))
            {
                Compound parent = inNode.Parent;
                while (parent != null && parent != Manager.CurCompound)
                {
                    port = parent.GetPortFromNode(port);
                    inNode = parent;
                    parent = parent.Parent;
                }

                if (!isOutput)
                {
                    PortLoc = new Point((int)(3.0 * LayoutSize), (int)((PortHeight / 4) + (inNode.UILabelY * 2 + 3) * LayoutSize + (double)port.DisplayIndex * PortHeight));
                }
                else
                {
                    PortLoc = new Point((int)((inNode.UIWidth - 3.0) * LayoutSize), (int)((PortHeight / 4) + (inNode.UILabelY * 2 + 3) * LayoutSize + (double)(port.DisplayIndex + ((inNode.AllowAddPorts && inNode.DynamicInputs) ? 1 : 0) + (port.DisplayIndex >= 0 ? inNode.InputsCount : 0)) * PortHeight));
                }

                PortLoc.Offset(new Point((int)(inNode.UIx * LayoutSize), (int)(inNode.UIy * LayoutSize)));
            }
            else
            {
                if (!isOutput)
                {
                    PortLoc = GetPortLocation(Outputs, Outputs.GetPort(port).Index);
                }
                else
                {
                    int i = Inputs.GetPort(port).Index + 1000;
                    PortLoc = GetPortLocation(Inputs, Inputs.GetPort(port).Index + 1000);
                }
            }

            return PortLoc;
        }

        private Point GetPortLocation(CompoundPortPad inPad, int portRealIndex)
        {
            bool isOutput = false;

            if (portRealIndex >= 1000)
            {
                portRealIndex -= 1000;
                isOutput = true;
            }

            Point PortLoc;
            Port port = inPad.Ports[portRealIndex];

            int custDisplayIndex = 0;
            foreach (Port curPort in inPad.Ports)
            {
                if (curPort.Index == portRealIndex)
                {
                    break;
                }

                if (curPort.Visible)
                {
                    custDisplayIndex++;
                }
            }

            if (!isOutput)
            {
                PortLoc = new Point(inPad.Visible ? 1 : inPad.Width, (int)(25 + custDisplayIndex * 20));
            }
            else
            {
                PortLoc = new Point(inPad.Visible ? inPad.Width - 1 : 0, (int)(25 + custDisplayIndex * 20));
            }

            return PointToClient(inPad.PointToScreen(PortLoc));
        }

        private Node GetHitNode(Point point)
        {
            List<Node> curNodes = Manager.CurCompound.Nodes;
            for (int i = curNodes.Count-1; i >= 0; i--)
            {
                Node curNode = curNodes[i];
                if (new Rectangle(new Point((int)(curNode.UIx * LayoutSize), (int)(curNode.UIy * LayoutSize)), new Size((int)(curNode.UIWidth * LayoutSize), (int)(curNode.UIHeight * LayoutSize))).Contains(point))
                {
                    return curNode;
                }
            }

            return null;
        }

        private CompoundPortPad GetHitPad(Point point)
        {
            if (Preferences.ShowRootPorts || Manager.CurCompound != Manager.Root)
            {
                Point Pos = PointToClient(Parent.PointToScreen(Inputs.Location));

                if (new Rectangle(Pos, Inputs.Size).Contains(point))
                {
                    return Inputs;
                }
                else
                {
                    Pos = PointToClient(Parent.PointToScreen(Outputs.Location));

                    if (new Rectangle(Pos, Outputs.Size).Contains(point))
                    {
                        return Outputs;
                    }
                }
            }

            return null;
        }

        private int GetPortClick(Node inNode, int X, int Y)
        {
            int portIndex = -1;

            if (X < (35 * LayoutSize) || X > (inNode.UIWidth - 35) * LayoutSize)
            {
                double rawIndex = (Y - LayoutSize * (inNode.UILabelY * 2)) / PortHeight;
                int InputsCount = inNode.InputsCount + ((inNode.AllowAddPorts && inNode.DynamicInputs) ? 1 : 0);
                int OutputsCount = inNode.OutputsCount + ((inNode.AllowAddPorts && inNode.DynamicOutputs) ? 1 : 0);

                if (rawIndex > 0)
                {
                    if (rawIndex < InputsCount)
                    {
                        portIndex = (int)rawIndex;
                    }
                    else
                    {
                        rawIndex -= InputsCount;

                        if (rawIndex < OutputsCount)
                        {
                            portIndex = 1000 + (int)rawIndex;
                        }
                    }
                }
            }

            return portIndex;
        }

        public void ChangeFocus(bool ResetSelection)
        {
            RefreshBreadCrumbs();

            if (Selection == null)
            {
                Selection = new SelectionManager(Manager);
            }
            else if (ResetSelection)
            {
                Selection.DeselectAll();
                Selection.DeselectLinks();
            }

            Inputs.SetPorts(Manager.CurCompound);
            Outputs.SetPorts(Manager.CurCompound);

            if ((Manager.CurCompound == Manager.Root && Preferences.ShowRootPorts) || (Manager.CurCompound != Manager.Root && Preferences.ShowCompoundPorts))
            {
                Inputs.Visible = true;
                Outputs.Visible = true;
            }
            else
            {
                Inputs.Visible = false;
                Outputs.Visible = false;
            }
            
            RefreshNodeSizes();
            IsInitialised = true;
            PerformMyLayout(new Point(1,1));
            OnFocusChanged(new FocusChangedEventArgs(Manager.BreadCrumbs));
        }

        private void RefreshBreadCrumbs()
        {
            BreadCrumbs.Items.Clear();
            int ButtonCounter = 0;
            if (Manager.BreadCrumbs.Count > 1)
            {
                foreach (Compound curCompound in Manager.BreadCrumbs)
                {

                    ToolStripLabel newButton = new ToolStripLabel();
                    newButton.BackColor = Color.FromArgb(255, 80, 80, 80);
                    newButton.AutoSize = true;
                    newButton.Text = curCompound.FullName;
                    newButton.Tag = ButtonCounter;
                    newButton.Click += new EventHandler(JumpCompoundButton_Click);
                    BreadCrumbs.Items.Add(newButton);
                    if (ButtonCounter == Manager.BreadCrumbs.Count - 1)
                    {
                        newButton.Enabled = false;
                    }

                    //ShortCuts for other compounds with same Name
                    List<string> modifiers = new List<string>() { "Left", "Right", "Top", "Bottom", "Front", "Back", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "_" };
                    string baseName = curCompound.Name;
                    foreach (string modifier in modifiers)
                    {
                        baseName = baseName.Replace(modifier, ".*");
                    }

                    if (!baseName.StartsWith(".*"))
                    {
                        baseName = ".*" + baseName;
                    }

                    if (!baseName.EndsWith(".*"))
                    {
                        baseName = baseName + ".*";
                    }

                    if (baseName.Replace(".*", "") == "")
                    {
                        baseName = curCompound.Name;
                    }

                    List<Node> otherComps = Manager.GetNodes(baseName, Manager.Root, true, false, true, true);
                    if (otherComps.Count > 1)
                    {
                        ToolStripDropDownButton dropD = new ToolStripDropDownButton();
                        ToolStripItem[] otherCompsItems = new System.Windows.Forms.ToolStripItem[otherComps.Count];
                        dropD.Text = "Related compounds v";
                        dropD.DisplayStyle = ToolStripItemDisplayStyle.Text;
                        dropD.AutoSize = true;

                        foreach (Node comp in otherComps)
                        {
                            if (comp.FullName != curCompound.FullName)
                            {
                                ToolStripMenuItem bt = new ToolStripMenuItem(comp.FullName, null, JumpCompoundButton_Click, comp.FullName);
                                bt.Tag = comp as Compound;
                                dropD.DropDownItems.Add(bt);
                            }
                        }

                        BreadCrumbs.Items.Add(dropD);
                    }

                    ButtonCounter++;
                }

                BreadCrumbs.Visible = true;
            }
            else
            {
                BreadCrumbs.Visible = false;
            }
        }

        void JumpCompoundButton_Click(object sender, EventArgs e)
        {
            Compound curComp = Manager.CurCompound;

            object tag = (sender as ToolStripItem).Tag;

            if (tag is int)
            {
                int index = (int)(sender as ToolStripItem).Tag;
                curComp = Manager.BreadCrumbs[index + 1];
                Manager.JumpCompound(index);
            }
            else
            {
                Compound comp = (Compound)(sender as ToolStripItem).Tag;
                int index = Manager.GetBreadCrumbsIndex(comp);
                if (index == -1)
                {
                    curComp = null;
                }
                else
                {
                    curComp = Manager.BreadCrumbs[index + 1];
                }
                Manager.JumpCompound(comp);
            }

            ChangeFocus(true);

            if (curComp != null)
            {
                Frame(new List<Node> { curComp });
            }
            //Frame(Manager.CurCompound.Nodes);
        }

        private Link GetHitLink(Point point)
        {
            Link hitLink = null;
            bool hovered = false;
            bool hoverChange = false;

            foreach (KeyValuePair<Link, GraphicsPath> item in paths)
            {
                //Exclude hidden links and link in process of reconnection
                if (item.Value == null || !item.Key.Selectable)
                {
                    continue;
                }

                if (!hovered && item.Value.GetBounds().Contains(point) && item.Value.IsOutlineVisible(point, widenPen))
                {
                    if(!hoverChange && !item.Key.isHovered)
                    {
                        item.Key.isHovered = true;
                        hoverChange = true;
                    }
                    hitLink = item.Key;
                    hovered = true;
                }
                else if (item.Key.isHovered)
                {
                    item.Key.isHovered = false;
                    hoverChange = true;
                }
            }

            if (hoverChange)
                Invalidate();

            return hitLink;
        }


        // SIZING =======================================================================

        void Parent_Resize(object sender, EventArgs e)
        {
            ResizeLayout();
            Inputs.Invalidate();
            Outputs.Invalidate();
        }

        public void SetSize(double NewSize)
        {
            SetSize(NewSize,new Point(1,1));
        }

        public void SetSize(double NewSize, Point NewLoc)
        {
            if (NewSize < Preferences.MinimumZoom)
            {
                NewSize = Preferences.MinimumZoom;
            }
            else
            {
                if (NewSize > Preferences.MaximumZoom)
                {
                    NewSize = Preferences.MaximumZoom;
                }
            }

            mLayoutSize = NewSize;
            PerformMyLayout(NewLoc);
        }

        private void ResizeLayout()
        {
            if (Parent != null && IsInitialised)
            {
                LeftStart = Inputs.Visible ? Inputs.Width : 0;
                RightStart = Outputs.Visible ? Outputs.Width : 0;

                //Set the minimum Size to Parent
                BaseWidth = (int)((Parent.Width - RightStart - LeftStart) / LayoutSize);
                BaseHeight = (int)(Parent.Height / LayoutSize);

                int NODEMARGIN = 500;

                //Nodes
                int LeftTrans = 0;
                int TopTrans = 0;

                int RightTrans = 0;
                int BottomTrans = 0;

                foreach (Node Node in Manager.CurCompound.Nodes)
                {
                    if ((Node.UIx - 10) < LeftTrans)
                    {
                        LeftTrans = (int)(Node.UIx - 10);
                    }
                    else if (Node.UIWidth + Node.UIx + NODEMARGIN > RightTrans)
                    {
                        RightTrans = (int)(Node.UIWidth + Node.UIx + NODEMARGIN);
                    }

                    if ((Node.UIy - 10) < TopTrans)
                    {
                        TopTrans = (int)(Node.UIy - 10);
                    }
                    else if (Node.UIHeight + Node.UIy + NODEMARGIN > BottomTrans)
                    {
                        BottomTrans = (int)(Node.UIHeight + Node.UIy + NODEMARGIN);
                    }
                }

                if (LeftTrans < 0 || TopTrans < 0 || RightTrans > 0 || BottomTrans > 0)
                {
                    if (LeftTrans < 0 || TopTrans < 0)
                    {
                        //translate nodes
                        foreach (Node node in Manager.CurCompound.Nodes)
                        {
                            node.UIx = (int)(node.UIx - LeftTrans);
                            node.UIy = (int)(node.UIy - TopTrans);
                        }
                    }

                    BaseWidth = Math.Max(RightTrans - LeftTrans, BaseWidth);
                    BaseHeight = Math.Max(BottomTrans - TopTrans, BaseHeight);
                }

                this.Size = new Size((int)(BaseWidth * mLayoutSize), (int)(BaseHeight * mLayoutSize));

                //Re-frame if necessary
                Frame(null);
            }

            Invalidate();
        }

        //LAYOUT =========================================================================

        public void PerformMyLayout(Point NewPos)
        {
            if (IsInitialised && Manager.CurCompound != null)
            {
                SuspendLayout();
                this.Size = new Size((int)(BaseWidth * mLayoutSize), (int)(BaseHeight * mLayoutSize));
                ResizeLayout();

                if (NewPos.X != 1)
                {
                    Location = NewPos;
                    Frame(null);
                }

                this.ResumeLayout();

                Invalidate();
            }
        }

        public void Frame()
        {
            Frame(Selection.GetSelectedNodes());
        }

        public void Frame(List<Node> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                Point MinLoc = new Point(Math.Min(RightStart, Location.X), Math.Min(0, Location.Y));
                Location = new Point(Math.Max(MinLoc.X, Parent.Width - LeftStart - Width), Math.Max(MinLoc.Y, Parent.Height - Height));
            }
            else
            {
                PointF MinLoc = new PointF(nodes[0].UIx, nodes[0].UIy);
                PointF MaxLoc = new PointF(nodes[0].UIx + nodes[0].UIWidth, nodes[0].UIy + nodes[0].UIHeight);

                for(int nodeCounter = 1; nodeCounter < nodes.Count; nodeCounter++)
                {
                    Node curNode = nodes[nodeCounter];

                    if (curNode.UIx < MinLoc.X)
                    {
                        MinLoc.X = curNode.UIx;
                    }
                    
                    if(curNode.UIx + curNode.UIWidth > MaxLoc.X)
                    {
                        MaxLoc.X = curNode.UIx + curNode.UIWidth;
                    }

                    if (curNode.UIy < MinLoc.Y)
                    {
                        MinLoc.Y = curNode.UIy;
                    }

                    if (curNode.UIy + curNode.UIHeight > MaxLoc.Y)
                    {
                        MaxLoc.Y = curNode.UIy + curNode.UIHeight;
                    }
                }

                Point Center = new Point((int)((MaxLoc.X + 10) / 2.0 + (MinLoc.X - 10) / 2), (int)((MaxLoc.Y + 10) / 2.0 + (MinLoc.Y - 10) / 2));

                double askedWidth = (MaxLoc.X + 10) - (MinLoc.X - 10);
                double askedHeigth = (MaxLoc.Y + 10) - (MinLoc.Y - 10);

                double factor = Math.Min((double)(Parent.Width - LeftStart - RightStart) / askedWidth, (double)Parent.Height / askedHeigth);
                SetSize(factor);

                FocusLayout(Center);

                Frame(null);
            }
        }

        private void FocusLayout(Point Center)
        {
            /*
            int XLoc = (int)(-Center.X * LayoutSize + (Parent.Width - LeftStart - RightStart) / 2.0) + LeftStart;
            int YLoc = (int)(-Center.Y * LayoutSize + Parent.Height / 2.0);

            Location = new Point(XLoc, YLoc);
            */

            XLoc = Center.X;
            YLoc = Center.Y;
        }

        public bool SaveLayout(string inPath)
        {
            SavedLayout layout = GetLayout();

            XmlSerializer ser = new XmlSerializer(typeof(SavedLayout));
            
            FileStream stream = null;
            FileInfo infosFile = new FileInfo(inPath);

            try
            {
                stream = infosFile.Create();
                ser.Serialize(stream, layout);
                stream.Close();

                return true;
            }
            catch (Exception) { if (stream != null) { stream.Close(); } }

            return false;
        }

        public SavedLayout LoadLayout(string inPath)
        {
            SavedLayout result = null;

            XmlSerializer ser = new XmlSerializer(typeof(SavedLayout));

            FileStream stream = null;
            FileInfo infosFile = new FileInfo(inPath);

            try
            {
                stream = infosFile.Open(FileMode.Open, FileAccess.Read);
                result = ser.Deserialize(stream) as SavedLayout;
                stream.Close();

                result.RebuildDictionary();

                return result;
            }
            catch (Exception) { if (stream != null) { stream.Close(); } }

            return null;
        }

        public bool ApplyLayout(SavedLayout layout, bool focus, bool nodesPositions)
        {
            if (focus)
            {
                Compound explored = Manager.GetNode(layout.ExploredCompound) as Compound;
                if (explored != null)
                {
                    Manager.EnterCompound(explored);
                    Location = new Point(layout.LayoutPosition[0], layout.LayoutPosition[1]);
                    LayoutSize = layout.Size;
                }
                else if(Manager.CurCompound.FullName == layout.ExploredCompound)
                {
                    Location = new Point(layout.LayoutPosition[0], layout.LayoutPosition[1]);
                    LayoutSize = layout.Size;
                }
            }

            if(nodesPositions)
            {
                ApplyNodesPositions(layout);
            }

            ChangeFocus(focus);

            return true;
        }

        public void ApplyNodesPositions(SavedLayout layout)
        {
            foreach (string key in layout.NodesPositions.Keys)
            {
                Node node = Manager.GetNode(key);

                if (node != null)
                {
                    node.UIx = layout.NodesPositions[key][0];
                    node.UIy = layout.NodesPositions[key][1];
                }
            }
        }

        public SavedLayout GetLayout()
        {
            return new SavedLayout(Manager.CurCompound.FullName, Location.X, Location.Y, LayoutSize, Manager.Root); 
        }

        // PAINT ==========================================================================

        protected override void OnPaint(PaintEventArgs e)
        {
            if (IsInitialised)
            {
                RefreshNodeSizes();

                e.Graphics.Clear(Preferences.BackgroundColor);

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Paint Grid

                if (this.Preferences.ShowGrid)
                {
                    int Step = (int)(this.Preferences.GridSpacing * this.mLayoutSize);

                    Step = Math.Max(1, Step);

                    int Curx = Step;
                    while (Curx < Width)
                    {
                        e.Graphics.DrawLine(GridPen, Curx, 0, Curx, this.Height);
                        Curx += Step;
                    }

                    int Cury = Step;
                    while (Cury < Height)
                    {
                        e.Graphics.DrawLine(GridPen, 0, Cury, this.Width, Cury);
                        Cury += Step;
                    }
                }

                //Frame
                e.Graphics.DrawRectangle(FramePen, new Rectangle(0, 0, Width - 1, Height - 1));

                List<Node> CurNodes = Manager.CurCompound.Nodes;

                if (Manager.CurCompound != null && CurNodes != null)
                {
                    // === DRAW LINKS ====================================================

                    //External links

                    links = Manager.GetLinks();
                    paths.Clear();
                    foreach(Link link in links)
                    {
                        paths.Add(link, null);
                    }
                    Port foundPort = null;
                    

                    foreach (Link Link in links)
                    {
                        GraphicsPath path = null;
                        if (LinkIsShowing(Link))
                        {
                            Node SourceNode = Link.Source.Owner;
                            Node TargetNode = Link.Target.Owner;

                            if (NodeIsShowing(SourceNode.NodeElementType) && NodeIsShowing(TargetNode.NodeElementType))
                            {
                                if (SourceNode.IsIn(Manager.CurCompound))
                                {
                                    if (Link.Target.Owner.IsIn(Manager.CurCompound))
                                    {
                                        path = overlay.DrawArrow(e.Graphics, GetPen(Link.NodeElementType), GetBrush(Link.NodeElementType), GetPortLocation(Link.Source.Owner, Link.Source.Index + 1000), GetPortLocation(Link.Target.Owner, Link.Target.Index), LayoutSize, Link.Selected, Link.State, Link.Target.Owner == Link.Source.Owner, 80, Link.isHovered);
                                        if (paths.ContainsKey(Link))
                                        {
                                            paths[Link] = path;
                                        }
                                        //else
                                        //{
                                        //    paths.Add(Link, path);
                                        //}
                                        //overlay.DrawPolygon(e.Graphics, lightFontBrush, Link.polygon);
                                    }
                                    else
                                    {
                                        foundPort = Outputs.GetPort(Link.Source);
                                        if (foundPort != null)
                                        {
                                            path = overlay.DrawArrow(e.Graphics, GetPen(Link.NodeElementType), GetBrush(Link.NodeElementType), GetPortLocation(Link.Source.Owner, Link.Source.Index + 1000), GetPortLocation(Outputs, foundPort.Index), LayoutSize, Link.Selected, Link.State, Link.Target.Owner == Link.Source.Owner, 80, Link.isHovered);
                                            if (paths.ContainsKey(Link))
                                            {
                                                paths[Link] = path;
                                            }
                                            //else
                                            //{
                                            //    paths.Add(Link, path);
                                            //}
                                            //paths.Add(Link, overlay.path);
                                            //overlay.DrawPolygon(e.Graphics, lightFontBrush, Link.polygon);
                                        }
                                        else
                                        {
                                            NodalDirector.Error("Cannot get source " + Link.Source.Name + " on " + Outputs.node.Name + " !!");
                                        }
                                    }
                                }
                                else
                                {
                                    if (Link.Target.Owner.IsIn(Manager.CurCompound))
                                    {
                                        foundPort = Inputs.GetPort(Link.Target);
                                        if (foundPort != null)
                                        {
                                            path = overlay.DrawArrow(e.Graphics, GetPen(Link.NodeElementType), GetBrush(Link.NodeElementType), GetPortLocation(Inputs, foundPort.Index + 1000), GetPortLocation(Link.Target.Owner, Link.Target.Index), LayoutSize, Link.Selected, Link.State, Link.Target.Owner == Link.Source.Owner, 80, Link.isHovered);
                                            if (paths.ContainsKey(Link))
                                            {
                                                paths[Link] = path;
                                            }
                                            //else
                                            //{
                                            //    paths.Add(Link, path);
                                            //}
                                            //overlay.DrawPolygon(e.Graphics, lightFontBrush, Link.polygon);
                                        }
                                        else
                                        {
                                            NodalDirector.Error("Cannot get Target " + Link.Target.Name + " on " + Inputs.node.Name + " !!");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //Draw Nodes
                    if (CurNodes.Count > 0)
                    {
                        lightFont = new Font(lightFont.Name, (float)(Preferences.NodePortsFont.Size * LayoutSize), lightFont.Style);
                        strongFont = new Font(strongFont.Name, (float)(Preferences.NodeLabelFont.Size * LayoutSize), strongFont.Style);
                        specialFont = new Font(specialFont.Name, (float)(8 * LayoutSize), specialFont.Style);
                        foreach (Node node in CurNodes)
                        {
                            if (NodeIsShowing(node.NodeElementType))
                            {
                                DrawNode(e.Graphics, node, node.UIx, node.UIy, LayoutSize);
                            }
                        }
                    }

                    //Draw Overlay
                    overlay.Draw(e.Graphics);

                    //Logs
                    Point cursor = PointToClient(Cursor.Position);

                    log.DrawLogs(e.Graphics, new PointF(cursor.X, cursor.Y), new Rectangle(LeftStart, 0, Width - RightStart, Height));
                }
            }
        }

        private bool LinkIsShowing(Link inLink)
        {
            string inType = inLink.NodeElementType;
            if (CurConnection2 != -1 && inLink == detachLink)
            {
                return false;
            }
            else if (TypesVisible.ContainsKey(inType))
            {
                return TypesVisible[inType];
            }

            return true;
        }

        private bool NodeIsShowing(string inType)
        {
            if (NodesVisible.ContainsKey(inType))
            {
                return NodesVisible[inType];
            }

            return true;
        }

        public void DrawNode(Graphics graphics, Node Ctrl, float inX, float inY, double inSize)
        {
            Brush toUse = GetBrush(Ctrl);
            DrawSmoothRectangle(graphics, Ctrl.Selected ? FatPen : FramePen, toUse, (int)(inX * inSize), (int)(inY * inSize), (int)(Ctrl.UIWidth * inSize), (int)(Ctrl.UIHeight * inSize), (int)(15 * inSize));

            if (Ctrl.Selected)
            {
                DrawSmoothRectangle(graphics, WhitePen, null, (int)(inX * inSize), (int)(inY * inSize), (int)(Ctrl.UIWidth * inSize), (int)(Ctrl.UIHeight * inSize), (int)(15 * inSize));
            }

            //Custom Icons
            string states = Ctrl.States;
            if (!string.IsNullOrEmpty(states))
            {
                string[] pieces = states.Split(",".ToCharArray());
                string piece;
                int iconX = (int)((inX + Ctrl.UIWidth - 20) * inSize);

                for (int i = pieces.Length - 1; i >= 0; i--)
                {
                    piece = pieces[i].Trim();
                    if (StatesIcons.ContainsKey(piece))
                    {
                        graphics.DrawImage(StatesIcons[piece], new Rectangle(iconX, (int)((inY + 4) * inSize), (int)(12 * inSize), (int)(12 * inSize)));
                        iconX -= 13 * (int)inSize;
                    }
                }
            }

            //Custom Color
            if (Ctrl.CustomColor != Color.Transparent)
            {
                RectangleF rect = new RectangleF((float)((inX + 8) * inSize), (float)((inY + Ctrl.UIHeight - 18) * inSize), (float)(10 * inSize), (float)(10 * inSize));
                graphics.FillEllipse(new SolidBrush(Ctrl.CustomColor), rect);
                graphics.DrawEllipse(FramePen, rect);
            }

            //Label
            graphics.DrawString(Ctrl.FullName, strongFont, strongFontBrush, new PointF((float)((inX + Ctrl.UILabelX) * inSize), (float)((inY + Ctrl.UILabelY / 3) * inSize)));

            //Display Icon
            graphics.DrawImage(mDisplayIcon, new Rectangle((int)((inX + 6) * inSize), (int)((inY + 6) * inSize), (int)(101 * inSize), (int)(11 * inSize)));

            //Inputs
            float Offset = 0f;

            foreach (Port port in Ctrl.Inputs)
            {
                if (port.IsVisible)
                {
                    string name = port.Name;
                    if (name.StartsWith(Ctrl.FullName + "_"))
                    {
                        name = name.Substring(Ctrl.FullName.Length + 1);
                    }

                    graphics.DrawString(name, GetPortFont(port), lightFontBrush, new PointF((float)(6 * inSize + (inX * inSize)), (float)(Offset + inSize * (Ctrl.UILabelY * 2 + inY))));
                    DrawPortPlug(graphics, GetBrush(port.NodeElementType), (int)((inX - 1) * inSize), (int)(Offset + inSize * (Ctrl.UILabelY * 2 + inY) + (PortHeight / 4)), (int)(inSize * 6));
                    Offset += PortHeight;
                }
            }

            //Dynamic input
            if (Ctrl.AllowAddPorts && Ctrl.DynamicInputs && Ctrl.DisplayState != NodeState.Collapsed)
            {
                graphics.DrawString("new port", specialFont, Brushes.DimGray, new PointF((float)(6 * inSize + (inX * inSize)), (float)(Offset + inSize * (Ctrl.UILabelY * 2 + inY))));
                DrawPortPlug(graphics, Brushes.DimGray, (int)((inX - 1) * inSize), (int)(Offset + inSize * (Ctrl.UILabelY * 2 + inY) + (PortHeight / 4)), (int)(inSize * 6));
                Offset += PortHeight;
            }

            //Outputs

            foreach (Port port in Ctrl.Outputs)
            {
                if (port.IsVisible)
                {
                    string name = port.Name;
                    if (name.StartsWith(Ctrl.FullName + "_"))
                    {
                        name = name.Substring(Ctrl.FullName.Length + 1);
                    }

                    Font portFont = GetPortFont(port);

                    SizeF stringSize = graphics.MeasureString(name, portFont);
                    graphics.DrawString(name, portFont, lightFontBrush, new PointF((float)((Ctrl.UIWidth + inX - 8) * inSize - stringSize.Width), (float)(Offset + inSize * (Ctrl.UILabelY * 2 + inY))));
                    DrawPortPlug(graphics, GetBrush(port.NodeElementType), (int)((inX + Ctrl.UIWidth - 6) * inSize), (int)(Offset + inSize * (Ctrl.UILabelY * 2 + inY) + (PortHeight / 4)), (int)(inSize * 6));
                    Offset += PortHeight;
                }
            }

            //Dynamic output
            if (Ctrl.AllowAddPorts && Ctrl.DynamicOutputs && Ctrl.DisplayState != NodeState.Collapsed)
            {
                SizeF stringSize = graphics.MeasureString("new port", specialFont);
                graphics.DrawString("new port", specialFont, Brushes.DimGray, new PointF((float)((Ctrl.UIWidth + inX - 8) * inSize - stringSize.Width), (float)(Offset + inSize * (Ctrl.UILabelY * 2 + inY))));
                DrawPortPlug(graphics, Brushes.DimGray, (int)((inX + Ctrl.UIWidth - 6) * inSize), (int)(Offset + inSize * (Ctrl.UILabelY * 2 + inY) + (PortHeight / 4)), (int)(inSize * 6));
            }
        }

        private Brush GetBrush(Node Ctrl)
        {
            string categ = Ctrl.NodeElementType.Split("_".ToCharArray())[0];

            if (!string.IsNullOrEmpty(Ctrl.NodeElementType) && NodeCategoryBrushes.ContainsKey(categ))
            {
                return (Ctrl.Selected ? NodeCategoryBrushes[categ][1] : NodeCategoryBrushes[categ][0]);
            }

            if (NodeCategoryBrushes.ContainsKey(Ctrl.NativeName))
            {
                return (Ctrl.Selected ? NodeCategoryBrushes[Ctrl.NativeName][1] : NodeCategoryBrushes[Ctrl.NativeName][0]);
            }

            return (Ctrl is Compound ? (Ctrl.Selected ? CompoundSelectedBrush : CompoundBrush) : (Ctrl.Selected ? NodeSelectedBrush : NodeBrush));
        }

        public Brush GetBrush(string categ)
        {
            if (LinkCategoryBrushes.ContainsKey(categ))
            {
                return LinkCategoryBrushes[categ];
            }
            else
            {
                return LinkBrush;
            }
        }

        public Pen GetPen(string categ)
        {
            if (LinkCategoryPens.ContainsKey(categ))
            {
                return LinkCategoryPens[categ];
            }
            else
            {
                return LinkPen;
            }
        }

        public void DrawSmoothRectangle(Graphics graphics,Pen pen, Brush brush, int inX, int inY, int inWidth, int inHeight, int Smoothness)
        {
            if (brush != null)
            {
                graphics.FillPie(brush, inX, inY, 2 * Smoothness, 2 * Smoothness, 180, 90);
                graphics.FillPie(brush, inX + inWidth - 2 * Smoothness - 1, inY, 2 * Smoothness, 2 * Smoothness, -90, 90);
                graphics.FillPie(brush, inX, inY + inHeight - 2 * Smoothness - 1, 2 * Smoothness, 2 * Smoothness, 180, -90);
                graphics.FillPie(brush, inX + inWidth - 2 * Smoothness - 1, inY + inHeight - 2 * Smoothness - 1, 2 * Smoothness, 2 * Smoothness, 0, 90);

                graphics.FillRectangle(brush, inX, inY + Smoothness, inWidth - 1, inHeight - 2 * Smoothness - 1);
                graphics.FillRectangle(brush, inX + Smoothness, inY, inWidth - 2 * Smoothness - 1, inHeight - 1);

            }

            graphics.DrawArc(pen, inX, inY, 2 * Smoothness, 2 * Smoothness, 180, 90);
            graphics.DrawArc(pen, inX + inWidth - 2 * Smoothness - 1, inY, 2 * Smoothness, 2 * Smoothness, -90, 90);
            graphics.DrawArc(pen, inX, inY + inHeight - 2 * Smoothness - 1, 2 * Smoothness, 2 * Smoothness, 180, -90);
            graphics.DrawArc(pen, inX + inWidth - 2 * Smoothness - 1, inY + inHeight - 2 * Smoothness - 1, 2 * Smoothness, 2 * Smoothness, 0, 90);

            graphics.DrawLine(pen, inX, inY + Smoothness, inX, inY + inHeight - Smoothness - 1);
            graphics.DrawLine(pen, inX + inWidth - 1, inY + Smoothness, inX + inWidth - 1, inY + inHeight - Smoothness - 1);
            graphics.DrawLine(pen, inX + Smoothness, inY, inX + inWidth - Smoothness - 1, inY);
            graphics.DrawLine(pen, inX + Smoothness, inY + inHeight - 1, inX + inWidth - Smoothness - 1, inY + inHeight - 1); 
        }

        public void DrawPortPlug(Graphics graphics, Brush brush, int inX, int inY, int Size)
        {
            graphics.FillEllipse(brush, inX, inY, Size, Size);

            graphics.DrawEllipse(FramePen, inX, inY, Size, Size);
            if (Size > 4)
            {
                graphics.DrawEllipse(FramePen, inX + Size / 4, inY + Size / 4, Size / 2, Size / 2);
            }
        }

        /// <summary>
        /// Create brushes used to draw nodes
        /// </summary>
        public void CreateNodesTools()
        {
            //Nodes
            NodeCategoryBrushes = new Dictionary<string, List<Brush>>();
            NodesVisible = new Dictionary<string, bool>();

            foreach (SelectableCategory categ in Preferences.NodeCategories)
            {
                if (!NodeCategoryBrushes.ContainsKey(categ.Name))
                {
                    List<Brush> brushes = new List<Brush>();
                    brushes.Add(new SolidBrush(categ.Color));
                    brushes.Add(new SolidBrush(categ.ColorSelected));

                    NodeCategoryBrushes.Add(categ.Name, brushes);
                }

                if (!NodesVisible.ContainsKey(categ.Name))
                {
                    NodesVisible.Add(categ.Name, categ.Visible);
                }
            }
        }

        /// <summary>
        /// Create brushes and pens used to draw links
        /// </summary>
        public void CreateLinksTools()
        {
            //Links
            LinkCategoryBrushes = new Dictionary<string, Brush>();
            LinkCategoryPens = new Dictionary<string, Pen>();
            TypesVisible = new Dictionary<string, bool>();
            LinkStates = new Dictionary<string, LinkState>();

            foreach (Category categ in Preferences.LinksCategories)
            {
                if (!LinkCategoryBrushes.ContainsKey(categ.Name))
                {
                    Brush brush = new SolidBrush(categ.Color);
                    LinkCategoryBrushes.Add(categ.Name, brush);
                    Pen penFromBrush = new Pen(brush);
                    if (categ.Dashed)
                    {
                        penFromBrush.DashPattern = DASHPATTERN;
                    }
                    LinkCategoryPens.Add(categ.Name, penFromBrush);
                }

                if(!TypesVisible.ContainsKey(categ.Name))
                {
                    TypesVisible.Add(categ.Name, categ.Visible);
                }
            }

            if (!LinkStates.ContainsKey("Default"))
            {
                LinkStates.Add("Default", new LinkState("Default", LinksArrows.None, LinksArrows.SolidArrow));
            }
        }

        /// <summary>
        /// Create brushes and pens used for Layout
        /// </summary>
        private void CreateLayoutTools()
        {
            GridPen = new Pen(Preferences.GridColor);

            LinkBrush = new SolidBrush(Preferences.DefaultLinkColor);
            LinkPen = new Pen(LinkBrush);

            NodeBrush = new SolidBrush(Preferences.DefaultNodeColor);
            NodeSelectedBrush = new SolidBrush(Preferences.DefaultSelectedNodeColor);
            CompoundBrush = new SolidBrush(Preferences.DefaultCompoundColor);
            CompoundSelectedBrush = new SolidBrush(Preferences.DefaultSelectedCompoundColor);
        }

        /// <summary>
        /// Create fonts
        /// </summary>
        private void CreateFonts()
        {
            lightFont = new Font(Preferences.NodePortsFont.Name, (float)(Preferences.NodePortsFont.Size * LayoutSize), Preferences.NodePortsFont.Style);
            strongFont = new Font(Preferences.NodeLabelFont.Name, (float)(Preferences.NodeLabelFont.Size * LayoutSize), Preferences.NodeLabelFont.Style);
            specialFont = new Font("Arial", (float)(8 * LayoutSize), FontStyle.Italic);

            lightFontBrush = new SolidBrush(Preferences.NodePortsFontColor);
            strongFontBrush = new SolidBrush(Preferences.NodeLabelFontColor);

            CompoundPadBrush = new SolidBrush(Preferences.CompoundPadFontColor);

            RefreshNodeSizes();
        }

        private void RefreshNodeSizes()
        {
            SizeF labelSize;
            SizeF portSize;
            string name;

            if (Manager.CurCompound != null && Manager.CurCompound.Nodes.Count > 0)
            {
                portHeight = (int)graphics.MeasureString("Coco", Preferences.NodePortsFont).Height + 3;
                int nodeWidth = 0;
                int nodeHeight = 0;

                foreach (Node node in Manager.CurCompound.Nodes)
                {                    
                    nodeWidth = 0;

                    labelSize = graphics.MeasureString(node.FullName, Preferences.NodeLabelFont);
                    node.UILabelY = (int)labelSize.Height;
                    nodeHeight = node.UILabelY;

                    nodeWidth = (int)labelSize.Width + 23;

                    foreach (Port port in node.Inputs)
                    {
                        if (port.IsVisible)
                        {
                            nodeHeight += portHeight;

                            name = port.Name;
                            if (name.StartsWith(node.FullName + "_"))
                            {
                                name = name.Substring(node.FullName.Length + 1);
                            }

                            portSize = graphics.MeasureString(name, GetPortFont(port));
                            if (portSize.Width > nodeWidth)
                            {
                                nodeWidth = (int)portSize.Width;
                            }
                        }
                    }

                    foreach (Port port in node.Outputs)
                    {
                        if (port.IsVisible)
                        {
                            nodeHeight += portHeight;

                            name = port.Name;
                            if (name.StartsWith(node.FullName + "_"))
                            {
                                name = name.Substring(node.FullName.Length + 1);
                            }

                            portSize = graphics.MeasureString(name, GetPortFont(port));
                            if (portSize.Width / LayoutSize > nodeWidth)
                            {
                                nodeWidth = (int)portSize.Width;
                            }
                        }
                    }

                    //Set the values
                    node.UIWidth = Math.Max(NODE_MIN_WIDTH, nodeWidth + 20);

                    node.UILabelX = (int)((node.UIWidth / 2) - (labelSize.Width / 2));

                    node.UIHeight = Math.Max(NODE_MIN_HEIGHT, nodeHeight + 18) + ((node.AllowAddPorts && node.DynamicInputs && node.DisplayState != NodeState.Collapsed) ? portHeight : 0) + ((node.AllowAddPorts && node.DynamicOutputs && node.DisplayState != NodeState.Collapsed) ? portHeight : 0);
                }
            }
        }

        private Font GetPortFont(Port port)
        {
            if (port.HighLight)
            {
                return strongFont;
            }

            return (port.Default ? lightFont : specialFont);
        }

        /// <summary>
        /// Create Image from state icons and add them in a dictionary
        /// </summary>
        private void CreateStates()
        {
            StatesIcons = new Dictionary<string, Image>();

            foreach (State state in Preferences.NodeStates)
            {
                ;
                FileInfo info = new FileInfo(PathHelper.ExpandedPath(state.IconPath));
                if (info.Exists)
                {
                    Bitmap bm = new Bitmap(info.FullName, false);
                    if (bm != null && !StatesIcons.ContainsKey(state.Name))
                    {
                        StatesIcons.Add(state.Name, bm);
                    }
                }
            }
        }

        // DRAG DROP ======================================================================

        private void NodesLayout_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void NodesLayout_DragDrop(object sender, DragEventArgs e)
        {
            Node toImport = null;

            if(e.Data.GetDataPresent(typeof(int)))
            {
                int Dropped = (int)e.Data.GetData(typeof(int));
                if(Manager.AvailableNodes.Count > Dropped)
                {
                    toImport = Manager.AvailableNodes[Dropped];
                }
            }
            else if(e.Data.GetDataPresent(typeof(string)))
            {
                string name = (string)e.Data.GetData(typeof(string));
            }
            else if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                TreeNode treeNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                if (treeNode.Tag != null && treeNode.Tag is stringNode)
                {
                    stringNode strNode = treeNode.Tag as stringNode;
                    if (strNode.Tag != null && strNode.Tag is Node)
                    {
                        toImport = strNode.Tag as Node;
                    }
                }
            }

            if(toImport != null)
            {
                //Where
                Point toClient = PointToClient(new Point(e.X, e.Y));
                if(toImport is Compound && ModifierKeys == Keys.Shift)
                {
                    //Open compound
                    if((Manager.CurCompound == Manager.Root && Manager.CurCompound.Nodes.Count == 0) || MessageBox.Show("You asked for an \"Open Compound\" and your current layout is not empty.\nAre you sure you want to overwrite ?", "Open Compound", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        Manager.NewLayout(toImport as Compound, true);
                        ChangeFocus(false);
                        Frame(Manager.CurCompound.Nodes);
                    }
                }
                else
                {
                    Manager.AddNode(toImport, Manager.CurCompound, (int)((toClient.X - 30) / LayoutSize), (int)((toClient.Y - 10) / LayoutSize), true);
                    ChangeFocus(false);
                }
            }
        }

        private void NodesLayout_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(int)))
                e.Effect = DragDropEffects.Copy; // Okay
            else
                e.Effect = DragDropEffects.None; // Unknown data, ignore it
        }

        // === CONTEXT MENU ================================================================

        // *** Nodes ***

        private void InitializeContextMenu()
        {
            // Node

            this.enterCompoundToolStripMenuItem.Tag = new NodeContextTag(false, true);
            this.explodeCompoundToolStripMenuItem.Tag = new NodeContextTag(false, true);
            this.toolStripMenuItem1.Tag = new NodeContextTag(false, true);

            this.copyToolStripMenuItem.Tag = new NodeContextTag(true, true);
            this.pasteToolStripMenuItem.Tag = new NodeContextTag(true, true);
            this.instanceToolStripMenuItem.Tag = new NodeContextTag(true, true);

            this.createCompoundToolStripMenuItem.Tag = new NodeContextTag(true, true);

            this.disconnectAllToolStripMenuItem.Tag = new NodeContextTag(true, true);

            this.deleteNodeToolStripMenuItem.Tag = new NodeContextTag(true, true);

            // Port
            this.deletePortToolStripMenuItem.Tag = new PortContextTag();

        }

        public void AddCustomContextMenuItem(string inText, Image inImg, EventHandler handler, NodeContextTag tag, bool nodeContext)
        {
            ContextMenuStrip strip = nodeContext ? nodeMenuStrip : rootMenuStrip;

            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(inText, inImg, handler);
            toolStripMenuItem.Tag = tag;
            toolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            strip.Items.Add(toolStripMenuItem);
        }

        public void AddCustomContextMenuItem(string inText, Image inImg, EventHandler handler, NodeContextTag tag)
        {
            AddCustomContextMenuItem(inText, inImg, handler, tag, true);
        }

        public void AddCustomContextSeparator(NodeContextTag tag, bool nodeContext)
        {
            ContextMenuStrip strip = nodeContext ? nodeMenuStrip : rootMenuStrip;

            ToolStripSeparator toolStripMenuItem = new ToolStripSeparator();
            toolStripMenuItem.Tag = tag;
            strip.Items.Add(toolStripMenuItem);
        }

        public void AddCustomContextSeparator(NodeContextTag tag)
        {
            AddCustomContextSeparator(tag, true);
        }

        public ToolStripMenuItem AddPortsContextMenuItem(string inText, Image inImg, EventHandler handler, PortContextTag tag)
        {
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(inText, inImg, handler);
            toolStripMenuItem.Tag = tag;
            toolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            this.customPortMenuStrip.Items.Add(toolStripMenuItem);

            return toolStripMenuItem;
        }

        public void AddPortsContextSeparator(PortContextTag tag)
        {
            ToolStripSeparator toolStripMenuItem = new ToolStripSeparator();
            toolStripMenuItem.Tag = tag;
            this.customPortMenuStrip.Items.Add(toolStripMenuItem);
        }

        public void AddLinksContextMenuItem(string inText, Image inImg, EventHandler handler, LinkContextTag tag)
        {
            ContextMenuStrip strip = linkMenuStrip;

            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(inText, inImg, handler);
            toolStripMenuItem.Tag = tag;
            toolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            strip.Items.Add(toolStripMenuItem);
        }

        //private void createCompoundToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    List<Node> nodes = Selection.GetSelectedNodes();
        //    if (nodes.Count > 0)
        //    {
        //        Compound compound = Manager.AddCompound(nodes);

        //        if (compound != null)
        //        {
        //            Manager.EnterCompound(compound);
        //        }

        //        ChangeFocus(true);
        //        Frame(Manager.CurCompound.Nodes);
        //    }
        //}

        private void createCompoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Node> nodes = Selection.GetSelectedNodes();
            List<string> nodesName = new List<string>();

            if (nodes.Count > 0)
            {
                foreach (Node Node in nodes)
                {
                    string nodeName = Node.FullName;
                    nodesName.Add(nodeName);
                }

                bool test = NodalDirector.CreateCompound(nodesName);
            }
                ChangeFocus(true);
                Frame(Manager.CurCompound.Nodes);
        }

        //private void disconnectAllToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    List<Node> nodes = Selection.GetSelectedNodes();
        //    if (nodes.Count > 0)
        //    {
        //        foreach (Node Node in nodes)
        //        {
        //            Manager.CurCompound.UnConnectAll(Node);
        //        }
        //    }
        //    else
        //    {
        //        Node Node = nodeMenuStrip.Tag as Node;

        //        Manager.CurCompound.UnConnectAll(Node);
        //    }

        //    Invalidate();
        //}

        private void disconnectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Node> nodes = Selection.GetSelectedNodes();
            List<string> nodesName = new List<string>();

            if (nodes.Count > 0)
            {
                foreach (Node Node in nodes)
                {
                    bool test = NodalDirector.DisconnectAll(Node.FullName);
                }
                
            }
            else
            {
                Node Node = nodeMenuStrip.Tag as Node;
                bool test = NodalDirector.DisconnectAll(Node.FullName);
            }
        }

        private void disconnectInputsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Node> nodes = Selection.GetSelectedNodes();
            List<string> nodesName = new List<string>();

            if (nodes.Count > 0)
            {
                foreach (Node Node in nodes)
                {
                    NodalDirector.DisconnectInputs(Node.FullName);
                }
                
            }
            else
            {
                Node Node = nodeMenuStrip.Tag as Node;
                NodalDirector.DisconnectInputs(Node.FullName);
            }

            Invalidate();
        }

        private void disconnectOutputsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Node> nodes = Selection.GetSelectedNodes();


            if (nodes.Count > 0)
            {
                foreach (Node Node in nodes)
                {
                    NodalDirector.DisconnectOutputs(Node.FullName);
                }
                
            }
            else
            {
                Node Node = nodeMenuStrip.Tag as Node;
                NodalDirector.DisconnectOutputs(Node.FullName);
            }

            Invalidate();

        }

        private void deleteNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }

        public void RefreshPorts()
        {
            Inputs.RefreshWidth();
            Outputs.RefreshWidth();
            ResizeLayout();
        }

        public void InvalidateAll()
        {
            Invalidate();
            RefreshPorts();
        }

        private void NodesLayout_Load(object sender, EventArgs e)
        {

        }

        // *** Links ***

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Link link = linkMenuStrip.Tag as Link;

            NodalDirector.Disconnect(link.Target.Owner.FullName, link.Target.FullName, link.Source.Owner.FullName, link.Source.FullName);
            /*
            Manager.CurCompound.UnConnect(link);

            Invalidate();*/
        }

        // Compound specific --------------------------

        private void enterCompoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IsInitialised = false;
            Compound Node = nodeMenuStrip.Tag as Compound;
            if (Node != null)
            {
                Manager.EnterCompound(Node);
            }

            ChangeFocus(true);
            Frame(Manager.CurCompound.Nodes);
        }

        private void explodeCompoundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Node> selNodes = Selection.GetSelectedNodes();
            List<Node> affectedNodes = new List<Node>();
            foreach (Node selNode in selNodes)
            {
                Compound Node = selNode as Compound;
                if (Node != null)
                {
                    affectedNodes.AddRange(Node.Nodes);
                    Manager.ExplodeCompound(Node);
                }
            }

            Selection.Select(affectedNodes);
            ChangeFocus(false);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Manager.ClipBoard = Selection.GetSelectedNodes();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Point toClient = PointToClient(Cursor.Position);
            Point Offset = new Point(toClient.X - (int)Manager.ClipBoard[0].UIx, toClient.Y - (int)Manager.ClipBoard[0].UIy);

            foreach (Node node in Manager.ClipBoard)
            {
                Manager.Copy(node, Manager.CurCompound, (int)((node.UIx + Offset.X -30) / LayoutSize), (int)((node.UIy + Offset.Y - 10) / LayoutSize));
            }

            ChangeFocus(true);
        }

        private void pasteRenamedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichDialogResult rslt = TKMessageBox.ShowInput(InputTypes.String, "Please provide string to search", "Paste renamed search string");
            if (rslt.Result != DialogResult.OK)
            {
                return;
            }
            string search = (string)rslt.Data;

            rslt = TKMessageBox.ShowInput(InputTypes.String, "Please provide string to replace", "Paste renamed replace string");
            if (rslt.Result != DialogResult.OK)
            {
                return;
            }
            string replace = (string)rslt.Data;

            Point toClient = PointToClient(Cursor.Position);
            Point Offset = new Point(toClient.X - (int)Manager.ClipBoard[0].UIx, toClient.Y - (int)Manager.ClipBoard[0].UIy);

            foreach (Node node in Manager.ClipBoard)
            {
                Manager.Copy(node, Manager.CurCompound, (int)((node.UIx + Offset.X - 30) / LayoutSize), (int)((node.UIy + Offset.Y - 10) / LayoutSize), search, replace);
            }

            ChangeFocus(true);
        }

        //private void exposeAllPortsToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    List<Node> selNodes = Selection.GetSelectedNodes();
        //    if (selNodes.Count > 0)
        //    {
        //        foreach (Node node in selNodes)
        //        {
        //            foreach (Port port in node.Inputs)
        //            {
        //                PortInstance parentPort = node.Parent.GetPortFromNode(port);
        //                parentPort.Visible = true;
        //            }

        //            foreach (Port port in node.Outputs)
        //            {
        //                PortInstance parentPort = node.Parent.GetPortFromNode(port);
        //                parentPort.Visible = true;
        //            }
        //        }

        //        ChangeFocus(false);
        //        RefreshPorts();
        //    }
        //}

        private void exposeAllPortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Node> selNodes = Selection.GetSelectedNodes();

            if (selNodes.Count > 0)
            {
                foreach (Node node in selNodes)
                {
                    NodalDirector.ExposeAllPorts(node.FullName);
                }

                ChangeFocus(false);
                RefreshPorts();
            }
        }

        //private void hideAllPortsToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    List<Node> selNodes = Selection.GetSelectedNodes();
        //    if (selNodes.Count > 0)
        //    {
        //        foreach (Node node in selNodes)
        //        {
        //            foreach (Port port in node.Inputs)
        //            {
        //                PortInstance parentPort = node.Parent.GetPortFromNode(port);

        //                if (!parentPort.IsLinked())
        //                {
        //                    parentPort.Visible = false;
        //                }
        //            }

        //            foreach (Port port in node.Outputs)
        //            {
        //                PortInstance parentPort = node.Parent.GetPortFromNode(port);

        //                if (!parentPort.IsLinked())
        //                {
        //                    parentPort.Visible = false;
        //                }
        //            }
        //        }

        //        ChangeFocus(false);
        //        RefreshPorts();
        //    }
        //}

        private void hideAllPortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Node> selNodes = Selection.GetSelectedNodes();

            if (selNodes.Count > 0)
            {
                foreach (Node node in selNodes)
                {
                    NodalDirector.HideAllPorts(node.FullName);
                }

                ChangeFocus(false);
                RefreshPorts();
            }
        }

        private void instanceToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public bool IsValidName(string p)
        {
            if (string.IsNullOrEmpty(p))
            {
                return false;
            }

            Regex reg = new Regex("^[0-9]|[^a-zA-Z0-9_]", RegexOptions.None);
            return !(reg.Match(p).Success);
        }

        public void EditPreferences(Form owner)
        {
            PreferencesForm prefsForm = new PreferencesForm();
            prefsForm.PrefChanged += new PreferencesForm.PrefChangedEventHandler(prefsForm_PrefChanged);
            prefsForm.Owner = owner;
            prefsForm.Show();
            prefsForm.Init("TK_NodalEditor Preferences", Preferences, RootPath + "\\Preferences", "Preferences.xml");
        }

        public void EditPreferences()
        {
            PreferencesForm prefsForm = new PreferencesForm();
            prefsForm.PrefChanged += new PreferencesForm.PrefChangedEventHandler(prefsForm_PrefChanged);

            prefsForm.Show();
            prefsForm.Init("TK_NodalEditor Preferences", Preferences, RootPath + "\\Preferences", "Preferences.xml");
        }

        void prefsForm_PrefChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "LinksCategories":
                    CreateLinksTools();
                    break;

                case "NodeCategories":
                    CreateNodesTools();
                    break;

                case "NodeStates":
                    CreateStates();
                    break;

                case "All":
                    CreateGraphicalElements();
                    break;
                default :
                    if (!e.PropertyName.Contains("Font") && e.PropertyName.Contains("Color"))
                    {
                        CreateLayoutTools();
                    }
                    else
                    {
                        CreateFonts();
                    }
                    break;
            }

            ChangeFocus(false);
        }

        public void Expand(bool selOnly)
        {
            Manager.Expand(selOnly);
            Invalidate();
        }

        public void Minimize(bool selOnly)
        {
            Manager.Minimize(selOnly);
            Invalidate();
        }

        public void Collapse(bool selOnly)
        {
            Manager.Collapse(selOnly);
            Invalidate();
        }

        public void SelectNodes(List<Node> list)
        {
            Selection.Select(list);
            Invalidate();
            OnLinkSelectionChanged(new LinkSelectionChangedEventArgs(null));
            OnSelectionChanged(new SelectionChangedEventArgs(Selection.Selection));
        }

        // Ports contextMenu

        private void SetPortsContext(Node selNode, bool isOutput)
        {
            portsMenuStrip.Items.Clear();
            portsMenuStrip.Text = isOutput ? "true" : "false";
            portsMenuStrip.Tag = selNode;

            List<Port> ports = isOutput ? selNode.Outputs : selNode.Inputs;

            foreach (Port port in ports)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(port.Name, null, new EventHandler(PortShowHide));
                item.CheckOnClick = true;
                item.Checked = port.Visible;
                item.Tag = port;
                if (port.HighLight)
                {
                    item.Font = HighlightedPortFont;
                }
                portsMenuStrip.Items.Add(item);
            }

            portsMenuStrip.Items.Add(toolStripMenuItem3);

            portsMenuStrip.Items.Add(showAllToolStripMenuItem);
            portsMenuStrip.Items.Add(hideAllToolStripMenuItem);
        }

        private void manageInputsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Node selNode = Selection.GetSelectedNode();
            if (selNode != null)
            {
                SetPortsContext(selNode, false);
                portsMenuStrip.Show(Form.MousePosition);

                ContextX = Form.MousePosition.X;
                ContextY = Form.MousePosition.Y;
                portsMenuStrip.Show(ContextX, ContextY);
            }
        }

        private void manageOutputsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Node selNode = Selection.GetSelectedNode();
            if (selNode != null)
            {
                SetPortsContext(selNode, true);

                ContextX = Form.MousePosition.X;
                ContextY = Form.MousePosition.Y;
                portsMenuStrip.Show(ContextX, ContextY);
            }
        }

        void PortShowHide(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            Port port = (Port)item.Tag;

            if (item.Checked || !port.IsLinked())
            {
                port.Visible = item.Checked;
                Invalidate();
            }
            else
            {
                item.Checked = true;
            }

            if (ModifierKeys == Keys.Shift)
            {
                portsMenuStrip.Show(ContextX, ContextY);
            }
        }

        private void hideAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Node inNode = (Node)portsMenuStrip.Tag;
            bool isOutput = (portsMenuStrip.Text == "true");

            inNode.SetPortsVisibility(isOutput, false);
            Invalidate();

            if (ModifierKeys == Keys.Shift)
            {
                SetPortsContext(inNode, isOutput);
                portsMenuStrip.Show(ContextX, ContextY);
            }
        }

        private void showAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Node inNode = (Node)portsMenuStrip.Tag;
            bool isOutput = (portsMenuStrip.Text == "true");

            inNode.SetPortsVisibility(isOutput, true);
            Invalidate();

            if (ModifierKeys == Keys.Shift)
            {
                SetPortsContext(inNode, isOutput);
                portsMenuStrip.Show(ContextX, ContextY);
            }
        }

        private void deletePortToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Port inPort = (Port)deletePortToolStripMenuItem.Tag;
            inPort.Owner.RemovePort(inPort);
            Invalidate();
        }

        public void FrameSelected()
        {
            List<Node> selNodes = Selection.GetSelectedNodes();
            if (selNodes.Count > 0)
            {
                Frame(selNodes);
            }
            else
            {
                Frame(Manager.CurCompound.Nodes);
            }
        }
        /* DEPRECATE
        public void DeleteNode(Node inNode)
        {
            Manager.RemoveNode(inNode);
            inNode.Deleted = true;

            RefreshPorts();
            ChangeFocus(true);
        }*/

        //public void DeleteSelected()
        //{
        //    List<Node> selNodes = Selection.GetSelectedNodes();
        //    if (selNodes.Count > 0)
        //    {
        //        List<string> nodesNames = new List<string>();
        //        foreach (Node node in selNodes)
        //        {
        //            nodesNames.Add(node.FullName);
        //        }

        //        NodalDirector.DeleteNodes(nodesNames);
        //        /*
        //        Manager.Companion.LaunchProcess("Delete nodes", selNodes.Count);

        //        foreach (Node node in selNodes)
        //        {
        //            Manager.RemoveNode(node);
        //            node.Deleted = true;
        //            Manager.Companion.ProgressBarIncrement();
        //        }

        //        RefreshPorts();
        //        Selection.Selection.Clear();
        //        ChangeFocus(true);
        //        Manager.Companion.EndProcess();
        //        */
        //    }
        //    else
        //    {
        //        List<Link> selLinks = Selection.GetSelectedLinks();
        //        if (selLinks.Count > 0)
        //        {
        //            NodalDirector.DeleteLinks(selLinks);
        //        }
        //    }
        //}

        
        public void DeleteSelected()
        {
            List<Node> selNodes = Selection.GetSelectedNodes();
            if (selNodes.Count > 0)
            {
                List<string> nodesNames = new List<string>();
                foreach (Node node in selNodes)
                {
                    nodesNames.Add(node.FullName);
                }



                NodalDirector.DeleteNode(nodesNames);
                /*
                Manager.Companion.LaunchProcess("Delete nodes", selNodes.Count);

                foreach (Node node in selNodes)
                {
                    Manager.RemoveNode(node);
                    node.Deleted = true;
                    Manager.Companion.ProgressBarIncrement();
                }

                RefreshPorts();
                Selection.Selection.Clear();
                ChangeFocus(true);
                Manager.Companion.EndProcess();
                */
            }
            else
            {
                List<Link> selLinks = Selection.GetSelectedLinks();
                if (selLinks.Count > 0)
                {
                    foreach (Link link in selLinks)
                    {
                        NodalDirector.Disconnect(link.Target.Owner.FullName, link.Target.FullName, link.Source.Owner.FullName, link.Source.FullName);
                    }
                    
                        
                }
            }
        }

        public void ParentNode()
        {
            List<Node> nodes = Selection.GetSelectedNodes();
            List<Compound> compounds = new List<Compound>();

            if (nodes.Count < 2)
            {
                TKMessageBox.ShowError("Please select at least 2 nodes, first any number of nodes to reparent, then at last a Compound to reparent the nodes into !", "Compound parent error");
                return;
            }

            Compound newParent = nodes[nodes.Count - 1] as Compound;

            if (newParent == null)
            {
                TKMessageBox.ShowError("Last selected node must be a compound to reparent the nodes into !", "Compound parent error");
                return;
            }

            nodes.Remove(nodes[nodes.Count - 1]);

            foreach (Node node in nodes)
            {
                if (node.Parent != null && node.Parent != newParent)
                {
                    NodalDirector.Parent(node.FullName, newParent.FullName);
                }
            }

            RefreshPorts();
            Selection.Selection.Clear();
            ChangeFocus(true);
        }

        //public void ParentNode()
        //{
        //    List<Node> nodes = Selection.GetSelectedNodes();
        //    List<string> nodesName = new List<string>();

        //    if (nodes.Count < 2)
        //    {
        //        TKMessageBox.ShowError("Please select at least 2 nodes, first any number of nodes to reparent, then at last a Compound to reparent the nodes into !", "Compound parent error");
        //        return;
        //    }

        //    Compound newParent = nodes[nodes.Count - 1] as Compound;

        //    if (newParent == null)
        //    {
        //        TKMessageBox.ShowError("Last selected node must be a compound to reparent the nodes into !", "Compound parent error");
        //        return;
        //    }

        //    foreach (Node node in nodes)
        //    {
        //        string nodeName = node.FullName;
        //        nodesName.Add(nodeName);
        //    }
        //    bool test = NodalDirector.Parent(nodesName);

        //    RefreshPorts();
        //    Selection.Selection.Clear();
        //    ChangeFocus(true);
        //}

        //public void UnParentNode()
        //{
        //    List<Node> nodes = Selection.GetSelectedNodes();
        //    foreach (Node node in nodes)
        //    {
        //        if (node.Parent != null && node.Parent.Parent != null)
        //        {
        //            Manager.MoveNodes(new List<Node> { node }, node.Parent.Parent);
        //        }
        //    }
        //    RefreshPorts();
        //    Selection.Selection.Clear();
        //    ChangeFocus(true);
        //}

        public void UnParentNode()
        {
            List<Node> nodes = Selection.GetSelectedNodes();
                      
            if (nodes.Count > 0)
            {
                foreach (Node Node in nodes)
                {
                    NodalDirector.UnParent(Node.FullName);
                }
            }

            RefreshPorts();
            Selection.Selection.Clear();
            ChangeFocus(true);
        }

        public void UpdateSelected()
        {
            List<Node> selNodes = Selection.GetSelectedNodes();
            if (selNodes.Count > 0)
            {
                foreach (Node node in selNodes)
                {
                    Node refNode = Manager.GetAvailableNode(node.NativeName);
                    if (refNode != null && !(refNode is Compound))
                    {
                        node.Update(refNode, false);
                    }
                }

                RefreshPorts();
                Selection.Selection.Clear();
                ChangeFocus(true);
            }
        }

        private void unparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnParentNode();
        }

        private void parentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParentNode();
        }
    }
}
