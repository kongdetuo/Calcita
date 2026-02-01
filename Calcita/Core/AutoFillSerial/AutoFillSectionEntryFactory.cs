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
 ****************************************************************************/using Calcita.Utility;

namespace Calcita
{
  public static class AutoFillSectionEntryFactory
  {
    public static IAutoFillSectionEntry Create(object value)
    {
      if (value == null)
      {
        return new NullAutoFillSectionEntry();
      }

      if (CellUtility.TryGetNumberData(value, out var number))
      {
        return new NumericalAutoFillSectionEntry(number);
      }

      return new TextAutoFillSectionEntry(value.ToString());
    }
  }
}

