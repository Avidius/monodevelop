//
// MSBuildImport.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Xml;


namespace MonoDevelop.Projects.Formats.MSBuild
{
	public class MSBuildImport: MSBuildObject
	{
		string evaluatedProjectPath;
		string target;

		static readonly string [] knownAttributes = { "Project", "Condition", "Label" };

		internal override string [] GetKnownAttributes ()
		{
			return knownAttributes;
		}

		internal override void ReadAttribute (string name, string value)
		{
			if (name == "Project")
				target = value;
			else
				base.ReadAttribute (name, value);
		}

		internal override string WriteAttribute (string name)
		{
			if (name == "Project")
				return target;
			else
				return base.WriteAttribute (name);
		}

		internal override string GetElementName ()
		{
			return "Import";
        }

		public string Project {
			get { return target; }
			set { target = value; NotifyChanged (); }
		}

		public string EvaluatedProject {
			get { return evaluatedProjectPath ?? target; }
		}

		internal void SetEvalResult (string evaluatedProjectPath)
		{
			this.evaluatedProjectPath = evaluatedProjectPath;
		}

		internal override void Write (XmlWriter writer, WriteContext context)
		{
			if (context.Evaluating) {
				var newTarget = MSBuildProjectService.GetImportRedirect (target);
				if (newTarget != null) {
					WritePatchedImport (writer, newTarget);
					return;
				}
			}

			base.Write (writer, context);
		}

		void WritePatchedImport (XmlWriter writer, string newTarget)
		{
			/* If an import redirect exists, add a fake import to the project which will be used only
			   if the original import doesn't exist. That is, the following import:

			   <Import Project = "PathToReplace" />

			   will be converted into:

			   <Import Project = "PathToReplace" Condition = "Exists('PathToReplace')"/>
			   <Import Project = "ReplacementPath" Condition = "!Exists('PathToReplace')" />
			*/

			// Modify the original import by adding a condition, so that this import will be used only
			// if the targets file exists.

			string cond = "Exists('" + target + "')";
			if (!string.IsNullOrEmpty (Condition))
				cond = "( " + Condition + " ) AND " + cond;
			
			writer.WriteStartElement ("Import", MSBuildProject.Schema);
			writer.WriteAttributeString ("Project", target);
			writer.WriteAttributeString ("Condition", cond);

			// Now add the fake import, with a condition so that it will be used only if the original
			// import does not exist.

			cond = "!Exists('" + target + "')";
			if (!string.IsNullOrEmpty (Condition))
				cond = "( " + Condition + " ) AND " + cond;

			writer.WriteStartElement ("Import", MSBuildProject.Schema);
			writer.WriteAttributeString ("Project", MSBuildProjectService.ToMSBuildPath (null, newTarget));
			writer.WriteAttributeString ("Condition", cond);
		}
	}

}
