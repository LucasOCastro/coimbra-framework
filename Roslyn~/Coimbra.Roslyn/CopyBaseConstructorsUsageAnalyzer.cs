﻿using Coimbra.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Coimbra.Roslyn
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CopyBaseConstructorsUsageAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Diagnostics.CopyBaseConstructorsRequiresPartialKeyword,
                                                                                                           Diagnostics.CopyBaseConstructorsDoesntSupportNestedTypes);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeEventDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeEventDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax
             || classDeclarationSyntax.Modifiers.Any(SyntaxKind.StaticKeyword)
             || context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol typeSymbol
             || !typeSymbol.HasAttribute(CoimbraTypes.CopyBaseConstructorsAttribute, out _, false))
            {
                return;
            }

            if (classDeclarationSyntax.Parent is TypeDeclarationSyntax parentTypeNode)
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.CopyBaseConstructorsDoesntSupportNestedTypes, classDeclarationSyntax.Identifier.GetLocation(), classDeclarationSyntax.GetTypeName(), parentTypeNode.GetTypeName()));
            }

            if (!classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.CopyBaseConstructorsRequiresPartialKeyword, classDeclarationSyntax.Identifier.GetLocation(), classDeclarationSyntax.GetTypeName()));
            }
        }
    }
}
