# D424 Task 4: Deployment Documentation

**Student:** Richard Robinson  
**Application:** Family Medication Management System (FMMS)  
**Date:** December 2024

---

## A1. Justification for Cloud Solution Provider Selection

I selected **GitHub Pages** as my cloud hosting solution for deploying the FMMS mobile application for the following reasons:

1. **Cost-Effectiveness**: GitHub Pages provides free hosting for static websites, making it an ideal choice for hosting a mobile application landing page without incurring any deployment costs.

2. **Simplicity and Reliability**: GitHub Pages offers straightforward deployment directly from a Git repository. Since my project is already version-controlled in GitLab, mirroring to GitHub for hosting provides a seamless workflow with automatic deployments on each commit.

3. **HTTPS Support**: GitHub Pages automatically provides free SSL certificates, ensuring users can securely access the landing page and download the APK file over an encrypted connection.

4. **Global CDN**: GitHub Pages leverages a global content delivery network (CDN), ensuring fast load times for users regardless of their geographic location.

5. **Integration with Version Control**: The tight integration between GitHub Pages and Git repositories means deployment is as simple as pushing code to the repository. This aligns with modern DevOps practices and continuous deployment workflows.

6. **Appropriate for Mobile App Distribution**: For a mobile application that doesn't require server-side processing, GitHub Pages is sufficient to host a static landing page that allows users to download the APK directly. This approach is commonly used for beta testing and internal app distribution outside of official app stores.

---

## A2. Container Implementation Explanation

Containers were **not utilized** for deploying this mobile application, and here is the justification:

**Understanding Containers**: Docker containers are designed to package applications along with their dependencies, runtime environment, and configuration into a single portable unit. Containers are particularly valuable for:
- Web applications requiring server-side processing (Node.js, Python, .NET backends)
- Microservices architectures where multiple services need isolation
- Applications that need consistent environments across development, testing, and production
- Scaling applications horizontally across multiple instances

**Why Containers Were Not Necessary for FMMS**:

1. **Static Content Only**: The deployed component is a static HTML landing page with a downloadable APK file. Static content does not require a runtime environment, application server, or backend processing that containers typically provide.

2. **No Server-Side Dependencies**: Unlike a web application with a database, API endpoints, or server-side logic, this deployment consists solely of static files (HTML, CSS, and the APK binary). GitHub Pages natively serves static files without needing containerization.

3. **Mobile App Architecture**: The FMMS application itself is a .NET MAUI mobile app that runs natively on Android devices. The app contains its own SQLite database and operates independently on the user's device. There is no backend server component that would benefit from containerization.

4. **Simplified Deployment**: Using containers for static file hosting would add unnecessary complexity without providing meaningful benefits. GitHub Pages abstracts away infrastructure concerns entirely, making it more efficient for this use case.

5. **Cost and Resource Efficiency**: Running a container (even a lightweight nginx container) on a cloud platform would require compute resources and potentially incur costs. For serving static files, this overhead is unnecessary when free static hosting solutions like GitHub Pages exist.

In summary, while containers are powerful tools for deploying complex applications with dependencies and backend services, they are not the appropriate solution for hosting a simple static landing page for mobile app distribution.

---

## Deployed Application

**GitHub Pages URL:** [Insert your GitHub Pages URL here]  
Example: `https://yourusername.github.io/fmms-app/`

**GitLab Repository URL:** [Insert your GitLab URL here]

---

## References

GitHub. (2024). GitHub Pages Documentation. Retrieved from https://docs.github.com/en/pages

Microsoft. (2024). .NET MAUI Documentation. Retrieved from https://learn.microsoft.com/en-us/dotnet/maui/

Docker. (2024). What is a Container? Retrieved from https://www.docker.com/resources/what-container/

