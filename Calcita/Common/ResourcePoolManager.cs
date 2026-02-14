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
 ****************************************************************************/using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Calcita.Graphics;

namespace Calcita.Common
{
    internal sealed class ResourcePoolManager : IDisposable
    {
        //private static readonly ResourcePoolManager instance = new ResourcePoolManager();
        //public static ResourcePoolManager Instance { get { return instance; } }

        internal ResourcePoolManager()
        {
            Logger.Log("resource pool", "create resource pool...");
        }

		public ImageBrush GetHatchImageBrush(HatchStyles style, SolidColor foreColor, SolidColor backColor, double spacing)
		{

			var key = new HatchBrushKey(style, spacing, foreColor, backColor);

			lock (hatchImageBrushes)
			{
				if (hatchImageBrushes.TryGetValue(key, out var brush)) return brush;

				// create small tile bitmap
				double tile = Math.Max(4.0, spacing * 4.0);
				int w = Math.Max(1, (int)Math.Ceiling(tile));
				int h = w;

				var rtb = new RenderTargetBitmap(new PixelSize(w, h));

                // draw into DrawingContext
                using (var ctx = rtb.CreateDrawingContext())
                {

                    // fill background
                    if (backColor.A > 0)
                    {
                        var backBrush = new SolidColorBrush(backColor);
                        ctx.FillRectangle(backBrush, new Rect(0, 0, w, h));
                    }

                    var foreBrush = new SolidColorBrush(foreColor);
                    var pen = new Pen(foreBrush, 1);

                    // special-case percent-based hatch styles: draw filled cells to approximate density
					if ((int)style >= (int)HatchStyles.Percent05 && (int)style <= (int)HatchStyles.Percent90)
					{
						int[] percentMap = new int[] { 5, 10, 20, 25, 30, 40, 50, 60, 70, 75, 80, 90 };
						int idx = (int)style - (int)HatchStyles.Percent05;
						int percent = percentMap[Math.Max(0, Math.Min(percentMap.Length - 1, idx))];

						int grid = 24; // 6x6 grid
						int total = grid * grid;
						int toFill = (int)Math.Round(total * percent / 100.0);

						int cellW = Math.Max(1, w / grid);
						int cellH = Math.Max(1, h / grid);

						int seed = percent * 97 + w;
						for (int i = 0; i < toFill; i++)
						{
							int index = (i * 37 + seed) % total;
							int gx = index % grid;
							int gy = index / grid;
							var rx = new Rect(gx * cellW, gy * cellH, cellW, cellH);
							ctx.FillRectangle(foreBrush, rx);
						}
					}
					else
					{
                    // Special precise cases
					if (style == HatchStyles.SolidDiamond)
					{
						// draw filled diamonds across the tile
						double centerX = w / 2.0;
						double centerY = h / 2.0;
						double dx = Math.Max(2, w / 4.0);
						double dy = Math.Max(2, h / 4.0);

						var poly = new Avalonia.Point[] {
							new Avalonia.Point(centerX, centerY - dy),
							new Avalonia.Point(centerX + dx, centerY),
							new Avalonia.Point(centerX, centerY + dy),
							new Avalonia.Point(centerX - dx, centerY)
						};
						// tile multiple diamonds
						for (double ty = -h; ty <= h * 2; ty += dy * 2)
						{
							for (double tx = -w; tx <= w * 2; tx += dx * 2)
							{
								var pts = poly.Select(p => new Avalonia.Point(p.X + tx, p.Y + ty)).ToArray();
								var fig = new PathFigure { StartPoint = pts[0], IsClosed = true };
								fig.Segments.Add(new LineSegment { Point = pts[1] });
								fig.Segments.Add(new LineSegment { Point = pts[2] });
								fig.Segments.Add(new LineSegment { Point = pts[3] });
								var ggeo = new PathGeometry();
								ggeo.Figures.Add(fig);
								ctx.DrawGeometry(foreBrush, null, ggeo);
							}
						}
					}
					else if (style == HatchStyles.OutlinedDiamond)
					{
						double centerX = w / 2.0;
						double centerY = h / 2.0;
						double dx = Math.Max(2, w / 4.0);
						double dy = Math.Max(2, h / 4.0);
						for (double ty = -h; ty <= h * 2; ty += dy * 2)
						{
							for (double tx = -w; tx <= w * 2; tx += dx * 2)
							{
								var p1 = new Avalonia.Point(centerX + tx, centerY - dy + ty);
								var p2 = new Avalonia.Point(centerX + dx + tx, centerY + ty);
								var p3 = new Avalonia.Point(centerX + tx, centerY + dy + ty);
								var p4 = new Avalonia.Point(centerX - dx + tx, centerY + ty);
								var geo = new PathGeometry();
								var fig = new PathFigure() { StartPoint = p1, IsClosed = true };
								fig.Segments.Add(new LineSegment() { Point = p2 });
								fig.Segments.Add(new LineSegment() { Point = p3 });
								fig.Segments.Add(new LineSegment() { Point = p4 });
								geo.Figures.Add(fig);
								ctx.DrawGeometry(null, pen, geo);
							}
						}
					}
					else if (style == HatchStyles.SmallCheckerBoard || style == HatchStyles.LargeCheckerBoard)
					{
						int cell = style == HatchStyles.SmallCheckerBoard ? 6 : 12;
						int cols = Math.Max(1, w / cell);
						int rows = Math.Max(1, h / cell);
						for (int ry = 0; ry <= rows; ry++)
						{
							for (int rx = 0; rx <= cols; rx++)
							{
								if (((rx + ry) & 1) == 0)
								{
									var rxr = new Rect(rx * cell, ry * cell, cell, cell);
									ctx.FillRectangle(foreBrush, rxr);
								}
							}
						}
					}
					else
					{
						var segments = GetHatchPattern(style, spacing);
						foreach (var seg in segments)
						{
							var p1 = new Avalonia.Point(seg.Item1.X, seg.Item1.Y);
							var p2 = new Avalonia.Point(seg.Item2.X, seg.Item2.Y);
							ctx.DrawLine(pen, p1, p2);
						}
					}
					}
                }

                var img = new ImageBrush(rtb)
				{
					TileMode = TileMode.Tile,
					Stretch = Stretch.None,
					AlignmentX = AlignmentX.Left,
					AlignmentY = AlignmentY.Top,
                    DestinationRect = new RelativeRect(0, 0, w, h, RelativeUnit.Absolute),
                };

				hatchImageBrushes[key] = img;
				return img;
			}
		}

