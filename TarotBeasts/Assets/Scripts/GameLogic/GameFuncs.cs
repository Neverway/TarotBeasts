using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

public static partial class GameFuncs
{
    //Where all Processors are stored
    private static Dictionary<string, List<Processor>> inputProcessing;
    private static Dictionary<string, List<Processor>> outputProcessing;

    //Ran at the beginning of the game, and can be called anytime to reset all registered Processors
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void ResetRegisteredProcessors()
    {
        inputProcessing = new Dictionary<string, List<Processor>>();
        outputProcessing = new Dictionary<string, List<Processor>>();
    }

    //---------------------------------------------------------- Delegate Definitions -----------------------------------------------------------------------

    //Main Input/Output processing function delegates (these are used inside of Processors)
    public delegate void ModInputFunc<T>(ref T input);
    public delegate void ModOutputFunc<T1, TOut>(T1 input, ref TOut output);

    //Other Input processing function delegates
    //(only used in helper functions, which become compacted into the MAIN Input processing function delegate via ToSingleInput(...))
    public delegate void ModInputFunc<T1, T2>(ref T1 input1, ref T2 input2);
    public delegate void ModInputFunc<T1, T2, T3>(ref T1 input1, ref T2 input2, ref T3 input3);
    public delegate void ModInputFunc<T1, T2, T3, T4>(ref T1 input1, ref T2 input2, ref T3 input3, ref T4 input4);

    //Other Output processing function delegates
    //(only used in helper functions, which become compacted into the MAIN Output processing function delegate via ToSingleInput(...))
    public delegate void ModOutputFunc<T>(ref T output);
    public delegate void ModOutputFunc<T1, T2, TOut>(T1 input1, T2 input2, ref TOut output);
    public delegate void ModOutputFunc<T1, T2, T3, TOut>(T1 input1, T2 input2, T3 input3, ref TOut output);
    public delegate void ModOutputFunc<T1, T2, T3, T4, TOut>(T1 input1, T2 input2, T3 input3, T4 input4, ref TOut output);


    //---------------------------------------------------------- Delegate Conversion Methods-----------------------------------------------------------------

    //These "ToSingleInput(...)" functions will take in a function of multiple input types and converts it into function using only 1 tuple input instead
    //Input processing function conversion methods
    public static ModInputFunc<(T1, T2)> ToSingleInput<T1, T2>(ModInputFunc<T1, T2> func)
        => (ref (T1, T2) input) => func.Invoke(ref input.Item1, ref input.Item2);
    public static ModInputFunc<(T1, T2, T3)> ToSingleInput<T1, T2, T3>(ModInputFunc<T1, T2, T3> func)
        => (ref (T1, T2, T3) input) => func.Invoke(ref input.Item1, ref input.Item2, ref input.Item3);
    public static ModInputFunc<(T1, T2, T3, T4)> ToSingleInput<T1, T2, T3, T4>(ModInputFunc<T1, T2, T3, T4> func)
        => (ref (T1, T2, T3, T4) input) => func.Invoke(ref input.Item1, ref input.Item2, ref input.Item3, ref input.Item4);

    //Output processing function conversion methods
    public static ModOutputFunc<object, TOut> ToSingleInput<TOut>(ModOutputFunc<TOut> func)
        => (object _, ref TOut output) => func.Invoke(ref output);
    public static ModOutputFunc<(T1, T2), TOut> ToSingleInput<T1, T2, TOut>(ModOutputFunc<T1, T2, TOut> func)
        => ((T1, T2) input, ref TOut output) => func.Invoke(input.Item1, input.Item2, ref output);
    public static ModOutputFunc<(T1, T2, T3), TOut> ToSingleInput<T1, T2, T3, TOut>(ModOutputFunc<T1, T2, T3, TOut> func)
        => ((T1, T2, T3) input, ref TOut output) => func.Invoke(input.Item1, input.Item2, input.Item3, ref output);
    public static ModOutputFunc<(T1, T2, T3, T4), TOut> ToSingleInput<T1, T2, T3, T4, TOut>(ModOutputFunc<T1, T2, T3, T4, TOut> func)
        => ((T1, T2, T3, T4) input, ref TOut output) => func.Invoke(input.Item1, input.Item2, input.Item3, input.Item4, ref output);

