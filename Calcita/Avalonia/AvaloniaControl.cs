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

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Calcita.AvaloniaPlatform;
using Calcita.Events;
using Calcita.Graphics;
using Calcita.Interaction;
using Calcita.Main;
using Calcita.Rendering;
using Calcita.Views;
using System;
using System.Linq;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using Point = Calcita.Graphics.Point;
using Size = Avalonia.Size;

namespace Calcita.Controls
{
    /// <summary>
    /// Calcita Spreadsheet Control
    /// </summary>
    [Avalonia.Controls.Metadata.TemplatePart("PART_SheetCanvas", typeof(SheetCanvas), IsRequired = true)]
    [Avalonia.Controls.Metadata.TemplatePart("PART_FormulaBar", typeof(FormulaBar))]
    [Avalonia.Controls.Metadata.TemplatePart("PART_HorizontalScrollBar", typeof(Avalonia.Controls.Primitives.ScrollBar))]
    [Avalonia.Controls.Metadata.TemplatePart("PART_VerticalScrollBar", typeof(Avalonia.Controls.Primitives.ScrollBar))]
    [Avalonia.Controls.Metadata.TemplatePart("PART_SheetTabControl", typeof(SheetTabControl))]
    public partial class CalcitaControl : TemplatedControl, IVisualWorkbook,
    IRangePickableControl, IContextMenuControl, IActionControl
    {
        internal const int ScrollBarSize = 18;

        internal ReoGridAvaloniaControlAdapter adapter;
        private SheetTabControl sheetTab;

        private ScrollBar horScrollbar;
        private ScrollBar verScrollbar;

        private SheetCanvas SheetCanvas;
        private FormulaBar formulaBar;
        private Action? formulaBarEditSessionEndedHandler;

        private readonly AvaloniaList<string> sheets = [];

        /// <summary>
        /// Create Calcita spreadsheet control
        /// </summary>
        public CalcitaControl()
        {
            this.Focusable = true;

            this.BeginInit();

            //this.horScrollbar.Scroll += (s, e) =>
            //{
            //    horScrollBar_Scroll(s, e);
            //};

            //this.verScrollbar.Scroll += (s, e) =>
            //{
            //    verScrollBar_Sroll(s, e);
            //};

            //this.sheetTab.NewSheetClick += SheetTab_NewSheetClick;
            //this.sheetTab.TabMoved += SheetTab_TabMoved;

            this.InitControl();


            InitWorkbook();

            this.EndInit();

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!string.IsNullOrEmpty(this.LoadFromFile))
                {
                    var file = new System.IO.FileInfo(this.LoadFromFile);
                    CurrentWorksheet?.Load(file.FullName);
                }
            }, Avalonia.Threading.DispatcherPriority.Input);



        }

        private void verScrollBar_Sroll(object? s, ScrollEventArgs e)
        {
            if (CurrentWorksheet?.ViewportController is IScrollableViewportController)
            {
                ((IScrollableViewportController)CurrentWorksheet.ViewportController).VerticalScroll(e.NewValue);
            }
        }

        private void horScrollBar_Scroll(object? s, ScrollEventArgs e)
        {
            if (CurrentWorksheet?.ViewportController is IScrollableViewportController)
            {
                ((IScrollableViewportController)CurrentWorksheet.ViewportController).HorizontalScroll(e.NewValue);
            }
        }

        private IDisposable?[] Disposables = [];
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            foreach (var disposable in Disposables)
            {
                disposable?.Dispose();
            }

            this.horScrollbar?.Scroll -= horScrollBar_Scroll;
            this.verScrollbar?.Scroll -= verScrollBar_Sroll;
            this.sheetTab?.NewSheetClick -= this.SheetTab_NewSheetClick;
            this.sheetTab?.TabMoved -= this.SheetTab_TabMoved;

            this.SheetCanvas = e.NameScope.Find<SheetCanvas>("PART_SheetCanvas");
            this.horScrollbar = e.NameScope.Find<ScrollBar>("PART_HorizontalScrollBar");
            this.verScrollbar = e.NameScope.Find<ScrollBar>("PART_VerticalScrollBar");
            this.sheetTab = e.NameScope.Find<SheetTabControl>("PART_SheetTabControl");

            if (this.formulaBar != null && this.formulaBarEditSessionEndedHandler != null)
            {
                this.formulaBar.EditSessionEnded -= this.formulaBarEditSessionEndedHandler;
            }

            this.formulaBar = e.NameScope.Find<FormulaBar>("PART_FormulaBar");

            if (this.formulaBar != null)
            {
                this.formulaBarEditSessionEndedHandler = () => this.adapter?.Focus();
                this.formulaBar.EditSessionEnded += this.formulaBarEditSessionEndedHandler;
            }

            this.horScrollbar?.SmallChange = Worksheet.InitDefaultColumnWidth;
            this.verScrollbar?.SmallChange = Worksheet.InitDefaultRowHeight;
            this.SheetCanvas?.Owner = this;

            var w = Workbook;
            if (w != null)
            {
                this.sheetTab?.ItemsSource = sheets;
            }

            this.adapter = new ReoGridAvaloniaControlAdapter(this);

            this.horScrollbar?.Scroll += horScrollBar_Scroll;
            this.verScrollbar?.Scroll += verScrollBar_Sroll;
            this.sheetTab?.NewSheetClick += this.SheetTab_NewSheetClick;
            this.sheetTab?.TabMoved += this.SheetTab_TabMoved;

            sheetTab?.SelectedIndexChanged += (s, e) =>
            {
                this.SelectedIndex = (s as SheetTabControl)!.SelectedIndex;
            };

            Disposables = [
                SheetCanvas?.Bind(SheetCanvas.WorksheetProperty, this[!CurrentWorksheetProperty]),
                sheetTab?.Bind(SheetTabControl.SelectedIndexProperty, this[!SelectedIndexProperty]),
                //bind(SheetCanvas, SheetCanvas.WorksheetProperty, () => this.GetObservable(CurrentWorksheetProperty)),

                bind(horScrollbar, ScrollBar.ValueProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.OffsetProperty, p => p.X)),
                bind(horScrollbar, ScrollBar.MaximumProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.ScrollBarMaximumProperty, p => p.X)),
                bind(horScrollbar, ScrollBar.MinimumProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.ScrollBarMinimumProperty, p => p.X)),
                bind(horScrollbar, ScrollBar.LargeChangeProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.LargeChangeProperty, p => p.Width)),
                bind(horScrollbar, ScrollBar.ViewportSizeProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.LargeChangeProperty, p => p.Width)),

                bind(verScrollbar, ScrollBar.ValueProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.OffsetProperty, p => p.Y)),
                bind(verScrollbar, ScrollBar.MaximumProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.ScrollBarMaximumProperty, p => p.Y)),
                bind(verScrollbar, ScrollBar.MinimumProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.ScrollBarMinimumProperty, p => p.Y)),
                bind(verScrollbar, ScrollBar.LargeChangeProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.LargeChangeProperty, p => p.Height)),
                bind(verScrollbar, ScrollBar.ViewportSizeProperty, () => this.SheetCanvas.GetObservable(SheetCanvas.LargeChangeProperty, p => p.Height)),

                bind(sheetTab, SheetTabControl.IsVisibleProperty, () => this.GetObservable(SheetTabVisibleProperty)),
                //bind(sheetTab, SheetTabControl.SelectedIndexProperty, ()=> this.GetObservable(SelectedIndexProperty)),
            ];

            this.CurrentWorksheet?.ControlAdapter = this.adapter;
            this.adapter.Invalidate();

            static IDisposable? bind<T>(Control? control, AvaloniaProperty<T> property, Func<IObservable<T>> observableFactory)
            {
                return control?.Bind(property, observableFactory(), Avalonia.Data.BindingPriority.Template);
            }
        }

        private void SheetTab_TabMoved(object? sender, SheetTabMovedEventArgs e)
        {
            var workbook = this.Workbook;

            if (workbook != null)
            {
                workbook.MoveWorksheet(e.Index, e.TargetIndex);
            }
        }

        private void SheetTab_NewSheetClick(object? sender, EventArgs e)
        {
            var workbook = this.Workbook;

            if (workbook != null)
            {
                var sheet = workbook.CreateWorksheet();
                workbook.AddWorksheet(sheet);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        #region Workbook

        /// <summary>
        /// Workbook StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<IWorkbook?> WorkbookProperty =
            AvaloniaProperty.Register<CalcitaControl, IWorkbook?>(nameof(Calcita.Workbook));

        /// <summary>
        /// Gets or sets the Workbook property. This StyledProperty
        /// indicates ....
        /// </summary>
        public IWorkbook? Workbook
        {
            get => this.GetValue(WorkbookProperty);
            set => SetValue(WorkbookProperty, value);
        }


        /// <summary>
        /// CurrentWorksheet StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<Worksheet?> CurrentWorksheetProperty =
            AvaloniaProperty.Register<CalcitaControl, Worksheet?>(nameof(CurrentWorksheet));

        /// <summary>
        /// Gets or sets the CurrentWorksheet property. This StyledProperty
        /// indicates ....
        /// </summary>
        public Worksheet? CurrentWorksheet
        {
            get => this.GetValue(CurrentWorksheetProperty);
            set => SetValue(CurrentWorksheetProperty, value);
        }

        /// <summary>
        /// SelectedIndex StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<int> SelectedIndexProperty =
            AvaloniaProperty.Register<CalcitaControl, int>(nameof(SelectedIndex), -1);

        /// <summary>
        /// Gets or sets the SelectedIndex property. This StyledProperty
        /// indicates ....
        /// </summary>
        public int SelectedIndex
        {
            get => this.GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        /// <summary>
        /// FormulaBarVisible StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<bool> FormulaBarVisibleProperty =
            AvaloniaProperty.Register<CalcitaControl, bool>(nameof(FormulaBarVisible), false);

        /// <summary>
        /// Gets or sets whether the formula bar is visible at the top of the control.
        /// </summary>
        public bool FormulaBarVisible
        {
            get => this.GetValue(FormulaBarVisibleProperty);
            set => SetValue(FormulaBarVisibleProperty, value);
        }

        private void OnWorkbookChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var old = e.GetOldValue<IWorkbook?>();
            var workbook = e.GetNewValue<IWorkbook?>();

            if (old != null)
            {
                old.WorkbookLoaded -= Workbook_WorkbookLoaded;
                old.WorkbookSaved -= Workbook_WorkbookSaved;

                old.WorksheetInserted -= Workbook_WorksheetInserted;
                old.WorksheetRemoved -= WorkBook_WorksheetRemoved;
                old.WorksheetMoved -= WorkBook_WorksheetMoved;

                old.WorksheetNameChanged -= Workbook_WorksheetNameChanged;
                old.WorksheetNameBackColorChanged -= Workbook_WorksheetNameBackColorChanged;
                old.WorksheetNameTextColorChanged -= Workbook_WorksheetNameTextColorChanged;

                old.ExceptionHappened -= Workbook_ExceptionHappened;

                if (old is Workbook w)
                {
                    w.WorkbookSaving -= Workbook_WorkbookSaving;
                    w.WorkbookLoading -= Workbook_WorkbookLoading;
                }
            }

            this.sheets.Clear();
            this.actionManager.Reset();
            this.UpdateUndoRedoStatus();
            this.CurrentWorksheet = null;

            if (workbook != null)
            {
                workbook.WorkbookLoaded += Workbook_WorkbookLoaded;
                workbook.WorkbookSaved += Workbook_WorkbookSaved;

                workbook.WorksheetInserted += Workbook_WorksheetInserted;
                workbook.WorksheetRemoved += WorkBook_WorksheetRemoved;
                workbook.WorksheetMoved += WorkBook_WorksheetMoved;

                workbook.WorksheetNameChanged += Workbook_WorksheetNameChanged;
                workbook.WorksheetNameBackColorChanged += Workbook_WorksheetNameBackColorChanged;
                workbook.WorksheetNameTextColorChanged += Workbook_WorksheetNameTextColorChanged;

                workbook.ExceptionHappened += Workbook_ExceptionHappened;

                if (workbook is Workbook w)
                {
                    this.Workbook = w;

                    w.WorkbookSaving += Workbook_WorkbookSaving;
                    w.WorkbookLoading += Workbook_WorkbookLoading;
                }

                foreach (var sheet in workbook.Worksheets)
                {
                    this.sheets.Add(sheet.Name);
                }
                this.CurrentWorksheet = workbook.Worksheets.FirstOrDefault();

            }
        }

        private void OnCurrentWorksheetChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var oldSheet = e.GetOldValue<Worksheet?>();

            if (oldSheet != null)
            {
                if (oldSheet != null && oldSheet.IsEditing)
                {
                    oldSheet.EndEdit(EndEditReason.NormalFinish);
                }
                oldSheet?.ControlAdapter = null;
            }
            var newSheet = e.GetNewValue<Worksheet?>();
            if (newSheet != null)
            {
                newSheet.ControlAdapter = this.adapter;
                // update bounds for viewport of worksheet
                newSheet.UpdateViewportControllBounds();

                // update bounds for viewport of worksheet
                if (newSheet.ViewportController is IScrollableViewportController scrollableViewportController)
                {
                    scrollableViewportController.SynchronizeScrollBar();
                }

                this.SelectedIndex = this.Workbook?.GetWorksheetIndex(newSheet) ?? -1;
            }
            else
            {
                this.SelectedIndex = -1;
            }
            this.adapter?.Invalidate();
        }

        private void Workbook_WorkbookSaving(object? sender, EventArgs e)
        {
            this.adapter?.ChangeCursor(CursorStyle.Busy);
        }
        private void Workbook_WorkbookLoading(object? sender, EventArgs e)
        {
            this.adapter?.ChangeCursor(CursorStyle.Busy);
        }

        private void Workbook_WorkbookSaved(object? sender, EventArgs e)
        {
            this.WorkbookSaved?.Invoke(sender, e);
        }

        private void Workbook_WorkbookLoaded(object? sender, EventArgs e)
        {
            if (this.Workbook.Worksheets.Count <= 0)
            {
                CurrentWorksheet = null;
            }
            else
            {
                if (CurrentWorksheet != this.Workbook.Worksheets[0])
                {
                    CurrentWorksheet = this.Workbook.Worksheets[0];
                }
                else
                {
                    CurrentWorksheet.UpdateViewportControllBounds();
                }
            }

            this.WorkbookLoaded?.Invoke(sender, e);
        }

        private void Workbook_ExceptionHappened(object? sender, Events.ExceptionHappenEventArgs e)
        {
            this.ExceptionHappened?.Invoke(this, e);
        }

        private void Workbook_WorksheetNameTextColorChanged(object? sender, Events.WorksheetEventArgs e)
        {
            var workbook = this.Workbook as Workbook;
            if (workbook != null)
            {
                var index = workbook.GetWorksheetIndex(e.Worksheet);
                var worksheet = e.Worksheet;
                this.sheets[index] = worksheet.Name;//.UpdateTab(index, worksheet.Name, worksheet.NameBackColor, worksheet.NameTextColor);
            }
        }

        private void Workbook_WorksheetNameBackColorChanged(object? sender, Events.WorksheetEventArgs e)
        {
            var workbook = this.Workbook as Workbook;
            if (workbook != null)
            {
                var index = workbook.GetWorksheetIndex(e.Worksheet);
                var worksheet = e.Worksheet;
                this.sheets[index] = worksheet.Name;//.UpdateTab(index, worksheet.Name, worksheet.NameBackColor, worksheet.NameTextColor);
            }
        }

        private void Workbook_WorksheetNameChanged(object? sender, Events.WorksheetNameChangingEventArgs e)
        {
            var workbook = this.Workbook as Workbook;
            if (workbook != null)
            {
                var index = workbook.GetWorksheetIndex(e.Worksheet);
                var worksheet = e.Worksheet;

                this.sheets[index] = e.NewName;//.UpdateTab(index, e.NewName, worksheet.NameBackColor, worksheet.NameTextColor);
            }
        }

        private void WorkBook_WorksheetMoved(object? sender, Events.WorksheetMovedEventArgs e)
        {
            var workbook = this.Workbook;
            if (workbook != null)
            {
                var sheet = workbook.Worksheets[e.NewIndex];

                this.sheets.RemoveAt(e.Index);
                // sheet management
                this.sheets.Insert(e.NewIndex, sheet.Name);
            }
        }

        private void WorkBook_WorksheetRemoved(object? sender, Events.WorksheetRemovedEventArgs e)
        {
            var workbook = this.Workbook;

            this.sheets.RemoveAt(e.Index);


            this.ClearActionHistoryForWorksheet(e.Worksheet);

            if (workbook?.Worksheets.Count > 0)
            {
                int index = this.SelectedIndex;

                if (index >= workbook.Worksheets.Count)
                {
                    index = workbook.Worksheets.Count - 1;
                }

                this.SelectedIndex = index;
                CurrentWorksheet = workbook.Worksheets[this.SelectedIndex];
            }
            else
            {
                this.SelectedIndex = -1;
                CurrentWorksheet = null;
            }

            this.adapter?.Invalidate();
        }

        private void Workbook_WorksheetInserted(object? sender, Events.WorksheetInsertedEventArgs e)
        {
            var workbook = this.Workbook;
            if (workbook != null)
            {
                var index = e.Index;
                var sheet = workbook.Worksheets[index];

                // sheet management
                this.sheets?.Insert(index, sheet.Name);
                this.SelectedIndex = index;

                // update current worksheet
                if (this.adapter != null && this.adapter.ControlInstance.CurrentWorksheet == null)
                {
                    this.adapter.ControlInstance.CurrentWorksheet = sheet;
                }
            }
        }

        #endregion

        #region Adapter
        internal class ReoGridAvaloniaControlAdapter : IControlAdapter
        {
            #region Constructor
            private readonly CalcitaControl canvas;
            internal InputTextBox editTextbox => canvas.SheetCanvas.editTextbox;

            private CellEditMode ActiveEditMode => this.canvas.CurrentWorksheet?.EditMode ?? CellEditMode.InCell;
            private FormulaBar? FormulaBar => this.canvas.formulaBar;

            internal ReoGridAvaloniaControlAdapter(CalcitaControl canvas)
            {
                this.canvas = canvas;
            }
            #endregion // Constructor

            #region IControlAdapter Members

            public IVisualWorkbook ControlInstance
            {
                get { return this.canvas; }
            }

            public ControlAppearanceStyle ControlStyle { get { return this.canvas.controlStyle; } }

            public IRenderer Renderer { get { return this.canvas.SheetCanvas?.renderer; } }

            public void ShowContextMenuStrip(ViewTypes viewType, Graphics.Point containerLocation)
            {
                var flyout = viewType switch
                {
                    ViewTypes.ColumnHeader => this.canvas.ColumnHeaderContextFlyout,
                    ViewTypes.RowHeader => this.canvas.RowHeaderContextFlyout,
                    ViewTypes.LeadHeader => this.canvas.LeadHeaderContextFlyout,
                    _ => this.canvas.CellsContextFlyout,
                };
                if (flyout != null)
                {
                    flyout.SetValue(Flyout.PlacementProperty, PlacementMode.Pointer);
                    flyout.ShowAt(this.canvas.SheetCanvas);
                }
            }

            private Cursor oldCursor = null;

            public void ChangeCursor(CursorStyle cursor)
            {
                oldCursor = this.canvas.Cursor;

                this.canvas.Cursor = cursor switch
                {
                    CursorStyle.PlatformDefault => new Cursor(StandardCursorType.Arrow),
                    CursorStyle.Hand => new Cursor(StandardCursorType.Hand),
                    CursorStyle.Selection => this.canvas.internalCurrentCursor,
                    CursorStyle.FullRowSelect => this.canvas.builtInFullRowSelectCursor,
                    CursorStyle.FullColumnSelect => this.canvas.builtInFullColSelectCursor,
                    CursorStyle.EntireSheet => this.canvas.builtInEntireSheetSelectCursor,
                    CursorStyle.Move => new Cursor(StandardCursorType.SizeAll),
                    //CursorStyle.Copy => throw new NotImplementedException(),
                    CursorStyle.ChangeColumnWidth => new Cursor(StandardCursorType.SizeWestEast),
                    CursorStyle.ChangeRowHeight => new Cursor(StandardCursorType.SizeNorthSouth),
                    CursorStyle.ResizeHorizontal => new Cursor(StandardCursorType.SizeWestEast),
                    CursorStyle.ResizeVertical => new Cursor(StandardCursorType.SizeNorthSouth),
                    CursorStyle.Busy => new Cursor(StandardCursorType.AppStarting),
                    CursorStyle.Cross => this.canvas.builtInCrossCursor,
                    _ => new Cursor(StandardCursorType.Arrow)
                };
            }

            public void RestoreCursor()
            {
                this.canvas.Cursor = oldCursor;
            }

            public void ChangeSelectionCursor(CursorStyle cursor)
            {
                switch (cursor)
                {
                    default:
                    case CursorStyle.PlatformDefault:
                        this.canvas.internalCurrentCursor = new Cursor(StandardCursorType.Arrow);
                        break;

                    case CursorStyle.Hand:
                        this.canvas.internalCurrentCursor = new Cursor(StandardCursorType.Hand);
                        break;
                }
            }

            public Rectangle GetContainerBounds()
            {
                return this.canvas.SheetCanvas?.Bounds.WithX(0).WithY(0) ?? new Rect(0, 0, 0, 0);

                double w = this.canvas.Bounds.Width;
                double h = this.canvas.Bounds.Height + 1;

                if (this.canvas.verScrollbar.IsVisible)
                {
                    w -= ScrollBarSize;
                }

                if (this.canvas.sheetTab.IsVisible
                    || this.canvas.horScrollbar.IsVisible)
                {
                    h -= ScrollBarSize;
                }

                if (w < 0) w = 0;
                if (h < 0) h = 0;

                return new Rectangle(0, 0, w, h);
            }

            public void Focus()
            {
                this.canvas.SheetCanvas.Focus();
            }

            public void Invalidate()
            {
                this.canvas.SheetCanvas?.Invalidate();
            }

            //public void ChangeBackColor(Color color)
            //{
            //    ((Canvas)this.canvas.Child).Background = new SolidColorBrush(color);
            //}

            public bool IsVisible
            {
                get { return this.canvas.IsVisible; }
            }

            public Graphics.Point PointToScreen(Graphics.Point p)
            {
                var pixelPoint = this.canvas.SheetCanvas.PointToScreen(p);
                return new Point(pixelPoint.X, pixelPoint.Y);
            }

            public IGraphics PlatformGraphics { get { return null; } }

            public void ChangeBackgroundColor(SolidColor color)
            {
            }

            public void ShowTooltip(Graphics.Point point, string content)
            {
                // not implemented
            }

            public ISheetTabControl SheetTabControl
            {
                get { return this.canvas.sheetTab; }
            }

            public double BaseScale { get { return 0f; } }
            public double MinScale { get { return 0.1f; } }
            public double MaxScale { get { return 4f; } }

            #endregion // IControlAdapter Members

            #region IEditableControlInterface Members

            public void ShowEditControl(Graphics.Rectangle bounds, Cell cell)
            {
                if (ActiveEditMode == CellEditMode.FormulaBar) return;

                var sheet = this.canvas.CurrentWorksheet;

                Color textColor;

                if (!cell.RenderColor.IsTransparent)
                {
                    textColor = cell.RenderColor;
                }
                else if (cell.InnerStyle.HasStyle(PlainStyleFlag.TextColor))
                {
                    // cell text color, specified by SetRangeStyle
                    textColor = cell.InnerStyle.TextColor;
                }
                else
                {
                    // default cell text color
                    textColor = this.canvas.controlStyle[ControlAppearanceColors.GridText];
                }

                Canvas.SetLeft(this.editTextbox, bounds.X);
                Canvas.SetTop(this.editTextbox, bounds.Y);

                this.editTextbox.Width = bounds.Width - 1;
                this.editTextbox.Height = bounds.Height - 1;

                this.editTextbox.CellSize = cell.Bounds.Size;
                this.editTextbox.VAlign = cell.InnerStyle.VAlign;
                this.editTextbox.FontFamily = new FontFamily(cell.InnerStyle.FontName);
                this.editTextbox.FontSize = cell.InnerStyle.FontSize * sheet.ScaleFactor * 96f / 72f;
                this.editTextbox.FontStyle = PlatformUtility.ToAvaloniaFontStyle(cell.InnerStyle.fontStyles);
                this.editTextbox.Foreground = this.Renderer.GetBrush(textColor);
                this.editTextbox.Background = this.Renderer.GetBrush(cell.InnerStyle.HasStyle(PlainStyleFlag.BackColor)
                    ? cell.InnerStyle.BackColor : this.canvas.controlStyle[ControlAppearanceColors.GridBackground]);
                this.editTextbox.SelectionStart = this.editTextbox.Text.Length;
                this.editTextbox.TextWrap = cell.InnerStyle.TextWrapMode != TextWrapMode.NoWrap;
                this.editTextbox.TextWrapping = (cell.InnerStyle.TextWrapMode == TextWrapMode.NoWrap)
                    ? TextWrapping.NoWrap : TextWrapping.Wrap;

                this.editTextbox.IsVisible = true;
                this.editTextbox.Focus();
            }

            public void HideEditControl()
            {
                this.editTextbox.IsVisible = false;
            }

            public void SetEditControlText(string text)
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    if (FormulaBar != null) FormulaBar.EditText = text;
                }
                else
                {
                    this.editTextbox.Text = text;
                }
            }

            public string GetEditControlText()
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    return FormulaBar?.EditText ?? string.Empty;
                }

                return this.editTextbox.Text;
            }

            public void EditControlSelectAll()
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    FormulaBar?.SelectAllEditBox();
                }
                else
                {
                    this.editTextbox.SelectAll();
                }
            }

            public void SetEditControlCaretPos(int pos)
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    FormulaBar?.SetCaretPos(pos);
                }
                else
                {
                    this.editTextbox.SelectionStart = pos;
                }
            }

            public int GetEditControlCaretPos()
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    return FormulaBar?.GetCaretPos() ?? 0;
                }

                return this.editTextbox.SelectionStart;
            }

            public int GetEditControlCaretLine()
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    return FormulaBar?.GetCaretPos() ?? 0;
                }

                return this.editTextbox.CaretIndex;
                //this.editTextbox.TextLayout.GetLineIndexFromCharacterIndex(this.editTextbox.SelectionStart,false);
            }

            public void SetEditControlAlignment(ReoGridHorAlign align)
            {
                switch (align)
                {
                    default:
                    case ReoGridHorAlign.Left:
                        this.editTextbox.HorizontalAlignment = HorizontalAlignment.Left;
                        break;

                    case ReoGridHorAlign.Center:
                    case ReoGridHorAlign.DistributedIndent:
                        this.editTextbox.HorizontalAlignment = HorizontalAlignment.Center;
                        break;

                    case ReoGridHorAlign.Right:
                        this.editTextbox.HorizontalAlignment = HorizontalAlignment.Right;
                        break;
                }
            }

            public void EditControlApplySystemMouseDown()
            {
                if (this.editTextbox.Text == null)
                    return;

                //Point p = System.Windows.Input.Mouse.GetPosition(this.editTextbox);

                //p.X += 2; // fix 2 pixels (borders of left and right)
                //p.Y -= 1; // fix 1 pixels (top)

                int caret = this.editTextbox.CaretIndex; //this.editTextbox.TextLayout.GetCharacterIndexFromPoint(p, true);

                if (caret >= 0 && caret <= this.editTextbox.Text.Length)
                {
                    this.editTextbox.SelectionStart = caret;
                }

                this.editTextbox.Focus();
            }

            public void EditControlCopy()
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    FormulaBar?.EditTextBox?.Copy();
                }
                else
                {
                    this.editTextbox.Copy();
                }
            }

            public void EditControlPaste()
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    FormulaBar?.EditTextBox?.Paste();
                }
                else
                {
                    this.editTextbox.Paste();
                }
            }

            public void EditControlCut()
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    FormulaBar?.EditTextBox?.Cut();
                }
                else
                {
                    this.editTextbox.Cut();
                }
            }

            public void EditControlUndo()
            {
                if (ActiveEditMode == CellEditMode.FormulaBar)
                {
                    FormulaBar?.EditTextBox?.Undo();
                }
                else
                {
                    this.editTextbox.Undo();
                }
            }
            #endregion

            #region IScrollableControlInterface Members

            public bool ScrollBarHorizontalVisible
            {
                get => this.canvas.HorizontalScrollBarVisible;
                set => this.canvas.HorizontalScrollBarVisible = value;
            }

            public bool ScrollBarVerticalVisible
            {
                get => this.canvas.VerticalScrollBarVisible;
                set => this.canvas.VerticalScrollBarVisible = value;
            }

            public double ScrollBarHorizontalMaximum
            {
                get { return this.canvas.SheetCanvas.ScrollBarMaximum.X; }
                set { Dispatcher.UIThread.InvokeAsync(() => this.canvas.SheetCanvas.ScrollBarMaximum = this.canvas.SheetCanvas.ScrollBarMaximum.WithX(value)); }
            }

            public double ScrollBarHorizontalMinimum
            {
                get { return this.canvas.SheetCanvas.ScrollBarMinimum.X; }
                set { Dispatcher.UIThread.InvokeAsync(() => this.canvas.SheetCanvas.ScrollBarMinimum = this.canvas.SheetCanvas.ScrollBarMinimum.WithX(value)); }
            }

            public double ScrollBarHorizontalValue
            {
                get { return this.canvas.SheetCanvas.Offset.X; }
                set { this.canvas.SheetCanvas.Offset = this.canvas.SheetCanvas.Offset.WithX(value); }
            }

            public double ScrollBarHorizontalLargeChange
            {
                get => this.canvas.SheetCanvas.LargeChange.Width;
                set => this.canvas.SheetCanvas.LargeChange = this.canvas.SheetCanvas.LargeChange.WithWidth(value);
            }

            public double ScrollBarVerticalMaximum
            {
                get { return this.canvas.SheetCanvas.ScrollBarMaximum.Y; }
                set { Dispatcher.UIThread.InvokeAsync(() => this.canvas.SheetCanvas.ScrollBarMaximum = this.canvas.SheetCanvas.ScrollBarMaximum.WithY(value)); }
            }

            public double ScrollBarVerticalMinimum
            {
                get { return this.canvas.SheetCanvas.ScrollBarMinimum.Y; }
                set { Dispatcher.UIThread.InvokeAsync(() => this.canvas.SheetCanvas.ScrollBarMinimum = this.canvas.SheetCanvas.ScrollBarMinimum.WithY(value)); }
            }

            public double ScrollBarVerticalValue
            {
                get { return this.canvas.SheetCanvas.Offset.Y; }
                set { this.canvas.SheetCanvas.Offset = this.canvas.SheetCanvas.Offset.WithY(value); }
            }

            public double ScrollBarVerticalLargeChange
            {
                get => this.canvas.SheetCanvas.LargeChange.Height;
                set => this.canvas.SheetCanvas.LargeChange = this.canvas.SheetCanvas.LargeChange.WithHeight(value);
            }

            #endregion

            #region ITimerSupportedControlInterface Members

            public void StartTimer()
            {
                throw new NotImplementedException();
            }

            public void StopTimer()
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion // Adapter
        #region Editor - TextBox

        #endregion // Editor - TextBox

        #region Context Menu Strips

        /// <summary>
        /// CellsContextFlyout StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<FlyoutBase?> CellsContextFlyoutProperty =
            AvaloniaProperty.Register<CalcitaControl, FlyoutBase?>(nameof(CellsContextFlyout));

        /// <summary>
        /// RowHeaderContextFlyout StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<FlyoutBase?> RowHeaderContextFlyoutProperty =
            AvaloniaProperty.Register<CalcitaControl, FlyoutBase?>(nameof(RowHeaderContextFlyout));

        /// <summary>
        /// ColumnHeaderContextFlyout StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<FlyoutBase?> ColumnHeaderContextFlyoutProperty =
            AvaloniaProperty.Register<CalcitaControl, FlyoutBase?>(nameof(ColumnHeaderContextFlyout));

        /// <summary>
        /// LeadHeaderContextFlyout StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<FlyoutBase?> LeadHeaderContextFlyoutProperty =
            AvaloniaProperty.Register<CalcitaControl, FlyoutBase?>(nameof(LeadHeaderContextFlyout));

        /// <summary>
        /// Get or set the cells context flyout
        /// </summary>
        public FlyoutBase? CellsContextFlyout
        {
            get => GetValue(CellsContextFlyoutProperty);
            set => SetValue(CellsContextFlyoutProperty, value);
        }

        /// <summary>
        /// Get or set the row header context flyout
        /// </summary>
        public FlyoutBase? RowHeaderContextFlyout
        {
            get => GetValue(RowHeaderContextFlyoutProperty);
            set => SetValue(RowHeaderContextFlyoutProperty, value);
        }

        /// <summary>
        /// Get or set the column header context flyout
        /// </summary>
        public FlyoutBase? ColumnHeaderContextFlyout
        {
            get => GetValue(ColumnHeaderContextFlyoutProperty);
            set => SetValue(ColumnHeaderContextFlyoutProperty, value);
        }

        /// <summary>
        /// Get or set the lead header context flyout
        /// </summary>
        public FlyoutBase? LeadHeaderContextFlyout
        {
            get => GetValue(LeadHeaderContextFlyoutProperty);
            set => SetValue(LeadHeaderContextFlyoutProperty, value);
        }

        #endregion // Context Menu Strips

        #region ScrollBar

        /// <summary>
        /// HorizontalScrollBarVisible StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<bool> HorizontalScrollBarVisibleProperty =
            AvaloniaProperty.Register<CalcitaControl, bool>(nameof(HorizontalScrollBarVisible), true);

        /// <summary>
        /// Gets or sets the HorizontalScrollBarVisible property. This StyledProperty
        /// indicates ....
        /// </summary>
        public bool HorizontalScrollBarVisible
        {
            get => this.GetValue(HorizontalScrollBarVisibleProperty);
            set => SetValue(HorizontalScrollBarVisibleProperty, value);
        }

        /// <summary>
        /// VerticalScrollBarVisible StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<bool> VerticalScrollBarVisibleProperty =
            AvaloniaProperty.Register<CalcitaControl, bool>(nameof(VerticalScrollBarVisible), true);

        /// <summary>
        /// Gets or sets the VerticalScrollBarVisible property. This StyledProperty
        /// indicates ....
        /// </summary>
        public bool VerticalScrollBarVisible
        {
            get => this.GetValue(VerticalScrollBarVisibleProperty);
            set => SetValue(VerticalScrollBarVisibleProperty, value);
        }

        #endregion

        /// <summary>
        /// Get or set filepath of startup template file
        /// </summary>
        public string LoadFromFile { get; set; }

        public void Dispose() { }
    }
}
