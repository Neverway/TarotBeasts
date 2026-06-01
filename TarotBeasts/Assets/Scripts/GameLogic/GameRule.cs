using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;

public abstract class GameRule
{
    private HashSet<GameFuncs.Processor> gameruleProcessors;
    protected bool HasBeenInitialized => gameruleProcessors != null;


    private bool _enabled = false;
    public bool Enabled {
        get => _enabled;
        set
        {
            if (_enabled == value) return;

            if (value) Enable();
            else Disable();
        }
    }

    protected void Initialize()
    {
        gameruleProcessors = new HashSet<GameFuncs.Processor>();
        var methods = GetType().GetMethods();
        foreach (var method in methods)
        {
            var modifierAttribute = method.GetAttribute<FuncModificationAttribute>();
            if (modifierAttribute != null)
                gameruleProcessors.Add(modifierAttribute.CreateProcessor(method.IsStatic ? null : this, method));
        }
    }
    public void Enable()
    {
        _enabled = true;
        if (!HasBeenInitialized) Initialize();
        
        foreach (var processor in gameruleProcessors)
            processor.Register();

        OnEnable();
    }
    public void Disable()
    {
        _enabled = false;
        if (!HasBeenInitialized) Initialize();

        foreach (var processor in gameruleProcessors)
            processor.Unregister();

        OnDisable();
    }

    public virtual void OnEnable() { }
    public virtual void OnDisable() { }

    //---------------------------------------------------------- Modify Func Attributes ---------------------------------------------------------------------
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    protected abstract class FuncModificationAttribute : Attribute
    {
        public readonly string targetFuncName;
        public FuncModificationAttribute(string targetFuncName) { this.targetFuncName = targetFuncName; }
        public abstract GameFuncs.Processor CreateProcessor(object instance, MethodInfo method);
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    protected class ModifyInputOfAttribute : FuncModificationAttribute
    {
        public ModifyInputOfAttribute(string targetFuncName) : base(targetFuncName) { }

        public override GameFuncs.Processor CreateProcessor(object instance, MethodInfo method) 
            => GameFuncs.NewInputProcessor(targetFuncName, instance, method);
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    protected class ModifyOutputOfAttribute : FuncModificationAttribute
    {
        public ModifyOutputOfAttribute(string targetFuncName) : base(targetFuncName) { }
        public override GameFuncs.Processor CreateProcessor(object instance, MethodInfo method) 
            => GameFuncs.NewOutputProcessor(targetFuncName, instance, method);
    }
}