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

using Calcita.Graphics;
using Calcita.Main;

namespace Calcita.Views
{
	interface IViewport : IView
	{
		Point ViewStart { get; set; }
		RGFloat ViewTop { get; }
		RGFloat ViewLeft { get; }
		//RGFloat ViewRight { get; }
		//RGFloat ViewBottom { get; }

		RGFloat ScrollX { get; set; }
		RGFloat ScrollY { get; set; }
		RGFloat ScrollViewTop { get; }
		RGFloat ScrollViewLeft { get; }

		ScrollDirection ScrollableDirections { get; set; }
		void Scroll(RGFloat offX, RGFloat offY);

		void ScrollTo(RGFloat x, RGFloat y);

		GridRegion VisibleRegion { get; set; }
	}
}





