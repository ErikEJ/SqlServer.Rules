namespace ErikEJ.DacFX.TSQLAnalyzer;

internal readonly record struct ColumnAdjustment(int Line, int StartColumn, int Delta);
