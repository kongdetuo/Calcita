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
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Styling;
using Avalonia.Threading;
using Calcita.AvaloniaPlatform;
using Calcita.Graphics;
using Calcita.Interaction;
using Calcita.Main;
using Calcita.Rendering;
using Calcita.Views;
using System;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using Point = Calcita.Graphics.Point;
using Size = Avalonia.Size;

namespace Calcita
{
    /// <summary>
    /// Calcita Spreadsheet Control
    /// </summary>
    public partial class CalcitaControl : Decorator, IVisualWorkbook,
    IRangePickableControl, IContextMenuControl, IPersistenceWorkbook, IActionControl, IWorkbook
    {
        internal const int ScrollBarSize = 18;

        internal ReoGridAvaloniaControlAdapter adapter;
        private SheetTabControl sheetTab;
        private InputTextBox editTextbox => SheetCanvas.editTextbox;

        private ScrollBar horScrollbar;
        private ScrollBar verScrollbar;

        private SheetCanvas SheetCanvas;

        /// <summary>
        /// Create Calcita spreadsheet control
        /// </summary>
        public CalcitaControl()
        {
            this.Styles.Add(new StyleInclude(new Uri("avares://Calcita/"))
            {
                Source = new Uri("avares://Calcita/Avalonia/Theme/Styles.axaml")
            });

            this.Focusable = true;

            this.BeginInit();

            this.sheetTab = new SheetTabControl()
            {
                ControlWidth = 400,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            this.horScrollbar = new ScrollBar()
            {
                Orientation = Orientation.Horizontal,
                Height = ScrollBarSize,
                SmallChange = Worksheet.InitDefaultColumnWidth,
                [Grid.RowProperty] = 0,
                [Grid.ColumnProperty] = 2,
                [!ScrollBar.IsVisibleProperty] = this[!HorizontalScrollBarVisibleProperty],
                [!ScrollBar.ValueProperty] = this.GetObservable(OffsetProperty, p => p.X).ToBinding(),
                [!ScrollBar.MaximumProperty] = this.GetObservable(ScrollBarMaximumProperty, p => p.X).ToBinding(),
                [!ScrollBar.MinimumProperty] = this.GetObservable(ScrollBarMinimumProperty, p => p.X).ToBinding(),
                [!ScrollBar.LargeChangeProperty] = this.GetObservable(LargeChangeProperty, p => p.Width).ToBinding(),
                [!ScrollBar.ViewportSizeProperty] = this.GetObservable(LargeChangeProperty, p => p.Width).ToBinding(),
            };

            this.verScrollbar = new ScrollBar()
            {
                [Grid.ColumnProperty] = 1,

                Orientation = Orientation.Vertical,
                Width = ScrollBarSize,
                SmallChange = Worksheet.InitDefaultRowHeight,
                [!ScrollBar.IsVisibleProperty] = this[!VerticalScrollBarVisibleProperty],
                [!ScrollBar.ValueProperty] = this.GetObservable(OffsetProperty, p=>p.Y).ToBinding(),
                [!ScrollBar.MaximumProperty] = this.GetObservable(ScrollBarMaximumProperty, p => p.Y).ToBinding(),
                [!ScrollBar.MinimumProperty] = this.GetObservable(ScrollBarMinimumProperty, p => p.Y).ToBinding(),
                [!ScrollBar.LargeChangeProperty] = this.GetObservable(LargeChangeProperty, p => p.Height).ToBinding(),
                [!ScrollBar.ViewportSizeProperty] = this.GetObservable(LargeChangeProperty, p => p.Height).ToBinding(),
            };

            this.SheetCanvas = new SheetCanvas()
            {
                Owner = this,
            };

            this.Child = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitions("* auto"),
                RowDefinitions = new RowDefinitions("* auto"),
                [!Grid.BackgroundProperty] = this.Resources.GetResourceObservable("ThemeControlMidBrush").ToBinding(),

                Children =
                {
                    SheetCanvas,
                    verScrollbar,
                    new Grid()
                    {
                        [Grid.RowProperty] = 1,
                        Height = 24,
                        ColumnDefinitions = new ColumnDefinitions("*, auto, 400"),
                        Children = {
                            sheetTab,
                            new GridSplitter()
                            {
                                [Grid.ColumnProperty] = 1,
                                HorizontalAlignment = HorizontalAlignment,
                                ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                            },
                            horScrollbar
                        }
                    },
                },
            };

            this.horScrollbar.Scroll += (s, e) =>
            {
                if (this.currentWorksheet.ViewportController is IScrollableViewportController)
                {
                    ((IScrollableViewportController)this.currentWorksheet.ViewportController).HorizontalScroll(e.NewValue);
                }
            };

            this.verScrollbar.Scroll += (s, e) =>
            {
                if (this.currentWorksheet.ViewportController is IScrollableViewportController)
                {
                    ((IScrollableViewportController)this.currentWorksheet.ViewportController).VerticalScroll(e.NewValue);
                }
            };

            this.sheetTab.NewSheetClick += SheetTab_NewSheetClick;
            this.sheetTab.TabMoved += SheetTab_TabMoved;

            this.InitControl();

            this.adapter = new ReoGridAvaloniaControlAdapter(this);
            this.adapter.editTextbox = this.editTextbox;

            InitWorkbook();

            this.EndInit();

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!string.IsNullOrEmpty(this.LoadFromFile))
                {
                    var file = new System.IO.FileInfo(this.LoadFromFile);
                    this.currentWorksheet.Load(file.FullName);
                }
            }, Avalonia.Threading.DispatcherPriority.Input);



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


        #region SheetTab & Scroll Bars Visibility


        private void SetHorizontalScrollBarSize()
        {
            double hsbWidth = this.Bounds.Width;

            if (this.sheetTab.IsVisible)
            {
                hsbWidth -= this.SheetTabWidth;
            }

            if (this.verScrollbar.IsVisible)
            {
                hsbWidth -= ScrollBarSize;
            }

            if (hsbWidth < 0) hsbWidth = 0;
            this.horScrollbar.Width = hsbWidth;
        }

        private void SetSheetTabSize()
        {
            double stWidth = 0;

            if (this.horScrollbar.IsVisible)
            {
                stWidth = this.SheetTabWidth;
            }
            else
            {
                stWidth = this.Width;
            }

            if (this.verScrollbar.IsVisible)
            {
                stWidth -= ScrollBarSize;
            }

            if (stWidth < 0) stWidth = 0;
            this.horScrollbar.Width = stWidth;
        }

        internal void UpdateSheetTabAndScrollBarsLayout()
        {
            //Canvas.SetTop(this.sheetTab, this.Bounds.Size.Height - ScrollBarSize);
            //Canvas.SetTop(this.horScrollbar, this.Bounds.Size.Height - ScrollBarSize);

            //this.sheetTab.Height = ScrollBarSize;
            //this.horScrollbar.Height = ScrollBarSize;

            //Canvas.SetLeft(verScrollbar, this.Bounds.Size.Width - ScrollBarSize);

            //var vsbHeight = this.Bounds.Size.Height - ScrollBarSize;
            //if (vsbHeight < 0) vsbHeight = 0;
            //verScrollbar.Height = vsbHeight;

            //if (this.sheetTab.IsVisible
            //    && this.horScrollbar.IsVisible)
            //{
            //    this.sheetTab.Width = this.SheetTabWidth;

            //    Canvas.SetLeft(this.horScrollbar, this.SheetTabWidth);
            //    SetHorizontalScrollBarSize();
            //}
            //else if (this.sheetTab.IsVisible)
            //{
            //    this.sheetTab.Width = this.Width;
            //}
            //else if (this.horScrollbar.IsVisible)
            //{
            //    Canvas.SetLeft(this.horScrollbar, 0);
            //    SetHorizontalScrollBarSize();
            //}
            //else
            //{
            //    this.verScrollbar.Height = this.Bounds.Size.Height;
            //}

            this.currentWorksheet?.UpdateViewportControllBounds();
        }

        private void ShowSheetTabControl()
        {
            if (!this.sheetTab.IsVisible)
            {
                this.sheetTab.IsVisible = true;
                this.UpdateSheetTabAndScrollBarsLayout();
            }
        }

        private void HideSheetTabControl()
        {
            if (this.sheetTab.IsVisible)
            {
                this.sheetTab.IsVisible = false;
                this.UpdateSheetTabAndScrollBarsLayout();
            }
        }

        private void ShowHorScrollBar()
        {
            if (!this.horScrollbar.IsVisible)
            {
                this.horScrollbar.IsVisible = true;
                this.UpdateSheetTabAndScrollBarsLayout();
            }
        }

        private void HideHorScrollBar()
        {
            if (this.horScrollbar.IsVisible)
            {
                this.horScrollbar.IsVisible = false;
                this.UpdateSheetTabAndScrollBarsLayout();
            }
        }

        private void ShowVerScrollBar()
        {
            if (!this.verScrollbar.IsVisible)
            {
                this.verScrollbar.IsVisible = true;
                this.UpdateSheetTabAndScrollBarsLayout();
            }
        }

        private void HideVerScrollBar()
        {
            if (this.verScrollbar.IsVisible)
            {
                this.verScrollbar.IsVisible = false;
                this.UpdateSheetTabAndScrollBarsLayout();
            }
        }

        #endregion // SheetTab & Scroll Bars Visibility

        #region Workbook

        /// <summary>
        /// Workbook StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<IWorkbook?> WorkbookProperty =
            AvaloniaProperty.Register<CalcitaControl, IWorkbook?>(nameof(Workbook));

        /// <summary>
        /// Gets or sets the Workbook property. This StyledProperty
        /// indicates ....
        /// </summary>
        public IWorkbook? Workbook
        {
            get => this.GetValue(WorkbookProperty);
            set => SetValue(WorkbookProperty, value);
        }

        private void OnWorkbookChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var old = e.GetOldValue<IWorkbook?>();
            var workbook = e.GetNewValue<IWorkbook?>();

            if(old != null)
            {
                old.WorkbookLoaded -= Workbook_WorkbookLoaded;
                old.WorkbookSaved -= Workbook_WorkbookSaved;

                old.WorksheetCreated -= Workbook_WorksheetCreated;
                old.WorksheetInserted -= Workbook_WorksheetInserted;
                old.WorksheetRemoved -= WorkBook_WorksheetRemoved;
                old.WorksheetMoved -= WorkBook_WorksheetMoved;

                old.WorksheetNameChanged -= Workbook_WorksheetNameChanged;
                old.WorksheetNameBackColorChanged -= Workbook_WorksheetNameBackColorChanged;
                old.WorksheetNameTextColorChanged -= Workbook_WorksheetNameTextColorChanged;

                old.ExceptionHappened -= Workbook_ExceptionHappened;

                if (old is Workbook w)
                {
                    w.SettingsChanged -= Workbook_SettingsChanged;
                    w.WorkbookSaving -= Workbook_WorkbookSaving;
                    w.WorkbookLoading -= Workbook_WorkbookLoading;
                }
            }

            if (workbook != null)
            {
                workbook.WorkbookLoaded += Workbook_WorkbookLoaded;
                workbook.WorkbookSaved += Workbook_WorkbookSaved;

                workbook.WorksheetCreated += Workbook_WorksheetCreated; ;
                workbook.WorksheetInserted += Workbook_WorksheetInserted;
                workbook.WorksheetRemoved += WorkBook_WorksheetRemoved;
                workbook.WorksheetMoved += WorkBook_WorksheetMoved;

                workbook.WorksheetNameChanged += Workbook_WorksheetNameChanged;
                workbook.WorksheetNameBackColorChanged += Workbook_WorksheetNameBackColorChanged;
                workbook.WorksheetNameTextColorChanged += Workbook_WorksheetNameTextColorChanged;

                workbook.ExceptionHappened += Workbook_ExceptionHappened;

                if(workbook is Workbook w)
                {
                    this.workbook = w;

                    w.SettingsChanged += Workbook_SettingsChanged;
                    w.WorkbookSaving += Workbook_WorkbookSaving;
                    w.WorkbookLoading += Workbook_WorkbookLoading;
                }
            }
        }

        private void Workbook_WorkbookSaving(object? sender, EventArgs e)
        {
            this.adapter.ChangeCursor(CursorStyle.Busy);
        }
        private void Workbook_WorkbookLoading(object? sender, EventArgs e)
        {
            this.adapter.ChangeCursor(CursorStyle.Busy);
        }
        private void Workbook_SettingsChanged(object? sender, EventArgs e)
        {
            var workbook = (Workbook)sender!;
            if (workbook.HasSettings(WorkbookSettings.View_ShowSheetTabControl))
            {
                ShowSheetTabControl();
            }
            else
            {
                HideSheetTabControl();
            }

            if (workbook.HasSettings(WorkbookSettings.View_ShowHorScroll))
            {
                ShowHorScrollBar();
            }
            else
            {
                HideHorScrollBar();
            }

            if (workbook.HasSettings(WorkbookSettings.View_ShowVerScroll))
            {
                ShowVerScrollBar();
            }
            else
            {
                HideVerScrollBar();
            }

            this.SettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Workbook_WorkbookSaved(object? sender, EventArgs e)
        {
            this.WorkbookSaved?.Invoke(sender, e);
        }

        private void Workbook_WorkbookLoaded(object? sender, EventArgs e)
        {
            if (this.workbook.worksheets.Count <= 0)
            {
                this.currentWorksheet = null;
            }
            else
            {
                if (this.currentWorksheet != this.workbook.worksheets[0])
                {
                    this.currentWorksheet = this.workbook.worksheets[0];
                }
                else
                {
                    this.currentWorksheet.UpdateViewportControllBounds();
                }
            }

            this.WorkbookLoaded?.Invoke(sender, e);
        }

        private void Workbook_ExceptionHappened(object? sender, Events.ExceptionHappenEventArgs e)
        {
            this.ExceptionHappened?.Invoke(this, e);
        }

        private void Workbook_WorksheetCreated(object? sender, Events.WorksheetCreatedEventArgs e)
        {
            this.WorksheetCreated?.Invoke(this, e);
        }

        private void Workbook_WorksheetNameTextColorChanged(object? sender, Events.WorksheetEventArgs e)
        {
            var workbook = this.Workbook as Workbook;
            if (workbook != null)
            {
                var index = workbook.GetWorksheetIndex(e.Worksheet);
                var worksheet = e.Worksheet;
                if (this.sheetTab != null)
                {
                    this.sheetTab.UpdateTab(index, worksheet.Name, worksheet.NameBackColor, worksheet.NameTextColor);
                }
            }
        }

        private void Workbook_WorksheetNameBackColorChanged(object? sender, Events.WorksheetEventArgs e)
        {
            var workbook = this.Workbook as Workbook;
            if (workbook != null)
            {
                var index = workbook.GetWorksheetIndex(e.Worksheet);
                var worksheet = e.Worksheet;
                if (this.sheetTab != null)
                {
                    this.sheetTab.UpdateTab(index, worksheet.Name, worksheet.NameBackColor, worksheet.NameTextColor);
                }
            }
        }

        private void Workbook_WorksheetNameChanged(object? sender, Events.WorksheetNameChangingEventArgs e)
        {
            var workbook = this.Workbook as Workbook;
            if (workbook != null)
            {
                var index = workbook.GetWorksheetIndex(e.Worksheet);
                var worksheet = e.Worksheet;
                if (this.sheetTab != null)
                {
                    this.sheetTab.UpdateTab(index, e.NewName, worksheet.NameBackColor, worksheet.NameTextColor);
                }

                this.WorksheetNameChanged?.Invoke(this, e);
            }
        }

        private void WorkBook_WorksheetMoved(object? sender, Events.WorksheetMovedEventArgs e)
        {
            var workbook = this.Workbook;
            if (workbook != null)
            {
                var sheet = workbook.Worksheets[e.NewIndex];

                this.sheetTab.RemoveTab(e.Index);
                // sheet management
                this.sheetTab.InsertTab(e.NewIndex, sheet.Name);
            }
        }

        private void WorkBook_WorksheetRemoved(object? sender, Events.WorksheetRemovedEventArgs e)
        {
            var workbook = this.Workbook;
            if (this.sheetTab != null)
            {
                this.sheetTab.RemoveTab(e.Index);
            }

            this.ClearActionHistoryForWorksheet(e.Worksheet);

            if (workbook?.Worksheets.Count > 0)
            {
                int index = this.sheetTab.SelectedIndex;

                if (index >= workbook.Worksheets.Count)
                {
                    index = workbook.Worksheets.Count - 1;
                }

                this.sheetTab.SelectedIndex = index;
                this.currentWorksheet = this.workbook.worksheets[this.sheetTab.SelectedIndex];
            }
            else
            {
                this.sheetTab.SelectedIndex = -1;
                this.currentWorksheet = null;
            }

            this.adapter.Invalidate();

            this.WorksheetRemoved?.Invoke(this, e);
        }

        private void Workbook_WorksheetInserted(object? sender, Events.WorksheetInsertedEventArgs e)
        {
            var workbook = this.Workbook;
            if(workbook != null)
            {
                var index = e.Index;
                var sheet = workbook.Worksheets[index];

                // sheet management
                this.sheetTab.InsertTab(index, sheet.Name);
                this.sheetTab.SelectedIndex = index;

                // update current worksheet
                if (this.adapter != null && this.adapter.ControlInstance.CurrentWorksheet == null)
                {
                    this.adapter.ControlInstance.CurrentWorksheet = sheet;
                }

                this.WorksheetInserted?.Invoke(this, e);
            }
        }

        #endregion

        #region Adapter
        internal class ReoGridAvaloniaControlAdapter : IControlAdapter
        {
            #region Constructor
            private readonly CalcitaControl canvas;
            internal InputTextBox editTextbox;

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

            public IRenderer Renderer { get { return this.canvas.SheetCanvas.renderer; } }

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
                return this.canvas.SheetCanvas.Bounds.WithX(0).WithY(0);

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
                this.canvas.SheetCanvas.InvalidateVisual();
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
                this.editTextbox.Text = text;
            }

            public string GetEditControlText()
            {
                return this.editTextbox.Text;
            }

            public void EditControlSelectAll()
            {
                this.editTextbox.SelectAll();
            }

            public void SetEditControlCaretPos(int pos)
            {
                this.editTextbox.SelectionStart = pos;
            }

            public int GetEditControlCaretPos()
            {
                return this.editTextbox.SelectionStart;
            }

            public int GetEditControlCaretLine()
            {
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
                this.editTextbox.Copy();
            }

            public void EditControlPaste()
            {
                this.editTextbox.Paste();
            }

            public void EditControlCut()
            {
                this.editTextbox.Cut();
            }

            public void EditControlUndo()
            {
                this.editTextbox.Undo();
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
                get { return this.canvas.ScrollBarMaximum.X; }
                set { Dispatcher.UIThread.InvokeAsync(() => this.canvas.ScrollBarMaximum = this.canvas.ScrollBarMaximum.WithX(value)); }
            }

            public double ScrollBarHorizontalMinimum
            {
                get { return this.canvas.ScrollBarMinimum.X; }
                set { Dispatcher.UIThread.InvokeAsync(() => this.canvas.ScrollBarMinimum = this.canvas.ScrollBarMinimum.WithX(value)); }
            }

            public double ScrollBarHorizontalValue
            {
                get { return this.canvas.Offset.X; }
                set { this.canvas.Offset = this.canvas.Offset.WithX(value); }
            }

            public double ScrollBarHorizontalLargeChange
            {
                get => this.canvas.LargeChange.Width;
                set => this.canvas.LargeChange = this.canvas.LargeChange.WithWidth(value);
            }

            public double ScrollBarVerticalMaximum
            {
                get { return this.canvas.ScrollBarMaximum.Y; }
                set { Dispatcher.UIThread.InvokeAsync(() => this.canvas.ScrollBarMaximum = this.canvas.ScrollBarMaximum.WithY(value)); }
            }

            public double ScrollBarVerticalMinimum
            {
                get { return this.canvas.ScrollBarMaximum.Y; }
                set { Dispatcher.UIThread.InvokeAsync(() => this.canvas.ScrollBarMinimum = this.canvas.ScrollBarMinimum.WithY(value)); }
            }

            public double ScrollBarVerticalValue
            {
                get { return this.canvas.Offset.Y; }
                set { this.canvas.Offset = this.canvas.Offset.WithY(value); }
            }

            public double ScrollBarVerticalLargeChange
            {
                get => this.canvas.LargeChange.Height;
                set => this.canvas.LargeChange = this.canvas.LargeChange.WithHeight(value);
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
        internal class InputTextBox : TextBox
        {
            internal SheetCanvas Owner { get; set; }
            internal bool TextWrap { get; set; }
            internal Avalonia.Size CellSize { get; set; }
            internal ReoGridVerAlign VAlign { get; set; }

            static InputTextBox()
            {
                TextProperty.Changed
                    .Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs>(OnTextChanged));
                IsKeyboardFocusWithinProperty.Changed
                    .Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(OnLostKeyboardFocus));
            }
            internal InputTextBox() : base()
            {
                AddHandler(KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
                AddHandler(TextInputEvent, OnPreviewTextInput, RoutingStrategies.Tunnel);

                this.AcceptsReturn = true;
                this.Template = new FuncControlTemplate<InputTextBox>((t, ns) =>
                {
                    var t1 = new TextPresenter()
                    {
                        Name = "PART_TextPresenter",
                        Margin = new Thickness(2),
                        [!TextPresenter.BackgroundProperty] = t[!TextBox.BackgroundProperty],
                        [!TextPresenter.WidthProperty] = t[!TextBox.WidthProperty],
                        [!TextPresenter.TextProperty] = t[!TextBox.TextProperty],
                        [!TextPresenter.CaretIndexProperty] = t[!TextBox.CaretIndexProperty],
                        [!TextPresenter.SelectionStartProperty] = t[!TextBox.SelectionStartProperty],
                        [!TextPresenter.SelectionEndProperty] = t[!TextBox.SelectionEndProperty],
                        [!TextPresenter.TextAlignmentProperty] = t[!TextBox.TextAlignmentProperty],
                        [!TextPresenter.TextWrappingProperty] = t[!TextBox.TextWrappingProperty],
                        [!TextPresenter.LineHeightProperty] = t[!TextBox.LineHeightProperty],
                        [!TextPresenter.LetterSpacingProperty] = t[!TextBox.LetterSpacingProperty],
                        [!TextPresenter.PasswordCharProperty] = t[!TextBox.PasswordCharProperty],
                        [!TextPresenter.RevealPasswordProperty] = t[!TextBox.RevealPasswordProperty],
                        [!TextPresenter.SelectionBrushProperty] = t[!TextBox.SelectionBrushProperty],
                        [!TextPresenter.SelectionForegroundBrushProperty] = t[!TextBox.SelectionForegroundBrushProperty],
                        [!TextPresenter.CaretBrushProperty] = t[!TextBox.CaretBrushProperty],
                        [!TextPresenter.HorizontalAlignmentProperty] = t[!TextBox.HorizontalAlignmentProperty],
                        [!TextPresenter.VerticalAlignmentProperty] = t[!TextBox.HorizontalContentAlignmentProperty],
                    };
                    var s = new ScrollViewer()
                    {
                        Name = "PART_ScrollViewer",
                        [!ScrollViewer.HorizontalScrollBarVisibilityProperty] = t[!ScrollViewer.HorizontalScrollBarVisibilityProperty],
                        [!ScrollViewer.VerticalScrollBarVisibilityProperty] = t[!ScrollViewer.VerticalScrollBarVisibilityProperty],
                        [!ScrollViewer.IsScrollChainingEnabledProperty] = t[!ScrollViewer.IsScrollChainingEnabledProperty],
                        [!ScrollViewer.AllowAutoHideProperty] = t[!ScrollViewer.AllowAutoHideProperty],
                        [!ScrollViewer.BringIntoViewOnFocusChangeProperty] = t[!ScrollViewer.BringIntoViewOnFocusChangeProperty],
                        Content = t1
                    };
                    ns.Register(s.Name, s);
                    ns.Register(t1.Name, t1);
                    return s;
                });

            }

            protected override void OnLostFocus(RoutedEventArgs e)
            {
                var sheet = this.Owner.Worksheet;

                if (sheet.currentEditingCell != null && IsVisible)
                {
                    sheet.EndEdit(Text);
                    IsVisible = false;
                }
                base.OnLostFocus(e);
            }

            private void OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
                var sheet = this.Owner.Worksheet;

                // in single line text
                if (!TextWrap && Text.IndexOf('\n') == -1)
                {
                    Action moveAction = null;

                    if (e.Key == Key.Up)
                    {
                        moveAction = () => sheet.MoveSelectionUp();
                    }
                    else if (e.Key == Key.Down)
                    {
                        moveAction = () => sheet.MoveSelectionDown();
                    }
                    else if (e.Key == Key.Left && SelectionStart == 0)
                    {
                        moveAction = () => sheet.MoveSelectionLeft();
                    }
                    else if (e.Key == Key.Right && SelectionStart == Text.Length)
                    {
                        moveAction = () => sheet.MoveSelectionRight();
                    }
                    if (moveAction != null)
                    {
                        sheet.EndEdit(Text);
                        moveAction();
                        e.Handled = true;
                    }
                }
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                var sheet = this.Owner.Worksheet;

                if (sheet.currentEditingCell != null && IsVisible)
                {
                    if (e.KeyModifiers == KeyModifiers.Control
                        && e.Key == Key.Enter)
                    {
                        var str = this.Text;
                        var selstart = this.SelectionStart;
                        str = str.Insert(this.SelectionStart, Environment.NewLine);
                        this.Text = str;
                        this.SelectionStart = selstart + Environment.NewLine.Length;
                    }
                    else if (e.KeyModifiers == KeyModifiers.None && e.Key == Key.Enter)
                    {
                        sheet.EndEdit(this.Text);
                        sheet.MoveSelectionForward();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Enter)
                    {
                        // TODO: auto adjust row height
                    }
                    // shift + tab
                    else if (e.KeyModifiers == KeyModifiers.Meta && e.Key == Key.Tab)
                    {
                        sheet.EndEdit(this.Text);
                        sheet.MoveSelectionBackward();
                        e.Handled = true;
                    }
                    // tab
                    else if (e.Key == Key.Tab)
                    {
                        sheet.EndEdit(this.Text);
                        sheet.MoveSelectionForward();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Escape)
                    {
                        sheet.EndEdit(EndEditReason.Cancel);
                        e.Handled = true;
                    }
                }

                base.OnKeyDown(e);
            }


            private static void OnLostKeyboardFocus(AvaloniaPropertyChangedEventArgs<bool> args)
            {
                var @this = args.Sender as InputTextBox;
                if (@this is not null && args.NewValue == false)
                {
                    @this.Owner.Worksheet?.EndEdit(@this.Text, EndEditReason.NormalFinish);
                }
            }
            private static void OnTextChanged(AvaloniaPropertyChangedEventArgs e)
            {
                var @this = e.Sender as InputTextBox;
                if (@this != null)
                {
                    @this.Text = @this.Owner.Worksheet?.RaiseCellEditTextChanging(@this.Text);
                }
            }

            private void OnPreviewTextInput(object sender, TextInputEventArgs e)
            {
                if (e.Text.Length > 0)
                {
                    int inputChar = e.Text[0];
                    if (inputChar != this.Owner.Worksheet?.RaiseCellEditCharInputed(inputChar))
                    {

                        e.Handled = true;
                    }

                }
            }
        }

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

        /// <summary>
        /// Offset StyledProperty definition
        /// </summary>
        internal static readonly StyledProperty<Vector> OffsetProperty =
            AvaloniaProperty.Register<CalcitaControl, Vector>(nameof(Offset));

        /// <summary>
        /// Gets or sets the Offset property. This StyledProperty
        /// indicates ....
        /// </summary>
        internal Vector Offset
        {
            get => this.GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        /// <summary>
        /// LargeChange DirectProperty definition
        /// </summary>
        internal static readonly DirectProperty<CalcitaControl, Size> LargeChangeProperty =
            AvaloniaProperty.RegisterDirect<CalcitaControl, Size>(nameof(LargeChange),
                o => o.LargeChange,
                (o, v) => o.LargeChange = v);

        private Size _LargeChange = default;
        /// <summary>
        /// Gets or sets the LargeChange property. This DirectProperty 
        /// indicates ....
        /// </summary>
        internal Size LargeChange
        {
            get => _LargeChange;
            set => SetAndRaise(LargeChangeProperty, ref _LargeChange, value);
        }

        /// <summary>
        /// ScrollBarMaximum DirectProperty definition
        /// </summary>
        internal static readonly DirectProperty<CalcitaControl, Vector> ScrollBarMaximumProperty =
            AvaloniaProperty.RegisterDirect<CalcitaControl, Vector>(nameof(ScrollBarMaximum),
                o => o.ScrollBarMaximum,
                (o, v) => o.ScrollBarMaximum = v);

        private Vector _ScrollBarMaximum = default;
        /// <summary>
        /// Gets or sets the ScrollBarMaximum property. This DirectProperty 
        /// indicates ....
        /// </summary>
        public Vector ScrollBarMaximum
        {
            get => _ScrollBarMaximum;
            set => SetAndRaise(ScrollBarMaximumProperty, ref _ScrollBarMaximum, value);
        }

        /// <summary>
        /// ScrollBarMinimum DirectProperty definition
        /// </summary>
        public static readonly DirectProperty<CalcitaControl, Vector> ScrollBarMinimumProperty =
            AvaloniaProperty.RegisterDirect<CalcitaControl, Vector>(nameof(ScrollBarMinimum),
                o => o.ScrollBarMinimum,
                (o, v) => o.ScrollBarMinimum = v);

        private Vector _ScrollBarMinimum = default;


        /// <summary>
        /// Gets or sets the ScrollBarMinimum property. This DirectProperty 
        /// indicates ....
        /// </summary>
        internal Vector ScrollBarMinimum
        {
            get => _ScrollBarMinimum;
            set => SetAndRaise(ScrollBarMinimumProperty, ref _ScrollBarMinimum, value);
        }

        #endregion

        /// <summary>
        /// Get or set filepath of startup template file
        /// </summary>
        public string LoadFromFile { get; set; }

        public void Dispose() { }
    }
}
