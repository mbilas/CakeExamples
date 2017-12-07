#tool nuget:?package=NUnit.ConsoleRunner&version=3.7.0
#addin "Cake.Figlet"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/Example/bin") + Directory(configuration);

Setup(context =>
{
    Information(Figlet("Cake Build Script"));
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./src/Example.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if(IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./src/Example.sln", settings =>
        settings.SetConfiguration(configuration));
    }
    else
    {
      // Use XBuild
      XBuild("./src/Example.sln", settings =>
        settings.SetConfiguration(configuration));
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3("./src/**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
        NoResults = true
        });
});

Task("Pack")
  .IsDependentOn("Run-Unit-Tests")
  .Does(() => {
    var nuGetPackSettings   = new NuGetPackSettings {
                                    Id                      = "CakeDemo",
                                    Version                 = "0.0.0.1",
                                    Title                   = "Cake Demo",
                                    Authors                 = new[] {"Marcin"},
                                    Description             = "Demo of creating cake.build scripts.",
                                    Summary                 = "Excellent summary of what the Cake (C# Make) build tool does.",
                                    Files                   = new [] {
                                                                        new NuSpecContent { Source = "Example.dll", Target = "bin" },
                                                                      },
                                    BasePath                = "./src/Example/bin/" + configuration,
                                    OutputDirectory         = "./nuget",
                                    Properties = new Dictionary<string, string>
                                    {
                                        { "Configuration", "Release" }
                                    }
                                };

    NuGetPack("./src/Example/Example.csproj", nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
