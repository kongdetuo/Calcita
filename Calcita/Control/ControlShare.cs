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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using Calcita.Common;

using Calcita.Actions;
using Calcita.Events;
using Calcita.Views;
using Calcita.Interaction;

using Calcita.Main;
using Calcita.Rendering;
using Avalonia;
using System.Runtime.CompilerServices;

namespace Calcita.Controls
{
    partial class CalcitaControl
    {
        internal IRenderer Renderer
        {
            get { return this.SheetCanvas.renderer; }
        }

        #region Initialize

        static CalcitaControl()
        {
#if NETCOREAPP3_1_OR_GREATER
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif // NETCOREAPP3_1_OR_GREATER

            WorkbookProperty.Changed.AddClassHandler<CalcitaControl>((x, e) =>
            {
                x.OnWorkbookChanged(e);
            });
            SelectedIndexProperty.Changed.AddClassHandler<CalcitaControl>((x, e) =>
            {
                var index = e.GetNewValue<int>();
                if(index >= 0 && x.Workbook?.Worksheets.Count > index) 
                {
                    x.CurrentWorksheet = x.Workbook?.Worksheets[x.SelectedIndex];
                }
                else
                {
                    x.CurrentWorksheet = null;
                }
            });
            CurrentWorksheetProperty.Changed.AddClassHandler<CalcitaControl>((x, e) =>
            {
                x.OnCurrentWorksheetChanged(e);
            });
        }

        private void InitControl()
        {
#if WINFORM || WPF
            // initialize cursors
            // normal grid selector
            this.builtInCellsSelectionCursor = LoadCursorFromResource(Calcita.Properties.Resources.grid_select);
            this.internalCurrentCursor = builtInCellsSelectionCursor;

            // cell picking
            this.defaultPickRangeCursor = LoadCursorFromResource(Calcita.Properties.Resources.pick_range);

            // full-row and full-col selector
            this.builtInFullColSelectCursor = LoadCursorFromResource(Calcita.Properties.Resources.full_col_select);
            this.builtInFullRowSelectCursor = LoadCursorFromResource(Calcita.Properties.Resources.full_row_select);

            this.builtInEntireSheetSelectCursor = this.builtInCellsSelectionCursor;

            this.builtInCrossCursor = LoadCursorFromResource(Calcita.Properties.Resources.cross);
#endif // WINFORM || WPF

#if AVALONIA
            // initialize cursors
            // normal grid selector
            this.builtInCellsSelectionCursor = LoadCursorFromResource(Calcita.Properties.Resources.grid_select, false);
            this.internalCurrentCursor = builtInCellsSelectionCursor;

            // cell picking
            this.defaultPickRangeCursor = LoadCursorFromResource(Calcita.Properties.Resources.pick_range);

            // full-row and full-col selector
            this.builtInFullColSelectCursor = LoadCursorFromResource(Calcita.Properties.Resources.full_col_select);
            this.builtInFullRowSelectCursor = LoadCursorFromResource(Calcita.Properties.Resources.full_row_select);

            this.builtInEntireSheetSelectCursor = this.builtInCellsSelectionCursor;

            this.builtInCrossCursor = LoadCursorFromResource(Calcita.Properties.Resources.cross);
#endif // WINFORM || WPF

            this.ControlStyle = ControlAppearanceStyle.CreateDefaultControlStyle();
            this.WorksheetScrolled += (s, e) => { this.ScrollCurrentWorksheet(e.X, e.Y); };
        }

