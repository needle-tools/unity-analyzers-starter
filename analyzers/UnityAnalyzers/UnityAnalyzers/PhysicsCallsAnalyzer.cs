using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using LanguageNames = Microsoft.CodeAnalysis.LanguageNames;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace UnityAnalyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PhysicsCallsAnalyzer : DiagnosticAnalyzer
	{
		internal static readonly DiagnosticDescriptor Rule = new(
			"SampleRaycastUsage",
			"Physics Raycast call",
			"Use of the '{0}' method: this is a sample",
			"Sample Analyzer",
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
			context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, SyntaxKind.InvocationExpression);
		}

		private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
		{
			if (context.Compilation.AssemblyName.Contains("Unity")) return;
			var invocationExpression = (InvocationExpressionSyntax)context.Node;
			var memberAccessExpression = invocationExpression.Expression as MemberAccessExpressionSyntax;
			if (memberAccessExpression?.Name.ToString() == "Raycast")
			{
				var memberSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpression).Symbol as IMethodSymbol;

				var containingType = memberSymbol?.ContainingType;
				if (containingType?.ContainingNamespace.Name == "UnityEngine" && containingType.Name == "Physics")
				{
					var diagnostic = Diagnostic.Create(Rule, invocationExpression.GetLocation(),
						memberAccessExpression.ToString());
					context.ReportDiagnostic(diagnostic);
				}
			}
		}
	}

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(PhysicsRaycasterCodeFix)), Shared]
	public class PhysicsRaycasterCodeFix : CodeFixProvider
	{
		private const string title = "Physics.Raycast sample fix does nothing!";

		public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(PhysicsCallsAnalyzer.Rule.Id);

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			return Task.Run(() => context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedDocument: c => ReplaceWithUtcNowAsync(context.Document, diagnosticSpan, c),
					equivalenceKey: title),
				diagnostic));
		}

		private async Task<Document> ReplaceWithUtcNowAsync(Document document, TextSpan span, CancellationToken cancellationToken)
		{
			return document;
			// var text = await document.GetTextAsync(cancellationToken);
			// var repl = "DateTime.UtcNow";
			// if (Regex.Replace(text.GetSubText(span).ToString(), @"\s+", string.Empty) == "System.DateTime.Now")
			// 	repl = "System.DateTime.UtcNow";
			// var newtext = text.Replace(span, repl);
			// return document.WithText(newtext);
		}
	}
}