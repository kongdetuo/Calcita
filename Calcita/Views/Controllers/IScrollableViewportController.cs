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
#define VP_DEBUG

using Calcita.Main;

namespace Calcita.Views
{
	internal interface IScrollableViewportController
	{
		void HorizontalScroll(RGIntDouble value);

		void VerticalScroll(RGIntDouble value);

		void ScrollViews(ScrollDirection dir, RGFloat x, RGFloat y);

		void ScrollOffsetViews(ScrollDirection dir, RGFloat x, RGFloat y);

		void ScrollToRange(RangePosition range, CellPosition pos);

		void SynchronizeScrollBar();
	}
}





