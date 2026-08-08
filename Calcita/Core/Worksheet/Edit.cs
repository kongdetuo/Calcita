using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calcita.Events;

#if EX_SCRIPT
using Calcita.ReoScript;
using Calcita.Script;
#endif // EX_SCRIPT;


using Calcita.Core;
using Calcita.Actions;
using Calcita.DataFormat;
using Calcita.Views;
using Calcita.Graphics;

namespace Calcita
{
	/// <summary>
	/// Edit source mode of a cell editing operation.
	/// </summary>
	public enum CellEditMode
	{
		/// <summary>
		/// Editing is performed in the in-cell editor overlaid on the cell.
		/// </summary>
		InCell = 0,

		/// <summary>
		/// Editing is performed in an external editor such as the formula bar.
		/// </summary>
		FormulaBar = 1,
	}

	partial class Worksheet
	{
		#region StartEdit

		internal Cell currentEditingCell;
		private string backupData;

		/// <summary>
		/// Get the edit source mode of the current editing operation.
		/// </summary>
		public CellEditMode EditMode { get; private set; } = CellEditMode.InCell;

		/// <summary>
		/// Start to edit selected cell
		/// </summary>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit()
		{
			return this.selStart.IsEmpty ? false : StartEdit(this.focusPos);
		}

		/// <summary>
		/// Start to edit selected cell
		/// </summary>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(string newText)
		{
			return this.selStart.IsEmpty ? false : StartEdit(this.focusPos, newText);
		}

		/// <summary>
		/// Start to edit selected cell in specified edit source mode.
		/// </summary>
		/// <param name="mode">Edit source mode, such as in-cell editor or formula bar.</param>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(CellEditMode mode)
		{
			return this.selStart.IsEmpty ? false : StartEdit(this.focusPos, null, mode);
		}

		/// <summary>
		/// Start to edit selected cell with specified initial text in specified edit source mode.
		/// </summary>
		/// <param name="newText">A text will be displayed in the edit field initially.</param>
		/// <param name="mode">Edit source mode, such as in-cell editor or formula bar.</param>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(string newText, CellEditMode mode)
		{
			return this.selStart.IsEmpty ? false : StartEdit(this.focusPos, newText, mode);
		}

		/// <summary>
		/// Start to edit specified cell
		/// </summary>
		/// <param name="pos">Position of specified cell</param>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(CellPosition pos)
		{
			return StartEdit(pos.Row, pos.Col);
		}

		/// <summary>
		/// Start to edit specified cell
		/// </summary>
		/// <param name="pos">Position of specified cell</param>
		/// <param name="newText">A text will be displayed in the edit field initially.</param>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(CellPosition pos, string newText)
		{
			return StartEdit(pos.Row, pos.Col, newText);
		}

		/// <summary>
		/// Start to edit specified cell in specified edit source mode.
		/// </summary>
		/// <param name="pos">Position of specified cell</param>
		/// <param name="mode">Edit source mode, such as in-cell editor or formula bar.</param>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(CellPosition pos, CellEditMode mode)
		{
			return StartEdit(pos.Row, pos.Col, null, mode);
		}

		/// <summary>
		/// Start to edit specified cell with specified initial text in specified edit source mode.
		/// </summary>
		/// <param name="pos">Position of specified cell</param>
		/// <param name="newText">A text will be displayed in the edit field initially.</param>
		/// <param name="mode">Edit source mode, such as in-cell editor or formula bar.</param>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(CellPosition pos, string newText, CellEditMode mode)
		{
			return StartEdit(pos.Row, pos.Col, newText, mode);
		}

		/// <summary>
		/// Start to edit specified cell
		/// </summary>
		/// <param name="row">Index of row of specified cell</param>
		/// <param name="col">Index of column of specified cell</param>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(int row, int col)
		{
			return StartEdit(row, col, null, CellEditMode.InCell);
		}

		/// <summary>
		/// Start to edit specified cell in specified edit source mode.
		/// </summary>
		/// <param name="row">Index of row of specified cell</param>
		/// <param name="col">Index of column of specified cell</param>
		/// <param name="mode">Edit source mode, such as in-cell editor or formula bar.</param>
		/// <returns>True if the editing operation has been started</returns>
		public bool StartEdit(int row, int col, CellEditMode mode)
		{
			return StartEdit(row, col, null, mode);
		}

