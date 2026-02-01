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
	/// Base action for all actions that are used for worksheet operations.
	/// </summary>
	public abstract class BaseWorksheetAction : IUndoableAction
	{
		/// <summary>
		/// Instance for the grid control will be setted before action performed.
		/// </summary>
		public Worksheet Worksheet { get; internal set; }

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
		/// Get friendly name of this action.
		/// </summary>
		/// <returns>Get friendly name of this action.</returns>
		public abstract string GetName();
	}
}


