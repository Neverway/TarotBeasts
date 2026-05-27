using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static UnityEditor.Rendering.CameraUI;

public static partial class GameFuncs
{
    private static Dictionary<string, List<Processor>> inputProcessing;
    private static Dictionary<string, List<Processor>> outputProcessing;

    private class Processor { public string funcName; }
    private class InputProcessor<TInput> : Processor
    {
        public Func<TInput, TInput> func;
        public InputProcessor(string funcName, Func<TInput, TInput> func)
        {
            this.funcName = funcName;
            this.func = func;
        }
    }
    private class OutputProcessor<TInput, TOutput> : Processor
    {
        public Func<TInput, TOutput, TOutput> func;
        public OutputProcessor(string funcName, Func<TInput, TOutput, TOutput> func)
        {
            this.funcName = funcName;
            this.func = func;
        }
    }

    public static void ResetRegisteredProcessing()
    {
        inputProcessing = new();
        outputProcessing = new();
    }

    private static void RegisterInputProcessing(string funcName, Processor proccesor)
    {
        if (!inputProcessing.ContainsKey(funcName))
            inputProcessing.Add(funcName, new List<Processor>());

        inputProcessing[funcName].Add(proccesor);
    }
    private static void RegisterOutputProcessing(string funcName, Processor proccesor)
    {
        if (!outputProcessing.ContainsKey(funcName))
            outputProcessing.Add(funcName, new List<Processor>());

        outputProcessing[funcName].Add(proccesor);
    }

    public static void RegisterOutputProcessing<TOutput>(string funcName, Func<TOutput, TOutput> onProcessOutput)
        => RegisterOutputProcessing(funcName, new OutputProcessor<TOutput, TOutput>(funcName, (i, o) => onProcessOutput(o)));
    public static void RegisterInputProcessing<TInput>(string funcName, Func<TInput, TInput> onProcessInput)
        => RegisterInputProcessing(funcName, new InputProcessor<TInput>(funcName, onProcessInput));
    public static void RegisterOutputProcessing<TInput, TOutput>(string funcName, Func<TInput, TOutput, TOutput> onProcessOutput)
        => RegisterOutputProcessing(funcName, new OutputProcessor<TInput, TOutput>(funcName, onProcessOutput));

    public static void RegisterInputProcessing<TInput1, TInput2>(string funcName, Func<TInput1, TInput2, Tuple<TInput1, TInput2>> onProcessInput)
        => RegisterInputProcessing(funcName, new InputProcessor<Tuple<TInput1, TInput2>>(funcName, (i) => onProcessInput(i.Item1, i.Item2)));
    public static void RegisterOutputProcessing<TInput1, TInput2, TOutput>(string funcName, Func<TInput1, TInput2, TOutput, TOutput> onProcessOutput)
        => RegisterOutputProcessing(funcName, new OutputProcessor<Tuple<TInput1, TInput2>, TOutput>(funcName, (i, o) => onProcessOutput(i.Item1, i.Item2, o)));

    public static void RegisterInputProcessing<TInput1, TInput2, TInput3>(string funcName, Func<TInput1, TInput2, TInput3, Tuple<TInput1, TInput2, TInput3>> onProcessInput)
        => RegisterInputProcessing(funcName, new InputProcessor<Tuple<TInput1, TInput2, TInput3>>(funcName, (i) => onProcessInput(i.Item1, i.Item2, i.Item3)));
    public static void RegisterOutputProcessing<TInput1, TInput2, TInput3, TOutput>(string funcName, Func<TInput1, TInput2, TInput3, TOutput, TOutput> onProcessOutput)
        => RegisterOutputProcessing(funcName, new OutputProcessor<Tuple<TInput1, TInput2, TInput3>, TOutput>(funcName, (i, o) => onProcessOutput(i.Item1, i.Item2, i.Item3, o)));

    public static void RegisterInputProcessing<TInput1, TInput2, TInput3, TInput4>(string funcName, Func<TInput1, TInput2, TInput3, TInput4, Tuple<TInput1, TInput2, TInput3, TInput4>> onProcessInput)
        => RegisterInputProcessing(funcName, new InputProcessor<Tuple<TInput1, TInput2, TInput3, TInput4>>(funcName, (i) => onProcessInput(i.Item1, i.Item2, i.Item3, i.Item4)));
    public static void RegisterOutputProcessing<TInput1, TInput2, TInput3, TInput4, TOutput>(string funcName, Func<TInput1, TInput2, TInput3, TInput4, TOutput, TOutput> onProcessOutput)
        => RegisterOutputProcessing(funcName, new OutputProcessor<Tuple<TInput1, TInput2, TInput3, TInput4>, TOutput>(funcName, (i, o) => onProcessOutput(i.Item1, i.Item2, i.Item3, i.Item4, o)));

