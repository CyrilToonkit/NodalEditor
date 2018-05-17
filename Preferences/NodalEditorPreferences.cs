using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Drawing.Design;
using System.Xml.Serialization;
using TK.BaseLib;
using TK.GraphComponents.CustomData;

namespace TK.NodalEditor
{
    [XmlType(TypeName = "Prefs")]
    public class NodalEditorPreferences : SaveableData
    {
        public NodalEditorPreferences()
        {

        }

        public NodalEditorPreferences(string BasePath)
        {
            strFilename = BasePath + "\\Preferences\\Preferences.xml";

            //verify if a default exists
            FileInfo defaultfile = new FileInfo(BasePath + "\\Preferences\\Default.xml");
            if (!defaultfile.Exists)
            {
                ToDefault();
                Save(defaultfile.FullName);
            }

            //take care of current Preferences
            FileInfo file = new FileInfo(strFilename);
            if (!file.Exists || !Load(strFilename))
            {
                ToDefault();
                Save(strFilename);
            }
        }

        string strFilename;

        // === Behavior ===========================================================

        Boolean mCheckCycles = true;
        [CategoryAttribute("Behavior")]
        [DescriptionAttribute("Tell the Nodal Editor to fordbid graph cycles or not")]
        public Boolean CheckCycles
        {
            get { return mCheckCycles; }
            set { mCheckCycles = value; }
        }

        // === LayoutPrefs ===========================================================

        Boolean mMagnetic = true;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Indicates if the nodes should snap to grid")]
        public Boolean Magnetic
        {
            get { return mMagnetic; }
            set { mMagnetic = value; }
        }

        Boolean mShowGrid = true;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Indicates if the Grid is visible")]
        public Boolean ShowGrid
        {
            get { return mShowGrid; }
            set { mShowGrid = value; }
        }

        Boolean mShowRootPorts = false;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Indicates if the Ports pads are visible for the Root Compound")]
        public Boolean ShowRootPorts
        {
            get { return mShowRootPorts; }
            set { mShowRootPorts = value; }
        }

        Boolean mShowCompoundPorts = true;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Indicates if the Ports pads are visible for Compounds")]
        public Boolean ShowCompoundPorts
        {
            get { return mShowCompoundPorts; }
            set { mShowCompoundPorts = value; }
        }


        Boolean mShowMap = true;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Indicates if the MiniMap is visible")]
        public Boolean ShowMap
        {
            get { return mShowMap; }
            set { mShowMap = value; }
        }

        int mMapWidth = 100;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Width of the MiniMap")]
        public int MapWidth
        {
            get { return mMapWidth; }
            set { mMapWidth = value; }
        }

        int mGridSpacing = 10;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Sets the grid spacing in pixels")]
        public int GridSpacing
        {
            get { return mGridSpacing; }
            set { mGridSpacing = value; }
        }

        double mMinimumZoom = 0.1;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Sets the minimum Zoom accessible with the mouseWheel")]
        public double MinimumZoom
        {
            get { return mMinimumZoom; }
            set { mMinimumZoom = value; }
        }

        double mMaximumZoom = 2;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Sets the maximum Zoom accessible with the mouseWheel")]
        public double MaximumZoom
        {
            get { return mMaximumZoom; }
            set { mMaximumZoom = value; }
        }

        bool mShowNodeTips = true;
        [CategoryAttribute("Layout")]
        [DescriptionAttribute("Display tooltips when mouse is over nodes or ports")]
        public bool ShowNodeTips
        {
            get { return mShowNodeTips; }
            set { mShowNodeTips = value; }
        }

        // === FontsPrefs ===========================================================

        Font mNodePortsFont = new Font("Verdana", 7f);
        [XmlIgnore]
        [CategoryAttribute("Fonts")]
        [DescriptionAttribute("Font used to display Node Ports")]
        public Font NodePortsFont
        {
            get { return mNodePortsFont; }
            set { mNodePortsFont = value; }
        }

        [XmlElement("NodePortsFont")]
        [BrowsableAttribute(false)]
        public string NodePortsFontXml
        {
            get { return NodalEditorPreferences.SerializeFont(mNodePortsFont); }
            set { mNodePortsFont = NodalEditorPreferences.DeserializeFont(value); }
        }

        Color mNodePortsFontColor;
        [XmlIgnore]
        [CategoryAttribute("Fonts")]
        [DescriptionAttribute("Color of the Node Ports font")]
        public Color NodePortsFontColor
        {
            get { return mNodePortsFontColor; }
            set { mNodePortsFontColor = value; }
        }

