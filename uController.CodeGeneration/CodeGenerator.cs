﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;

namespace uController.CodeGeneration
{
    public class CodeGenerator
    {
        private readonly HttpModel _model;
        private readonly StringBuilder _codeBuilder = new StringBuilder();
        private readonly MetadataLoadContext _metadataLoadContext;
        private int _indent;

        public CodeGenerator(HttpModel model, MetadataLoadContext metadataLoadContext)
        {
            _model = model;
            _metadataLoadContext = metadataLoadContext;
        }

        // Resolve the type in the current metadata load context
        private Type T(Type type) => _metadataLoadContext.LoadFromAssemblyName(type.Assembly.FullName).GetType(type.FullName);

        // Pretty print the type name
        private string S(Type type) => TypeNameHelper.GetTypeDisplayName(type);

        public void Indent()
        {
            _indent++;
        }

        public void Unindent()
        {
            _indent--;
        }

        public string Generate()
        {
            var className = $"{_model.HandlerType.Name}_Generated";
            WriteLine("using Microsoft.AspNetCore.Builder;");
            WriteLine("using Microsoft.Extensions.DependencyInjection;");
            WriteLine("// This assembly attribute is part of the generated code to help register the routes");
            WriteLine($"[assembly: {S(typeof(EndpointRouteProviderAttribute))}(typeof({_model.HandlerType.Namespace}.{className}))]");
            WriteLine($"namespace {_model.HandlerType.Namespace}");
            WriteLine("{");
            Indent();
            WriteLine($"public class {className} : {S(typeof(IEndpointRouteProvider))}");
            WriteLine("{");
            Indent();
            var ctors = _model.HandlerType.GetConstructors();
            if (ctors.Length > 1 || ctors[0].GetParameters().Length > 0)
            {
                WriteLine($"private readonly {S(typeof(ObjectFactory))} _factory;");
                WriteLine($"public {className}()");
                WriteLine("{");
                Indent();
                WriteLine($"_factory = {S(typeof(ActivatorUtilities))}.CreateFactory(typeof({S(_model.HandlerType)}), Type.EmptyTypes);");
                Unindent();
                WriteLine("}");
                WriteLine("");
            }

            foreach (var method in _model.Methods)
            {
                Generate(method);
            }
            GenerateRoutes();
            Unindent();
            WriteLine("}");
            Unindent();
            WriteLine("}");

            return _codeBuilder.ToString();
        }

        private void GenerateRoutes()
        {
            // void IEndpointRouteProvider.MapRoutes(IEndpointRouteBuilder routes)
            WriteLine($"{S(typeof(void))} {S(typeof(IEndpointRouteProvider))}.MapRoutes({S(typeof(IEndpointRouteBuilder))} routes)");
            WriteLine("{");
            Indent();
            foreach (var method in _model.Methods)
            {
                Write($"routes.Map(\"{method.RoutePattern.RawText}\", {method.MethodInfo.Name})");
                bool first = true;
                foreach (CustomAttributeData metadata in method.Metadata)
                {
                    if (first)
                    {
                        WriteNoIndent($".WithMetadata(");
                    }
                    else
                    {
                        WriteNoIndent(", ");
                    }

                    WriteNoIndent($"new {S(metadata.AttributeType)}()");
                    first = false;
                }
                WriteLineNoIndent(");");
            }
            Unindent();
            WriteLine("}");
        }