        private void InitWorkbook()
        {
            // todo 在 OnWorkbookChanged 中初始化控件

            // create workbook
            this.Workbook = new Workbook();

#if EX_SCRIPT
            this.workbook.SRMInitialized += (s, e) =>
                {
                    if (this.workbook.workbookObj != null)
                    {
                        this.workbook.workbookObj.ControlInstance = this;
                    }
                };
#endif // EX_SCRIPT

            // create and set default worksheet
            this.Workbook.AddWorksheet(this.Workbook.CreateWorksheet());

            this.sheetTab?.SelectedIndexChanged += (s, e) =>
            {
                if (this.sheetTab.SelectedIndex >= 0 && this.sheetTab.SelectedIndex < this.Workbook.Worksheets.Count)
                {
                    this.CurrentWorksheet = this.Workbook.Worksheets[this.sheetTab.SelectedIndex];
                }
            };


            this.actionManager.BeforePerformAction += (s, e) =>
                {
                    if (this.BeforeActionPerform != null)
                    {
                        var arg = new BeforeActionPerformEventArgs(e.Action);

                        this.BeforeActionPerform(this, arg);

                        e.Cancel = arg.IsCancelled;
                    }
                };

            // register for moniting reusable action
            this.actionManager.AfterPerformAction += (s, e) =>
            {
                if (e.Action is WorksheetReusableAction)
                {
                    this.lastReusableAction = e.Action as WorksheetReusableAction;
                }

                this.ActionPerformed?.Invoke(this, new WorkbookActionEventArgs(e.Action));
            };
        }

        #endregion // Initialize

        #region Memory Workbook
        /// <summary>
        /// Create an instance of ReoGrid workbook in memory. <br/>
        /// The memory workbook is the non-GUI version of ReoGrid control, which can do almost all operations, 
        /// such as reading and saving from Excel file, RGF file, changing data, formulas, styles, borders and etc.
        /// </summary>
        /// <returns>Instance of memory workbook.</returns>
        public static IWorkbook CreateMemoryWorkbook()
        {
            var workbook = new Workbook();

            var defaultWorksheet = workbook.CreateWorksheet();
            workbook.AddWorksheet(defaultWorksheet);

            return workbook;
        }
        #endregion // Memory Workbook

        #region Workbook & Worksheet

        /// <summary>
        /// Event raised when workbook loaded from stream or file.
        /// </summary>
        public event EventHandler WorkbookLoaded;

        /// <summary>
        /// Event raised when workbook saved into stream or file.
        /// </summary>
        public event EventHandler WorkbookSaved;

        #region Worksheet Management

        #endregion // Worksheet Management

        /// <summary>
        /// Determine whether or not this workbook is read-only (Reserved v0.8.8)
        /// </summary>
        [Description("Determine whether or not this workbook is read-only")]
        [DefaultValue(false)]
        public bool Readonly
        {
            get
            {
                return this.Workbook.Readonly;
            }
            set
            {
                this.Workbook?.Readonly = value;
            }
        }


        ///// <summary>
        ///// Check whether or not current workbook is empty (all worksheets don't have any cells)
        ///// </summary>
        //public bool IsWorkbookEmpty
        //{
        //    get
        //    {
        //        return this.Workbook.IsEmpty;
        //    }
        //}

        #endregion // Workbook & Worksheet

        #region Actions


        /// <summary>
        /// CanUndoDirectProperty definition
        /// </summary>
        public static readonly DirectProperty<CalcitaControl, bool> CanUndoProperty =
            AvaloniaProperty.RegisterDirect<CalcitaControl, bool>(nameof(CanUndo),
                o => o.CanUndo);


        private bool _CanUndo = default;
        /// <summary>
        /// Gets or sets the CanUndo property. This DirectProperty 
        /// indicates ....
        /// </summary>
        public bool CanUndo
        {
            get => _CanUndo;
            private set => SetAndRaise(CanUndoProperty, ref _CanUndo, value);
        }


        /// <summary>
        /// CanRedoDirectProperty definition
        /// </summary>
        public static readonly DirectProperty<CalcitaControl, bool> CanRedoProperty =
            AvaloniaProperty.RegisterDirect<CalcitaControl, bool>(nameof(CanRedo),
                o => o.CanRedo,
                (o, v) => o.CanRedo = v);

