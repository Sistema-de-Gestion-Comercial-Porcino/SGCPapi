$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')
$proyecto = 'src/SGCP.Api'
$cadena = Read-Host 'Cadena MySQL (servidor, puerto, base sgcp, usuario y contraseña)' -MaskInput
$clave = Read-Host 'Contraseña inicial de admin (mínimo 10 caracteres)' -MaskInput
if ($clave.Length -lt 10) { throw 'La contraseña debe tener al menos 10 caracteres.' }
$bytes = New-Object byte[] 48
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
$config = @{
    'ConnectionStrings:Sgcp' = $cadena
    'Jwt:Clave' = [Convert]::ToBase64String($bytes)
    'DatosIniciales:ClaveAdministrador' = $clave
    'DatosIniciales:Ejemplos' = 'true'
}
$config | ConvertTo-Json | dotnet user-secrets set --project $proyecto
if ($LASTEXITCODE -ne 0) { throw 'No se pudo guardar la configuración local.' }
dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'No se pudo restaurar dotnet-ef.' }
dotnet ef database update --project $proyecto --context SgcpContext
if ($LASTEXITCODE -ne 0) { throw 'Falló la migración. Revisa servidor, credenciales y permisos.' }
Write-Host 'Base preparada. Ejecuta: dotnet run --project src/SGCP.Api'
