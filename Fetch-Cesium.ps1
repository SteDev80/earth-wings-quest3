$ErrorActionPreference = 'Stop'
$packagePath = Join-Path $PSScriptRoot 'Packages/com.cesium.unity-1.25.1.tgz'
if (-not (Test-Path -LiteralPath $packagePath)) {
    Invoke-WebRequest 'https://github.com/CesiumGS/cesium-unity/releases/download/v1.25.1/com.cesium.unity-1.25.1.tgz' -OutFile $packagePath
}
Write-Output "Cesium package available: $packagePath"
