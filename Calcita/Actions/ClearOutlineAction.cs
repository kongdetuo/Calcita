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

#if OUTLINE
using Calcita.Outline;

namespace Calcita.Actions
{

	/// <summary>
	/// Action to clear all outlines
	/// </summary>
	public class ClearOutlineAction : BaseOutlineAction
	{
		/// <summary>
		/// Create action to clear all outlines on specified range
		/// </summary>
		/// <param name="rowOrColumn">The range to clear outlines (row, column or both row and column)</param>
		public ClearOutlineAction(RowOrColumn rowOrColumn)
			: base(rowOrColumn)
		{
		}

		private List<IReoGridOutline> rowBackupOutlines;
		private List<IReoGridOutline> colBackupOutlines;

		/// <summary>
		/// Do this action
		/// </summary>
		public override void Do()
		{
			if (this.Worksheet != null)
			{
				if ((this.rowOrColumn & Calcita.RowOrColumn.Row) == Calcita.RowOrColumn.Row)
				{
					this.rowBackupOutlines = new List<IReoGridOutline>();
					this.Worksheet.IterateOutlines(Calcita.RowOrColumn.Row, (g, o) => { this.rowBackupOutlines.Add(o); return true; });
				}

				if ((this.rowOrColumn & Calcita.RowOrColumn.Column) == Calcita.RowOrColumn.Column)
				{
					this.colBackupOutlines = new List<IReoGridOutline>();
					this.Worksheet.IterateOutlines(Calcita.RowOrColumn.Column, (g, o) => { this.colBackupOutlines.Add(o); return true; });
				}

				this.Worksheet.ClearOutlines(this.rowOrColumn);
			}
		}

		/// <summary>
		/// Undo this action
		/// </summary>
		public override void Undo()
		{
			if (this.Worksheet != null)
			{
				if ((this.rowOrColumn & Calcita.RowOrColumn.Row) == Calcita.RowOrColumn.Row
					&& this.rowBackupOutlines != null)
				{
					foreach (var o in this.rowBackupOutlines)
					{
						this.Worksheet.AddOutline(Calcita.RowOrColumn.Row, o.Start, o.Count);
					}
				}

				if ((this.rowOrColumn & Calcita.RowOrColumn.Column) == Calcita.RowOrColumn.Column
					&& this.colBackupOutlines != null)
				{
					foreach (var o in this.colBackupOutlines)
					{
						this.Worksheet.AddOutline(Calcita.RowOrColumn.Column, o.Start, o.Count);
					}
				}
			}
		}

		/// <summary>
		/// Get friendly name of this action
		/// </summary>
		/// <returns>Name of action</returns>
		public override string GetName()
		{
			return string.Format("Clear {0} Outlines", base.GetRowOrColumnDesc());
		}
	}
}

#endif // OUTLINE


