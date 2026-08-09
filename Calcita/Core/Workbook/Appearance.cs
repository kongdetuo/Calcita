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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Calcita.Controls;
using Calcita.Graphics;
using Calcita.Rendering;
using Calcita.Utility;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;


namespace Calcita
{
	#region Appearance

	/// <summary>
	/// Key of control appearance item
	/// </summary>
	internal enum ControlAppearanceColors : short
	{
#pragma warning disable 1591
		LeadHeadNormal = 1,
		LeadHeadSelected = 3,

		LeadHeadIndicator = 11,

		ColHeadNormal = 21,
		ColHeadHover = 23,
		ColHeadSelected = 25,
		ColHeadFullSelected = 27,
		ColHeadInvalid = 29,
		ColHeadText = 36,

		RowHeadSplitter = 40,
		RowHeadNormal = 41,
		RowHeadHover = 42,
		RowHeadSelected = 43,
		RowHeadFullSelected = 44,
		RowHeadInvalid = 45,
		RowHeadText = 51,

		SelectionBorder = 61,
		SelectionFill = 62,

		GridBackground = 81,
		GridText = 82,

		GridLine = 83,

		OutlinePanelBorder = 91,
		OutlinePanelBackground = 92,
		OutlineButtonBorder = 93,
		OutlineButtonText = 94,

		//SheetTabBorder = 201,
		//SheetTabBackground = 202,
		//SheetTabText = 203,
		//SheetTabSelected = 204,

#pragma warning restore 1591
	}
		
	/// <summary>
	/// ReoGrid Control Appearance Colors
	/// </summary>
	internal class ControlAppearanceStyle
	{
		private Dictionary<ControlAppearanceColors, IRGBrush> brushes = new Dictionary<ControlAppearanceColors, IRGBrush>(100);

		private Dictionary<(ControlAppearanceColors key, RGFloat weight), IRGPen> pens = new Dictionary<(ControlAppearanceColors, RGFloat), IRGPen>();

		internal static readonly Dictionary<ControlAppearanceColors, string> ResourceKeys =
			new Dictionary<ControlAppearanceColors, string>
			{
				{ControlAppearanceColors.LeadHeadNormal, "CalcitaGridLeadHeaderBrush"},
				{ControlAppearanceColors.LeadHeadSelected, "CalcitaGridLeadHeaderSelectedBrush"},
				{ControlAppearanceColors.LeadHeadIndicator, "CalcitaGridLeadHeaderIndicatorBrush"},
				{ControlAppearanceColors.ColHeadNormal, "CalcitaGridColHeaderBrush"},
				{ControlAppearanceColors.ColHeadHover, "CalcitaGridColHeaderHoverBrush"},
				{ControlAppearanceColors.ColHeadSelected, "CalcitaGridColHeaderSelectedBrush"},
				{ControlAppearanceColors.ColHeadFullSelected, "CalcitaGridColHeaderFullSelectedBrush"},
				{ControlAppearanceColors.ColHeadInvalid, "CalcitaGridColHeaderInvalidBrush"},
				{ControlAppearanceColors.ColHeadText, "CalcitaGridColHeaderTextBrush"},
				{ControlAppearanceColors.RowHeadSplitter, "CalcitaGridRowSplitterBrush"},
				{ControlAppearanceColors.RowHeadNormal, "CalcitaGridRowHeaderBrush"},
				{ControlAppearanceColors.RowHeadHover, "CalcitaGridRowHeaderHoverBrush"},
				{ControlAppearanceColors.RowHeadSelected, "CalcitaGridRowHeaderSelectedBrush"},
				{ControlAppearanceColors.RowHeadFullSelected, "CalcitaGridRowHeaderFullSelectedBrush"},
				{ControlAppearanceColors.RowHeadInvalid, "CalcitaGridRowHeaderInvalidBrush"},
				{ControlAppearanceColors.RowHeadText, "CalcitaGridRowHeaderTextBrush"},
				{ControlAppearanceColors.SelectionBorder, "CalcitaGridSelectionBorderBrush"},
				{ControlAppearanceColors.SelectionFill, "CalcitaGridSelectionFillBrush"},
				{ControlAppearanceColors.GridBackground, "CalcitaGridBackgroundBrush"},
				{ControlAppearanceColors.GridText, "CalcitaGridTextBrush"},
				{ControlAppearanceColors.GridLine, "CalcitaGridLineBrush"},
				{ControlAppearanceColors.OutlinePanelBorder, "CalcitaCtrlOutlinePanelBorderBrush"},
				{ControlAppearanceColors.OutlinePanelBackground, "CalcitaCtrlOutlinePanelBackgroundBrush"},
				{ControlAppearanceColors.OutlineButtonBorder, "CalcitaCtrlOutlineButtonBorderBrush"},
				{ControlAppearanceColors.OutlineButtonText, "CalcitaCtrlOutlineButtonTextBrush"},
			};

