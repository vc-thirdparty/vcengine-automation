using VcEngineRunner.Simulation;

namespace VcEngineRunner
{
    class Program
    {
        static int Main(string[] args)
        {
            string invokedVerb = null;
            object invokedVerbInstance = null;

            var options = new Options();
            CommandLine.Parser.Default.ParseArgumentsStrict(args, options,
                (verb, subOptions) =>
                {
                    // if parsing succeeds the verb name and correct instance
                    // will be passed to onVerbCommand delegate (string,object)
                    invokedVerb = verb;
                    invokedVerbInstance = subOptions;
                });
            if (invokedVerb == "run-simulation")
            {
                return new RunSimulation((RunSimulationOptions)invokedVerbInstance).Run();
            }
            else if (invokedVerb == "record-simulation")
            {
                return new RecordSimulation((RecordSimulationOptions)invokedVerbInstance).Run();
            }
            return 1;
        }
    }
}
