<#
.SYNOPSIS
    Writes versionCode and versionName into the Android manifest.

.DESCRIPTION
    Classic Xamarin.Android reads both values only from Properties/AndroidManifest.xml (there are no
    ApplicationVersion/ApplicationDisplayVersion properties as in .NET MAUI). The CI and release
    workflows call this script before building; locally it is not needed.

.EXAMPLE
    ./scripts/Set-AndroidVersion.ps1 -VersionCode 42 -VersionName 2.1.0
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [ValidateRange(1, 2100000000)] [int] $VersionCode,
    [Parameter(Mandatory)] [ValidatePattern('^\d+\.\d+\.\d+([-+][0-9A-Za-z.-]+)?$')] [string] $VersionName
)

$ErrorActionPreference = 'Stop'
$manifest = Join-Path (Split-Path -Parent $PSScriptRoot) 'src/Waldmann.Android/Properties/AndroidManifest.xml'

$content = [IO.File]::ReadAllText($manifest)
$content = $content -replace 'android:versionCode="[^"]*"', "android:versionCode=`"$VersionCode`""
$content = $content -replace 'android:versionName="[^"]*"', "android:versionName=`"$VersionName`""
[IO.File]::WriteAllText($manifest, $content)

Write-Host "Android version set to $VersionName ($VersionCode)."
