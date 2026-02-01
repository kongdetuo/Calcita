/*****************************************************
 * 
 * ReoGrid - .NET Spreadsheet Control
 * 
 * https://reogrid.net/
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * Author: Jingwood <jingwood at unvell.com>
 *
 * Copyright (c) 2012-2025 Jingwood <jingwood at unvell.com>
 * Copyright (c) 2012-2025 UNVELL Inc. All rights reserved.
 * 
 ****************************************************************************/
#if DRAWING



using Calcita.Graphics;
using System;
using DrawingContext = Calcita.Rendering.DrawingContext;

namespace Calcita.Drawing.Shapes
{

    #region Path
    /// <summary>
    /// Represents path shape drawing object.
    /// </summary>
    public abstract class PathShape : ShapeObject
    {
        public override void OnBoundsChanged(Graphics.Rectangle oldRect)
        {
            base.OnBoundsChanged(oldRect);

            if (Width > 0 && Height > 0)
            {
                UpdatePath();
            }
        }


        protected Avalonia.Media.PathGeometry Path = new Avalonia.Media.PathGeometry();


        protected abstract void UpdatePath();

        /// <summary>
        /// Render path shape to graphics context.
        /// </summary>
        /// <param name="dc">Platform no-associated drawing context instance.</param>
        protected override void OnPaint(DrawingContext dc)
        {
            var g = dc.Graphics;

            if (!this.FillColor.IsTransparent)
            {
                g.FillPath(this.FillColor, this.Path);
            }

            if (!this.LineColor.IsTransparent)
            {
                g.DrawPath(this.LineColor, this.Path);
            }

            base.OnPaintText(dc);
        }

    }
    #endregion // Path

    #region Rounded Rectangle
    /// <summary>
    /// Represents a rounded rectangle shape.
    /// </summary>
    public class RoundedRectangleShape : PathShape
    {
        private RGFloat roundRate = 0.2f;

        /// <summary>
        /// Get or set the rounded corner rate relative to the minimum value between width and height. (0.0f ~ 1.0f)
        /// </summary>
        public RGFloat RoundRate
        {
            get { return roundRate; }
            set
            {
                if (this.roundRate != value)
                {
                    this.roundRate = value;
                    this.Invalidate();
                }
            }
        }

        protected override void UpdatePath()
        {
            RGFloat min = Math.Min(Width, Height);
            RGFloat c = roundRate * min;

            // todo Avalonia


            Path.Figures.Clear();

            if (c > 0)
            {
                var rectangle = new Avalonia.Controls.Shapes.Rectangle
                { Width = Width, Height = Height, RadiusX = c, RadiusY = c };


                // Path.Add(rectangle.C );
            }
            else
            {
                //  Path.Figures.Add();
            }
        }

        protected override Rectangle TextBounds
        {
            get
            {
                RGFloat min = Math.Min(Width, Height) / 4;
                RGFloat c = roundRate * min;

                var rect = base.TextBounds;
                rect.Inflate(-c, -c);

                return rect;
            }
        }
    }
    #endregion // Rounded Rectangle

    #region Pie
    /// <summary>
    /// Represents a pie shape 
    /// </summary>
    public class PieShape : PathShape
    {
        #region Attributes
        private RGFloat startAngle = 0;

        /// <summary>
        /// Get or set the start angle of pie shape
        /// </summary>
        public virtual RGFloat StartAngle
        {
            get { return this.startAngle; }
            set
            {
                if (this.startAngle != value)
                {
                    this.startAngle = value;
                    this.UpdatePath();
                }
            }
        }

        private RGFloat sweepAngle = 30;

        /// <summary>
        /// Get or set the sweep angle of pie shape (Sweep from start angle)
        /// </summary>
        public virtual RGFloat SweepAngle
        {
            get { return this.sweepAngle; }
            set
            {
                if (this.sweepAngle != value)
                {
                    this.sweepAngle = value;
                    this.UpdatePath();
                }
            }
        }
        #endregion // Attributes

        protected override void UpdatePath()
        {
            var clientRect = this.ClientBounds;

            Path.Figures.Clear();

            if (this.sweepAngle > 0)
            {
                Avalonia.Media.PathFigure pf = new Avalonia.Media.PathFigure();

                pf.Segments.Add(new Avalonia.Media.LineSegment { Point = this.OriginPoint });
                pf.Segments.Add(new Avalonia.Media.ArcSegment
                {
                    Point = new Avalonia.Point(0, 0),
                    Size = new Avalonia.Size(this.Width, this.Height),
                    RotationAngle = this.sweepAngle,
                    IsLargeArc = true,
                    SweepDirection = Avalonia.Media.SweepDirection.Clockwise
                });

                Path.Figures.Add(pf);
            }
        }
    }
    #endregion // Pie

}

#endif // DRAWING



