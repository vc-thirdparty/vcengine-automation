#addin "Cake.FileHelpers&version=1.0.4"
#addin "nuget:?package=Cake.Sonar"
#tool "nuget:?package=MSBuild.SonarQube.Runner.Tool"

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
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var outputDir = Directory("./output");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
	{
		CleanDirectories(outputDir);
		CleanDirectories("./src/**/bin");
		CleanDirectories("./src/**/obj");
	});

Task("Init")
	.Does(() =>
	{
		CreateDirectory(outputDir);
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
	.IsDependentOn("Init")
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
	.Does(() =>
	{
		NuGetRestore("./src/VcEngineAutomation.sln");
	});

Task("Nuget-Pack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		NuGetPack("./nuspec/VcEngineAutomation.nuspec", new NuGetPackSettings {
			Version = version,
			OutputDirectory = outputDir
		});
	});

Task("Zip-Pack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		var fileExtensions = new string[] { ".dll", ".exe" };
		var files = GetFiles("./src/VcEngineRunner/bin/Release/*.*")
			.Where(f => fileExtensions.Contains(f.GetExtension().ToLower()))
			.Concat(GetFiles("./LICENSE"));

			EnsureDirectoryExists("./output/release");		
		CopyFiles(files, "./output/release");
		Zip("./output/release/", outputDir.Path.GetFilePath("VcEngineRunner." + version + ".zip"));
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
	.IsDependentOn("Clean")
	.IsDependentOn("Sonar-Init")
	.IsDependentOn("Build")
	.IsDependentOn("Nuget-Pack")
	.IsDependentOn("Zip-Pack")
	.IsDependentOn("Sonar-Analyse");

	
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
