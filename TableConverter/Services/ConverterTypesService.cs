using System.Collections.Generic;
using System.Threading.Tasks;
using TableConverter.DataModels;

namespace TableConverter.Services
{
    internal static class ConverterTypesService
    {
        public static Task<List<ConverterType>> GetConverterTypesAsync()
        {
            return Task.FromResult(new List<ConverterType>()
            {
                new ConverterType("CSV", [ ".csv", ".txt" ], [ "text/csv" ],
                        "CSV stands for Comma-Separated Values. CSV file format is a text file that has a specific format which allows data to be saved in a table structured format.",
                        new ConverterHandlerCsvInputService(), new ConverterHandlerCsvOutputService()),
                new ConverterType("SQL", [ ".sql", ".txt" ], [ "text/plain" ],
                        "SQL stands for Structured Query Language. It is used for storing, retrieving, managing and manipulating data in relational database management system (RDMS).",
                        new ConverterHandlerSQLInputService(), new ConverterHandlerSQLOutputService()),
                new ConverterType("Ascii Tables", [ ".txt" ], [ "text/plain" ],
                        "ASCII stands for American Standard Code for Information Interchange,It is a code for representing 128 English characters as numbers, with each letter assigned a number from 0 to 127.",
                        null, new ConverterHandlerAsciiOutputService()),
                new ConverterType("Markdown", [ ".md" ], [ "text/markdown" ],
                        "Markdown is a text-to-HTML conversion tool for web writers. Markdown allows you to write using an easy-to-read, easy-to-write plain text format, then convert it to HTML.",
                        null, new ConverterHandlerMarkdownOutputService()),
                new ConverterType("XML", [ ".xml" ], [ "text/xml" ],
                        "XML stands for eXtensible Markup Language. XML file is a markup language much like HTML and it was designed to store and transport data.",
                        new ConverterHandlerXmlInputService(), new ConverterHandlerXmlOutputService()),
                new ConverterType("HTML", [ ".html" ], [ "text/html" ],
                        "HTML stands for Hypertext Markup Language. HTML is the code that is used to structure a web page and its content, paragraphs, list, images and tables etc.",
                        new ConverterHandlerHtmlInputService(), new ConverterHandlerHtmlOutputService()),
                new ConverterType("LaTeX", [ ".tex" ], [ "application/x-tex" ],
                        "LaTeX is a typesetting and document preparation system that includes features designed for the production of technical and scientific documentation, LaTeX allows typesetting math easily.",
                        null, new ConverterHandlerLaTexOutputService()),
                new ConverterType("JSON", [ ".json" ], [ "application/json" ],
                        "JSON stands for JavaScript Object Notation. JSON file is a text-based format for representing structured data based on JavaScript object syntax.",
                        new ConverterHandlerJsonInputService(), new ConverterHandlerJsonOutputService()),
                new ConverterType("JSONLines", [ ".jsonl" ], [ "application/x-jsonlines" ],
                        "JSON Lines is a convenient format for storing structured data that may be processed one record at a time. It works well with unix-style text processing tools and shell pipelines. It's a great format for log files. It's also a flexible format for passing messages between cooperating processes.",
                        new ConverterHandlerJsonLinesInputService(), new ConverterHandlerJsonLinesOutputService()),
                new ConverterType("Excel", [ ".xlsx" ], [ "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ],
                        "Microsoft Excel is an electronic spreadsheet application that enables users to store, organize, calculate and manipulate the data with formulas using a spreadsheet system broken up by rows and columns.",
                        new ConverterHandlerExcelInputService(), new ConverterHandlerExcelOutputService()),
                new ConverterType("Word", [ ".docx" ], [ "application/vnd.openxmlformats-officedocument.wordprocessingml.document" ],
                        "Microsoft Word is a word processor developed by Microsoft. It was first released on October 25, 1983 under the name Multi-Tool Word for Xenix systems.",
                        null, null),
                new ConverterType("PHP", [ ".php" ], [ "" ],
                        "PHP (recursive acronym for PHP Hypertext Preprocessor ) is a widely-used open source general-purpose scripting language that is especially suited for web development and can be embedded into HTML.",
                        null, null),
                new ConverterType("Ruby", [ ".rb" ], [ "" ],
                        "Ruby is a dynamic, open source programming language with a focus on simplicity and productivity. It has an elegant syntax that is natural to read and easy to write.",
                        null, null),
                new ConverterType("ASP", [ ".asp" ], [ "" ],
                        "ASP stands for Active Server Pages. ASP is a development framework for building web pages. ASP supports many different development models Classic ASP. ASP.NET Web Forms.",
                        null, null),
                new ConverterType("Multi-Line", [ ".txt" ], [ "" ],
                        "Multi-Line is a text file that has a specific format which allows data to be saved in a table structured format.",
                        null, null),
                new ConverterType("PDF", [ ".pdf" ], [ "" ],
                        "PDF stands for Portable Document Format. PDF is a file format developed by Adobe in the 1990s to present documents, including text formatting and images, in a manner independent of application software, hardware, and operating systems.",
                        null, null),
                new ConverterType("YAML", [ ".yaml" ], [ "" ],
                        "YAML stands for YAML Ain't Markup Language. YAML is a human-readable data serialization language. It is commonly used for configuration files and in applications where data is being stored or transmitted.",
                        null, null)
            });
        }
    }
}
