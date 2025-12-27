# Get all remote tags except v0.8
$remoteTags = git ls-remote --tags origin | ForEach-Object {
    if ($_ -match 'refs/tags/(.+)$') {
        $matches[1] -replace '\^\{\}', ''
    }
} | Select-Object -Unique | Where-Object { $_ -ne "v0.8" }

# Delete each remote tag
foreach ($tag in $remoteTags) {
    Write-Host "🗑️ Deleting remote tag: $tag" -ForegroundColor Yellow
    git push origin --delete $tag
}

Write-Host "✅ Remote tags deleted from GitHub (kept v0.8)" -ForegroundColor Green