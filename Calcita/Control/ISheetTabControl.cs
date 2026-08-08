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
namespace Calcita.Main
{
	/// <summary>
	/// Represents the sheet tab control.
	/// The control displays the worksheets of a workbook as a set of tabs
	/// and is responsible for managing them (switching, adding, renaming, ...).
	/// </summary>
	internal interface ISheetTabControl
	{
		/// <summary>
		/// Get or set the workbook whose worksheets are displayed as tabs.
		/// </summary>
		IWorkbook Workbook { get; set; }

		/// <summary>
		/// Get or set the currently selected worksheet.
		/// </summary>
		Worksheet CurrentWorksheet { get; set; }

		/// <summary>
		/// Determine whether or not allow to move tab by dragging mouse.
		/// </summary>
		bool AllowDragToMove { get; set; }

		/// <summary>
		/// Determine whether or not to show new sheet button.
		/// </summary>
		bool NewButtonVisible { get; set; }
	}
}