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
using System.ComponentModel;

using Calcita.Interaction;

namespace Calcita.Main
{
	/// <summary>
	/// Mouse event arguments for sheet tab control.
	/// </summary>
	public class SheetTabMouseEventArgs : EventArgs
	{
		/// <summary>
		/// Mouse button flags. (Left, Right or Middle)
		/// </summary>
		public MouseButtons MouseButtons { get; set; }

		/// <summary>
		/// Mouse location related to sheet tab control.
		/// </summary>
		public RGPoint Location { get; set; }

		/// <summary>
		/// Number of tab specified by this index to be moved.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// Get or set whether the user-code handled this event. 
		/// Built-in operations will be cancelled if this property is set to true.
		/// </summary>
		public bool Handled { get; set; }
	}

	/// <summary>
	/// Sheet moved event arguments.
	/// </summary>
	public class SheetTabMovedEventArgs : EventArgs
	{
		/// <summary>
		/// Number of tab specified by this index to be moved.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// Number of tab as position moved to.
		/// </summary>
		public int TargetIndex { get; set; }
	}

	/// <summary>
	/// Represents the border style of tab item.
	/// </summary>
	public enum SheetTabBorderStyle
	{
		/// <summary>
		/// Sharp Rectangle
		/// </summary>
		RectShadow,

		/// <summary>
		/// Separated Rounded Rectangle
		/// </summary>
		SplitRouned,

		/// <summary>
		/// No Borders (Windows 8 Style)
		/// </summary>
		NoBorder,
	}

	/// <summary>
	/// Position of tab control will be located.
	/// </summary>
	public enum SheetTabControlPosition
	{
		/// <summary>
		/// Put at top to other controls.
		/// </summary>
		Top,

		/// <summary>
		/// Put at bottom to other controls.
		/// </summary>
		Bottom,
	}

	/// <summary>
	/// Representes the sheet tab control interface.
	/// </summary>
	internal interface ISheetTabControl
	{
		/// <summary>
		/// Get or set the current tab index.
		/// </summary>
		int SelectedIndex { get; set; }

		/// <summary>
		/// Event raised when tab item is moved.
		/// </summary>
		event EventHandler<SheetTabMovedEventArgs> TabMoved;

		/// <summary>
		/// Event raised when selected tab is changed.
		/// </summary>
		event EventHandler SelectedIndexChanged;

		/// <summary>
		/// Event raised when sheet list button is clicked.
		/// </summary>
		event EventHandler SheetListClick;

		/// <summary>
		/// Event raised when new sheet butotn is clicked.
		/// </summary>
		event EventHandler NewSheetClick;

		///// <summary>
		///// Move item to specified position.
		///// </summary>
		///// <param name="index">number of tab to be moved.</param>
		///// <param name="targetIndex">position of moved to.</param>
		//void MoveItem(int index, int targetIndex);

		/// <summary>
		/// Scroll view to show tab item by specified index.
		/// </summary>
		/// <param name="index">Number of item to scrolled.</param>
		void ScrollToItem(int index);

		/// <summary>
		/// Add tab.
		/// </summary>
		/// <param name="title">Title of tab.</param>
		void AddTab(string title);

		/// <summary>
		/// Insert tab
		/// </summary>
		/// <param name="index">Zero-based number of tab.</param>
		/// <param name="title">Title of tab.</param>
		void InsertTab(int index, string title);

		/// <summary>
		/// Update tab title.
		/// </summary>
		/// <param name="index">Zero-based number of tab.</param>
		/// <param name="title">Title of tab.</param>
		void UpdateTab(int index, string title, RGColor backgroundColor, RGColor foregroundColor);

		/// <summary>
		/// Remove specified tab.
		/// </summary>
		/// <param name="index">Zero-based number of tab.</param>
		void RemoveTab(int index);

		/// <summary>
		/// Clear all tabs.
		/// </summary>
		void ClearTabs();

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


