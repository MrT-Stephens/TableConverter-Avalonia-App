using NPOI.Util;
using Org.BouncyCastle.Crypto.Engines;
using System.Collections.Generic;
using System.Linq;
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

            UndoStack.Push((column_values.Copy(), row_values.Copy()));
        }

        private void AddRedo(string[] column_values, string[][] row_values)
        {
            if (RedoStack.Count >= MaxUndoRedoCount)
            {
                RedoStack.Pop();
            }

            RedoStack.Push((column_values.Copy(), row_values.Copy()));
        }

        public void ClearUndoRedo()
        {
            UndoStack = new Stack<(string[], string[][])>();
            RedoStack = new Stack<(string[], string[][])>();
        }

        public Task<(string[], string[][])> Undo(string[] column_values, string[][] row_values)
        {
            return Task.Run(() =>
            {
                if (UndoStack.Count == 0)
                {
                    return (null, null);
                }

                var results = UndoStack.Pop();

                AddRedo(column_values.Copy(), row_values.Copy());

                return (results.Item1.Copy(), results.Item2.Copy());
            });
        }

        public Task<(string[], string[][])> Redo(string[] column_values, string[][] row_values)
        {
            return Task.Run(() => 
            {
                if (RedoStack.Count == 0)
                {
                    return (null, null);
                }

                var results = RedoStack.Pop();

                AddUndo(column_values.Copy(), row_values.Copy());

                return (results.Item1.Copy(), results.Item2.Copy());
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
                    if (!uniqueRows.Any((unique_row) => unique_row.SequenceEqual(row)))
                    {
                        uniqueRows.Add(row);
                    }
                }

                return (column_values, uniqueRows.ToArray());
            });
        }

        public Task<(string[], string[][])> Transpose(string[] column_values, string[][] row_values)
        {
            return Task.Run(() =>
            {
                AddUndo(column_values, row_values);

                // Merge the column and row values into a single array
                string[][] merged_data = new string[row_values.Length + 1][];

                merged_data[0] = column_values;

                for (int i = 0; i < row_values.Length; i++)
                {
                    merged_data[i + 1] = row_values[i];
                }

                string[][] transposed_data = new string[column_values.Length][];

                // Rotate the array 90 degrees anti-clockwise
                for (int i = 0; i < transposed_data.Length; i++)
                {
                    for (int j = 0; j < merged_data.Length; j++)
                    {
                        if (transposed_data[i] == null)
                        {
                            transposed_data[i] = new string[merged_data.Length];
                        }

                        transposed_data[i][j] = merged_data[j][i];
                    }
                }

                // Re-assign the column and row values
                column_values = new string[transposed_data[0].Length];

                column_values = transposed_data[0];

                row_values = new string[transposed_data.Length - 1][];

                for (int i = 1; i < transposed_data.Length; i++)
                {
                    row_values[i - 1] = new string[transposed_data[i].Length];
                    row_values[i - 1] = transposed_data[i];
                }

                // Return the new column and row values
                return (column_values, row_values);
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
