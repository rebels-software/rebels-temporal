// Copyright (C) 2025 Rebels Software
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Rebels.Temporal.SourceGenerator;

[Generator]
public class TemporalMatcherGenerator : IIncrementalGenerator
{
    private static class Diagnostics
    {
        public static readonly DiagnosticDescriptor REBEL005_AnchorToleranceNotConstant = new(
            id: "REBEL005",
            title: "AnchorTolerance must be a compile-time constant",
            messageFormat: "Policy '{0}' has non-constant AnchorTolerance. Policy properties must be compile-time constants for source generation to work correctly.",
            category: "Rebels.Temporal.SourceGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The AnchorTolerance property must return a compile-time constant expression. Use TimeTolerance.None, TimeTolerance.Symmetric(TimeSpan.FromSeconds(constant)), or new TimeTolerance(TimeSpan.FromX(constant), TimeSpan.FromY(constant)).");

        public static readonly DiagnosticDescriptor REBEL006_CandidateToleranceNotConstant = new(
            id: "REBEL006",
            title: "CandidateTolerance must be a compile-time constant",
            messageFormat: "Policy '{0}' has non-constant CandidateTolerance. Policy properties must be compile-time constants for source generation to work correctly.",
            category: "Rebels.Temporal.SourceGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The CandidateTolerance property must return a compile-time constant expression. Use TimeTolerance.None, TimeTolerance.Symmetric(TimeSpan.FromSeconds(constant)), or new TimeTolerance(TimeSpan.FromX(constant), TimeSpan.FromY(constant)).");

        public static readonly DiagnosticDescriptor REBEL007_AllowedRelationsNotConstant = new(
            id: "REBEL007",
            title: "AllowedTemporalRelations must be a compile-time constant",
            messageFormat: "Policy '{0}' has non-constant AllowedTemporalRelations. Policy properties must be compile-time constants for source generation to work correctly.",
            category: "Rebels.Temporal.SourceGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The AllowedTemporalRelations property must return a compile-time constant expression. Use AllowedRelations enum values (e.g., AllowedRelations.Any, AllowedRelations.Overlaps | AllowedRelations.During).");

        public static readonly DiagnosticDescriptor REBEL008_InputOrderingNotConstant = new(
            id: "REBEL008",
            title: "InputOrdering must be a compile-time constant",
            messageFormat: "Policy '{0}' has non-constant InputOrdering. Policy properties must be compile-time constants for source generation to work correctly.",
            category: "Rebels.Temporal.SourceGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "The InputOrdering property must return a compile-time constant expression. Use InputOrdering enum values (InputOrdering.None, InputOrdering.Candidates, or InputOrdering.Both).");

        public static readonly DiagnosticDescriptor REBEL009_TimeSpanNotConstant = new(
            id: "REBEL009",
            title: "TimeSpan value must be a compile-time constant",
            messageFormat: "Policy '{0}' property '{1}' uses non-constant TimeSpan value. Use TimeSpan.FromSeconds(constant) or similar factory methods with constant arguments.",
            category: "Rebels.Temporal.SourceGenerator",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "TimeSpan values in policy properties must be created from compile-time constant expressions.");
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all invocations of TemporalMatcher<TPolicy>.Match* methods
        var invocationProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsTemporalMatcherInvocation(node),
                transform: static (ctx, _) => GetMatchInvocationInfo(ctx))
            .Where(static info => info.HasValue)
            .Select(static (info, _) => info!.Value);

        var compilation = context.CompilationProvider.Combine(invocationProvider.Collect());

