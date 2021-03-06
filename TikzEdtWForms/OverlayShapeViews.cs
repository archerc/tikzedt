﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TikzEdt.Overlay;
using System.Windows.Forms;
using System.Drawing;
using TikzEdt;

namespace TikzEdtWForms
{

    /// <summary>
    /// Provides drawing and hit test methods for overlay shapes
    /// </summary>
    public static class OverlayShapeExtensions
    {
        public static void Draw(this OverlayShapeVM os, Graphics dc, int Height)
        {

                if (os is OverlayScope)
                    (os as OverlayScope).Draw(dc, Height);
                else if (os is OverlayControlPoint)
                    (os as OverlayControlPoint).Draw(dc, Height);
                else if (os is OverlayNode)
                    (os as OverlayNode).Draw(dc, Height);

        }


        public static void Draw(this OverlayControlPoint ocp, Graphics dc, int Height)
        {
            Pen p = ocp.IsSelected ? PensAndBrushes.OverlayNodeSelPen : PensAndBrushes.OverlayNodePen;
            RectangleF lBB = ocp.BB.UpsideDown(Height).ToRectangleF();

            // draw lines
            if (ocp.Origin1 != null)
            {
                var L1Origin = ocp.Origin1.Center.UpsideDown(Height);
                dc.DrawLine(PensAndBrushes.DashedPen, L1Origin.ToPointF(), lBB.Center());
            }
            if (ocp.Origin2 != null)
            {
                var L2Origin = ocp.Origin2.Center.UpsideDown(Height);
                dc.DrawLine(PensAndBrushes.DashedPen, L2Origin.ToPointF(), lBB.Center());
            }

            // draw CP
            dc.FillEllipse(Brushes.Gray, lBB);
            dc.DrawEllipse(p, lBB);
        }


        public static void Draw(this OverlayScope os, Graphics dc, int Height)
        {
            RectangleF lBB = os.BB.UpsideDown(Height).ToRectangleF();
            Pen p = os.IsSelected ? PensAndBrushes.OverlayScopeSelPen : PensAndBrushes.OverlayScopePen;

            dc.DrawRectangle(p, lBB.X, lBB.Y, lBB.Width, lBB.Height);

            // todo: draw adorner
        }

        public static void Draw(this OverlayNode on, Graphics dc, int Height)
        {
            Pen p = on.IsSelected ? PensAndBrushes.OverlayNodeSelPen : PensAndBrushes.OverlayNodePen;
            RectangleF lBB = on.BB.UpsideDown(Height).ToRectangleF();
            dc.DrawLine(p, lBB.TopLeft(), lBB.BottomRight());
            dc.DrawLine(p, lBB.BottomLeft(), lBB.TopRight());
        }


        /// <summary>
        /// Tests whether the point p (in TL centric coordinates) lies within the object.
        /// If yes returns a "distance", based on which the object is selected. (smallest distance wins)
        /// If no returns a large value (1000000)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double HitTest(this OverlayShapeVM os, double x, double y, double Height)
        {
            // no overloading for extensions in c# -> we have to forward the call
            if (os is OverlayScope)
                return (os as OverlayScope).HitTest(x,y,Height);

            var lBB = os.BB.UpsideDown(Height);
            if (lBB.Contains(x, y))
            {
                return (new System.Windows.Point(x, y) - lBB.Center()).Length;
            }
            else return 1000000;
        }


        public static double HitTest(this OverlayScope os, double x, double y, double Height)
        {
            var lBB = os.BB.UpsideDown(Height);
            var lBBs = os.BB.UpsideDown(Height);
            lBBs.Inflate(10, 10);
            if (lBB.Contains(x, y) && !lBBs.Contains(x, y))
                return 10;
            else
                return 1000000;
        }

    }

