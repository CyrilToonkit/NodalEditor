using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TK.NodalEditor.NodesLayout
{
    public class Overlay
    {
        public Overlay(NodesLayout inLayout)
        {
            Layout = inLayout;
        }

        public float ZoomRatio;
        public int NewX;
        public int NewY;

        public int LeftStart;
        public int TopStart;
        public int VisWidth;
        public int VisHeight;

        NodesLayout Layout;

        Pen Hash = Pens.Black;
        Brush NodeBrush = Brushes.Blue;
        Brush TransBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0));

        public bool DragSelect = false;
        public Rectangle SelectRectangle = new Rectangle();

        public Brush ConnectBrush = Brushes.Black;
        public Pen ConnectPen = Pens.Black;
        public Point[] ConnectArrow = new Point[0];

        private const float penWidth = 1;

        public void Draw(Graphics e)
        {
            if(DragSelect)
            {
                e.DrawRectangle(Hash, SelectRectangle);
            }

            // LINKS *********************************************************

            if (ConnectArrow.Length == 2)
            {
                DrawArrow(e, ConnectPen, ConnectBrush, ConnectArrow[0], ConnectArrow[1], 0, false, Layout.LinkStates["Default"], false);
            }

            if (Layout.Preferences.ShowMap)
            {
                ZoomRatio = (float)Layout.Preferences.MapWidth / (float)(Layout.Width - Layout.LeftStart - Layout.RightStart);
                ZoomRatio = Math.Min(ZoomRatio, (float)Layout.Preferences.MapWidth / (float)Layout.Height);

                int NewWidth = (int)(Layout.Width * ZoomRatio);
                int NewHeight = (int)(Layout.Height * ZoomRatio);

                NewX = Layout.Parent.Width - NewWidth - 10 - Layout.Location.X - Layout.RightStart;
                NewY = 10 - Layout.Location.Y;

                //Layout
                e.DrawRectangle(Hash, new Rectangle(NewX, NewY, NewWidth, NewHeight));

                //Nodes
                foreach (Node Node in Layout.Manager.CurCompound.Nodes)
                {
                    e.FillRectangle(NodeBrush, new Rectangle(NewX + (int)(Node.UIx * Layout.LayoutSize * ZoomRatio), NewY + (int)(Node.UIy * Layout.LayoutSize * ZoomRatio), (int)(Node.UIWidth * Layout.LayoutSize * ZoomRatio), (int)(Node.UIHeight * Layout.LayoutSize * ZoomRatio)));
                }

                //Visible part
                if (Layout.Parent.Width - Layout.LeftStart - Layout.RightStart  < Layout.Width || Layout.Parent.Height < Layout.Height)
                {
                    LeftStart = NewX - (int)((Layout.Location.X - Layout.LeftStart) * ZoomRatio);
                    TopStart = NewY - (int)(Layout.Location.Y * ZoomRatio);

                    VisWidth = (int)((Layout.Parent.Width - Layout.LeftStart - Layout.RightStart) * ZoomRatio);
                    VisHeight = (int)(Layout.Parent.Height * ZoomRatio);

                    e.DrawRectangle(Hash, new Rectangle(LeftStart, TopStart , VisWidth, VisHeight));

                    int LeftArea = (int)((float)(-Layout.Location.X + Layout.LeftStart) * ZoomRatio);
                    if (LeftArea > 1)
                    {
                        e.FillRectangle(TransBrush, new Rectangle(NewX + 1, NewY + 1, LeftArea, NewHeight - 1));
                    }

                    int RightArea = NewWidth - VisWidth - LeftArea;
                    if (RightArea > 1)
                    {
                        e.FillRectangle(TransBrush, new Rectangle(NewX + LeftArea + VisWidth + 1, NewY + 1, RightArea, NewHeight - 1));
                    }

                    int TopArea = (int)((float)-Layout.Location.Y * ZoomRatio);
                    if (TopArea > 1)
                    {
                        e.FillRectangle(TransBrush, new Rectangle(NewX + LeftArea + 1, NewY + 1, VisWidth, TopArea));
                    }

                    int BottomArea = NewHeight - VisHeight - TopArea;
                    if (BottomArea > 1)
                    {
                        e.FillRectangle(TransBrush, new Rectangle(NewX + LeftArea + 1, NewY + VisHeight + TopArea, VisWidth, BottomArea));
                    }
                }
            }
        }
        
        public GraphicsPath DrawArrow(Graphics graphics, Pen inPen, Brush inBrush, Point point, Point point_2, double Size, bool Selected, LinkState state, bool Hovered)
        {
            return DrawArrow(graphics, inPen, inBrush, point, point_2, Size, Selected, state, false, 0, Hovered);
        }

        public GraphicsPath DrawArrow(Graphics graphics, Pen inPen, Brush inBrush, Point point, Point point_2, double Size, bool Selected, LinkState state, bool cycling, int YOffset, bool Hovered)
        {
            GraphicsPath path = null;
            double hyp = Math.Sqrt(Math.Pow(point_2.Y - point.Y, 2) + Math.Pow(point.X - point_2.X, 2));
            if (hyp > 0)
            {
                Point Handle1 = new Point((int)((4 * point_2.X + point.X) / 5.0), point.Y);
                Point Handle2 = new Point((int)((4 * point.X + point_2.X) / 5.0), point_2.Y);

                if (cycling)
                {
                    int Y = point.Y;
                    int Y_2 = point.Y + Math.Abs(point.Y - point_2.Y);

                    if (point.Y > point_2.Y)
                    {
                        Y = point_2.Y;
                        Y_2 = point_2.Y + Math.Abs(point.Y - point_2.Y);
                    }

                    Y += (int)(YOffset * Size);
                    Y_2 += (int)(YOffset * Size);

                    Handle1 = new Point((int)(point.X + 100 * Size), Y);
                    Handle2 = new Point((int)(point_2.X - 100 * Size), Y_2);
                }

                if (Selected)
                {
                    path = new GraphicsPath();
                    path.AddBezier(point, Handle1, Handle2, point_2);
                    graphics.DrawPath(Layout.FatPen, path);
                    //graphics.DrawBezier(Layout.FatPen, point, Handle1, Handle2, point_2);
                }

                //if (Hovered == true && path != null)
                //{
                //    Console.WriteLine("je suis dans Draw 1");
                //    Pen pe = new Pen(Color.Blue, 2);
                //    graphics.DrawPath(pe, path);
                //}
                //if (Hovered == false && path != null)
                //{
                //    Console.WriteLine("je suis dans Draw 2");
                //    Pen pe = new Pen(Color.Red, 1);
                //    graphics.DrawPath(pe, path);
                //}

                //condition ? true : false

                path = new GraphicsPath();
                path.AddBezier(point, Handle1, Handle2, point_2);
                graphics.DrawPath(Hovered ? Layout.HoverPen : inPen, path);

                //graphics.DrawBezier(inPen, point, Handle1, Handle2, point_2);

                //if (Size > 0)
                //{
                //    int arrowSize = (int)(12 * Size);

                //    //Start
                //    if (state.StartArrow != LinksArrows.None)
                //    {
                //        DrawLinkBound(graphics, state.StartArrow, inBrush, point, arrowSize, 1);
                //    }

                //    //End
                //    if (state.EndArrow != LinksArrows.None)
                //    {
                //        DrawLinkBound(graphics, state.EndArrow, inBrush, point_2, arrowSize, -1);
                //    }
                //}
            }
            return path;
        }
        private void DrawLinkBound(Graphics graphics, LinksArrows inArrow, Brush inBrush, Point inPoint, int inArrowSize, int inSign)
        {
            Point ArrowUp = new Point(inPoint.X + (inArrowSize * inSign), inPoint.Y + Math.Max(1, inArrowSize / 2));
            Point ArrowDown = new Point(inPoint.X + (inArrowSize * inSign), inPoint.Y - Math.Max(1, inArrowSize / 2));
            Point ArrowCenter = new Point(inPoint.X + (int)Math.Max(1, (double)inArrowSize * .7) * inSign, inPoint.Y);

            switch(inArrow)
            {
                case LinksArrows.Lock:
                    graphics.FillRectangle(inBrush, inPoint.X + (inSign * 3 * (inArrowSize / 4)) - (inArrowSize / 4), inPoint.Y - (inArrowSize / 2), inArrowSize / 2, inArrowSize);
                    //graphics.FillClosedCurve(inBrush, new Point[] { ArrowUp, ArrowCenter, ArrowDown });
                    break;
                case LinksArrows.SolidArrow:

                    graphics.FillPolygon(inBrush, new Point[] { ArrowUp, ArrowCenter, ArrowDown });
                    break;
                case LinksArrows.Scaling:
                    graphics.FillRectangle(inBrush, inPoint.X + (inSign * 4 * (inArrowSize / 4)) - (inArrowSize / 6), inPoint.Y - (inArrowSize / 2), inArrowSize / 3, inArrowSize / 3);
                    graphics.FillRectangle(inBrush, inPoint.X + (inSign * 4 * (inArrowSize / 3)) - (inArrowSize / 4), inPoint.Y - inArrowSize, inArrowSize/2, inArrowSize/2);
                    break;
                default:

                    graphics.FillPolygon(inBrush, new Point[] { inPoint, ArrowUp, ArrowCenter, ArrowDown });
                    break;
            }
        }

        internal void DrawPolygon(Graphics graphics, Brush inBrush, Point[] point)
        {
            graphics.FillPolygon(inBrush, point);
        }
    }
}
