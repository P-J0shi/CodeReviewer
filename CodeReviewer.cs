using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace D365CodeReviewer
{
    public enum DiscrepancyType
    {
        MissingEntity,
        MissingFunction,
        ParameterMismatch,
        UnimplementedRequirement,
        MissingExtension
    }

    public enum DiscrepancySeverity
    {
        Low,
        Medium,
        High
    }

    public class Discrepancy
    {
        public DiscrepancyType Type { get; set; }
        public DiscrepancySeverity Severity { get; set; }
        public string Description { get; set; } = string.Empty;
        
        // For missing entity or function
        public string EntityName { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
        
        // For parameter mismatch
        public string ExpectedParams { get; set; } = string.Empty;
        public string ActualParams { get; set; } = string.Empty;
        
        // For unimplemented requirement
        public string Requirement { get; set; } = string.Empty;
        public List<string>? Keywords { get; set; }
        
        // For missing extension
        public string BaseName { get; set; } = string.Empty;
    }

    public class ImplementedFeature
    {
        public string Type { get; set; } = string.Empty; // "entity", "function", "requirement", "extension"
        public string Name { get; set; } = string.Empty;
        public string Implementation { get; set; } = string.Empty;
        public string ImplementationName { get; set; } = string.Empty;
        public string ContainerType { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public List<MethodMatch>? Matches { get; set; }
    }

    public class MethodMatch
    {
        public string Method { get; set; } = string.Empty;
        public string Container { get; set; } = string.Empty;
        public double MatchScore { get; set; }
    }

    public class MissingFeature
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string>? Keywords { get; set; }
        public string BaseName { get; set; } = string.Empty;
    }

    public class AnalysisNote
    {
        public string Type { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string Implementation { get; set; } = string.Empty;
        public string Function { get; set; } = string.Empty;
        public string ContainerInfo { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public string ExtendsClass { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }

    public class CodeReviewer
    {
        private FDDParser fddParser;
        private AXPPParser axppParser;
        
        public List<Discrepancy> Discrepancies { get; private set; } = new List<Discrepancy>();
        public List<AnalysisNote> AnalysisNotes { get; private set; } = new List<AnalysisNote>();
        public List<ImplementedFeature> ImplementedFeatures { get; private set; } = new List<ImplementedFeature>();
        public List<MissingFeature> MissingFeatures { get; private set; } = new List<MissingFeature>();

        public CodeReviewer(FDDParser fddParser, AXPPParser axppParser)
        {
            this.fddParser = fddParser;
            this.axppParser = axppParser;
        }

        public void Review()
        {
            // Check for entity implementation
            ReviewEntities();
            
            // Check for function implementation
            ReviewFunctions();
            
            // Check for general requirements implementation
            ReviewRequirements();
            
            // Check for extensions
            ReviewExtensions();
        }

        private void ReviewEntities()
        {
            List<FDDEntity> fddEntities = fddParser.Entities;
            List<AXPPClass> axppClasses = axppParser.Classes;
            List<AXPPTable> axppTables = axppParser.Tables;
            
            // Track found entities
            HashSet<string> foundEntities = new HashSet<string>();
            
            // Check if required entities (classes and tables) are implemented
            foreach (FDDEntity fddEntity in fddEntities)
            {
                string entityName = fddEntity.Name;
                bool entityFound = false;
                
                // Check if entity exists as a class
                foreach (AXPPClass axppClass in axppClasses)
                {
                    if (NamesMatch(axppClass.Name, entityName))
                    {
                        entityFound = true;
                        foundEntities.Add(entityName);
                        
                        ImplementedFeatures.Add(new ImplementedFeature
                        {
                            Type = "entity",
                            Name = entityName,
                            Implementation = "class",
                            ImplementationName = axppClass.Name
                        });
                        break;
                    }
                }
                
                if (!entityFound)
                {
                    // Check if entity exists as a table
                    foreach (AXPPTable axppTable in axppTables)
                    {
                        if (NamesMatch(axppTable.Name, entityName))
                        {
                            entityFound = true;
                            foundEntities.Add(entityName);
                            
                            ImplementedFeatures.Add(new ImplementedFeature
                            {
                                Type = "entity",
                                Name = entityName,
                                Implementation = "table",
                                ImplementationName = axppTable.Name
                            });
                            break;
                        }
                    }
                }
                
                if (!entityFound)
                {
                    // Entity is missing
                    Discrepancies.Add(new Discrepancy
                    {
                        Type = DiscrepancyType.MissingEntity,
                        EntityName = entityName,
                        Description = fddEntity.Description,
                        Severity = DiscrepancySeverity.High
                    });
                    
                    MissingFeatures.Add(new MissingFeature
                    {
                        Type = "entity",
                        Name = entityName,
                        Description = fddEntity.Description
                    });
                }
            }
            
            // Check for classes or tables in implementation that are not in the design
            foreach (AXPPClass axppClass in axppClasses)
            {
                string className = axppClass.Name;
                if (!fddEntities.Any(e => NamesMatch(className, e.Name)))
                {
                    // This is a new class not mentioned in the FDD
                    AnalysisNotes.Add(new AnalysisNote
                    {
                        Type = "extra_entity",
                        Entity = className,
                        Implementation = "class",
                        Note = "Class exists in implementation but not in design document"
                    });
                }
            }
            
            foreach (AXPPTable axppTable in axppTables)
            {
                string tableName = axppTable.Name;
                if (!fddEntities.Any(e => NamesMatch(tableName, e.Name)))
                {
                    // This is a new table not mentioned in the FDD
                    AnalysisNotes.Add(new AnalysisNote
                    {
                        Type = "extra_entity",
                        Entity = tableName,
                        Implementation = "table",
                        Note = "Table exists in implementation but not in design document"
                    });
                }
            }
        }

        private void ReviewFunctions()
        {
            List<FDDFunction> fddFunctions = fddParser.Functions;
            List<AXPPMethod> axppMethods = axppParser.Methods;
            
            // Track found functions
            HashSet<string> foundFunctions = new HashSet<string>();
            
            // Check if required functions are implemented
            foreach (FDDFunction fddFunction in fddFunctions)
            {
                string functionName = fddFunction.Name;
                bool functionFound = false;
                
                // Check if function exists
                foreach (AXPPMethod axppMethod in axppMethods)
                {
                    if (NamesMatch(axppMethod.Name, functionName))
                    {
                        functionFound = true;
                        foundFunctions.Add(functionName);
                        
                        // Track implementation
                        string containerType = null;
                        string containerName = null;
                        
                        if (!string.IsNullOrEmpty(axppMethod.ClassName))
                        {
                            containerType = "class";
                            containerName = axppMethod.ClassName;
                        }
                        else if (!string.IsNullOrEmpty(axppMethod.TableName))
                        {
                            containerType = "table";
                            containerName = axppMethod.TableName;
                        }
                        else if (!string.IsNullOrEmpty(axppMethod.FormName))
                        {
                            containerType = "form";
                            containerName = axppMethod.FormName;
                        }
                        else if (!string.IsNullOrEmpty(axppMethod.ReportName))
                        {
                            containerType = "report";
                            containerName = axppMethod.ReportName;
                        }
                        
                        ImplementedFeatures.Add(new ImplementedFeature
                        {
                            Type = "function",
                            Name = functionName,
                            ImplementationName = axppMethod.Name,
                            ContainerType = containerType,
                            ContainerName = containerName
                        });
                        
                        // Check function parameters if available
                        string fddParams = fddFunction.Parameters;
                        string axppParams = axppMethod.Parameters;
                        
                        if (!string.IsNullOrEmpty(fddParams) && !string.IsNullOrEmpty(axppParams) && !ParametersMatch(fddParams, axppParams))
                        {
                            Discrepancies.Add(new Discrepancy
                            {
                                Type = DiscrepancyType.ParameterMismatch,
                                FunctionName = functionName,
                                ExpectedParams = fddParams,
                                ActualParams = axppParams,
                                Severity = DiscrepancySeverity.Medium
                            });
                        }
                        
                        break;
                    }
                }
                
                if (!functionFound)
                {
                    // Function is missing
                    Discrepancies.Add(new Discrepancy
                    {
                        Type = DiscrepancyType.MissingFunction,
                        FunctionName = functionName,
                        Description = fddFunction.Description,
                        Parameters = fddFunction.Parameters,
                        Severity = DiscrepancySeverity.High
                    });
                    
                    MissingFeatures.Add(new MissingFeature
                    {
                        Type = "function",
                        Name = functionName,
                        Description = fddFunction.Description
                    });
                }
            }
            
            // Check for methods in implementation that are not in the design
            foreach (AXPPMethod axppMethod in axppMethods)
            {
                string methodName = axppMethod.Name;
                
                // Skip standard X++ methods that don't need to be in the FDD
                if (StandardMethods.Contains(methodName.ToLower()))
                    continue;
                
                if (!fddFunctions.Any(f => NamesMatch(methodName, f.Name)))
                {
                    // This is a new method not mentioned in the FDD
                    string containerInfo = "";
                    
                    if (!string.IsNullOrEmpty(axppMethod.ClassName))
                        containerInfo = $" in class {axppMethod.ClassName}";
                    else if (!string.IsNullOrEmpty(axppMethod.TableName))
                        containerInfo = $" in table {axppMethod.TableName}";
                    else if (!string.IsNullOrEmpty(axppMethod.FormName))
                        containerInfo = $" in form {axppMethod.FormName}";
                    else if (!string.IsNullOrEmpty(axppMethod.ReportName))
                        containerInfo = $" in report {axppMethod.ReportName}";
                    
                    AnalysisNotes.Add(new AnalysisNote
                    {
                        Type = "extra_function",
                        Function = methodName,
                        ContainerInfo = containerInfo,
                        Note = $"Method exists in implementation{containerInfo} but not in design document"
                    });
                }
            }
        }

        private void ReviewRequirements()
        {
            List<string> fddRequirements = fddParser.Requirements;
            
            foreach (string req in fddRequirements)
            {
                // Extract key terms from requirement
                List<string> keywords = ExtractKeywords(req);
                
                // Skip if no meaningful keywords
                if (keywords.Count == 0)
                    continue;
                
                // Search for keywords in method bodies
                bool foundInCode = false;
                List<MethodMatch> matchingMethods = new List<MethodMatch>();
                
                foreach (AXPPMethod method in axppParser.Methods)
                {
                    string methodBody = method.Body;
                    if (string.IsNullOrEmpty(methodBody))
                        continue;
                    
                    // Check if requirement keywords appear in the method
                    int matches = 0;
                    foreach (string keyword in keywords)
                    {
                        if (methodBody.ToLower().Contains(keyword.ToLower()))
                            matches++;
                    }
                    
                    // Consider a significant match if more than half of keywords are found
                    if (matches >= Math.Max(1, keywords.Count / 2))
                    {
                        foundInCode = true;
                        
                        string containerInfo = "";
                        if (!string.IsNullOrEmpty(method.ClassName))
                            containerInfo = $"class {method.ClassName}";
                        else if (!string.IsNullOrEmpty(method.TableName))
                            containerInfo = $"table {method.TableName}";
                        else if (!string.IsNullOrEmpty(method.FormName))
                            containerInfo = $"form {method.FormName}";
                        else if (!string.IsNullOrEmpty(method.ReportName))
                            containerInfo = $"report {method.ReportName}";
                        
                        matchingMethods.Add(new MethodMatch
                        {
                            Method = method.Name,
                            Container = containerInfo,
                            MatchScore = (double)matches / keywords.Count
                        });
                    }
                }
                
                // Add to results
                if (foundInCode)
                {
                    // Sort matching methods by match score
                    matchingMethods = matchingMethods
                        .OrderByDescending(m => m.MatchScore)
                        .Take(3) // Top 3 matches
                        .ToList();
                    
                    ImplementedFeatures.Add(new ImplementedFeature
                    {
                        Type = "requirement",
                        Name = TruncateForName(req, 50),
                        Implementation = "code",
                        Matches = matchingMethods
                    });
                }
                else
                {
                    // Requirement might not be implemented
                    Discrepancies.Add(new Discrepancy
                    {
                        Type = DiscrepancyType.UnimplementedRequirement,
                        Requirement = req,
                        Keywords = keywords,
                        Severity = DiscrepancySeverity.Medium
                    });
                    
                    MissingFeatures.Add(new MissingFeature
                    {
                        Type = "requirement",
                        Description = req,
                        Keywords = keywords
                    });
                }
            }
        }

        private void ReviewExtensions()
        {
            List<FDDExtension> fddExtensions = fddParser.Extensions;
            List<AXPPExtension> axppExtensions = axppParser.Extensions;
            
            // Check if required extensions are implemented
            foreach (FDDExtension fddExt in fddExtensions)
            {
                string extType = fddExt.Type;
                string baseName = fddExt.BaseName;
                bool extFound = false;
                
                // Check if extension exists
                foreach (AXPPExtension axppExt in axppExtensions)
                {
                    if (NamesMatch(axppExt.ExtendsClass, baseName))
                    {
                        extFound = true;
                        
                        ImplementedFeatures.Add(new ImplementedFeature
                        {
                            Type = "extension",
                            Name = axppExt.Name,
                            Implementation = "extension",
                            ImplementationName = $"extends {axppExt.ExtendsClass}"
                        });
                        
                        break;
                    }
                }
                
                if (!extFound)
                {
                    // Extension is missing
                    Discrepancies.Add(new Discrepancy
                    {
                        Type = DiscrepancyType.MissingExtension,
                        BaseName = baseName,
                        Description = fddExt.Description,
                        Severity = DiscrepancySeverity.Medium
                    });
                    
                    MissingFeatures.Add(new MissingFeature
                    {
                        Type = "extension",
                        BaseName = baseName,
                        Description = fddExt.Description
                    });
                }
            }
            
            // Check for extensions in implementation that are not in the design
            foreach (AXPPExtension axppExt in axppExtensions)
            {
                string baseName = axppExt.ExtendsClass;
                
                if (!fddExtensions.Any(e => NamesMatch(baseName, e.BaseName)))
                {
                    // This is a new extension not mentioned in the FDD
                    AnalysisNotes.Add(new AnalysisNote
                    {
                        Type = "extra_extension",
                        Extension = axppExt.Name,
                        ExtendsClass = baseName,
                        Note = $"Extension of {baseName} exists in implementation but not in design document"
                    });
                }
            }
        }

        private bool NamesMatch(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return false;
            
            // Strip potential prefixes/suffixes often added in implementation
            string cleanName1 = Regex.Replace(name1, @"^(?:Ax|CUS|ISV|USR)_?", "");
            string cleanName2 = Regex.Replace(name2, @"^(?:Ax|CUS|ISV|USR)_?", "");
            
            // Remove common suffixes
            cleanName1 = Regex.Replace(cleanName1, @"(?:Table|Class|Form|Query|Report|Ext)$", "");
            cleanName2 = Regex.Replace(cleanName2, @"(?:Table|Class|Form|Query|Report|Ext)$", "");
            
            // Normalize case and whitespace
            cleanName1 = cleanName1.ToLower().Trim();
            cleanName2 = cleanName2.ToLower().Trim();
            
            // Direct match
            if (cleanName1 == cleanName2)
                return true;
            
            // Partial match (one name contains the other)
            if (cleanName1.Contains(cleanName2) || cleanName2.Contains(cleanName1))
                return true;
            
            // Check edit distance for typos/minor differences
            if (cleanName1.Length > 3 && cleanName2.Length > 3)
            {
                double similarity = StringSimilarity(cleanName1, cleanName2);
                if (similarity > 0.8)  // 80% similarity threshold
                    return true;
            }
            
            return false;
        }

        private double StringSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0.0;
            
            if (s1 == s2)
                return 1.0;
            
            int len1 = s1.Length;
            int len2 = s2.Length;
            
            int[,] matrix = new int[len1 + 1, len2 + 1];
            
            // Initialize first column and first row
            for (int i = 0; i <= len1; i++)
                matrix[i, 0] = i;
            
            for (int j = 0; j <= len2; j++)
                matrix[0, j] = j;
            
            // Fill the rest of the matrix
            for (int i = 1; i <= len1; i++)
            {
                for (int j = 1; j <= len2; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }
            
            return 1.0 - ((double)matrix[len1, len2] / Math.Max(len1, len2));
        }

        private bool ParametersMatch(string params1, string params2)
        {
            if (string.IsNullOrEmpty(params1) || string.IsNullOrEmpty(params2))
                // If either is empty, they might be compatible
                return true;
            
            // Clean and normalize parameter strings
            string cleanParams1 = Regex.Replace(params1, @"\s+", " ").Trim().ToLower();
            string cleanParams2 = Regex.Replace(params2, @"\s+", " ").Trim().ToLower();
            
            if (cleanParams1 == cleanParams2)
                return true;
            
            // Count parameters
            int paramCount1 = cleanParams1.Count(c => c == ',') + 1;
            int paramCount2 = cleanParams2.Count(c => c == ',') + 1;
            
            // Different parameter counts is a strong indication of mismatch
            if (paramCount1 != paramCount2)
                return false;
            
            // Parse and compare parameter types
            try
            {
                // Extract parameter types (rough approximation)
                List<string> types1 = Regex.Matches(cleanParams1, @"(\w+)\s+\w+")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ToList();
                
                List<string> types2 = Regex.Matches(cleanParams2, @"(\w+)\s+\w+")
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ToList();
                
                // Check if types match
                for (int i = 0; i < Math.Min(types1.Count, types2.Count); i++)
                {
                    string t1 = types1[i];
                    string t2 = types2[i];
                    
                    // X++ type compatibility checks
                    if (t1 != t2 && !TypesCompatible(t1, t2))
                        return false;
                }
                
                return true;
            }
            catch
            {
                // If parsing fails, fall back to basic similarity check
                double similarity = StringSimilarity(cleanParams1, cleanParams2);
                return similarity > 0.7;  // 70% similarity threshold
            }
        }

        private bool TypesCompatible(string type1, string type2)
        {
            // Basic compatibility classes
            string[] numericTypes = { "int", "int64", "real", "decimal", "num" };
            string[] stringTypes = { "str", "string" };
            string[] dateTypes = { "date", "utcdatetime", "datetime" };
            
            // Check if both types are in the same compatibility group
            if (numericTypes.Contains(type1) && numericTypes.Contains(type2))
                return true;
            
            if (stringTypes.Contains(type1) && stringTypes.Contains(type2))
                return true;
            
            if (dateTypes.Contains(type1) && dateTypes.Contains(type2))
                return true;
            
            // Consider container types compatible with themselves
            string[] containerTypes = { "array", "list", "map", "set" };
            if (containerTypes.Contains(type1) && type1 == type2)
                return true;
            
            // Common type conversions in X++
            Dictionary<string, string> compatiblePairs = new Dictionary<string, string>
            {
                { "any", "object" },
                { "object", "any" },
                { "boolean", "bool" },
                { "bool", "boolean" },
                { "int", "enum" },
                { "enum", "int" },
                { "record", "common" }
            };
            
            return (compatiblePairs.TryGetValue(type1, out string? value) && value == type2) ||
                   (compatiblePairs.TryGetValue(type2, out string? value2) && value2 == type1);
        }

        private List<string> ExtractKeywords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();
            
            // Tokenize
            string[] tokens = text.ToLower().Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            // Remove stopwords and punctuation
            var filteredTokens = tokens
                .Where(word => !StopWords.Contains(word) && word.Length > 2 && word.All(char.IsLetterOrDigit))
                .ToList();
            
            // Get unique keywords
            var keywords = filteredTokens.Distinct().ToList();
            
            // Filter out very common words in programming contexts
            string[] commonProgWords = { "use", "set", "get", "create", "update", "delete", "show", "display" };
            keywords = keywords.Where(word => !commonProgWords.Contains(word)).ToList();
            
            return keywords;
        }

        private string TruncateForName(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            
            if (text.Length <= maxLength)
                return text;
            
            return text.Substring(0, maxLength) + "...";
        }

        public Dictionary<string, object> GenerateReport(string outputFile)
        {
            var report = new Dictionary<string, object>
            {
                ["review_summary"] = new
                {
                    total_discrepancies = Discrepancies.Count,
                    total_implemented_features = ImplementedFeatures.Count,
                    total_missing_features = MissingFeatures.Count,
                    total_analysis_notes = AnalysisNotes.Count
                },
                ["discrepancies"] = Discrepancies,
                ["implemented_features"] = ImplementedFeatures,
                ["missing_features"] = MissingFeatures,
                ["analysis_notes"] = AnalysisNotes
            };
            
            // Write JSON report
            File.WriteAllText(outputFile, JsonConvert.SerializeObject(report, Formatting.Indented));
            
            return report;
        }

        public bool GenerateHtmlReport(string outputFile)
        {
            StringBuilder html = new StringBuilder();
            
            // Basic HTML template
            html.AppendLine(@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>X++ Code Review Report</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 20px;
            color: #333;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
        }
        h1, h2, h3 {
            color: #0066cc;
        }
        .summary {
            background-color: #f5f5f5;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        .section {
            margin-bottom: 30px;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        th, td {
            border: 1px solid #ddd;
            padding: 8px 12px;
            text-align: left;
        }
        th {
            background-color: #f2f2f2;
        }
        tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        .severity-high {
            color: #d9534f;
            font-weight: bold;
        }
        .severity-medium {
            color: #f0ad4e;
        }
        .severity-low {
            color: #5bc0de;
        }
        .requirement {
            background-color: #e6f7ff;
            padding: 10px;
            border-left: 4px solid #0066cc;
            margin-bottom: 10px;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <h1>X++ Code Review Report</h1>
        
        <div class=""summary"">
            <h2>Review Summary</h2>
            <p><strong>Total Discrepancies:</strong> " + Discrepancies.Count + @"</p>
            <p><strong>Implemented Features:</strong> " + ImplementedFeatures.Count + @"</p>
            <p><strong>Missing Features:</strong> " + MissingFeatures.Count + @"</p>
            <p><strong>Analysis Notes:</strong> " + AnalysisNotes.Count + @"</p>
        </div>
        
        <div class=""section"">
            <h2>Discrepancies</h2>");
            
            if (Discrepancies.Count > 0)
            {
                html.AppendLine(@"
            <table>
                <tr>
                    <th>Type</th>
                    <th>Description</th>
                    <th>Severity</th>
                </tr>");
                
                foreach (var disc in Discrepancies)
                {
                    string severityClass = $"severity-{disc.Severity.ToString().ToLower()}";
                    
                    string description = "";
                    if (disc.Type == DiscrepancyType.MissingEntity)
                    {
                        description = $"Entity '<strong>{disc.EntityName}</strong>' is missing from implementation. ";
                        description += $"<div class='requirement'>{disc.Description}</div>";
                    }
                    else if (disc.Type == DiscrepancyType.MissingFunction)
                    {
                        description = $"Function '<strong>{disc.FunctionName}</strong>' is missing from implementation. ";
                        if (!string.IsNullOrEmpty(disc.Parameters))
                        {
                            description += $"Parameters: {disc.Parameters}. ";
                        }
                        description += $"<div class='requirement'>{disc.Description}</div>";
                    }
                    else if (disc.Type == DiscrepancyType.ParameterMismatch)
                    {
                        description = $"Function '<strong>{disc.FunctionName}</strong>' has parameter mismatch. ";
                        description += $"Expected: {disc.ExpectedParams}, ";
                        description += $"Actual: {disc.ActualParams}";
                    }
                    else if (disc.Type == DiscrepancyType.UnimplementedRequirement)
                    {
                        description = $"Requirement not implemented: ";
                        description += $"<div class='requirement'>{disc.Requirement}</div>";
                        if (disc.Keywords != null && disc.Keywords.Any())
                        {
                            description += $"Keywords: {string.Join(", ", disc.Keywords)}";
                        }
                    }
                    else if (disc.Type == DiscrepancyType.MissingExtension)
                    {
                        description = $"Extension for '<strong>{disc.BaseName}</strong>' is missing from implementation. ";
                        description += $"<div class='requirement'>{disc.Description}</div>";
                    }
                    else
                    {
                        description = disc.Description;
                    }
                    
                    html.AppendLine($@"
                <tr>
                    <td>{disc.Type.ToString().Replace("_", " ")}</td>
                    <td>{description}</td>
                    <td class=""{severityClass}"">{disc.Severity.ToString().ToUpper()}</td>
                </tr>");
                }
                
                html.AppendLine(@"
            </table>");
            }
            else
            {
                html.AppendLine("<p>No discrepancies found.</p>");
            }
            
            html.AppendLine(@"
        </div>
        
        <div class=""section"">
            <h2>Missing Features</h2>");
            
            if (MissingFeatures.Count > 0)
            {
                html.AppendLine(@"
            <table>
                <tr>
                    <th>Type</th>
                    <th>Name</th>
                    <th>Description</th>
                </tr>");
                
                foreach (var feature in MissingFeatures)
                {
                    string name = !string.IsNullOrEmpty(feature.Name) ? feature.Name : feature.BaseName;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "Unknown";
                    }
                    
                    html.AppendLine($@"
                <tr>
                    <td>{feature.Type.Replace("_", " ")}</td>
                    <td>{name}</td>
                    <td>{feature.Description}</td>
                </tr>");
                }
                
                html.AppendLine(@"
            </table>");
            }
            else
            {
                html.AppendLine("<p>No missing features found.</p>");
            }
            
            html.AppendLine(@"
        </div>
        
        <div class=""section"">
            <h2>Implemented Features</h2>");
            
            if (ImplementedFeatures.Count > 0)
            {
                html.AppendLine(@"
            <table>
                <tr>
                    <th>Type</th>
                    <th>Name</th>
                    <th>Implementation</th>
                </tr>");
                
                foreach (var feature in ImplementedFeatures)
                {
                    string name = feature.Name;
                    
                    string implementation = "";
                    if (feature.Type == "entity")
                    {
                        implementation = $"{feature.Implementation}: {feature.ImplementationName}";
                    }
                    else if (feature.Type == "function")
                    {
                        implementation = $"{feature.ImplementationName}";
                        if (!string.IsNullOrEmpty(feature.ContainerType) && !string.IsNullOrEmpty(feature.ContainerName))
                        {
                            implementation += $" in {feature.ContainerType}: {feature.ContainerName}";
                        }
                    }
                    else if (feature.Type == "requirement")
                    {
                        var matches = feature.Matches;
                        if (matches != null && matches.Any())
                        {
                            implementation = "Implemented in: ";
                            for (int i = 0; i < matches.Count; i++)
                            {
                                if (i > 0)
                                {
                                    implementation += ", ";
                                }
                                implementation += $"{matches[i].Method} ({matches[i].Container})";
                            }
                        }
                    }
                    else if (feature.Type == "extension")
                    {
                        implementation = feature.ImplementationName;
                    }
                    
                    html.AppendLine($@"
                <tr>
                    <td>{feature.Type.Replace("_", " ")}</td>
                    <td>{name}</td>
                    <td>{implementation}</td>
                </tr>");
                }
                
                html.AppendLine(@"
            </table>");
            }
            else
            {
                html.AppendLine("<p>No implemented features found.</p>");
            }
            
            html.AppendLine(@"
        </div>
        
        <div class=""section"">
            <h2>Analysis Notes</h2>");
            
            if (AnalysisNotes.Count > 0)
            {
                html.AppendLine(@"
            <table>
                <tr>
                    <th>Type</th>
                    <th>Description</th>
                </tr>");
                
                foreach (var note in AnalysisNotes)
                {
                    string description = "";
                    if (note.Type == "extra_entity")
                    {
                        description = $"Entity '<strong>{note.Entity}</strong>' exists in implementation ({note.Implementation}) ";
                        description += "but is not mentioned in the design document.";
                    }
                    else if (note.Type == "extra_function")
                    {
                        description = $"Function '<strong>{note.Function}</strong>' {note.ContainerInfo} ";
                        description += "exists in implementation but is not mentioned in the design document.";
                    }
                    else if (note.Type == "extra_extension")
                    {
                        description = $"Extension '<strong>{note.Extension}</strong>' extending {note.ExtendsClass} ";
                        description += "exists in implementation but is not mentioned in the design document.";
                    }
                    else
                    {
                        description = note.Note;
                    }
                    
                    html.AppendLine($@"
                <tr>
                    <td>{note.Type.Replace("_", " ").Title()}</td>
                    <td>{description}</td>
                </tr>");
                }
                
                html.AppendLine(@"
            </table>");
            }
            else
            {
                html.AppendLine("<p>No analysis notes.</p>");
            }
            
            html.AppendLine(@"
        </div>
    </div>
</body>
</html>");
            
            // Write HTML report
            File.WriteAllText(outputFile, html.ToString());
            
            return true;
        }

        // Common stop words
        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't", "as", "at",
            "be", "because", "been", "before", "being", "below", "between", "both", "but", "by",
            "can't", "cannot", "could", "couldn't",
            "did", "didn't", "do", "does", "doesn't", "doing", "don't", "down", "during",
            "each",
            "few", "for", "from", "further",
            "had", "hadn't", "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll", "he's", "her", "here", "here's",
            "hers", "herself", "him", "himself", "his", "how", "how's",
            "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into", "is", "isn't", "it", "it's", "its", "itself",
            "let's",
            "me", "more", "most", "mustn't", "my", "myself",
            "no", "nor", "not",
            "of", "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own",
            "same", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't", "so", "some", "such",
            "than", "that", "that's", "the", "their", "theirs", "them", "themselves", "then", "there", "there's", "these", "they",
            "they'd", "they'll", "they're", "they've", "this", "those", "through", "to", "too",
            "under", "until", "up",
            "very",
            "was", "wasn't", "we", "we'd", "we'll", "we're", "we've", "were", "weren't", "what", "what's", "when", "when's",
            "where", "where's", "which", "while", "who", "who's", "whom", "why", "why's", "with", "won't", "would", "wouldn't",
            "you", "you'd", "you'll", "you're", "you've", "your", "yours", "yourself", "yourselves"
        };

        // Standard X++ methods that don't need to be in the FDD
        private static readonly HashSet<string> StandardMethods = new HashSet<string>
        {
            "new", "run", "main", "construct", "delete", "insert", "update", "delete",
            "find", "getfromid", "init", "pack", "unpack", "validate", "cansubmit",
            "executequery", "fetchnext", "next", "first", "last", "reread", "research", "forupdate",
            "fieldsort", "exists", "getchanges", "getfieldname", "getfieldtype", "getindexname", "getprimarykey",
            "getrecordid", "settableid", "skipdeleted", "crosscompany", "setcompany", "gettableinfo"
        };
    }

    // String extension method for title case
    public static class StringExtensions
    {
        public static string Title(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            
            // Split by spaces and apply title case to each word
            string[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            
            return string.Join(" ", words);
        }
    }
}