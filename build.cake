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
var nugetApiKey = EnvironmentVariable("NUGET_API_KEY") ?? "NOOP";

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
	.IsDependentOn("NuGetRestorePackages")
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


Task("NuGetRestorePackages")
	.Does(() =>
	{
		NuGetRestore("./src/VcEngineAutomation.sln");
	});

Task("NugetPack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		NuGetPack("./nuspec/VcEngineAutomation.nuspec", new NuGetPackSettings {
			Version = version,
			OutputDirectory = outputDir
		});
	});
	
Task("NugetPush")
	.Does(() =>
	{
		NuGetPush(GetFiles("./**/*.nupkg"), new NuGetPushSettings {
			ApiKey = nugetApiKey,
			Source = "https://www.nuget.org/api/v2/package"
		});
	});

Task("ZipPack")
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

Task("ChocoPack")
	.IsDependentOn("Build")
	.Does(() =>
	{
		ChocolateyPack("./nuspec/VcEngineRunner.nuspec", new ChocolateyPackSettings {
			Version = version
		});
	});

Task("SonarInit")
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
  
Task("SonarAnalyse")
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
	.IsDependentOn("SonarInit")
	.IsDependentOn("Build")
	.IsDependentOn("NugetPack")
	.IsDependentOn("ZipPack")
	.IsDependentOn("SonarAnalyse");

	
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
