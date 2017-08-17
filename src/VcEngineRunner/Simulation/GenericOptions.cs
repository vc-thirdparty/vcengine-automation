using CommandLine;
using VcEngineAutomation;

namespace VcEngineRunner.Simulation
{
    public class GenericOptions
    {
        public GenericOptions()
        {
            InstallationPath = VcEngine.DefaultInstallationPath;
        }
        [Option("installation-path", HelpText = "Path to the installation folder")]
        public string InstallationPath { get; set; }
    }
}
