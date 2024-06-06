using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using OfficeOpenXml;

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
    private const double GeneticAlgorithmConfigurationInfoColWidth = 20.50;
    private const double GeneticAlgorithmConfigurationDataColWidth = 9.40;
    private const double GenerationNumberColWidth = 10.60;
    private const double FitnessArrayColWidth = 21.00;
    

    /// <summary>
	/// Inserisce i dati in un documento excel nel foglio corrispondente alla configurazione.
    /// Se non c'è un foglio corrispondente, ne crea uno nuovo
	/// </summary>
	/// <param name="fitnessArray">I valori di fitness da inserire nel foglio</param> 
    /// <param name="geneticAlgorithmConfiguration">La configurazione dell'algoritmo genetico</param>
    public void WriteDataInSheet(int [] fitnessArray, GeneticAlgorithmConfiguration geneticAlgorithmConfiguration)
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
                InsertDataInExsistingSheet(fitnessArray, worksheet);
                package.Save();

                string successString = "[color=green]Dati inseriti con successo in " + file.Name + ". [/color]";
                GD.PrintRich(successString);
            }
            else
            {
                InsertDataInNewSheet(fitnessArray, geneticAlgorithmConfiguration, package);
                package.Save();

                string successString = "[color=green]Dati inseriti con successo in " + file.Name + ". [/color]";
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
    private void InsertDataInExsistingSheet(int[] fitnessArray, ExcelWorksheet worksheet)
    {
        // Cerco la colonna in cui inserire i dati
        int columnToInsertData = GetExcelColumnNumber(FitnessArrayStartingCol);
        while (worksheet.Cells[DataStartingRow, columnToInsertData].Value != null)
        {
            columnToInsertData++;
        }
        
        // Inserimento dati 
        worksheet.Cells[DataStartingRow - 1, columnToInsertData].Value = "Fitness Miglior Individuo";
        
        for (int i = 0, offset = DataStartingRow; i < fitnessArray.Length; i++, offset++)
        {
            worksheet.Cells[GenerationNumberCol + offset].Value = i + 1;
            worksheet.Cells[offset, columnToInsertData].Value = fitnessArray[i];
        }

        // -- Formattazione stile -- 

        // Larghezza colonne
        worksheet.Column(columnToInsertData).Width = FitnessArrayColWidth;
        
        // Formattazione testo 
        worksheet.Cells[DataStartingRow - 1, columnToInsertData].Style.Font.Bold = true;
        ExcelRange range = worksheet.Cells[DataStartingRow - 1, columnToInsertData, DataStartingRow + fitnessArray.Length, columnToInsertData];
        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
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
            for (int sheetIndex = DataStartingRow, arrayIndex = 0; sheetIndex < DataStartingRow + fields.Length; sheetIndex++, arrayIndex++)
            {
                string parameterToCheck = (string) worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + sheetIndex].Value;

                double parameterValue = (double) worksheet.Cells[GeneticAlgorithmConfigurationDataCol + sheetIndex].Value;
                int configurationValue = (int) fieldsDictionary[parameterToCheck];   

                if (parameterValue != configurationValue)
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
    private void InsertDataInNewSheet(int[] fitnessArray, GeneticAlgorithmConfiguration geneticAlgorithmConfiguration, ExcelPackage package)
    {
        int worksheetCount = package.Workbook.Worksheets.Count;
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Foglio" + (worksheetCount + 1));

        // -- Inserimento dei dati relativi alla configurazione dell'algoritmo genetico --

        // Inserimento indice
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1)].Value = "Configurazione Algoritmo Genetico";


        // Inserimento dati
        Type type = typeof(GeneticAlgorithmConfiguration);
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0, offset = DataStartingRow; i < fields.Length; i++, offset++)
        {
            string fieldName = fields[i].Name;
            object fieldValue = fields[i].GetValue(geneticAlgorithmConfiguration);

            worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + offset].Value = fieldName;
            worksheet.Cells[GeneticAlgorithmConfigurationDataCol + offset].Value = fieldValue;
        }

        // -- Inserimento dei dati di fitness --

        // Inserimento indice
        worksheet.Cells[GenerationNumberCol + (DataStartingRow - 1)].Value = "Generazione";
        worksheet.Cells[FitnessArrayStartingCol + (DataStartingRow - 1)].Value = "Fitness Miglior Individuo";

        // Inserimento dati
        for (int i = 0, offset = DataStartingRow; i < fitnessArray.Length; i++, offset++)
        {
            worksheet.Cells[GenerationNumberCol + offset].Value = i + 1;
            worksheet.Cells[FitnessArrayStartingCol + offset].Value = fitnessArray[i];
        }

        // -- Formattazione stile -- 

        // Larghezza colonne
        int colIndex = GetExcelColumnNumber(GeneticAlgorithmConfigurationInfoCol);
        worksheet.Column(colIndex).Width = GeneticAlgorithmConfigurationInfoColWidth;
        colIndex = GetExcelColumnNumber(GeneticAlgorithmConfigurationDataCol);
        worksheet.Column(colIndex).Width = GeneticAlgorithmConfigurationDataColWidth;
        colIndex = GetExcelColumnNumber(GenerationNumberCol);
        worksheet.Column(colIndex).Width = GenerationNumberColWidth;
        colIndex = GetExcelColumnNumber(FitnessArrayStartingCol);
        worksheet.Column(colIndex).Width = FitnessArrayColWidth;

        // Formattazione testo 
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1) + ":" + GeneticAlgorithmConfigurationDataCol + (DataStartingRow - 1)].Merge = true;
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1)].Style.Font.Bold = true;
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1)].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1)].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);


        ExcelRange range = worksheet.Cells[GeneticAlgorithmConfigurationInfoCol + (DataStartingRow - 1) + ":" + GeneticAlgorithmConfigurationDataCol + (DataStartingRow + fields.Length - 1)];
        range.Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

        range = worksheet.Cells[GenerationNumberCol + (DataStartingRow - 1) + ":" + FitnessArrayStartingCol + (DataStartingRow + fitnessArray.Length)];
        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        range.Style.Font.Bold = true;
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