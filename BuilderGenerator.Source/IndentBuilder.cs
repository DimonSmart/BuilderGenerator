using System;
using System.Text;

namespace BuilderGenerator.Source
{
    public partial class BuilderIncrementalGenerator
    {
        class IndentBuilder : IDisposable
        {
            private readonly StringBuilder _sb;
            private int _indentLevel;
            private const int IndentSize = 4;

            public IndentBuilder(StringBuilder sb, int initialIndent = 0)
            {
                _sb = sb;
                _indentLevel = initialIndent;
            }

            public IDisposable Indent(string line)
            {
                AppendLine(line);
                AppendLine("{");
                _indentLevel++;
                return this;
            }

            public void AppendLine(string line = "")
            {
                _sb.AppendLine($"{new string(' ', _indentLevel * IndentSize)}{line}");
            }

            public void Dispose()
            {
                _indentLevel--;
                AppendLine("}");
            }
        }
    }
}
