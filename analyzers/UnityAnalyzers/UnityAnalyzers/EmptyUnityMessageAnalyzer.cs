using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityAnalyzers.Core
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EmptyUnityMessageAnalyzer : DiagnosticAnalyzer
	{
		internal static readonly DiagnosticDescriptor Rule = new(
			id: "UNT0001",
			title: "title",
			messageFormat: "This is the message123",
			category: "Performance",
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "description.");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
		}

		private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
		{
			var method = context.Node as MethodDeclarationSyntax;
			if (method?.Body == null)
				return;

			if (HasPolymorphicModifier(method))
				return;

			if (method.Body.Statements.Count > 0)
				return;

			var classDeclaration = method.FirstAncestorOrSelf<ClassDeclarationSyntax>();
			if (classDeclaration == null)
				return;

			// var typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
			// var scriptInfo = new ScriptInfo(typeSymbol);
			// if (!scriptInfo.HasMessages)
			// 	return;
			//
			var symbol = context.SemanticModel.GetDeclaredSymbol(method);
			// if (!scriptInfo.IsMessage(symbol))
			// 	return;

			context.ReportDiagnostic(Diagnostic.Create(Rule, method.Identifier.GetLocation(), symbol?.Name));
		}

		private static bool HasPolymorphicModifier(MethodDeclarationSyntax method)
		{
			foreach (var modifier in method.Modifiers)
			{
				switch (modifier.Kind())
				{
					case SyntaxKind.AbstractKeyword:
					case SyntaxKind.VirtualKeyword:
					case SyntaxKind.OverrideKeyword:
						return true;
				}
			}

			return false;
		}
	}
}