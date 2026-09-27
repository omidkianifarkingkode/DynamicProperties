using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamicProperty.SourceGen
{
    internal sealed class SchemaEnumSyntaxReceiver :
        ISyntaxReceiver
    {
        public List<EnumDeclarationSyntax> Candidates { get; } =
            new List<EnumDeclarationSyntax>();

        public List<InterfaceDeclarationSyntax> InterfaceCandidates { get; } =
            new List<InterfaceDeclarationSyntax>();

        public void OnVisitSyntaxNode(
            SyntaxNode syntaxNode)
        {
            if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclaration)
            {
                if (interfaceDeclaration.BaseList != null ||
                    interfaceDeclaration.AttributeLists.Count > 0 ||
                    interfaceDeclaration.Members
                        .OfType<PropertyDeclarationSyntax>()
                        .Any(property => property.AttributeLists.Count > 0))
                {
                    InterfaceCandidates.Add(interfaceDeclaration);
                }

                return;
            }

            if (!(syntaxNode is EnumDeclarationSyntax enumDeclaration))
                return;

            // Cheap syntax-level pre-filter only.
            //
            // This does NOT decide whether the enum is a schema.
            // Symbol-based validation happens later.
            bool hasAttributedMember =
                enumDeclaration.Members.Any(
                    member =>
                        member.AttributeLists.Count > 0);

            if (!hasAttributedMember)
                return;

            Candidates.Add(enumDeclaration);
        }
    }
}
