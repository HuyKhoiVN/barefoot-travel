# ============================================
# Setup Script for 3D Globe Libraries
# ============================================
# This script downloads all necessary libraries
# and data files for the 3D Globe implementation
# ============================================

Write-Host "Setting up 3D Globe Libraries..." -ForegroundColor Cyan
Write-Host ""

# Base paths
$baseDir = $PSScriptRoot
$wwwrootDir = Join-Path $baseDir "wwwroot\ui-user-template"

# Create directory structure
Write-Host "Creating directory structure..." -ForegroundColor Yellow

$directories = @(
    "libs\three",
    "libs\globe-gl",
    "libs\topojson",
    "globe-data",
    "textures"
)

foreach ($dir in $directories) {
    $fullPath = Join-Path $wwwrootDir $dir
    if (-not (Test-Path $fullPath)) {
        New-Item -ItemType Directory -Force -Path $fullPath | Out-Null
        Write-Host "  [+] Created: $dir" -ForegroundColor Green
    } else {
        Write-Host "  [i] Exists: $dir" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "Downloading libraries..." -ForegroundColor Yellow

# Download Three.js
$threeJsUrl = "https://cdn.jsdelivr.net/npm/three@0.158.0/build/three.min.js"
$threeJsPath = Join-Path $wwwrootDir "libs\three\three.min.js"

Write-Host "  Downloading Three.js (v0.158.0)..." -ForegroundColor White
try {
    Invoke-WebRequest -Uri $threeJsUrl -OutFile $threeJsPath -UseBasicParsing
    $fileSize = [math]::Round((Get-Item $threeJsPath).Length / 1KB, 2)
    Write-Host "  [OK] Three.js downloaded successfully ($fileSize KB)" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to download Three.js: $_" -ForegroundColor Red
}

# Download Globe.gl
$globeGlUrl = "https://unpkg.com/globe.gl@2.31.0/dist/globe.gl.min.js"
$globeGlPath = Join-Path $wwwrootDir "libs\globe-gl\globe.gl.min.js"

Write-Host "  Downloading Globe.gl (v2.31.0)..." -ForegroundColor White
try {
    Invoke-WebRequest -Uri $globeGlUrl -OutFile $globeGlPath -UseBasicParsing
    $fileSize = [math]::Round((Get-Item $globeGlPath).Length / 1KB, 2)
    Write-Host "  [OK] Globe.gl downloaded successfully ($fileSize KB)" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to download Globe.gl: $_" -ForegroundColor Red
}

# Download Topojson Client
$topojsonUrl = "https://unpkg.com/topojson-client@3.1.0/dist/topojson-client.min.js"
$topojsonPath = Join-Path $wwwrootDir "libs\topojson\topojson-client.min.js"

Write-Host "  Downloading Topojson Client (v3.1.0)..." -ForegroundColor White
try {
    Invoke-WebRequest -Uri $topojsonUrl -OutFile $topojsonPath -UseBasicParsing
    $fileSize = [math]::Round((Get-Item $topojsonPath).Length / 1KB, 2)
    Write-Host "  [OK] Topojson Client downloaded successfully ($fileSize KB)" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to download Topojson Client: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Downloading map data..." -ForegroundColor Yellow

# Download Earth Topology
$earthTopoUrl = "https://cdn.jsdelivr.net/npm/world-atlas@2/countries-110m.json"
$earthTopoPath = Join-Path $wwwrootDir "globe-data\earth-topology.json"

Write-Host "  Downloading World Atlas data..." -ForegroundColor White
try {
    Invoke-WebRequest -Uri $earthTopoUrl -OutFile $earthTopoPath -UseBasicParsing
    $fileSize = [math]::Round((Get-Item $earthTopoPath).Length / 1KB, 2)
    Write-Host "  [OK] Earth topology downloaded successfully ($fileSize KB)" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to download Earth topology: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Downloading textures (optional)..." -ForegroundColor Yellow

# Download Earth Blue Marble texture
$earthTextureUrl = "https://unpkg.com/three-globe@2.30.0/example/img/earth-blue-marble.jpg"
$earthTexturePath = Join-Path $wwwrootDir "textures\earth-blue-marble.jpg"

Write-Host "  Downloading Earth texture..." -ForegroundColor White
try {
    Invoke-WebRequest -Uri $earthTextureUrl -OutFile $earthTexturePath -UseBasicParsing
    $fileSize = [math]::Round((Get-Item $earthTexturePath).Length / 1KB, 2)
    Write-Host "  [OK] Earth texture downloaded successfully ($fileSize KB)" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to download Earth texture: $_" -ForegroundColor Red
}

# Download Earth Topology/Bump texture
$earthBumpUrl = "https://unpkg.com/three-globe@2.30.0/example/img/earth-topology.png"
$earthBumpPath = Join-Path $wwwrootDir "textures\earth-topology.png"

Write-Host "  Downloading Earth bump map..." -ForegroundColor White
try {
    Invoke-WebRequest -Uri $earthBumpUrl -OutFile $earthBumpPath -UseBasicParsing
    $fileSize = [math]::Round((Get-Item $earthBumpPath).Length / 1KB, 2)
    Write-Host "  [OK] Earth bump map downloaded successfully ($fileSize KB)" -ForegroundColor Green
} catch {
    Write-Host "  [ERROR] Failed to download Earth bump map: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Creating Vietnam cities data..." -ForegroundColor Yellow

# Create Vietnam cities JSON
$vietnamCitiesJson = @"
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "properties": {
        "name": "Ha Noi",
        "name_en": "Hanoi",
        "type": "capital",
        "population": 8000000,
        "color": "#ff4757"
      },
      "geometry": {
        "type": "Point",
        "coordinates": [105.8342, 21.0278]
      }
    },
    {
      "type": "Feature",
      "properties": {
        "name": "Da Nang",
        "name_en": "Da Nang",
        "type": "city",
        "population": 1200000,
        "color": "#ffa502"
      },
      "geometry": {
        "type": "Point",
        "coordinates": [108.2022, 16.0544]
      }
    },
    {
      "type": "Feature",
      "properties": {
        "name": "TP Ho Chi Minh",
        "name_en": "Ho Chi Minh City",
        "type": "metropolis",
        "population": 9000000,
        "color": "#ff6348"
      },
      "geometry": {
        "type": "Point",
        "coordinates": [106.6297, 10.8231]
      }
    }
  ]
}
"@

$vietnamCitiesPath = Join-Path $wwwrootDir "globe-data\vietnam-cities.json"
$vietnamCitiesJson | Out-File -FilePath $vietnamCitiesPath -Encoding UTF8
Write-Host "  [OK] Vietnam cities data created" -ForegroundColor Green

# Create Vietnam routes JSON
$vietnamRoutesJson = @"
{
  "type": "FeatureCollection",
  "features": [
    {
      "type": "Feature",
      "properties": {
        "from": "Ha Noi",
        "to": "Da Nang",
        "color": "#00d2ff",
        "stroke_width": 2
      },
      "geometry": {
        "type": "LineString",
        "coordinates": [
          [105.8342, 21.0278],
          [108.2022, 16.0544]
        ]
      }
    },
    {
      "type": "Feature",
      "properties": {
        "from": "Da Nang",
        "to": "TP Ho Chi Minh",
        "color": "#00d2ff",
        "stroke_width": 2
      },
      "geometry": {
        "type": "LineString",
        "coordinates": [
          [108.2022, 16.0544],
          [106.6297, 10.8231]
        ]
      }
    },
    {
      "type": "Feature",
      "properties": {
        "from": "Ha Noi",
        "to": "TP Ho Chi Minh",
        "color": "#00d2ff",
        "stroke_width": 2
      },
      "geometry": {
        "type": "LineString",
        "coordinates": [
          [105.8342, 21.0278],
          [106.6297, 10.8231]
        ]
      }
    }
  ]
}
"@

$vietnamRoutesPath = Join-Path $wwwrootDir "globe-data\vietnam-routes.json"
$vietnamRoutesJson | Out-File -FilePath $vietnamRoutesPath -Encoding UTF8
Write-Host "  [OK] Vietnam routes data created" -ForegroundColor Green

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Setup completed successfully!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  - Three.js library" -ForegroundColor White
Write-Host "  - Globe.gl library" -ForegroundColor White
Write-Host "  - Topojson client" -ForegroundColor White
Write-Host "  - World map data" -ForegroundColor White
Write-Host "  - Earth textures" -ForegroundColor White
Write-Host "  - Vietnam cities data" -ForegroundColor White
Write-Host "  - Vietnam routes data" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Read: Document\3D-Globe-Hero-Implementation.md" -ForegroundColor White
Write-Host "  2. Create: wwwroot\ui-user-template\globe-hero.css" -ForegroundColor White
Write-Host "  3. Create: wwwroot\ui-user-template\globe-hero.js" -ForegroundColor White
Write-Host "  4. Update: Views\Home\Index.cshtml" -ForegroundColor White
Write-Host "  5. Test: Run the application and verify" -ForegroundColor White
Write-Host ""
Write-Host "Happy coding!" -ForegroundColor Cyan
