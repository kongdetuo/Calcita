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
using Avalonia.Controls.Templates;
using Avalonia.Input;
using System;
using Calcita.Events;
using Calcita.Main;
using Avalonia.Controls.Metadata;

namespace Calcita.Controls
{
    [TemplatePart(Name = "PART_ScrollLeftButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_ScrollRightButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "PART_AddButton", Type = typeof(Button))]
    public class SheetTabControl : TabStrip, ISheetTabControl
    {
        RepeatButton? scrollLeftButton;
        RepeatButton? scrollRightButton;
        Button? addButton;
        ScrollViewer? scrollViewer;

        private readonly AvaloniaList<string> sheets = [];

        static SheetTabControl()
        {
            SelectingItemsControl.SelectionModeProperty.OverrideDefaultValue<SheetTabControl>(SelectionMode.AlwaysSelected);
            InputElement.FocusableProperty.OverrideDefaultValue(typeof(SheetTabControl), defaultValue: false);
            ItemsControl.ItemsPanelProperty.OverrideDefaultValue<SheetTabControl>(new FuncTemplate<Panel?>(() =>
                new StackPanel()
            ));

            WorkbookProperty.Changed.AddClassHandler<SheetTabControl>((s, e) => s.OnWorkbookChanged(e));
            CurrentWorksheetProperty.Changed.AddClassHandler<SheetTabControl>((s, e) => s.OnCurrentWorksheetChanged(e));
            SelectedIndexProperty.Changed.AddClassHandler<SheetTabControl>((s, e) => s.OnSelectionChanged(e));
        }

        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return new SheetTabItem()
            {
                
            };
        }

        protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            return NeedsContainer<SheetTabItem>(item, out recycleKey);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            scrollLeftButton?.Click -= ScrollLeftButton_Click;
            scrollRightButton?.Click -= ScrollRightButton_Click;
            addButton?.Click -= AddButton_Click;

            scrollLeftButton = e.NameScope.Find<RepeatButton>("PART_ScrollLeftButton");
            scrollRightButton = e.NameScope.Find<RepeatButton>("PART_ScrollRightButton");
            addButton = e.NameScope.Find<Button>("PART_AddButton");
            scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");

            scrollLeftButton?.Click += ScrollLeftButton_Click;
            scrollRightButton?.Click += ScrollRightButton_Click;
            addButton?.Click += AddButton_Click;

            void ScrollLeftButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            {
                scrollViewer?.Offset = new Vector(scrollViewer.Offset.X - 5, scrollViewer.Offset.Y);
            }
            void ScrollRightButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            {
                scrollViewer?.Offset = new Vector(scrollViewer.Offset.X + 5, scrollViewer.Offset.Y);
            }
            void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
            {
                CreateNewWorksheet();
            }
        }

        #region Dependency Properties

        /// <summary>
        /// NewButtonVisible StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<bool> NewButtonVisibleProperty =
            AvaloniaProperty.Register<SheetTabControl, bool>(nameof(NewButtonVisible), true);

        public bool NewButtonVisible
        {
            get { return GetValue(NewButtonVisibleProperty); }
            set { SetValue(NewButtonVisibleProperty, value); }
        }

        /// <summary>
        /// Workbook StyledProperty definition.
        /// </summary>
        public static readonly StyledProperty<IWorkbook?> WorkbookProperty =
            AvaloniaProperty.Register<SheetTabControl, IWorkbook?>(nameof(Workbook));

        /// <summary>
        /// Get or set the workbook whose worksheets are displayed as tabs.
        /// </summary>
        public IWorkbook? Workbook
        {
            get => GetValue(WorkbookProperty);
            set => SetValue(WorkbookProperty, value);
        }

        /// <summary>
        /// CurrentWorksheet StyledProperty definition.
        /// </summary>
        public static readonly StyledProperty<Worksheet?> CurrentWorksheetProperty =
            AvaloniaProperty.Register<SheetTabControl, Worksheet?>(nameof(CurrentWorksheet));

        /// <summary>
        /// Get or set the currently selected worksheet.
        /// </summary>
        public Worksheet? CurrentWorksheet
        {
            get => GetValue(CurrentWorksheetProperty);
            set => SetValue(CurrentWorksheetProperty, value);
        }

        #endregion // Dependency Properties

        /// <summary>
        /// Determine whether or not allow to move tab by dragging mouse
        /// </summary>
        public bool AllowDragToMove { get; set; }

        /// <summary>
        /// Create and add a new worksheet via the current workbook.
        /// </summary>
        private void CreateNewWorksheet()
        {
            if (this.Workbook is { } workbook)
            {
                workbook.AddWorksheet(workbook.CreateWorksheet());
            }
        }

        private void OnWorkbookChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is IWorkbook old)
            {
                DetachWorkbookEvents(old);
            }

            var workbook = e.NewValue as IWorkbook;

            this.sheets.Clear();

            if (workbook != null)
            {
                AttachWorkbookEvents(workbook);

                foreach (var sheet in workbook.Worksheets)
                {
                    this.sheets.Add(sheet.Name);
                }
            }

