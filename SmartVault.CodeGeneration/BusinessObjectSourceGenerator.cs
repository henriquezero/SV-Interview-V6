﻿using Microsoft.CodeAnalysis;
using System.Xml.Serialization;
using System.IO;
using System;
using SmartVault.Library;

namespace SmartVault.CodeGeneration
{
    [Generator]
    public class BusinessObjectSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Find the main method
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            // Build up the source code
            for (int i = 0; i < 3; i++)
            {
                var file = context.AdditionalFiles[i];
                var fileContents = file.GetText().ToString();

                var serializer = new XmlSerializer(typeof(BusinessObject));
                using var reader = new StringReader(fileContents);
                var name = Path.GetFileNameWithoutExtension(file.Path);
                var businessObjectModel = (BusinessObject)serializer.Deserialize(reader);

                string propertiesString = "";
                for (int j = 0; j < businessObjectModel.PropertyGroup.Property.Count; j++)
                {
                    var property = businessObjectModel.PropertyGroup.Property[j];
                    propertiesString += string.Format("        public {0} {1} {{ get; set; }}{2}", property.Type, property.Name, Environment.NewLine);
                }

                string businessObjectClassString = $@"// <auto-generated/>
                                                        namespace {mainMethod.ContainingNamespace.ToDisplayString()}.BusinessObjects
                                                        {{
                                                            public partial class {name}
                                                            {{
                                                                {propertiesString}
                                                            }}
                                                        }}
                                                        ";
                context.AddSource($"{name}.generated.cs", businessObjectClassString);


            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this one
        }
    }
}