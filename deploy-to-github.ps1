# FMMS Deployment Script
# This script automates the deployment of the FMMS mobile app to GitHub Pages
# Usage: .\deploy-to-github.ps1

Write-Host "=== FMMS Deployment Script ===" -ForegroundColor Cyan

# Step 1: Build the APK
Write-Host "`n[1/4] Building Android APK..." -ForegroundColor Yellow
dotnet publish FMMS/FMMS.csproj -f net9.0-android -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Build successful!" -ForegroundColor Green

# Step 2: Copy APK to docs folder
Write-Host "`n[2/4] Copying APK to docs folder..." -ForegroundColor Yellow
Copy-Item "FMMS\bin\Release\net9.0-android\com.companyname.fmms-Signed.apk" "docs\FMMS.apk" -Force
Write-Host "APK copied!" -ForegroundColor Green

# Step 3: Stage and commit changes
Write-Host "`n[3/4] Committing changes..." -ForegroundColor Yellow
git add docs/
git commit -m "Deploy: Update APK $(Get-Date -Format 'yyyy-MM-dd HH:mm')"

# Step 4: Push to GitHub
Write-Host "`n[4/4] Pushing to GitHub..." -ForegroundColor Yellow
git push

Write-Host "`n=== Deployment Complete ===" -ForegroundColor Green
Write-Host "Your app will be live at: https://robinjas.github.io/Medication_tracker/" -ForegroundColor Cyan