		/// <summary>
		/// Start to edit specified cell.
		/// </summary>
		/// <param name="row">Index of row of specified cell.</param>
		/// <param name="col">Index of column of specified cell.</param>
		/// <param name="newText">A text displayed in the text field to be edited.</param>
		/// <returns>True if worksheet entered edit-mode successfully; Otherwise return false.</returns>
		public bool StartEdit(int row, int col, string newText)
		{
			return StartEdit(row, col, newText, CellEditMode.InCell);
		}

		/// <summary>
		/// Start to edit specified cell in specified edit source mode.
		/// </summary>
		/// <param name="row">Index of row of specified cell.</param>
		/// <param name="col">Index of column of specified cell.</param>
		/// <param name="newText">A text displayed in the text field to be edited.</param>
		/// <param name="mode">Edit source mode, such as in-cell editor or formula bar.</param>
		/// <returns>True if worksheet entered edit-mode successfully; Otherwise return false.</returns>
		public bool StartEdit(int row, int col, string newText, CellEditMode mode)
		{
			if (row < 0 || col < 0 || row >= this.cells.RowCapacity || col >= this.cells.ColCapacity) return false;

			// if cell is part of merged cell
			if (!IsValidCell(row, col))
			{
				// find the merged cell
				Cell cell = GetMergedCellOfRange(row, col);

				// start edit on merged cell
				return StartEdit(cell, newText, mode);
			}
			else
			{
				return StartEdit(CreateAndGetCell(row, col), newText, mode);
			}
		}

		internal bool StartEdit(Cell cell)
		{
			return this.StartEdit(cell, null, CellEditMode.InCell);
		}

		internal bool StartEdit(Cell cell, CellEditMode mode)
		{
			return this.StartEdit(cell, null, mode);
		}

		internal bool StartEdit(Cell cell, string newText)
		{
			return this.StartEdit(cell, newText, CellEditMode.InCell);
		}

