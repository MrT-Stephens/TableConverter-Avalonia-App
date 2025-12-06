using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TableConverter.Commands.Handlers.TableData;
using TableConverter.Contracts;
using TableConverter.Interfaces;
using TableConverter.ViewModels.Base;
using TableConverter.ViewModels.Documents;

namespace TableConverter.ViewModels.Workspaces;

public partial class TableWorkspaceEditorViewModel : BaseWorkspaceEditorViewModel
{
    #region Constructors
    
    public TableWorkspaceEditorViewModel(IServiceProvider serviceProvider) 
        : base(serviceProvider, "Table Editor", "TableIcon", 2)
    {
    }
    
    #endregion

    #region Methods

    public override void Initialise()
    {
        base.Initialise();
        
        MainCommands.Add(this[TableDataCommandNames.NewFile]);
        MainCommands.Add(this[TableDataCommandNames.ImportFile]);
    }

    public override IPaneDocument CreateNewDocumentInstance()
    {
        return _serviceProvider.GetRequiredService<TableDataViewModel>();
    }

    protected override IPaneDocument CreateDefaultDocumentInstance()
    {
        if (CreateNewDocumentInstance() is not TableDataViewModel tableData)
        {
            throw new InvalidOperationException("Failed to create default TableDataViewModel instance.");
        }
        
        tableData.Title = "Example Table";
        tableData.TableData = new ObservableTableData(
            [
                "ID", "First Name", "Last Name", "Email", "Age", "Is 18 or Above", "Country", "City", "Occupation", "Salary", "Join Date",  
            ],
            [
                new List<object> { 1, "Alice",   "Johnson",  "alice.johnson@example.com", 29, true, "USA",     "New York",    "Engineer", 72000, new DateTime(2020, 3, 12) },
                new List<object> { 2, "Bob",     "Smith",    "bob.smith@example.com",     34, true, "UK",      "London",      "Designer", 65000, new DateTime(2019, 7, 25) },
                new List<object> { 3, "Charlie", "Davis",    "charlie.davis@example.com", 41, true, "Canada",  "Toronto",     "Manager", 85000, new DateTime(2018, 1, 5) },
                new List<object> { 4, "Diana",   "Evans",    "diana.evans@example.com",   16, false, "Germany", "Berlin",      "Analyst", 56000, new DateTime(2021, 4, 14) },
                new List<object> { 5, "Ethan",   "Brown",    "ethan.brown@example.com",   38, true, "France",  "Paris",       "Architect",95000, new DateTime(2017, 10, 30) },
                new List<object> { 6, "Fiona",   "Wilson",   "fiona.wilson@example.com",  31, true,  "Australia","Sydney",    "Scientist",78000, new DateTime(2019, 2, 19) },
                new List<object> { 7, "George",  "Miller",   "george.miller@example.com", 10, false, "USA",     "Chicago",    "Director", 102000, new DateTime(2016, 8, 9) },
                new List<object> { 8, "Hannah",  "Clark",    "hannah.clark@example.com",  25, true, "Spain",   "Madrid",     "Intern",   32000, new DateTime(2022, 5, 21) },
                new List<object> { 9, "Ian",     "Lopez",    "ian.lopez@example.com",     33, true, "Mexico",  "Monterrey",  "Developer",72000, new DateTime(2020, 9, 10) },
                new List<object> {10, "Julia",   "Taylor",   "julia.taylor@example.com",  28, true, "Italy",   "Rome",       "Consultant",64000, new DateTime(2021, 1, 3) }
            ]
        );

        return tableData;
    }

    #endregion
}