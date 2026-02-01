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
 ****************************************************************************/using System;

using Calcita.Common;
using Calcita.Graphics;

namespace Calcita.Rendering
{
    /// <summary>
    /// Draw borders at the specified location.
    /// </summary>
    sealed class BorderPainter : IDisposable
    {
        private static BorderPainter instance;

        /// <summary>
        /// Get BorderPainter instance
        /// </summary>
        public static BorderPainter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BorderPainter();
                }

                return instance;
            }
        }


        private readonly RGPen[] pens = new RGPen[14];

        private BorderPainter()
        {

            RGPen p;
            static RGPen CreatePen(Avalonia.Media.IBrush color, double thickness, RGDashStyle dashStyle)
            {
                return new RGPen(color, 1)
                {
                    DashStyle = dashStyle
                };
            }

            // Solid
            p = CreatePen(RGPenColor.Black, 1, RGDashStyles.Solid);
            pens[(byte)BorderLineStyle.Solid] = p;

            // Dahsed
            p = CreatePen(RGPenColor.Black, 1, RGDashStyles.Dash);
            pens[(byte)BorderLineStyle.Dashed] = p;

            // Dotted
            p = CreatePen(RGPenColor.Black, 1, RGDashStyles.Dot);
            pens[(byte)BorderLineStyle.Dotted] = p;

            // DoubleLine
            p = CreatePen(RGPenColor.Black, 1, RGDashStyles.Solid);
            pens[(byte)BorderLineStyle.DoubleLine] = p;

            // Dashed2
            p = CreatePen(RGPenColor.Black, 1, RGDashStyles.Solid);
            pens[(byte)BorderLineStyle.Dashed2] = p;

            // DashDot
            p = CreatePen(RGPenColor.Black, 1, RGDashStyles.DashDot);
            pens[(byte)BorderLineStyle.DashDot] = p;

            // DashDotDot
            p = CreatePen(RGPenColor.Black, 1, RGDashStyles.DashDotDot);
            pens[(byte)BorderLineStyle.DashDotDot] = p;

            // BoldDashDot
            p = CreatePen(RGPenColor.Black, 2, RGDashStyles.DashDot);
            pens[(byte)BorderLineStyle.BoldDashDot] = p;

            // BoldDashDotDot
            p = CreatePen(RGPenColor.Black, 2, RGDashStyles.DashDotDot);
            pens[(byte)BorderLineStyle.BoldDashDotDot] = p;

            // BoldDotted
            p = CreatePen(RGPenColor.Black, 2, RGDashStyles.Dot);
            pens[(byte)BorderLineStyle.BoldDotted] = p;

            // BoldDashed
            p = CreatePen(RGPenColor.Black, 2, RGDashStyles.Dash);
            pens[(byte)BorderLineStyle.BoldDashed] = p;

            // BoldSolid
            p = CreatePen(RGPenColor.Black, 2, RGDashStyles.Solid);
            pens[(byte)BorderLineStyle.BoldSolid] = p;

            // BoldSolidStrong
            p = CreatePen(RGPenColor.Black, 3, RGDashStyles.Solid);
            pens[(byte)BorderLineStyle.BoldSolidStrong] = p;

        }

        /// <summary>
        /// Draw border at specified location
        /// </summary>
        /// <param name="g">instance for graphics object</param>
        /// <param name="x">x coordinate of start point</param>
        /// <param name="y">y coordinate of start point</param>
        /// <param name="x2">x coordinate of end point</param>
        /// <param name="y2">y coordinate of end point</param>
        /// <param name="style">style instance of border</param>
        public void DrawLine(PlatformGraphics g, RGFloat x, RGFloat y, RGFloat x2, RGFloat y2, RangeBorderStyle style)
        {
            DrawLine(g, x, y, x2, y2, style.Style, style.Color);
        }


        /// <summary>
        /// Draw border at specified position.
        /// </summary>
        /// <param name="g">Instance for graphics object.</param>
        /// <param name="x">X coordinate of start point.</param>
        /// <param name="y">Y coordinate of start point.</param>
        /// <param name="x2">X coordinate of end point.</param>
        /// <param name="y2">Y coordinate of end point.</param>
        /// <param name="style">Style flag of border.</param>
        /// <param name="color">Color of border.</param>
        /// <param name="bgPen">Fill pen used when drawing double outline.</param>
        public void DrawLine(PlatformGraphics g, RGFloat x, RGFloat y, RGFloat x2, RGFloat y2, BorderLineStyle style,
            SolidColor color, RGPen bgPen = null)
        {
            if (style == BorderLineStyle.None) return;

            // get template pen from cache list
            var tp = pens[(byte)style];

            // create new WPF pen
            var p = new RGPen(new RGSolidBrush(color), tp.Thickness);
            // copy the pen style from template
            p.DashStyle = tp.DashStyle;
            p.LineCap = Avalonia.Media.PenLineCap.Square;
            g.DrawLine(p, new RGPointF(x, y), new RGPointF(x2, y2));


            if (style == BorderLineStyle.DoubleLine && bgPen != null)
            {
                lock (bgPen)
                {
                    g.DrawLine(bgPen, new RGPointF(x, y), new RGPointF(x2, y2));
                }
            }
        }

        /// <summary>
        /// Release all cached objects.
        /// </summary>
        public void Dispose()
        {
        }

    }
}


