# KIEM TRA TRUOC KHI DEMO (chay 15 phut truoc gio bao cao)
# Cach chay:  powershell -ExecutionPolicy Bypass -File docs\demo\demo_check.ps1
# Ket qua cuoi cung la GO (demo live duoc) hoac NO-GO (dung video / SQLite).

$ok = $true
function Show($name, $pass, $detail) {
    if ($pass) { Write-Host ("[ OK ] " + $name + "  " + $detail) -ForegroundColor Green }
    else       { Write-Host ("[LOI ] " + $name + "  " + $detail) -ForegroundColor Red; $script:ok = $false }
}

# 1) Bien moi truong
$db = [Environment]::GetEnvironmentVariable('BATTLESHIP_DB', 'User')
$cs = [Environment]::GetEnvironmentVariable('SUPABASE_DB_CONNECTION', 'User')
Show "BATTLESHIP_DB = supabase" ($db -eq 'supabase') ("(dang la: '" + $db + "')")
Show "Co chuoi ket noi SUPABASE_DB_CONNECTION" (-not [string]::IsNullOrWhiteSpace($cs)) ""

# 2) May chu pooler cua Supabase co mo duoc cong 5432 khong (mang truong co chan khong)
if ($cs) {
    $hostLine = ($cs -split ';' | Where-Object { $_ -like 'Host=*' }) -replace 'Host=', ''
    $reach = $false
    try {
        $c = New-Object System.Net.Sockets.TcpClient
        $reach = $c.ConnectAsync($hostLine, 5432).Wait(5000) -and $c.Connected
        $c.Close()
    } catch { $reach = $false }
    Show ("Noi duoc toi Supabase DB (" + $hostLine + ":5432)") $reach ""
}

# 3) Web service REST (chinh la phan demo bang xep hang)
$url = "https://hqhggiixuqhhpuydparb.supabase.co/rest/v1/leaderboard?select=*&limit=1"
$key = "sb_publishable__CEmQx8woYvxjvD0qUCeKg_4Gf8LMA3"
try {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $r = Invoke-WebRequest -UseBasicParsing -TimeoutSec 8 -Uri $url -Headers @{ apikey = $key }
    $sw.Stop()
    Show "REST bang xep hang tra ve 200" ($r.StatusCode -eq 200) ("(" + $sw.ElapsedMilliseconds + " ms)")
    Show "REST phan hoi duoi 3 giay (du nhanh de demo live)" ($sw.ElapsedMilliseconds -lt 3000) ""
} catch {
    Show "REST bang xep hang" $false ("(" + $_.Exception.Message + ")")
}

# 4) Cong 8000 con trong khong (server cu chua tat se chiem cong)
$busy = (Get-NetTCPConnection -LocalPort 8000 -State Listen -ErrorAction SilentlyContinue) -ne $null
Show "Cong 8000 dang trong (chua co server cu chay)" (-not $busy) "(neu loi: tat Battleship.Server.exe cu)"

Write-Host ""
if ($ok) { Write-Host "==> GO: demo LIVE duoc." -ForegroundColor Green }
else     { Write-Host "==> NO-GO: bat video du phong, hoac dat BATTLESHIP_DB=sqlite de demo offline." -ForegroundColor Yellow }
