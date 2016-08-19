﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Pihrtsoft.CodeAnalysis.CSharp.CSharpFactory;

namespace Pihrtsoft.CodeAnalysis.CSharp.Refactorings
{
    internal static class ReplaceConditionalExpressionWithIfElseRefactoring
    {
        private const string Title = "Replace conditional expression with 'if-else'";

        public static void ComputeRefactoring(RefactoringContext context, ConditionalExpressionSyntax expression)
        {
            SyntaxNode parent = expression.Parent;

            if (parent == null)
                return;

            SyntaxKind kind = parent.Kind();

            if (kind == SyntaxKind.ReturnStatement)
            {
                context.RegisterRefactoring(
                    Title,
                    cancellationToken => RefactorAsync(context.Document, expression, (ReturnStatementSyntax)parent, cancellationToken));
            }
            else if (kind == SyntaxKind.YieldReturnStatement)
            {
                context.RegisterRefactoring(
                    Title,
                    cancellationToken => RefactorAsync(context.Document, expression, (YieldStatementSyntax)parent, cancellationToken));
            }
            else if (kind == SyntaxKind.SimpleAssignmentExpression)
            {
                parent = parent.Parent;

                if (parent?.IsKind(SyntaxKind.ExpressionStatement) == true)
                {
                    context.RegisterRefactoring(
                        Title,
                        cancellationToken => RefactorAsync(context.Document, expression, (ExpressionStatementSyntax)parent, cancellationToken));
                }
            }
            else
            {
                LocalDeclarationStatementSyntax localDeclaration = GetLocalDeclaration(expression);

                if (localDeclaration != null)
                {
                    context.RegisterRefactoring(
                        Title,
                        cancellationToken => RefactorAsync(context.Document, expression, localDeclaration, cancellationToken));
                }
            }
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            ReturnStatementSyntax returnStatement,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            IfStatementSyntax ifStatement = ReplaceWithIfElseWithReturn(conditionalExpression, returnStatement)
                .WithFormatterAnnotation();

            SyntaxNode newRoot = root.ReplaceNode(returnStatement, ifStatement);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            YieldStatementSyntax yieldStatement,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            IfStatementSyntax ifStatement = ReplaceWithIfElseWithYieldReturn(conditionalExpression, yieldStatement)
                .WithFormatterAnnotation();

            SyntaxNode newRoot = root.ReplaceNode(yieldStatement, ifStatement);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            ExpressionStatementSyntax expressionStatement,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var assignmentExpression = (AssignmentExpressionSyntax)conditionalExpression.Parent;

            IfStatementSyntax ifStatement = ReplaceWithIfElseWithAssignment(conditionalExpression, assignmentExpression.Left.WithoutTrivia())
                .WithTriviaFrom(expressionStatement)
                .WithFormatterAnnotation();

            SyntaxNode newRoot = root.ReplaceNode(expressionStatement, ifStatement);

            return document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> RefactorAsync(
            Document document,
            ConditionalExpressionSyntax conditionalExpression,
            LocalDeclarationStatementSyntax localDeclaration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SemanticModel semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);

            var block = (BlockSyntax)localDeclaration.Parent;

            LocalDeclarationStatementSyntax newLocalDeclaration = GetNewLocalDeclaration(conditionalExpression, localDeclaration, semanticModel)
                .WithLeadingTrivia(localDeclaration.GetLeadingTrivia())
                .WithFormatterAnnotation();

            var variableDeclarator = (VariableDeclaratorSyntax)conditionalExpression.Parent.Parent;

            IfStatementSyntax ifStatement = ReplaceWithIfElseWithAssignment(conditionalExpression, IdentifierName(variableDeclarator.Identifier.ValueText))
                .WithTrailingTrivia(localDeclaration.GetTrailingTrivia())
                .WithFormatterAnnotation();

            SyntaxList<StatementSyntax> statements = block.Statements;

            statements = statements
                .Replace(localDeclaration, newLocalDeclaration)
                .Insert(statements.IndexOf(localDeclaration) + 1, ifStatement);

            SyntaxNode newRoot = root.ReplaceNode(block, block.WithStatements(statements));

            return document.WithSyntaxRoot(newRoot);
        }

