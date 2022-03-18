using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace UnityAnalyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class DebugLogAnalyzer : DiagnosticAnalyzer
	{
		internal static readonly DiagnosticDescriptor Rule = new(
			"DebugLogAnalyzer",
			"Sample DebugLog Analyzer",
			"This is a sample: {0}",
			"Sample Analyzer",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: false);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
		
		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
		}

		private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			if (context.Compilation.AssemblyName.Contains("Unity")) return;
			var invocationExpression = (InvocationExpressionSyntax)context.Node;
			var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
			if (memberAccessExpression?.Name.ToString() == "Log")
			{
				var memberSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;

				var containingType = memberSymbol?.ContainingType;
				if (containingType?.ContainingNamespace.Name == "UnityEngine" && containingType.Name == "Debug")
				{
					var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(),
						memberAccessExpression.ToString());
					context.ReportDiagnostic(diagnostic);
				}
			}
		}
	}
}