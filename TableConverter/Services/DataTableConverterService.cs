using System.Collections.Generic;
using System.Data;

namespace TableConverter.Services
{
    public class DataTableConverterService
    {
        public readonly int MaxUndoRedoCount = 20;

        private Stack<DataTable> UndoStack { get; set; }
        private Stack<DataTable> RedoStack { get; set; }

        public DataTableConverterService()
        {
            UndoStack = new Stack<DataTable>();
            RedoStack = new Stack<DataTable>();
        }

        private void AddUndo(DataTable dataTable)
        {
            if (UndoStack.Count >= MaxUndoRedoCount)
            {
                UndoStack.Pop();
            }

            DataTable copy = dataTable.Copy();

            UndoStack.Push(copy);
        }

        private void AddRedo(DataTable dataTable)
        {
            if (RedoStack.Count >= MaxUndoRedoCount)
            {
                RedoStack.Pop();
            }

            DataTable copy = dataTable.Copy();

            RedoStack.Push(copy);
        }

        public void ClearUndoRedo()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }

        public void Undo(DataTable dataTable)
        {
            if (UndoStack.Count == 0)
            {
                return;
            }

            var undoDataTable = UndoStack.Pop();
            AddRedo(dataTable);

            dataTable = undoDataTable;
        }

        public void Redo(DataTable dataTable)
        {
            if (RedoStack.Count == 0)
            {
                return;
            }

            var redoDataTable = RedoStack.Pop();
            AddUndo(dataTable);

            dataTable = redoDataTable;
        }

        public void Capitalize(DataTable dataTable)
        {
            AddUndo(dataTable);

            foreach (DataColumn column in dataTable.Columns)
            {
                column.ColumnName = char.ToUpper(column.ColumnName[0]) + column.ColumnName.Substring(1);
            }

            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    row[i] = char.ToUpper(row[i].ToString()[0]) + row[i].ToString().Substring(1);
                }
            }
        }

        public void Uppercase(DataTable dataTable)
        {
            AddUndo(dataTable);

            foreach (DataColumn column in dataTable.Columns)
            {
                column.ColumnName = column.ColumnName.ToUpper();
            }

            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    row[i] = row[i].ToString().ToUpper();
                }
            }
        }

        public void Lowercase(DataTable dataTable)
        {
            AddUndo(dataTable);

            foreach (DataColumn column in dataTable.Columns)
            {
                column.ColumnName = column.ColumnName.ToLower();
            }

            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    row[i] = row[i].ToString().ToLower();
                }
            }
        }

        public void DeleteDuplicateRows(DataTable dataTable)
        {
            AddUndo(dataTable);

            var uniqueRows = new List<DataRow>();

            foreach (DataRow row in dataTable.Rows)
            {
                if (!uniqueRows.Contains(row))
                {
                    uniqueRows.Add(row);
                }
            }

            dataTable.Rows.Clear();

            foreach (DataRow row in uniqueRows)
            {
                dataTable.Rows.Add(row);
            }
        }

        public void Transpose(DataTable dataTable)
        {
            AddUndo(dataTable);

            var newDataTable = new DataTable();

            foreach (DataColumn column in dataTable.Columns)
            {
                newDataTable.Columns.Add(column.ColumnName);
            }

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var newRow = newDataTable.NewRow();

                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    newRow[j] = dataTable.Rows[j][i];
                }

                newDataTable.Rows.Add(newRow);
            }

            dataTable.Rows.Clear();

            foreach (DataRow row in newDataTable.Rows)
            {
                dataTable.Rows.Add(row);
            }
        }

        public void DeleteSpaces(DataTable dataTable)
        {
            AddUndo(dataTable);

            foreach (DataColumn column in dataTable.Columns)
            {
                column.ColumnName = column.ColumnName.Replace(" ", "");
            }

            foreach (DataRow row in dataTable.Rows)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    row[i] = row[i].ToString().Replace(" ", "");
                }
            }
        }
    }
}
