$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
$env:SGCP_MYSQL_TEST = Read-Host 'Cadena de la instancia de pruebas MySQL (permiso CREATE DATABASE)' -MaskInput
try {
    dotnet run --project tests/SGCP.Verificacion
    if ($LASTEXITCODE -ne 0) { throw 'La verificación falló; revisar la salida.' }
} finally {
    Remove-Item Env:SGCP_MYSQL_TEST -ErrorAction SilentlyContinue
}