        private bool _CanRedo = default;
        /// <summary>
        /// Gets or sets the CanRedo property. This DirectProperty 
        /// indicates ....
        /// </summary>
        public bool CanRedo
        {
            get => _CanRedo;
            set => SetAndRaise(CanRedoProperty, ref _CanRedo, value);
        }

        private void UpdateUndoRedoStatus()
        {
            this.CanUndo = this.actionManager.CanUndo();
            this.CanRedo = this.actionManager.CanRedo();
        }

        internal ActionManager actionManager = new ActionManager();

        private WorksheetReusableAction? lastReusableAction;

        public void DoAction(BaseWorksheetAction action)
        {
            this.DoAction(this.CurrentWorksheet, action);
            UpdateUndoRedoStatus();
        }

        /// <summary>Do specified action. 
        /// 
        /// An action does the operation as well as undoes for worksheet.
        /// Actions performed by this method will be appended to action history stack 
        /// in order to undo, redo and repeat.
        /// 
        /// There are built-in actions available for many base operations, such as:
        ///   <code>SetCellDataAction</code> - set cell data
        ///   <code>SetRangeDataAction</code> - set data into range
        ///   <code>SetRangeBorderAction</code> - set border to specified range
        ///   <code>SetRangeStyleAction</code> - set styles to specified range
        ///   ...
        ///   
        /// It is possible to make custom action by inherting BaseWorksheetAction.
        /// </summary>
        /// <example>
        /// ReoGrid uses ActionManager, unvell lightweight undo framework, 
        /// to implement the Do/Undo/Redo/Repeat method.
        /// 
        /// To do action:
        /// <code>
        ///   var action = new SetCellDataAction("B1", 10);
        ///   workbook.DoAction(targetSheet, action);
        /// </code>
        /// 
        /// To undo action:
        /// <code>
        ///   workbook.Undo();
        /// </code>
        /// 
        /// To redo action:
        /// <code>
        ///		workbook.Redo();
        /// </code>
        /// 
        /// To repeat last action:
        /// <code>
        ///		workbook.RepeatLastAction(targetSheet, new ReoGridRange("B1:C3"));
        /// </code>
        /// 
        /// It is possible to do multiple actions at same time:
        /// <code>
        ///   var action1 = new SetRangeDataAction(...);
        ///   var action2 = new SetRangeBorderAction(...);
        ///   var action3 = new SetRangeStyleAction(...);
        ///   
        ///		var actionGroup = new WorksheetActionGroup();
        ///		actionGroup.Actions.Add(action1);
        ///		actionGroup.Actions.Add(action2);
        ///		actionGroup.Actions.Add(action3);
        ///		
        ///		workbook.DoAction(targetSheet, actionGroup);
        /// </code>
        /// 
        /// Actions added into action group will be performed by one time,
        /// they will be also undone by one time.
        /// </example>
        /// <seealso cref="ActionGroup"/>
        /// <seealso cref="BaseWorksheetAction"/>
        /// <seealso cref="WorksheetActionGroup"/>
        /// <param name="sheet">worksheet of the target container to perform specified action</param>
        /// <param name="action">action to be performed</param>
        public void DoAction(Worksheet? sheet, BaseWorksheetAction action)
        {
            if(sheet is null)
                throw new ArgumentNullException(nameof(sheet), "Target worksheet cannot be null.");

            action.Worksheet = sheet;

            this.actionManager.DoAction(action);

            if (action is WorksheetReusableAction reusableAction)
            {
                this.lastReusableAction = reusableAction;
            }

            if (this.CurrentWorksheet != sheet)
            {
                sheet.RequestInvalidate();
                this.CurrentWorksheet = sheet;
            }

            UpdateUndoRedoStatus();

            // fix #282, https://github.com/unvell/ReoGrid/issues/282
            // comment out to avoid invoke ActionPerformed event, which is already invoked by actionManager above.
            //if (ActionPerformed != null) ActionPerformed(this, new WorkbookActionEventArgs(action));
        }

