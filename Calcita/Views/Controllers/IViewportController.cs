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
using Calcita.Rendering;
using Calcita.Interaction;
using Calcita.Main;

namespace Calcita.Views
{
	internal interface IViewportController : IUserVisual, IVisualController
	{
		Worksheet Worksheet { get; }

		Rectangle Bounds { get; set; }

		IView View { get; }
		IView FocusView { get; set; }

		void Draw(CellDrawingContext dc);

		void UpdateController();
		void Reset();

		void SetViewVisible(ViewTypes view, bool visible);
	}
}



