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
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Media;
using Calcita.AvaloniaPlatform;
using Calcita.Events;
using Calcita.Graphics;
using Calcita.Interaction;
using Calcita.Main;
using Calcita.Rendering;
using System;
using System.Diagnostics;
using static Calcita.CalcitaControl;

namespace Calcita
{
    public class SheetCanvas : Decorator, ICompViewAdapter
    {
        private Cursor? oldCursor = null;
        internal readonly AvaloniaRenderer renderer = new();
        private readonly Canvas canvas = new();
        internal readonly InputTextBox editTextbox;

        internal CalcitaControl? Owner { get; set; }

        #region Dps

        /// <summary>
        /// Worksheet StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<Worksheet?> WorksheetProperty =
            AvaloniaProperty.Register<SheetCanvas, Worksheet?>(nameof(Worksheet));

        /// <summary>
        /// CurrentScale StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<double> CurrentScaleProperty =
            AvaloniaProperty.Register<SheetCanvas, double>(nameof(CurrentScale), 1.0);

        /// <summary>
        /// MaxScale StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<double> MaxScaleProperty =
            AvaloniaProperty.Register<SheetCanvas, double>(nameof(MaxScale), 4.0);

        /// <summary>
        /// MinScale StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<double> MinScaleProperty =
            AvaloniaProperty.Register<SheetCanvas, double>(nameof(MinScale), 0.1);

        /// <summary>
        /// BaseScale StyledProperty definition
        /// </summary>
        public static readonly StyledProperty<double> BaseScaleProperty =
            AvaloniaProperty.Register<SheetCanvas, double>(nameof(BaseScale), 0.0);
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the BaseScale property. This StyledProperty
        /// indicates ....
        /// </summary>
        public double BaseScale
        {
            get => this.GetValue(BaseScaleProperty);
            set => SetValue(BaseScaleProperty, value);
        }

        /// <summary>
        /// Gets or sets the MinScale property. This StyledProperty
        /// indicates ....
        /// </summary>
        public double MinScale
        {
            get => this.GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, value);
        }

        /// <summary>
        /// Gets or sets the MaxScale property. This StyledProperty
        /// indicates ....
        /// </summary>
        public double MaxScale
        {
            get => this.GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }

        /// <summary>
        /// Gets or sets the CurrentScale property. This StyledProperty
        /// indicates ....
        /// </summary>
        public double CurrentScale
        {
            get => this.GetValue(CurrentScaleProperty);
            set => SetValue(CurrentScaleProperty, value);
        }

        /// <summary>
        /// Gets or sets the Worksheet property. This StyledProperty
        /// indicates ....
        /// </summary>
        public Worksheet? Worksheet
        {
            get => this.GetValue(WorksheetProperty);
            set => SetValue(WorksheetProperty, value);
        }
        #endregion

        #region Init
        static SheetCanvas()
        {

        }

        public SheetCanvas()
        {
            this.Focusable = true;

            this.SetValue(ToolTip.PlacementProperty, PlacementMode.Custom);

            this.Child = canvas;

            this.editTextbox = new InputTextBox()
            {
                Owner = this,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                [ScrollViewer.HorizontalScrollBarVisibilityProperty] = ScrollBarVisibility.Hidden,
                [ScrollViewer.VerticalScrollBarVisibilityProperty] = ScrollBarVisibility.Hidden,
            };
            this.canvas.Children.Add(editTextbox);

            // todo Gestures
            // Gestures.PinchEvent
            // Gestures.ScrollGestureEvent

            this.SizeChanged += CalcitaControl_SizeChanged;
            this.AddHandler(PointerPressedEvent, MouseDownHandler, handledEventsToo: true);
            this.AddHandler(PointerReleasedEvent, MouseUpHandler, handledEventsToo: true);
            PointerMoved += OnMouseMove;
            PointerWheelChanged += OnMouseWheel;
        }
        #endregion


        private void MouseUpHandler(object sender, PointerReleasedEventArgs e)
        {
            this.OnWorksheetMouseUp(e.GetPosition(this), AvaloniaUtility.ConvertToUIMouseButtons(e.InitialPressMouseButton));

            //if (mouseCaptured) ReleaseMouseCapture();
        }