    public static TInput ProcessInput<TInput>(TInput input, [CallerMemberName] string funcName = "") => ProcessInput(ref input, funcName);
    public static TInput ProcessInput<TInput>(ref TInput input, [CallerMemberName] string funcName = "")
    {
        if (!inputProcessing.ContainsKey(funcName))
            return input;

        foreach (Processor processor in inputProcessing[funcName])
        {
            if (processor is InputProcessor<TInput> inputProcessor)
                input = inputProcessor.func.Invoke(input);
            else
                throw new Exception($"Invalid InputProcessor has been registered to method: \"{funcName}\"... " +
                    $"Did you provide the wrong type of inputs somewhere? Or wrong function name?");
        }
        return input;
    }

    public static TOutput ProcessOutput<TInput, TOutput>(TInput input, ref TOutput output, [CallerMemberName] string funcName = "") => output = ProcessOutput(input, output, funcName);
    public static TOutput ProcessOutput<TInput, TOutput>(TInput input, TOutput output, [CallerMemberName] string funcName = "")
    {
        if (!outputProcessing.ContainsKey(funcName))
            return output;

        foreach (Processor processor in outputProcessing[funcName])
        {
            if (processor is OutputProcessor<TInput, TOutput> outputProcessor)
                output = outputProcessor.func.Invoke(input, output);
            else if (processor is OutputProcessor<object, TOutput> outputProcessorNoInput)
                output = outputProcessorNoInput.func.Invoke(null, output);
            else
                throw new Exception($"Invalid InputProcessor has been registered to method: \"{funcName}\"... " +
                    $"Did you provide the wrong type of inputs somewhere? Or wrong function name?");
        }
        return output;
    }

    public static Tuple<TInput1, TInput2> ProcessInput<TInput1, TInput2>(TInput1 input1, TInput2 input2, [CallerMemberName] string funcName = "")
        => ProcessInput(new Tuple<TInput1, TInput2>(input1, input2), funcName);
    public static void ProcessInput<TInput1, TInput2>(ref TInput1 input1, ref TInput2 input2, [CallerMemberName] string funcName = "")
        => (input1, input2) = ProcessInput(new Tuple<TInput1, TInput2>(input1, input2), funcName);
    public static TOutput ProcessOutput<TInput1, TInput2, TOutput>(TInput1 input1, TInput2 input2, TOutput output, [CallerMemberName] string funcName = "")
        => ProcessOutput(new Tuple<TInput1, TInput2>(input1, input2), output, funcName);
    public static Tuple<TInput1, TInput2, TInput3> ProcessInput<TInput1, TInput2, TInput3>(TInput1 input1, TInput2 input2, TInput3 input3, [CallerMemberName] string funcName = "")
        => ProcessInput(new Tuple<TInput1, TInput2, TInput3>(input1, input2, input3), funcName);
    public static void ProcessInput<TInput1, TInput2, TInput3>(ref TInput1 input1, ref TInput2 input2, ref TInput3 input3, [CallerMemberName] string funcName = "")
        => (input1, input2, input3) = ProcessInput(new Tuple<TInput1, TInput2, TInput3>(input1, input2, input3), funcName);
    public static TOutput ProcessOutput<TInput1, TInput2, TInput3, TOutput>(TInput1 input1, TInput2 input2, TInput3 input3, TOutput output, [CallerMemberName] string funcName = "")
        => ProcessOutput(new Tuple<TInput1, TInput2, TInput3>(input1, input2, input3), output, funcName);
    public static Tuple<TInput1, TInput2, TInput3, TInput4> ProcessInput<TInput1, TInput2, TInput3, TInput4>(TInput1 input1, TInput2 input2, TInput3 input3, TInput4 input4, [CallerMemberName] string funcName = "")
        => ProcessInput(new Tuple<TInput1, TInput2, TInput3, TInput4>(input1, input2, input3, input4), funcName);
    public static void ProcessInput<TInput1, TInput2, TInput3, TInput4>(ref TInput1 input1, ref TInput2 input2, ref TInput3 input3, ref TInput4 input4, [CallerMemberName] string funcName = "")
        => (input1, input2, input3, input4) = ProcessInput(new Tuple<TInput1, TInput2, TInput3, TInput4>(input1, input2, input3, input4), funcName);
    public static TOutput ProcessOutput<TInput1, TInput2, TInput3, TInput4, TOutput>(TInput1 input1, TInput2 input2, TInput3 input3, TInput4 input4, TOutput output, [CallerMemberName] string funcName = "")
        => ProcessOutput(new Tuple<TInput1, TInput2, TInput3, TInput4>(input1, input2, input3, input4), output, funcName);

}












public static partial class GameFuncs
{
    public class Animal { }


    public static void RegisterRabbitUpgrade()
    {
        GameFuncs.RegisterOutputProcessing<Animal, Animal, bool>(
            nameof(DoesAnimalBeatOtherAnimal), RabbitUpgradeProcessing);
    }
    public static bool RabbitUpgradeProcessing(Animal animal, Animal target, bool output)
    {

        //Logic here
        return output;
    }






    public static bool DoesAnimalBeatOtherAnimal(Animal current, Animal target)
    {
        ProcessInput(ref current, ref target);

        //Logic here

        return ProcessOutput(current, target, false);
    }






    public static int AddOne(int n)
    {
        ProcessInput(ref n);

        n += 1;

        return ProcessOutput(n, n);
    }
}