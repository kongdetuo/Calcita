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

namespace Calcita.Views
{
    /// <summary>
    /// Interface for freezable ViewportController
    /// </summary>
    internal interface IFreezableViewportController
    {
        /// <summary>
        /// Freeze to specified cell and position.
        /// </summary>
        /// <param name="pos">Position of cell to start freeze.</param>
        /// <param name="area">Decides the frozen view area.</param>
        void Freeze(CellPosition pos, FreezeArea area = Calcita.FreezeArea.LeftTop);
    }
}





