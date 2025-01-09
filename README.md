<h1 align="center">
  <br>
  TableConverter
  <br>
</h1>

<h4 align="center">A cross-platform desktop application for converting between tabular formats such as (CSV, JSON, XML, etc), generating random data, and exporting it in various tabular file types.</h4>

<p align="center">
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
  - 
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

<div style="min-width: 100%; width: 100%; overflow-x: auto;">
  <table style="width: 100%; border-collapse: collapse; text-align: left; table-layout: fixed;">
    <thead>
      <tr>
        <th style="border: 1px solid #ddd; padding: 8px;">Format</th>
        <th style="border: 1px solid #ddd; padding: 8px;">Input</th>
        <th style="border: 1px solid #ddd; padding: 8px;">Output</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">CSV</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">JSON</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">JSONLines</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">XML</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">SQL</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">HTML</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">Markdown</td>
        <td style="border: 1px solid #ddd; padding: 8px;">❌</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">LaTeX</td>
        <td style="border: 1px solid #ddd; padding: 8px;">❌</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">Ascii Tables</td>
        <td style="border: 1px solid #ddd; padding: 8px;">❌</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">YAML</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">Excel</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">Word</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">PDF</td>
        <td style="border: 1px solid #ddd; padding: 8px;">❌</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">PHP</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">Ruby</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">ASP</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
      <tr>
        <td style="border: 1px solid #ddd; padding: 8px;">Multi-Line</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
        <td style="border: 1px solid #ddd; padding: 8px;">✅</td>
      </tr>
    </tbody>
  </table>
</div>

> **Notes:**
> **Input and Output**: Formats with ✅ in both columns support both importing and exporting.  
> **Output Only**: Formats like Markdown, LaTeX, and ASCII tables are output-only.  

## Installation

*Instructions for installation go here.*

## How To Use

*Instructions on how to use the application go here.*

## Libraries Used

*Details about the libraries and frameworks used go here.*