        #region Brush
#if WINFORM || WPF || AVALONIA
        private Dictionary<SolidColor, RGSolidBrush> cachedBrushes = new Dictionary<SolidColor, RGSolidBrush>();

        public RGSolidBrush GetBrush(SolidColor color)
        {
            if (color.A == 0) return null;

            lock (cachedBrushes)
            {
                if (cachedBrushes.TryGetValue(color, out var b))
                {
                    return b;
                }
                else
                {
                    b = new RGSolidBrush(color);
                    cachedBrushes.Add(color, b);

                    if ((cachedBrushes.Count % 10) == 0)
                    {
                        Logger.Log("resource pool", "solid brush count: " + cachedBrushes.Count);
                    }

                    return b;
                }
            }
        }
#endif // WINFORM || WPF

#if WINFORM
		private Dictionary<HatchStyleBrushInfo, HatchBrush> hatchBrushes = new Dictionary<HatchStyleBrushInfo, HatchBrush>();

		public HatchBrush GetHatchBrush(HatchStyle style, SolidColor foreColor, SolidColor backColor)
		{
			HatchStyleBrushInfo info = new HatchStyleBrushInfo(style, foreColor, backColor);

			lock (this.hatchBrushes)
			{
				if (hatchBrushes.TryGetValue(info, out var hb))
				{
					return hb;
				}
				else
				{
					HatchBrush b = new HatchBrush(style, foreColor, backColor);
					hatchBrushes.Add(info, b);

					Logger.Log("resource pool", "add hatch brush, count: " + hatchBrushes.Count);
					return b;
				}
			}
		}
		private struct HatchStyleBrushInfo
		{
			internal HatchStyle style;
			internal SolidColor foreColor;
			internal SolidColor backgroundColor;