		/// <summary>
		/// Get or set selection border weight
		/// </summary>
		public float SelectionBorderWidth { get; set; }

		/// <summary>
		/// Construct empty control appearance
		/// </summary>
		private ControlAppearanceStyle()
		{
			this.SelectionBorderWidth = 3f;
		}

		internal IRGBrush GetColHeadBrush(bool isHover, bool isSelected, bool isFullSelected, bool isInvalid)
		{
			if (isFullSelected)
				return GetBrush(ControlAppearanceColors.ColHeadFullSelected);
			else if (isSelected)
				return GetBrush(ControlAppearanceColors.ColHeadSelected);
			else if (isHover)
				return GetBrush(ControlAppearanceColors.ColHeadHover);
			else if (isInvalid)
				return GetBrush(ControlAppearanceColors.ColHeadInvalid);
			else
				return GetBrush(ControlAppearanceColors.ColHeadNormal);
		}
		internal IRGBrush GetRowHeadBrush(bool isHover, bool isSelected, bool isFullSelected, bool isInvalid)
		{
			if (isFullSelected)
				return GetBrush(ControlAppearanceColors.RowHeadFullSelected);
			else if (isSelected)
				return GetBrush(ControlAppearanceColors.RowHeadSelected);
			else if (isHover)
				return GetBrush(ControlAppearanceColors.RowHeadHover);
			else if (isInvalid)
				return GetBrush(ControlAppearanceColors.RowHeadInvalid);
			else
				return GetBrush(ControlAppearanceColors.RowHeadNormal);
		}

/// <summary>
		/// Rebuild the appearance brushes from the theme resources reachable from
		/// the given host. Slots without a matching resource fall back to their
		/// default style brush. Called on every theme or resource change; the
		/// resulting repaint is coalesced to the next frame, so no change
		/// detection is needed here.
		/// </summary>
		internal void RefreshFromHost(StyledElement host)
		{
			if (host == null) return;

			var theme = host.ActualThemeVariant;
			var defaults = CreateDefaultControlStyle();
			var newBrushes = new Dictionary<ControlAppearanceColors, IRGBrush>(defaults.brushes);

			foreach (var slot in (ControlAppearanceColors[])Enum.GetValues(typeof(ControlAppearanceColors)))
			{
				if (ResourceKeys.TryGetValue(slot, out var key) &&
					host.TryFindResource(key, theme, out var value))
				{
					if (value is IRGBrush brush)
					{
						newBrushes[slot] = brush.ToImmutable();
					}
				}
			}

			this.brushes = newBrushes;
			this.pens.Clear();
		}

		/// <summary>
		/// Get the brush resource attached to the given appearance slot. Brushes are
		/// taken directly from the host theme (they may be gradients or solids), so
		/// consumers should pass them to <see cref="IGraphics"/> without resolving a
		/// color first.
		/// </summary>
		internal IRGBrush GetBrush(ControlAppearanceColors key)
		{
			this.brushes.TryGetValue(key, out var brush);
			return brush;
		}

		/// <summary>
		/// Get a single-pixel solid pen made from the brush of the given appearance
		/// slot. The brush is resolved from the host theme like <see cref="GetBrush"/>.
		/// </summary>
		internal IRGPen GetPen(ControlAppearanceColors key)
		{
			return GetPen(key, 1);
		}

		/// <summary>
		/// Get a solid pen with the given weight made from the brush of the given
		/// appearance slot. The brush is resolved from the host theme like
		/// <see cref="GetBrush"/>. Pens are cached by slot and weight until the
		/// theme resources are refreshed.
		/// </summary>
		internal IRGPen GetPen(ControlAppearanceColors key, RGFloat weight)
		{
			if (this.pens.TryGetValue((key, weight), out var pen))
			{
				return pen;
			}

			var brush = GetBrush(key);
			if (brush == null) return null;

			pen = new RGPen(brush, weight).ToImmutable();
			this.pens[(key, weight)] = pen;
			return pen;
		}