    /*
    abstract class OverlayShapeView : IOverlayShapeView
    {
        /// <summary>
        /// Stores the underlying OverlayShape displayed by this view.
        /// </summary>
        public OverlayShapeVM TheUnderlyingShape { get; set; }

        /// <summary>
        ///  In upside down coordinates!
        /// </summary>
        protected System.Windows.Rect BB = new System.Windows.Rect(0,0,0,0);

        protected Control Parent;

        // in upside down coordinates
        public virtual System.Windows.Rect GetBB(double CanvasHeight)
        {
            return new System.Windows.Rect(BB.X, Parent.Height - BB.Y - BB.Height, BB.Width, BB.Height);
        }

        public string ToolTip { get; set; }
        public void SetToolTip(string Text)
        {
            this.ToolTip = Text;
        }


        /// <summary>
        /// Tests whether the point p (in TL centric coordinates) lies within the object.
        /// If yes returns a "distance", based on which the object is selected. (smallest distance wins)
        /// If no returns a large value (1000000)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual double HitTest(double x, double y)
        {
            var lBB = GetBB(Parent.Height);
            if (lBB.Contains(x, y))
            {
                return (new System.Windows.Point(x, y) - lBB.Center()).Length;
            }
            else return 1000000;
        }

        //double ParentHeight { get { return (Parent as Canvas).Height; } }
        //double ParentWidth { get { return (Parent as Canvas).Width; } }


        public virtual double GetLeft()
        {
            return BB.X + BB.Width / 2;
        }

        public virtual double GetBottom()
        {
            return BB.Y + BB.Height / 2;
        }

        public virtual void SetPosition(double Left, double Bottom, bool Relative = false)
        {
            if (Relative)
            {
                BB.X = BB.X + Left;
                BB.Y = BB.Y + Bottom;
            }
            else
            {
                BB.X = Left - BB.Width / 2;
                BB.Y = Bottom - BB.Height / 2;
            }
            Parent.Invalidate();
        }

        protected bool IsSelected = false;
        public void SetStdColor() { IsSelected = false; Parent.Invalidate(); }
        public void SetSelColor() { IsSelected = true; Parent.Invalidate(); }

        protected abstract Pen StdPen { get ; }
        protected abstract Pen SelPen { get; }

        public OverlayShapeView(Control Parent)
        {
            this.Parent = Parent;
        }

        public abstract void Draw(Graphics dc);
    }

    class OverlayNodeView : OverlayShapeView
    {
       
        /// <summary>
        /// Draw an X
        /// </summary>
        /// <param name="dc"></param>
        public override void Draw(Graphics dc)
        {
            Pen p = IsSelected ? SelPen : StdPen;
            RectangleF lBB = GetBB(Parent.Height).ToRectangleF();
            dc.DrawLine(p, lBB.TopLeft(), lBB.BottomRight());
            dc.DrawLine(p, lBB.BottomLeft(), lBB.TopRight());
        }

        public OverlayNodeView(Control Parent) : base(Parent)
        {
            BB.Width = 10;
            BB.Height = 10;

        }

        protected override Pen SelPen
        {
            get { return PensAndBrushes.OverlayNodeSelPen; }
        }
        protected override Pen StdPen
        {
            get { return PensAndBrushes.OverlayNodePen; }
        }
    }

    class OverlayScopeView : OverlayShapeView, IOverlayScopeView
    {

        public override void SetPosition(double Left, double Bottom, bool Relative = false)
        {
            if (Relative)
            {
                BB.X = BB.X + Left;
                BB.Y = BB.Y + Bottom;
            }
            else
            {
                BB.X = Left;
                BB.Y = Bottom;
            }
            Parent.Invalidate();
        }

        public OverlayScopeView(Control Parent) : base(Parent)
        {
        }
        protected override Pen SelPen
        {
            get { return PensAndBrushes.OverlayScopeSelPen; }
        }
        protected override Pen StdPen
        {
            get { return PensAndBrushes.OverlayScopePen; }
        }

        public override double HitTest(double x, double y)
        {
            var lBB = GetBB(Parent.Height);
            var lBBs = GetBB(Parent.Height);
            lBBs.Inflate(10,10);
            if (lBB.Contains(x, y) && lBBs.Contains(x, y))
                return 10;
            else
                return 1000000;
        }

        public void SetSize(double Width, double Height)
        {
            BB.Width = Width;
            BB.Height = Height;
        }

        public bool IsAdornerVisible = false;
        public void ShowAdorner()
        {
            IsAdornerVisible = true; Parent.Invalidate();
        }

        public void RemoveAdorner()
        {
            IsAdornerVisible = false; Parent.Invalidate();
        }

        public override double GetLeft()
        {
            return BB.X;
        }

        public override double GetBottom()
        {
            return BB.Y;
        }

        public override void Draw(Graphics dc)
        {
            RectangleF lBB = GetBB(Parent.Height).ToRectangleF();
            Pen p = IsSelected ? SelPen : StdPen;

            dc.DrawRectangle(p, lBB.X, lBB.Y, lBB.Width, lBB.Height);

            // todo: draw adorner
        }
    }

    class OverlayCPView : OverlayShapeView, IOverlayCPView
    {
        //public Line lineToOrigin1 = new Line() { Stroke = Brushes.Gray, StrokeDashArray = new DoubleCollection(new double[] { 4, 4 }) };
        //public Line lineToOrigin2 = new Line() { Stroke = Brushes.Gray, StrokeDashArray = new DoubleCollection(new double[] { 4, 4 }) };

        System.Windows.Point L1Origin, L2Origin;
        bool L1Visible = false, L2Visible = false;
        public OverlayCPView(Control Parent)
            : base(Parent)
        {
            BB.Width = 10;
            BB.Height = 10;
        }
        protected override Pen SelPen
        {
            get { return PensAndBrushes.OverlayNodeSelPen; }
        }
        protected override Pen StdPen
        {
            get { return PensAndBrushes.OverlayNodePen; }
        }

        public void SetOrigin1(double Left, double Top, double CanvasHeight)
        {
            L1Origin = new System.Windows.Point(Left, Top);
            L1Visible = true;
            Parent.Invalidate();
        }

        public void SetOrigin2(double Left, double Top, double CanvasHeight)
        {
            L2Origin = new System.Windows.Point(Left, Top);
            L2Visible = true;
            Parent.Invalidate();
        }


        static Pen DashedPen = PensAndBrushes.DashedPenGray;
        public override void Draw(Graphics dc)
        {
            Pen p = IsSelected ? SelPen : StdPen;
            RectangleF lBB = GetBB(Parent.Height).ToRectangleF();

            // draw lines
            if (L1Visible)
                dc.DrawLine(DashedPen, L1Origin.ToPointF(), lBB.Center());
            if (L2Visible)
                dc.DrawLine(DashedPen, L2Origin.ToPointF(), lBB.Center());

            // draw CP
            dc.FillEllipse(Brushes.Gray, lBB);
            dc.DrawEllipse(p, lBB);


        }


    }

    */
    public abstract class WFShapeBase : IPreviewShape
    {
        protected Control TheCanvas;
        