		internal bool StartEdit(Cell cell, string newText, CellEditMode mode)
		{
			// abort if either spreadsheet or cell is readonly
			if (this.HasSettings(WorksheetSettings.Edit_Readonly)
				|| cell == null || cell.IsReadOnly)
			{
				return false;
			}

			if (this.focusPos != cell.Position)
			{
				this.FocusPos = cell.Position;
			}
			else
			{
				this.ScrollToCell(cell);
			}

			string editText = null;

			if (newText == null)
			{
				editText = cell.GetEditText();
				this.backupData = editText;
			}
			else
			{
				editText = newText;

				this.backupData = cell.DisplayText;
			}

			if (cell.DataFormat == CellDataFormatFlag.Percent
				&& this.HasSettings(WorksheetSettings.Edit_FriendlyPercentInput))
			{
				if (double.TryParse(editText, out var val))
				{
					editText = (newText == null ? (val * 100) : val) + "%";
				}
			}

			if (BeforeCellEdit != null)
			{
				CellBeforeEditEventArgs arg = new CellBeforeEditEventArgs(cell)
				{
					EditText = editText,
				};

				BeforeCellEdit(this, arg);

				if (arg.IsCancelled)
				{
					return false;
				}

				editText = arg.EditText;
			}

#if EX_SCRIPT
			// v0.8.2: 'beforeCellEdit' renamed to 'onCellEdit'
			// v0.8.5: 'onCellEdit' renamed to 'oncelledit'
			object scriptReturn = RaiseScriptEvent("oncelledit", new RSCellObject(this, cell.InternalPos, cell));
			if (scriptReturn != null && !ScriptRunningMachine.GetBoolValue(scriptReturn))
			{
				return false;
			}
#endif

			if (cell.body != null)
			{
				bool canContinue = cell.body.OnStartEdit();
				if (!canContinue) return false;
			}

			if (currentEditingCell != null)
			{
				EndEdit(this.controlAdapter.GetEditControlText());
			}

			this.EditMode = mode;

			this.currentEditingCell = cell;

			this.controlAdapter.SetEditControlText(editText);

			if (cell.DataFormat == CellDataFormatFlag.Percent && editText.EndsWith("%"))
			{
				this.controlAdapter.SetEditControlCaretPos(editText.Length - 1);
			}

			if (CellEditStarted != null)
			{
				CellEditStarted(this, new CellEditStartedEventArgs(cell)
				{
					EditText = editText,
					EditMode = mode,
				});
			}

			if (mode != CellEditMode.InCell)
			{
				return true;
			}

			RGFloat x = 0;

			RGFloat width = (cell.Width - 1) * this.renderScaleFactor;

			int cellIndentSize = 0;

			//if ((cell.InnerStyle.Flag & PlainStyleFlag.Indent) == PlainStyleFlag.Indent)
			//{
			//indentSize = (int)Math.Round(cell.InnerStyle.Indent * this.indentSize * this.scaleFactor);
			//width -= indentSize;
			//}


			// why + 6 ?
			if (width < cell.TextBounds.Width) width = cell.TextBounds.Width + 6;

			width--;
			//width = (width - 1);

			RGFloat scale = this.renderScaleFactor;

			#region Horizontal alignment
			switch (cell.RenderHorAlign)
			{
				default:
				case ReoGridRenderHorAlign.Left:
					this.controlAdapter.SetEditControlAlignment(ReoGridHorAlign.Left);
					x = cell.Left * scale + 1 + cellIndentSize;
					break;

				case ReoGridRenderHorAlign.Center:
					this.controlAdapter.SetEditControlAlignment(ReoGridHorAlign.Center);
					x = (cell.Left * scale + (((cell.Width - 1) * scale - 1) - width) / 2) + 1;
					break;

				case ReoGridRenderHorAlign.Right:
					this.controlAdapter.SetEditControlAlignment(ReoGridHorAlign.Right);
					x = (cell.Right - 1) * scale - width - cellIndentSize;
					break;
			}

			if (cell.InnerStyle.HAlign == ReoGridHorAlign.DistributedIndent)
			{
				this.controlAdapter.SetEditControlAlignment(ReoGridHorAlign.Center);
			}
			#endregion // Horizontal alignment

			RGFloat y = cell.Top * scale + 1;

			var activeViewport = viewportController.FocusView as IViewport;

			int boxX = (int)Math.Round(x + viewportController.FocusView.Left - (activeViewport == null ? 0 : (activeViewport.ScrollViewLeft * scale)));
			int boxY = (int)Math.Round(y + viewportController.FocusView.Top - (activeViewport == null ? 0 : (activeViewport.ScrollViewTop * scale)));

			RGFloat height = (cell.Height - 1) * scale - 1;

			if (!cell.IsMergedCell && cell.InnerStyle.TextWrapMode != TextWrapMode.NoWrap)
			{
				if (height < cell.TextBounds.Height) height = cell.TextBounds.Height;
			}

			int offsetHeight = 0;// (int)Math.Round(height);// (int)Math.Round(height + 2 - (cell.Height));

			if (offsetHeight > 0)
			{
				switch (cell.InnerStyle.VAlign)
				{
					case ReoGridVerAlign.Top:
						break;
					default:
					case ReoGridVerAlign.Middle:
						boxY -= offsetHeight / 2;
						break;
					case ReoGridVerAlign.Bottom:
						boxY -= offsetHeight;
						break;
				}
			}

			Rectangle rect = new Rectangle(boxX, boxY, width, height);

			this.controlAdapter.ShowEditControl(rect, cell);

			return true;

		}

		#endregion // StartEdit

		#region EndEdit

		/// <summary>
		/// Check whether any cell current in edit mode
		/// </summary>
		/// <returns>true if any cell is editing</returns>
		public bool IsEditing
		{
			get
			{
				return currentEditingCell != null;
			}
		}

		/// <summary>
		/// Get instance of current editing cell.
		/// </summary>
		public Cell EditingCell
		{
			get { return this.currentEditingCell; }
		}

		private bool endEditProcessing = false;

		/// <summary>
		/// Force end current editing operation with the specified reason.
		/// </summary>
		/// <param name="reason">Ending Reason of editing operation</param>
		/// <returns>True if currently in editing mode, and operation has been
		/// finished successfully.</returns>
		public bool EndEdit(EndEditReason reason)
		{
			return EndEdit(reason == EndEditReason.NormalFinish ? this.controlAdapter.GetEditControlText() : null, reason);
		}

		/// <summary>
		/// Force end current editing operation.
		/// Uses specified data instead of the data of user edited.
		/// </summary>
		/// <param name="data">New data to be set to the edited cell</param>
		/// <returns>True if currently in editing mode, and operation has been
		/// finished successfully.</returns>
		public bool EndEdit(object data)
		{
			return EndEdit(data, EndEditReason.NormalFinish);
		}

