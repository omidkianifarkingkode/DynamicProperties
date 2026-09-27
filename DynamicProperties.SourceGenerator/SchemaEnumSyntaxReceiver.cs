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

        public void OnVisitSyntaxNode(
            SyntaxNode syntaxNode)
        {
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