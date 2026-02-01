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
#if OUTLINE
using Calcita.Outline;

namespace Calcita.Actions
{
	/// <summary>
	/// Remove outline action
	/// </summary>
	public class RemoveOutlineAction : OutlineAction
	{
		private IReoGridOutline removedOutline;

		/// <summary>
		/// Instance of removed outline if operation was successfully
		/// </summary>
		public IReoGridOutline RemovedOutline { get { return this.removedOutline; } }

		/// <summary>
		/// Create action instance to remove outline
		/// </summary>
		/// <param name="rowOrColumn">Row or column to find specified outline</param>
		/// <param name="start">Number of line of specified outline</param>
		/// <param name="count">Number of lines of specified outline</param>
		public RemoveOutlineAction(RowOrColumn rowOrColumn, int start, int count)
			: base(rowOrColumn, start, count)
		{
		}

		/// <summary>
		/// Do this action
		/// </summary>
		public override void Do()
		{
			if (this.Worksheet != null)
			{
				this.removedOutline = this.Worksheet.RemoveOutline(this.rowOrColumn, start, count);
			}
		}

		/// <summary>
		/// Undo this action
		/// </summary>
		public override void Undo()
		{
			if (this.Worksheet != null)
			{
				this.Worksheet.AddOutline(this.rowOrColumn, start, count);
				this.removedOutline = null;
			}
		}

		/// <summary>
		/// Get friendly name of this action
		/// </summary>
		/// <returns>Name of action</returns>
		public override string GetName()
		{
			return string.Format("Remove {0} Outline, Start at {1}, Count: {2}", 
				base.GetRowOrColumnDesc(), this.start, this.count);
		}	
	}
}
#endif // OUTLINE




