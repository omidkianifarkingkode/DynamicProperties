using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamicProperty.SourceGen
{
    internal sealed class SchemaSyntaxReceiver : ISyntaxReceiver
    {
        public List<InterfaceDeclarationSyntax> InterfaceCandidates { get; } =
            new List<InterfaceDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclaration &&
                interfaceDeclaration.BaseList != null)
            {
                InterfaceCandidates.Add(interfaceDeclaration);
            }
        }
    }
}
