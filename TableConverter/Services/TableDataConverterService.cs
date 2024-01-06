using System.Collections.Generic;
using System.Threading.Tasks;

namespace TableConverter.Services
{
    public class TableDataConverterService
    {
        public readonly int MaxUndoRedoCount = 20;

        private Stack<(string[], string[][])> UndoStack { get; set; }
        private Stack<(string[], string[][])> RedoStack { get; set; }

        public TableDataConverterService()
        {
            UndoStack = new Stack<(string[], string[][])>();
            RedoStack = new Stack<(string[], string[][])>();
        }

        private void AddUndo(string[] column_values, string[][] row_values)
        {
            if (UndoStack.Count >= MaxUndoRedoCount)
            {
                UndoStack.Pop();
            }

            string[] column_values_copy = new string[column_values.Length];
            string[][] row_values_copy = new string[row_values.Length][];

            column_values.CopyTo(column_values_copy, 0);

            for (int i = 0; i < row_values.Length; i++)
            {
                row_values_copy[i] = new string[row_values[i].Length];
                row_values[i].CopyTo(row_values_copy[i], 0);
            }

            UndoStack.Push((column_values_copy, row_values_copy));
        }

        private void AddRedo(string[] column_values, string[][] row_values)
        {
            if (RedoStack.Count >= MaxUndoRedoCount)
            {
                RedoStack.Pop();
            }

            RedoStack.Push((column_values, row_values));
        }

        public void ClearUndoRedo()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }

        public Task<(string[], string[][])> Undo()
        {
            return Task.Run(() =>
            {
                if (UndoStack.Count == 0)
                {
                    return (new string[] { }, new string[][] { });
                }

                var results = UndoStack.Pop();
                AddRedo(results.Item1, results.Item2);

                string[] column_values_copy = new string[results.Item1.Length];
                string[][] row_values_copy = new string[results.Item2.Length][];

                results.Item1.CopyTo(column_values_copy, 0);

                for (int i = 0; i < results.Item2.Length; i++)
                {
                    row_values_copy[i] = new string[results.Item2[i].Length];
                    results.Item2[i].CopyTo(row_values_copy[i], 0);
                }

                return (column_values_copy, row_values_copy);
            });
        }

        public Task<(string[], string[][])> Redo()
        {
            return Task.Run(() => 
            {
                if (RedoStack.Count == 0)
                {
                    return (new string[] { }, new string[][] { });
                }

                var results = RedoStack.Pop();
                AddUndo(results.Item1, results.Item2);

                string[] column_values_copy = new string[results.Item1.Length];
                string[][] row_values_copy = new string[results.Item2.Length][];

                results.Item1.CopyTo(column_values_copy, 0);

                for (int i = 0; i < results.Item2.Length; i++)
                {
                    row_values_copy[i] = new string[results.Item2[i].Length];
                    results.Item2[i].CopyTo(row_values_copy[i], 0);
                }

                return (column_values_copy, row_values_copy);
            });
        }

        public Task<(string[], string[][])> Capitalize(string[] column_values, string[][] row_values)
        {
            return Task.Run(() =>
            {
                AddUndo(column_values, row_values);

                for (int i = 0; i < column_values.Length; i++)
                {
                    column_values[i] = char.ToUpper(column_values[i][0]) + column_values[i].Substring(1);
                }

                foreach (string[] row in row_values)
                {
                    for (int i = 0; i < column_values.Length; i++)
                    {
                        row[i] = char.ToUpper(row[i].ToString()[0]) + row[i].ToString().Substring(1);
                    }
                }

                return (column_values, row_values);
            });
        }

        public Task<(string[], string[][])> Uppercase(string[] column_values, string[][] row_values)
        {
            return Task.Run(() =>
            {
                AddUndo(column_values, row_values);

                for (int i = 0; i < column_values.Length; i++)
                {
                    column_values[i] = column_values[i].ToUpper();
                }

                foreach (string[] row in row_values)
                {
                    for (int i = 0; i < column_values.Length; i++)
                    {
                        row[i] = row[i].ToUpper();
                    }
                }

                return (column_values, row_values);
            });
        }

        public Task<(string[], string[][])> Lowercase(string[] column_values, string[][] row_values)
        {
            return Task.Run(() =>
            {
                AddUndo(column_values, row_values);

                for (int i = 0; i < column_values.Length; i++)
                {
                    column_values[i] = column_values[i].ToLower();
                }

                foreach (string[] row in row_values)
                {
                    for (int i = 0; i < column_values.Length; i++)
                    {
                        row[i] = row[i].ToLower();
                    }
                }

                return (column_values, row_values);
            });
        }

        public Task<(string[], string[][])> DeleteDuplicateRows(string[] column_values, string[][] row_values)
        {
            return Task.Run(() =>
            {
                AddUndo(column_values, row_values);

                var uniqueRows = new List<string[]>();

                foreach (string[] row in row_values)
                {
                    if (!uniqueRows.Contains(row))
                    {
                        uniqueRows.Add(row);
                    }
                }

                row_values = uniqueRows.ToArray();

                return (column_values, row_values);
            });
        }

        public Task<(string[], string[][])> Transpose(string[] column_values, string[][] row_values)
        {
            return Task.Run(() =>
            {
                AddUndo(column_values, row_values);

                var newColumnValues = new string[row_values.Length];
                var newRowValues = new string[column_values.Length][];

                for (int i = 0; i < row_values.Length; i++)
                {
                    newColumnValues[i] = row_values[i][0];
                }

                for (int i = 0; i < column_values.Length; i++)
                {
                    newRowValues[i] = new string[row_values.Length];

                    for (int j = 0; j < row_values.Length; j++)
                    {
                        newRowValues[i][j] = row_values[j][i];
                    }
                }

                return (newColumnValues, newRowValues);
            });
        }

        public Task<(string[], string[][])> DeleteSpaces(string[] column_values, string[][] row_values)
        {
            return Task.Run(() =>
            {
                AddUndo(column_values, row_values);

                for (int i = 0; i < column_values.Length; i++)
                {
                    column_values[i] = column_values[i].Replace(" ", "");
                }

                foreach (string[] row in row_values)
                {
                    for (int i = 0; i < column_values.Length; i++)
                    {
                        row[i] = row[i].Replace(" ", "");
                    }
                }

                return (column_values, row_values);
            });
        }
    }
}