		/// <summary>
		/// Force end current editing operation with the specified reason.
		/// Uses specified data instead of the data of user edited.
		/// </summary>
		/// <param name="data">New data to be set to the edited cell</param>
		/// <param name="reason">Ending Reason of editing operation</param>
		/// <returns>True if currently in editing mode, and operation has been
		/// finished successfully.</returns>
		public bool EndEdit(object data, EndEditReason reason)
		{
			if (currentEditingCell == null || endEditProcessing) return false;

			endEditProcessing = true;

			if (data == null)
			{
				data = this.controlAdapter.GetEditControlText();
			}

			if (AfterCellEdit != null)
			{
				CellAfterEditEventArgs arg = new CellAfterEditEventArgs(currentEditingCell)
				{
					EndReason = reason,
					NewData = data,
				};

				AfterCellEdit(this, arg);
				data = arg.NewData;
				reason = arg.EndReason;
			}

			switch (reason)
			{
				case EndEditReason.Cancel:
					break;

				case EndEditReason.NormalFinish:
					if (data is string)
					{
						var datastr = (string)data;

						if (string.IsNullOrEmpty(datastr))
						{
							data = null;
						}
						else
						{
							// convert data into cell data format
							switch (currentEditingCell.DataFormat)
							{
								case CellDataFormatFlag.Number:
								case CellDataFormatFlag.Currency:
									if (double.TryParse(datastr, out var numericValue))
									{
										data = numericValue;
									}
									break;

								case CellDataFormatFlag.Percent:
									if (datastr.EndsWith("%"))
									{
										if (double.TryParse(datastr.Substring(0, datastr.Length - 1), out var val))
										{
											data = val / 100;
										}
									}
									else if (datastr == "%")
									{
										data = null;
									}
									break;

								case CellDataFormatFlag.DateTime:
									{
										if (DateTime.TryParse(datastr, out var dt))
										{
											data = dt;
										}
									}
									break;
							}
						}
					}

					if (string.IsNullOrEmpty(backupData)) backupData = null;

					var body = currentEditingCell.body;

					if (body != null)
					{
						data = body.OnEndEdit(data);
					}

					if (!object.Equals(data, backupData))
					{
						DoAction(new SetCellDataAction(currentEditingCell.InternalRow, currentEditingCell.InternalCol, data));
					}

					break;
			}

			this.controlAdapter.HideEditControl();
			this.controlAdapter.Focus();
			currentEditingCell = null;
			this.EditMode = CellEditMode.InCell;

			endEditProcessing = false;

			return true;
		}

		#endregion // EndEdit

		#region Events

		/// <summary>
		/// Event raised before cell changed to edit mode
		/// </summary>
		public event EventHandler<CellBeforeEditEventArgs> BeforeCellEdit;

		/// <summary>
		/// Event raised after cell changed to edit mode
		/// </summary>
		public event EventHandler<CellAfterEditEventArgs> AfterCellEdit;

		/// <summary>
		/// Event raised after cell entered edit mode.
		/// </summary>
		public event EventHandler<CellEditStartedEventArgs> CellEditStarted;

		/// <summary>
		/// Event raised after input text changing
		/// </summary>
		public event EventHandler<CellEditTextChangingEventArgs> CellEditTextChanging;

		/// <summary>
		/// Event raised after any characters is input
		/// </summary>
		public event EventHandler<CellEditCharInputEventArgs> CellEditCharInputed;

		internal string RaiseCellEditTextChanging(string text)
		{
			if (this.CellEditTextChanging == null)
				return text;
			else
			{
				var arg = new CellEditTextChangingEventArgs(this.currentEditingCell) { Text = text };
				CellEditTextChanging(this, arg);
				return arg.Text;
			}
		}

		internal int RaiseCellEditCharInputed(int @char)
		{
			if (this.CellEditCharInputed == null)
			{
				return @char;
			}
			else
			{
				var arg = new CellEditCharInputEventArgs(this.currentEditingCell,
					this.currentEditingCell != null ? this.controlAdapter.GetEditControlText() : null,
					@char, this.controlAdapter.GetEditControlCaretPos(),
					this.controlAdapter.GetEditControlCaretLine());

				CellEditCharInputed(this, arg);

				return arg.InputChar;
			}
		}

		#endregion // Events

		#region Editing Text
		/// <summary>
		/// Get or set the current text in edit textbox of cell
		/// </summary>
		public string CellEditText
		{
			// TODO: move to control
			get { return this.controlAdapter.GetEditControlText(); }
			set { this.controlAdapter.SetEditControlText(RaiseCellEditTextChanging(value)); }
		}
		#endregion // Editing Text
	}
}