		/// <summary>
		/// Create default style for grid control.
		/// </summary>
		/// <returns>Default style created</returns>
		public static ControlAppearanceStyle CreateDefaultControlStyle()
		{
			return new ControlAppearanceStyle
			{
				brushes = new Dictionary<ControlAppearanceColors, IRGBrush>
					{
						{ControlAppearanceColors.LeadHeadNormal, new SolidColorBrush((Avalonia.Media.Color)SolidColor.Lavender).ToImmutable()},
						{ControlAppearanceColors.LeadHeadSelected, new SolidColorBrush((Avalonia.Media.Color)SolidColor.Lavender).ToImmutable()},
						{ControlAppearanceColors.LeadHeadIndicator, new SolidColorBrush((Avalonia.Media.Color)SolidColor.Gainsboro).ToImmutable()},
						{ControlAppearanceColors.ColHeadNormal, new SolidColorBrush((Avalonia.Media.Color)SolidColor.White).ToImmutable()},
						{ControlAppearanceColors.ColHeadHover, new SolidColorBrush((Avalonia.Media.Color)SolidColor.LightGoldenrodYellow).ToImmutable()},
						{ControlAppearanceColors.ColHeadSelected, new SolidColorBrush((Avalonia.Media.Color)SolidColor.LightGoldenrodYellow).ToImmutable()},
						{ControlAppearanceColors.ColHeadFullSelected, new SolidColorBrush((Avalonia.Media.Color)SolidColor.WhiteSmoke).ToImmutable()},
						{ControlAppearanceColors.ColHeadText, new SolidColorBrush((Avalonia.Media.Color)SolidColor.DarkBlue).ToImmutable()},
						{ControlAppearanceColors.RowHeadSplitter, new SolidColorBrush((Avalonia.Media.Color)SolidColor.LightSteelBlue).ToImmutable()},
						{ControlAppearanceColors.RowHeadNormal, new SolidColorBrush((Avalonia.Media.Color)SolidColor.AliceBlue).ToImmutable()},
						{ControlAppearanceColors.RowHeadHover, new SolidColorBrush((Avalonia.Media.Color)SolidColor.LightSteelBlue).ToImmutable()},
						{ControlAppearanceColors.RowHeadSelected, new SolidColorBrush((Avalonia.Media.Color)SolidColor.PaleGoldenrod).ToImmutable()},
						{ControlAppearanceColors.RowHeadFullSelected, new SolidColorBrush((Avalonia.Media.Color)SolidColor.LemonChiffon).ToImmutable()},
						{ControlAppearanceColors.RowHeadText, new SolidColorBrush((Avalonia.Media.Color)SolidColor.DarkBlue).ToImmutable()},
						{ControlAppearanceColors.GridText, new SolidColorBrush((Avalonia.Media.Color)SolidColor.Black).ToImmutable()},
						{ControlAppearanceColors.GridBackground, new SolidColorBrush((Avalonia.Media.Color)SolidColor.White).ToImmutable()},
						{ControlAppearanceColors.GridLine, new SolidColorBrush((Avalonia.Media.Color)SolidColor.FromArgb(255, 208, 215, 229)).ToImmutable()},
						{ControlAppearanceColors.SelectionBorder, new SolidColorBrush((Avalonia.Media.Color)ColorUtility.FromAlphaColor(180, StaticResources.SystemColor_Highlight)).ToImmutable()},
						{ControlAppearanceColors.SelectionFill, new SolidColorBrush((Avalonia.Media.Color)ColorUtility.FromAlphaColor(30, StaticResources.SystemColor_Highlight)).ToImmutable()},
						{ControlAppearanceColors.OutlineButtonBorder, new SolidColorBrush((Avalonia.Media.Color)SolidColor.Black).ToImmutable()},
						{ControlAppearanceColors.OutlinePanelBackground, new SolidColorBrush((Avalonia.Media.Color)StaticResources.SystemColor_Control).ToImmutable()},
						{ControlAppearanceColors.OutlinePanelBorder, new SolidColorBrush((Avalonia.Media.Color)SolidColor.Silver).ToImmutable()},
						{ControlAppearanceColors.OutlineButtonText, new SolidColorBrush((Avalonia.Media.Color)StaticResources.SystemColor_WindowText).ToImmutable()},
				},

				SelectionBorderWidth = 3,
			};
		}
	}
	#endregion // Appearance

}


