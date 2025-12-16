# FMMS Deployment Documentation
## D424 Software Engineering Capstone - Task 4

**Student Name:** Richard Robinson  
**Application:** Family Medication Management System (FMMS)  
**Date:** December 2024

---

## A. Deployment to Cloud Service Provider

The FMMS mobile application landing page has been deployed to **[Render.com / Azure / Your Choice]** using Docker containerization.

**Deployed Application URL:** [Insert your deployed URL here]

### A1. Justification for Cloud Solution Provider Selection

I selected **[Render.com / Azure Container Instances / Google Cloud Run]** as my cloud service provider for the following reasons:

1. **Cost-Effectiveness**: [Render offers a free tier / Azure provides $200 free credits / etc.] that allows deploying containerized applications at no cost for development and demonstration purposes.

2. **Docker Support**: The platform provides native support for Docker containers, allowing me to deploy my nginx-based landing page container directly from the Dockerfile without modification.

3. **Ease of Deployment**: The platform offers straightforward deployment workflows that integrate with Git repositories, enabling automated deployments when code is pushed to the main branch.

4. **Reliability**: [Provider name] offers high availability with automatic health checks and container restarts, ensuring the application remains accessible.

5. **Scalability**: While the current deployment uses minimal resources, the platform allows easy scaling if the application needs to handle more traffic in the future.

6. **SSL/HTTPS**: The platform provides free SSL certificates, ensuring secure connections for users downloading the APK file.

### A2. Container Image Implementation

The FMMS landing page is containerized using Docker with the following implementation:

#### Dockerfile Overview

```dockerfile
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
RUN rm -rf ./*
COPY docs/ .
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/ || exit 1
CMD ["nginx", "-g", "daemon off;"]
```

#### Container Image Components:

1. **Base Image**: `nginx:alpine` - A lightweight Linux distribution with nginx web server, chosen for its small footprint (~23MB) and security.

2. **Static Content**: The `docs/` directory containing:
   - `index.html` - Landing page with download instructions
   - `FMMS.apk` - The compiled Android application package

3. **Custom nginx Configuration**: The `nginx.conf` file configures:
   - Proper MIME types for APK file downloads
   - Security headers (X-Frame-Options, X-Content-Type-Options, X-XSS-Protection)
   - Health check endpoint at `/health`
   - Gzip compression for improved performance

4. **Health Check**: Docker HEALTHCHECK directive ensures container reliability by periodically verifying the web server responds correctly.

#### Build and Deployment Process:

1. **Build the container image:**
   ```bash
   docker build -t fmms-landing-page:latest .
   ```

2. **Test locally:**
   ```bash
   docker run -d -p 8080:80 fmms-landing-page:latest
   ```

3. **Push to container registry:**
   ```bash
   docker tag fmms-landing-page:latest [registry]/fmms-landing-page:latest
   docker push [registry]/fmms-landing-page:latest
   ```

4. **Deploy to cloud platform:**
   - Connected Git repository to [cloud provider]
   - Platform automatically builds and deploys from Dockerfile
   - Container runs on port 80, exposed via HTTPS

---

## B. Project Export

The complete project has been exported from GitLab as a compressed file, including:

- Source code for the .NET MAUI mobile application
- Docker deployment files (Dockerfile, nginx.conf, docker-compose.yml)
- Deployment automation scripts (deploy.ps1, deploy.sh)
- Render.com blueprint specification (render.yaml)
- Unit tests
- Documentation

**GitLab Repository URL:** [Insert your GitLab URL]

---

## C. Panopto Video Recording

A demonstration video has been recorded showing:

1. The Docker container build process
2. Local testing of the containerized application
3. Deployment to the cloud platform
4. Verification of the live deployed application
5. Demonstration of the APK download functionality

**Panopto Video URL:** [Insert your Panopto URL]

---

## D. References

Docker. (2024). Docker Documentation. Retrieved from https://docs.docker.com/

nginx. (2024). nginx Documentation. Retrieved from https://nginx.org/en/docs/

Microsoft. (2024). .NET MAUI Documentation. Retrieved from https://learn.microsoft.com/en-us/dotnet/maui/

[Cloud Provider]. (2024). [Provider Documentation]. Retrieved from [URL]

---

## Appendix: Deployment Files

### Dockerfile
See: `/Dockerfile`

### nginx Configuration
See: `/nginx.conf`

### Deployment Scripts
- Windows PowerShell: `/deploy.ps1`
- Linux/macOS Bash: `/deploy.sh`

### Docker Compose (Local Testing)
See: `/docker-compose.yml`

### Cloud Platform Blueprint
See: `/render.yaml`

