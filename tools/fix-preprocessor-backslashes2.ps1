# Replace literal backslash sequences "\r\n" that appear after comment terminator and before preprocessor directives
$files = Get-ChildItem -Path . -Recurse -Include *.cs -File
foreach ($f in $files) {
    $text = Get-Content -Raw -Path $f.FullName -ErrorAction SilentlyContinue
    if (-not $text) { continue }

    $orig = $text
    # Replace patterns like '*/\r\n#if' or '*/\r\n#' with actual newline before '#'
    $text = [regex]::Replace($text, '\*/\\r\\n(?=#)', '*/' + [Environment]::NewLine)
    # Replace patterns like '*/#' directly following comment end
    $text = [regex]::Replace($text, '\*/(?=#)', '*/' + [Environment]::NewLine)
    # Also replace any '\r\n#' occurrences
    $text = [regex]::Replace($text, '\\r\\n(?=#)', [Environment]::NewLine)

    if ($text -ne $orig) {
        Set-Content -Path $f.FullName -Value $text -Encoding UTF8
        Write-Host "Fixed preprocessor newlines in: $($f.FullName)"
    }
}
Write-Host "Done."