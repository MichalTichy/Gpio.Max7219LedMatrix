namespace Raspberry.IO.Max7219LedMatrix.Module
{
    public interface IMax7219MatrixModule
    {
        int ModuleNumber { get; }
        Max7219MatrixModule ApplyPreprocessing();
        IMax7219MatrixModule ShiftModuleRight(uint columnCount);
        IMax7219MatrixModule ShiftModuleLeft(uint columnCount);
        IMax7219MatrixModule ShiftModuleUp(uint rowCount);
        IMax7219MatrixModule ShiftModuleDown(uint rowCount);
        IMax7219MatrixModule FlipModuleVertical();
        IMax7219MatrixModule FlipModuleHorizontal();
        IMax7219MatrixModule InvertModule();
        IMax7219MatrixModule SetRow(uint rowId, byte value);
        byte GetRow(uint rowId);
        IMax7219MatrixModule Fill();
        void Set(byte[] data);
        byte[] Get();
        IMax7219MatrixModule ClearModule();
        uint GetRowCount();
        uint GetColumnCount();
        void SetColumn(uint column, byte data);
        byte GetColumn(uint column);
    }
}