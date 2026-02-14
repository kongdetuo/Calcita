using Avalonia.Media;
using Avalonia.Media.Imaging;
using Calcita.CellTypes;
using Calcita.Chart;
using Calcita.Drawing.Shapes;
using Calcita.Graphics;
using Calcita.IO.OpenXML.Schema;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcita.Demo.ViewModel
{
    internal class MainViewModel : ReactiveUI.ReactiveObject
    {
        public MainViewModel()
        {
            this.Workbook = new Workbook();

            Workbook.Worksheets.Clear();

            // add demo sheet 1: document template
            AddDemoSheet1();

            // add demo sheet 2: chart and drawing
            AddDemoSheet2();

            // add demo sheet 3: cell types
            AddDemoSheet3();
            
            // add demo sheet 4: Hatch Style
            AddDemoSheet4();
        }


        public Workbook Workbook { get; set => this.RaiseAndSetIfChanged(ref field, value); }


        #region Demo Sheet 1 : Document Template
        private void AddDemoSheet1()
        {
            /****************** Sheet1 : Document Template ********************/
            var worksheet = Workbook.AddWorksheet("Document");

            // load template
            using (MemoryStream ms = new MemoryStream(Properties.Resources.order_sample))
            {
                worksheet.LoadRGF(ms);
            }

            // fill data into worksheet
            var dataRange = worksheet.Ranges["A21:F35"];

            dataRange.Data = new object[,]
            {
                {"[23423423]", "Product ABC", 15, 150},
                {"[45645645]", "Product DEF", 1, 75},
                {"[78978978]", "Product GHI", 2, 30},
            };

            // set subtotal formula
            worksheet.Cells["G21"].Formula = "E21*F21";

            // auto fill other subtotals
            worksheet.AutoFillSerial("G21", "G22:G35");
        }
        #endregion // Demo Sheet 1 : Document Template

        #region Demo Sheet 2 : Chart & Drawing
        private void AddDemoSheet2()
        {
            /****************** Sheet2 : Chart & Drawing ********************/
            var worksheet = Workbook.AddWorksheet("Chart & Drawing");

            worksheet["A2"] = new object[,] {
                    {null, 2008,  2009, 2010, 2011, 2012},
                    {"City 1",  5,  10, 12, 11, 14},
                    {"City 2",  7,  8,  7,  6,  4},
                    {"City 3",  13, 10, 9,  10, 9},
                    {"Total", 25, 28, 28, 27, 27},
            };

            worksheet.AddOutline(RowOrColumn.Row, 3, 4);

            var range = worksheet.Ranges["B3:F6"];
            worksheet.AddHighlightRange(range);

            var chart = new Chart.LineChart
            {
                Location = new Point(360, 140),

                Title = "Line Chart Sample",

                DataSource = new WorksheetChartDataSource(worksheet, "A2:A6", "B3:F6")
                {
                    CategoryNameRange = new RangePosition("B2:F2"),
                },
            };

            worksheet.FloatingObjects.Add(chart);

            // flow chart
            Line line1, line2;

            worksheet.FloatingObjects.Add(new RectangleShape
            {
                Location = new Graphics.Point(100, 200),
                Size = new Graphics.Size(160, 40),

                Text = "1. Add Data Source",
            });

            worksheet.FloatingObjects.Add(line1 = new Line
            {
                StartPoint = new Graphics.Point(180, 240),
                EndPoint = new Graphics.Point(180, 270),
            });

            worksheet.FloatingObjects.Add(new RectangleShape
            {
                Location = new Graphics.Point(100, 270),
                Size = new Graphics.Size(160, 40),

                Text = "2. Create Data Source",
            });

            worksheet.FloatingObjects.Add(line2 = new Line
            {
                StartPoint = new Graphics.Point(180, 310),
                EndPoint = new Graphics.Point(180, 340),
            });

            worksheet.FloatingObjects.Add(new RectangleShape
            {
                Location = new Graphics.Point(100, 340),
                Size = new Graphics.Size(160, 40),

                Text = "3. Create and Put Chart",
            });

            // not available yet
            //line1.Style.EndCap = Graphics.LineCapStyles.Arrow;
            //line2.Style.EndCap = Graphics.LineCapStyles.Arrow;
        }
        #endregion // Demo Sheet 2 : Chart & Drawing

        #region Demo Sheet 3 : Built-in Cell Types
        private void AddDemoSheet3()
        {
            /****************** Sheet3 : Built-in Cell Types ********************/
            var worksheet = Workbook.AddWorksheet("Cell Types");

            // set default sheet style
            worksheet.SetRangeStyles(RangePosition.EntireRange, new WorksheetRangeStyle
            {
                Flag = PlainStyleFlag.FontName | PlainStyleFlag.VerticalAlign,
                FontName = "Arial",
                VAlign = ReoGridVerAlign.Middle,
            });

            worksheet.SetSettings(WorksheetSettings.View_ShowGridLine |
                 WorksheetSettings.Edit_DragSelectionToMoveCells, false);
            worksheet.SelectionMode = WorksheetSelectionMode.Cell;
            worksheet.SelectionStyle = WorksheetSelectionStyle.FocusRect;

            var middleStyle = new WorksheetRangeStyle
            {
                Flag = PlainStyleFlag.Padding | PlainStyleFlag.HorizontalAlign,
                Padding = new PaddingValue(2),
                HAlign = ReoGridHorAlign.Center,
            };

            var grayTextStyle = new WorksheetRangeStyle
            {
                Flag = PlainStyleFlag.TextColor,
                TextColor = Colors.DimGray
            };

            worksheet.MergeRange(1, 1, 1, 6);

            worksheet.SetRangeStyles(1, 1, 1, 6, new WorksheetRangeStyle
            {
                Flag = PlainStyleFlag.TextColor | PlainStyleFlag.FontSize,
                TextColor = Colors.DarkGreen,
                FontSize = 18,
            });

            worksheet[1, 1] = "Built-in Cell Bodies";

            worksheet.SetColumnsWidth(1, 1, 100);
            worksheet.SetColumnsWidth(2, 1, 30);
            worksheet.SetColumnsWidth(3, 1, 100);
            worksheet.SetColumnsWidth(6, 2, 65);

            // button
            worksheet.MergeRange(3, 2, 1, 2);
            var btn = new ButtonCell("Hello");
            worksheet[3, 1] = new object[] { "Button: ", btn };
            btn.Click += (s, e) => ShowText(worksheet, "Button clicked.");

            // link
            worksheet.MergeRange(5, 2, 1, 3);
            var link = new HyperlinkCell("http://www.google.com");
            worksheet[5, 1] = new object[] { "Hyperlink", link };

            // checkbox
            var checkbox = new CheckBoxCell();
            worksheet.SetRangeStyles(7, 2, 1, 1, middleStyle);
            worksheet.SetRangeStyles(8, 2, 1, 1, grayTextStyle);
            worksheet[7, 1] = new object[] { "Check box", checkbox, "Auto destroy after 5 minutes." };
            worksheet[8, 2] = "(Keyboard is also supported to change the status of control)";
            checkbox.CheckChanged += (s, e) => ShowText(worksheet, "Check box switch to " + checkbox.IsChecked.ToString());

            // radio & radio group
            worksheet[10, 1] = "Radio Button";
            worksheet.SetRangeStyles(10, 2, 3, 1, middleStyle);
            var radioGroup = new RadioButtonGroup();
            worksheet[10, 2] = new object[,] {
                {new RadioButtonCell() { RadioGroup = radioGroup }, "Apple"},
                {new RadioButtonCell() { RadioGroup = radioGroup }, "Orange"},
                {new RadioButtonCell() { RadioGroup = radioGroup }, "Banana"}
            };
            radioGroup.RadioButtons.ForEach(rb => rb.CheckChanged += (s, e) =>
                ShowText(worksheet, "Radio button selected: " + worksheet[rb.Cell.Row, rb.Cell.Column + 1]));
            worksheet[10, 2] = true;
            worksheet[13, 2] = "(By adding radio buttons into same RadioGroup to make them toggle each other automatically)";
            worksheet.SetRangeStyles(13, 2, 1, 1, grayTextStyle);

            // dropdown - Not available yet - Planned from next version
            //worksheet.MergeRange(15, 2, 1, 3);
            //var dropdown = new DropdownListCell("Apple", "Orange", "Banana", "Pear", "Pumpkin", "Cherry", "Coconut");
            //worksheet[15, 1] = new object[] { "Dropdown", dropdown };
            //worksheet.SetRangeBorders(15, 2, 1, 3, BorderPositions.Outside, RangeBorderStyle.GraySolid);

            // custom cell type - slide cell body
            worksheet.MergeRange(15, 2, 1, 2);
            worksheet[15, 1] = new object[] { "Brightness", new SlideCellBody() };
            worksheet[15, 2] = 1;
            worksheet.CellDataChanged += (s, e) =>
            {
                if (e.Cell.Position == new CellPosition(15, 2))
                {
                    byte val = (byte)(worksheet.GetCellData<double>(e.Cell.Position) * 255);
                    worksheet.SetRangeStyles(RangePosition.EntireRange, new WorksheetRangeStyle
                    {
                        Flag = PlainStyleFlag.BackColor,
                        BackColor = new Graphics.SolidColor(val, val, val),
                    });
                }
            };

            // image
            worksheet.MergeRange(2, 6, 5, 2);

            Bitmap image;

            using (MemoryStream memory = new MemoryStream(Properties.Resources.computer_laptop_png))
            {
                image = new Bitmap(memory);
            }

            worksheet[2, 6] = new ImageCell(image);

            // information cell
            worksheet.SetRangeBorders(19, 0, 1, 10, BorderPositions.Top, RangeBorderStyle.GraySolid);
        }

        private void ShowText(Worksheet sheet, string text)
        {
            sheet[19, 0] = text;
        }
        #endregion // Demo Sheet 3 : Built-in Cell Types


        private void AddDemoSheet4()
        {
            var worksheet = Workbook.AddWorksheet("Hatch Style");

            // set default sheet style
            worksheet.SetRangeStyles(RangePosition.EntireRange, new WorksheetRangeStyle
            {
                Flag = PlainStyleFlag.FontName | PlainStyleFlag.VerticalAlign,
                FontName = "Arial",
                VAlign = ReoGridVerAlign.Middle,
            });

            worksheet[1,1] = "Hatch Style Sample";

            // 生成两列，第一列是样式名称，第二列是对应的样式预览

            var hatchStyles = Enum.GetValues(typeof(HatchStyles)).Cast<HatchStyles>();

            var row = 2;
            foreach (var style in hatchStyles)
            {
                worksheet[row, 1] = style.ToString();

                worksheet[row, 2] = "1";
                worksheet.SetRangeStyles(row, 2, 10, 1, new WorksheetRangeStyle
                {
                    Flag = PlainStyleFlag.FillPatternStyle | PlainStyleFlag.FillPatternColor,
                    FillPatternStyle = style,
                    FillPatternColor = Colors.Black,
                    BackColor = Colors.Red,
                });

                

                // 设置行高以适应样式预览
                worksheet.SetRowsHeight(row, 1, 80);

                row++;
            }

        }

    }
}
