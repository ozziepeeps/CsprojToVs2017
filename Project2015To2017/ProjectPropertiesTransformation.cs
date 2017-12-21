﻿using Project2015To2017.Definition;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Project2015To2017
{
    internal sealed class ProjectPropertiesTransformation : ITransformation
    {
        public Task TransformAsync(XDocument projectFile, DirectoryInfo projectFolder, Project definition)
        {
            XNamespace nsSys = "http://schemas.microsoft.com/developer/msbuild/2003";
            var propertyGroups = projectFile.Element(nsSys + "Project").Elements(nsSys + "PropertyGroup");

			var unconditionalPropertyGroups = propertyGroups.Where(x => x.Attribute("Condition") == null);
			if (unconditionalPropertyGroups == null)
			{
				throw new NotSupportedException("No unconditional property group found. Cannot determine important properties like target framework and others.");
			}
			else
			{
				var targetFramework = unconditionalPropertyGroups.Elements(nsSys + "TargetFrameworkVersion").FirstOrDefault()?.Value;

				definition.Optimize = "true".Equals(unconditionalPropertyGroups.Elements(nsSys + "Optimize").FirstOrDefault()?.Value, StringComparison.OrdinalIgnoreCase);
				definition.TreatWarningsAsErrors = "true".Equals(unconditionalPropertyGroups.Elements(nsSys + "TreatWarningsAsErrors").FirstOrDefault()?.Value, StringComparison.OrdinalIgnoreCase);
				definition.AllowUnsafeBlocks = "true".Equals(unconditionalPropertyGroups.Elements(nsSys + "AllowUnsafeBlocks").FirstOrDefault()?.Value, StringComparison.OrdinalIgnoreCase);

				definition.RootNamespace = unconditionalPropertyGroups.Elements(nsSys + "RootNamespace").FirstOrDefault()?.Value;
				definition.AssemblyName = unconditionalPropertyGroups.Elements(nsSys + "AssemblyName").FirstOrDefault()?.Value;
				definition.Type = unconditionalPropertyGroups.Elements(nsSys + "TestProjectType").Any()
					? ApplicationType.TestProject
					: ToApplicationType(unconditionalPropertyGroups.Elements(nsSys + "OutputType").FirstOrDefault()?.Value);
				if (targetFramework != null)
				{
					definition.TargetFrameworks = new[] { ToTargetFramework(targetFramework) };
				}
			}

            definition.ConditionalPropertyGroups = propertyGroups.Where(x => x.Attribute("Condition") != null).ToArray();

            foreach (var conditionalPropertyGroup in definition.ConditionalPropertyGroups)
            {
                conditionalPropertyGroup.Elements().Where(e => e.Name.LocalName.Contains("CodeContracts")).Remove();
            }

            if (definition.Type == ApplicationType.Unknown)
            {
                throw new NotSupportedException("Unable to parse output type.");
            }


            return Task.CompletedTask;
        }

        private string ToTargetFramework(string targetFramework)
        {
            if (targetFramework.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                return "net" + targetFramework.Substring(1).Replace(".", string.Empty);
            }

            throw new NotSupportedException($"Target framework {targetFramework} is not supported.");
        }

		private ApplicationType ToApplicationType(string outputType)
		{
			if (string.IsNullOrWhiteSpace(outputType))
			{
				return ApplicationType.Unknown;
			}

			switch (outputType.ToLowerInvariant())
			{
				case "exe": return ApplicationType.ConsoleApplication;
				case "library": return ApplicationType.ClassLibrary;
				case "winexe": return ApplicationType.WindowsApplication;
				default: throw new NotSupportedException($"OutputType {outputType} is not supported.");
			}
		}
    }
}
