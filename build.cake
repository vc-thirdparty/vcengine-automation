#addin "nuget:?package=Cake.Sonar"
#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool"
#addin "Cake.FileHelpers"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var version = EnvironmentVariable("PKG_VERSION") ?? Argument("pkgVersion", "0.0.0");

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var sonarcloudKey = EnvironmentVariable("SONARCLOUD_API_KEY") ?? Argument("sonarcloudKey", "");
var nugetKey = EnvironmentVariable("NUGET_API_KEY") ?? Argument("nugetKey", "");

var travis = EnvironmentVariable("TRAVIS") ?? "false";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
	{
		CleanDirectories("./src/**/bin");
		CleanDirectories("./src/**/obj");
	});

Task("SetVersion")
	.WithCriteria(() => travis == "true")
	.Does(() => {
	   ReplaceRegexInFiles("./**/AssemblyInfo.cs", 
						   "(?<=AssemblyVersion\\(\")(.+?)(?=\"\\))", 
						   version);
	   ReplaceRegexInFiles("./**/AssemblyInfo.cs", 
						   "(?<=AssemblyFileVersion\\(\")(.+?)(?=\"\\))", 
						   version);
   });

Task("Build")
	.IsDependentOn("NuGet-Restore-Packages")
	.IsDependentOn("SetVersion")
	.Does(() =>
	{
		if(IsRunningOnWindows())
		{
		  // Use MSBuild
		  MSBuild("./src/VcEngineAutomation.sln", settings => settings.SetConfiguration(configuration));
		}
		else
		{
		  // Use XBuild
		  XBuild("./src/VcEngineAutomation.sln", settings => settings.SetConfiguration(configuration));
		}
	});


Task("NuGet-Restore-Packages")
	.IsDependentOn("Clean")
	.Does(() =>
	{
		NuGetRestore("./src/VcEngineAutomation.sln");
	});

Task("Nuget-Pack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		NuGetPack("./nuspec/VcEngineAutomation.nuspec", new NuGetPackSettings {
			Version = version
		});
	});

Task("Choco-Pack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		ChocolateyPack("./nuspec/VcEngineRunner.nuspec", new ChocolateyPackSettings {
			Version = version
		});
	});

Task("Sonar-Init")
	.WithCriteria(IsRunningOnWindows())
	.WithCriteria(() => sonarcloudKey != "")
	.Does(() => 
	{
		SonarBegin(new SonarBeginSettings{
			Key = "VcEngineAutomation",
			Organization = "redsolo-github",
			Url = "https://sonarcloud.io"
		});
	});
  
Task("Sonar-Analyse")
	.WithCriteria(IsRunningOnWindows())
	.WithCriteria(() => sonarcloudKey != "")
	.Does(() => {
		SonarEnd(new SonarEndSettings{
			Login = sonarcloudKey
		});
	});
  



//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Sonar-Init")
	.IsDependentOn("Build")
	.IsDependentOn("Nuget-Pack")
	.IsDependentOn("Sonar-Analyse");
	;
	
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