        private void MouseDownHandler(object sender, PointerPressedEventArgs e)
        {
            Focus();

            var pos = e.GetPosition(this);

            //double right = this.Bounds.Size.Width;
            //double bottom = this.Bounds.Size.Height;

            //if (this.verScrollbar.IsVisible)
            //{
            //    right = Canvas.GetLeft(this.verScrollbar);
            //}

            //if (this.sheetTab.IsVisible)
            //{
            //    bottom = Canvas.GetTop(this.sheetTab);
            //}
            //else if (this.horScrollbar.IsVisible)
            //{
            //    bottom = Canvas.GetTop(this.horScrollbar);
            //}
            double right = this.Bounds.Size.Width;
            double bottom = this.Bounds.Size.Height;
            if (pos.X < right && pos.Y < bottom)
            {
                if (e.ClickCount == 2)
                {
                    this.Worksheet.OnMouseDoubleClick(e.GetPosition(this), AvaloniaUtility.ConvertToUIMouseButtons(e));
                }
                else
                {
                    this.OnWorksheetMouseDown(e.GetPosition(this), AvaloniaUtility.ConvertToUIMouseButtons(e));
                    //if (CaptureMouse()) mouseCaptured = true;
                }
            }
        }

        private void CalcitaControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            if (this.IsVisible)
            {
                if (e.PreviousSize.Width > 0)
                {
                    this.Owner.SheetTabWidth += e.NewSize.Width - e.PreviousSize.Width;
                    if (this.Owner.SheetTabWidth < 0) this.Owner.SheetTabWidth = 0;
                }
            }

            this.Owner.UpdateSheetTabAndScrollBarsLayout();