    //Conversion method via reflection (this is black magic, ChatGPT had to help me on this one unfortunately, I modified it a bit though)
    //Takes a method (using multiple input types) and wraps it in a Lambda that lets you provide a tuple of those input types, and it
    //  will properly assign the values as a ref to the inputs/outputs (much like in the functions above this) to the provided method
    public static Delegate ToSingleInput(object instance, MethodInfo method, Type inputTupleType, Type outputType = null)
    {
        bool isForInputs = outputType == null;
        bool isStatic = instance == null;
        // Define params for the lambda
        ParameterExpression tupleInputParam = Expression.Parameter(isForInputs ? inputTupleType.MakeByRefType() : inputTupleType, "input");
        ParameterExpression outputParam = isForInputs ? null : Expression.Parameter(outputType.MakeByRefType(), "output");

        // Create Args for invoking the given method by extracting Item1, Item2, Item3... etc from the tuple
        List<Expression> methodArgs = inputTupleType.GetFields()
            .Where(f => f.Name.StartsWith("Item"))
            .OrderBy(f => f.Name)
            .Select(f => (Expression) Expression.Field(tupleInputParam, f))
            .ToList();
        // Append "ref output" to the args if this method is for outputs
        if (!isForInputs) methodArgs.Add(outputParam);

        // Define the body of the lambda (invokes the given method with the methodArgs)
        MethodCallExpression body = isStatic ? 
            Expression.Call(method, methodArgs) :
            Expression.Call(Expression.Constant(instance), method, methodArgs);

        // Get the delegate type, which depends on whether this method uses outputs (known via whether an output type was provided)
        Type delegateType = isForInputs ?
            typeof(ModInputFunc<>).MakeGenericType(inputTupleType) :
            typeof(ModOutputFunc<,>).MakeGenericType(inputTupleType, outputType);

        //Create the lambdo expression and return it
        return isForInputs ?
            Expression.Lambda(delegateType, body, tupleInputParam).Compile() :
            Expression.Lambda(delegateType, body, tupleInputParam, outputParam).Compile();
    }


    //---------------------------------------------------------- Processor Classes --------------------------------------------------------------------------
    //Base Processor class, meant to define how to modify the inputs or outputs of certain functions that use the ProcessInput() and ProcessOutput() functions
    //can be registered or unregistered and will automatically assign it to input or output processing depending on its type
    public abstract class Processor
    {
        //Target function to process inputs/outputs for
        protected string funcName;
        public string TargetFuncName => funcName;

        //Registers this Processor to START processing the Inputs/Outputs of the given function whenever it is called
        public abstract Processor Register();
        //Unregisters this Processor to STOP processing the Inputs/Outputs of the given function
        public abstract Processor Unregister();
    }
    //Processes Inputs for the provided function
    private class InputProcessor<TInput> : Processor
    {
        public ModInputFunc<TInput> func;
        public InputProcessor(string funcName, ModInputFunc<TInput> func)
        {
            this.funcName = funcName;
            this.func = func;
        }

        public override Processor Register() => RegisterInputProcessor(funcName, this);
        public override Processor Unregister() => UnregisterInputProcessor(funcName, this);
    }
    //Processes Outputs for the provided function
    private class OutputProcessor<TInput, TOutput> : Processor
    {
        public ModOutputFunc<TInput, TOutput> func;
        public OutputProcessor(string funcName, ModOutputFunc<TInput, TOutput> func)
        {
            this.funcName = funcName;
            this.func = func;
        }
        public override Processor Register() => RegisterOutputProcessor(funcName, this);
        public override Processor Unregister() => UnregisterOutputProcessor(funcName, this);
    }

    //---------------------------------------------------------- Processor Registering/Unregistering --------------------------------------------------------
    //Methods for registering processors
    public static Processor RegisterProcessor(string funcName, Processor processor)
    {
        if (inputProcessing.ContainsKey(funcName))
            processor.Unregister();

        return processor;
    }
    public static Processor RegisterInputProcessor(string funcName, Processor processor)
    {
        if (!inputProcessing.ContainsKey(funcName))
            inputProcessing.Add(funcName, new List<Processor>());

        var list = inputProcessing[funcName];
        if (!list.Contains(processor)) list.Add(processor);

        return processor;
    }
    public static Processor RegisterOutputProcessor(string funcName, Processor processor)
    {
        if (!outputProcessing.ContainsKey(funcName))
            outputProcessing.Add(funcName, new List<Processor>());

        var list = outputProcessing[funcName];
        if (!list.Contains(processor)) list.Add(processor);

        return processor;
    }

    //Methods for unregistering processors
    public static Processor UnregisterProcessor(string funcName, Processor processor)
    {
        if (inputProcessing.ContainsKey(funcName))
            processor.Unregister();
        return processor;
    }
    public static Processor UnregisterInputProcessor(string funcName, Processor processor)
    {
        if (!inputProcessing.ContainsKey(funcName))
            inputProcessing[funcName].Remove(processor);
        return processor;
    }
    public static Processor UnregisterOutputProcessor(string funcName, Processor processor)
    {
        if (outputProcessing.ContainsKey(funcName))
            outputProcessing[funcName].Remove(processor);
        return processor;
    }

