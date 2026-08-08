/*****************************************************
 * 
 * Calcita - .NET Spreadsheet Control
 * 
 * Formula bar (Excel-style edit bar) for the Avalonia control.
 *
 ****************************************************************************/

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Calcita.Events;
using System;

namespace Calcita.Controls
{
    /// <summary>
    /// Excel-style formula bar: name box (current cell address), the edit
    /// box, and confirm / cancel buttons. Binds to a <see cref="Worksheet"/>
    /// and coordinates the edit session with it directly.
    /// </summary>
    public class FormulaBar : TemplatedControl
    {
        private TextBox? editTextBox;
        private TextBlock? nameBox;
        private Button? confirmButton;
        private Button? cancelButton;

        /// <summary>
        /// Raised when a confirm / cancel operation ended the edit session,
        /// so the host control can return keyboard focus to the grid.
        /// </summary>
        internal event Action? EditSessionEnded;

        /// <summary>
        /// Worksheet dependency property definition.
        /// </summary>
        public static readonly StyledProperty<Worksheet?> WorksheetProperty =
            AvaloniaProperty.Register<FormulaBar, Worksheet?>(nameof(Worksheet));

        /// <summary>
        /// Get or set the worksheet this formula bar displays and coordinates with.
        /// </summary>
        public Worksheet? Worksheet
        {
            get => this.GetValue(WorksheetProperty);
            set => SetValue(WorksheetProperty, value);
        }

        static FormulaBar()
        {
            WorksheetProperty.Changed.AddClassHandler<FormulaBar>(
                (fb, e) => fb.OnWorksheetChanged(e));
        }

        protected override Type StyleKeyOverride => typeof(FormulaBar);

        internal TextBox? EditTextBox => this.editTextBox;

        /// <summary>
        /// Get or set the text displayed in the formula bar edit box.
        /// </summary>
        internal string EditText
        {
            get => this.editTextBox?.Text ?? string.Empty;
            set
            {
                if (this.editTextBox != null && this.editTextBox.Text != value)
                {
                    this.editTextBox.Text = value;
                }
            }
        }

        /// <summary>
        /// Update the name box with the given cell address.
        /// </summary>
        internal void UpdateName(string? name)
        {
            if (this.nameBox != null)
            {
                this.nameBox.Text = name ?? string.Empty;
            }
        }

        /// <summary>
        /// Focus the edit box and select all its content, ready for typing.
        /// </summary>
        internal void FocusEditBox()
        {
            if (this.editTextBox != null)
            {
                this.editTextBox.Focus();
                this.editTextBox.SelectAll();
            }
        }

        /// <summary>
        /// Set the caret position of the edit box.
        /// </summary>
        internal void SetCaretPos(int pos)
        {
            if (this.editTextBox != null && pos >= 0 && pos <= this.editTextBox.Text.Length)
            {
                this.editTextBox.SelectionStart = pos;
            }
        }

        internal void SelectAllEditBox()
        {
            this.editTextBox?.SelectAll();
        }

        internal int GetCaretPos() => this.editTextBox?.SelectionStart ?? 0;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            this.editTextBox?.TextChanged -= EditTextBox_TextChanged;
            this.editTextBox?.KeyDown -= EditTextBox_KeyDown;
            this.editTextBox?.GotFocus -= EditTextBox_GotFocus;
            this.confirmButton?.Click -= ConfirmButton_Click;
            this.cancelButton?.Click -= CancelButton_Click;

            this.editTextBox = e.NameScope.Find<TextBox>("PART_EditTextBox");
            this.nameBox = e.NameScope.Find<TextBlock>("PART_NameBox");
            this.confirmButton = e.NameScope.Find<Button>("PART_ConfirmButton");
            this.cancelButton = e.NameScope.Find<Button>("PART_CancelButton");

            this.editTextBox?.TextChanged += EditTextBox_TextChanged;
            this.editTextBox?.KeyDown += EditTextBox_KeyDown;
            this.editTextBox?.GotFocus += EditTextBox_GotFocus;
            this.confirmButton?.Click += ConfirmButton_Click;
            this.cancelButton?.Click += CancelButton_Click;

            Refresh();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if(change.Property == IsVisibleProperty)
            {
                Refresh();
            }
            base.OnPropertyChanged(change);
        }