        [XmlElement("NodePortsFontColor")]
        [BrowsableAttribute(false)]
        public string NodePortsFontColorXml
        {
            get { return NodalEditorPreferences.SerializeColor(mNodePortsFontColor); }
            set { mNodePortsFontColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Font mNodeLabelFont = new Font("Tahoma", 7f, FontStyle.Bold);
        [XmlIgnore]
        [CategoryAttribute("Fonts")]
        [DescriptionAttribute("Font used to display Node label")]
        public Font NodeLabelFont
        {
            get { return mNodeLabelFont; }
            set { mNodeLabelFont = value; }
        }

        [XmlElement("NodeLabelFont")]
        [BrowsableAttribute(false)]
        public string NodeLabelFontXml
        {
            get { return NodalEditorPreferences.SerializeFont(mNodeLabelFont); }
            set { mNodeLabelFont = NodalEditorPreferences.DeserializeFont(value); }
        }

        Color mNodeLabelFontColor;
        [XmlIgnore]
        [CategoryAttribute("Fonts")]
        [DescriptionAttribute("Color of the Node label font")]
        public Color NodeLabelFontColor
        {
            get { return mNodeLabelFontColor; }
            set { mNodeLabelFontColor = value; }
        }

        [XmlElement("NodeLabelFontColor")]
        [BrowsableAttribute(false)]
        public string NodeLabelFontColorXml
        {
            get { return NodalEditorPreferences.SerializeColor(mNodeLabelFontColor); }
            set { mNodeLabelFontColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Font mCompoundPadFont = new Font("Tahoma", 7f, FontStyle.Bold);
        [XmlIgnore]
        [CategoryAttribute("Fonts")]
        [DescriptionAttribute("Font used to display ports in Compound Pads")]
        public Font CompoundPadFont
        {
            get { return mCompoundPadFont; }
            set { mCompoundPadFont = value; }
        }

        [XmlElement("CompoundPadFont")]
        [BrowsableAttribute(false)]
        public string CompoundPadFontXml
        {
            get { return NodalEditorPreferences.SerializeFont(mCompoundPadFont); }
            set { mCompoundPadFont = NodalEditorPreferences.DeserializeFont(value); }
        }

        Color mCompoundPadFontColor;
        [XmlIgnore]
        [CategoryAttribute("Fonts")]
        [DescriptionAttribute("Color of the Compound pad")]
        public Color CompoundPadFontColor
        {
            get { return mCompoundPadFontColor; }
            set { mCompoundPadFontColor = value; }
        }

        [XmlElement("CompoundPadFontColor")]
        [BrowsableAttribute(false)]
        public string CompoundPadFontColorXml
        {
            get { return NodalEditorPreferences.SerializeColor(mCompoundPadFontColor); }
            set { mCompoundPadFontColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        // === ColorsPrefs ===========================================================

        Color mBackgroundColor;
        [XmlIgnore]
        [CategoryAttribute("Colors")]
        [DescriptionAttribute("Color of the TK_NodalEditor background")]
        public Color BackgroundColor
        {
            get { return mBackgroundColor; }
            set { mBackgroundColor = value; }
        }

        [XmlElement("BackgroundColor")]
        [BrowsableAttribute(false)]
        public string mBackgroundXmlColor
        {
            get { return NodalEditorPreferences.SerializeColor(mBackgroundColor); }
            set { mBackgroundColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Color mCompoundPadColor;
        [XmlIgnore]
        [CategoryAttribute("Colors")]
        [DescriptionAttribute("Color of the Compound pad")]
        public Color CompoundPadColor
        {
            get { return mCompoundPadColor; }
            set { mCompoundPadColor = value; }
        }

        [XmlElement("CompoundPadColor")]
        [BrowsableAttribute(false)]
        public string CompoundPadColorXml
        {
            get { return NodalEditorPreferences.SerializeColor(mCompoundPadColor); }
            set { mCompoundPadColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Color mGridColor;
        [XmlIgnore]
        [CategoryAttribute("Colors")]
        [DescriptionAttribute("Color of the TK_NodalEditor grid")]
        public Color GridColor
        {
            get { return mGridColor; }
            set { mGridColor = value; }
        }

        [XmlElement("GridColor")]
        [BrowsableAttribute(false)]
        public string mGridXmlColor
        {
            get { return NodalEditorPreferences.SerializeColor(mGridColor); }
            set { mGridColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Color mDefaultLinkColor;
        [XmlIgnore]
        [CategoryAttribute("Colors")]
        [DescriptionAttribute("Color of a link without any specific category")]
        public Color DefaultLinkColor
        {
            get { return mDefaultLinkColor; }
            set { mDefaultLinkColor = value; }
        }

        [XmlElement("DefaultLinkColor")]
        [BrowsableAttribute(false)]
        public string mDefaultLinkXmlColor
        {
            get { return NodalEditorPreferences.SerializeColor(mDefaultLinkColor); }
            set { mDefaultLinkColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Color mDefaultNodeColor;
        [XmlIgnore]
        [CategoryAttribute("Colors")]
        [DescriptionAttribute("Color of a Node without any specific category")]
        public Color DefaultNodeColor
        {
            get { return mDefaultNodeColor; }
            set { mDefaultNodeColor = value; }
        }

        [XmlElement("DefaultNodeColor")]
        [BrowsableAttribute(false)]
        public string mDefaultNodeXmlColor
        {
            get { return NodalEditorPreferences.SerializeColor(mDefaultNodeColor); }
            set { mDefaultNodeColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Color mDefaultSelectedNodeColor;
        [XmlIgnore]
        [CategoryAttribute("Colors")]
        [DescriptionAttribute("Color of a SelectedNode without any specific category")]
        public Color DefaultSelectedNodeColor
        {
            get { return mDefaultSelectedNodeColor; }
            set { mDefaultSelectedNodeColor = value; }
        }

        [XmlElement("DefaultSelectedNodeColor")]
        [BrowsableAttribute(false)]
        public string mDefaultSelectedNodeXmlColor
        {
            get { return NodalEditorPreferences.SerializeColor(mDefaultSelectedNodeColor); }
            set { mDefaultSelectedNodeColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Color mDefaultCompoundColor;
        [XmlIgnore]
        [CategoryAttribute("Colors")]
        [DescriptionAttribute("Color of a Compound without any specific category")]
        public Color DefaultCompoundColor
        {
            get { return mDefaultCompoundColor; }
            set { mDefaultCompoundColor = value; }
        }

        [XmlElement("DefaultCompoundColor")]
        [BrowsableAttribute(false)]
        public string mDefaultCompoundXmlColor
        {
            get { return NodalEditorPreferences.SerializeColor(mDefaultCompoundColor); }
            set { mDefaultCompoundColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        Color mDefaultSelectedCompoundColor;
        [XmlIgnore]
        [CategoryAttribute("Colors")]
        [DescriptionAttribute("Color of a SelectedCompound without any specific category")]
        public Color DefaultSelectedCompoundColor
        {
            get { return mDefaultSelectedCompoundColor; }
            set { mDefaultSelectedCompoundColor = value; }
        }

        [XmlElement("DefaultSelectedCompoundColor")]
        [BrowsableAttribute(false)]
        public string mDefaultSelectedCompoundXmlColor
        {
            get { return NodalEditorPreferences.SerializeColor(mDefaultSelectedCompoundColor); }
            set { mDefaultSelectedCompoundColor = NodalEditorPreferences.DeserializeColor(value); }
        }

        // === CategoriesPrefs ===========================================================

        List<SelectableCategory> mNodeCategories = new List<SelectableCategory>();
        [CategoryAttribute("Categories")]
        [DescriptionAttribute("Define colors and categories for nodes (Beware if adding or removing items)")]
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<SelectableCategory> NodeCategories
        {
            get { return mNodeCategories; }
            set { mNodeCategories = value; }
        }

        List<Category> mLinksCategories = new List<Category>();
        [CategoryAttribute("Categories")]
        [DescriptionAttribute("Define colors and categories for links (Beware if adding or removing items)")]
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<Category> LinksCategories
        {
            get { return mLinksCategories; }
            set { mLinksCategories = value; }
        }

        /*
        List<LinkState> mLinksStates = new List<LinkState>();
        [CategoryAttribute("Categories")]
        [DescriptionAttribute("Define custom start and end arrows for links")]
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<LinkState> LinksStates
        {
            get { return mLinksStates; }
            set { mLinksStates = value; }
        }
         * */

        // === StatesPrefs ===========================================================

        List<State> mNodeStates = new List<State>();
        [CategoryAttribute("States")]
        [DescriptionAttribute("Define node special states icons, up to 12*12 px")]
        [Editor(typeof(MyCollectionEditor), typeof(UITypeEditor))]
        public List<State> NodeStates
        {
            get { return mNodeStates; }
            set { mNodeStates = value; }
        }

        public void ToDefault()
        {
            mCheckCycles = true;

            mMagnetic = true;
            mShowGrid = true;
            mShowRootPorts = false;
            mShowCompoundPorts = true;
            mShowMap = true;
            mMapWidth = 100;
            mGridSpacing = 10;
            mMinimumZoom = 0.1;
            mMaximumZoom = 2;
            mShowNodeTips = true;

            mNodePortsFont = new Font("Verdana", 7f);
            mNodePortsFontColor = Color.Black;
            mNodeLabelFont = new Font("Tahoma", 7f, FontStyle.Bold);
            mNodeLabelFontColor = Color.Black;
            mCompoundPadFont = new Font("Tahoma", 7f, FontStyle.Bold);
            mCompoundPadFontColor = Color.Black;

            mBackgroundColor = Color.White;
            mCompoundPadColor = Color.Linen;
            mGridColor = Color.LightGray;

            mDefaultLinkColor = Color.Red;

            mDefaultNodeColor = Color.DodgerBlue;
            mDefaultSelectedNodeColor = Color.DeepSkyBlue;

            mDefaultCompoundColor = Color.SandyBrown;
            mDefaultSelectedCompoundColor = Color.Orange;

            mNodeCategories.Clear();
            mLinksCategories.Clear();
            //mLinksStates.Clear();

            mNodeStates.Clear();
        }

        public override void Clone(SaveableData Data)
        {
            NodalEditorPreferences clonePrefs = Data as NodalEditorPreferences;

            if (clonePrefs != null)
            {
                mCheckCycles = clonePrefs.CheckCycles;

                mMagnetic = clonePrefs.Magnetic;
                mShowGrid = clonePrefs.ShowGrid;
                mShowRootPorts = clonePrefs.ShowRootPorts;
                mShowCompoundPorts = clonePrefs.ShowCompoundPorts;
                mShowMap = clonePrefs.ShowMap;
                mMapWidth = clonePrefs.MapWidth;
                mGridSpacing = clonePrefs.GridSpacing;
                mMinimumZoom = clonePrefs.MinimumZoom;
                mMaximumZoom = clonePrefs.MaximumZoom;
                mShowNodeTips = clonePrefs.ShowNodeTips;

                mNodePortsFont = clonePrefs.NodePortsFont;
                mNodePortsFontColor = clonePrefs.NodePortsFontColor;
                mNodeLabelFont = clonePrefs.NodeLabelFont;
                mNodeLabelFontColor = clonePrefs.NodeLabelFontColor;
                mCompoundPadFont = clonePrefs.CompoundPadFont;
                mCompoundPadFontColor = clonePrefs.CompoundPadFontColor;

                mBackgroundColor = clonePrefs.BackgroundColor;
                mCompoundPadColor = clonePrefs.CompoundPadColor;
                mGridColor = clonePrefs.GridColor;

                mDefaultLinkColor = clonePrefs.DefaultLinkColor;

                mDefaultNodeColor = clonePrefs.DefaultNodeColor;
                mDefaultSelectedNodeColor = clonePrefs.DefaultSelectedNodeColor;

                mDefaultCompoundColor = clonePrefs.DefaultCompoundColor;
                mDefaultSelectedCompoundColor = clonePrefs.DefaultSelectedCompoundColor;
                //mLinksStates = clonePrefs.mLinksStates;

                mLinksCategories = clonePrefs.LinksCategories;
                mNodeCategories = clonePrefs.NodeCategories;
                mNodeStates = clonePrefs.NodeStates;
            }
        }

        public Color GetNodeColor(Node inNode)
        {
            Dictionary<string, Color> customColors = new Dictionary<string, Color>();
            foreach (SelectableCategory categ in NodeCategories)
            {
                customColors.Add(categ.Name, categ.Color);
            }

            customColors.Add("Compound", mDefaultCompoundColor);

            string type = inNode.NodeElementType != "Default" ? inNode.NodeElementType : inNode.NodeType;

            if (customColors.ContainsKey(type))
            {
                return customColors[type];
            }

            return mDefaultNodeColor;
        }

        public static Color RandomColor()
        {
            Random rand = new Random();

            int R = rand.Next(255);
            int G = rand.Next(255);
            int B = rand.Next(255);

            return Color.FromArgb(255, R, G, B);
        }
    }
}
