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

#if DEBUG
using System.Diagnostics;
#endif

#if EX_SCRIPT
using Calcita.ReoScript;
using Calcita.Script;
#endif // EX_SCRIPT

#if WINFORM || ANDROID
using RGFloat = System.Single;
#elif !GLOBALUSING
using RGFloat = System.Double;
#endif // WINFORM

using Calcita.Common;

#if WINFORM || WPF
using Calcita.Common.Win32Lib;
#endif // WINFORM || WPF

using Calcita.Core;
using Calcita.Utility;
using Calcita.Rendering;
using Calcita.Events;
using Calcita.Actions;
using Calcita.Data;
using Calcita.Graphics;
using Calcita.Interaction;

namespace Calcita.Views
{
	interface IRangeSelectableView : IViewport
	{
	}
}


