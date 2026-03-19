# 数据库备份脚本
# 用法：.\backup-db.ps1 或 dotnet run --project backup-db.ps1

$backupDir = "E:\code\vueShop\app-api\backups"
$dbPath = "E:\code\vueShop\app-api\app.db"

# 创建备份目录
if (-not (Test-Path $backupDir)) {
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "创建备份目录：$backupDir" -ForegroundColor Green
}

# 生成备份文件名（带时间戳）
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupFile = Join-Path $backupDir "app_$timestamp.db"

# 复制数据库文件
if (Test-Path $dbPath) {
    Copy-Item -Path $dbPath -Destination $backupFile -Force
    Write-Host "✓ 数据库已备份到：$backupFile" -ForegroundColor Green
    
    # 同时备份 -shm 和 -wal 文件（如果存在）
    if (Test-Path "$dbPath-shm") {
        Copy-Item -Path "$dbPath-shm" -Destination "$backupFile-shm" -Force
    }
    if (Test-Path "$dbPath-wal") {
        Copy-Item -Path "$dbPath-wal" -Destination "$backupFile-wal" -Force
    }
    
    # 清理 7 天前的备份
    $daysToKeep = 7
    $cutoffDate = (Get-Date).AddDays(-$daysToKeep)
    $oldBackups = Get-ChildItem -Path $backupDir -Filter "app_*.db" | Where-Object { $_.LastWriteTime -lt $cutoffDate }
    
    foreach ($oldBackup in $oldBackups) {
        Remove-Item -Path $oldBackup.FullName -Force
        Remove-Item -Path "$($oldBackup.FullName)-shm" -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "$($oldBackup.FullName)-wal" -Force -ErrorAction SilentlyContinue
        Write-Host "✓ 清理旧备份：$($oldBackup.Name)" -ForegroundColor Yellow
    }
    
    Write-Host "`n备份完成！保留最近 $daysToKeep 天的备份。" -ForegroundColor Green
} else {
    Write-Host "✗ 数据库文件不存在：$dbPath" -ForegroundColor Red
    exit 1
}