            this.InvalidateVisual();
        }


        #region Mouse

        bool mouseCaptured = false;

        protected void OnMouseMove(object sender, PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            this.OnWorksheetMouseMove(e.GetPosition(this), AvaloniaUtility.ConvertToUIMouseButtons(point.Properties));
        }

        protected void OnMouseWheel(object sender, PointerWheelEventArgs e)
        {
            this.Worksheet.OnMouseWheel(e.GetPosition(this), e.Delta * 120, e.KeyModifiers,
                AvaloniaUtility.ConvertToUIMouseButtons(MouseButton.Middle));
        }

        #region Mouse
        private void OnWorksheetMouseDown(RGPointF location, MouseButtons buttons)
        {
            var sheet = this.Worksheet;

            if (sheet != null)
            {
                // if currently control is in editing mode, make the input fields invisible
                if (sheet.currentEditingCell != null)
                {
                    if (this.Owner?.adapter is IEditableControlAdapter editableAdapter)
                    {
                        sheet.EndEdit(editableAdapter.GetEditControlText());
                    }
                }

                sheet.ViewportController?.OnMouseDown(location, buttons);
            }
        }

        private void OnWorksheetMouseMove(RGPointF location, MouseButtons buttons)
        {
            this.Worksheet?.ViewportController?.OnMouseMove(location, buttons);
        }

        private void OnWorksheetMouseUp(RGPointF location, MouseButtons buttons)
        {
            this.Worksheet?.ViewportController?.OnMouseUp(location, buttons);
        }
        #endregion // Mouse

        #endregion // Mouse

        #region Keyboard

        /// <summary>
        /// Handle event when key down.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!this.Worksheet.IsEditing)
            {
                var wfkeys = AvaloniaUtility.GetKeyCode(e.Key);

                if(wfkeys == KeyCode.LControlKey || wfkeys == KeyCode.RControlKey)
                    wfkeys = KeyCode.Control;

                if (wfkeys == KeyCode.LShiftKey || wfkeys == KeyCode.LShiftKey)
                    wfkeys = KeyCode.Shift;

                if ((e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
                {
                    wfkeys |= KeyCode.Control;
                }
                else if ((e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
                {
                    wfkeys |= KeyCode.Shift;
                }
                else if ((e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
                {
                    wfkeys |= KeyCode.Alt;
                }

                if (wfkeys != KeyCode.Control
                    && wfkeys != KeyCode.Shift
                    && wfkeys != KeyCode.Alt)
                {
                    if (this.Worksheet.OnKeyDown(wfkeys))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (!this.Worksheet.IsEditing)
            {
                var wfkeys = AvaloniaUtility.GetKeyCode(e.Key);

                if (wfkeys == KeyCode.LControlKey || wfkeys == KeyCode.RControlKey)
                    wfkeys = KeyCode.Control;

                if (wfkeys == KeyCode.LShiftKey || wfkeys == KeyCode.LShiftKey)
                    wfkeys = KeyCode.Shift;

                if ((e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control)
                {
                    wfkeys |= KeyCode.Control;
                }
                else if ((e.KeyModifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
                {
                    wfkeys |= KeyCode.Shift;
                }
                else if ((e.KeyModifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
                {
                    wfkeys |= KeyCode.Alt;
                }

                if (wfkeys != KeyCode.Control
                    && wfkeys != KeyCode.Shift
                    && wfkeys != KeyCode.Alt)
                {
                    if (this.Worksheet.OnKeyUp(wfkeys))
                    {
                        e.Handled = true;
                    }
                }

                //base.OnKeyUp(e);
            }
        }

        /// <summary>
        /// Handle event when text inputted
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
        }

        private void OnTextInputStart(object sender, TextInputEventArgs args)
        {
            if (!this.Worksheet.IsEditing)
            {
                this.Worksheet.StartEdit();
                this.Worksheet.CellEditText = string.Empty;
            }
        }

        #endregion // Keyboard

        public override void Render(PlatformGraphics dc)
        {
#if DEBUG
            Stopwatch watch = Stopwatch.StartNew();
#endif
            var sheet = this.Worksheet;

            using var ds = dc.PushTransform(Matrix.CreateTranslation(0.5, 0.5));
            if (this.Owner != null
                && sheet != null
                && sheet.workbook != null
                && sheet.controlAdapter != null)
            {

                SolidColorBrush bgBrush;
                if (this.Owner.ControlStyle.TryGetColor(ControlAppearanceColors.GridBackground, out SolidColor bgColor))
                {
                    bgBrush = new SolidColorBrush(bgColor);
                }
                else
                {
                    bgBrush = new SolidColorBrush(Colors.White);
                }

                dc.DrawRectangle(bgBrush, null, new Rect(0, 0, this.Bounds.Size.Width, this.Bounds.Size.Height));

                this.renderer.Reset();
                this.renderer.SetPlatformGraphics(dc);

                var rgdc = new CellDrawingContext(sheet, DrawMode.View, this.renderer);
                sheet.ViewportController?.Draw(rgdc);
            }

#if DEBUG
            watch.Stop();
            var elapsed = watch.Elapsed;
            if (elapsed.TotalMilliseconds > 30)
            {
                Debug.WriteLine(string.Format("end draw: {0} ms.", elapsed.TotalMilliseconds));
            }
#endif
            base.Render(dc);
        }

        #region Interface

        IVisualWorkbook ICompViewAdapter.ControlInstance => throw new System.NotImplementedException();

        IRenderer ICompViewAdapter.Renderer => renderer;

        ControlAppearanceStyle ICompViewAdapter.ControlStyle => throw new System.NotImplementedException();

        double ICompViewAdapter.BaseScale => this.BaseScale;

        double ICompViewAdapter.MinScale => this.MinScale;

        double ICompViewAdapter.MaxScale => this.MaxScale;

        bool ICompViewAdapter.IsVisible => this.IsVisible;

        ISheetTabControl? IMultisheetAdapter.SheetTabControl => throw new System.NotImplementedException();

        void ICompViewAdapter.ChangeCursor(CursorStyle cursor)
        {
            throw new System.NotImplementedException();
        }

        void ICompViewAdapter.RestoreCursor()
        {
            throw new System.NotImplementedException();
        }

        void ICompViewAdapter.ChangeSelectionCursor(CursorStyle cursor)
        {
            throw new System.NotImplementedException();
        }

        Rectangle ICompViewAdapter.GetContainerBounds()
        {
            return this.Bounds;
        }

        void ICompViewAdapter.Focus()
        {
            this.Focus();
        }

        void ICompViewAdapter.Invalidate()
        {
            this.InvalidateVisual();
        }

        void ICompViewAdapter.ChangeBackgroundColor(SolidColor color)
        {
            //throw new System.NotImplementedException();
        }

        Graphics.Point ICompViewAdapter.PointToScreen(Graphics.Point point)
        {
            var p = this.PointToScreen(point);
            return new Graphics.Point(p.X, p.Y);
        }

        void ICompViewAdapter.ShowTooltip(Graphics.Point point, string content)
        {
            this.SetValue(ToolTip.CustomPopupPlacementCallbackProperty, new CustomPopupPlacementCallback(ps =>
            {
                ps.Offset = new Avalonia.Point(point.X, point.Y);
            }));
            this.SetValue(ToolTip.TipProperty, content);
            this.SetValue(ToolTip.IsOpenProperty, true);
        }
        #endregion
    }
}





