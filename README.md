# D365FO Code Reviewer

A tool to review D365FO X++ code against Functional Design Documents (FDD) to identify discrepancies and ensure implementation matches design requirements.

## Features

- **FDD Document Analysis**: Parses PDF, DOCX, or text-based FDD documents to extract requirements, functions, entities, and extensions.
- **X++ Code Analysis**: Parses .axpp project files to extract classes, methods, tables, extensions, forms, queries, and reports.
- **Comprehensive Comparison**: Compares implementation against the design to identify:
  - Missing entities (classes/tables)
  - Missing functions/methods
  - Parameter mismatches
  - Unimplemented requirements
  - Missing extensions
- **Detailed Reporting**: Generates both JSON and HTML reports with:
  - Discrepancies categorized by severity
  - Implemented features with their locations
  - Missing features list
  - Analysis notes for developers
- **User-Friendly Interface**: Simple Windows Forms UI to:
  - Select FDD and X++ files
  - View results in a clear, categorized format
  - Export findings to CSV for further analysis

## Requirements

- .NET 6.0 or higher
- Windows operating system
- Required packages:
  - DocumentFormat.OpenXml
  - iTextSharp
  - Newtonsoft.Json
  - System.IO.Compression

## Installation

1. Clone this repository
2. Build the solution using Visual Studio
3. Run the compiled executable

## Usage

1. Launch the application
2. Select a Functional Design Document (PDF, DOCX, or TXT format)
3. Select the D365FO project/solution file (.axpp or .xml format)
4. Choose an output location for the reports
5. Click "Start Code Review"
6. Review the results in the application interface
7. Use the generated JSON and HTML reports for sharing findings

## How It Works

1. **FDD Parsing**:
   - Extracts requirements, function specifications, entity definitions, and expected extensions
   - Uses NLP-like techniques to identify implied requirements when explicit ones aren't present

2. **X++ Code Parsing**:
   - Parses .axpp files (typically ZIP archives containing XML definitions)
   - Alternatively, can parse raw X++ code or XML files
   - Extracts classes, methods, tables, extensions, and other artifacts

3. **Comparison Engine**:
   - Entity matching considering common naming variations and prefixes/suffixes
   - Method matching with parameter compatibility checks
   - Requirement matching using keyword extraction and matching

4. **Report Generation**:
   - Categorizes findings with appropriate severity levels
   - Generates detailed reports in JSON and HTML formats

## Limitations

- While the tool attempts to parse various FDD formats, more structured FDDs produce better results
- Complex X++ code patterns may not be fully analyzed
- The analysis is based on static code and doesn't consider runtime behavior
- The tool currently only supports English-language documents

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.