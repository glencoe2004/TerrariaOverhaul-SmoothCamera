using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria.ModLoader;

namespace TerrariaOverhaul.Core.Configuration;

public sealed partial class ConfigSystem : ModSystem
{
	private class CategoryData
	{
		public readonly Dictionary<string, IConfigEntry> EntriesByName = new();
	}

	private static readonly Dictionary<string, IConfigEntry> entriesByName = new();
	private static readonly Dictionary<string, CategoryData> categoriesByName = new();

	public static IReadOnlyDictionary<string, IConfigEntry> EntriesByName { get; } = new ReadOnlyDictionary<string, IConfigEntry>(entriesByName);

	public override void Load()
	{
		ForceInitializeStaticConstructors();

		foreach (var entry in entriesByName.Values) {
			entry.Initialize(Mod);
		}

		InitializeIO();
		InitializeNetworking();

		LoadConfig();
	}

	private void ForceInitializeStaticConstructors()
	{

		var assembly = Assembly.GetExecutingAssembly();
		string assemblyName = assembly.GetName().Name ?? throw new InvalidOperationException("Executing assembly lacks a 'Name'.");

		foreach (var mod in ModLoader.Mods) {
			var modAssembly = mod.GetType().Assembly;

			if (mod != Mod && !modAssembly.GetReferencedAssemblies().Any(n => n.Name == assemblyName)) {
				continue;
			}

			foreach (var type in modAssembly.GetTypes()) {
				if (type.IsEnum) {
					continue;
				}
			}
		}
	}

	public static void ResetConfig()
	{
		foreach (var entry in entriesByName.Values) {
			entry.LocalValue = entry.DefaultValue;
		}
	}

	internal static void RegisterEntry(IConfigEntry entry)
	{
		entriesByName.Add(entry.Name, entry);

		if (!categoriesByName.TryGetValue(entry.Category, out var category)) {
			categoriesByName[entry.Category] = category = new();
		}

		category.EntriesByName.Add(entry.Name, entry);
	}
}
