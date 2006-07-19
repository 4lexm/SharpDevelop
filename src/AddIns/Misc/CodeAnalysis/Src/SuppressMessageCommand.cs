﻿/*
 * Created by SharpDevelop.
 * User: Daniel Grunwald
 * Date: 19.07.2006
 * Time: 17:57
 */

using System;
using System.Collections.Generic;
using System.Text;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Refactoring;
using ICSharpCode.TextEditor.Document;
using Ast = ICSharpCode.NRefactory.Parser.Ast;

namespace ICSharpCode.CodeAnalysis
{
	public class SuppressMessageCommand : AbstractMenuCommand
	{
		public override void Run()
		{
			TaskView view = (TaskView)Owner;
			foreach (Task t in new List<Task>(view.SelectedTasks)) {
				FxCopTaskTag tag = t.Tag as FxCopTaskTag;
				if (tag == null)
					continue;
				CodeGenerator codegen = tag.ProjectContent.Language.CodeGenerator;
				if (codegen == null)
					continue;
				Position p = tag.ProjectContent.GetPosition(tag.MemberName);
				if (p == null || p.Cu == null || p.Cu.FileName == null || p.Line <= 0)
					continue;
				IWorkbenchWindow window = FileService.OpenFile(p.Cu.FileName);
				if (window == null)
					continue;
				ITextEditorControlProvider provider = window.ViewContent as ITextEditorControlProvider;
				if (provider == null)
					continue;
				IDocument document = provider.TextEditorControl.Document;
				if (p.Line >= document.TotalNumberOfLines)
					continue;
				LineSegment line = document.GetLineSegment(p.Line - 1);
				StringBuilder indentation = new StringBuilder();
				for (int i = line.Offset; i < document.TextLength; i++) {
					char c = document.GetCharAt(i);
					if (c == ' ' || c == '\t')
						indentation.Append(c);
					else
						break;
				}
				string code = codegen.GenerateCode(CreateSuppressAttribute(p.Cu, tag), indentation.ToString());
				if (!code.EndsWith("\n")) code += Environment.NewLine;
				document.Insert(line.Offset, code);
				TaskService.Remove(t);
				ParserService.ParseViewContent(window.ViewContent);
			}
		}
		
		const string NamespaceName = "System.Diagnostics.CodeAnalysis";
		const string AttributeName = "SuppressMessage";
		
		static Ast.AbstractNode CreateSuppressAttribute(ICompilationUnit cu, FxCopTaskTag tag)
		{
			//System.Diagnostics.CodeAnalysis.SuppressMessageAttribute
			bool importedCodeAnalysis = CheckImports(cu.ProjectContent.DefaultImports);
			if (importedCodeAnalysis == false) {
				foreach (IUsing u in cu.Usings) {
					if (CheckImports(u)) {
						importedCodeAnalysis = true;
						break;
					}
				}
			}
			
			// [SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters", MessageId:="fileIdentifier"]
			Ast.Attribute a = new Ast.Attribute(importedCodeAnalysis ? AttributeName : NamespaceName + "." + AttributeName, null, null);
			a.PositionalArguments.Add(new Ast.PrimitiveExpression(tag.Category, tag.Category));
			a.PositionalArguments.Add(new Ast.PrimitiveExpression(tag.CheckID, tag.CheckID));
			if (tag.MessageID != null) {
				a.NamedArguments.Add(new Ast.NamedArgumentExpression("MessageId",
				                                                     new Ast.PrimitiveExpression(tag.MessageID, tag.MessageID)));
			}
			
			Ast.AttributeSection sec = new Ast.AttributeSection(null, null);
			sec.Attributes.Add(a);
			return sec;
		}
		
		static bool CheckImports(IUsing u)
		{
			if (u == null)
				return false;
			else
				return u.Usings.Contains(NamespaceName);
		}
	}
}
