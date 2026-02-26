# Magnetic Pulse Africa

Magnetic Pulse Africa is a modern web application dedicated to promoting and providing **PEMF (Pulsed Electromagnetic Field) Therapy**, specifically featuring the South African trademarked **Boboski** device. The platform serves as a bridge for energy healing, offering non-invasive treatment options for various health conditions.

## 🌟 Core Features

- **Consultation Booking**: Seamless interface for users to book therapeutic sessions.
- **Testimonial Management**: Dynamic system for patients to share their healing journeys and success stories.
- **Admin Dashboard**: Secure portal for administrators to manage consultations, testimonials, and view site analytics.
- **PEMF Education**: Comprehensive information about the benefits of Boboski therapy, including muscle relaxation, pain management, and improved circulation.
- **Secure Authentication**: Cookie-based authentication for administrative access.
- **SMS Integration**: Automated SMS notifications and services via Twilio.

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 6.0 (MVC)
- **Database**: Firebase Realtime Database
- **Authentication**: Firebase Auth & ASP.NET Core Cookie Authentication
- **External APIs**: Twilio SMS Service
- **Front-end**: HTML5, CSS3, JavaScript, Bootstrap 5, Animate.css
- **Backend Services**: FireSharp & FirebaseDatabase.net for seamless data operations.

## 📂 Project Structure

```text
MagneticPulseAfrica/
├── Controllers/       # Logic for Home, Admin, Consultations, and Testimonials
├── Models/            # Data structures and ViewModels
├── Views/             # Razor views for the user interface
├── Services/          # External service integrations (Twilio SMS)
├── wwwroot/           # Static assets (CSS, Images, Videos, JS)
└── Program.cs         # Application configuration and middleware pipeline
```

## 🚀 Getting Started

### Prerequisites
- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or VS Code

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/tshepo-motebele/Magnetic-Pulse-Africa.git
   ```
2. Open the solution file `MagneticPulseAfrica.sln` in your preferred IDE.
3. Configure your API keys in `appsettings.json` (Firebase and Twilio credentials required).
4. Restore dependencies and run the project:
   ```bash
   dotnet run --project MagneticPulseAfrica
   ```

## 🔐 Environment Variables

Ensure the following configurations are set in your environment or `appsettings.json`:
- `Firebase:AuthSecret`
- `Firebase:BasePath`
- `Twilio:AccountSid`
- `Twilio:AuthToken`
- `Twilio:PhoneNumber`

---
Developed with ❤️ for Magnetic Pulse Africa.
