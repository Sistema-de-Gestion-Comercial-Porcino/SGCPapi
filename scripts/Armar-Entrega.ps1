$ErrorActionPreference = 'Stop'
Set-Location (Join-Path $PSScriptRoot '..')

$salida = if ($args.Count -gt 0) { $args[0] } else { Join-Path (Get-Location) 'artifacts\SGCPapi-WebIII.zip' }
$directorio = Split-Path -Parent $salida
if ($directorio -and -not (Test-Path $directorio)) {
    New-Item -ItemType Directory -Path $directorio | Out-Null
}
if (Test-Path $salida) { Remove-Item $salida -Force }

$excluirCarpetas = @('bin', 'obj', '.vs', '.git', 'artifacts', 'TestResults')
$excluirArchivos = @('*.user', 'appsettings.Development.json', 'appsettings.Local.json', '.env')

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::Open($salida, [System.IO.Compression.ZipArchiveMode]::Create)
$raiz = (Get-Location).Path.TrimEnd('\')
$incluidos = 0

try {
    Get-ChildItem -Recurse -File -Force | ForEach-Object {
        $relativa = $_.FullName.Substring($raiz.Length + 1)
        $partes = $relativa -split '[\\/]'
        foreach ($carpeta in $excluirCarpetas) {
            if ($partes -contains $carpeta) { return }
        }
        foreach ($patron in $excluirArchivos) {
            if ($_.Name -like $patron) { return }
        }
        $entrada = $relativa -replace '\\', '/'
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $zip, $_.FullName, $entrada, [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
        $incluidos++
    }
}
finally {
    $zip.Dispose()
}

$nombres = [System.IO.Compression.ZipFile]::OpenRead($salida).Entries |
    ForEach-Object { $_.FullName }
$prohibidos = $nombres | Where-Object { $_ -match '(^|/)(bin|obj|\.vs)/' }
$obligatorios = @(
    'README.md',
    'src/SGCP.Api/Datos/MigracionesSqlServer/',
    'docs/Evidencias_API_Integrador.pdf',
    'pruebas/API_Integrador.http'
)
$faltan = @()
foreach ($ruta in $obligatorios) {
    $presente = $nombres | Where-Object { $_ -eq $ruta -or $_.StartsWith($ruta) }
    if (-not $presente) { $faltan += $ruta }
}

if ($prohibidos -or $faltan) {
    Remove-Item $salida -Force
    if ($prohibidos) { throw "El ZIP incluye carpetas prohibidas: $($prohibidos -join ', ')" }
    throw "El ZIP no incluye: $($faltan -join ', ')"
}

Write-Host "ZIP listo: $salida"
Write-Host "Archivos: $incluidos. Sin bin, obj ni .vs. Con README, migraciones de SQL Server, PDF y pruebas HTTP."