        /// <summary>
        /// Undo the last action.
        /// </summary>
        public void Undo()
        {
            if (this.CurrentWorksheet != null)
            {
                if (this.CurrentWorksheet.IsEditing)
                {
                    this.CurrentWorksheet.EndEdit(EndEditReason.NormalFinish);
                }
            }

            var action = this.actionManager.Undo();

            if (action != null)
            {
                if (action is WorkbookAction)
                {
                    // seems nothing to do
                }
                else if (action is BaseWorksheetAction worksheetAction)
                {
                    var sheet = worksheetAction.Worksheet;

                    if (action is WorksheetReusableAction reusableAction)
                    {
                        if (sheet != null)
                        {
                            sheet.SelectRange(reusableAction.Range);
                        }
                    }

                    if (sheet != null)
                    {
                        sheet.RequestInvalidate();
                        this.CurrentWorksheet = sheet;
                    }
                }

                Undid?.Invoke(this, new WorkbookActionEventArgs(action));
            }
            UpdateUndoRedoStatus();
        }

        /// <summary>
        /// Redo the last action.
        /// </summary>
        public void Redo()
        {
            if (this.CurrentWorksheet != null)
            {
                if (this.CurrentWorksheet.IsEditing)
                {
                    this.CurrentWorksheet.EndEdit(EndEditReason.NormalFinish);
                }
            }

            var action = this.actionManager.Redo();

            if (action != null)
            {
                if (action is BaseWorksheetAction worksheetAction)
                {
                    var sheet = worksheetAction.Worksheet;

                    if (action is WorksheetReusableAction reusableAction)
                    {
                        this.lastReusableAction = reusableAction;

                        if (sheet != null)
                        {
                            sheet.SelectRange(this.lastReusableAction.Range);
                        }
                    }

                    if (sheet != null && this.CurrentWorksheet != sheet)
                    {
                        sheet.RequestInvalidate();
                        this.CurrentWorksheet = sheet;
                    }
                }

                Redid?.Invoke(this, new WorkbookActionEventArgs(action));
            }
            UpdateUndoRedoStatus();
        }

        /// <summary>
        /// Repeat to do last action and apply to another specified range.
        /// </summary>
        /// <param name="range">The new range to be applied for the last action.</param>
        public void RepeatLastAction(RangePosition range)
        {
            this.RepeatLastAction(this.CurrentWorksheet, range);
        }

        /// <summary>
        /// Repeat to do last action and apply to another specified range and worksheet.
        /// </summary>
        /// <param name="worksheet">The target worksheet to perform the action.</param>
        /// <param name="range">The new range to be applied for the last action.</param>
        public void RepeatLastAction(Worksheet? worksheet, RangePosition range)
        {
            if (worksheet is null)
                return;

            if (this.CurrentWorksheet != null)
            {
                if (this.CurrentWorksheet.IsEditing)
                {
                    this.CurrentWorksheet.EndEdit(EndEditReason.NormalFinish);
                }
            }

            if (this.CanRedo)
            {
                this.Redo();
            }
            else
            {
                if (this.lastReusableAction != null)
                {
                    var newAction = lastReusableAction.Clone(range);
                    newAction.Worksheet = worksheet;

                    this.actionManager.DoAction(newAction);

                    // fix #282, https://github.com/unvell/ReoGrid/issues/282
                    //this.ActionPerformed?.Invoke(this, new WorkbookActionEventArgs(newAction));

                    this.CurrentWorksheet?.RequestInvalidate();
                    UpdateUndoRedoStatus();
                }
            }
        }

        /// <summary>
        /// Clear all undo/redo actions from workbook action history.
        /// </summary>
        public void ClearActionHistory()
        {
            this.actionManager.Reset();

            this.lastReusableAction = null;
            UpdateUndoRedoStatus();
        }

