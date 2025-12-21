# Simple test script with 2 stop bits
$port = new-Object System.IO.Ports.SerialPort COM6,38400,None,8,Two
$port.Open()
Start-Sleep -Milliseconds 300
$port.Write("ID;")
Start-Sleep -Milliseconds 200
$response = $port.ReadExisting()
Write-Host "Response: $response"
$port.Close()