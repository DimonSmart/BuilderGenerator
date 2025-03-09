﻿using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace BuilderGenerator.Source
{
    [Generator]
    public partial class BuilderIncrementalGenerator : IIncrementalGenerator
    {
        private const string AttributeSource = @"
using System;
namespace BuilderGenerator.Runtime
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateBuilderAttribute : Attribute
    {
        public Type TargetType { get; set; }
    }
}
";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
#endif
            // Добавляем атрибут через пост-инициализацию.
            context.RegisterPostInitializationOutput(ctx =>
            {
                ctx.AddSource("GenerateBuilderAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8));
            });

            var candidateClasses = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, ct) => IsCandidateClass(node),
                    transform: static (ctx, ct) => (ClassDeclarationSyntax)ctx.Node)
                .Where(static classDecl => classDecl is not null);

            IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses =
                context.CompilationProvider.Combine(candidateClasses.Collect());

            context.RegisterSourceOutput(compilationAndClasses, (spc, source) =>
            {
                var (compilation, classes) = source;
                foreach (var classDecl in classes)
                {
                    var model = compilation.GetSemanticModel(classDecl.SyntaxTree);
                    if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
                        continue;

                    var hasAttribute = classSymbol.GetAttributes().Any(ad =>
                        ad.AttributeClass?.ToDisplayString() == "BuilderGenerator.Runtime.GenerateBuilderAttribute" ||
                        ad.AttributeClass?.Name == "GenerateBuilderAttribute");

                    if (!hasAttribute)
                        continue;

                    // Определяем тип для генерации билдера.
                    ITypeSymbol targetType = classSymbol;
                    foreach (var attr in classSymbol.GetAttributes())
                    {
                        if (attr.AttributeClass?.ToDisplayString() == "BuilderGenerator.Runtime.GenerateBuilderAttribute" ||
                            attr.AttributeClass?.Name == "GenerateBuilderAttribute")
                        {
                            foreach (var namedArg in attr.NamedArguments)
                            {
                                if (namedArg.Key == "TargetType" && namedArg.Value.Value is INamedTypeSymbol ts)
                                    targetType = ts;
                            }
                        }
                    }

                    var builderSource = GenerateBuilderClass(compilation, targetType, classSymbol);
                    spc.AddSource($"{classSymbol.Name}Builder.g.cs", SourceText.From(builderSource, Encoding.UTF8));
                }
            });
        }

        private static bool IsCandidateClass(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0;
        }

        private static string GenerateBuilderClass(
       Compilation compilation,
       ITypeSymbol targetType,
       INamedTypeSymbol classSymbol)
        {
            var namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
                ? null
                : classSymbol.ContainingNamespace.ToDisplayString();

            var builderClassName = classSymbol.Name + "Builder";

            var sb = new StringBuilder();

            var builder = new IndentBuilder(sb);

            builder.AppendLine("// <auto-generated />");
            builder.AppendLine("using System;");
            builder.AppendLine();

            if (!string.IsNullOrEmpty(namespaceName))
            {
                using (builder.Indent($"namespace {namespaceName}"))
                {
                    GenerateBuilderClassBody(builder, targetType, builderClassName);
                }
            }
            else
            {
                GenerateBuilderClassBody(builder, targetType, builderClassName);
            }

            return sb.ToString();
        }


        private static void GenerateBuilderClassBody(IndentBuilder builder, ITypeSymbol targetType, string builderClassName)
        {
            var properties = targetType.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
                .ToList();

            using (builder.Indent($"public class {builderClassName}"))
            {
                builder.AppendLine($"private readonly {targetType.ToDisplayString()} _instance = new {targetType.ToDisplayString()}();");
                builder.AppendLine();

                using (builder.Indent($"public static {builderClassName} Create()"))
                {
                    builder.AppendLine($"return new {builderClassName}();");
                }

                foreach (var prop in properties)
                {
                    if (prop.Type.TypeKind == TypeKind.Class && prop.Type.SpecialType != SpecialType.System_String)
                    {
                        var propBuilderClassName = prop.Type.Name + "Builder";
                        using (builder.Indent($"public {builderClassName} {prop.Name}(Action<{propBuilderClassName}<{targetType.ToDisplayString()}>> buildAction)"))
                        {
                            builder.AppendLine($"var builder = new {propBuilderClassName}<{targetType.ToDisplayString()}>(_instance, (parent, child) => child.{targetType.Name} = parent);");
                            builder.AppendLine("buildAction(builder);");
                            builder.AppendLine($"_instance.{prop.Name} = builder.Build();");
                            builder.AppendLine("return this;");
                        }
                    }
                    else
                    {
                        using (builder.Indent($"public {builderClassName} {prop.Name}({prop.Type.ToDisplayString()} value)"))
                        {
                            builder.AppendLine($"_instance.{prop.Name} = value;");
                            builder.AppendLine("return this;");
                        }
                    }
                }

                using (builder.Indent($"public {targetType.ToDisplayString()} Build()"))
                {
                    builder.AppendLine("return _instance;");
                }

                using (builder.Indent($"public static implicit operator {targetType.ToDisplayString()}({builderClassName} builder)"))
                {
                    builder.AppendLine("return builder.Build();");
                }
            }

            using (builder.Indent($"public class {builderClassName}<TParent>"))
            {
                builder.AppendLine($"private readonly {targetType.ToDisplayString()} _instance = new {targetType.ToDisplayString()}();");
                builder.AppendLine("private readonly TParent _parent;");
                builder.AppendLine($"private readonly Action<TParent, {targetType.ToDisplayString()}> _setParentAction;");

                using (builder.Indent($"public {builderClassName}(TParent parent, Action<TParent, {targetType.ToDisplayString()}> setParentAction)"))
                {
                    builder.AppendLine("_parent = parent;");
                    builder.AppendLine("_setParentAction = setParentAction;");
                }

                foreach (var prop in properties)
                {
                    using (builder.Indent($"public {builderClassName}<TParent> {prop.Name}({prop.Type.ToDisplayString()} value)"))
                    {
                        builder.AppendLine($"_instance.{prop.Name} = value;");
                        builder.AppendLine("return this;");
                    }
                }

                using (builder.Indent("public TParent BuildAndSetParent()"))
                {
                    builder.AppendLine("_setParentAction(_parent, _instance);");
                    builder.AppendLine("return _parent;");
                }

                using (builder.Indent($"public {targetType.ToDisplayString()} Build()"))
                {
                    builder.AppendLine("return _instance;");
                }
            }
        }



    }
}