        /// <summary>
        /// Delete all actions that belongs to specified worksheet.
        /// </summary>
        /// <param name="sheet">Actions belongs to this worksheet will be deleted from workbook action histroy.</param>
        public void ClearActionHistoryForWorksheet(Worksheet sheet)
        {
            List<IUndoableAction> undoActions = this.actionManager.UndoStack;
            for (int i = 0; i < undoActions.Count;)
            {
                var action = undoActions[i];

                var worksheetAction = (action as BaseWorksheetAction);

                if (worksheetAction != null && worksheetAction.Worksheet == sheet)
                {
                    undoActions.RemoveAt(i);
                    continue;
                }

                i++;
            }

            int totalActions = undoActions.Count;
            var redoActions = new List<IUndoableAction>(this.actionManager.RedoStack);

            for (int i = 0; i < redoActions.Count;)
            {
                IUndoableAction action = redoActions[i];

                var worksheetAction = (action as BaseWorksheetAction);

                if (worksheetAction != null && worksheetAction.Worksheet == sheet)
                {
                    redoActions.RemoveAt(i);
                    continue;
                }

                i++;
            }

            this.actionManager.RedoStack.Clear();

            for (int i = redoActions.Count - 1; i >= 0; i--)
            {
                this.actionManager.RedoStack.Push(redoActions[i]);
            }

            totalActions += redoActions.Count;

            if (totalActions <= 0)
            {
                this.lastReusableAction = null;
            }
            UpdateUndoRedoStatus();
        }

        /// <summary>
        /// Event fired before action perform.
        /// </summary>
        public event EventHandler<WorkbookActionEventArgs> BeforeActionPerform;

        /// <summary>
        /// Event fired when any action performed.
        /// </summary>
        public event EventHandler<WorkbookActionEventArgs> ActionPerformed;

        /// <summary>
        /// Event fired when Undo operation performed by user.
        /// </summary>
        public event EventHandler<WorkbookActionEventArgs> Undid;

        /// <summary>
        /// Event fired when Reod operation performed by user.
        /// </summary>
        public event EventHandler<WorkbookActionEventArgs> Redid;

        #endregion // Actions

        #region Script

        ///// <summary>
        ///// Get or set script content
        ///// </summary>
        //public string Script
        //{
        //    get { return this.Workbook.Script; }
        //    set { this.Workbook.Script = value; }
        //}

#if EX_SCRIPT
        // TODO: srm should have only one instance 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public unvell.ReoScript.ScriptRunningMachine Srm
        {
            get { return this.workbook.Srm; }
        }

        /// <summary>
        /// Run workbook script.
        /// </summary>
        /// <returns>Return value from script.</returns>
        public object RunScript()
        {
            return this.workbook.RunScript();
        }

        /// <summary>
        /// Run specified script by workbook.
        /// </summary>
        /// <param name="script">Script to be executed.</param>
        /// <returns>Return value from specified script.</returns>
        public object RunScript(string script = null)
        {
            return this.workbook.RunScript(script);
        }
#endif

        #endregion // Script

        #region Internal Exceptions
        /// <summary>
        /// Event raised when exception has been happened during internal operations.
        /// Usually the internal operations are raised by hot-keys pressed by end-user.
        /// </summary>
        public event EventHandler<ExceptionHappenEventArgs> ExceptionHappened;

        /// <summary>
        /// Notify that there are exceptions happen on any worksheet. 
        /// The event ExceptionHappened of workbook will be invoked.
        /// </summary>
        /// <param name="sheet">Worksheet where the exception happened.</param>
        /// <param name="ex">Exception to describe the details of error information.</param>
        public void NotifyExceptionHappen(Worksheet sheet, Exception ex)
        {
            if (this.Workbook != null)
            {
                this.Workbook.NotifyExceptionHappen(sheet, ex);
            }
        }
        #endregion // Internal Exceptions

