using System;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace DateKit.Benchmarks;

[AttributeUsage(AttributeTargets.Class)]
public class StandardConfigAttribute : Attribute, IConfigSource
{
	public IConfig Config { get; } = ManualConfig.CreateEmpty()
		.AddDiagnoser(
			new DisassemblyDiagnoser(
				new DisassemblyDiagnoserConfig(
					printInstructionAddresses: false)));
}
