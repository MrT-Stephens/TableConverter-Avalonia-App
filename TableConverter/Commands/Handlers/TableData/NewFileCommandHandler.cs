using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;
using TableConverter.Extensions;
using TableConverter.Utilities.Database.Contexts;
using TableConverter.ViewModels.Documents;
using TableConverter.ViewModels.Forms;
using TableConverter.ViewModels.Workspaces;

namespace TableConverter.Commands.Handlers.TableData;

public static partial class TableDataCommandNames
{
    public const string NewFile = "TableData.NewFile";
}

public class NewFileCommandHandler(
    ISukiDialogManager dialogManager,
    ISukiToastManager toastManager,
    Utilities.Database.Interfaces.IDatabaseContextFactory<TableStoreDbContext> databaseContextFactory) 
    : ICommandHandlerAsync
{
    public ICommandMetadata CommandMetadata => new CommandMetadata(
        TableDataCommandNames.NewFile,
        "New File",
        "Add a new table data file.",
        "AddFile",
        "File",
        0,
        ["Ctrl+N"]);
    
    public bool CanExecute(object? parameter, ICommandContext context)
    {
        return context.Parent is TableWorkspaceEditorViewModel;
    }

    public async Task Execute(object? parameter, ICommandContext context)
    {
        if (context.Parent is not TableWorkspaceEditorViewModel editorViewModel)
        {
            return;
        }

        var settings = new NewFileSettingsTableDataForm();

        var result = await dialogManager.CreateDialog()
            .WithTitle("Add New File")
            .WithForm(settings)
            .Dismiss().ByClickingBackground()
            .WithOkResult("Ok")
            .TryShowAsync();

        if (result is false)
        {
            return;
        }

        if (editorViewModel.CreateNewDocumentInstance() is not TableDataViewModel document)
        {
            throw new InvalidOperationException("Document should be of type TableDataViewModel");
        }

        document.Title = settings.Name;

        await using var dbContext = await databaseContextFactory.CreateAsync(document.Path);

        await Task.Run(() => GenerateTableData(dbContext, settings));
        
        editorViewModel.Documents.Add(document);
        editorViewModel.SelectedDocument = document;
        document.InvalidateData();

        toastManager.CreateSimpleInfoToast()
            .OfType(NotificationType.Success)
            .WithTitle("Added New File")
            .WithContent("The new file has been created successfully.")
            .Queue();
    }

    private static async Task GenerateTableData(TableStoreDbContext dbContext, NewFileSettingsTableDataForm settings)
    {
        const string newFileSql = """
            -- CREATE TEMPORARY NUMBERS TABLE
            CREATE TEMP TABLE IF NOT EXISTS NUMBERS (
                N INTEGER PRIMARY KEY
            );
            
            -- POPULATE NUMBERS TABLE
            WITH RECURSIVE SEQ(N) AS (
                SELECT 1
                UNION ALL
                SELECT N + 1
                FROM SEQ
                WHERE N < MAX(@HEADERS, @ROWS)
            )
            INSERT INTO NUMBERS (N)
            SELECT N
            FROM SEQ;
            
            -- INSERT COLUMNS
            INSERT INTO COLUMNS (NAME, DATA_TYPE, ORDINAL_POSITION)
            SELECT
                'Column ' || N,
                0,
                N
            FROM NUMBERS
            WHERE N <= @HEADERS;
            
            -- INSERT ROWS
            INSERT INTO ROWS (ID)
            SELECT N
            FROM NUMBERS
            WHERE N <= @ROWS;
            
            -- INSERT CELLS
            INSERT INTO CELLS (COLUMN_ID, ROW_ID, VALUE)
            SELECT
                C.N AS COLUMN_ID,
                R.N AS ROW_ID,
                CASE @FILL_MODE
                    WHEN 1 THEN CAST(C.N * R.N AS TEXT)                         -- MULTIPLY
                    WHEN 2 THEN CAST(((R.N - 1) * @HEADERS + C.N) AS TEXT)      -- SEQUENTIAL
                    WHEN 3 THEN CAST(ABS(RANDOM()) % 100 AS TEXT)               -- RANDOM
                    WHEN 4 THEN CAST(C.N AS TEXT)                               -- COLUMN NUMBERS
                    WHEN 5 THEN CAST(R.N AS TEXT)                               -- ROW NUMBERS
                    ELSE NULL                                                   -- EMPTY
                END AS VALUE
            FROM NUMBERS C
            CROSS JOIN NUMBERS R
            WHERE C.N <= @HEADERS AND R.N <= @ROWS;
            
            -- CLEAN UP
            DROP TABLE NUMBERS;
            """;
        
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(newFileSql, 
                new SqliteParameter("@HEADERS", settings.Headers), 
                new SqliteParameter("@ROWS", settings.Rows),
                new SqliteParameter("@FILL_MODE", (int)settings.FillMode));

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}