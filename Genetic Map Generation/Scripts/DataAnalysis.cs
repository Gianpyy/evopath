using System.IO;
using Godot;
using OfficeOpenXml;

//GA_1: SinglePoint crossover_10populationSize

public class DataAnalysis
{
    

    //private int count = 0;
    
    public void TestWrite(int [] fitnessArray)
    {
        // Impostazione del contesto della licenza EPPlus
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        FileInfo file = new FileInfo("Data/GAData.xlsx");

        using (var package = new ExcelPackage(file))
        {
            var worksheet = package.Workbook.Worksheets[0]; // Sostituisci "Sheet1" con il nome del foglio di lavoro desiderato
        
            GD.Print("Lunghezza array:"+fitnessArray.Length);

            for(int i = 0, offset = 7; i < fitnessArray.Length; i++, offset++)
            {
                worksheet.Cells["C"+offset].Value = i+1;
                worksheet.Cells["D"+offset].Value = fitnessArray[i];
            }
        

            // Salvataggio delle modifiche
            package.Save();
        }

    }
   
        
}