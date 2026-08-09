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
 ****************************************************************************/using Calcita.Graphics;
using Calcita.Rendering;
using Calcita.Interaction;

namespace Calcita.Views
{
	class LeadHeaderView : View
	{
		protected Worksheet sheet;

		public LeadHeaderView(ViewportController vc)
			: base(vc)
		{
			this.sheet = vc.worksheet;
		}

		#region Draw
		public override void Draw(CellDrawingContext dc)
		{
			if (bounds.Width <= 0 || bounds.Height <= 0 || sheet.controlAdapter == null) return;

			var g = dc.Graphics;
			var controlStyle = dc.Renderer.ControlStyle;

			g.FillRectangle(bounds, controlStyle.GetBrush(ControlAppearanceColors.LeadHeadNormal));

			var indicatorBrush = sheet.isLeadHeadSelected ?
					controlStyle.GetBrush(ControlAppearanceColors.LeadHeadIndicator)
					: controlStyle.GetBrush(ControlAppearanceColors.LeadHeadSelected);

			dc.Renderer.DrawLeadHeadArrow(bounds, indicatorBrush);
		}
		#endregion // Draw

		public override bool OnMouseDown(Point location, MouseButtons buttons)
		{
			// mouse down in LeadHead?
			switch (this.sheet.operationStatus)
			{
				case OperationStatus.Default:
					if (this.sheet.selectionMode != WorksheetSelectionMode.None)
					{
						this.sheet.SelectRange(RangePosition.EntireRange);

						// show context menu
						if (buttons == MouseButtons.Right)
						{
							this.sheet.controlAdapter.ShowContextMenuStrip(ViewTypes.LeadHeader, location);
						}

						return true;
					}
					break;
			}

			return false;
		}

		public override bool OnMouseMove(Point location, MouseButtons buttons)
		{
			this.sheet.controlAdapter.ChangeCursor(CursorStyle.Selection);

			return false;
		}
	}
}