        private void ConfirmButton_Click(object? sender, RoutedEventArgs e) => Confirm();

        private void CancelButton_Click(object? sender, RoutedEventArgs e) => Cancel();

        private void OnWorksheetChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue is Worksheet oldSheet) DetachWorksheetEvents(oldSheet);
            if (e.NewValue is Worksheet newSheet) AttachWorksheetEvents(newSheet);

            Refresh();
        }

        private void AttachWorksheetEvents(Worksheet sheet)
        {
            sheet.FocusPosChanged += Worksheet_FocusPosChanged;
            sheet.SelectionRangeChanged += Worksheet_SelectionRangeChanged;
            sheet.CellEditStarted += Worksheet_CellEditStarted;
            sheet.CellEditTextChanging += Worksheet_CellEditTextChanging;
            sheet.AfterCellEdit += Worksheet_AfterCellEdit;
        }

        private void DetachWorksheetEvents(Worksheet sheet)
        {
            sheet.FocusPosChanged -= Worksheet_FocusPosChanged;
            sheet.SelectionRangeChanged -= Worksheet_SelectionRangeChanged;
            sheet.CellEditStarted -= Worksheet_CellEditStarted;
            sheet.CellEditTextChanging -= Worksheet_CellEditTextChanging;
            sheet.AfterCellEdit -= Worksheet_AfterCellEdit;
        }

        private void Worksheet_FocusPosChanged(object? sender, CellPosEventArgs e) => Refresh();

        private void Worksheet_SelectionRangeChanged(object? sender, RangeEventArgs e) => Refresh();

        private void Worksheet_AfterCellEdit(object? sender, CellAfterEditEventArgs e) => Refresh();

        private void Worksheet_CellEditStarted(object? sender, CellEditStartedEventArgs e)
        {
            this.EditText = e.EditText;

            if (e.EditMode == CellEditMode.FormulaBar)
            {
                this.FocusEditBox();
            }
        }

        private void Worksheet_CellEditTextChanging(object? sender, CellEditTextChangingEventArgs e)
        {
            this.EditText = e.Text;
        }

        private void EditTextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            var sheet = this.Worksheet;

            if (sheet != null && sheet.IsEditing && sheet.EditMode == CellEditMode.FormulaBar)
            {
                sheet.CellEditText = this.EditText;
            }
        }

        private void EditTextBox_GotFocus(object? sender, FocusChangedEventArgs e)
        {
            var sheet = this.Worksheet;

            if (sheet != null && !sheet.IsEditing)
            {
                sheet.StartEdit(CellEditMode.FormulaBar);
            }
        }

        private void EditTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Confirm();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                Cancel();
                e.Handled = true;
            }
        }

        private void Confirm()
        {
            var sheet = this.Worksheet;

            if (sheet != null && sheet.IsEditing)
            {
                if (sheet.EditMode == CellEditMode.FormulaBar)
                {
                    sheet.EndEdit(sheet.CellEditText);
                }
                else
                {
                    sheet.EndEdit(EndEditReason.NormalFinish);
                }
            }

            this.EditSessionEnded?.Invoke();
        }

        private void Cancel()
        {
            var sheet = this.Worksheet;

            if (sheet != null && sheet.IsEditing && sheet.EditMode == CellEditMode.FormulaBar)
            {
                sheet.EndEdit(EndEditReason.Cancel);
            }

            this.EditSessionEnded?.Invoke();
        }

        private void Refresh()
        {
            if (this.editTextBox == null || !this.IsVisible) return;

            var sheet = this.Worksheet;

            if (sheet == null)
            {
                this.UpdateName(null);
                this.editTextBox.Text = string.Empty;
                return;
            }

            var pos = sheet.FocusPos;

            this.UpdateName(pos.IsEmpty ? null : pos.ToRelativeAddress());

            if (sheet.IsEditing) return;

            this.editTextBox.Text = GetEditDisplayText(sheet, pos);
        }

        private static string GetEditDisplayText(Worksheet sheet, CellPosition pos)
        {
            if (pos.IsEmpty) return string.Empty;

            var cell = sheet.GetCell(pos.Row, pos.Col);
            return cell?.GetEditText() ?? string.Empty;
        }
    }
}
