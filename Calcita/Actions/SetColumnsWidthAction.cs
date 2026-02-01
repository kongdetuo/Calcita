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
 ****************************************************************************/using System.Collections.Generic;

namespace Calcita.Actions
{
	/// <summary>
	/// Action for adjusting columns width.
	/// </summary>
	public class SetColumnsWidthAction : WorksheetReusableAction
	{
		private ushort width;

		/// <summary>
		/// Width to be set
		/// </summary>
		public ushort Width { get { return width; } set { width = value; } }

		/// <summary>
		/// Create instance for SetColsWidthAction
		/// </summary>
		/// <param name="col">Index of column start to set</param>
		/// <param name="count">Number of columns to be set</param>
		/// <param name="width">Width of column to be set</param>
		public SetColumnsWidthAction(int col, int count, ushort width)
			: base(new RangePosition(0, col, -1, count))
		{
			this.width = width;
		}

		/// <summary>
		/// Do this action
		/// </summary>
		public override void Do()
		{
			int col = base.Range.Col;
			int count = base.Range.Cols;

			backupCols.Clear();

			int c2 = col + count;
			for (int c = col; c < c2; c++)
			{
				ColumnHeader colHead = Worksheet.RetrieveColumnHeader(c);
				backupCols.Add(c, colHead.InnerWidth);
			}

			Worksheet.SetColumnsWidth(col, count, width);
		}

		private Dictionary<int, ushort> backupCols = new Dictionary<int, ushort>();

		/// <summary>
		/// Undo this action
		/// </summary>
		public override void Undo()
		{
			int col = base.Range.Col;
			int count = base.Range.Cols;

			Worksheet.SetColumnsWidth(col, count, c => backupCols[c]);
		}

		/// <summary>
		/// Get friendly name of this action
		/// </summary>
		/// <returns></returns>
		public override string GetName()
		{
			return "Set Cols Width: " + width;
		}

		/// <summary>
		/// Create a copy from this action in order to apply the operation to another range.
		/// </summary>
		/// <param name="range">New range where this operation will be appiled to.</param>
		/// <returns>New action instance copied from this action.</returns>
		public override WorksheetReusableAction Clone(RangePosition range)
		{
			return new SetColumnsWidthAction(range.Col, range.Cols, width);
		}
	}
}


