#if AVALONIA
global using RGFloat = System.Double;
global using RGPoint = Avalonia.Point;
global using RGPointF = Avalonia.Point;
global using IntOrDouble = System.Double;
global using RGIntDouble = System.Double;
global using RGRect = Avalonia.Rect;
global using RGColor = Avalonia.Media.Color;
global using RGBrush = Avalonia.Media.SolidColorBrush;
global using RGPen = Avalonia.Media.Pen;
global using PlatformGraphics = Avalonia.Media.DrawingContext;
global using RGPath = Avalonia.Media.Geometry;
global using RGImage = Avalonia.Media.Imaging.Bitmap;
global using RGTransform = Avalonia.Matrix;
global using RGSizeF = Avalonia.Size;
global using CellArray = Calcita.Data.Index4DArray<Calcita.Cell>;
global using HBorderArray = Calcita.Data.Index4DArray<Calcita.Core.ReoGridHBorder>;
global using VBorderArray = Calcita.Data.Index4DArray<Calcita.Core.ReoGridVBorder>;
global using RGFont = Avalonia.Media.Typeface;
global using RGPenColor = Avalonia.Media.Brushes;
global using RGSolidBrush = Avalonia.Media.SolidColorBrush;
global using RGBrushes = Avalonia.Media.Brushes;
global using RGDashStyle = Avalonia.Media.IDashStyle;

global using RGDashStyles = Calcita.AvaloniaPlatform.DashStyles;
global using SystemColors = Calcita.AvaloniaPlatform.SystemColors;

global using Cursor = Avalonia.Input.Cursor;
#endif