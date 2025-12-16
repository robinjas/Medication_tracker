#!/bin/bash

# FMMS Deployment Script for Linux/macOS
# This script automates the build and deployment process for the FMMS container
# Usage: ./deploy.sh [build|push|deploy|all]

set -e

# Configuration
REGISTRY="${REGISTRY:-docker.io}"
IMAGE_NAME="${IMAGE_NAME:-fmms-landing-page}"
TAG="${TAG:-latest}"
FULL_IMAGE_NAME="$REGISTRY/$IMAGE_NAME:$TAG"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

print_step() {
    echo -e "\n${CYAN}========================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}========================================${NC}\n"
}

build_image() {
    print_step "Building Docker Image"
    
    # Navigate to project root
    cd "$(dirname "$0")"
    
    echo -e "${YELLOW}Building image: $FULL_IMAGE_NAME${NC}"
    docker build -t "$FULL_IMAGE_NAME" -t "$IMAGE_NAME:latest" .
    
    echo -e "${GREEN}Build successful!${NC}"
}

push_image() {
    print_step "Pushing Docker Image to Registry"
    
    echo -e "${YELLOW}Pushing image: $FULL_IMAGE_NAME${NC}"
    docker push "$FULL_IMAGE_NAME"
    
    echo -e "${GREEN}Push successful!${NC}"
}

deploy_container() {
    print_step "Deploying Container Locally"
    
    # Stop existing container if running
    if docker ps -aq -f name=fmms-landing-page | grep -q .; then
        echo -e "${YELLOW}Stopping existing container...${NC}"
        docker stop fmms-landing-page 2>/dev/null || true
        docker rm fmms-landing-page 2>/dev/null || true
    fi
    
    # Run new container
    echo -e "${YELLOW}Starting new container...${NC}"
    docker run -d \
        --name fmms-landing-page \
        -p 8080:80 \
        --restart unless-stopped \
        "$FULL_IMAGE_NAME"
    
    echo -e "\n${GREEN}Deployment successful!${NC}"
    echo -e "${GREEN}Application available at: http://localhost:8080${NC}"
}

show_help() {
    cat << EOF

FMMS Deployment Script
======================

Usage: ./deploy.sh [command]

Commands:
    build       Build the Docker image
    push        Push the image to container registry
    deploy      Deploy the container locally
    all         Run all steps (build, push, deploy)
    help        Show this help message

Environment Variables:
    REGISTRY    Container registry (default: docker.io)
    IMAGE_NAME  Image name (default: fmms-landing-page)
    TAG         Image tag (default: latest)

Examples:
    ./deploy.sh build                     # Build image only
    ./deploy.sh deploy                    # Deploy locally
    ./deploy.sh all                       # Full deployment pipeline
    REGISTRY=myregistry ./deploy.sh all   # Deploy to custom registry

EOF
}

# Main execution
case "${1:-help}" in
    build)
        build_image
        ;;
    push)
        push_image
        ;;
    deploy)
        deploy_container
        ;;
    all)
        build_image
        push_image
        deploy_container
        print_step "Deployment Complete"
        echo -e "${GREEN}Image: $FULL_IMAGE_NAME${NC}"
        ;;
    help|*)
        show_help
        ;;
esac

