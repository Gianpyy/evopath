using System;
using System.Collections.Generic;
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
    

    /// <summary>
	/// Inserisce i dati in un documento excel nel foglio corrispondente alla configurazione.
    /// Se non c'è un foglio corrispondente, ne crea uno nuovo
	/// </summary>
	/// <param name="fitnessArray">I valori di fitness da inserire nel foglio</param> 
    /// <param name="geneticAlgorithmConfiguration">La configurazione dell'algoritmo genetico</param>
    public void WriteDataInSheet(GeneticAlgorithmData geneticAlgorithmData, GeneticAlgorithmConfiguration geneticAlgorithmConfiguration)
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
            }
            else
            {
                worksheetIndex = package.Workbook.Worksheets.Count;
                InsertDataInNewSheet(geneticAlgorithmData, geneticAlgorithmConfiguration, package);
                package.Save();

                string successString = "[color=green]Dati inseriti con successo in Foglio" + (worksheetIndex + 1) + ". [/color]";
                GD.PrintRich(successString);
            }
        }
        catch (Exception e) when (e is IOException || e is InvalidOperationException)
        {
            string errorString = "[color=red]Impossibie scrivere in "+ file.Name + " perché il file è attualmente aperto da un'altra applicazione. Chiudere e riprovare. [/color]";
            GD.PrintRich(errorString);
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
        int columnToInsertData = GetExcelColumnNumber(FitnessArrayStartingCol);
        while (worksheet.Cells[DataStartingRow, columnToInsertData].Value != null)
        {
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
        for ( ; i < geneticAlgorithmData.fitnessArray.Length; i++, offset++)
        {
            worksheet.Cells[offset, columnToInsertData].Value = "";
        }
        offset = 0;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = geneticAlgorithmData.bestMapFitness;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = geneticAlgorithmData.pathLenght;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = geneticAlgorithmData.numberOfCorners;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++, columnToInsertData].Value = geneticAlgorithmData.numberOfObstacles;
        worksheet.Cells[DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset, columnToInsertData].Value = geneticAlgorithmData.elapsedTime.ToString(@"hh\:mm\:ss\:fffff");


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
        for ( ; i < geneticAlgorithmData.fitnessArray.Length; i++, offset++)
        {
            worksheet.Cells[GenerationNumberCol + offset].Value = i + 1;
                worksheet.Cells[FitnessArrayStartingCol + offset].Value = "";
        }
        offset = 0;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = geneticAlgorithmData.bestMapFitness;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = geneticAlgorithmData.pathLenght;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = geneticAlgorithmData.numberOfCorners;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset++)].Value = geneticAlgorithmData.numberOfObstacles;
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow + geneticAlgorithmConfiguration.generationLimit + offset)].Value = geneticAlgorithmData.elapsedTime.ToString(@"hh\:mm\:ss\:fffff");

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
        
}