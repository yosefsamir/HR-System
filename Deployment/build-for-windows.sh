#!/bin/bash
# Build HR System for Windows deployment (from Linux)

echo "========================================"
echo "   Building HR System for Windows"
echo "========================================"
echo ""

SCRIPT_DIR="$(dirname "$0")"
cd "$SCRIPT_DIR/../HR-system"

# Get version from csproj
VERSION=$(grep -oP '(?<=<Version>)[^<]+' HR-system.csproj 2>/dev/null || echo "1.0.0")
echo "Version: $VERSION"
echo ""

# Clean previous builds
echo "[1/5] Cleaning previous builds..."
dotnet clean -c Release > /dev/null 2>&1
rm -rf "../Deployment/publish" 2>/dev/null

# Restore packages
echo "[2/5] Restoring packages..."
dotnet restore

# Build for Windows x64 - SELF-CONTAINED with all dependencies
echo "[3/5] Building for Windows x64 (self-contained)..."
dotnet publish -c Release -r win-x64 --self-contained true -o "../Deployment/publish" -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true

# Copy deployment scripts
echo "[4/5] Copying deployment scripts..."
cp "../Deployment/StartHRSystem.bat" "../Deployment/publish/"
cp "../Deployment/StopHRSystem.bat" "../Deployment/publish/"
cp "../Deployment/UpdateHRSystem.bat" "../Deployment/publish/"
cp "../Deployment/Install.bat" "../Deployment/publish/"
cp "../Deployment/CheckForUpdates.bat" "../Deployment/publish/"
cp "../Deployment/INSTALLATION_GUIDE.md" "../Deployment/publish/"
cp "../Deployment/DownloadRequirements.bat" "../Deployment/publish/" 2>/dev/null

# Create version file
echo "$VERSION" > "../Deployment/publish/version.txt"

# Create required folders
mkdir -p "../Deployment/publish/Update"
mkdir -p "../Deployment/publish/Backup"

# Convert line endings for Windows batch files
echo "[5/5] Converting line endings for Windows..."
if command -v unix2dos &> /dev/null; then
    unix2dos "../Deployment/publish/"*.bat 2>/dev/null
else
    # Manual conversion if unix2dos not available
    for file in "../Deployment/publish/"*.bat; do
        sed -i 's/$/\r/' "$file" 2>/dev/null
    done
fi

# Create release zip for GitHub
echo "[6/6] Creating release zip for GitHub..."
cd "../Deployment"
rm -f "HRSystem-v${VERSION}.zip" 2>/dev/null
cd publish
zip -r "../HRSystem-v${VERSION}.zip" . > /dev/null 2>&1
cd ..

echo ""
echo "========================================"
echo "   Build completed successfully!"
echo "========================================"
echo ""
echo "Version: $VERSION"
echo "Output folder: Deployment/publish/"
echo "Release zip:   Deployment/HRSystem-v${VERSION}.zip"
echo ""
echo "This is a SELF-CONTAINED build."
echo "Client does NOT need to install .NET Runtime!"
echo ""
echo "========================================"
echo "   To release on GitHub:"
echo "========================================"
echo ""
echo "1. git add ."
echo "2. git commit -m 'Release v$VERSION'"
echo "3. git tag v$VERSION"
echo "4. git push origin main --tags"
echo "5. Go to: https://github.com/yosefsamir/HR-System/releases/new"
echo "6. Select tag: v$VERSION"
echo "7. Upload: HRSystem-v${VERSION}.zip"
echo "8. Click 'Publish release'"
echo ""
echo "3. Run Install.bat as Administrator"
echo ""
