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
#if AVALONIA

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using System;
using Calcita.Main;
using Avalonia.Controls.Metadata;

namespace Calcita.AvaloniaPlatform
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
        static SheetTabControl()
        {
            SelectingItemsControl.SelectionModeProperty.OverrideDefaultValue<SheetTabControl>(SelectionMode.AlwaysSelected);
            InputElement.FocusableProperty.OverrideDefaultValue(typeof(SheetTabControl), defaultValue: false);
            ItemsControl.ItemsPanelProperty.OverrideDefaultValue<SheetTabControl>(new FuncTemplate<Panel?>(() =>
                new StackPanel()
            ));

            SelectedIndexProperty.Changed.AddClassHandler<SheetTabControl>((s, e) =>
            {
                s.SelectedIndexChanged?.Invoke(s, EventArgs.Empty);
            });
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
                this.NewSheetClick?.Invoke(this, EventArgs.Empty);
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

        #endregion // Dependency Properties

        /// <summary>
        /// Determine whether or not allow to move tab by dragging mouse
        /// </summary>
        public bool AllowDragToMove { get; set; }

        #region Tab Management

        #endregion // Tab Management

        public void MoveItem(int index, int targetIndex)
        {
            if (index < 0 || index > this.Items.Count - 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var tab = this.Items[index];

            this.Items.RemoveAt(index);

            if (targetIndex > index) targetIndex--;

            this.Items.Insert(targetIndex, tab);
        }

        public event EventHandler<SheetTabMovedEventArgs>? TabMoved;

        public event EventHandler? SelectedIndexChanged;

        public event EventHandler? SplitterMoving;
        public event EventHandler? SheetListClick;

        public event EventHandler? NewSheetClick;

        public event EventHandler<SheetTabMouseEventArgs>? TabMouseDown;
    }

    public class SheetTabItem : ListBoxItem
    {

    }
}

#endif // WPF