        public WFShapeBase(Control TheCanvas)
        {
            this.TheCanvas = TheCanvas;
        }

        bool _Visible = false;
        public bool Visible
        {
            get
            {
                return _Visible;
            }
            set
            {
                if (value != _Visible)
                {
                    _Visible = value;
                    TheCanvas.Invalidate();
                }
            }
        }

        protected System.Windows.Rect BB = new System.Windows.Rect(0,0,0,0);
        public System.Windows.Rect GetBB()
        {
            return BB;
        }


        private double _Rotation;

	    public double Rotation
	    {
		    get { return _Rotation;}
		    set { _Rotation = value; TheCanvas.Invalidate(); }
	    }

        public void Refresh()
        {
            TheCanvas.Invalidate();
        }

        public Pen ThePen = null;
        public Brush TheFill = null;

        public abstract void Draw(Graphics dc);
    }

    class WFRectangleShape : WFShapeBase, IRectangleShape
    {
        public WFRectangleShape(Control TheCanvas)
            : base(TheCanvas)
        { }

        public void SetPosition(double Left, double Top, double Width, double Height)
        {
            BB.X = Left;
            BB.Y = Top;
            BB.Width = Width;
            BB.Height = Height;
            //GlobalUI.UI.AddStatusLine(this, "setpos");
            Refresh();
        }

        /// <summary>
        /// in upside down coordinates
        /// </summary>
        protected System.Windows.Point Center = new System.Windows.Point(0, 0);
        public void SetCenter(double Left, double Bottom)
        {
            Center = new System.Windows.Point(Left, Bottom);
            // adjust BB
            BB.X = Center.X - BB.Width / 2;
            BB.Y = TheCanvas.Height - ( Center.Y + BB.Height / 2 );
        }

        
        public override void Draw(Graphics dc)
        {
            if (TheFill != null)
                dc.FillRectangle(TheFill, BB.ToRectangleF());
            if (ThePen != null)
            {
                var r = BB.ToRectangleF();
                dc.DrawRectangle(ThePen, r);
                //dc.DrawRectangle(Pens.Green, new RectangleF(20, 20, 200, 200));
                //GlobalUI.UI.AddStatusLine(this, "p " + BB.Width + " " + BB.Height + " " + r);
            }
        }
    }

    class WFEllipseShape : WFRectangleShape
    {
        public override void Draw(Graphics dc)
        {
            if (TheFill != null)
                dc.FillEllipse(TheFill, BB.ToRectangleF());
            if (ThePen != null)
                dc.DrawEllipse(ThePen, BB.ToRectangleF());
        }

        public WFEllipseShape(Control C) : base(C) { }
    }

    /// <summary>
    /// Describes a fan, i.e., a pie segment with multiple "spokes"
    /// 
    /// It describe by a center point Center, a Radius and the various spokes
    /// </summary>
    class WFFanshape : WFShapeBase, IFanShape
    {
        public WFFanshape(Control TheCanvas) : base(TheCanvas) { }

