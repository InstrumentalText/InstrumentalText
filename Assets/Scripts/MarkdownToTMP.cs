using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

public static class MarkdownToTMP
{
    public enum BlockType { Text, Table }

    public class ContentBlock
    {
        public BlockType Type;
        public string RichText;           // for Text blocks
        public string[] Headers;          // for Table blocks
        public List<string[]> Rows;       // for Table blocks
    }

    /// <summary>
    /// Parse markdown into a list of content blocks (text and table).
    /// </summary>
    public static List<ContentBlock> Parse(string markdown)
    {
        var blocks = new List<ContentBlock>();
        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var textBuffer = new StringBuilder();

        int i = 0;
        while (i < lines.Length)
        {
            if (IsTableRow(lines[i]) && i + 1 < lines.Length && IsTableSeparator(lines[i + 1]))
            {
                FlushText(textBuffer, blocks);

                var table = new ContentBlock { Type = BlockType.Table };
                table.Headers = ParseTableRow(lines[i]);
                i += 2; // skip header + separator

                table.Rows = new List<string[]>();
                while (i < lines.Length && IsTableRow(lines[i]))
                {
                    table.Rows.Add(ParseTableRow(lines[i]));
                    i++;
                }
                blocks.Add(table);
                continue;
            }

            textBuffer.AppendLine(ConvertLine(lines[i]));
            i++;
        }

        FlushText(textBuffer, blocks);
        return blocks;
    }

    /// <summary>
    /// Legacy: convert entire markdown to a single rich text string.
    /// </summary>
    public static string Convert(string markdown)
    {
        var blocks = Parse(markdown);
        var sb = new StringBuilder();
        foreach (var block in blocks)
        {
            if (block.Type == BlockType.Text)
            {
                sb.Append(block.RichText);
            }
            else if (block.Type == BlockType.Table)
            {
                sb.AppendLine("<b>" + string.Join("  |  ", block.Headers) + "</b>");
                foreach (var row in block.Rows)
                    sb.AppendLine(string.Join("  |  ", row));
                sb.AppendLine();
            }
        }
        return sb.ToString().TrimEnd();
    }

    private static void FlushText(StringBuilder buffer, List<ContentBlock> blocks)
    {
        if (buffer.Length == 0) return;
        blocks.Add(new ContentBlock
        {
            Type = BlockType.Text,
            RichText = buffer.ToString()
        });
        buffer.Clear();
    }

    private static string ConvertLine(string line)
    {
        line = Regex.Replace(line, @"\s*:contentReference\[.*?\]\{.*?\}", "");

        if (line.StartsWith("### "))
            return $"<size=120%><b>{ProcessInline(line.Substring(4))}</b></size>";
        if (line.StartsWith("## "))
            return $"<size=140%><b>{ProcessInline(line.Substring(3))}</b></size>";
        if (line.StartsWith("# "))
            return $"<size=160%><b>{ProcessInline(line.Substring(2))}</b></size>";

        if (line.StartsWith("- "))
            return $"  \u2022 {ProcessInline(line.Substring(2))}";

        var olMatch = Regex.Match(line, @"^(\d+)\.\s+(.*)");
        if (olMatch.Success)
            return $"  {olMatch.Groups[1].Value}. {ProcessInline(olMatch.Groups[2].Value)}";

        if (string.IsNullOrWhiteSpace(line))
            return "";

        return ProcessInline(line);
    }

    private static string ProcessInline(string text)
    {
        text = Regex.Replace(text, @"\s*:contentReference\[.*?\]\{.*?\}", "");
        text = Regex.Replace(text, @"\*\*(.+?)\*\*", "<b>$1</b>");
        text = Regex.Replace(text, @"__(.+?)__", "<b>$1</b>");
        text = Regex.Replace(text, @"\*(.+?)\*", "<i>$1</i>");
        text = Regex.Replace(text, @"(?<!\w)_(.+?)_(?!\w)", "<i>$1</i>");
        text = Regex.Replace(text, @"`(.+?)`", "<mark=#00000040><font=\"RobotoMono\">$1</font></mark>");
        return text;
    }

    private static bool IsTableRow(string line)
    {
        return line.TrimStart().StartsWith("|") && line.TrimEnd().EndsWith("|");
    }

    private static bool IsTableSeparator(string line)
    {
        return IsTableRow(line) && Regex.IsMatch(line, @"^\|[\s\-:]+(\|[\s\-:]+)*\|$");
    }

    private static string[] ParseTableRow(string line)
    {
        line = line.Trim();
        if (line.StartsWith("|")) line = line.Substring(1);
        if (line.EndsWith("|")) line = line.Substring(0, line.Length - 1);

        var cells = line.Split('|');
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = Regex.Replace(cells[i].Trim(), @"\s*:contentReference\[.*?\]\{.*?\}", "");
            cells[i] = ProcessInline(cells[i]);
        }
        return cells;
    }
}
