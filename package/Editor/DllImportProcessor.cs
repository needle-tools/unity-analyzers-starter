using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle.Analyzers
{ 
	public class DllImportProcessor : AssetPostprocessor
	{
		
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			foreach (var imp in importedAssets)
			{
				if (imp.EndsWith(".dll", StringComparison.Ordinal) && imp.StartsWith("Packages/com.needle.analyzers/"))
				{
					var dll = AssetDatabase.LoadAssetAtPath<Object>(imp);
					var labels = AssetDatabase.GetLabels(dll);
					if (!labels.Contains("RoslynAnalyzer"))
					{
						Debug.Log("Add Analyzer label to " + dll);
						var list = labels.ToList();
						list.Add("RoslynAnalyzer");
						labels = list.ToArray();
						AssetDatabase.SetLabels(dll, labels);
					}
				}
			}
		}
	}
}