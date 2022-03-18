using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
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
			messageFormat: "This is the message 123",
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
	
	

	[ExportCodeFixProvider(LanguageNames.CSharp)]
	public class EmptyUnityMessageCodeFix : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(EmptyUnityMessageAnalyzer.Rule.Id);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var declaration = await context.GetFixableNodeAsync<MethodDeclarationSyntax>();
			if (declaration == null)
				return;

			context.RegisterCodeFix(
				CodeAction.Create(
					"Remove empty Unity message",
					ct => DeleteEmptyMessageAsync(context.Document, declaration, ct),
					declaration.ToFullString()),
				context.Diagnostics);
		}

		private static async Task<Document> DeleteEmptyMessageAsync(Document document, MethodDeclarationSyntax declaration, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = root.RemoveNode(declaration, SyntaxRemoveOptions.KeepNoTrivia);
			if (newRoot == null)
				return document;

			return document.WithSyntaxRoot(newRoot);
		}
	}
	
	
	internal static class CodeFixContextExtensions
	{
		public static async Task<T?> GetFixableNodeAsync<T>(this CodeFixContext context) where T : SyntaxNode
		{
			return await GetFixableNodeAsync<T>(context, _ => true);
		}

		public static async Task<T?> GetFixableNodeAsync<T>(this CodeFixContext context, Func<T, bool> predicate) where T : SyntaxNode
		{
			var root = await context
				.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			return root?
				.FindNode(context.Span)
				.DescendantNodesAndSelf()
				.OfType<T>()
				.FirstOrDefault(predicate);
		}
	}
}