        #region Cursors
#if WINFORM || WPF || AVALONIA
        private Cursor builtInCellsSelectionCursor = null;
        private Cursor builtInFullColSelectCursor = null;
        private Cursor builtInFullRowSelectCursor = null;
        private Cursor builtInEntireSheetSelectCursor = null;
        private Cursor builtInCrossCursor = null;

        private Cursor customCellsSelectionCursor = null;
        private Cursor defaultPickRangeCursor = null;
        private Cursor internalCurrentCursor = null;

        /// <summary>
        /// Get or set the mouse cursor on cells selection
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Cursor CellsSelectionCursor
        {
            get { return this.customCellsSelectionCursor ?? this.builtInCellsSelectionCursor; }
            set
            {
                this.customCellsSelectionCursor = value;
                this.internalCurrentCursor = value;
            }
        }

        /// <summary>
        /// Cursor symbol displayed when moving mouse over on row headers
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Cursor FullRowSelectionCursor { get; set; }

        /// <summary>
        /// Cursor symbol displayed when moving mouse over on column headers
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Cursor FullColumnSelectionCursor { get; set; }

        /// <summary>
        /// Get or set the mouse cursor of lead header part
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Cursor EntireSheetSelectionCursor { get; set; }

#if AVALONIA
        private static Cursor LoadCursorFromResource(byte[] res, bool center = true)
        {
            using var ms = new MemoryStream(res);
            RGImage bitmap = new(ms);
            if (center)
            {
                return new Cursor(bitmap, new Avalonia.PixelPoint((int)bitmap.Size.Width / 2, (int)bitmap.Size.Height / 2));
            }
            else
            {
                return new Cursor(bitmap, new Avalonia.PixelPoint(0, 0));
            }
        }

#else
        private static Cursor LoadCursorFromResource(byte[] res)
        {
            using (var ms = new MemoryStream(res))
            {
                return new Cursor(ms);
            }
        }
#endif

#endif // WINFORM || WPF
        #endregion Cursors

        #region Pick Range
#if WINFORM || WPF || AVALONIA
        /// <summary>
        /// Start to pick a range from current worksheet.
        /// </summary>
        /// <param name="onPicked">Callback function invoked after range is picked.</param>
        public void PickRange(Func<Worksheet, RangePosition, bool> onPicked)
        {
            this.PickRange(onPicked, this.defaultPickRangeCursor);
        }

        /// <summary>
        /// Start to pick a range from current worksheet.
        /// </summary>
        /// <param name="onPicked">Callback function invoked after range is picked.</param>
        /// <param name="pickerCursor">Cursor style during picking.</param>
        public void PickRange(Func<Worksheet, RangePosition, bool> onPicked, Cursor pickerCursor)
        {
            this.internalCurrentCursor = pickerCursor;

            this.CurrentWorksheet.PickRange((sheet, range) =>
            {
                bool ret = onPicked(sheet, range);
                return ret;
            });
        }

        /// <summary>
        /// Start to pick ranges and copy the styles to the picked range
        /// </summary>
        public void StartPickRangeAndCopyStyle()
        {
            this.CurrentWorksheet.StartPickRangeAndCopyStyle();
        }

        /// <summary>
        /// End pick range operation
        /// </summary>
        public void EndPickRange()
        {
            this.CurrentWorksheet.EndPickRange();

            this.internalCurrentCursor = (this.customCellsSelectionCursor ?? this.builtInCellsSelectionCursor);
        }
#endif // WINFORM || WPF
        #endregion // Pick Range

        #region Appearance

        /// <summary>
        /// Retrieve control instance of workbook.
        /// </summary>
        public CalcitaControl ControlInstance { get { return null; } }

        private ControlAppearanceStyle controlStyle = null;

