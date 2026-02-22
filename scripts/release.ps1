[CmdletBinding()]
param(
    [switch]$Push,
    [switch]$PublishToNuGet,
    [string]$NuGetSource = "https://api.nuget.org/v3/index.json",
    [string]$NuGetApiKey = $env:NUGET_API_KEY,
    [string]$RuntimeIdentifier,
    [switch]$SkipValidation,
    [switch]$SkipPack,
    [string]$DefaultInitialVersion = "0.1.0"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$Command
    )

    Write-Host "`n==> $Name" -ForegroundColor Cyan
    Write-Host "    $Command" -ForegroundColor DarkGray
    Invoke-Expression $Command
}

function Get-LatestSemVerTag {
    $rawTags = @(
        git tag --list "v*" |
        ForEach-Object { $_.Trim() } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    )

    if ($rawTags.Count -eq 0) {
        return $null
    }

    $parsed = foreach ($rawTag in $rawTags) {
        $normalizedTag = $rawTag -replace '^v\.', 'v'
        $m = [regex]::Match($normalizedTag, '^v(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)$')
        if (-not $m.Success) {
            continue
        }

        [pscustomobject]@{
            Tag = $normalizedTag
            Version = [version]::new([int]$m.Groups['major'].Value, [int]$m.Groups['minor'].Value, [int]$m.Groups['patch'].Value)
        }
    }

    if (@($parsed).Count -eq 0) {
        return $null
    }

    return ($parsed | Sort-Object Version -Descending | Select-Object -First 1).Tag
}

function Get-NextVersion {
    param([string]$FallbackVersion)

    $latestTag = Get-LatestSemVerTag
    if (-not $latestTag) {
        return $FallbackVersion
    }

    $latest = [version]($latestTag.TrimStart('v'))
    return "{0}.{1}.{2}" -f $latest.Major, $latest.Minor, ($latest.Build + 1)
}

function Assert-CleanWorkingTree {
    $status = git status --porcelain
    if ($status) {
        throw "Working tree is not clean. Commit or stash changes before running release automation."
    }
}

function Assert-MainBranch {
    $branch = (git rev-parse --abbrev-ref HEAD).Trim()
    if ($branch -ne "main") {
        throw "Release automation must run from 'main'. Current branch: '$branch'."
    }
}

Write-Host "NativeData automated release" -ForegroundColor Green

if ([string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
    if ($IsWindows) {
        $RuntimeIdentifier = "win-x64"
    }
    elseif ($IsLinux) {
        $RuntimeIdentifier = "linux-x64"
    }
    elseif ($IsMacOS) {
        $RuntimeIdentifier = "osx-x64"
    }
    else {
        throw "Could not determine default RuntimeIdentifier for this OS. Pass -RuntimeIdentifier explicitly."
    }
}

Write-Host "AOT runtime identifier: $RuntimeIdentifier" -ForegroundColor DarkYellow

Invoke-Step -Name "Fetch latest refs and tags" -Command "git fetch origin --tags"
Assert-MainBranch
Assert-CleanWorkingTree
Invoke-Step -Name "Fast-forward main" -Command "git pull --ff-only origin main"

$version = Get-NextVersion -FallbackVersion $DefaultInitialVersion

$tag = "v$version"

& git "show-ref" "--verify" "--quiet" "refs/tags/$tag" | Out-Null
if ($LASTEXITCODE -eq 0) {
    throw "Tag '$tag' already exists."
}

Write-Host "`nCalculated release version: $version" -ForegroundColor Yellow

if (-not $SkipValidation) {
    Invoke-Step -Name "Restore" -Command "dotnet restore NativeData.slnx"
    Invoke-Step -Name "Build" -Command "dotnet build NativeData.slnx -c Release -warnaserror --no-restore"
    Invoke-Step -Name "Test" -Command "dotnet test NativeData.slnx -c Release --no-build"
    Invoke-Step -Name "AOT smoke publish" -Command "dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r $RuntimeIdentifier -p:PublishAot=true -p:PublishTrimmed=true"
}

$artifactRoot = Join-Path -Path "artifacts" -ChildPath "release/$version"
if (-not $SkipPack) {
    New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
    Invoke-Step -Name "Pack" -Command "dotnet pack NativeData.slnx -c Release --no-build -p:Version=$version -o `"$artifactRoot`""
}

if ($PublishToNuGet) {
    if ([string]::IsNullOrWhiteSpace($NuGetApiKey)) {
        throw "NuGet API key missing. Set NUGET_API_KEY or pass -NuGetApiKey."
    }

    $packages = Get-ChildItem -Path $artifactRoot -Filter *.nupkg | Where-Object { $_.Name -notlike "*.snupkg" }
    if (-not $packages -or $packages.Count -eq 0) {
        throw "No .nupkg files found at '$artifactRoot'."
    }

    foreach ($package in $packages) {
        Invoke-Step -Name "Push package $($package.Name)" -Command "dotnet nuget push `"$($package.FullName)`" --source `"$NuGetSource`" --api-key `"$NuGetApiKey`" --skip-duplicate"
    }
}

Invoke-Step -Name "Create tag" -Command "git tag -a $tag -m `"Release $tag`""

if ($Push) {
    Invoke-Step -Name "Push main" -Command "git push origin main"
    Invoke-Step -Name "Push tag" -Command "git push origin $tag"
}

Write-Host "`nRelease automation complete." -ForegroundColor Green
Write-Host "Version: $version"
Write-Host "Tag: $tag"
Write-Host "Artifacts: $artifactRoot"
Write-Host "`nNext: create GitHub release notes from docs/release-notes-v$version.md (or template)." -ForegroundColor DarkYellow