        public double R { get; set; }
        public System.Windows.Point Center { get; set; }
        public List<double> Spokes { get; set; }    // the angles of the spokes, in radians


        //public double R { get { return TheShape.R; } set { TheShape.R = value; } }
        //public Point Center { get { return TheShape.Center; } set { TheShape.Center = value; } }
        //public List<double> Spokes { get { return TheShape.Spokes; } set { TheShape.Spokes = value; } }    // the angles of the spokes, in radians

        /// <summary>
        /// Draw an arc
        /// </summary>
        /// <param name="dc"></param>
        public override void Draw(Graphics dc)
        {
            if (R == 0 || Spokes == null || Spokes.Count < 2 || ThePen == null)
                return;

            dc.DrawLine(ThePen, Center.ToPointF(), spokep(0));

            for (int i = 1; i < Spokes.Count; i++)
            {
                if (Math.Abs(Spokes[i] - Spokes[i - 1]) > 2 * Math.PI - .001)
                {
                    // Display a circle
                    double ControlPointRatio = (Math.Sqrt(2) - 1) * 4 / 3;

                    var x0 = (float)(Center.X - R);
                    var x1 = (float)(Center.X - R * ControlPointRatio);
                    var x2 = (float)(Center.X);
                    var x3 = (float)(Center.X + R * ControlPointRatio);
                    var x4 = (float)(Center.X + R);

                    var y0 = (float)( Center.Y - R);
                    var y1 = (float)(Center.Y - R * ControlPointRatio);
                    var y2 = (float)(Center.Y);
                    var y3 = (float)(Center.Y + R * ControlPointRatio);
                    var y4 = (float)(Center.Y + R);

                    PointF[] pts = new PointF[] { new PointF(x2, y0), new PointF(x3, y0), new PointF(x4, y1), 
                        new PointF(x4, y2), new PointF(x1, y4), new PointF(x0, y3), new PointF(x0, y2),
                        new PointF(x0, y1), new PointF(x1, y0), new PointF(x2, y0) };

                    dc.DrawBeziers(ThePen, pts);

                    /* context.BeginFigure(new Point(x2, y0), true, true);
                    context.BezierTo(new Point(x3, y0), new Point(x4, y1), new Point(x4, y2), true, true);
                    context.BezierTo(new Point(x4, y3), new Point(x3, y4), new Point(x2, y4), true, true);
                    context.BezierTo(new Point(x1, y4), new Point(x0, y3), new Point(x0, y2), true, true);
                    context.BezierTo(new Point(x0, y1), new Point(x1, y0), new Point(x2, y0), true, true); */

                }
                else
                {
                    bool largearc = Math.Abs(Spokes[i] - Spokes[i - 1]) > Math.PI;
                   /* SweepDirection sd = SweepDirection.Counterclockwise;
                    if (Spokes[i] < Spokes[i - 1])
                        sd = SweepDirection.Clockwise;

                    dc.DrawArc(ThePen, (float)Center.X, (float(Center.Y), (float)R, (float)R, )
                    context.ArcTo(spokep(i), new Size(R, R), 0, largearc, sd, true, false);*/
                    //TODO:support
                }

                dc.DrawLine(ThePen, Center.ToPointF(), spokep(i));

            }
        }


        PointF spokep(int i)
        {
            return (Center + R * (new System.Windows.Vector(Math.Cos(Spokes[i]), -Math.Sin(Spokes[i])))).ToPointF();
        }
    }



    class WFPreviewGridShape : WFRectangleShape
    {

        static Pen DashedPen = PensAndBrushes.DashedPen;

        /// <summary>
        /// Draw a Grid
        /// </summary>
        /// <param name="dc"></param>
        public override void Draw(Graphics dc)
        {
            RectangleF lBB = GetBB().ToRectangleF();
            dc.DrawRectangle(DashedPen, lBB);

            dc.DrawLine(DashedPen, lBB.X + lBB.Width / 2, lBB.Y, lBB.X + lBB.Width / 2, lBB.Y + lBB.Height);
            dc.DrawLine(DashedPen, lBB.X, lBB.Y + lBB.Height/2, lBB.X + lBB.Width , lBB.Y + lBB.Height/2);
        }

        public WFPreviewGridShape(Control C) : base(C) { }
    }

    class WFArcShape : WFShapeBase, IArcShape
    {
        public System.Windows.Point p1  {get; set;}
        public System.Windows.Point p2  {get; set;}
        public System.Windows.Point center {get; set;}
        public bool IsPie { get; set; }
        public bool IsLargeArc { get; set; }