        /// <summary>
        /// Control Style Settings
        /// </summary>
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public ControlAppearanceStyle ControlStyle
        {
            get { return this.controlStyle; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("ControlStyle", "cannot set ControlStyle to null");
                }

                if (this.controlStyle != value)
                {
                    if (this.controlStyle != null) this.controlStyle?.CurrentControl = null;
                    this.controlStyle = value;
                }
                //workbook.SetControlStyle(value);

                this.SheetCanvas?.renderer.ControlStyle = value;

                this.ApplyControlStyle();
            }
        }

        internal void ApplyControlStyle()
        {
            this.controlStyle?.CurrentControl = this;

            this.adapter?.Invalidate();
        }

        //private AppearanceStyle appearanceStyle = new AppearanceStyle(this);

        #endregion // Appearance




        protected override void OnPointerExited(Avalonia.Input.PointerEventArgs args)
        {

            if (this.CurrentWorksheet != null)
            {
                this.adapter.ChangeCursor(CursorStyle.PlatformDefault);
                this.CurrentWorksheet?.HoverPos = CellPosition.Empty;
            }
        }

#if PRINT
        /// <summary>
        /// Create a print session to print all worksheets.
        /// </summary>
        /// <returns>Print session to print specified worksheets.</returns>
        public Print.PrintSession CreatePrintSession()
        {
            return this.workbook.CreatePrintSession();
        }
#endif // PRINT

        #region SheetTabControl


        /// <summary>
        /// SheetTabVisible StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<bool> SheetTabVisibleProperty =
            AvaloniaProperty.Register<CalcitaControl, bool>(nameof(SheetTabVisible), true);

        /// <summary>
        /// Gets or sets the SheetTabVisible property. This StyledProperty
        /// indicates ....
        /// </summary>
        public bool SheetTabVisible
        {
            get => this.GetValue(SheetTabVisibleProperty);
            set => SetValue(SheetTabVisibleProperty, value);
        }

        /// <summary>
        /// Determines that whether or not to display the new button on sheet tab control.
        /// </summary>
        public bool SheetTabNewButtonVisible
        {
            get { return this.sheetTab?.NewButtonVisible == true; }
            set { this.sheetTab?.NewButtonVisible = value; }
        }
        #endregion // SheetTabControl

        #region Scroll

        /// <summary>
        /// Scroll current active worksheet.
        /// </summary>
        /// <param name="x">Scroll value on horizontal direction.</param>
        /// <param name="y">Scroll value on vertical direction.</param>
        public void ScrollCurrentWorksheet(RGFloat x, RGFloat y)
        {
            if (this.CurrentWorksheet?.ViewportController is IScrollableViewportController svc)
            {
                svc.ScrollViews(ScrollDirection.Both, x, y);

                svc.SynchronizeScrollBar();
            }
        }

        /// <summary>
        /// Event raised when current worksheet is scrolled.
        /// </summary>
        public event EventHandler<WorksheetScrolledEventArgs> WorksheetScrolled;

        /// <summary>
        /// Raise the event of worksheet scrolled.
        /// </summary>
        /// <param name="worksheet">Instance of scrolled worksheet.</param>
        /// <param name="x">Scroll value on horizontal direction.</param>
        /// <param name="y">Scroll value on vertical direction.</param>
        public void RaiseWorksheetScrolledEvent(Worksheet worksheet, RGFloat x, RGFloat y)
        {
            this.WorksheetScrolled?.Invoke(this, new WorksheetScrolledEventArgs(worksheet)
            {
                X = x,
                Y = y,
            });
        }

        private bool showScrollEndSpacing = true;

        [DefaultValue(100)]
        [Browsable(true)]
        [Description("Determines whether or not show the white spacing at bottom and right of worksheet.")]
        public bool ShowScrollEndSpacing
        {
            get { return this.showScrollEndSpacing; }
            set
            {
                if (this.showScrollEndSpacing != value)
                {
                    this.showScrollEndSpacing = value;
                    this.CurrentWorksheet.UpdateViewportController();
                }
            }
        }

        #endregion Scroll
    }
}


