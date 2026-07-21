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
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Calcita.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calcita.Controls
{
    [TemplatePart("PART_SortAZItem", typeof(RadioButton))]
    [TemplatePart("PART_SortZAItem", typeof(RadioButton))]
    [TemplatePart("PART_OkButton", typeof(Button))]
    [TemplatePart("PART_CancelButton", typeof(Button))]
    [TemplatePart("PART_SelectAll", typeof(CheckBox))]
    [TemplatePart("PART_ScrollViewer", typeof(ScrollViewer))]
    public class FilterBox : SelectingItemsControl
    {
        protected override Type StyleKeyOverride => typeof(FilterBox);

        static FilterBox()
        {
            SelectionChangedEvent.AddClassHandler<FilterBox>((x, e) => x.OnSelectionChanged());
        }
        
        private void OnSelectionChanged()
        {
            if(SelectAllButton is null || this.SelectedItems is null)
            {
                return;
            }
            if (this.SelectedItems.Count == 0)
            {
                SelectAllButton.IsChecked = false;
            }
            else if(this.SelectedItems.Count == (this.ItemsSource as List<String>)?.Count)
            {
                SelectAllButton.IsChecked = true;
            }
            else
            {
                SelectAllButton.IsChecked = null;
            }
        }

        // todo add styled properties

        ToggleButton? SortAZItem;
        ToggleButton? SortZAItem;
        Button? OkButton;
        Button? CancelButton;
        CheckBox? SelectAllButton;
  
        Calcita.Data.AutoColumnFilter.AutoColumnFilterBody HeaderBody { get; set; } = null!;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            this.SortAZItem?.Click -= SortAZItem_Click;
            this.SortZAItem?.Click -= SortZAItem_Click;
            this.OkButton?.Click -= OkButton_Click;
            this.CancelButton?.Click -= CancelButton_Click;
            this.SelectAllButton?.Click -= SelectAll_CheckedChanged;

            base.OnApplyTemplate(e);

            this.SortAZItem = e.NameScope.Find<ToggleButton>("PART_SortAZItem");
            this.SortZAItem = e.NameScope.Find<ToggleButton>("PART_SortZAItem");
            this.OkButton = e.NameScope.Find<Button>("PART_OkButton");
            this.CancelButton = e.NameScope.Find<Button>("PART_CancelButton");
            this.SelectAllButton = e.NameScope.Find<CheckBox>("PART_SelectAll");

            this.SortAZItem?.Click += SortAZItem_Click;
            this.SortZAItem?.Click += SortZAItem_Click;
            this.OkButton?.Click += OkButton_Click;
            this.CancelButton?.Click += CancelButton_Click;
            this.SelectAllButton?.IsCheckedChanged += SelectAll_CheckedChanged;
        }

        private void SelectAll_CheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(this.SelectAllButton?.IsChecked == true)
            {
                SelectAll();
            }else if(this.SelectAllButton?.IsChecked == false)
            {
                this.SelectedItems?.Clear();
            }
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.HeaderBody.ContextFlyout!.Hide();
        }

        private void OkButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var items = this.SelectedItems.OfType<string>().ToList();

            HeaderBody.IsSelectAll = false;
            HeaderBody.selectedTextItems.Clear();
            HeaderBody.SelectedTextItems.AddRange(this.SelectedItems.OfType<string>());
            HeaderBody.autoFilter.Apply();
            this.HeaderBody.ContextFlyout!.Hide();
        }

        private void SortZAItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var worksheet = HeaderBody.ColumnHeader.Worksheet;
            try
            {
                worksheet.SortColumn(HeaderBody.ColumnHeader.Index, HeaderBody.autoFilter.ApplyRange, SortOrder.Descending);
            }
            catch (Exception ex)
            {
                worksheet.NotifyExceptionHappen(ex);
            }
        }

        private void SortAZItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var headerBody = this.HeaderBody;
            var worksheet = headerBody.ColumnHeader.Worksheet;
            try
            {

                worksheet.SortColumn(headerBody.ColumnHeader.Index, headerBody.autoFilter.ApplyRange, SortOrder.Ascending);
            }
            catch (Exception ex)
            {
                worksheet.NotifyExceptionHappen(ex);
            }
        }

        void SelectAll()
        {
            this.Selection.SingleSelect = false;
            this.Selection.SelectAll();
        }

        void SetSelectedItems(List<string> selectedTextItems)
        {
            var set = selectedTextItems.ToHashSet();
            foreach (var item in selectedTextItems)
            {
                this.SelectedItems.Add(item);
            }
        }

        void SetItems(List<string> items)
        {
            this.ItemsSource = items;
        }

        private void Cb_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (Items.OfType<CheckBox>().All(p => p.IsChecked == true))
            {
                this.SelectAllButton?.IsChecked = true;
            }
            else
            {
                this.SelectAllButton?.IsChecked = false;
            }
        }

        internal static void ShowFilterPanel(Calcita.Data.AutoColumnFilter.AutoColumnFilterBody headerBody, Graphics.Point point)
        {
            if (headerBody.ColumnHeader == null || headerBody.ColumnHeader.Worksheet == null) return;

            var worksheet = headerBody.ColumnHeader.Worksheet;
            if (worksheet == null) return;

            RGRect headerRect = Calcita.Views.ColumnHeaderView.GetColHeaderBounds(worksheet, headerBody.ColumnHeader.Index, point);
            if (headerRect.Width == 0 || headerRect.Height == 0) return;

            RGRect buttonRect = headerBody.GetColumnFilterButtonRect(headerRect.Size);

            if (headerBody.ContextFlyout == null)
            {
                var filterPanel = new FilterBox()
                {
                    HeaderBody = headerBody,
                };

                headerBody.ContextFlyout = new Flyout()
                {
                    Content = filterPanel,
                };
            }

            if (headerBody.ContextFlyout != null)
            {
                if (headerBody.ContextFlyout is Flyout { Content: FilterBox filterPanel } flyout)
                {
                    if (headerBody.DataDirty)
                    {
                        // todo: keep select status for every items before clear

                        try
                        {
                            headerBody.ColumnHeader.Worksheet.ControlAdapter.ChangeCursor(CursorStyle.Busy);

                            var items = headerBody.GetDistinctItems();
                            filterPanel.ItemsSource = items;
                            filterPanel.SetItems(items);

                            if (headerBody.IsSelectAll == true)
                            {
                                filterPanel.SelectAll();
                            }
                            else
                            {
                                filterPanel.SetSelectedItems(headerBody.selectedTextItems);
                            }
                        }
                        finally
                        {
                            headerBody.ColumnHeader.Worksheet.ControlAdapter.ChangeCursor(CursorStyle.PlatformDefault);
                        }

                        headerBody.DataDirty = false;
                        headerBody.IsSelectAll = true;
                    }
                    flyout.SetValue(Flyout.ShowModeProperty, FlyoutShowMode.Standard);
                    flyout.SetValue(Flyout.PlacementProperty, PlacementMode.Pointer);
                    flyout.ShowAt(worksheet.ControlAdapter.ControlInstance as Control);
                }
            }
        }

        protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
        {
            return new FilterBoxItem();
        }

        protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
        {
            return NeedsContainer<FilterBoxItem>(item, out recycleKey);
        }
    }

    /// <summary>
    /// A selectable item in a <see cref="ListBox"/>.
    /// </summary>
    [PseudoClasses(":pressed", ":selected")]
    public class FilterBoxItem : ContentControl, ISelectable
    {
        /// <summary>
        /// Defines the <see cref="IsSelected"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsSelectedProperty =
            SelectingItemsControl.IsSelectedProperty.AddOwner<FilterBoxItem>();

        /// <summary>
        /// Initializes static members of the <see cref="FilterBoxItem"/> class.
        /// </summary>
        static FilterBoxItem()
        {
            SelectableMixin.Attach<FilterBoxItem>(IsSelectedProperty);
            PressedMixin.Attach<FilterBoxItem>();
            FocusableProperty.OverrideDefaultValue<FilterBoxItem>(true);
            AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<FilterBoxItem>(IsOffscreenBehavior.FromClip);
            PlatformFeedback.FeedbackTypeProperty.OverrideDefaultValue<FilterBoxItem>(FeedbackType.Auto);
        }

        /// <summary>
        /// Gets or sets the selection state of the item.
        /// </summary>
        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ListItemAutomationPeer(this);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            UpdateSelectionFromEvent(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            UpdateSelectionFromEvent(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            UpdateSelectionFromEvent(e);
        }

        protected bool UpdateSelectionFromEvent(RoutedEventArgs e) => SelectingItemsControl.ItemsControlFromItemContainer(this)?.UpdateSelectionFromEvent(this, e) ?? false;
    }
}
