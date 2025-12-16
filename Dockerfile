# FMMS - Family Medication Management System
# Docker container for hosting the APK download landing page
# Uses nginx alpine for a lightweight, secure web server

FROM nginx:alpine

# Set working directory
WORKDIR /usr/share/nginx/html

# Remove default nginx static content
RUN rm -rf ./*

# Copy the landing page and APK to nginx html directory
COPY docs/ .

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Expose port 80 for HTTP traffic
EXPOSE 80

# Health check to ensure the container is running properly
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/ || exit 1

# Start nginx in foreground mode
CMD ["nginx", "-g", "daemon off;"]

