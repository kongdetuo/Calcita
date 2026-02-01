# Fix files where comment end '*/' is immediately followed by literal backslash sequences or '#' preprocessor
$files = Get-ChildItem -Path . -Recurse -Include *.cs -File
foreach ($f in $files) {
    $text = Get-Content -Raw -Path $f.FullName -ErrorAction SilentlyContinue
    if (-not $text) { continue }

    $orig = $text
    # replace literal '*/#' -> '*/\n#' (actual newline)
    $text = $text -replace '\*/#','*/' + [Environment]::NewLine + '#'
    # replace '*/\\r\\n#' (backslash r backslash n) -> actual newline
    $text = $text -replace '\*/\\r\\n#','*/' + [Environment]::NewLine + '#'
    # replace '*/\\r\\n' followed by any preprocessor (#) -> actual newline
    $text = [regex]::Replace($text, '\*/\\r\\n(?=#)', '*/' + [Environment]::NewLine)
    # replace occurrences of '*/\\r\\n' standalone
    $text = $text -replace '\*/\\r\\n','*/' + [Environment]::NewLine
    # replace '*/\r\n' (literal backslash then r then backslash n)
    $text = $text -replace '\*/\\r\\n','*/' + [Environment]::NewLine

    if ($text -ne $orig) {
        Set-Content -Path $f.FullName -Value $text -Encoding UTF8
        Write-Host "Fixed backslashes in: $($f.FullName)"
    }
}
Write-Host "Done."