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
#if PRINT

#if WPF

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Calcita.Print;

namespace Calcita.Print
{
	partial class PrintSession
	{
		internal void Init() { }

		public void Dispose() { }

		/// <summary>
		/// Start output document to printer.
		/// </summary>
		public void Print()
		{
			throw new NotImplementedException("WPF Print is not implemented yet. Try use Windows Form version to print document as XPS file.");
		}
	}
}

namespace Calcita
{
	partial class Worksheet
	{
	}
}

#endif // WPF

#endif // PRINT



