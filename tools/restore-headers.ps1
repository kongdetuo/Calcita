# Restore original ReoGrid headers across .cs files
$orig = @'
/*****************************************************************************
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
'@

Write-Host "Scanning for .cs files..."
$files = Get-ChildItem -Path . -Recurse -Include *.cs -File

foreach ($f in $files) {
    $text = Get-Content -Raw -Path $f.FullName -ErrorAction SilentlyContinue
    if (-not $text) { continue }

    # find a leading comment block that starts with /* and ends with */
    $pattern = '(?ms)^\s*/\*{1,}.*?\*/\s*'
    $m = [regex]::Match($text, $pattern)
    if ($m.Success) {
        $block = $m.Value
        if ($block -match 'Calcita -|Calcita contributors|Original Author: Jingwood') {
            $newText = $orig + $text.Substring($m.Length)
            Set-Content -Path $f.FullName -Value $newText -Encoding UTF8
            Write-Host "Restored header in: $($f.FullName)"
        }
    }
}
Write-Host "Done."