using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace D365CodeReviewer
{
    public class AXPPParser
    {
        private string axppPath;
        
        public List<AXPPClass> Classes { get; private set; } = new List<AXPPClass>();
        public List<AXPPMethod> Methods { get; private set; } = new List<AXPPMethod>();
        public List<AXPPTable> Tables { get; private set; } = new List<AXPPTable>();
        public List<AXPPExtension> Extensions { get; private set; } = new List<AXPPExtension>();
        public List<AXPPForm> Forms { get; private set; } = new List<AXPPForm>();
        public List<AXPPQuery> Queries { get; private set; } = new List<AXPPQuery>();
        public List<AXPPReport> Reports { get; private set; } = new List<AXPPReport>();

        public AXPPParser(string axppPath)
        {
            this.axppPath = axppPath;
            
            // Extract file extension
            string extension = Path.GetExtension(axppPath).ToLower();
            
            if (extension != ".axpp" && extension != ".xml")
            {
                throw new ArgumentException($"Input file must be an .axpp or .xml file, got: {extension}");
            }
            
            // Parse the AXPP file
            ParseAXPP();
        }

        private void ParseAXPP()
        {
            try
            {
                // First try to parse as a ZIP file (AXPP is usually a zip with metadata and XML)
                using (ZipArchive archive = ZipFile.OpenRead(axppPath))
                {
                    // Extract XML files that describe the project elements
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            using (Stream stream = entry.Open())
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string xmlContent = reader.ReadToEnd();
                                ParseXmlContent(xmlContent, entry.FullName);
                            }
                        }
                    }
                }
            }
            catch (InvalidDataException)
            {
                // If not a zip, try to parse as XML directly
                try
                {
                    string xmlContent = File.ReadAllText(axppPath);
                    ParseXmlContent(xmlContent, axppPath);
                }
                catch (Exception ex) when (ex is XmlException || ex is IOException)
                {
                    // Last resort: try to parse as raw X++ code
                    try
                    {
                        string rawContent = File.ReadAllText(axppPath);
                        ParseRawXpp(rawContent);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException($"Could not parse file {axppPath} in any supported format");
                    }
                }
            }
        }

        private void ParseXmlContent(string xmlContent, string filename)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                
                // Determine element type from filename or XML structure
                string elementType = DetermineElementType(filename, doc);
                
                // Parse according to element type
                switch (elementType)
                {
                    case "class":
                        ParseClass(doc, filename);
                        break;
                    case "table":
                        ParseTable(doc, filename);
                        break;
                    case "form":
                        ParseForm(doc, filename);
                        break;
                    case "query":
                        ParseQuery(doc, filename);
                        break;
                    case "report":
                        ParseReport(doc, filename);
                        break;
                }
            }
            catch (XmlException)
            {
                // If XML parsing fails, try as raw X++ code
                ParseRawXpp(xmlContent);
            }
        }

        private string DetermineElementType(string filename, XmlDocument doc)
        {
            // Check filename first
            filename = filename.ToLower();
            
            if (filename.Contains("class"))
                return "class";
            else if (filename.Contains("table"))
                return "table";
            else if (filename.Contains("form"))
                return "form";
            else if (filename.Contains("query"))
                return "query";
            else if (filename.Contains("report"))
                return "report";
            
            // Check XML structure
            XmlElement? root = doc.DocumentElement;
            
            if (root != null)
            {
                Dictionary<string, List<string>> elementTags = new Dictionary<string, List<string>>
                {
                    ["class"] = new List<string> { "Class", "AxClass" },
                    ["table"] = new List<string> { "Table", "AxTable" },
                    ["form"] = new List<string> { "Form", "AxForm" },
                    ["query"] = new List<string> { "Query", "AxQuery" },
                    ["report"] = new List<string> { "Report", "AxReport" }
                };
                
                foreach (var elementType in elementTags.Keys)
                {
                    foreach (string tag in elementTags[elementType])
                    {
                        if (root.Name == tag || root.SelectSingleNode($"//{tag}") != null)
                        {
                            return elementType;
                        }
                    }
                }
            }
            
            // Default to class if can't determine
            return "class";
        }

        private void ParseClass(XmlDocument doc, string filename)
        {
            // Extract class name
            string className = "";
            foreach (string nameTag in new[] { "Name", "name", "AxClassName", "className" })
            {
                XmlNode? nameNode = doc.SelectSingleNode($"//{nameTag}");
                if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                {
                    className = nameNode.InnerText.Trim();
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(className))
            {
                // Try to extract from filename
                className = Path.GetFileNameWithoutExtension(filename);
            }
            
            // Extract methods
            List<AXPPMethod> methods = new List<AXPPMethod>();
            foreach (string methodTag in new[] { "Method", "method", "AxMethod", "Methods" })
            {
                XmlNodeList? methodNodes = doc.SelectNodes($"//{methodTag}");
                if (methodNodes != null)
                {
                    foreach (XmlNode methodNode in methodNodes)
                    {
                        string methodName = "";
                        string methodParams = "";
                        string methodBody = "";
                        string methodReturn = "";
                        
                        // Extract method name
                        foreach (string nameTag in new[] { "Name", "name", "MethodName" })
                        {
                            XmlNode? nameNode = methodNode.SelectSingleNode($".//{nameTag}");
                            if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                            {
                                methodName = nameNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        // Extract parameters
                        foreach (string paramsTag in new[] { "Parameters", "params", "MethodParameters" })
                        {
                            XmlNode? paramsNode = methodNode.SelectSingleNode($".//{paramsTag}");
                            if (paramsNode != null && !string.IsNullOrEmpty(paramsNode.InnerText))
                            {
                                methodParams = paramsNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        // Extract body
                        foreach (string bodyTag in new[] { "Source", "source", "MethodSource", "Body" })
                        {
                            XmlNode? bodyNode = methodNode.SelectSingleNode($".//{bodyTag}");
                            if (bodyNode != null && !string.IsNullOrEmpty(bodyNode.InnerText))
                            {
                                methodBody = bodyNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        // Extract return type
                        foreach (string returnTag in new[] { "ReturnType", "returnType", "MethodReturnType" })
                        {
                            XmlNode? returnNode = methodNode.SelectSingleNode($".//{returnTag}");
                            if (returnNode != null && !string.IsNullOrEmpty(returnNode.InnerText))
                            {
                                methodReturn = returnNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(methodName))
                        {
                            AXPPMethod method = new AXPPMethod
                            {
                                Name = methodName,
                                Parameters = methodParams,
                                Body = methodBody,
                                ReturnType = methodReturn,
                                ClassName = className
                            };
                            
                            methods.Add(method);
                        }
                    }
                }
            }
            
            // Check if it's an extension class
            bool isExtension = false;
            string extendsClass = "";
            
            foreach (string extTag in new[] { "Extends", "extends", "ExtendsClass" })
            {
                XmlNode? extNode = doc.SelectSingleNode($"//{extTag}");
                if (extNode != null && !string.IsNullOrEmpty(extNode.InnerText))
                {
                    isExtension = true;
                    extendsClass = extNode.InnerText.Trim();
                    break;
                }
            }
            
            // Add to appropriate collection
            if (isExtension)
            {
                Extensions.Add(new AXPPExtension
                {
                    Name = className,
                    ExtendsClass = extendsClass,
                    Methods = methods
                });
            }
            else
            {
                Classes.Add(new AXPPClass
                {
                    Name = className,
                    Methods = methods
                });
            }
            
            // Add all methods to methods list
            Methods.AddRange(methods);
        }

        private void ParseTable(XmlDocument doc, string filename)
        {
            // Extract table name
            string tableName = "";
            foreach (string nameTag in new[] { "Name", "name", "AxTableName", "tableName" })
            {
                XmlNode? nameNode = doc.SelectSingleNode($"//{nameTag}");
                if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                {
                    tableName = nameNode.InnerText.Trim();
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(tableName))
            {
                // Try to extract from filename
                tableName = Path.GetFileNameWithoutExtension(filename);
            }
            
            // Extract fields
            List<AXPPField> fields = new List<AXPPField>();
            foreach (string fieldTag in new[] { "Field", "field", "AxField", "Fields" })
            {
                XmlNodeList? fieldNodes = doc.SelectNodes($"//{fieldTag}");
                if (fieldNodes != null)
                {
                    foreach (XmlNode fieldNode in fieldNodes)
                    {
                        string fieldName = "";
                        string fieldType = "";
                        
                        // Extract field name
                        foreach (string nameTag in new[] { "Name", "name", "FieldName" })
                        {
                            XmlNode? nameNode = fieldNode.SelectSingleNode($".//{nameTag}");
                            if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                            {
                                fieldName = nameNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        // Extract field type
                        foreach (string typeTag in new[] { "Type", "type", "FieldType", "ExtendedDataType" })
                        {
                            XmlNode? typeNode = fieldNode.SelectSingleNode($".//{typeTag}");
                            if (typeNode != null && !string.IsNullOrEmpty(typeNode.InnerText))
                            {
                                fieldType = typeNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(fieldName))
                        {
                            fields.Add(new AXPPField
                            {
                                Name = fieldName,
                                Type = fieldType
                            });
                        }
                    }
                }
            }
            
            // Extract methods
            List<AXPPMethod> methods = new List<AXPPMethod>();
            foreach (string methodTag in new[] { "Method", "method", "AxMethod", "Methods" })
            {
                XmlNodeList? methodNodes = doc.SelectNodes($"//{methodTag}");
                if (methodNodes != null)
                {
                    foreach (XmlNode methodNode in methodNodes)
                    {
                        string methodName = "";
                        string methodParams = "";
                        string methodBody = "";
                        
                        // Extract method info (similar to class methods)
                        foreach (string nameTag in new[] { "Name", "name", "MethodName" })
                        {
                            XmlNode? nameNode = methodNode.SelectSingleNode($".//{nameTag}");
                            if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                            {
                                methodName = nameNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        foreach (string paramsTag in new[] { "Parameters", "params", "MethodParameters" })
                        {
                            XmlNode? paramsNode = methodNode.SelectSingleNode($".//{paramsTag}");
                            if (paramsNode != null && !string.IsNullOrEmpty(paramsNode.InnerText))
                            {
                                methodParams = paramsNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        foreach (string bodyTag in new[] { "Source", "source", "MethodSource", "Body" })
                        {
                            XmlNode? bodyNode = methodNode.SelectSingleNode($".//{bodyTag}");
                            if (bodyNode != null && !string.IsNullOrEmpty(bodyNode.InnerText))
                            {
                                methodBody = bodyNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(methodName))
                        {
                            AXPPMethod method = new AXPPMethod
                            {
                                Name = methodName,
                                Parameters = methodParams,
                                Body = methodBody,
                                TableName = tableName
                            };
                            
                            methods.Add(method);
                        }
                    }
                }
            }
            
            // Add to tables collection
            AXPPTable table = new AXPPTable
            {
                Name = tableName,
                Fields = fields,
                Methods = methods
            };
            
            Tables.Add(table);
            
            // Add methods to the main methods list
            Methods.AddRange(methods);
        }

        private void ParseForm(XmlDocument doc, string filename)
        {
            // Extract form name
            string formName = "";
            foreach (string nameTag in new[] { "Name", "name", "AxFormName", "formName" })
            {
                XmlNode? nameNode = doc.SelectSingleNode($"//{nameTag}");
                if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                {
                    formName = nameNode.InnerText.Trim();
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(formName))
            {
                // Try to extract from filename
                formName = Path.GetFileNameWithoutExtension(filename);
            }
            
            // Extract methods
            List<AXPPMethod> methods = new List<AXPPMethod>();
            foreach (string methodTag in new[] { "Method", "method", "AxMethod", "Methods" })
            {
                XmlNodeList? methodNodes = doc.SelectNodes($"//{methodTag}");
                if (methodNodes != null)
                {
                    foreach (XmlNode methodNode in methodNodes)
                    {
                        // Similar method extraction as in classes and tables
                        string methodName = "";
                        string methodParams = "";
                        string methodBody = "";
                        
                        foreach (string nameTag in new[] { "Name", "name", "MethodName" })
                        {
                            XmlNode? nameNode = methodNode.SelectSingleNode($".//{nameTag}");
                            if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                            {
                                methodName = nameNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        foreach (string paramsTag in new[] { "Parameters", "params", "MethodParameters" })
                        {
                            XmlNode? paramsNode = methodNode.SelectSingleNode($".//{paramsTag}");
                            if (paramsNode != null && !string.IsNullOrEmpty(paramsNode.InnerText))
                            {
                                methodParams = paramsNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        foreach (string bodyTag in new[] { "Source", "source", "MethodSource", "Body" })
                        {
                            XmlNode? bodyNode = methodNode.SelectSingleNode($".//{bodyTag}");
                            if (bodyNode != null && !string.IsNullOrEmpty(bodyNode.InnerText))
                            {
                                methodBody = bodyNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(methodName))
                        {
                            AXPPMethod method = new AXPPMethod
                            {
                                Name = methodName,
                                Parameters = methodParams,
                                Body = methodBody,
                                FormName = formName
                            };
                            
                            methods.Add(method);
                        }
                    }
                }
            }
            
            // Add to forms collection
            AXPPForm form = new AXPPForm
            {
                Name = formName,
                Methods = methods
            };
            
            Forms.Add(form);
            
            // Add methods to the main methods list
            Methods.AddRange(methods);
        }

        private void ParseQuery(XmlDocument doc, string filename)
        {
            // Extract query name
            string queryName = "";
            foreach (string nameTag in new[] { "Name", "name", "AxQueryName", "queryName" })
            {
                XmlNode? nameNode = doc.SelectSingleNode($"//{nameTag}");
                if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                {
                    queryName = nameNode.InnerText.Trim();
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(queryName))
            {
                // Try to extract from filename
                queryName = Path.GetFileNameWithoutExtension(filename);
            }
            
            // Extract data sources
            List<AXPPDataSource> dataSources = new List<AXPPDataSource>();
            foreach (string dsTag in new[] { "DataSource", "dataSource", "AxDataSource", "DataSources" })
            {
                XmlNodeList? dsNodes = doc.SelectNodes($"//{dsTag}");
                if (dsNodes != null)
                {
                    foreach (XmlNode dsNode in dsNodes)
                    {
                        string dsName = "";
                        string tableName = "";
                        
                        foreach (string nameTag in new[] { "Name", "name" })
                        {
                            XmlNode? nameNode = dsNode.SelectSingleNode($".//{nameTag}");
                            if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                            {
                                dsName = nameNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        foreach (string tableTag in new[] { "Table", "table", "TableName" })
                        {
                            XmlNode? tableNode = dsNode.SelectSingleNode($".//{tableTag}");
                            if (tableNode != null && !string.IsNullOrEmpty(tableNode.InnerText))
                            {
                                tableName = tableNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(dsName) || !string.IsNullOrEmpty(tableName))
                        {
                            dataSources.Add(new AXPPDataSource
                            {
                                Name = dsName,
                                TableName = tableName
                            });
                        }
                    }
                }
            }
            
            // Add to queries collection
            AXPPQuery query = new AXPPQuery
            {
                Name = queryName,
                DataSources = dataSources
            };
            
            Queries.Add(query);
        }

        private void ParseReport(XmlDocument doc, string filename)
        {
            // Extract report name
            string reportName = "";
            foreach (string nameTag in new[] { "Name", "name", "AxReportName", "reportName" })
            {
                XmlNode? nameNode = doc.SelectSingleNode($"//{nameTag}");
                if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                {
                    reportName = nameNode.InnerText.Trim();
                    break;
                }
            }
            
            if (string.IsNullOrEmpty(reportName))
            {
                // Try to extract from filename
                reportName = Path.GetFileNameWithoutExtension(filename);
            }
            
            // Extract methods
            List<AXPPMethod> methods = new List<AXPPMethod>();
            foreach (string methodTag in new[] { "Method", "method", "AxMethod", "Methods" })
            {
                XmlNodeList? methodNodes = doc.SelectNodes($"//{methodTag}");
                if (methodNodes != null)
                {
                    foreach (XmlNode methodNode in methodNodes)
                    {
                        string methodName = "";
                        string methodParams = "";
                        string methodBody = "";
                        
                        foreach (string nameTag in new[] { "Name", "name", "MethodName" })
                        {
                            XmlNode? nameNode = methodNode.SelectSingleNode($".//{nameTag}");
                            if (nameNode != null && !string.IsNullOrEmpty(nameNode.InnerText))
                            {
                                methodName = nameNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        foreach (string paramsTag in new[] { "Parameters", "params", "MethodParameters" })
                        {
                            XmlNode? paramsNode = methodNode.SelectSingleNode($".//{paramsTag}");
                            if (paramsNode != null && !string.IsNullOrEmpty(paramsNode.InnerText))
                            {
                                methodParams = paramsNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        foreach (string bodyTag in new[] { "Source", "source", "MethodSource", "Body" })
                        {
                            XmlNode? bodyNode = methodNode.SelectSingleNode($".//{bodyTag}");
                            if (bodyNode != null && !string.IsNullOrEmpty(bodyNode.InnerText))
                            {
                                methodBody = bodyNode.InnerText.Trim();
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(methodName))
                        {
                            AXPPMethod method = new AXPPMethod
                            {
                                Name = methodName,
                                Parameters = methodParams,
                                Body = methodBody,
                                ReportName = reportName
                            };
                            
                            methods.Add(method);
                        }
                    }
                }
            }
            
            // Add to reports collection
            AXPPReport report = new AXPPReport
            {
                Name = reportName,
                Methods = methods
            };
            
            Reports.Add(report);
            
            // Add methods to the main methods list
            Methods.AddRange(methods);
        }

        private void ParseRawXpp(string content)
        {
            // Basic class pattern
            string classPattern = @"class\s+(\w+)(?:\s+extends\s+(\w+))?\s*{([^}]*)}";
            
            // Method pattern
            string methodPattern = @"(?:public|private|protected|internal)?\s*(?:static)?\s*(\w+)\s+(\w+)\s*\(([^)]*)\)\s*{([^}]*)}";
            
            // Table pattern
            string tablePattern = @"table\s+(\w+)\s*{([^}]*)}";
            
            // Parse classes
            MatchCollection classMatches = Regex.Matches(content, classPattern, RegexOptions.Singleline);
            foreach (Match match in classMatches.Cast<Match>())
            {
                string className = match.Groups[1].Value;
                string extendsClass = match.Groups[2].Success ? match.Groups[2].Value : "";
                string classBody = match.Groups[3].Value;
                
                // Find methods in the class
                List<AXPPMethod> methods = new List<AXPPMethod>();
                MatchCollection methodMatches = Regex.Matches(classBody, methodPattern, RegexOptions.Singleline);
                
                foreach (Match methodMatch in methodMatches.Cast<Match>())
                {
                    string returnType = methodMatch.Groups[1].Value;
                    string methodName = methodMatch.Groups[2].Value;
                    string parameters = methodMatch.Groups[3].Value;
                    string methodBody = methodMatch.Groups[4].Value;
                    
                    AXPPMethod method = new AXPPMethod
                    {
                        Name = methodName,
                        Parameters = parameters,
                        ReturnType = returnType,
                        Body = methodBody,
                        ClassName = className
                    };
                    
                    methods.Add(method);
                }
                
                // Add to appropriate collection
                if (!string.IsNullOrEmpty(extendsClass))
                {
                    Extensions.Add(new AXPPExtension
                    {
                        Name = className,
                        ExtendsClass = extendsClass,
                        Methods = methods
                    });
                }
                else
                {
                    Classes.Add(new AXPPClass
                    {
                        Name = className,
                        Methods = methods
                    });
                }
                
                // Add methods to main methods list
                Methods.AddRange(methods);
            }
            
            // Parse tables
            MatchCollection tableMatches = Regex.Matches(content, tablePattern, RegexOptions.Singleline);
            foreach (Match match in tableMatches.Cast<Match>())
            {
                string tableName = match.Groups[1].Value;
                string tableBody = match.Groups[2].Value;
                
                // Extract fields (simplified)
                string fieldPattern = @"field\s+(\w+)\s*{([^}]*)}";
                List<AXPPField> fields = new List<AXPPField>();
                
                MatchCollection fieldMatches = Regex.Matches(tableBody, fieldPattern, RegexOptions.Singleline);
                foreach (Match fieldMatch in fieldMatches.Cast<Match>())
                {
                    string fieldName = fieldMatch.Groups[1].Value;
                    string fieldBody = fieldMatch.Groups[2].Value;
                    
                    // Try to extract type
                    Match typeMatch = Regex.Match(fieldBody, @"type\s*=\s*(\w+)");
                    string fieldType = typeMatch.Success ? typeMatch.Groups[1].Value : "";
                    
                    fields.Add(new AXPPField
                    {
                        Name = fieldName,
                        Type = fieldType
                    });
                }
                
                // Find methods in the table
                List<AXPPMethod> methods = new List<AXPPMethod>();
                MatchCollection methodMatches = Regex.Matches(tableBody, methodPattern, RegexOptions.Singleline);
                
                foreach (Match methodMatch in methodMatches.Cast<Match>())
                {
                    string returnType = methodMatch.Groups[1].Value;
                    string methodName = methodMatch.Groups[2].Value;
                    string parameters = methodMatch.Groups[3].Value;
                    string methodBody = methodMatch.Groups[4].Value;
                    
                    AXPPMethod method = new AXPPMethod
                    {
                        Name = methodName,
                        Parameters = parameters,
                        ReturnType = returnType,
                        Body = methodBody,
                        TableName = tableName
                    };
                    
                    methods.Add(method);
                }
                
                // Add to tables collection
                AXPPTable table = new AXPPTable
                {
                    Name = tableName,
                    Fields = fields,
                    Methods = methods
                };
                
                Tables.Add(table);
                
                // Add methods to main methods list
                Methods.AddRange(methods);
            }
        }
    }

    public class AXPPClass
    {
        public string Name { get; set; } = string.Empty;
        public List<AXPPMethod> Methods { get; set; } = new List<AXPPMethod>();
    }

    public class AXPPMethod
    {
        public string Name { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        
        // Container references (only one will be populated)
        public string ClassName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
    }

    public class AXPPTable
    {
        public string Name { get; set; } = string.Empty;
        public List<AXPPField> Fields { get; set; } = new List<AXPPField>();
        public List<AXPPMethod> Methods { get; set; } = new List<AXPPMethod>();
    }

    public class AXPPField
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class AXPPExtension
    {
        public string Name { get; set; } = string.Empty;
        public string ExtendsClass { get; set; } = string.Empty;
        public List<AXPPMethod> Methods { get; set; } = new List<AXPPMethod>();
    }

    public class AXPPForm
    {
        public string Name { get; set; } = string.Empty;
        public List<AXPPMethod> Methods { get; set; } = new List<AXPPMethod>();
    }

    public class AXPPQuery
    {
        public string Name { get; set; } = string.Empty;
        public List<AXPPDataSource> DataSources { get; set; } = new List<AXPPDataSource>();
    }

    public class AXPPDataSource
    {
        public string Name { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
    }

    public class AXPPReport
    {
        public string Name { get; set; } = string.Empty;
        public List<AXPPMethod> Methods { get; set; } = new List<AXPPMethod>();
    }
}