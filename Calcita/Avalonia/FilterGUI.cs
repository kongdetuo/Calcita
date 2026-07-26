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
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Calcita.Interaction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Calcita.Controls
{
    // todo clear filter 
    [TemplatePart("PART_SortAscending", typeof(RadioButton))]
    [TemplatePart("PART_SortDescending", typeof(RadioButton))]
    [TemplatePart("PART_OkButton", typeof(Button))]
    [TemplatePart("PART_CancelButton", typeof(Button))]
    public class FilterControl : TemplatedControl
    {
        private bool selecting;
        protected override Type StyleKeyOverride => typeof(FilterControl);

        static FilterControl()
        {
            TextSelectedAllProperty.Changed.AddClassHandler<FilterControl>((x, e) => x.OnTextSelectedAllChanged());
        }

        private void OnTextSelectedAllChanged()
        {
            if (TextSelectedAll == null)
                return;

            selecting = true;

            foreach (var item in TextFilterItems)
                item.IsSelected = TextSelectedAll.Value;

            selecting = false;
        }

        RadioButton? SortAZItem;
        RadioButton? SortZAItem;
        Button? OkButton;
        Button? CancelButton;

        Button? ClearButton;

        /// <summary>
        /// TextSelectedAllStyledProperty definition
        /// </summary>
        public static readonly StyledProperty<bool?> TextSelectedAllProperty =
            AvaloniaProperty.Register<FilterControl, bool?>(nameof(TextSelectedAll));

        /// <summary>
        /// Gets or sets the TextSelectedAll property. This StyledProperty
        /// indicates ....
        /// </summary>
        public bool? TextSelectedAll
        {
            get => this.GetValue(TextSelectedAllProperty);
            set => SetValue(TextSelectedAllProperty, value);
        }

        public IEnumerable<TextFilterItem> TextFilterItems { get; private set; } = [];

        Calcita.Data.AutoColumnFilter.AutoColumnFilterBody HeaderBody { get; set; } = null!;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            this.SortAZItem?.Click -= SortAZItem_Click;
            this.SortZAItem?.Click -= SortZAItem_Click;
            this.OkButton?.Click -= OkButton_Click;
            this.CancelButton?.Click -= CancelButton_Click;
            this.ClearButton?.Click -= ClearButton_Click;

            base.OnApplyTemplate(e);

            this.SortAZItem = e.NameScope.Find<RadioButton>("PART_SortAscending");
            this.SortZAItem = e.NameScope.Find<RadioButton>("PART_SortDescending");
            this.OkButton = e.NameScope.Find<Button>("PART_OkButton");
            this.CancelButton = e.NameScope.Find<Button>("PART_CancelButton");
            this.ClearButton = e.NameScope.Find<Button>("PART_ClearButton");

            this.SortAZItem?.Click += SortAZItem_Click;
            this.SortZAItem?.Click += SortZAItem_Click;
            this.OkButton?.Click += OkButton_Click;
            this.CancelButton?.Click += CancelButton_Click;
            this.ClearButton?.Click += ClearButton_Click;
        }

        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.HeaderBody.ContextFlyout!.Hide();
        }

        private void OkButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var selectedItems = this.TextFilterItems
                .Where(item=>item.IsSelected)
                .Select(item=>item.Text);

            HeaderBody.IsSelectAll = false;
            HeaderBody.selectedTextItems.Clear();
            HeaderBody.SelectedTextItems.AddRange(selectedItems);
            HeaderBody.autoFilter.Apply();

            this.HeaderBody.ContextFlyout!.Hide();
        }

        private void ClearButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            HeaderBody.IsSelectAll = true;
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
                var filterPanel = new FilterControl()
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
                if (headerBody.ContextFlyout is Flyout { Content: FilterControl filterPanel } flyout)
                {
                    if (headerBody.DataDirty)
                    {
                        // todo: keep select status for every items before clear

                        try
                        {
                            headerBody.ColumnHeader.Worksheet.ControlAdapter?.ChangeCursor(CursorStyle.Busy);
                            filterPanel.UpdateData();
                        }
                        finally
                        {
                            headerBody.ColumnHeader.Worksheet.ControlAdapter?.ChangeCursor(CursorStyle.PlatformDefault);
                        }

                        headerBody.DataDirty = false;
                        headerBody.IsSelectAll = true;
                    }
                    flyout.SetValue(Flyout.ShowModeProperty, FlyoutShowMode.Standard);
                    flyout.SetValue(Flyout.PlacementProperty, PlacementMode.Pointer);
                    flyout.ShowAt((Control)worksheet.ControlAdapter!.ControlInstance);
                }
            }
        }

        private void UpdateData()
        {
            foreach (var item in TextFilterItems)
                item.PropertyChanged -= Item_PropertyChanged;

            var sourceItems = HeaderBody.GetDistinctItems();
            bool isSelectedAll = HeaderBody.IsSelectAll == true;
            var set = isSelectedAll ? HeaderBody.SelectedTextItems.ToHashSet() : [];
            var items = sourceItems.Select(text => new TextFilterItem(text, isSelectedAll || set.Contains(text))).ToList();

            TextFilterItems = items;

            foreach (var item in TextFilterItems)
                item.PropertyChanged += Item_PropertyChanged;

            Item_PropertyChanged(null, null!);
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (selecting)
                return;

            if (TextFilterItems.All(p => p.IsSelected))
                this.SetCurrentValue(TextSelectedAllProperty, true);
            else if (TextFilterItems.All(p => !p.IsSelected))
                this.SetCurrentValue(TextSelectedAllProperty, false);
            else
                this.SetCurrentValue(TextSelectedAllProperty, null);
        }
    }



    public abstract class FilterItem : INotifyPropertyChanged
    {
        public bool IsSelected
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class TextFilterItem : FilterItem
    {
        public TextFilterItem(string text, bool selected)
        {
            this.IsSelected = selected;
            this.Text = text;
        }

        public string Text { get; private set; }
    }
}
