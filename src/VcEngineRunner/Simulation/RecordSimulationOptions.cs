using CommandLine;

namespace VcEngineRunner.Simulation
{
    public enum RecordType
    {
        Video,
        Experience
    }
    public class RecordSimulationOptions : GenericOptions
    {
        [Option('f', "file", HelpText = "Layout file to record", Required = true)]
        public string LayoutFile { get; set; }

        [Option('d', "duration", HelpText = "Duration to record simulation (hours:minutes:seconds)", DefaultValue = "0:0:5")]
        public string Duration { get; set; }

        [Option('s', "speed", HelpText = "Speed during simulation (0-100 %, 50 = real time)", DefaultValue = 50)]
        public double SpeedFactor { get; set; }

        [Option('o', "output-file", HelpText = "Record to file name", Required = true)]
        public string OutputFile { get; set; }

        [Option('t', "output-type", HelpText = "Type of output, defaults to Video")]
        public RecordType OutputFileType { get; set; }
    }
}
