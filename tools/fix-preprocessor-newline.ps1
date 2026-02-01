$files = Get-ChildItem -Path . -Recurse -Include *.cs -File
foreach ($f in $files) {
    $text = Get-Content -Raw -Path $f.FullName -ErrorAction SilentlyContinue
    if (-not $text) { continue }
    if ($text -match '\*/#') {
        $new = $text -replace '\*/#','*/\r\n#'
        Set-Content -Path $f.FullName -Value $new -Encoding UTF8
        Write-Host "Fixed: $($f.FullName)"
    }
}
Write-Host "Done."