    //---------------------------------------------------------- Processor Creation Methods -----------------------------------------------------------------
    public static Processor NewInputProcessor<TInput>(string funcName, ModInputFunc<TInput> onProcessInput)
        => new InputProcessor<TInput>(funcName, onProcessInput);
    public static Processor NewInputProcessor<T1, T2>(string funcName, ModInputFunc<T1, T2> onProcessInput)
        => new InputProcessor<(T1, T2)>(funcName, ToSingleInput(onProcessInput));
    public static Processor NewInputProcessor<T1, T2, T3>(string funcName, ModInputFunc<T1, T2, T3> onProcessInput)
        => new InputProcessor<(T1, T2, T3)>(funcName, ToSingleInput(onProcessInput));
    public static Processor NewInputProcessor<T1, T2, T3, T4>(string funcName, ModInputFunc<T1, T2, T3, T4> onProcessInput)
        => new InputProcessor<(T1, T2, T3, T4)>(funcName, ToSingleInput(onProcessInput));

    public static Processor NewOutputProcessor<TOut>(string funcName, ModOutputFunc<TOut> onProcessOutput)
        => new OutputProcessor<object, TOut>(funcName, ToSingleInput(onProcessOutput));
    public static Processor NewOutputProcessor<T1, TOut>(string funcName, ModOutputFunc<T1, TOut> onProcessOutput)
        => new OutputProcessor<T1, TOut>(funcName, onProcessOutput);
    public static Processor NewOutputProcessor<T1, T2, TOutput>(string funcName, ModOutputFunc<T1, T2, TOutput> onProcessOutput)
        => new OutputProcessor<(T1, T2), TOutput>(funcName, ToSingleInput(onProcessOutput));
    public static Processor NewOutputProcessor<T1, T2, T3, TOutput>(string funcName, ModOutputFunc<T1, T2, T3, TOutput> onProcessOutput)
        => new OutputProcessor<(T1, T2, T3), TOutput>(funcName, ToSingleInput(onProcessOutput));
    public static Processor NewOutputProcessor<T1, T2, T3, T4, TOutput>(string funcName, ModOutputFunc<T1, T2, T3, T4, TOutput> onProcessOutput)
        => new OutputProcessor<(T1, T2, T3, T4), TOutput>(funcName, ToSingleInput(onProcessOutput));

    public static Processor NewInputProcessor(string funcName, object instance, MethodInfo onProcessInput)
    {
        //Get Input type from given method parameters
        var parameterTypes = onProcessInput.GetParameters().Select((p) => p.ParameterType.GetElementType()).ToArray();
        bool HasOnly1Input = parameterTypes.Length == 1;
        Type inputType = HasOnly1Input ? parameterTypes[0] : parameterTypes.ToValueTupleType();

        //Get type of Processor for the methods Input type
        Type processorType = typeof(InputProcessor<>).MakeGenericType(inputType);

        //Create the function for the Processor that invokes the given method
        Type funcType = typeof(ModInputFunc<>).MakeGenericType(inputType);
        object func = HasOnly1Input ?
            (instance == null ? Delegate.CreateDelegate(funcType, onProcessInput) : Delegate.CreateDelegate(funcType, instance, onProcessInput)) :
            ToSingleInput(instance, onProcessInput, inputType);

        //Construct the processor and return it
        ConstructorInfo constructor = processorType.GetConstructor(new[] { typeof(string), funcType });

        return (Processor)constructor.Invoke(new object[] {funcName, func});
    }
    public static Processor NewOutputProcessor(string funcName, object instance, MethodInfo onProcessOutput)
    {
        var parameterTypes = onProcessOutput.GetParameters().Select((p) => p.ParameterType);
        var inputParameters = parameterTypes.Take(parameterTypes.Count() - 1).ToArray();
        bool HasOnly1Input = inputParameters.Length == 1;
        Type inputType = HasOnly1Input ? inputParameters[0] : inputParameters.ToValueTupleType();
        Type outputType = parameterTypes.Last().GetElementType();

        //Get type of Processor for the methods Input type
        Type processorType = typeof(OutputProcessor<,>).MakeGenericType(inputType, outputType);

        //Create the function for the Processor that invokes the given method
        Type funcType = typeof(ModOutputFunc<,>).MakeGenericType(inputType, outputType);
        object func = HasOnly1Input ?
            (instance == null ? Delegate.CreateDelegate(funcType, onProcessOutput) : Delegate.CreateDelegate(funcType, instance, onProcessOutput)) : 
            ToSingleInput(instance, onProcessOutput, inputType, outputType);

        //Construct the processor and return it
        ConstructorInfo constructor = processorType.GetConstructor(new[] { typeof(string), funcType });

        return (Processor)constructor.Invoke(new object[] { funcName, func });
    }

