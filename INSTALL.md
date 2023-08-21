# .NET 6 Web API Installation Guide

1. **Install .NET 6 SDK**: Download and install from [here](https://dotnet.microsoft.com/download/dotnet/6.0).
2. **Clone Repository**: `git clone https://github.com/fathi-ch/Clinic.Oncology.git`.
3. **Build Solution**: Navigate to your project folder and run `dotnet build`.
4. **Publish**: `dotnet publish -c Release -o ./publish`.
5. **Install [IIS](https://www.iis.net/)**: Ensure the ASP.NET Core Hosting Bundle is installed.
6. **Configure IIS**: Create a new site, set the physical path to the `./publish` directory.
7. **Bind & Start**: Bind the site to your desired port and start the site.

Your API should now be running locally on IIS!