            this.ItemsSource = this.sheets;
        }

        private void AttachWorkbookEvents(IWorkbook workbook)
        {
            workbook.WorksheetInserted += Workbook_WorksheetInserted;
            workbook.WorksheetRemoved += Workbook_WorksheetRemoved;
            workbook.WorksheetMoved += Workbook_WorksheetMoved;
            workbook.WorksheetNameChanged += Workbook_WorksheetNameChanged;
            workbook.WorksheetNameBackColorChanged += Workbook_WorksheetNameBackColorChanged;
            workbook.WorksheetNameTextColorChanged += Workbook_WorksheetNameTextColorChanged;
        }

        private void DetachWorkbookEvents(IWorkbook workbook)
        {
            workbook.WorksheetInserted -= Workbook_WorksheetInserted;
            workbook.WorksheetRemoved -= Workbook_WorksheetRemoved;
            workbook.WorksheetMoved -= Workbook_WorksheetMoved;
            workbook.WorksheetNameChanged -= Workbook_WorksheetNameChanged;
            workbook.WorksheetNameBackColorChanged -= Workbook_WorksheetNameBackColorChanged;
            workbook.WorksheetNameTextColorChanged -= Workbook_WorksheetNameTextColorChanged;
        }

        private void Workbook_WorksheetInserted(object? sender, WorksheetInsertedEventArgs e)
        {
            if (e.Index < 0) return;

            this.sheets.Insert(e.Index, e.Worksheet.Name);
            this.CurrentWorksheet = e.Worksheet;
        }

        private void Workbook_WorksheetRemoved(object? sender, WorksheetRemovedEventArgs e)
        {
            var workbook = this.Workbook;

            this.sheets.RemoveAt(e.Index);

            if (workbook == null) return;

            if (workbook.Worksheets.Count <= 0)
            {
                this.CurrentWorksheet = null;
            }
            else if (this.CurrentWorksheet == null
                || workbook.GetWorksheetIndex(this.CurrentWorksheet) < 0)
            {
                var index = Math.Min(e.Index, workbook.Worksheets.Count - 1);
                this.CurrentWorksheet = workbook.Worksheets[index];
            }
        }

        private void Workbook_WorksheetMoved(object? sender, WorksheetMovedEventArgs e)
        {
            if (e.Index < 0 || e.NewIndex < 0) return;

            this.sheets.RemoveAt(e.Index);
            this.sheets.Insert(e.NewIndex, e.Worksheet.Name);
        }

        private void Workbook_WorksheetNameChanged(object? sender, WorksheetNameChangingEventArgs e)
        {
            var workbook = this.Workbook;
            if (workbook == null) return;

            var index = workbook.GetWorksheetIndex(e.Worksheet);
            if (index >= 0 && index < this.sheets.Count) this.sheets[index] = e.NewName;
        }

        private void Workbook_WorksheetNameBackColorChanged(object? sender, WorksheetEventArgs e)
        {
            UpdateTabName(e.Worksheet);
        }

        private void Workbook_WorksheetNameTextColorChanged(object? sender, WorksheetEventArgs e)
        {
            UpdateTabName(e.Worksheet);
        }

        private void UpdateTabName(Worksheet sheet)
        {
            var workbook = this.Workbook;
            if (workbook == null) return;

            var index = workbook.GetWorksheetIndex(sheet);
            if (index >= 0 && index < this.sheets.Count) this.sheets[index] = sheet.Name;
        }

        private void OnCurrentWorksheetChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var workbook = this.Workbook;
            var sheet = e.GetNewValue<Worksheet?>();

            if (workbook != null && sheet != null)
            {
                var index = workbook.GetWorksheetIndex(sheet);
                if (index >= 0 && this.SelectedIndex != index)
                {
                    this.SelectedIndex = index;
                }
            }
            else
            {
                this.SelectedIndex = -1;
            }
        }

        private void OnSelectionChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var workbook = this.Workbook;
            var index = e.GetNewValue<int>();

            if (workbook != null && index >= 0 && index < workbook.Worksheets.Count)
            {
                var sheet = workbook.Worksheets[index];
                if (!ReferenceEquals(this.CurrentWorksheet, sheet))
                {
                    this.CurrentWorksheet = sheet;
                }
            }
        }

        public void MoveItem(int index, int targetIndex)
        {
            // TODO: Not supported yet - move only reorders the visible tab items,
            // it does NOT change the order of the worksheets in the workbook.
            // Drag-to-reorder handling is not implemented yet, so this is a broken
            // placeholder kept as a reminder to wire it up together with
            // Workbook.MoveWorksheet when drag support is added.
            if (index < 0 || index > this.Items.Count - 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var tab = this.Items[index];

            this.Items.RemoveAt(index);

            if (targetIndex > index) targetIndex--;

            this.Items.Insert(targetIndex, tab);
        }
    }

    public class SheetTabItem : ListBoxItem
    {

    }
}