using CommandLine;

namespace VcEngineRunner.Simulation
{
    public class RunSimulationOptions
    {
        [Option('f', "file", HelpText = "Layout file to run", Required = true)]
        public string LayoutFile { get; set; }

        [Option('d', "duration", HelpText = "Duration to run simulation (hours:minutes:seconds)", DefaultValue = "0:0:5")]
        public string Duration { get; set; }

        [Option('s', "speed", HelpText = "Speed during simulation (0-100 %, 50 = real time)", DefaultValue = 50)]
        public double SpeedFactor { get; set; }

        [Option('v', "video-file", HelpText = "Record to video file name")]
        public string VideoFile { get; set; }
    }
}
