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

using Calcita.IO;
using Calcita.Actions;
using Calcita.Events;
using Calcita.Graphics;

using Calcita.Views;
using Calcita.Rendering;
using Avalonia.Controls.Primitives;

namespace Calcita.Main
{


	internal enum ScrollDirection : byte
	{
		None = 0,
		Horizontal = 1,
		Vertical = 2,
		Both = Horizontal | Vertical,
	}

	internal interface IRangePickableControl
	{
		void PickRange(Func<Worksheet, RangePosition, bool> handler);
		void EndPickRange();
		void StartPickRangeAndCopyStyle();
	}

	internal interface IContextMenuControl
	{
		FlyoutBase? CellsContextFlyout { get; }
        FlyoutBase? RowHeaderContextFlyout { get; }
        FlyoutBase? ColumnHeaderContextFlyout { get; }
        FlyoutBase? LeadHeaderContextFlyout { get; }
    }

#if EX_SCRIPT
    internal interface IScriptExecutableControl
	{
		string Script { get; set; }

		unvell.ReoScript.ScriptRunningMachine Srm { get; }

		object RunScript(string script);
	}
#endif // EX_SCRIPT

	internal interface IPersistenceWorkbook
	{
		void Save(string path, FileFormat format = FileFormat._Auto, System.Text.Encoding encoding = null);
		void Save(System.IO.Stream stream, FileFormat format = FileFormat._Auto, System.Text.Encoding encoding = null);

		void Load(string path, FileFormat format = FileFormat._Auto, System.Text.Encoding encoding = null);
		void Load(System.IO.Stream stream, FileFormat format = FileFormat._Auto, System.Text.Encoding encoding = null);
	}

	internal interface IActionControl
	{
		//unvell.Common.ActionManager ActionManager { get; }
		void DoAction(Worksheet sheet, BaseWorksheetAction action);
		void Undo();
		void Redo();
		void RepeatLastAction(RangePosition range);
        bool CanUndo { get; }

        bool CanRedo { get; }

        event EventHandler<WorkbookActionEventArgs> ActionPerformed;
		event EventHandler<WorkbookActionEventArgs> Undid;
		event EventHandler<WorkbookActionEventArgs> Redid;
		//event EventHandler Repeated;
		void ClearActionHistory();
		void ClearActionHistoryForWorksheet(Worksheet sheet);
	}

	internal interface IVisualWorkbook : IScrollableWorksheetContainer
	{
		Worksheet CurrentWorksheet { get; set; }
	}

	internal interface IScrollableWorksheetContainer
	{
		void RaiseWorksheetScrolledEvent(Worksheet worksheet, RGFloat x, RGFloat y);

		// FIXME: not implemented
		bool ShowScrollEndSpacing { get; }
	}

	internal interface IEditableControlAdapter
	{
		void ShowEditControl(Graphics.Rectangle bounds, Cell cell);
		void HideEditControl();

		void SetEditControlText(string text);
		string GetEditControlText();

		void EditControlSelectAll();
		void SetEditControlCaretPos(int pos);
		int GetEditControlCaretPos();
		int GetEditControlCaretLine();
		void SetEditControlAlignment(ReoGridHorAlign align);

		void EditControlApplySystemMouseDown();

		void EditControlCopy();
		void EditControlPaste();
		void EditControlCut();

		void EditControlUndo();
	}

	internal interface IScrollableControlAdapter
	{
		//bool ScrollBarHorizontalVisible { get; set; }
		//bool ScrollBarVerticalVisible { get; set; }

		RGIntDouble ScrollBarHorizontalMaximum { get; set; }
		RGIntDouble ScrollBarHorizontalMinimum { get; set; }
		RGIntDouble ScrollBarHorizontalValue { get; set; }
		RGIntDouble ScrollBarHorizontalLargeChange { get; set; }

		RGIntDouble ScrollBarVerticalMaximum { get; set; }
		RGIntDouble ScrollBarVerticalMinimum { get; set; }
		RGIntDouble ScrollBarVerticalValue { get; set; }
		RGIntDouble ScrollBarVerticalLargeChange { get; set; }
	}

	internal interface ITimerSupportedAdapter
	{
		void StartTimer();
		void StopTimer();
	}

	internal interface IShowContextMenuAdapter
	{
		void ShowContextMenuStrip(ViewTypes viewType, Point containerLocation);
	}

	internal interface IMultisheetAdapter
	{
		ISheetTabControl SheetTabControl { get; }
	}

	internal interface ICompViewAdapter : IMultisheetAdapter
	{
		IVisualWorkbook ControlInstance { get; }
		IRenderer Renderer { get; }
		ControlAppearanceStyle ControlStyle { get; }

		RGFloat BaseScale { get; }
		RGFloat MinScale { get; }
		RGFloat MaxScale { get; }

		void ChangeCursor(Interaction.CursorStyle cursor);
		void RestoreCursor();
		void ChangeSelectionCursor(Interaction.CursorStyle cursor);

		Rectangle GetContainerBounds();
		void Focus();
		void Invalidate();

		void ChangeBackgroundColor(SolidColor color);
		bool IsVisible { get; }
		Point PointToScreen(Point point);
		void ShowTooltip(Point point, string content);
	}

	internal interface IControlAdapter : ICompViewAdapter, 
		IEditableControlAdapter, IScrollableControlAdapter, ITimerSupportedAdapter,
		IShowContextMenuAdapter
	{
	}
}


