using CommandLine;
using CommandLine.Text;
using VcEngineRunner.Simulation;

namespace VcEngineRunner
{
    public class Options
    {
        public Options()
        {
            RunSimulationVerb = new RunSimulationOptions();
            RecordSimulationOptions = new RecordSimulationOptions();
        }

        [VerbOption("run-simulation", HelpText = "Run a simulation")]
        public RunSimulationOptions RunSimulationVerb { get; set; }

        [VerbOption("record-simulation", HelpText = "Record simulation to video")]
        public RecordSimulationOptions RecordSimulationOptions { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