			public HatchStyleBrushInfo(HatchStyle style, SolidColor foreColor, SolidColor backgroundColor)
			{
				this.style = style;
				this.foreColor = foreColor;
				this.backgroundColor = backgroundColor;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is HatchStyleBrushInfo)) return false;

				HatchStyleBrushInfo right = (HatchStyleBrushInfo)obj;
				return (this.style == right.style
					&& this.foreColor == right.foreColor
					&& this.backgroundColor == right.backgroundColor);
			}

			public static bool operator ==(HatchStyleBrushInfo left, HatchStyleBrushInfo right)
			{
				return left.Equals(right);

				// type converted from class
				//if (left == null && right == null) return true;
				//if (left == null || right == null) return false;

				//if (left == null)
				//	return right.Equals(left);
				//else
				//	return left.Equals(right);
			}

			public static bool operator !=(HatchStyleBrushInfo left, HatchStyleBrushInfo right)
			{
				return !(left == right);
			}

			public override int GetHashCode()
			{
				return (short)style * (foreColor.ToArgb() + backgroundColor.ToArgb());
			}
		}
#endif // WINFORM
        #endregion Brush

#if AVALONIA
		// simple cache for hatch patterns (tile-based line segments)
		private readonly Dictionary<HatchPatternKey, List<(Avalonia.Point, Avalonia.Point)>> hatchPatterns
			= new Dictionary<HatchPatternKey, List<(Avalonia.Point, Avalonia.Point)>>();

		private readonly Dictionary<HatchBrushKey, ImageBrush> hatchImageBrushes = new();

		private struct HatchBrushKey
		{
			public HatchStyles Style;
			public double Spacing;
			public SolidColor Fore;
			public SolidColor Back;

			public HatchBrushKey(HatchStyles s, double sp, SolidColor f, SolidColor b)
			{
				Style = s; Spacing = sp; Fore = f; Back = b;
			}

			public override bool Equals(object obj)
			{
				if (obj is HatchBrushKey k)
				{
					return k.Style == Style && Math.Abs(k.Spacing - Spacing) < 1e-6 && k.Fore.Equals(Fore) && k.Back.Equals(Back);
				}
				return false;
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int h = (int)Style;
					h = h * 31 + Spacing.GetHashCode();
					h = h * 31 + Fore.GetHashCode();
					h = h * 31 + Back.GetHashCode();
					return h;
				}
			}
		}

		private struct HatchPatternKey
		{
			public HatchStyles Style;
			public double Spacing;

			public HatchPatternKey(HatchStyles s, double sp)
			{
				Style = s;
				Spacing = sp;
			}

			public override bool Equals(object obj)
			{
				if (obj is HatchPatternKey k)
				{
					return k.Style == Style && Math.Abs(k.Spacing - Spacing) < 1e-6;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (int)Style * 397 ^ Spacing.GetHashCode();
			}
		}

		// Return a cached tile pattern (list of line segments in tile coordinates)
		public List<(Avalonia.Point, Avalonia.Point)> GetHatchPattern(HatchStyles style, double spacing)
		{
			var key = new HatchPatternKey(style, spacing);

			lock (hatchPatterns)
			{
				if (hatchPatterns.TryGetValue(key, out var list))
					return list;

				list = CreateHatchPattern(style, spacing);
				hatchPatterns[key] = list;
				return list;
			}
		}

		private List<(Avalonia.Point, Avalonia.Point)> CreateHatchPattern(HatchStyles style, double spacing)
		{
			var list = new List<(Avalonia.Point, Avalonia.Point)>();
			double s = Math.Max(1.0, spacing);
			double tile = s * 4; // tile size to cover diagonals

			switch (style)
			{
				case HatchStyles.Horizontal:
					for (double y = 0; y <= tile; y += s)
						list.Add((new Avalonia.Point(0, y), new Avalonia.Point(tile, y)));
					break;

				case HatchStyles.Vertical:
					for (double x = 0; x <= tile; x += s)
						list.Add((new Avalonia.Point(x, 0), new Avalonia.Point(x, tile)));
					break;

				case HatchStyles.Cross:
					for (double y = 0; y <= tile; y += s)
						list.Add((new Avalonia.Point(0, y), new Avalonia.Point(tile, y)));
					for (double x = 0; x <= tile; x += s)
						list.Add((new Avalonia.Point(x, 0), new Avalonia.Point(x, tile)));
					break;

				case HatchStyles.ForwardDiagonal:
				case HatchStyles.BackwardDiagonal:
				case HatchStyles.DiagonalCross:
					// create diagonal lines across tile
					for (double k = -tile; k <= tile * 2; k += s)
					{
						if (style == HatchStyles.ForwardDiagonal || style == HatchStyles.DiagonalCross)
							list.Add((new Avalonia.Point(k, 0), new Avalonia.Point(k + tile, tile)));
						if (style == HatchStyles.BackwardDiagonal || style == HatchStyles.DiagonalCross)
							list.Add((new Avalonia.Point(k, tile), new Avalonia.Point(k + tile, 0)));
					}
					break;

				case HatchStyles.LightDownwardDiagonal:
					// denser forward diagonal
					for (double k = -tile; k <= tile * 2; k += s * 0.5)
						list.Add((new Avalonia.Point(k, 0), new Avalonia.Point(k + tile, tile)));
					break;

				case HatchStyles.LightUpwardDiagonal:
					for (double k = -tile; k <= tile * 2; k += s * 0.5)
						list.Add((new Avalonia.Point(k, tile), new Avalonia.Point(k + tile, 0)));
					break;

				case HatchStyles.DarkDownwardDiagonal:
					// thicker: draw two nearby diagonals
					for (double k = -tile; k <= tile * 2; k += s)
					{
						list.Add((new Avalonia.Point(k, 0), new Avalonia.Point(k + tile, tile)));
						list.Add((new Avalonia.Point(k + 1, 0), new Avalonia.Point(k + tile + 1, tile)));
					}
					break;

				case HatchStyles.DarkUpwardDiagonal:
					for (double k = -tile; k <= tile * 2; k += s)
					{
						list.Add((new Avalonia.Point(k, tile), new Avalonia.Point(k + tile, 0)));
						list.Add((new Avalonia.Point(k + 1, tile), new Avalonia.Point(k + tile + 1, 0)));
					}
					break;

				case HatchStyles.WideDownwardDiagonal:
					for (double k = -tile; k <= tile * 2; k += s * 2)
						list.Add((new Avalonia.Point(k, 0), new Avalonia.Point(k + tile * 1.2, tile)));
					break;

				case HatchStyles.WideUpwardDiagonal:
					for (double k = -tile; k <= tile * 2; k += s * 2)
						list.Add((new Avalonia.Point(k, tile), new Avalonia.Point(k + tile * 1.2, 0)));
					break;

				case HatchStyles.LightVertical:
					for (double x = 0; x <= tile; x += s * 0.5)
						list.Add((new Avalonia.Point(x, 0), new Avalonia.Point(x, tile)));
					break;

				case HatchStyles.LightHorizontal:
					for (double y = 0; y <= tile; y += s * 0.5)
						list.Add((new Avalonia.Point(0, y), new Avalonia.Point(tile, y)));
					break;

				case HatchStyles.NarrowVertical:
					for (double x = 0; x <= tile; x += s * 0.75)
						list.Add((new Avalonia.Point(x, 0), new Avalonia.Point(x, tile)));
					break;

				case HatchStyles.NarrowHorizontal:
					for (double y = 0; y <= tile; y += s * 0.75)
						list.Add((new Avalonia.Point(0, y), new Avalonia.Point(tile, y)));
					break;

				case HatchStyles.DarkVertical:
					for (double x = 0; x <= tile; x += s)
					{
						list.Add((new Avalonia.Point(x, 0), new Avalonia.Point(x, tile)));
						list.Add((new Avalonia.Point(x + 1, 0), new Avalonia.Point(x + 1, tile)));
					}
					break;

				case HatchStyles.DarkHorizontal:
					for (double y = 0; y <= tile; y += s)
					{
						list.Add((new Avalonia.Point(0, y), new Avalonia.Point(tile, y)));
						list.Add((new Avalonia.Point(0, y + 1), new Avalonia.Point(tile, y + 1)));
					}
					break;

				case HatchStyles.DashedDownwardDiagonal:
				case HatchStyles.DashedUpwardDiagonal:
				case HatchStyles.DashedHorizontal:
				case HatchStyles.DashedVertical:
					{
						double dash = s * 1.5;
						if (style == HatchStyles.DashedHorizontal)
						{
							for (double y = 0; y <= tile; y += s)
							{
								for (double x = 0; x <= tile; x += dash * 2)
									list.Add((new Avalonia.Point(x, y), new Avalonia.Point(Math.Min(tile, x + dash), y)));
							}
						}
						else if (style == HatchStyles.DashedVertical)
						{
							for (double x = 0; x <= tile; x += s)
							{
								for (double y = 0; y <= tile; y += dash * 2)
									list.Add((new Avalonia.Point(x, y), new Avalonia.Point(x, Math.Min(tile, y + dash))));
							}
						}
						else
						{
							for (double k = -tile; k <= tile * 2; k += s)
							{
								for (double off = 0; off <= tile; off += dash * 2)
								{
									double x1 = k + off;
									double y1 = 0 + off;
									double x2 = Math.Min(k + tile, k + off + dash);
									double y2 = Math.Min(tile, off + dash);
									if (style == HatchStyles.DashedDownwardDiagonal)
										list.Add((new Avalonia.Point(x1, y1), new Avalonia.Point(x2, y2)));
									else
										list.Add((new Avalonia.Point(x1, tile - y1), new Avalonia.Point(x2, tile - y2)));
								}
							}
						}
					}
					break;

				case HatchStyles.SmallConfetti:
				case HatchStyles.LargeConfetti:
					{
						int step = style == HatchStyles.SmallConfetti ? 6 : 12;
						int seed = (int)style * 997;
						for (int y = 0; y <= (int)tile; y += step)
						{
							for (int x = 0; x <= (int)tile; x += step)
							{
								int vx = (x * 33 + y * 97 + seed) % (step);
								int vy = (x * 71 + y * 13 + seed) % (step);
								list.Add((new Avalonia.Point(x + vx, y + vy), new Avalonia.Point(x + vx + 0.5, y + vy)));
							}
						}
					}
					break;

				case HatchStyles.ZigZag:
				case HatchStyles.Wave:
					{
						// approximate as small horizontal zig segments
						for (double y = 0; y <= tile; y += s * 2)
						{
							for (double x = 0; x <= tile; x += s)
							{
								double nx = Math.Min(tile, x + s * 0.6);
								double dy = (style == HatchStyles.ZigZag) ? ((x / s) % 2 == 0 ? 1 : -1) * (s * 0.3) : Math.Sin(x / s) * (s * 0.3);
								list.Add((new Avalonia.Point(x, y), new Avalonia.Point(nx, y + dy)));
							}
						}
					}
					break;

				case HatchStyles.SmallGrid:
					for (double y = 0; y <= tile; y += s * 0.5)
						list.Add((new Avalonia.Point(0, y), new Avalonia.Point(tile, y)));
					for (double x = 0; x <= tile; x += s * 0.5)
						list.Add((new Avalonia.Point(x, 0), new Avalonia.Point(x, tile)));
					break;

				case HatchStyles.SmallCheckerBoard:
				case HatchStyles.LargeCheckerBoard:
					{
						double cell = style == HatchStyles.SmallCheckerBoard ? s : s * 2;
						for (double y = 0; y <= tile; y += cell)
						{
							for (double x = 0; x <= tile; x += cell)
							{
								// draw top edge of square
								list.Add((new Avalonia.Point(x, y), new Avalonia.Point(Math.Min(tile, x + cell), y)));
							}
						}
					}
					break;

				case HatchStyles.OutlinedDiamond:
				case HatchStyles.SolidDiamond:
					// crisscross diamonds approximate
					for (double k = -tile; k <= tile * 2; k += s)
					{
						list.Add((new Avalonia.Point(k, tile / 2), new Avalonia.Point(k + tile / 2, tile)));
						list.Add((new Avalonia.Point(k, tile / 2), new Avalonia.Point(k + tile / 2, 0)));
					}
					break;

				default:
					// fallback: sparse points as tiny horizontal segments
					for (double y = 0; y <= tile; y += s)
						for (double x = 0; x <= tile; x += s)
							list.Add((new Avalonia.Point(x, y), new Avalonia.Point(x + 0.5, y)));
					break;
			}

			return list;
		}
#endif // AVALONIA

        #region Pen
        private Dictionary<SolidColor, List<RGPen>> cachedPens = new Dictionary<SolidColor, List<RGPen>>();
        public RGPen GetPen(SolidColor color)
        {
            return GetPen(color, 1, RGDashStyles.Solid);
        }
        public RGPen GetPen(SolidColor color, RGFloat weight, RGDashStyle style)
        {
            if (color.A == 0) return null;

            RGPen pen;
            List<RGPen> penlist;

            lock (cachedPens)
            {
                if (!cachedPens.TryGetValue(color, out penlist))
                {
                    penlist = cachedPens[color] = new List<RGPen>();

                    penlist.Add(pen = new RGPen(new RGSolidBrush(color), weight));

                    pen.DashStyle = style;

                    if ((cachedPens.Count % 10) == 0)
                    {
                        Logger.Log("resource pool", "wf pen count: " + cachedPens.Count);
                    }
                }
                else
                {
                    lock (penlist)
                    {
                        pen = penlist.FirstOrDefault(p => p.Thickness == weight && p.DashStyle == style);
                    }

                    if (pen == null)
                    {

                        penlist.Add(pen = new RGPen(new RGSolidBrush(color), weight));
                        pen.DashStyle = style;

                        if ((cachedPens.Count % 10) == 0)
                        {
                            Logger.Log("resource pool", "pen count: " + cachedPens.Count);
                        }
                    }
                }
            }

            return pen;
        }
        #endregion // Pen


        #region Font

#if AVALONIA

        private Dictionary<string, FontFamily> fontFamilies = new();

        public FontFamily GetFontFamily(string name)
        {
            FontFamily ff = null;
            this.fontFamilies.TryGetValue(name, out ff);
            if (ff == null)
            {
                ff = new FontFamily(name);
                this.fontFamilies[name] = ff;
            }
            return ff;
        }

        private Dictionary<string, List<Typeface>> typefaces = new();

        public Typeface GetTypeface(string name)
        {
            return GetTypeface(name, FontWeight.Regular, FontStyle.Normal, FontStretch.Normal);
        }

        public Typeface GetTypeface(string name, FontWeight weight, FontStyle style, FontStretch stretch)
        {
            if (!typefaces.TryGetValue(name, out var list))
            {
                this.typefaces[name] = list = new();
            }

            var typeface = list.FirstOrDefault(t => t.Weight == weight && t.Style == style);
            if (typeface == default)
            {
                list.Add(typeface = new Typeface(new FontFamily(name), style, weight, stretch));
            }

            return typeface;
        }
#else // AVALONIA


        private Dictionary<string, List<WFFont>> fonts = new Dictionary<string, List<WFFont>>();

#if WINFORM
		public WFFont GetFont(string familyName, float emSize, WFFontStyle wfs)
		{

#elif WPF
		public WFFont GetFont(string familyName, double emSizeD, WFFontStyle wfs)
		{
			float emSize = (float)emSizeD;
#endif // WPF

#if DEBUG
			Stopwatch sw = Stopwatch.StartNew();
#endif // DEBUG

			if (string.IsNullOrEmpty(familyName))
			{
				familyName = System.Drawing.SystemFonts.DefaultFont.FontFamily.Name;
			}

			WFFont font = null;
			List<WFFont> fontGroup = null;
			System.Drawing.FontFamily family = null;

			lock (this.fonts)
			{
				if (this.fonts.TryGetValue(familyName, out fontGroup))
				{
					if (fontGroup.Count > 0)
					{
						family = fontGroup[0].FontFamily;
					}

					lock (fontGroup)
					{
						font = fontGroup.FirstOrDefault(f => f.Size == emSize && f.Style == wfs);
					}
				}
			}

			if (font != null) return font;

			if (family == null)
			{
				try
				{
					family = new System.Drawing.FontFamily(familyName);
				}
				catch (ArgumentException ex)
				{
					//throw new FontNotFoundException(ex.ParamName);
					family = System.Drawing.SystemFonts.DefaultFont.FontFamily;
					Logger.Log("resource pool", "font family error: " + familyName + ": " + ex.Message);
				}

				if (!family.IsStyleAvailable(wfs))
				{
					try
					{
						wfs = FindFirstAvailableFontStyle(family);
					}
					catch
					{
						return System.Drawing.SystemFonts.DefaultFont;
					}
				}
			}

			lock (this.fonts)
			{
				if (fonts.TryGetValue(family.Name, out fontGroup))
				{
					lock (fontGroup)
					{
						font = fontGroup.FirstOrDefault(f => f.Size == emSize && f.Style == wfs);
					}
				}
			}

			if (font == null)
			{
				font = new WFFont(family, emSize, wfs);

				if (fontGroup == null)
				{
					lock (this.fonts)
					{
						fonts.Add(family.Name, fontGroup = new List<WFFont> { font });
					}
					Logger.Log("resource pool", "font resource group added. font groups: " + fonts.Count);
				}
				else
				{
					lock (fontGroup)
					{
						fontGroup.Add(font);
					}
					Logger.Log("resource pool", "font resource added. fonts: " + fontGroup.Count);
				}

			}

#if DEBUG
			sw.Stop();
			long ms = sw.ElapsedMilliseconds;
			if (ms > 10)
			{
				Debug.WriteLine("resource pool: font scan: " + sw.ElapsedMilliseconds + " ms.");
			}
#endif // DEBUG
			return font;
		}

		private static WFFontStyle FindFirstAvailableFontStyle(System.Drawing.FontFamily ff)
		{
			if (ff.IsStyleAvailable(WFFontStyle.Regular))
				return WFFontStyle.Regular;
			else if (ff.IsStyleAvailable(WFFontStyle.Bold))
				return WFFontStyle.Bold;
			else if (ff.IsStyleAvailable(WFFontStyle.Italic))
				return WFFontStyle.Italic;
			else if (ff.IsStyleAvailable(WFFontStyle.Strikeout))
				return WFFontStyle.Strikeout;
			else if (ff.IsStyleAvailable(WFFontStyle.Underline))
				return WFFontStyle.Underline;
			else
			{
				Logger.Log("resource pool", "no available font style found: " + ff.Name);
				throw new NoAvailableFontStyleException();
			}
		}

		internal class NoAvailableFontStyleException : Exception
		{
		}

#if WPF

		private Dictionary<string, System.Windows.Media.FontFamily> fontFamilies
			= new Dictionary<string, System.Windows.Media.FontFamily>();

		public System.Windows.Media.FontFamily GetFontFamily(string name)
		{
			System.Windows.Media.FontFamily ff = null;
			this.fontFamilies.TryGetValue(name, out ff);
			if (ff == null)
			{
				ff = new System.Windows.Media.FontFamily(name);
				this.fontFamilies[name] = ff;
			}
			return ff;
		}

		private Dictionary<string, List<System.Windows.Media.Typeface>> typefaces 
			= new Dictionary<string, List<System.Windows.Media.Typeface>>();

		public System.Windows.Media.Typeface GetTypeface(string name)
		{
			return GetTypeface(name, System.Windows.FontWeights.Regular, System.Windows.FontStyles.Normal,
				System.Windows.FontStretches.Normal);
		}

		public System.Windows.Media.Typeface GetTypeface(string name, System.Windows.FontWeight weight, 
			System.Windows.FontStyle style, System.Windows.FontStretch stretch)
		{
			List<System.Windows.Media.Typeface> list;

			if (!typefaces.TryGetValue(name, out list))
			{
				this.typefaces[name] = list = new List<System.Windows.Media.Typeface>();
			}

			var typeface = list.FirstOrDefault(t=>t.Weight == weight && t.Style == style);
			if (typeface == null)
			{
				list.Add(typeface = new System.Windows.Media.Typeface(new System.Windows.Media.FontFamily(name), style, weight, stretch));
			}

			return typeface;
		}
#endif // WPF

#endif

        #endregion // Font

        #region Image
#if WINFORM && IMAGE_POOL
		private Dictionary<Guid, ImageResource> images 
			= new Dictionary<Guid, ImageResource>();
		public ImageResource GetImageResource(Guid id)
		{
			return images.Values.FirstOrDefault(i => i.ResId.Equals(id));
		}
		public ImageResource GetImage(string fullPath)
		{
			ImageResource res = images.Values.FirstOrDefault(
				i => i.FullPath != null &&
					i.FullPath.ToLower().Equals(fullPath.ToLower()));
			if (res != null)
			{
				if (res.Image != null) res.Image.Dispose();
				res.Image = Image.FromFile(fullPath);
				return res;
			}
			else
			{
				Image image;
				try
				{
					image = Image.FromFile(fullPath);
				}
				catch(Exception ex) {
					Logger.Log("resource pool", "add image file failed: " + ex.Message);
					return null;
				}

				return AddImage(Guid.NewGuid(), image, fullPath);
			}
		}
		public ImageResource AddImage(Guid id, Image image, string fullPath)
		{
			ImageResource res;

			if (!images.TryGetValue(id, out res))
			{
				images.Add(id, res = new ImageResource()
				{
					ResId = id,
					FullPath = fullPath,
				});

				Logger.Log("resource pool", "image added. count: " + images.Count);
			}

			if (res.Image != null)
			{
				res.Image.Dispose();
			}

			res.Image = image;

			return res;
		}
#endif
        #endregion

        #region Graphics
#if !AVALONIA

		private static System.Drawing.Bitmap bitmapForCachedGDIGraphics;
		private static WFGraphics cachedGDIGraphics;
		public static WFGraphics CachedGDIGraphics
		{
			get
			{
				if (cachedGDIGraphics == null)
				{
					bitmapForCachedGDIGraphics = new System.Drawing.Bitmap(1, 1);
					cachedGDIGraphics = WFGraphics.FromImage(bitmapForCachedGDIGraphics);
				}

				return cachedGDIGraphics;
			}
		}
#endif
        #endregion // Graphics

        #region FormattedText

        #endregion // FormattedText

        internal void ReleaseAllResources()
        {
            Logger.Log("resource pool", "release all resources...");

            int count =
                cachedPens.Count +

#if WINFORM
				hatchBrushes.Count + fonts.Values.Sum(f => f.Count) +
#endif
                /*images.Count +*/ cachedBrushes.Count
#if WPF
				+ typefaces.Sum(t=>t.Value.Count)
#endif
                ;

            // pens
            foreach (var plist in cachedPens.Values)
            {
#if WINFORM
				foreach (var p in plist) p.Dispose();
#endif // WINFORM
                plist.Clear();
            }

            cachedPens.Clear();

#if WINFORM
			// fonts
			foreach (var fl in fonts.Values)
			{
				foreach (var f in fl)
				{
					f.FontFamily.Dispose();
					f.Dispose();
				}
				fl.Clear();
			}

			fonts.Clear();

			foreach (var hb in this.hatchBrushes.Values)
			{
				hb.Dispose();
			}

			hatchBrushes.Clear();

			foreach (var sb in this.cachedBrushes.Values)
			{
				sb.Dispose();
			}
#elif WPF
			foreach (var list in typefaces)
			{
				list.Value.Clear();
			}
#endif // WPF

            cachedBrushes.Clear();

#if WINFORM

			//if (cachedGDIGraphics != null) cachedGDIGraphics.Dispose();
			//if (bitmapForCachedGDIGraphics != null) bitmapForCachedGDIGraphics.Dispose();
#endif // WINFORM

            Logger.Log("resource pool", count + " objects released.");
        }

        public void Dispose()
        {
            ReleaseAllResources();
        }
    }
}


