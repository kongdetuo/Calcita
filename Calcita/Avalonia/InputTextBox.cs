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
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;
using System;

namespace Calcita.Controls;

internal class InputTextBox : TextBox
{
    internal SheetCanvas Owner { get; set; }
    internal bool TextWrap { get; set; }
    internal Avalonia.Size CellSize { get; set; }
    internal ReoGridVerAlign VAlign { get; set; }

    protected override Type StyleKeyOverride => typeof(InputTextBox);

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
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        var sheet = this.Owner.Worksheet;

        if (sheet?.currentEditingCell != null && IsVisible)
        {
            sheet.EndEdit(Text);
            IsVisible = false;
        }
        base.OnLostFocus(e);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        var sheet = this.Owner.Worksheet;
        if (sheet == null) return;

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
        if (sheet == null) return;

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

