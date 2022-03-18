using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace UnityAnalyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DisableDateTimeNowAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "DDTN0001";
		private const string Category = "Illegal Method Calls";

		private static DiagnosticDescriptor Rule = new(DiagnosticId, 
			"title", 
			"Datetime error", Category, 
			DiagnosticSeverity.Warning,
			isEnabledByDefault: true, 
			description: "description.");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterCompilationStartAction((compilationStartContext) =>
			{
				var dateTimeType = compilationStartContext.Compilation.GetTypeByMetadataName("System.DateTime");
				compilationStartContext.RegisterSyntaxNodeAction((analysisContext) =>
				{
					var invocations =
						analysisContext.Node.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
					foreach (var invocation in invocations)
					{
						ExpressionSyntax e;
						if (invocation.Expression is MemberAccessExpressionSyntax)
						{
							e = (MemberAccessExpressionSyntax)invocation.Expression;
						}
						else if (invocation.Expression is IdentifierNameSyntax)
						{
							e = (IdentifierNameSyntax)invocation.Expression;
						}
						else
							continue;

						var typeInfo = ModelExtensions.GetTypeInfo(analysisContext.SemanticModel, e).Type as INamedTypeSymbol;
						if (typeInfo?.ConstructedFrom == null)
							continue;

						if (!typeInfo.ConstructedFrom.Equals(dateTimeType, SymbolEqualityComparer.Default))
							continue;
						if (invocation.Name.ToString() == "Now")
						{
							analysisContext.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
						}
					}
				}, SyntaxKind.MethodDeclaration);
			});
		}
	}

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisableDateTimeNowCodeFixProvider)), Shared]
	public class DisableDateTimeNowCodeFixProvider : CodeFixProvider
	{
		private const string title = "Call DateTime.UtcNow rather than DateTime.Now";

		public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DisableDateTimeNowAnalyzer.DiagnosticId);

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
			var text = await document.GetTextAsync(cancellationToken);
			var repl = "DateTime.UtcNow";
			if (Regex.Replace(text.GetSubText(span).ToString(), @"\s+", string.Empty) == "System.DateTime.Now")
				repl = "System.DateTime.UtcNow";
			var newtext = text.Replace(span, repl);
			return document.WithText(newtext);
		}
	}
}