        context.RegisterSourceOutput(compilation, (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsTemporalMatcherInvocation(SyntaxNode node)
    {
        if (node is not InvocationExpressionSyntax invocation)
            return false;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        return memberAccess.Expression is GenericNameSyntax genericName &&
               genericName.Identifier.Text == "TemporalMatcher";
    }

    private static MatchInvocationInfo? GetMatchInvocationInfo(GeneratorSyntaxContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var operation = context.SemanticModel.GetOperation(invocation);

        if (operation is not IInvocationOperation invocationOp)
            return null;

        var method = invocationOp.TargetMethod;
        var containingType = method.ContainingType as INamedTypeSymbol;

        if (containingType?.Name != "TemporalMatcher" || !containingType.IsGenericType)
            return null;

        if (containingType.ContainingNamespace?.ToDisplayString() != "Rebels.Temporal")
            return null;

        var policyType = containingType.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
        if (policyType == null)
            return null;

        var matchType = method.Name switch
        {
            "MatchPointToPoint" => MatchMethodType.PointToPoint,
            "MatchPointToInterval" => MatchMethodType.PointToInterval,
            "MatchIntervalToPoint" => MatchMethodType.IntervalToPoint,
            "MatchIntervalToInterval" => MatchMethodType.IntervalToInterval,
            _ => (MatchMethodType?)null
        };

        if (matchType == null)
            return null;

        return new MatchInvocationInfo(policyType, matchType.Value);
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<MatchInvocationInfo> invocations,
        SourceProductionContext context)
    {
        if (invocations.IsEmpty)
            return;

        // Group by policy only
        var byPolicy = invocations
            .Where(i => i.PolicyType != null)
            .GroupBy(i => i.PolicyType, SymbolEqualityComparer.Default)
            .ToList();

        foreach (var policyGroup in byPolicy)
        {
            var policyType = (INamedTypeSymbol)policyGroup.Key!;
            bool validationFailed = false;

            var anchorTolerance = ExtractToleranceInfo(policyType, "AnchorTolerance", compilation, context, ref validationFailed);
            var candidateTolerance = ExtractToleranceInfo(policyType, "CandidateTolerance", compilation, context, ref validationFailed);
            var allowedRelations = ExtractAllowedRelations(policyType, "AllowedTemporalRelations", compilation, context, ref validationFailed);
            var inputOrdering = ExtractInputOrdering(policyType, "InputOrdering", compilation, context, ref validationFailed);

            // Only generate if validation passed
            if (!validationFailed)
            {
                var policyConfig = new PolicyConfiguration(
                    anchorTolerance,
                    candidateTolerance,
                    allowedRelations,
                    inputOrdering);

                var source = GeneratePartialClass(policyType, policyGroup.ToList(), policyConfig);
                var fileName = $"TemporalMatcher.{policyType.Name}.g.cs";

                context.AddSource(fileName, source);
            }
        }
    }

    private static ToleranceInfo ExtractToleranceInfo(
        INamedTypeSymbol policyType,
        string propertyName,
        Compilation compilation,
        SourceProductionContext context,
        ref bool validationFailed)
    {
        var property = policyType.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
        if (property == null)
            return new ToleranceInfo(true, "System.TimeSpan.Zero", "System.TimeSpan.Zero");

        var syntaxRef = property.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef == null)
            return new ToleranceInfo(true, "System.TimeSpan.Zero", "System.TimeSpan.Zero");

        var semanticModel = compilation.GetSemanticModel(syntaxRef.SyntaxTree);
        var syntax = syntaxRef.GetSyntax();

        if (syntax is PropertyDeclarationSyntax propDecl)
        {
            ExpressionSyntax? expression = propDecl.ExpressionBody?.Expression;

            if (expression == null && propDecl.AccessorList != null)
            {
                var getter = propDecl.AccessorList.Accessors
                    .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                var returnStmt = getter?.Body?.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault();
                expression = returnStmt?.Expression;
            }

            if (expression != null)
            {
                var operation = semanticModel.GetOperation(expression);

                // Validate constantness
                var validationResult = ValidateToleranceConstantness(operation, policyType, propertyName);
                if (!validationResult.IsValid)
                {
                    var diagnostic = propertyName == "AnchorTolerance"
                        ? Diagnostics.REBEL005_AnchorToleranceNotConstant
                        : Diagnostics.REBEL006_CandidateToleranceNotConstant;

                    context.ReportDiagnostic(Diagnostic.Create(
                        diagnostic,
                        expression.GetLocation(),
                        policyType.ToDisplayString()));

                    validationFailed = true;
                    return new ToleranceInfo(true, "System.TimeSpan.Zero", "System.TimeSpan.Zero");
                }

                return ParseToleranceOperation(operation);
            }
        }

        return new ToleranceInfo(true, "System.TimeSpan.Zero", "System.TimeSpan.Zero");
    }

    private static ToleranceInfo ParseToleranceOperation(IOperation? operation)
    {
        if (operation == null)
            return new ToleranceInfo(true, "System.TimeSpan.Zero", "System.TimeSpan.Zero");

        // Check for TimeTolerance.None
        if (operation is IPropertyReferenceOperation propRef)
        {
            if (propRef.Property.Name == "None" && propRef.Property.ContainingType?.Name == "TimeTolerance")
            {
                return new ToleranceInfo(true, "System.TimeSpan.Zero", "System.TimeSpan.Zero");
            }
        }

        // Check for TimeTolerance.Symmetric(timespan)
        if (operation is IInvocationOperation invocation)
        {
            if (invocation.TargetMethod.Name == "Symmetric" &&
                invocation.TargetMethod.ContainingType?.Name == "TimeTolerance")
            {
                var arg = invocation.Arguments.FirstOrDefault();
                var timeSpan = ExtractTimeSpanValue(arg?.Value);
                return new ToleranceInfo(false, timeSpan, timeSpan);
            }
        }

        // Check for new TimeTolerance(before, after)
        if (operation is IObjectCreationOperation objCreation)
        {
            if (objCreation.Type?.Name == "TimeTolerance" && objCreation.Arguments.Length == 2)
            {
                var before = ExtractTimeSpanValue(objCreation.Arguments[0].Value);
                var after = ExtractTimeSpanValue(objCreation.Arguments[1].Value);
                return new ToleranceInfo(false, before, after);
            }
        }

        return new ToleranceInfo(true, "System.TimeSpan.Zero", "System.TimeSpan.Zero");
    }

    private static string ExtractTimeSpanValue(IOperation? operation)
    {
        if (operation == null)
            return "System.TimeSpan.Zero";

        // Check for TimeSpan.FromSeconds/FromMilliseconds/etc.
        if (operation is IInvocationOperation invocation)
        {
            var method = invocation.TargetMethod;
            if (method.ContainingType?.Name == "TimeSpan" && method.ContainingType.ContainingNamespace?.Name == "System")
            {
                var arg = invocation.Arguments.FirstOrDefault();
                if (arg?.Value?.ConstantValue.HasValue == true)
                {
                    var value = arg.Value.ConstantValue.Value;
                    return $"System.TimeSpan.{method.Name}({value})";
                }
            }
        }

        // Check for TimeSpan.Zero
        if (operation is IPropertyReferenceOperation propRef)
        {
            if (propRef.Property.Name == "Zero" && propRef.Property.ContainingType?.Name == "TimeSpan")
            {
                return "System.TimeSpan.Zero";
            }
        }

        return "System.TimeSpan.Zero";
    }

    private static AllowedRelationsInfo ExtractAllowedRelations(
        INamedTypeSymbol policyType,
        string propertyName,
        Compilation compilation,
        SourceProductionContext context,
        ref bool validationFailed)
    {
        var property = policyType.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
        if (property == null)
            return new AllowedRelationsInfo("AllowedRelations.Any", true);

        var syntaxRef = property.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef == null)
            return new AllowedRelationsInfo("AllowedRelations.Any", true);

        var semanticModel = compilation.GetSemanticModel(syntaxRef.SyntaxTree);
        var syntax = syntaxRef.GetSyntax();

        if (syntax is PropertyDeclarationSyntax propDecl)
        {
            ExpressionSyntax? expression = propDecl.ExpressionBody?.Expression;

            if (expression == null && propDecl.AccessorList != null)
            {
                var getter = propDecl.AccessorList.Accessors
                    .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                var returnStmt = getter?.Body?.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault();
                expression = returnStmt?.Expression;
            }

            if (expression != null)
            {
                var operation = semanticModel.GetOperation(expression);

                // Validate constantness
                var validationResult = ValidateAllowedRelationsConstantness(operation);
                if (!validationResult.IsValid)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.REBEL007_AllowedRelationsNotConstant,
                        expression.GetLocation(),
                        policyType.ToDisplayString()));

                    validationFailed = true;
                    return new AllowedRelationsInfo("AllowedRelations.Any", true);
                }

