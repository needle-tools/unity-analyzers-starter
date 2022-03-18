using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using DiagnosticDescriptor = Microsoft.CodeAnalysis.DiagnosticDescriptor;
using DiagnosticSeverity = Microsoft.CodeAnalysis.DiagnosticSeverity;
using LanguageNames = Microsoft.CodeAnalysis.LanguageNames;
using SyntaxKind = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace UnityAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PhysicsCallsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new("InvalidRaycastUsage",
            "Calling Physics.Raycast is not allowed",
            "Use of the '{0}' method is not allowed",
            "Forbidden.Stuff",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "This method is forbidden.");
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
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
}