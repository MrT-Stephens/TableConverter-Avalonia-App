<h1 align="center">
  <br>
  TableConverter
  <br>
</h1>

<h4 align="center">A cross-platform desktop application for converting between tabular formats such as (CSV, JSON, XML, etc), generating random data, and exporting it in various tabular file types.</h4>

<h5 align="center">
  <i>
    TableConverter is the successor to one of my other projects made in C++ and wxWidgets called <a href="https://github.com/MrT-Stephens/Csv-to-Desktop-Application">Csv to</a> – A conversion tool for CSV files.
  </i>
</h4>

<p align="center">
  <a href="#how-it-was-made">How It Was Made</a> ▪︎
  <a href="#key-features">Key Features</a> ▪︎
  <a href="#features">Features</a> ▪︎
  <a href="#supported-file-formats">Supported File Formats</a> ▪︎
  <a href="#installation">Installation</a> ▪︎
  <a href="#how-to-use">How To Use</a> ▪︎
  <a href="#libraries-used">Libraries Used</a>
</p>

<p align="center">
  <img src="https://github.com/user-attachments/assets/1c399110-7e26-47ef-bfd5-ceeda1b18e31" alt="drawing" align="center"/>
</p>

## How It Was Made

TableConverter was developed using the following technologies and architecture:

### Technologies Used:
- **<a href="https://dotnet.microsoft.com/en-us/languages/csharp">C#</a>**: The core language for building the application logic and handling file conversions, data generation, and UI interactions.
- **<a href="https://avaloniaui.net/">Avalonia UI</a>**: A cross-platform UI framework for building the application's graphical interface. Avalonia enables TableConverter to run smoothly on Windows, macOS, and Linux.
- **<a href="https://github.com/kikipoulet/SukiUI">Suki UI</a>**: A customizable UI toolkit used to build modern, visually appealing user interfaces that enhance the user experience.

### Project Structure:
The TableConverter project is split into four main parts:

1. **TableConverter Project (Main UI)**  
   This is the core part of the application, which contains the Avalonia-based user interface. It handles all interactions and visualizations, enabling users to work with different file formats and data generation.

2. **TableConverter.Desktop Project**  
   This solution compiles the TableConverter Avalonia application into a desktop executable for multiple platforms, ensuring seamless cross-platform compatibility.

3. **TableConverter.FileConverters Project**  
   A library responsible for the conversion logic, including the handling of different file formats (CSV, JSON, SQL, etc.). It provides the backend functionality for importing, exporting, and transforming tabular data.

4. **TableConverter.DataGeneration Project**  
   A library dedicated to generating random data for tabular datasets. It leverages **FakerJs** to provide various field types and locale-specific data, allowing users to generate realistic datasets easily.

### Testing Projects:
In addition to the main application, several testing projects are used to ensure the reliability and correctness of the conversion and data generation functionality. These testing solutions verify that the input/output operations and data transformations work as expected across various file formats.

## Key Features

- **Convert Between Multiple Tabular Formats**  
  Effortlessly transform data between formats like CSV, JSON, XML, Markdown, and SQL, with support for input and output in most formats.

- **Built-in Data Generation**  
  Create realistic tabular datasets with 159 supported field types and locale-specific data (en, en_GB, zh_CN).

- **Customizable Editing Tools**  
  Edit raw files, tabular data, and exported files directly within the application.

- **Export to Popular File Types**  
  Save or copy data in formats such as Excel (.xlsx), JSONLines (.jsonl), HTML, and more.

- **Cross-Platform Support**  
  Built with Avalonia and Suki UI, TableConverter runs seamlessly on Windows, macOS, and Linux.

## Features

### File Conversion Workflow
- **Flexible Input Handling**  
  - Load multiple files for batch processing.  
  - Formats include CSV, JSON, XML, SQL, Markdown, YAML, and others.  
  - Input editor allows direct modification of file content before conversion.

![Input Handling](https://github.com/user-attachments/assets/6f81a42a-e503-452a-819b-85dea35b6e31)

- **Tabular Data Editing**  
  - Edit data in a structured grid interface after conversion.  
  - Fine-tune content or adjust formats for specific needs.  

![Tabular Data Editing](https://github.com/user-attachments/assets/a525f8cc-607a-4910-a829-00fff2e5f5f5)

- **Output Options**

  - Export files to various formats, including both document-based (PDF, Word) and tabular (CSV, Excel).  
  - Supports export-only formats like LaTeX and ASCII tables.

![Output Options](https://github.com/user-attachments/assets/33eaa8b3-ab08-4423-ad78-e4a1c1add47a)

### Data Generation Capabilities
- **Mockaroo-Inspired Interface**  
  - Add fields to the data grid intuitively.  
  - Choose from 159 data types, such as names, dates, addresses, and custom fields.  

![Data Generation UI](https://github.com/user-attachments/assets/df635f39-fdfb-4c70-a56b-4e4f22505f24)

- **Locale Support**  
  - Generate data specific to locales like `en`, `en_GB`, and `zh_CN`.  

- **Integrated Workflow**  
  - Automatically add generated data into the conversion pipeline for further editing and exporting.

## Supported File Formats

- **CSV**: ✅ Input | ✅ Output  
- **SQL**: ✅ Input | ✅ Output  
- **ASCII Tables**: ❌ Input | ✅ Output  
- **Markdown**: ❌ Input | ✅ Output  
- **XML**: ✅ Input | ✅ Output  
- **HTML**: ✅ Input | ✅ Output  
- **LaTeX**: ❌ Input | ✅ Output  
- **JSON**: ✅ Input | ✅ Output  
- **JSONLines**: ✅ Input | ✅ Output  
- **Excel**: ✅ Input | ✅ Output  
- **Word**: ✅ Input | ✅ Output  
- **PHP**: ✅ Input | ✅ Output  
- **Ruby**: ✅ Input | ✅ Output  
- **ASP**: ✅ Input | ✅ Output  
- **Multi-Line**: ✅ Input | ✅ Output  
- **PDF**: ❌ Input | ✅ Output  
- **YAML**: ✅ Input | ✅ Output  

> **Notes:**
> **Input and Output**: Formats with ✅ in both columns support both importing and exporting.  
> **Output Only**: Formats like Markdown, LaTeX, and ASCII tables are output-only.  

## Installation

*Instructions for installation go here.*

## How To Use

*Instructions on how to use the application go here.*

## Libraries Used

*Details about the libraries and frameworks used go here.*
