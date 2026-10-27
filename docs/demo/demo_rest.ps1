# DEMO WEB SERVICE: lay bang xep hang qua REST API cua Supabase
# Cach chay:  powershell -ExecutionPolicy Bypass -File docs\demo\demo_rest.ps1
# (Ghi chu: viet khong dau de PowerShell 5.1 doc file khong bi loi font)

$base = "https://hqhggiixuqhhpuydparb.supabase.co/rest/v1"
$key  = "sb_publishable__CEmQx8woYvxjvD0qUCeKg_4Gf8LMA3"   # publishable key: duoc phep cong khai
$hdr  = @{ apikey = $key }

Write-Host "`n[1] Goi web service lay Top 10 bang xep hang" -ForegroundColor Cyan
Write-Host "GET $base/leaderboard?select=*&order=Score.desc&limit=10"
$rows = Invoke-RestMethod -Uri "$base/leaderboard?select=*&order=Score.desc,Wins.desc&limit=10" -Headers $hdr

Write-Host "`n[2] Du lieu tra ve (bang)" -ForegroundColor Cyan
$rows | Format-Table -AutoSize

Write-Host "[3] Du lieu tra ve (JSON tho)" -ForegroundColor Cyan
$rows | ConvertTo-Json

Write-Host "`n[4] Thu doc thang bang goc 'Users' bang CUNG khoa cong khai (phai bi chan)" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$base/Users?select=*" -Headers $hdr | Out-Null
    Write-Host "!!! KHONG bi chan - phai kiem tra lai RLS !!!" -ForegroundColor Red
} catch {
    $code = [int]$_.Exception.Response.StatusCode
    Write-Host "Bi chan dung nhu mong doi: HTTP $code  (bang goc khoa RLS, chi view leaderboard doc duoc, khong co cot mat khau)" -ForegroundColor Green
}
