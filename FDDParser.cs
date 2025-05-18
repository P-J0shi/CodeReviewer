using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace D365CodeReviewer
{
    public class FDDParser
    {
        private string fddPath;
        private string parsedContent;
        
        public List<string> Requirements { get; private set; } = new List<string>();
        public List<FDDFunction> Functions { get; private set; } = new List<FDDFunction>();
        public List<FDDEntity> Entities { get; private set; } = new List<FDDEntity>();
        public List<FDDExtension> Extensions { get; private set; } = new List<FDDExtension>();

        public FDDParser(string fddPath)
        {
            this.fddPath = fddPath;
            
            // Extract file extension
            string extension = Path.GetExtension(fddPath).ToLower();
            
            // Parse based on file type
            switch (extension)
            {
                case ".pdf":
                    ParsePdf();
                    break;
                case ".docx":
                    ParseDocx();
                    break;
                case ".txt":
                case ".md":
                    ParseText();
                    break;
                default:
                    throw new ArgumentException($"Unsupported FDD format: {extension}");
            }
            
            // Extract requirements after parsing
            ExtractRequirements();
        }

        private void ParsePdf()
        {
            StringBuilder text = new StringBuilder();
            
            using (PdfReader reader = new PdfReader(fddPath))
            {
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    string pageText = PdfTextExtractor.GetTextFromPage(reader, page);
                    text.Append(pageText);
                    text.Append(Environment.NewLine);
                }
            }
            
            parsedContent = text.ToString();
        }

        private void ParseDocx()
        {
            StringBuilder text = new StringBuilder();
            
            using (WordprocessingDocument doc = WordprocessingDocument.Open(fddPath, false))
            {
                Body? body = doc.MainDocumentPart?.Document.Body;
                
                if (body != null)
                {
                    // Extract paragraphs
                    foreach (var paragraph in body.Elements<Paragraph>())
                    {
                        text.AppendLine(paragraph.InnerText);
                    }
                    
                    // Extract tables
                    foreach (var table in body.Elements<Table>())
                    {
                        foreach (var row in table.Elements<TableRow>())
                        {
                            foreach (var cell in row.Elements<TableCell>())
                            {
                                text.Append(cell.InnerText);
                                text.Append(" | ");
                            }
                            text.AppendLine();
                        }
                        text.AppendLine();
                    }
                }
            }
            
            parsedContent = text.ToString();
        }

        private void ParseText()
        {
            parsedContent = File.ReadAllText(fddPath);
        }

        private void ExtractRequirements()
        {
            // Common requirement identifiers in FDDs
            List<string> reqPatterns = new List<string>
            {
                @"REQ-\d+\s*:(.+?)(?=REQ-\d+\s*:|$)",
                @"Requirement\s+\d+\s*:(.+?)(?=Requirement\s+\d+\s*:|$)",
                @"R\d+\s*:(.+?)(?=R\d+\s*:|$)",
                @"(?:shall|must|will|should)(.+?)(?:\.|$)",
                @"(?:Function|Method|Procedure)\s+([a-zA-Z0-9_]+)(.+?)(?:\.|$)"
            };
            
            // Extract sections of interest
            Dictionary<string, List<string>> sections = new Dictionary<string, List<string>>
            {
                ["requirements"] = new List<string> { "Requirements", "Functional Requirements", "System Requirements", "Business Requirements", "Technical Requirements" },
                ["functions"] = new List<string> { "Functions", "Methods", "Procedures", "Operations", "X++ Methods" },
                ["entities"] = new List<string> { "Entities", "Tables", "Data Model", "Data Structures", "Classes" },
                ["extensions"] = new List<string> { "Extensions", "Customizations", "Overrides", "Class Extensions" }
            };
            
            // Identify document sections
            Dictionary<string, string> sectionContent = new Dictionary<string, string>();
            string currentSection = "general";
            sectionContent[currentSection] = "";
            
            string[] lines = parsedContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            foreach (string line in lines)
            {
                bool matched = false;
                
                foreach (var sectionType in sections.Keys)
                {
                    foreach (string sectionName in sections[sectionType])
                    {
                        if (Regex.IsMatch(line, $@"^{Regex.Escape(sectionName)}(?:\s+|:|$)", RegexOptions.IgnoreCase))
                        {
                            currentSection = sectionType;
                            if (!sectionContent.ContainsKey(currentSection))
                            {
                                sectionContent[currentSection] = "";
                            }
                            matched = true;
                            break;
                        }
                    }
                    
                    if (matched) break;
                }
                
                // Add content to current section
                sectionContent[currentSection] += line + Environment.NewLine;
            }
            
            // Process each section with its patterns
            foreach (var sectionPair in sectionContent)
            {
                string sectionType = sectionPair.Key;
                string content = sectionPair.Value;
                
                if (sectionType == "requirements" || sectionType == "general")
                {
                    foreach (string pattern in reqPatterns)
                    {
                        MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        
                        foreach (Match match in matches)
                        {
                            string reqText = match.Groups.Count > 1 ? match.Groups[1].Value.Trim() : match.Value.Trim();
                            
                            if (reqText.Length > 10)  // Filter out very short matches
                            {
                                Requirements.Add(reqText);
                            }
                        }
                    }
                }
                else if (sectionType == "functions")
                {
                    string methodPattern = @"(?:Function|Method|Procedure|void|str|int)\s+([a-zA-Z0-9_]+)\s*\(([^)]*)\)";
                    MatchCollection matches = Regex.Matches(content, methodPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    
                    foreach (Match match in matches)
                    {
                        string methodName = match.Groups[1].Value.Trim();
                        string parameters = match.Groups[2].Value.Trim();
                        
                        FDDFunction function = new FDDFunction
                        {
                            Name = methodName,
                            Parameters = parameters,
                            Description = FindDescriptionNear(content, methodName)
                        };
                        
                        Functions.Add(function);
                    }
                }
                else if (sectionType == "entities")
                {
                    string entityPattern = @"(?:Table|Entity|Class)\s+([a-zA-Z0-9_]+)";
                    MatchCollection matches = Regex.Matches(content, entityPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    
                    foreach (Match match in matches)
                    {
                        string entityName = match.Groups[1].Value.Trim();
                        
                        FDDEntity entity = new FDDEntity
                        {
                            Name = entityName,
                            Description = FindDescriptionNear(content, entityName)
                        };
                        
                        Entities.Add(entity);
                    }
                }
                else if (sectionType == "extensions")
                {
                    string extensionPattern = @"(?:Extension|Customization)\s+([a-zA-Z0-9_]+)(?:\s+on|:)\s+([a-zA-Z0-9_]+)";
                    MatchCollection matches = Regex.Matches(content, extensionPattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    
                    foreach (Match match in matches)
                    {
                        string extensionType = match.Groups[1].Value.Trim();
                        string baseName = match.Groups[2].Value.Trim();
                        
                        FDDExtension extension = new FDDExtension
                        {
                            Type = extensionType,
                            BaseName = baseName,
                            Description = FindDescriptionNear(content, $"{extensionType} {baseName}")
                        };
                        
                        Extensions.Add(extension);
                    }
                }
            }
            
            // Deduplicate requirements
            Requirements = Requirements.Distinct().ToList();
            
            // Additional processing to find implied requirements if we don't have enough
            if (Requirements.Count < 5)
            {
                ExtractImpliedRequirements();
            }
        }

        private string FindDescriptionNear(string content, string term, int window = 100)
        {
            int termPos = content.IndexOf(term);
            
            if (termPos >= 0)
            {
                int start = Math.Max(0, termPos - window);
                int end = Math.Min(content.Length, termPos + window);
                string context = content.Substring(start, end - start);
                
                // Look for sentence or description patterns
                string descPattern = @"(?:description|desc|details|implements)(?:\s*:|\s+is|\s+are)?\s*([^.]+)";
                Match match = Regex.Match(context, descPattern, RegexOptions.IgnoreCase);
                
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
                else
                {
                    // Return nearby text
                    return context.Replace(term, "").Trim();
                }
            }
            
            return "";
        }

        private void ExtractImpliedRequirements()
        {
            // Use simple NLP-like techniques to extract implied requirements
            
            // Look for sentences with modal verbs that often indicate requirements
            List<string> requirementIndicators = new List<string> { "shall", "must", "will", "should", "needs to", "has to", "required" };
            
            string[] sentences = Regex.Split(parsedContent, @"(?<=[.!?])\s+");
            
            foreach (string sentence in sentences)
            {
                string sentText = sentence.Trim();
                
                // Skip very short sentences
                if (sentText.Length < 15)
                    continue;
                
                // Check for requirement indicators
                bool hasIndicator = requirementIndicators.Any(indicator => sentText.ToLower().Contains(indicator));
                
                // Check for imperative mood sentences (starting with a verb)
                bool startsWithVerb = Regex.IsMatch(sentText, @"^\w+(?:s|es|ed|ing)?\b", RegexOptions.IgnoreCase);
                
                if (hasIndicator || startsWithVerb)
                {
                    if (!Requirements.Contains(sentText))
                    {
                        Requirements.Add(sentText);
                    }
                }
            }
        }

        public List<string> SearchKeyword(string keyword)
        {
            List<string> results = new List<string>();
            
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 3)
                return results;
            
            // Prepare keyword for search (normalize)
            string keywordNorm = keyword.ToLower();
            
            // Search in the raw content with context
            string pattern = $".{{0,100}}{Regex.Escape(keywordNorm)}.{{0,100}}";
            MatchCollection matches = Regex.Matches(parsedContent.ToLower(), pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            foreach (Match match in matches)
            {
                string context = parsedContent.Substring(match.Index, match.Length);
                
                // Clean up the context
                context = Regex.Replace(context, @"\s+", " ").Trim();
                
                if (!results.Contains(context))
                {
                    results.Add(context);
                }
            }
            
            return results;
        }
    }

    public class FDDFunction
    {
        public string Name { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class FDDEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class FDDExtension
    {
        public string Type { get; set; } = string.Empty;
        public string BaseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}