        private void Generate(MethodModel method)
        {
            // [DebuggerStepThrough]
            WriteLine($"[{S(typeof(DebuggerStepThroughAttribute))}]");
            WriteLine($"public async {S(typeof(Task))} {method.MethodInfo.Name}({S(typeof(HttpContext))} httpContext)");
            WriteLine("{");
            Indent();
            var ctors = _model.HandlerType.GetConstructors();
            if (ctors.Length > 1 || ctors[0].GetParameters().Length > 0)
            {
                // Lazy, defer to DI system if
                WriteLine($"var handler = ({S(_model.HandlerType)})_factory(httpContext.RequestServices);");
            }
            else
            {
                WriteLine($"var handler = new {S(_model.HandlerType)}();");
            }

            // Declare locals
            foreach (var parameter in method.Parameters)
            {
                if (parameter.ParameterType == T(typeof(HttpContext)))
                {
                    WriteLine($"var {parameter.Name} = httpContext;");
                }
                else if (parameter.ParameterType == T(typeof(IFormCollection)))
                {
                    WriteLine($"var {parameter.Name} = await httpContext.Request.ReadFormAsync();");
                }
                else if (parameter.FromRoute != null)
                {
                    GenerateConvert(parameter.Name, parameter.ParameterType, parameter.FromRoute, "httpContext.Request.RouteValues", nullable: true);
                }
                else if (parameter.FromQuery != null)
                {
                    GenerateConvert(parameter.Name, parameter.ParameterType, parameter.FromQuery, "httpContext.Request.Query");
                }
                else if (parameter.FromHeader != null)
                {
                    GenerateConvert(parameter.Name, parameter.ParameterType, parameter.FromRoute, "httpContext.Request.Headers");
                }
                else if (parameter.FromServices)
                {
                    WriteLine($"var {parameter.Name} = httpContext.RequestServices.GetRequiredService<{S(parameter.ParameterType)}>();");
                }
                else if (parameter.FromForm != null)
                {
                    WriteLine($"var formCollection = await httpContext.Request.ReadFormAsync();");
                    WriteLine($"var {parameter.Name} = formCollection[{parameter.FromForm}]");
                }
                else if (parameter.FromBody)
                {
                    WriteLine($"var reader = httpContext.RequestServices.GetRequiredService<{S(typeof(IHttpRequestReader))}>();");
                    WriteLine($"var {parameter.Name} = ({S(parameter.ParameterType)})await reader.ReadAsync(httpContext, typeof({S(parameter.ParameterType)}));");
                }
            }

            AwaitableInfo awaitableInfo = default;
            // Populate locals
            if (method.MethodInfo.ReturnType == T(typeof(void)))
            {
                Write("");
            }
            else
            {
                if (AwaitableInfo.IsTypeAwaitable(method.MethodInfo.ReturnType, T, out awaitableInfo))
                {
                    if (awaitableInfo.ResultType == T(typeof(void)))
                    {
                        Write("await ");
                    }
                    else
                    {
                        Write("var result = await ");
                    }
                }
                else
                {
                    Write("var result = ");
                }
            }
            WriteNoIndent($"handler.{method.MethodInfo.Name}(");
            bool first = true;
            foreach (var parameter in method.Parameters)
            {
                if (!first)
                {
                    WriteNoIndent(", ");
                }
                WriteNoIndent(parameter.Name);
                first = false;
            }
            WriteLineNoIndent(");");
            var unwrappedType = awaitableInfo.ResultType ?? method.MethodInfo.ReturnType;
            if (T(typeof(Result)).IsAssignableFrom(unwrappedType))
            {
                WriteLine("await result.ExecuteAsync(httpContext);");
            }
            else if (unwrappedType != T(typeof(void)))
            {
                WriteLine($"await new {T(typeof(ObjectResult))}(result).ExecuteAsync(httpContext);");
            }
            Unindent();
            WriteLine("}");
            WriteLine("");
        }

        private void GenerateConvert(string sourceName, Type type, string key, string sourceExpression, bool nullable = false)
        {
            if (type == T(typeof(string)))
            {
                WriteLine($"var {sourceName} = {sourceExpression}[\"{key}\"]" + (nullable ? "?.ToString();" : ".ToString();"));
            }
            else
            {
                WriteLine($"var {sourceName}Value = {sourceExpression}[\"{key}\"]" + (nullable ? "?.ToString();" : ".ToString();"));

                // TODO: Handle cases where TryParse isn't available

                WriteLine($"if ({sourceName}Value == null || !{S(type)}.TryParse({sourceName}Value, out var {sourceName}))");
                WriteLine("{");
                Indent();
                WriteLine($"{sourceName} = default;");
                Unindent();
                WriteLine("}");
            }
        }

        private void WriteLineNoIndent(string value)
        {
            _codeBuilder.AppendLine(value);
        }

        private void WriteNoIndent(string value)
        {
            _codeBuilder.Append(value);
        }

        private void Write(string value)
        {
            if (_indent > 0)
            {
                _codeBuilder.Append(new string(' ', _indent * 4));
            }
            _codeBuilder.Append(value);
        }

        private void WriteLine(string value)
        {
            if (_indent > 0)
            {
                _codeBuilder.Append(new string(' ', _indent * 4));
            }
            _codeBuilder.AppendLine(value);
        }
    }
}
