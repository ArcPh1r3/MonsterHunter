using HG.Reflection;
using MonsterHunter.Content.MonsterParts;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MonsterHunter.Modules
{
    public enum PartIndex
    {
        None = -1,
    }

    // TO-DO: part weight
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RegisterPartAttribute : SearchableAttribute
    {
        [CompilableComment("unique identifier for part type")]
        public string identifier { get; private set; }

        public RegisterPartAttribute(string identifier)
        {
            this.identifier = identifier;
        }
    }

    public static class PartCatalog
    {
        public readonly struct RegisteredPart
        {
            public readonly PartIndex partIndex;
            public readonly string identifier;
            public readonly Type partType;

            internal RegisteredPart(PartIndex partIndex, Type partType, string identifier)
            {
                this.partIndex = partIndex;
                this.identifier = identifier;
                this.partType = partType;
            }
        }

        private static RegisteredPart[] _registeredParts = Array.Empty<RegisteredPart>();
        public static int registeredPartCount = _registeredParts.Length;
        private static Dictionary<Type, PartIndex> _typeToPartIndex = new Dictionary<Type, PartIndex>();
        private static Dictionary<string, PartIndex> _identifierToPartIndex = new Dictionary<string, PartIndex>();
        private static readonly Dictionary<Type, BaseMonsterPart> _partInstances = new Dictionary<Type, BaseMonsterPart>();

        // TO-DO: _partToBodyDict


        // [SystemInitializer]
        public static void Initalize()
        {
            Debug.Log("MonsterHunter.PartCatalog.Initialize : Initializing monster parts");

            List<RegisterPartAttribute> foundParts = new List<RegisterPartAttribute>();
            SearchableAttribute.GetInstances<RegisterPartAttribute>(foundParts);
            Debug.Log("MonsterHunter.PartCatalog.Initialize : Initial scan found " + foundParts.Count + " parts to attempt to register..");

            List<RegisterPartAttribute> validParts = new List<RegisterPartAttribute>();
            Type baseType = typeof(BaseMonsterPart);

            // validate parts
            foreach (RegisterPartAttribute part in foundParts)
            {
                Type type = (Type)part.target;

                if (type.IsAbstract)
                {
                    Debug.LogWarning("MonsterHunter.PartCatalog.Initialize : " + type.FullName + " is Abstract and can't be registered");
                    continue;
                }

                if (!type.IsSubclassOf(baseType))
                {
                    Debug.LogWarning("MonsterHunter.PartCatalog.Initialize : " + type.FullName + " does not derive from BaseMonsterPart and will not be registered");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(part.identifier))
                {
                    Debug.LogWarning("MonsterHunter.PartCatalog.Initialize : " + type.FullName + " has null/whitespace identifier and will not be registered");
                    continue;
                }

                // type.InvokeMethod("Initialize"); apparently only good for static methods
                validParts.Add(part);
            }

            // maybe overkill to fully catalogue these but oh well lets have some fun
            validParts.Sort((a, b) => string.CompareOrdinal(a.identifier, b.identifier));
            Array.Resize(ref _registeredParts, validParts.Count);

            // add each part to catalog + dicts
            for (PartIndex i = 0; i < (PartIndex)validParts.Count; ++i)
            {
                RegisterPartAttribute att = validParts[(int)i];
                Type targetType = att.target as Type;

                RegisteredPart part = new RegisteredPart(i, targetType, att.identifier);
                _registeredParts[(int)i] = part;
                _identifierToPartIndex.Add(att.identifier, i);
                _typeToPartIndex.Add(targetType, i);
            }

            // initialize an instance of each part type to set up their HUD components
            foreach (RegisteredPart part in _registeredParts)
            {
                var instance = (BaseMonsterPart)Activator.CreateInstance(part.partType, nonPublic: true);
                _partInstances.Add(part.partType, instance);
                instance.Initialize();
            }

            Debug.Log("MonsterHunter.PartCatalog.Initialize : Initialized a total of " + _registeredParts.Length + " parts!");
        }

        [ConCommand(commandName = "list_monsterparts", flags = ConVarFlags.None, helpText = "Lists all registered Monster Parts.")]
        private static void CC_ListParts(ConCommandArgs args)
        {
            var parts = new List<PartCatalog.RegisteredPart>();

            PartCatalog.GetRegisteredParts(parts);

            var output = new System.Text.StringBuilder();

            foreach (var part in parts)
            {
                output.AppendLine($"[{(int)part.partIndex}] {part.identifier}");
            }

            args.Log(parts.Count > 0 ? parts.ToString() : "No monster parts registered");
        }

        public static PartIndex FindPartIndex(Type type)
        {
            if (_typeToPartIndex.TryGetValue(type, out var index))
            {
                return index;
            }
            return PartIndex.None;
        }

        public static PartIndex FindPartIndex(string id)
        {
            if (_identifierToPartIndex.TryGetValue(id, out var index))
            {
                return index;
            }
            return PartIndex.None;
        }

        public static void GetRegisteredParts(in List<RegisteredPart> output)
        {
            output.Clear();
            for (int i = 0; i < registeredPartCount; i++) 
            {
                output.Add(_registeredParts[i]);
            }
        }
    }
}
