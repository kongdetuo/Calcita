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
 ****************************************************************************/using Calcita.Graphics;

namespace Calcita.Rendering
{
    // Class define for multiple platform
    //
    // Don't add any platform-associated methods or properties into these classes,
    // methods and properties should be defined inside each platform files.
    //

    static partial class PlatformUtility
    {
    }

    static partial class StaticResources
    {
    }

    internal interface IRenderer : IGraphics
#if WINFORM
		, System.IDisposable
#endif // WINFORM
    {
        ControlAppearanceStyle ControlStyle { get; }

        void DrawRunningFocusRect(RGFloat x, RGFloat y, RGFloat w, RGFloat h, SolidColor color, int runningOffset);

        void BeginCappedLine(LineCapStyles startCap, Size startSize, LineCapStyles endCap, Size endSize, SolidColor color, RGFloat width);

        void DrawCappedLine(RGFloat x1, RGFloat y1, RGFloat x2, RGFloat y2);

        void EndCappedLine();

        void BeginDrawLine(IRGPen pen);

        void DrawLine(RGFloat x1, RGFloat y1, RGFloat x2, RGFloat y2);

        void EndDrawLine();

        void DrawCellText(Cell cell, DrawMode drawMode, RGFloat scale);

        void UpdateCellRenderFont(Cell cell, Core.UpdateFontReason reason);

        Size MeasureCellText(Cell cell, DrawMode drawMode, RGFloat scale);

        void BeginDrawHeaderText(RGFloat scale);

        void DrawHeaderText(string text, IRGBrush brush, Rectangle rect);

        void DrawLeadHeadArrow(Graphics.Rectangle bounds, IRGBrush brush);

        RGPen GetPen(SolidColor color);

        void ReleasePen(RGPen pen);

        RGBrush GetBrush(SolidColor color);

        Common.ResourcePoolManager ResourcePoolManager { get; }
    }

}


