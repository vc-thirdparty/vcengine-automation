#addin "nuget:?package=Cake.Sonar"
#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool"
#addin "Cake.FileHelpers"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var version = Argument("version", "0.0.3");

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var sonarqubeKey = EnvironmentVariable("SONARQUBE_API_KEY") ?? Argument("sonarqubeKey", "");
var nugetKey = EnvironmentVariable("NUGET_API_KEY") ?? Argument("nugetKey", "");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
	{
	});

Task("SetVersion")
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

Task("Package")
	.IsDependentOn("Build")
	.Does(() =>
	{
		NuGetPack("./nuspec/VcEngineAutomation.nuspec", new NuGetPackSettings {
			Version = version
		});
		ChocolateyPack("./nuspec/VcEngineRunner.nuspec", new ChocolateyPackSettings {
			Version = version
		});
	});

Task("Initialise-Sonar")
	.Does(() => 
	{
		SonarBegin(new SonarBeginSettings{
			Key = "VcEngineAutomation",
			Organization = "redsolo-github",
			Url = "https://sonarcloud.io"
		});
	});
  
Task("Sonar-Analyse")
	.Does(() => {
		SonarEnd(new SonarEndSettings{
			Login = sonarqubeKey
		});
	});
  



//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
//    .IsDependentOn("Initialise-Sonar")
	.IsDependentOn("Build")
	.IsDependentOn("Package")
//    .IsDependentOn("Sonar-Analyse");
	;
	
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);