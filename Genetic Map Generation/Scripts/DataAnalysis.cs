using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using OfficeOpenXml;
using OfficeOpenXml.Style;

//GA_1: SinglePoint crossover_10populationSize

public class DataAnalysis
{
    // Il path del file excel in cui vogliamo scrivere
    private const string FileName = "Data/GAData.xlsx";

    // Indici celle
    private const string GeneticAlgorithmConfigurationInfoCol = "B";
    private const string GeneticAlgorithmConfigurationDataCol = "C";
    private const string GenerationNumberCol = "G";
    private const string FitnessArrayStartingCol = "H";
    private const int DataStartingRow = 6;

    // Larghezze celle
    private const double GeneticAlgorithmConfigurationInfoColWidth = 34.00;
    private const double GeneticAlgorithmConfigurationDataColWidth = 16.00;
    private const double GenerationNumberColWidth = 20.30;
    private const double FitnessArrayColWidth = 21.00;
    private const double AdditionalDataColWidth = 13.50;
    

    /// <summary>
	/// Inserisce i dati in un documento excel nel foglio corrispondente alla configurazione.
    /// Se non c'è un foglio corrispondente, ne crea uno nuovo
	/// </summary>
	/// <param name="fitnessArray">I valori di fitness da inserire nel foglio</param> 
    /// <param name="geneticAlgorithmConfiguration">La configurazione dell'algoritmo genetico</param>
    public int WriteDataInSheet(GeneticAlgorithmData geneticAlgorithmData, GeneticAlgorithmConfiguration geneticAlgorithmConfiguration)
    {
        // Impostazione del contesto della licenza EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        FileInfo file = new FileInfo(FileName);
        using ExcelPackage package = new ExcelPackage(file);
        
        try
        {
            int worksheetIndex = CheckForCorrespondingSheet(geneticAlgorithmConfiguration, package);

            if (worksheetIndex != -1)
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets[worksheetIndex];
                InsertDataInExsistingSheet(geneticAlgorithmData, geneticAlgorithmConfiguration, worksheet);
                package.Save();

                string successString = "[color=green]Dati inseriti con successo in Foglio" + (worksheetIndex + 1) + ". [/color]";
                GD.PrintRich(successString);
                return worksheetIndex+1;
            }
            else
            {
                worksheetIndex = package.Workbook.Worksheets.Count;
                InsertDataInNewSheet(geneticAlgorithmData, geneticAlgorithmConfiguration, package);
                package.Save();

                string successString = "[color=green]Dati inseriti con successo in Foglio" + (worksheetIndex + 1) + ". [/color]";
                return worksheetIndex+1;
            }
        }
        catch (Exception e) when (e is IOException || e is InvalidOperationException)
        {
            string errorString = "[color=red]Impossibie scrivere in "+ file.Name + " perché il file è attualmente aperto da un'altra applicazione. Chiudere e riprovare. [/color]";
            GD.PrintRich(errorString);
            return 0;
        }   
    }


    /// <summary>
	/// Inserisce i dati in un documento excel nel foglio corrispondente alla configurazione.
	/// </summary>
	/// <param name="fitnessArray">I valori di fitness da inserire nel foglio</param> 
    /// <param name="worksheet">Il foglio in cui inserire i dati</param>
    private void InsertDataInExsistingSheet(GeneticAlgorithmData geneticAlgorithmData, GeneticAlgorithmConfiguration geneticAlgorithmConfiguration ,ExcelWorksheet worksheet)
    {
        // Cerco la colonna in cui inserire i dati
        List<TimeSpan> elapsedTimes = new List<TimeSpan>();

        int columnToInsertData = GetExcelColumnNumber(FitnessArrayStartingCol);
        while (worksheet.Cells[DataStartingRow, columnToInsertData].Value != null)
        {
            // Nel frattempo mi salvo i valori del tempo di esecuzione per ogni colonna che trovo
            string cellValue = worksheet.Cells[41, columnToInsertData].Text;
            if (TimeSpan.TryParseExact(cellValue, @"hh\:mm\:ss\:fffff", CultureInfo.InvariantCulture, out TimeSpan timeSpan))
            {
                elapsedTimes.Add(timeSpan);
            }
            else
            {
                GD.Print($"Errore di conversione per il valore: {cellValue}");
            }

            columnToInsertData++;

        }
        
        // Inserimento dati 
        int offset = DataStartingRow;
        int i;
        worksheet.Cells[DataStartingRow - 1, columnToInsertData].Value = "Fitness Miglior Individuo";
        
        for (i = 0; i < geneticAlgorithmData.fitnessArray.Length; i++, offset++)
        {
            float valueToInsert = geneticAlgorithmData.fitnessArray[i];

            if (valueToInsert != 0)
            {
                worksheet.Cells[offset, columnToInsertData].Value = geneticAlgorithmData.fitnessArray[i];
            }
            else
            {
                break;
            }
        }
        int generationCount = i;
        for ( ; i < geneticAlgorithmData.fitnessArray.Length; i++, offset++)
        {
            worksheet.Cells[offset, columnToInsertData].Value = "";
        }
        offset = 0;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = generationCount;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = geneticAlgorithmData.bestMapFitness;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = geneticAlgorithmData.pathLenght;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = geneticAlgorithmData.numberOfCorners;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = geneticAlgorithmData.numberOfObstacles;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, columnToInsertData].Value = geneticAlgorithmData.elapsedTime.ToString(@"hh\:mm\:ss\:fffff");

        // Inserimento media e valore più alto
        elapsedTimes.Add(geneticAlgorithmData.elapsedTime);
        double averageTicks = elapsedTimes.Average(time => time.Ticks);
        double maxTicks = elapsedTimes.Max(time => time.Ticks);
        TimeSpan averageTime = TimeSpan.FromTicks((long) averageTicks);
        TimeSpan maxTime = TimeSpan.FromTicks((long) maxTicks);


        int AdditionalDataColumnIndex = GetExcelColumnNumber(GenerationNumberCol) - 2;
        int AdditionalDataRowIndex = DataStartingRow + geneticAlgorithmConfiguration.generationLimit - 1;
        string columnToInsertDataLetter = GetExcelColumnName(columnToInsertData);
        
        offset = 0;
        worksheet.Cells[AdditionalDataRowIndex, AdditionalDataColumnIndex].Value = "Media";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex].Formula = "AVERAGE("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex].Formula = "AVERAGE("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex].Formula = "AVERAGE("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex].Formula = "AVERAGE("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex].Formula = "AVERAGE("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex].Value = averageTime.ToString(@"hh\:mm\:ss\:fffff");

        offset = 0;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex + 1].Formula = "MAX("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex + 1].Formula = "MAX("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex + 1].Formula = "MAX("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex + 1].Formula = "MAX("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex + 1].Formula = "MAX("+FitnessArrayStartingCol+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)+":"+columnToInsertDataLetter+(DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)+")";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex + 1].Value = maxTime.ToString(@"hh\:mm\:ss\:fffff");


        // -- Formattazione stile -- 

        // Larghezza colonne
        worksheet.Column(columnToInsertData).Width = FitnessArrayColWidth;
        
        // Formattazione parte di sopra 
        worksheet.Cells[DataStartingRow - 1, columnToInsertData].Style.Font.Bold = true;
        ExcelRange range = worksheet.Cells[DataStartingRow - 1, columnToInsertData, DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, columnToInsertData];
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Formattazione parte di sotto
        range = worksheet.Cells[DataStartingRow + geneticAlgorithmData.fitnessArray.Length, columnToInsertData, DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, columnToInsertData];
        range.Style.Font.Bold = true;

        // Formattazione sfondo (parte di sotto)
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

        // Formattazione bordi (parte di sotto)
        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;

    }

    /// <summary>
	/// Cerca un foglio contenente la configurazione dell'algoritmo genetico corrispondente.
	/// </summary>
	/// <param name="geneticAlgorithmConfiguration">La configurazione dell'algoritmo genetico</param>
    /// <param name="package">Il package di Excel</param>
    /// <returns>L'indice del foglio contente la configurazione corrispondente, -1 altrimenti</returns>
    private int CheckForCorrespondingSheet(GeneticAlgorithmConfiguration geneticAlgorithmConfiguration, ExcelPackage package)
    {
        ExcelWorksheet worksheet;
        int worksheetIndex;
        bool sheetFound;

        // Converto la configurazione dell'algoritmo genetico in un dizionario
        Type type = typeof(GeneticAlgorithmConfiguration);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        Dictionary<string, object> fieldsDictionary = fields.ToDictionary(field => field.Name, field => field.GetValue(geneticAlgorithmConfiguration));

        // Controllo in ogni worksheet se trovo una configurazione uguale
        for (worksheetIndex = 0; worksheetIndex < package.Workbook.Worksheets.Count; worksheetIndex++)
        {
            worksheet = package.Workbook.Worksheets[worksheetIndex];
            sheetFound = true;

            // Controllo sul singolo foglio
            for (int sheetIndex = DataStartingRow; sheetIndex < DataStartingRow + fields.Length; sheetIndex++)
            {
                string parameterToCheck = (string) worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + sheetIndex].Value;
                if (parameterToCheck == null)
                {
                    sheetFound = false;
                    break;
                }

                object cellValue = worksheet.Cells[GeneticAlgorithmConfigurationDataCol + sheetIndex].Value;
                string parameterValue = cellValue != null ? cellValue.ToString() : string.Empty; 
                string configurationValue = fieldsDictionary[parameterToCheck].ToString();

                if (parameterValue.CompareTo(configurationValue) != 0)
                {
                    sheetFound = false;
                    break;
                }
            }

            if (sheetFound)
                return worksheetIndex;
                
        }
        
        return -1;
    }

    /// <summary>
	/// Inserisce i dati in un nuovo foglio.
	/// </summary>
    /// <param name="fitnessArray">I valori di fitness da inserire nel foglio</param> 
	/// <param name="geneticAlgorithmConfiguration">La configurazione dell'algoritmo genetico</param>
    /// <param name="package">Il package di Excel</param>
    private void InsertDataInNewSheet(GeneticAlgorithmData geneticAlgorithmData, GeneticAlgorithmConfiguration geneticAlgorithmConfiguration, ExcelPackage package)
    {
        int worksheetCount = package.Workbook.Worksheets.Count;
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Foglio" + (worksheetCount + 1));

        // -- Inserimento dei dati relativi alla configurazione dell'algoritmo genetico --

        // Inserimento indice
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1)].Value = "Configurazione Algoritmo Genetico";


        // Inserimento dati
        Type type = typeof(GeneticAlgorithmConfiguration);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        int offset = DataStartingRow;
        int i;
        for (i = 0; i < fields.Length; i++, offset++)
        {
            string fieldName = fields[i].Name;
            object fieldValue = fields[i].GetValue(geneticAlgorithmConfiguration);

            worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + offset].Value = fieldName;
            worksheet.Cells[GeneticAlgorithmConfigurationDataCol + offset].Value = fieldValue;
        }

        // -- Inserimento dei dati  --

        // Inserimento indici
        offset = DataStartingRow + geneticAlgorithmData.fitnessArray.Length;
        worksheet.Cells[GenerationNumberCol + (DataStartingRow - 1)].Value = "Generazione";
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow - 1)].Value = "Fitness Miglior Individuo";
        worksheet.Cells[GenerationNumberCol + offset++].Value = "Generazioni usate";
        worksheet.Cells[GenerationNumberCol + offset++].Value = "Fitness miglior mappa";
        worksheet.Cells[GenerationNumberCol + offset++].Value = "Lunghezza percorso";
        worksheet.Cells[GenerationNumberCol + offset++].Value = "Numero curve";
        worksheet.Cells[GenerationNumberCol + offset++].Value = "Numero ostacoli";
        worksheet.Cells[GenerationNumberCol + offset].Value = "Tempo di esecuzione";

        // Inserimento dati
        offset = DataStartingRow;
        for (i = 0; i < geneticAlgorithmData.fitnessArray.Length; i++, offset++)
        {
            float valueToInsert = geneticAlgorithmData.fitnessArray[i];

            if (valueToInsert != 0)
            {
                worksheet.Cells[GenerationNumberCol + offset].Value = i + 1;
                worksheet.Cells[FitnessArrayStartingCol + offset].Value = geneticAlgorithmData.fitnessArray[i];
            }
            else
            {
                break;
            }
        }
        int generationCount = i;
        for ( ; i < geneticAlgorithmData.fitnessArray.Length; i++, offset++)
        {
            worksheet.Cells[GenerationNumberCol + offset].Value = i + 1;
                worksheet.Cells[FitnessArrayStartingCol + offset].Value = "";
        }
        offset = 0;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = generationCount;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = geneticAlgorithmData.bestMapFitness;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = geneticAlgorithmData.pathLenght;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = geneticAlgorithmData.numberOfCorners;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = geneticAlgorithmData.numberOfObstacles;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)].Value = geneticAlgorithmData.elapsedTime.ToString(@"hh\:mm\:ss\:fffff");

        // Inserimento media e valore più alto
        int AdditionalDataColumnIndex = GetExcelColumnNumber(GenerationNumberCol) - 2;
        int AdditionalDataRowIndex = DataStartingRow + geneticAlgorithmConfiguration.generationLimit - 1;
        
        offset = 0;
        worksheet.Cells[AdditionalDataRowIndex, AdditionalDataColumnIndex].Value = "Media";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex].Value = generationCount;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex].Value = geneticAlgorithmData.bestMapFitness;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex].Value = geneticAlgorithmData.pathLenght;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex].Value = geneticAlgorithmData.numberOfCorners;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex].Value = geneticAlgorithmData.numberOfObstacles;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex].Value = geneticAlgorithmData.elapsedTime.ToString(@"hh\:mm\:ss\:fffff");

        offset = 0;
        worksheet.Cells[AdditionalDataRowIndex, AdditionalDataColumnIndex + 1].Value = "Highest";
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex + 1].Value = generationCount;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex + 1].Value = geneticAlgorithmData.bestMapFitness;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex + 1].Value = geneticAlgorithmData.pathLenght;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex + 1].Value = geneticAlgorithmData.numberOfCorners;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, AdditionalDataColumnIndex + 1].Value = geneticAlgorithmData.numberOfObstacles;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, AdditionalDataColumnIndex + 1].Value = geneticAlgorithmData.elapsedTime.ToString(@"hh\:mm\:ss\:fffff");



        // ---- Formattazione stile ---- 

        // Larghezza colonne
        int colIndex = GetExcelColumnNumber(GeneticAlgorithmConfigurationInfoCol);
        worksheet.Column(colIndex).Width = GeneticAlgorithmConfigurationInfoColWidth;
        colIndex = GetExcelColumnNumber(GeneticAlgorithmConfigurationDataCol);
        worksheet.Column(colIndex).Width = GeneticAlgorithmConfigurationDataColWidth;
        colIndex = GetExcelColumnNumber(GenerationNumberCol);
        worksheet.Column(colIndex).Width = GenerationNumberColWidth;
        colIndex = GetExcelColumnNumber(FitnessArrayStartingCol);
        worksheet.Column(colIndex).Width = FitnessArrayColWidth;

        // -- Formattazione dati relativi alla configurazione dell'algoritmo genetico --

        // Formattazione sfondo 
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1) + ":" + GeneticAlgorithmConfigurationDataCol + (DataStartingRow - 1)].Merge = true;
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1)].Style.Font.Bold = true;
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1)].Style.Fill.PatternType = ExcelFillStyle.Solid;
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1)].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

        // Formattazione bordo
        ExcelRange range = worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1) + ":" + GeneticAlgorithmConfigurationDataCol + (DataStartingRow - 1)];

        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        range = worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1) + ":" + GeneticAlgorithmConfigurationDataCol + (DataStartingRow + fields.Length - 1)];
        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);

        range = worksheet.Cells[GeneticAlgorithmConfigurationDataCol + DataStartingRow + ":" + GeneticAlgorithmConfigurationDataCol + (DataStartingRow + fields.Length - 1)];
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

        // -- Formattazione dati --

        // Formattazione parte di sopra
        range = worksheet.Cells[GenerationNumberCol + (DataStartingRow - 1) + ":" + FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)];
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        range = worksheet.Cells[GenerationNumberCol + (DataStartingRow - 1) + ":" + FitnessArrayStartingCol + (DataStartingRow -1)];
        range.Style.Font.Bold = true;

        // Formattazione parte di sotto
        range = worksheet.Cells[GenerationNumberCol + (DataStartingRow + geneticAlgorithmData.fitnessArray.Length) + ":" + FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)];
        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range.Style.Font.Bold = true;
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

        // Formattazione media e valore più alto
        worksheet.Column(AdditionalDataColumnIndex).Width = AdditionalDataColWidth;
        worksheet.Column(AdditionalDataColumnIndex + 1).Width = AdditionalDataColWidth;

        range = worksheet.Cells[AdditionalDataRowIndex, AdditionalDataColumnIndex, AdditionalDataRowIndex + 6, AdditionalDataColumnIndex + 1]; //TODO: no hardcoded
        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        range.Style.Font.Bold = true;
        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);

        range = worksheet.Cells[AdditionalDataRowIndex, AdditionalDataColumnIndex + 1, AdditionalDataRowIndex + 6, AdditionalDataColumnIndex + 1]; //TODO: no hardcoded
        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

    }



    /// <summary>
	/// Converte un indice letterale di excel in un indice numerico.
	/// </summary>
    /// <param name="columnName">L'indice in formato letterale da convertire</param>
    /// <returns>L'indice in formato intero</returns>
    public static int GetExcelColumnNumber(string columnName)
    {
        int columnNumber = 0;
        int pow = 1;

        for (int i = columnName.Length - 1; i >= 0; i--)
        {
            columnNumber += (columnName[i] - 'A' + 1) * pow;
            pow *= 26;
        }

        return columnNumber;
    }

    /// <summary>
	/// Converte un indice numerico di excel in un indice letterale.
	/// </summary>
    /// <param name="columnName">L'indice in formato numerico da convertire</param>
    /// <returns>L'indice in letterale intero</returns>
    private string GetExcelColumnName(int columnNumber)
    {
        string columnName = "";

        while (columnNumber > 0)
        {
            int modulo = (columnNumber - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            columnNumber = (columnNumber - modulo) / 26;
        } 

        return columnName;
    }
        
}