        double r { get { return (p1 - center).Length; } }

        public bool IsDashed
        {
            get;
            set;
        }

        public WFArcShape(Control TheCanvas)
            : base(TheCanvas)
        {
            ThePen = Pens.Black;
        }

        public override void Draw(Graphics dc)
        {
            Pen p = IsDashed ? PensAndBrushes.DashedPen : ThePen;
            float R = (float) (center-p1).Length;   // the radius of the circle
            var diag = new System.Windows.Vector(R,R);
            RectangleF re = new System.Windows.Rect(center+diag, center-diag).ToRectangleF();  // BB of the circle
            System.Windows.Vector v1 = p1 - center, v2 = p2 - center;


            if (IsPie)
            {
                dc.DrawLine(p, center.ToPointF(), p1.ToPointF());
                dc.DrawLine(p, center.ToPointF(), p2.ToPointF());
            }

            float angle2 = (float)( Math.Atan2(v2.Y, v2.X) *180 / Math.PI);
            float angle1 = (float)( Math.Atan2(v1.Y, v1.X) *180 / Math.PI);
            float angle = angle2 - angle1;
            float bngle = angle + (angle>0?-360:360);


            if (     ( IsLargeArc && Math.Abs(angle) < 180)
                 ||  (!IsLargeArc && Math.Abs(angle) > 180) )
                angle = bngle;

            if (re.Height * re.Width > 0)
                dc.DrawArc(p, re, angle1, angle );
            /* TODO
            context.BeginFigure(p1, false, IsPie);
            
            SweepDirection sd;
            if ((v1.X * v2.Y - v1.Y * v2.X > 0) != LargeArc)
                sd = SweepDirection.Clockwise;
            else
                sd = SweepDirection.Counterclockwise;

            context.ArcTo(p2, new Size(r, r), 0, LargeArc, sd, true, false);*/

            
        }
    }

    public static class PensAndBrushes
    {
        public static Pen DashedPen;
        public static Pen OverlayScopePen;
        public static Pen OverlayScopeSelPen;
        public static Pen OverlayNodePen;
        public static Pen OverlayNodeSelPen;
        public static Pen DashedPenGray;
        public static Brush SelectionRectFill;
        public static Pen AdornerPen;


        static PensAndBrushes()
        {
            CreatePens();
            DashedPen = new Pen(Brushes.Black) { DashPattern = new float[] { 4, 4 } };
            DashedPenGray = new Pen(Brushes.Gray) { DashPattern = new float[] { 4, 4 } };
            SelectionRectFill = new SolidBrush(Color.FromArgb(0x23, 0x00, 0x8A, 0xCA));
            AdornerPen = new Pen(new System.Drawing.Drawing2D.HatchBrush(System.Drawing.Drawing2D.HatchStyle.ForwardDiagonal, Color.Black, Color.White), 5);

            Properties.Settings.Default.PropertyChanged += (s, e) =>
                {
                    var names = new string[] { "Overlay_ScopeColor", "Overlay_ScopeSelColor", "Overlay_CoordColor", "Overlay_CoordSelColor" };
                    if (names.Contains(e.PropertyName))
                        PensAndBrushes.RecreatePens();
                };
        }


        static void CreatePens()
        {
            OverlayScopePen = new Pen(Properties.Settings.Default.Overlay_ScopeColor, 5);
            OverlayScopeSelPen = new Pen(Properties.Settings.Default.Overlay_ScopeSelColor, 5);
            OverlayNodePen = new Pen(Properties.Settings.Default.Overlay_CoordColor);
            OverlayNodeSelPen = new Pen(Properties.Settings.Default.Overlay_CoordSelColor);
        }

        static void DisposePens()
        {
            //if (DashedPen != null)          { DashedPen.Dispose(); DashedPen = null; }
            //if (DashedPenGray != null)      { DashedPenGray.Dispose(); DashedPenGray = null; }
            if (OverlayScopePen != null)    { OverlayScopePen.Dispose(); OverlayScopePen = null; }
            if (OverlayScopeSelPen != null) { OverlayScopeSelPen.Dispose(); OverlayScopeSelPen = null; }
            if (OverlayNodePen != null)     { OverlayNodePen.Dispose(); OverlayNodePen = null; }
            if (OverlayNodeSelPen != null)  { OverlayNodeSelPen.Dispose(); OverlayNodeSelPen = null; }
        }

        public static void RecreatePens()
        {
            DisposePens(); 
            CreatePens();
        }


    }

}
