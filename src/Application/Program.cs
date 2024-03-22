// <copyright file="Program.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Allors.Excel;

namespace Application
{
    public class Program : IProgram
    {
        private readonly Dictionary<IWorksheet, Binder> binderByWorksheet;

        private int counter;

        public Program(IServiceLocator serviceLocator)
        {
            ServiceLocator = serviceLocator;
            binderByWorksheet = new Dictionary<IWorksheet, Binder>();
            this.counter = 0;
        }

        public IServiceLocator ServiceLocator { get; }

        public IAddIn AddIn { get; private set; }

        public async Task OnStart(IAddIn addIn)
        {
            AddIn = addIn;

            await Task.CompletedTask;
        }

        public async Task OnHandle(string handle, params object[] argument)
        {
            switch (handle)
            {
                case Actions.Dosomething:
                    Console.WriteLine("Boom!");
                    break;

                case Actions.Hide:
                    var worksheet = AddIn.Workbooks.First(v => v.IsActive).Worksheets.First(v => v.IsActive);

                    foreach (var index in Enumerable.Range(5, 5))
                    {
                        worksheet.Row(index).Hidden = true;
                    }

                    worksheet.Flush();

                    break;
            }
        }

        public async Task OnStop() => await Task.CompletedTask;

        public async Task OnNew(IWorkbook workbook)
        {
            CanWriteCellStyle = new Style(Color.LightBlue, Color.Black);
            CanNotWriteCellStyle = new Style(Color.MistyRose, Color.Black);
            ChangedCellStyle = new Style(Color.DeepSkyBlue, Color.Black);

            var sheet = workbook.AddWorksheet();
            //sheet.Name = $"{++counter}";

            for (var i = 0; i < 50; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    sheet[i, j].Value = decimal.Parse($"{i},{j}");
                    if (j == 0 || j == 2)
                    {
                        sheet[i, j].Style = CanWriteCellStyle;
                        sheet[i, j].NumberFormat = "#.###,00";
                    }
                    else
                    {
                        sheet[i, j].Style = CanNotWriteCellStyle;
                    }
                }
            }

            var style = new Style(Color.Red, Color.White);
            sheet[3, 3].Style = style;
            sheet[3, 5].Style = style;
            sheet[4, 4].Style = style;
            sheet[5, 3].Style = style;
            sheet[5, 5].Style = style;

            sheet.Flush();

            sheet[0, 0].Value = "Whoppa!";
            sheet[0, 0].Comment = "De Poppa!";

            sheet[10, 2].Style = CanNotWriteCellStyle;

            sheet[3, 12].Value = "Walter";
            sheet[3, 13].Value = "Martien";
            sheet[3, 14].Value = "Koen";

            sheet[2, 11].Value = "Person:";
            sheet[2, 12].Options = new Range(row: 3, column: 12, columns: 3, worksheet: sheet);

            if (!binderByWorksheet.TryGetValue(sheet, out var binder))
            {
                binder = new Binder(sheet);
                binderByWorksheet.Add(sheet, binder);
            }

            var binding = new Binding(toDomain: cell =>
            {
                var message = $"Binder toDomain: {cell.Row}:{cell.Column}";
            });
            binder.Set(5, 12, binding);


            for (var day = 1; day <= 31; ++day)
            {
                sheet[day + 5, 10].NumberFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                sheet[day + 5, 10].Value = new DateTime(2020, 7, day);
            }


            sheet.Flush();

            sheet.AutoFit();

            //sheet.CellsChanged += (sender, v) =>
            //{
            //    foreach (var cell in v.Cells)
            //    {
            //        cell.Style = this.ChangedCellStyle;
            //    }

            //    ((IWorksheet)sender).Flush();

            //    string message = $"Cells changed: {string.Join(",", v.Cells.Select(w => $"{w.Row}:{w.Column}"))}";
            //    this.ServiceLocator.Alerter.Alert(message);
            //};
        }

        public async Task OnNew(IWorksheet worksheet)
        {
            worksheet.Name = $"{++counter}";

            await Task.CompletedTask;
        }


        public Style CanNotWriteCellStyle { get; set; }

        public Style CanWriteCellStyle { get; set; }

        public Style ChangedCellStyle { get; set; }

        public void OnClose(IWorkbook workbook, ref bool cancel)
        {
        }

        public Task OnBeforeDelete(IWorksheet worksheet) => Task.CompletedTask;

        public async Task OnLogin() => await Task.CompletedTask;

        public async Task OnLogout() => await Task.CompletedTask;

        public bool IsEnabled(string controlId, string controlTag) => true;
    }
}
