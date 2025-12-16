# FMMS Deployment Script for Windows PowerShell
# This script automates the build and deployment process for the FMMS container
# Usage: .\deploy.ps1 [-Build] [-Push] [-Deploy] [-All]

param(
    [switch]$Build,
    [switch]$Push,
    [switch]$Deploy,
    [switch]$All,
    [string]$Registry = "docker.io",
    [string]$ImageName = "fmms-landing-page",
    [string]$Tag = "latest"
)

# Configuration
$ErrorActionPreference = "Stop"
$FullImageName = "$Registry/$ImageName`:$Tag"

function Write-Step {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Build-Image {
    Write-Step "Building Docker Image"
    
    # Ensure we're in the project directory
    $ProjectRoot = Split-Path -Parent $MyInvocation.PSCommandPath
    Set-Location $ProjectRoot
    
    Write-Host "Building image: $FullImageName" -ForegroundColor Yellow
    docker build -t $FullImageName -t "${ImageName}:latest" .
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build successful!" -ForegroundColor Green
    } else {
        Write-Host "Build failed!" -ForegroundColor Red
        exit 1
    }
}

function Push-Image {
    Write-Step "Pushing Docker Image to Registry"
    
    Write-Host "Pushing image: $FullImageName" -ForegroundColor Yellow
    docker push $FullImageName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Push successful!" -ForegroundColor Green
    } else {
        Write-Host "Push failed!" -ForegroundColor Red
        exit 1
    }
}

function Deploy-Container {
    Write-Step "Deploying Container Locally"
    
    # Stop existing container if running
    $existingContainer = docker ps -aq -f name=fmms-landing-page
    if ($existingContainer) {
        Write-Host "Stopping existing container..." -ForegroundColor Yellow
        docker stop fmms-landing-page 2>$null
        docker rm fmms-landing-page 2>$null
    }
    
    # Run new container
    Write-Host "Starting new container..." -ForegroundColor Yellow
    docker run -d --name fmms-landing-page -p 8080:80 --restart unless-stopped $FullImageName
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nDeployment successful!" -ForegroundColor Green
        Write-Host "Application available at: http://localhost:8080" -ForegroundColor Green
    } else {
        Write-Host "Deployment failed!" -ForegroundColor Red
        exit 1
    }
}

function Show-Help {
    Write-Host @"

FMMS Deployment Script
======================

Usage: .\deploy.ps1 [-Build] [-Push] [-Deploy] [-All]

Options:
    -Build      Build the Docker image
    -Push       Push the image to container registry
    -Deploy     Deploy the container locally
    -All        Run all steps (Build, Push, Deploy)
    -Registry   Container registry (default: docker.io)
    -ImageName  Image name (default: fmms-landing-page)
    -Tag        Image tag (default: latest)

Examples:
    .\deploy.ps1 -Build                    # Build image only
    .\deploy.ps1 -Build -Deploy            # Build and deploy locally
    .\deploy.ps1 -All                      # Full deployment pipeline
    .\deploy.ps1 -All -Registry myregistry # Deploy to custom registry

"@
}

# Main execution
if (-not ($Build -or $Push -or $Deploy -or $All)) {
    Show-Help
    exit 0
}

Write-Host "`nFMMS Deployment Script" -ForegroundColor Magenta
Write-Host "======================" -ForegroundColor Magenta

if ($All -or $Build) {
    Build-Image
}

if ($All -or $Push) {
    Push-Image
}

if ($All -or $Deploy) {
    Deploy-Container
}

Write-Step "Deployment Complete"
Write-Host "Image: $FullImageName" -ForegroundColor Green

