using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace googlesheetApp
{
    static class Program
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "CrudApp";
        static string SpreadSheetId = "1v4RW80yut-FOF-cNXnmCp9MK7cIUQhrlQ7rKjjTXbpU";
        static string SheetName = "congress";
        static SheetsService sheetsService;
        static GoogleCredential credential;
        static void Main()
        {
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            // Create Google Sheets API service.
            sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            ReadEntries();
            //CreateEntries();
            //UpdateEntries();
            //DeleteEntries();
        }
        static void ReadEntries()
        {
            SpreadsheetsResource.ValuesResource.GetRequest request = sheetsService.Spreadsheets.Values.Get(SpreadSheetId, SheetName);
            ValueRange response = request.Execute();
            var values = response.Values;
            DimensionRange dimensionRange = new DimensionRange();
            dimensionRange.Dimension = response.MajorDimension;
            //foreach (var val in values)
            //{
            //    Console.WriteLine(val.ToString());
            //}
            Console.WriteLine(JsonConvert.SerializeObject(response));
        }
        static void CreateEntries()
        {
            var Range = $"{SheetName}!A:F";
            var f = new List<Object>() { 1, 2, 3 };
            ValueRange valueRange = new ValueRange();
            valueRange.Values = new List<IList<Object>> { f };
            var request = sheetsService.Spreadsheets.Values.Append(valueRange, SpreadSheetId, SheetName /*Range*/);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var response = request.Execute();
        }
        static void UpdateEntries()
        {
            var Range = $"{SheetName}!D543";
            var f = new List<Object>() { 1, 2, 3 };

            ValueRange valueRange = new ValueRange();
            valueRange.Values = new List<IList<Object>> { f };
            var request = sheetsService.Spreadsheets.Values.Update(valueRange, SpreadSheetId, /*SheetName*/ Range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var response = request.Execute();
        }
        static void DeleteEntries()
        {
            var Range = $"{SheetName}!A543:F543";

            var requestBody = new ClearValuesRequest();
            var request = sheetsService.Spreadsheets.Values.Clear(requestBody, SpreadSheetId, /*SheetName*/ Range);
            var response = request.Execute();
        }
    }
}
