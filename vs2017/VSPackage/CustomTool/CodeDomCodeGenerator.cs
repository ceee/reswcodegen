using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace ChristianHelle.DeveloperTools.CodeGenerators.Resw.VSPackage.CustomTool
{
    public class CodeDomCodeGenerator : CodeGenerator, IDisposable
    {  
        private readonly TypeAttributes? classAccessibility;
        private readonly VisualStudioVersion visualStudioVersion;
        private readonly string className;
        private readonly CodeNamespace codeNamespace;
        private readonly CodeCompileUnit compileUnit;
        private readonly CodeDomProvider provider;

        public CodeDomCodeGenerator(IResourceParser resourceParser,
                                    string className,
                                    string defaultNamespace,
                                    CodeDomProvider codeDomProvider = null,
                                    TypeAttributes? classAccessibility = null,
                                    VisualStudioVersion visualStudioVersion = VisualStudioVersion.VS2012)
            : base(resourceParser, defaultNamespace)
        {
            this.className = className;
            this.classAccessibility = classAccessibility;
            this.visualStudioVersion = visualStudioVersion;
            compileUnit = new CodeCompileUnit();
            provider = codeDomProvider ?? new CSharpCodeProvider();
            codeNamespace = new CodeNamespace(defaultNamespace);
        }

        public override string GenerateCode()
        {
            codeNamespace.Comments.Add(
                new CodeCommentStatement("--------------------------------------------------------------------------------------------------"));
            codeNamespace.Comments.Add(new CodeCommentStatement("<auto-generatedInfo>"));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tThis code was generated by ResW File Code Generator MOD (http://bit.ly/reswcodegen)"));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tResW File Code Generator was written by Christian Resma Helle (modified by Tobias Klika)"));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tand is under GNU General Public License version 2 (GPLv2)"));
            codeNamespace.Comments.Add(new CodeCommentStatement(string.Empty));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tThis code contains a helper class exposing property representations"));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tof the string resources defined in the specified .ResW file"));
            codeNamespace.Comments.Add(new CodeCommentStatement(string.Empty));
            codeNamespace.Comments.Add(new CodeCommentStatement("\tGenerated: " + DateTime.Now.ToString(CultureInfo.InvariantCulture)));
            codeNamespace.Comments.Add(new CodeCommentStatement("</auto-generatedInfo>"));
            codeNamespace.Comments.Add(
                new CodeCommentStatement("--------------------------------------------------------------------------------------------------"));

            codeNamespace.Imports.Add(new CodeNamespaceImport("Windows.ApplicationModel.Resources"));

            var targetClass = new CodeTypeDeclaration(className)
            {
                IsClass = true,
                IsPartial = true, 
                TypeAttributes = (classAccessibility ?? TypeAttributes.Public),
                Attributes = MemberAttributes.Public | MemberAttributes.Static | MemberAttributes.Final
            };

            const string resourceLoaderType = "ResourceLoader";
            var resourceLoaderField = new CodeMemberField(resourceLoaderType, "resourceLoader")
            {
                Attributes = MemberAttributes.Private | MemberAttributes.Static | MemberAttributes.Final
            };
            targetClass.Members.Add(resourceLoaderField);

            var constructor = new CodeTypeConstructor();

            var executingAssemblyVar = new CodeVariableDeclarationStatement(typeof (string), "executingAssemblyName");
            var executingAssemblyInit = new CodeAssignStatement(new CodeVariableReferenceExpression("executingAssemblyName"),
                                                                new CodeSnippetExpression("Windows.UI.Xaml.Application.Current.GetType().AssemblyQualifiedName"));
            var executingAssemblySplit = new CodeVariableDeclarationStatement(typeof (string[]), "executingAssemblySplit");
            var executingAssemblyInit2 = new CodeAssignStatement(new CodeVariableReferenceExpression("executingAssemblySplit"),
                                                                 new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("executingAssemblyName"),
                                                                                                "Split",
                                                                                                new CodePrimitiveExpression(',')));
            var executingAssemblyInit3 = new CodeAssignStatement(new CodeVariableReferenceExpression("executingAssemblyName"),
                                                                 new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("executingAssemblySplit"),
                                                                                                new CodePrimitiveExpression(1)));

            var currentAssemblyVar = new CodeVariableDeclarationStatement(typeof (string), "currentAssemblyName");
            var currentAssemblyInit = new CodeAssignStatement(new CodeVariableReferenceExpression("currentAssemblyName"),
                                                              new CodePropertyReferenceExpression(new CodeTypeOfExpression(className), "AssemblyQualifiedName"));
            var currentAssemblySplit = new CodeVariableDeclarationStatement(typeof (string[]), "currentAssemblySplit");
            var currentAssemblyInit2 = new CodeAssignStatement(new CodeVariableReferenceExpression("currentAssemblySplit"),
                                                               new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("currentAssemblyName"),
                                                                                              "Split",
                                                                                              new CodePrimitiveExpression(',')));
            var currentAssemblyInit3 = new CodeAssignStatement(new CodeVariableReferenceExpression("currentAssemblyName"),
                                                               new CodeArrayIndexerExpression(new CodeVariableReferenceExpression("currentAssemblySplit"),
                                                                                              new CodePrimitiveExpression(1)));

            var createResourceLoader = new CodeConditionStatement(
                new CodeSnippetExpression("executingAssemblyName.Equals(currentAssemblyName)"),
                new CodeStatement[] // true
                {
                    visualStudioVersion == VisualStudioVersion.VS2013
                        ? new CodeAssignStatement(new CodeFieldReferenceExpression(null, "resourceLoader"),
                                                  new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("ResourceLoader"),
                                                                                 "GetForCurrentView",
                                                                                 new CodeSnippetExpression("\"" + className + "\"")))
                        : new CodeAssignStatement(new CodeFieldReferenceExpression(null, "resourceLoader"),
                                                  new CodeObjectCreateExpression(new CodeTypeReference("ResourceLoader"),
                                                                                 new CodeSnippetExpression("\"" + className + "\"")))
                },
                new CodeStatement[] // false
                {
                    visualStudioVersion == VisualStudioVersion.VS2013
                        ? new CodeAssignStatement(new CodeFieldReferenceExpression(null, "resourceLoader"),
                                                  new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("ResourceLoader"),
                                                                                 "GetForCurrentView",
                                                                                 new CodeSnippetExpression("currentAssemblyName + \"/" + className + "\"")))
                        : new CodeAssignStatement(new CodeFieldReferenceExpression(null, "resourceLoader"),
                                                  new CodeObjectCreateExpression(new CodeTypeReference("ResourceLoader"),
                                                                                 new CodeSnippetExpression("currentAssemblyName + \"/" + className + "\"")))
                });

            constructor.Statements.Add(executingAssemblyVar);
            constructor.Statements.Add(executingAssemblyInit);
            constructor.Statements.Add(executingAssemblySplit);
            constructor.Statements.Add(executingAssemblyInit2);
            constructor.Statements.Add(executingAssemblyInit3);
            constructor.Statements.Add(currentAssemblyVar);
            constructor.Statements.Add(currentAssemblyInit);
            constructor.Statements.Add(currentAssemblySplit);
            constructor.Statements.Add(currentAssemblyInit2);
            constructor.Statements.Add(currentAssemblyInit3);
            constructor.Statements.Add(createResourceLoader);

            targetClass.Members.Add(constructor);

            var resources = ResourceParser.Parse();
            var idx = 0;

            StringBuilder resourceKeysStringBuilder = new StringBuilder();
            resourceKeysStringBuilder.Append("new string[] { ");

            foreach (var item in resources)
            {
                if (string.IsNullOrEmpty(item.Name))
                    continue;

                string name = item.Name.Trim();
                string key = item.Name;

                if (name.Contains("."))
                {
                  name = name.Replace(".", "_");
                  key = key.Replace(".", "/");
                }

                var property = new CodeMemberField
                {
                    Name = name,
                    Attributes = MemberAttributes.Public,
                    Type = new CodeTypeReference(typeof (string))
                };

                property.Name += " { get; private set; } //";
                
                resourceKeysStringBuilder.Append($"\"{name}\"");
                if (idx < resources.Count - 1)
                {
                  resourceKeysStringBuilder.Append(", ");
                }

                targetClass.Members.Add(property);

                idx += 1;
            }
      
            resourceKeysStringBuilder.Append(" }");

            var keysField = new CodeMemberField()
            {
                Name = "ResourceKeys",
                Type = new CodeTypeReference(typeof(string[])),
                Attributes = MemberAttributes.Private | MemberAttributes.Final,
                InitExpression = new CodeSnippetExpression(resourceKeysStringBuilder.ToString())
            };

            targetClass.Members.Add(keysField);

            codeNamespace.Types.Add(targetClass);
            compileUnit.Namespaces.Add(codeNamespace);

            return GenerateCodeFromCompileUnit();
        }

        private string GenerateCodeFromCompileUnit()
        {
            var options = new CodeGeneratorOptions {BracingStyle = "C"};

            var code = new StringBuilder();

            using (var writer = new StringWriter(code))
                provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);

            return code.ToString();
        }

        #region IDisposable

        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
        }

        ~CodeDomCodeGenerator()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (disposed)
                return;
            disposed = true;

            if (dispose)
                provider.Dispose();
        }

        #endregion
    }
}