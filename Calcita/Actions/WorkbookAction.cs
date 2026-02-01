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
 ****************************************************************************/using Calcita.Common;

namespace Calcita.Actions
{
	/// <summary>
	/// Represents an action of workbook.
	/// </summary>
	public abstract class WorkbookAction : IUndoableAction
	{
		/// <summary>
		/// Get the workbook instance.
		/// </summary>
		public IWorkbook Workbook { get; internal set; }

		/// <summary>
		/// Create workbook action with specified workbook instance.
		/// </summary>
		/// <param name="workbook"></param>
		public WorkbookAction(IWorkbook workbook = null)
		{
			this.Workbook = workbook;
		}

		/// <summary>
		/// Do this action.
		/// </summary>
		public abstract void Do();

		/// <summary>
		/// Undo this action.
		/// </summary>
		public abstract void Undo();

		/// <summary>
		/// Redo this action.
		/// </summary>
		public virtual void Redo()
		{
			this.Do();
		}

		/// <summary>
		/// Get the friendly name of this action.
		/// </summary>
		/// <returns></returns>
		public abstract string GetName();
	}

}