    //---------------------------------------------------------- Input/Output Processing main logic ---------------------------------------------------------
    //"ProcessInput(ref ...)" (or its helper methods below) are to be used at the beginning of the method to allow all Processors a chance to modify the Inputs
    public static TInput ProcessInput<TInput>(TInput input, [CallerMemberName] string funcName = "") => ProcessInput(ref input, funcName);
    public static TInput ProcessInput<TInput>(ref TInput input, [CallerMemberName] string funcName = "")
    {
        if (!inputProcessing.ContainsKey(funcName))
            return input;

        foreach (Processor processor in inputProcessing[funcName])
        {
            if (processor is InputProcessor<TInput> inputProcessor)
                inputProcessor.func.Invoke(ref input);
            else
                throw new Exception($"Invalid InputProcessor has been registered to method: \"{funcName}\"... " +
                    $"Did you provide the wrong type of inputs somewhere? Or wrong function name?");
        }
        return input;
    }

    //"ProcessOutput(input, ref ...)" (or its helper methods below) are to be used at the end of the method to allow all Processors a chance to modify the Inputs
    public static TOutput ProcessOutput<TInput, TOutput>(TInput input, TOutput output, [CallerMemberName] string funcName = "") => ProcessOutput(input, ref output, funcName);
    public static TOutput ProcessOutput<TInput, TOutput>(TInput input, ref TOutput output, [CallerMemberName] string funcName = "")
    {
        if (!outputProcessing.ContainsKey(funcName))
            return output;

        foreach (Processor processor in outputProcessing[funcName])
        {
            if (processor is OutputProcessor<TInput, TOutput> outputProcessor)
                outputProcessor.func.Invoke(input, ref output);
            else if (processor is OutputProcessor<object, TOutput> outputProcessorNoInput)
                outputProcessorNoInput.func.Invoke(null, ref output);
            else
                throw new Exception($"Invalid InputProcessor has been registered to method: \"{funcName}\"... " +
                    $"Did you provide the wrong type of inputs somewhere? Or wrong function name?");
        }
        return output;
    }

    //ProcessInput helper methods for cases where there are multiple inputs to the function (this saves you from having to use tuples by doing it for you)
    public static void ProcessInput<T1, T2>(ref T1 input1, ref T2 input2, [CallerMemberName] string funcName = "")
        => (input1, input2) = ProcessInput((input1, input2), funcName);
    public static void ProcessInput<T1, T2, T3>(ref T1 input1, ref T2 input2, ref T3 input3, [CallerMemberName] string funcName = "")
        => (input1, input2, input3) = ProcessInput((input1, input2, input3), funcName);
    public static void ProcessInput<T1, T2, T3, T4>(ref T1 input1, ref T2 input2, ref T3 input3, ref T4 input4, [CallerMemberName] string funcName = "")
        => (input1, input2, input3, input4) = ProcessInput((input1, input2, input3, input4), funcName);
    //ProcessOutput helper methods for cases where there are multiple inputs to the function (this saves you from having to use tuples by doing it for you)
    public static TOutput ProcessOutput<T1, T2, TOutput>(T1 input1, T2 input2, TOutput output, [CallerMemberName] string funcName = "")
        => ProcessOutput((input1, input2), output, funcName);
    public static TOutput ProcessOutput<T1, T2, T3, TOutput>(T1 input1, T2 input2, T3 input3, TOutput output, [CallerMemberName] string funcName = "")
        => ProcessOutput((input1, input2, input3), output, funcName);
    public static TOutput ProcessOutput<T1, T2, T3, T4, TOutput>(T1 input1, T2 input2, T3 input3, T4 input4, TOutput output, [CallerMemberName] string funcName = "")
        => ProcessOutput((input1, input2, input3, input4), output, funcName);



    //---------------------------------------------------------- General use Extension Methods --------------------------------------------------------------
    //Converts an array of Types into a single generic ValueTuple type
    private static Type ToValueTupleType(this Type[] types)
    {
        if (types.Length <= 7)
        {
            Type tupleType = types.Length switch
            {
                1 => typeof(ValueTuple<>),
                2 => typeof(ValueTuple<,>),
                3 => typeof(ValueTuple<,,>),
                4 => typeof(ValueTuple<,,,>),
                5 => typeof(ValueTuple<,,,,>),
                6 => typeof(ValueTuple<,,,,,>),
                7 => typeof(ValueTuple<,,,,,,>),
                _ => throw new InvalidOperationException()
            };

            return tupleType.MakeGenericType(types);
        }

        var first = types.Take(7).ToList();
        var rest = types.Skip(7).ToArray();

        Type restTuple = ToValueTupleType(rest);

        first.Add(restTuple);

        return typeof(ValueTuple<,,,,,,,>)
            .MakeGenericType(first.ToArray());
    }
}