                return ParseAllowedRelationsOperation(operation);
            }
        }

        return new AllowedRelationsInfo("AllowedRelations.Any", true);
    }

    private static AllowedRelationsInfo ParseAllowedRelationsOperation(IOperation? operation)
    {
        if (operation == null)
            return new AllowedRelationsInfo("AllowedRelations.Any", true);

        // Check for AllowedRelations.Any
        if (operation is IPropertyReferenceOperation propRef)
        {
            if (propRef.Property.Name == "Any" && propRef.Property.ContainingType?.Name == "AllowedRelations")
            {
                return new AllowedRelationsInfo("AllowedRelations.Any", true);
            }
        }

        // Check for field reference (enum value)
        if (operation is IFieldReferenceOperation fieldRef)
        {
            if (fieldRef.Field.ContainingType?.Name == "AllowedRelations")
            {
                var fieldName = fieldRef.Field.Name;
                return new AllowedRelationsInfo($"AllowedRelations.{fieldName}", fieldName == "Any");
            }
        }

        // Check for bitwise OR (combination of relations)
        if (operation is IBinaryOperation binaryOp && binaryOp.OperatorKind == BinaryOperatorKind.Or)
        {
            // Return the expression as-is
            var expressionText = operation.Syntax.ToString();
            return new AllowedRelationsInfo(expressionText, false);
        }

        return new AllowedRelationsInfo("AllowedRelations.Any", true);
    }

    private static InputOrderingInfo ExtractInputOrdering(
        INamedTypeSymbol policyType,
        string propertyName,
        Compilation compilation,
        SourceProductionContext context,
        ref bool validationFailed)
    {
        var property = policyType.GetMembers(propertyName).OfType<IPropertySymbol>().FirstOrDefault();
        if (property == null)
            return InputOrderingInfo.None;

        var syntaxRef = property.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef == null)
            return InputOrderingInfo.None;

        var semanticModel = compilation.GetSemanticModel(syntaxRef.SyntaxTree);
        var syntax = syntaxRef.GetSyntax();

        if (syntax is PropertyDeclarationSyntax propDecl)
        {
            ExpressionSyntax? expression = propDecl.ExpressionBody?.Expression;

            if (expression == null && propDecl.AccessorList != null)
            {
                var getter = propDecl.AccessorList.Accessors
                    .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                var returnStmt = getter?.Body?.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault();
                expression = returnStmt?.Expression;
            }

            if (expression != null)
            {
                var operation = semanticModel.GetOperation(expression);

                // Validate constantness
                var validationResult = ValidateInputOrderingConstantness(operation);
                if (!validationResult.IsValid)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        Diagnostics.REBEL008_InputOrderingNotConstant,
                        expression.GetLocation(),
                        policyType.ToDisplayString()));

                    validationFailed = true;
                    return InputOrderingInfo.None;
                }

                return ParseInputOrderingOperation(operation);
            }
        }

        return InputOrderingInfo.None;
    }

    private static InputOrderingInfo ParseInputOrderingOperation(IOperation? operation)
    {
        if (operation == null)
            return InputOrderingInfo.None;

        if (operation is IFieldReferenceOperation fieldRef)
        {
            if (fieldRef.Field.ContainingType?.Name == "InputOrdering")
            {
                return fieldRef.Field.Name switch
                {
                    "None" => InputOrderingInfo.None,
                    "Candidates" => InputOrderingInfo.Candidates,
                    "Both" => InputOrderingInfo.Both,
                    _ => InputOrderingInfo.None
                };
            }
        }

        return InputOrderingInfo.None;
    }

    private readonly struct ValidationResult
    {
        public ValidationResult(bool isValid, string? errorMessage = null)
        {
            IsValid = isValid;
            ErrorMessage = errorMessage;
        }

        public bool IsValid { get; }
        public string? ErrorMessage { get; }
    }

    private static ValidationResult ValidateToleranceConstantness(
        IOperation? operation,
        INamedTypeSymbol policyType,
        string propertyName)
    {
        if (operation == null)
            return new ValidationResult(false, "Operation is null");

        // Case 1: TimeTolerance.None (static property)
        if (operation is IPropertyReferenceOperation propRef)
        {
            if (propRef.Property.Name == "None" &&
                propRef.Property.ContainingType?.Name == "TimeTolerance" &&
                propRef.Property.IsStatic)
            {
                return new ValidationResult(true);
            }
            return new ValidationResult(false, "Non-constant property reference");
        }

        // Case 2: TimeTolerance.Symmetric(timespan) or new TimeTolerance(before, after)
        if (operation is IInvocationOperation invocation)
        {
            // Validate Symmetric factory method
            if (invocation.TargetMethod.Name == "Symmetric" &&
                invocation.TargetMethod.ContainingType?.Name == "TimeTolerance")
            {
                var arg = invocation.Arguments.FirstOrDefault();
                return ValidateTimeSpanConstantness(arg?.Value);
            }

            return new ValidationResult(false, "Non-constant invocation");
        }

        // Case 3: new TimeTolerance(before, after)
        if (operation is IObjectCreationOperation objCreation)
        {
            if (objCreation.Type?.Name == "TimeTolerance" && objCreation.Arguments.Length == 2)
            {
                var beforeResult = ValidateTimeSpanConstantness(objCreation.Arguments[0].Value);
                if (!beforeResult.IsValid) return beforeResult;

                var afterResult = ValidateTimeSpanConstantness(objCreation.Arguments[1].Value);
                if (!afterResult.IsValid) return afterResult;

                return new ValidationResult(true);
            }

            return new ValidationResult(false, "Non-constant object creation");
        }

        return new ValidationResult(false, "Unrecognized operation type");
    }

    private static ValidationResult ValidateTimeSpanConstantness(IOperation? operation)
    {
        if (operation == null)
            return new ValidationResult(false, "TimeSpan operation is null");

        // Case 1: TimeSpan.Zero, TimeSpan.MinValue, etc.
        if (operation is IPropertyReferenceOperation propRef)
        {
            if (propRef.Property.ContainingType?.Name == "TimeSpan" &&
                propRef.Property.ContainingType.ContainingNamespace?.Name == "System" &&
                propRef.Property.IsStatic)
            {
                return new ValidationResult(true);
            }
        }

        // Case 2: TimeSpan.FromSeconds(constant), etc.
        if (operation is IInvocationOperation invocation)
        {
            var method = invocation.TargetMethod;
            if (method.ContainingType?.Name == "TimeSpan" &&
                method.ContainingType.ContainingNamespace?.Name == "System" &&
                method.Name.StartsWith("From"))
            {
                // Validate that argument is constant
                var arg = invocation.Arguments.FirstOrDefault();
                if (arg?.Value?.ConstantValue.HasValue == true)
                {
                    return new ValidationResult(true);
                }
                return new ValidationResult(false, "TimeSpan factory method argument is not constant");
            }
        }

        // Case 3: Direct constant (rare but valid)
        if (operation.ConstantValue.HasValue)
        {
            return new ValidationResult(true);
        }

        return new ValidationResult(false, "TimeSpan is not a compile-time constant");
    }

    private static ValidationResult ValidateAllowedRelationsConstantness(IOperation? operation)
    {
        if (operation == null)
            return new ValidationResult(false, "Operation is null");

        // Case 1: AllowedRelations.Any (static property)
        if (operation is IPropertyReferenceOperation propRef)
        {
            if (propRef.Property.ContainingType?.Name == "AllowedRelations")
            {
                return new ValidationResult(true);
            }
        }

        // Case 2: Single enum value (AllowedRelations.Overlaps)
        if (operation is IFieldReferenceOperation fieldRef)
        {
            if (fieldRef.Field.ContainingType?.Name == "AllowedRelations" &&
                (fieldRef.Field.HasConstantValue || fieldRef.Field.IsStatic))
            {
                return new ValidationResult(true);
            }
        }

        // Case 3: Bitwise OR combination
        if (operation is IBinaryOperation binaryOp && binaryOp.OperatorKind == BinaryOperatorKind.Or)
        {
            var leftResult = ValidateAllowedRelationsConstantness(binaryOp.LeftOperand);
            if (!leftResult.IsValid) return leftResult;

            var rightResult = ValidateAllowedRelationsConstantness(binaryOp.RightOperand);
            if (!rightResult.IsValid) return rightResult;

            return new ValidationResult(true);
        }

        // Case 4: Direct constant value
        if (operation.ConstantValue.HasValue)
        {
            return new ValidationResult(true);
        }

        return new ValidationResult(false, "AllowedRelations is not a compile-time constant");
    }

    private static ValidationResult ValidateInputOrderingConstantness(IOperation? operation)
    {
        if (operation == null)
            return new ValidationResult(false, "Operation is null");

        // InputOrdering is a simple enum - must be field reference
        if (operation is IFieldReferenceOperation fieldRef)
        {
            if (fieldRef.Field.ContainingType?.Name == "InputOrdering" &&
                (fieldRef.Field.HasConstantValue || fieldRef.Field.IsStatic))
            {
                return new ValidationResult(true);
            }
        }

        // Or direct constant
        if (operation.ConstantValue.HasValue)
        {
            return new ValidationResult(true);
        }

        return new ValidationResult(false, "InputOrdering is not a compile-time constant");
    }

    private static string GeneratePartialClass(
        INamedTypeSymbol policyType,
        List<MatchInvocationInfo> invocations,
        PolicyConfiguration config)
    {
        var sb = new StringBuilder();
        var year = System.DateTime.UtcNow.Year;

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine($"// Copyright (C) {year} Rebels Software");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("namespace Rebels.Temporal;");
        sb.AppendLine();
        sb.AppendLine("public static partial class TemporalMatcher<TPolicy>");
        sb.AppendLine("    where TPolicy : IMatchPolicy");
        sb.AppendLine("{");

        // Generate implementations for each unique method type
        var methodTypes = invocations.Select(i => i.MethodType).Distinct().ToList();

        foreach (var methodType in methodTypes)
        {
            switch (methodType)
            {
                case MatchMethodType.PointToPoint:
                    GeneratePointToPointMethod(sb, config);
                    break;
                case MatchMethodType.PointToInterval:
                    GeneratePointToIntervalMethod(sb, config);
                    break;
                case MatchMethodType.IntervalToPoint:
                    GenerateIntervalToPointMethod(sb, config);
                    break;
                case MatchMethodType.IntervalToInterval:
                    GenerateIntervalToIntervalMethod(sb, config);
                    break;
            }
        }

        // Add helper methods if needed
        var needsHelpers = methodTypes.Any(m =>
            m != MatchMethodType.PointToPoint ||
            !config.AnchorTolerance.IsExact ||
            !config.CandidateTolerance.IsExact);

        if (needsHelpers)
        {
            GenerateHelperMethods(sb, config.AllowedRelations);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GeneratePointToPointMethod(StringBuilder sb, PolicyConfiguration config)
    {
        bool anchorHasTolerance = !config.AnchorTolerance.IsExact;
        bool candidateHasTolerance = !config.CandidateTolerance.IsExact;

        if (!anchorHasTolerance && !candidateHasTolerance)
        {
            GeneratePointToPointExact(sb, config);
        }
        else if (anchorHasTolerance && !candidateHasTolerance)
        {
            GeneratePointToPointAnchorWindow(sb, config);
        }
        else if (!anchorHasTolerance && candidateHasTolerance)
        {
            GeneratePointToPointCandidateWindow(sb, config);
        }
        else // Both have tolerance
        {
            GeneratePointToPointBothWindows(sb, config);
        }
    }

    private static void GeneratePointToPointExact(StringBuilder sb, PolicyConfiguration config)
    {
        sb.AppendLine("    static partial void MatchPointToPointGenerated<TAnchor, TCandidate>(");
        sb.AppendLine("        System.ReadOnlySpan<TAnchor> anchors,");
        sb.AppendLine("        System.ReadOnlySpan<TCandidate> candidates,");
        sb.AppendLine("        IPairMatchVisitor<TAnchor, TCandidate> visitor)");
        sb.AppendLine("        where TAnchor : ITemporalPoint");
        sb.AppendLine("        where TCandidate : ITemporalPoint");
        sb.AppendLine("    {");

        switch (config.InputOrdering)
        {
            case InputOrderingInfo.None:
                GenerateExactNestedLoops(sb);
                break;
            case InputOrderingInfo.Candidates:
                GenerateExactBinarySearch(sb);
                break;
            case InputOrderingInfo.Both:
                GenerateExactDualPointer(sb);
                break;
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateExactNestedLoops(StringBuilder sb)
    {
        sb.AppendLine("        // Exact matching with unsorted inputs (O(n*m) nested loops)");
        sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            var anchor = anchors[i];");
        sb.AppendLine("            var anchorTime = anchor.At;");
        sb.AppendLine("            var hasMatch = false;");
        sb.AppendLine();
        sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
        sb.AppendLine("            {");
        sb.AppendLine("                var candidate = candidates[j];");
        sb.AppendLine("                var candidateTime = candidate.At;");
        sb.AppendLine();
        sb.AppendLine("                if (anchorTime == candidateTime)");
        sb.AppendLine("                {");
        sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
        sb.AppendLine("                        anchor, candidate, MatchType.PointExact);");
        sb.AppendLine("                    visitor.OnMatch(in pair);");
        sb.AppendLine("                    hasMatch = true;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (!hasMatch)");
        sb.AppendLine("                visitor.OnMiss(anchor);");
        sb.AppendLine("        }");
    }

    private static void GenerateExactBinarySearch(StringBuilder sb)
    {
        sb.AppendLine("        // Exact matching with sorted candidates (O(n log m) binary search)");
        sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            var anchor = anchors[i];");
        sb.AppendLine("            var anchorTime = anchor.At;");
        sb.AppendLine("            var hasMatch = false;");
        sb.AppendLine();
        sb.AppendLine("            // Binary search for first match");
        sb.AppendLine("            int left = 0;");
        sb.AppendLine("            int right = candidates.Length - 1;");
        sb.AppendLine("            int firstMatch = -1;");
        sb.AppendLine();
        sb.AppendLine("            while (left <= right)");
        sb.AppendLine("            {");
        sb.AppendLine("                int mid = left + (right - left) / 2;");
        sb.AppendLine("                var candidateTime = candidates[mid].At;");
        sb.AppendLine();
        sb.AppendLine("                if (candidateTime == anchorTime)");
        sb.AppendLine("                {");
        sb.AppendLine("                    firstMatch = mid;");
        sb.AppendLine("                    right = mid - 1; // Continue searching left");
        sb.AppendLine("                }");
        sb.AppendLine("                else if (candidateTime < anchorTime)");
        sb.AppendLine("                {");
        sb.AppendLine("                    left = mid + 1;");
        sb.AppendLine("                }");
        sb.AppendLine("                else");
        sb.AppendLine("                {");
        sb.AppendLine("                    right = mid - 1;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            // Collect all matches (timestamps may not be unique)");
        sb.AppendLine("            if (firstMatch >= 0)");
        sb.AppendLine("            {");
        sb.AppendLine("                int j = firstMatch;");
        sb.AppendLine("                while (j < candidates.Length && candidates[j].At == anchorTime)");
        sb.AppendLine("                {");
        sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
        sb.AppendLine("                        anchor, candidates[j], MatchType.PointExact);");
        sb.AppendLine("                    visitor.OnMatch(in pair);");
        sb.AppendLine("                    hasMatch = true;");
        sb.AppendLine("                    j++;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (!hasMatch)");
        sb.AppendLine("                visitor.OnMiss(anchor);");
        sb.AppendLine("        }");
    }

    private static void GenerateExactDualPointer(StringBuilder sb)
    {
        sb.AppendLine("        // Exact matching with both sorted (O(n+m) dual-pointer scan)");
        sb.AppendLine("        int candidateIndex = 0;");
        sb.AppendLine();
        sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            var anchor = anchors[i];");
        sb.AppendLine("            var anchorTime = anchor.At;");
        sb.AppendLine("            var hasMatch = false;");
        sb.AppendLine();
        sb.AppendLine("            // Advance candidate pointer to anchor time");
        sb.AppendLine("            while (candidateIndex < candidates.Length &&");
        sb.AppendLine("                   candidates[candidateIndex].At < anchorTime)");
        sb.AppendLine("            {");
        sb.AppendLine("                candidateIndex++;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            // Collect all matches at this timestamp");
        sb.AppendLine("            int j = candidateIndex;");
        sb.AppendLine("            while (j < candidates.Length && candidates[j].At == anchorTime)");
        sb.AppendLine("            {");
        sb.AppendLine("                var pair = new MatchPair<TAnchor, TCandidate>(");
        sb.AppendLine("                    anchor, candidates[j], MatchType.PointExact);");
        sb.AppendLine("                visitor.OnMatch(in pair);");
        sb.AppendLine("                hasMatch = true;");
        sb.AppendLine("                j++;");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (!hasMatch)");
        sb.AppendLine("                visitor.OnMiss(anchor);");
        sb.AppendLine("        }");
    }

    private static void GeneratePointToPointAnchorWindow(StringBuilder sb, PolicyConfiguration config)
    {
        sb.AppendLine("    static partial void MatchPointToPointGenerated<TAnchor, TCandidate>(");
        sb.AppendLine("        System.ReadOnlySpan<TAnchor> anchors,");
        sb.AppendLine("        System.ReadOnlySpan<TCandidate> candidates,");
        sb.AppendLine("        IPairMatchVisitor<TAnchor, TCandidate> visitor)");
        sb.AppendLine("        where TAnchor : ITemporalPoint");
        sb.AppendLine("        where TCandidate : ITemporalPoint");
        sb.AppendLine("    {");
        sb.AppendLine($"        var anchorBefore = {config.AnchorTolerance.Before};");
        sb.AppendLine($"        var anchorAfter = {config.AnchorTolerance.After};");

        if (config.InputOrdering == InputOrderingInfo.Both)
        {
            sb.AppendLine("        int candidateIndex = 0;");
        }
        sb.AppendLine();

        sb.AppendLine("        // Anchor window matching");
        sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            var anchor = anchors[i];");
        sb.AppendLine("            var anchorTime = anchor.At;");
        sb.AppendLine("            var hasMatch = false;");
        sb.AppendLine();
        sb.AppendLine("            var windowStart = anchorTime - anchorBefore;");
        sb.AppendLine("            var windowEnd = anchorTime + anchorAfter;");
        sb.AppendLine();

        if (config.InputOrdering == InputOrderingInfo.None)
        {
            sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
            sb.AppendLine("            {");
            sb.AppendLine("                var candidate = candidates[j];");
            sb.AppendLine("                var candidateTime = candidate.At;");
            sb.AppendLine();
            sb.AppendLine("                if (candidateTime >= windowStart && candidateTime <= windowEnd)");
            sb.AppendLine("                {");
            sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
            sb.AppendLine("                        anchor, candidate, MatchType.PointInInterval);");
            sb.AppendLine("                    visitor.OnMatch(in pair);");
            sb.AppendLine("                    hasMatch = true;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        else if (config.InputOrdering == InputOrderingInfo.Candidates)
        {
            sb.AppendLine("            // Binary search for window start");
            sb.AppendLine("            int left = 0, right = candidates.Length - 1;");
            sb.AppendLine("            int firstInWindow = -1;");
            sb.AppendLine();
            sb.AppendLine("            while (left <= right)");
            sb.AppendLine("            {");
            sb.AppendLine("                int mid = left + (right - left) / 2;");
            sb.AppendLine("                var candidateTime = candidates[mid].At;");
            sb.AppendLine();
            sb.AppendLine("                if (candidateTime >= windowStart)");
            sb.AppendLine("                {");
            sb.AppendLine("                    firstInWindow = mid;");
            sb.AppendLine("                    right = mid - 1;");
            sb.AppendLine("                }");
            sb.AppendLine("                else");
            sb.AppendLine("                {");
            sb.AppendLine("                    left = mid + 1;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            // Scan forward through window");
            sb.AppendLine("            if (firstInWindow >= 0)");
            sb.AppendLine("            {");
            sb.AppendLine("                int j = firstInWindow;");
            sb.AppendLine("                while (j < candidates.Length && candidates[j].At <= windowEnd)");
            sb.AppendLine("                {");
            sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
            sb.AppendLine("                        anchor, candidates[j], MatchType.PointInInterval);");
            sb.AppendLine("                    visitor.OnMatch(in pair);");
            sb.AppendLine("                    hasMatch = true;");
            sb.AppendLine("                    j++;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
        }
        else // Both
        {
            sb.AppendLine("            // Dual-pointer optimization");
            sb.AppendLine("            while (candidateIndex < candidates.Length &&");
            sb.AppendLine("                   candidates[candidateIndex].At < windowStart)");
            sb.AppendLine("            {");
            sb.AppendLine("                candidateIndex++;");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            int j = candidateIndex;");
            sb.AppendLine("            while (j < candidates.Length && candidates[j].At <= windowEnd)");
            sb.AppendLine("            {");
            sb.AppendLine("                var pair = new MatchPair<TAnchor, TCandidate>(");
            sb.AppendLine("                    anchor, candidates[j], MatchType.PointInInterval);");
            sb.AppendLine("                visitor.OnMatch(in pair);");
            sb.AppendLine("                hasMatch = true;");
            sb.AppendLine("                j++;");
            sb.AppendLine("            }");
        }

        sb.AppendLine();
        sb.AppendLine("            if (!hasMatch)");
        sb.AppendLine("                visitor.OnMiss(anchor);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GeneratePointToPointCandidateWindow(StringBuilder sb, PolicyConfiguration config)
    {
        sb.AppendLine("    static partial void MatchPointToPointGenerated<TAnchor, TCandidate>(");
        sb.AppendLine("        System.ReadOnlySpan<TAnchor> anchors,");
        sb.AppendLine("        System.ReadOnlySpan<TCandidate> candidates,");
        sb.AppendLine("        IPairMatchVisitor<TAnchor, TCandidate> visitor)");
        sb.AppendLine("        where TAnchor : ITemporalPoint");
        sb.AppendLine("        where TCandidate : ITemporalPoint");
        sb.AppendLine("    {");
        sb.AppendLine($"        var candidateBefore = {config.CandidateTolerance.Before};");
        sb.AppendLine($"        var candidateAfter = {config.CandidateTolerance.After};");
        sb.AppendLine();

        sb.AppendLine("        // Candidate window matching");
        sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            var anchor = anchors[i];");
        sb.AppendLine("            var anchorTime = anchor.At;");
        sb.AppendLine("            var hasMatch = false;");
        sb.AppendLine();
        sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
        sb.AppendLine("            {");
        sb.AppendLine("                var candidate = candidates[j];");
        sb.AppendLine("                var candidateTime = candidate.At;");
        sb.AppendLine();
        sb.AppendLine("                var windowStart = candidateTime - candidateBefore;");
        sb.AppendLine("                var windowEnd = candidateTime + candidateAfter;");
        sb.AppendLine();
        sb.AppendLine("                if (anchorTime >= windowStart && anchorTime <= windowEnd)");
        sb.AppendLine("                {");
        sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
        sb.AppendLine("                        anchor, candidate, MatchType.PointInInterval);");
        sb.AppendLine("                    visitor.OnMatch(in pair);");
        sb.AppendLine("                    hasMatch = true;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (!hasMatch)");
        sb.AppendLine("                visitor.OnMiss(anchor);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GeneratePointToPointBothWindows(StringBuilder sb, PolicyConfiguration config)
    {
        sb.AppendLine("    static partial void MatchPointToPointGenerated<TAnchor, TCandidate>(");
        sb.AppendLine("        System.ReadOnlySpan<TAnchor> anchors,");
        sb.AppendLine("        System.ReadOnlySpan<TCandidate> candidates,");
        sb.AppendLine("        IPairMatchVisitor<TAnchor, TCandidate> visitor)");
        sb.AppendLine("        where TAnchor : ITemporalPoint");
        sb.AppendLine("        where TCandidate : ITemporalPoint");
        sb.AppendLine("    {");
        sb.AppendLine($"        var anchorBefore = {config.AnchorTolerance.Before};");
        sb.AppendLine($"        var anchorAfter = {config.AnchorTolerance.After};");
        sb.AppendLine($"        var candidateBefore = {config.CandidateTolerance.Before};");
        sb.AppendLine($"        var candidateAfter = {config.CandidateTolerance.After};");
        sb.AppendLine($"        var allowedRelations = {config.AllowedRelations.Expression};");
        sb.AppendLine();

        sb.AppendLine("        // Both have tolerance - use interval matching with Allen's relations");
        sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            var anchor = anchors[i];");
        sb.AppendLine("            var anchorTime = anchor.At;");
        sb.AppendLine("            var hasMatch = false;");
        sb.AppendLine();
        sb.AppendLine("            var anchorStart = anchorTime - anchorBefore;");
        sb.AppendLine("            var anchorEnd = anchorTime + anchorAfter;");
        sb.AppendLine();
        sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
        sb.AppendLine("            {");
        sb.AppendLine("                var candidate = candidates[j];");
        sb.AppendLine("                var candidateTime = candidate.At;");
        sb.AppendLine();
        sb.AppendLine("                var candidateStart = candidateTime - candidateBefore;");
        sb.AppendLine("                var candidateEnd = candidateTime + candidateAfter;");
        sb.AppendLine();
        sb.AppendLine("                var relation = DetermineAllenRelation(");
        sb.AppendLine("                    anchorStart, anchorEnd, candidateStart, candidateEnd);");
        sb.AppendLine();
        sb.AppendLine("                if (IsRelationAllowed(relation, allowedRelations))");
        sb.AppendLine("                {");
        sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
        sb.AppendLine("                        anchor, candidate, MatchType.Interval, relation);");
        sb.AppendLine("                    visitor.OnMatch(in pair);");
        sb.AppendLine("                    hasMatch = true;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (!hasMatch)");
        sb.AppendLine("                visitor.OnMiss(anchor);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GeneratePointToIntervalMethod(StringBuilder sb, PolicyConfiguration config)
    {
        sb.AppendLine("    static partial void MatchPointToIntervalGenerated<TAnchor, TCandidate>(");
        sb.AppendLine("        System.ReadOnlySpan<TAnchor> anchors,");
        sb.AppendLine("        System.ReadOnlySpan<TCandidate> candidates,");
        sb.AppendLine("        IPairMatchVisitor<TAnchor, TCandidate> visitor)");
        sb.AppendLine("        where TAnchor : ITemporalPoint");
        sb.AppendLine("        where TCandidate : ITemporalInterval");
        sb.AppendLine("    {");

        bool anchorHasTolerance = !config.AnchorTolerance.IsExact;

        if (!anchorHasTolerance)
        {
            // Simple point-in-interval check
            sb.AppendLine("        // Point-to-interval matching without anchor tolerance");
            sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
            sb.AppendLine("        {");
            sb.AppendLine("            var anchor = anchors[i];");
            sb.AppendLine("            var anchorTime = anchor.At;");
            sb.AppendLine("            var hasMatch = false;");
            sb.AppendLine();
            sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
            sb.AppendLine("            {");
            sb.AppendLine("                var candidate = candidates[j];");
            sb.AppendLine();
            sb.AppendLine("                if (anchorTime >= candidate.Start && anchorTime <= candidate.End)");
            sb.AppendLine("                {");
            sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
            sb.AppendLine("                        anchor, candidate, MatchType.PointInInterval);");
            sb.AppendLine("                    visitor.OnMatch(in pair);");
            sb.AppendLine("                    hasMatch = true;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (!hasMatch)");
            sb.AppendLine("                visitor.OnMiss(anchor);");
            sb.AppendLine("        }");
        }
        else
        {
            // Convert anchor point to interval and use Allen's relations
            sb.AppendLine($"        var anchorBefore = {config.AnchorTolerance.Before};");
            sb.AppendLine($"        var anchorAfter = {config.AnchorTolerance.After};");
            sb.AppendLine($"        var allowedRelations = {config.AllowedRelations.Expression};");
            sb.AppendLine();
            sb.AppendLine("        // Point-to-interval with anchor tolerance (interval matching)");
            sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
            sb.AppendLine("        {");
            sb.AppendLine("            var anchor = anchors[i];");
            sb.AppendLine("            var anchorTime = anchor.At;");
            sb.AppendLine("            var hasMatch = false;");
            sb.AppendLine();
            sb.AppendLine("            var anchorStart = anchorTime - anchorBefore;");
            sb.AppendLine("            var anchorEnd = anchorTime + anchorAfter;");
            sb.AppendLine();
            sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
            sb.AppendLine("            {");
            sb.AppendLine("                var candidate = candidates[j];");
            sb.AppendLine();
            sb.AppendLine("                var relation = DetermineAllenRelation(");
            sb.AppendLine("                    anchorStart, anchorEnd, candidate.Start, candidate.End);");
            sb.AppendLine();
            sb.AppendLine("                if (IsRelationAllowed(relation, allowedRelations))");
            sb.AppendLine("                {");
            sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
            sb.AppendLine("                        anchor, candidate, MatchType.Interval, relation);");
            sb.AppendLine("                    visitor.OnMatch(in pair);");
            sb.AppendLine("                    hasMatch = true;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (!hasMatch)");
            sb.AppendLine("                visitor.OnMiss(anchor);");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateIntervalToPointMethod(StringBuilder sb, PolicyConfiguration config)
    {
        sb.AppendLine("    static partial void MatchIntervalToPointGenerated<TAnchor, TCandidate>(");
        sb.AppendLine("        System.ReadOnlySpan<TAnchor> anchors,");
        sb.AppendLine("        System.ReadOnlySpan<TCandidate> candidates,");
        sb.AppendLine("        IPairMatchVisitor<TAnchor, TCandidate> visitor)");
        sb.AppendLine("        where TAnchor : ITemporalInterval");
        sb.AppendLine("        where TCandidate : ITemporalPoint");
        sb.AppendLine("    {");

        bool candidateHasTolerance = !config.CandidateTolerance.IsExact;

        if (!candidateHasTolerance)
        {
            // Simple point-in-interval check
            sb.AppendLine("        // Interval-to-point matching without candidate tolerance");
            sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
            sb.AppendLine("        {");
            sb.AppendLine("            var anchor = anchors[i];");
            sb.AppendLine("            var hasMatch = false;");
            sb.AppendLine();
            sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
            sb.AppendLine("            {");
            sb.AppendLine("                var candidate = candidates[j];");
            sb.AppendLine("                var candidateTime = candidate.At;");
            sb.AppendLine();
            sb.AppendLine("                if (candidateTime >= anchor.Start && candidateTime <= anchor.End)");
            sb.AppendLine("                {");
            sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
            sb.AppendLine("                        anchor, candidate, MatchType.PointInInterval);");
            sb.AppendLine("                    visitor.OnMatch(in pair);");
            sb.AppendLine("                    hasMatch = true;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (!hasMatch)");
            sb.AppendLine("                visitor.OnMiss(anchor);");
            sb.AppendLine("        }");
        }
        else
        {
            // Convert candidate point to interval and use Allen's relations
            sb.AppendLine($"        var candidateBefore = {config.CandidateTolerance.Before};");
            sb.AppendLine($"        var candidateAfter = {config.CandidateTolerance.After};");
            sb.AppendLine($"        var allowedRelations = {config.AllowedRelations.Expression};");
            sb.AppendLine();
            sb.AppendLine("        // Interval-to-point with candidate tolerance (interval matching)");
            sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
            sb.AppendLine("        {");
            sb.AppendLine("            var anchor = anchors[i];");
            sb.AppendLine("            var hasMatch = false;");
            sb.AppendLine();
            sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
            sb.AppendLine("            {");
            sb.AppendLine("                var candidate = candidates[j];");
            sb.AppendLine("                var candidateTime = candidate.At;");
            sb.AppendLine();
            sb.AppendLine("                var candidateStart = candidateTime - candidateBefore;");
            sb.AppendLine("                var candidateEnd = candidateTime + candidateAfter;");
            sb.AppendLine();
            sb.AppendLine("                var relation = DetermineAllenRelation(");
            sb.AppendLine("                    anchor.Start, anchor.End, candidateStart, candidateEnd);");
            sb.AppendLine();
            sb.AppendLine("                if (IsRelationAllowed(relation, allowedRelations))");
            sb.AppendLine("                {");
            sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
            sb.AppendLine("                        anchor, candidate, MatchType.Interval, relation);");
            sb.AppendLine("                    visitor.OnMatch(in pair);");
            sb.AppendLine("                    hasMatch = true;");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            if (!hasMatch)");
            sb.AppendLine("                visitor.OnMiss(anchor);");
            sb.AppendLine("        }");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateIntervalToIntervalMethod(StringBuilder sb, PolicyConfiguration config)
    {
        sb.AppendLine("    static partial void MatchIntervalToIntervalGenerated<TAnchor, TCandidate>(");
        sb.AppendLine("        System.ReadOnlySpan<TAnchor> anchors,");
        sb.AppendLine("        System.ReadOnlySpan<TCandidate> candidates,");
        sb.AppendLine("        IPairMatchVisitor<TAnchor, TCandidate> visitor)");
        sb.AppendLine("        where TAnchor : ITemporalInterval");
        sb.AppendLine("        where TCandidate : ITemporalInterval");
        sb.AppendLine("    {");
        sb.AppendLine($"        var allowedRelations = {config.AllowedRelations.Expression};");
        sb.AppendLine();
        sb.AppendLine("        // Interval-to-interval matching with Allen's relations");
        sb.AppendLine("        for (int i = 0; i < anchors.Length; i++)");
        sb.AppendLine("        {");
        sb.AppendLine("            var anchor = anchors[i];");
        sb.AppendLine("            var hasMatch = false;");
        sb.AppendLine();
        sb.AppendLine("            for (int j = 0; j < candidates.Length; j++)");
        sb.AppendLine("            {");
        sb.AppendLine("                var candidate = candidates[j];");
        sb.AppendLine();
        sb.AppendLine("                var relation = DetermineAllenRelation(");
        sb.AppendLine("                    anchor.Start, anchor.End, candidate.Start, candidate.End);");
        sb.AppendLine();
        sb.AppendLine("                if (IsRelationAllowed(relation, allowedRelations))");
        sb.AppendLine("                {");
        sb.AppendLine("                    var pair = new MatchPair<TAnchor, TCandidate>(");
        sb.AppendLine("                        anchor, candidate, MatchType.Interval, relation);");
        sb.AppendLine("                    visitor.OnMatch(in pair);");
        sb.AppendLine("                    hasMatch = true;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            if (!hasMatch)");
        sb.AppendLine("                visitor.OnMiss(anchor);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void GenerateHelperMethods(StringBuilder sb, AllowedRelationsInfo allowedRelations)
    {
        sb.AppendLine("    #region Relation Filtering Helper Methods");
        sb.AppendLine();

        // IsRelationAllowed method - policy-specific optimization
        sb.AppendLine("    private static bool IsRelationAllowed(");
        sb.AppendLine("        TemporalRelation relation,");
        sb.AppendLine("        AllowedRelations allowedRelations)");
        sb.AppendLine("    {");

        if (allowedRelations.IsAny)
        {
            // Optimization: if Any is allowed, skip the check entirely
            sb.AppendLine("        // All relations allowed - skip check");
            sb.AppendLine("        return true;");
        }
        else
        {
            sb.AppendLine("        var relationFlag = relation switch");
            sb.AppendLine("        {");
            sb.AppendLine("            TemporalRelation.Before => AllowedRelations.Before,");
            sb.AppendLine("            TemporalRelation.Meets => AllowedRelations.Meets,");
            sb.AppendLine("            TemporalRelation.Overlaps => AllowedRelations.Overlaps,");
            sb.AppendLine("            TemporalRelation.Starts => AllowedRelations.Starts,");
            sb.AppendLine("            TemporalRelation.During => AllowedRelations.During,");
            sb.AppendLine("            TemporalRelation.Finishes => AllowedRelations.Finishes,");
            sb.AppendLine("            TemporalRelation.Equal => AllowedRelations.Equal,");
            sb.AppendLine("            TemporalRelation.After => AllowedRelations.After,");
            sb.AppendLine("            TemporalRelation.MetBy => AllowedRelations.MetBy,");
            sb.AppendLine("            TemporalRelation.OverlappedBy => AllowedRelations.OverlappedBy,");
            sb.AppendLine("            TemporalRelation.StartedBy => AllowedRelations.StartedBy,");
            sb.AppendLine("            TemporalRelation.Contains => AllowedRelations.Contains,");
            sb.AppendLine("            TemporalRelation.FinishedBy => AllowedRelations.FinishedBy,");
            sb.AppendLine("            _ => throw new System.ArgumentOutOfRangeException(nameof(relation))");
            sb.AppendLine("        };");
            sb.AppendLine();
            sb.AppendLine("        return (allowedRelations & relationFlag) != 0;");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    #endregion");
    }

    private readonly struct MatchInvocationInfo
    {
        public MatchInvocationInfo(INamedTypeSymbol policyType, MatchMethodType methodType)
        {
            PolicyType = policyType;
            MethodType = methodType;
        }

        public INamedTypeSymbol PolicyType { get; }
        public MatchMethodType MethodType { get; }
    }

    private enum MatchMethodType
    {
        PointToPoint,
        PointToInterval,
        IntervalToPoint,
        IntervalToInterval
    }

    private readonly struct PolicyConfiguration
    {
        public PolicyConfiguration(
            ToleranceInfo anchorTolerance,
            ToleranceInfo candidateTolerance,
            AllowedRelationsInfo allowedRelations,
            InputOrderingInfo inputOrdering)
        {
            AnchorTolerance = anchorTolerance;
            CandidateTolerance = candidateTolerance;
            AllowedRelations = allowedRelations;
            InputOrdering = inputOrdering;
        }

        public ToleranceInfo AnchorTolerance { get; }
        public ToleranceInfo CandidateTolerance { get; }
        public AllowedRelationsInfo AllowedRelations { get; }
        public InputOrderingInfo InputOrdering { get; }
    }

    private readonly struct ToleranceInfo
    {
        public ToleranceInfo(bool isExact, string before, string after)
        {
            IsExact = isExact;
            Before = before;
            After = after;
        }

        public bool IsExact { get; }
        public string Before { get; }
        public string After { get; }
    }

    private readonly struct AllowedRelationsInfo
    {
        public AllowedRelationsInfo(string expression, bool isAny)
        {
            Expression = expression;
            IsAny = isAny;
        }

        public string Expression { get; }
        public bool IsAny { get; }
    }

    private enum InputOrderingInfo
    {
        None,
        Candidates,
        Both
    }
}
