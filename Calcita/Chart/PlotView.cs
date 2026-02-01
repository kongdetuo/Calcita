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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Calcita.Data;
using Calcita.Drawing;
using Calcita.Graphics;
using Calcita.Rendering;

namespace Calcita.Chart
{
	/// <summary>
	/// Chart Plot View 
	/// </summary>
	public interface IPlotView : IDrawingObject
	{
	}

	/// <summary>
	/// Represents common chart plot view.
	/// </summary>
	public class ChartPlotView : DrawingObject, IPlotView
	{
		/// <summary>
		/// Get or set the owner chart to this plot view.
		/// </summary>
		public Chart Chart { get; set; }

		/// <summary>
		/// Create common chart plot view object.
		/// </summary>
		/// <param name="chart">Owner chart instance.</param>
		public ChartPlotView(Chart chart)
		{
			this.Chart = chart;

			this.FillColor = SolidColor.Transparent;
			this.LineColor = SolidColor.Transparent;
		}
	}


}

#endif // DRAWING



