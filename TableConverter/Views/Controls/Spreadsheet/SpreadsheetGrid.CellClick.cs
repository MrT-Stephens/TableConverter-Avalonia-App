namespace TableConverter.Views.Controls.Spreadsheet;

public partial class SpreadsheetGrid
{
    private void UpdateHighlightRange()
    {
        if (_ClickCellRowId == -1 || _ClickCellColId == -1)
        {
            _singleCellSelectionBorder.IsVisible = false;
            return;
        }

        _singleCellSelectionBorder.IsVisible = true;
        _singleCellSelectionBorder.Width = SheetData.GetColWidthInUi((uint)_ClickCellColId);
        _singleCellSelectionBorder.Height = SheetData.GetRowHeightInUi((uint)_ClickCellRowId);

        // Adjust position to account for pixel-level scrolling
        double topPosition = CalculateRowPositionY(_ClickCellRowId);

        //Set the border position, taking into account pixel-level scrolling
        SetTop(_singleCellSelectionBorder, topPosition);
        SetLeft(_singleCellSelectionBorder, _SingleCellSelectionBorderLeft);

        _log.Debug($"Updating highlight at row={_ClickCellRowId}, position={topPosition}");
    }

    // private bool isInEditMode = false;
    private void UpdateEditorTextBox()
    {
        if (_ClickCellRowId == -1 || _ClickCellColId == -1)
        {
            _editorTextBox.IsVisible = false;
            return;
        }

        _editorTextBox.IsVisible = true;
        _editorTextBox.ZIndex = 1;
        _editorTextBox.Width = SheetData.GetColWidthInUi((uint)_ClickCellColId);
        _editorTextBox.Height = SheetData.GetRowHeightInUi((uint)_ClickCellRowId);
        _editorTextBox.Text = SheetData.GetCellText(_ClickCellRowId, _ClickCellColId);

        // Adjust position to account for pixel-level scrolling
        double topPosition = CalculateRowPositionY(_ClickCellRowId);

        //Set the text box position, taking into account pixel-level scrolling
        SetTop(_editorTextBox, topPosition);
        SetLeft(_editorTextBox, _SingleCellSelectionBorderLeft);

        // isInEditMode = true;
    }

    private void UpdateClearHighlightRange()
    {
        _ClickCellRowId = -1;
        _ClickCellColId = -1;
        _singleCellSelectionBorder.IsVisible = false;
        _editorTextBox.IsVisible = false;
        _editorTextBox.Text = "";
        Focus();
    }
}