        private static LocalDeclarationStatementSyntax GetNewLocalDeclaration(
            ConditionalExpressionSyntax conditionalExpression,
            LocalDeclarationStatementSyntax localDeclaration,
            SemanticModel semanticModel)
        {
            bool isVar = localDeclaration.Declaration.Type.IsVar;

            localDeclaration = localDeclaration.RemoveNode(
                conditionalExpression.Parent,
                SyntaxRemoveOptions.KeepExteriorTrivia);

            if (isVar)
            {
                ITypeSymbol typeSymbol = semanticModel.GetTypeInfo(conditionalExpression).Type;

                if (typeSymbol?.IsErrorType() == false)
                {
                    localDeclaration = localDeclaration.ReplaceNode(
                        localDeclaration.Declaration.Type,
                        TypeSyntaxRefactoring.CreateTypeSyntax(typeSymbol).WithSimplifierAnnotation());
                }
            }

            return localDeclaration;
        }

        private static IfStatementSyntax ReplaceWithIfElseWithAssignment(
            ConditionalExpressionSyntax conditionalExpression,
            ExpressionSyntax left)
        {
            ExpressionSyntax condition = conditionalExpression.Condition;

            if (condition.IsKind(SyntaxKind.ParenthesizedExpression))
                condition = ((ParenthesizedExpressionSyntax)condition).Expression;

            condition = condition.WithoutTrivia();

            return IfStatement(
                    condition,
                    Block(
                    ExpressionStatement(
                    SimpleAssignmentExpression(
                                left,
                                conditionalExpression.WhenTrue.WithoutTrivia()))),
                    ElseClause(
                        Block(
                            ExpressionStatement(
                                SimpleAssignmentExpression(
                                    left,
                                    conditionalExpression.WhenFalse.WithoutTrivia())))));
        }

        private static IfStatementSyntax ReplaceWithIfElseWithReturn(
            ConditionalExpressionSyntax conditionalExpression,
            ReturnStatementSyntax returnStatement)
        {
            ExpressionSyntax condition = conditionalExpression.Condition.WithoutTrivia();

            if (condition.IsKind(SyntaxKind.ParenthesizedExpression))
                condition = ((ParenthesizedExpressionSyntax)condition).Expression;

            IfStatementSyntax ifStatement = CreateIfStatement(
                condition,
                ReturnStatement(conditionalExpression.WhenTrue.WithoutTrivia()),
                ReturnStatement(conditionalExpression.WhenFalse.WithoutTrivia()));

            return ifStatement.WithTriviaFrom(returnStatement);
        }

        private static IfStatementSyntax ReplaceWithIfElseWithYieldReturn(
            ConditionalExpressionSyntax conditionalExpression,
            YieldStatementSyntax yieldStatement)
        {
            ExpressionSyntax condition = conditionalExpression.Condition.WithoutTrivia();

            if (condition.IsKind(SyntaxKind.ParenthesizedExpression))
                condition = ((ParenthesizedExpressionSyntax)condition).Expression;

            IfStatementSyntax ifStatement = CreateIfStatement(
                condition,
                YieldReturnStatement(conditionalExpression.WhenTrue.WithoutTrivia()),
                YieldReturnStatement(conditionalExpression.WhenFalse.WithoutTrivia()));

            return ifStatement.WithTriviaFrom(yieldStatement);
        }

        private static IfStatementSyntax CreateIfStatement(ExpressionSyntax condition, StatementSyntax trueStatement, StatementSyntax falseStatement)
        {
            return IfStatement(
                condition,
                Block(trueStatement),
                ElseClause(
                    Block(falseStatement)));
        }

        private static LocalDeclarationStatementSyntax GetLocalDeclaration(this ExpressionSyntax expression)
        {
            SyntaxNode parent = expression.Parent;

            if (parent?.IsKind(SyntaxKind.EqualsValueClause) != true)
                return null;

            parent = parent.Parent;

            if (parent?.IsKind(SyntaxKind.VariableDeclarator) != true)
                return null;

            parent = parent.Parent;

            if (parent?.IsKind(SyntaxKind.VariableDeclaration) != true)
                return null;

            parent = parent.Parent;

            if (parent?.IsKind(SyntaxKind.LocalDeclarationStatement) != true)
                return null;

            return (LocalDeclarationStatementSyntax)parent;
        }
    }
}