using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Calcita.Controls;
using Calcita.Demo.ViewModel;

namespace Calcita.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new ViewModel.MainViewModel();
            // don't use Clear method in actual application,
            // instead, load template into the first worksheet directly.


            // handles event to update menu check status.
            grid.PropertyChanged += (s, e) => UpdateMenuChecks();

            grid.GetObservable(CalcitaControl.CurrentWorksheetProperty).Subscribe(new AnonymousObserver<Worksheet?>(ws => UpdateMenuChecks()));

        }

        private void UpdateMenuChecks()
        {
            this.viewFormulaBarVisible.IsChecked = grid.FormulaBarVisible;
            this.viewSheetTabVisible.IsChecked = grid.HorizontalScrollBarVisible;// workbook.HasSettings(Calcita.WorkbookSettings.View_ShowHorScroll);
            this.viewVerticalScrollbarVisible.IsChecked = grid.VerticalScrollBarVisible;// workbook.HasSettings(Calcita.WorkbookSettings.View_ShowVerScroll);
            this.viewSheetTabVisible.IsChecked = grid.SheetTabVisible;
            this.viewSheetTabNewButtonVisible.IsChecked = grid.SheetTabNewButtonVisible;

            var sheet = grid.CurrentWorksheet;
            this.viewGuideLineVisible.IsChecked = sheet?.HasSettings(WorksheetSettings.View_ShowGridLine) == true;
            this.viewPageBreaksVisible.IsChecked = sheet?.HasSettings(WorksheetSettings.View_ShowPageBreaks) == true;
        }

        #region Menu - File
        private void File_New_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainViewModel)this.DataContext!;
            vm.Workbook = Workbook.CreateBlankWorkbook();
        }

        private async void File_Open_Click(object sender, RoutedEventArgs e)
        {
            var storage = this.StorageProvider;
            var options = new FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new ("Excel 2007 Document") { Patterns = ["*.xlsx"] },
                ]
            };
            var file = await storage.OpenFilePickerAsync(options);
            if(file.Count > 0)
            {
                var vm = (MainViewModel)this.DataContext!;
                await using var stream = await file[0].OpenReadAsync();

                var workbook = new Workbook();
                workbook.Load(stream, IO.FileFormat.Excel2007);

                vm.Workbook = workbook;
            }
        }

        private async void File_Save_Click(object sender, RoutedEventArgs e)
        {
            if (grid.Workbook is null)
                return;

            var file = await this.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                DefaultExtension = ".xlsx",
                FileTypeChoices =
                [
                    new ("Excel 2007 Document") { Patterns = ["*.xlsx"] },
                ]
            });

            if (file is not null)
            {
                // 打开文件的写入流。
                await using var stream = await file.OpenWriteAsync();
                grid.Workbook.Save(stream, IO.FileFormat.Excel2007);
            }


        }

        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion // Menu - File

        #region Menu - View
        private void View_FormulaBar_Click(object sender, RoutedEventArgs e)
        {
            grid.FormulaBarVisible = (sender as MenuItem)?.IsChecked == true;
        }

        private void View_SheetTab_Click(object sender, RoutedEventArgs e)
        {
            grid.SheetTabVisible = (sender as MenuItem)?.IsChecked == true;
        }

        private void View_SheetTabNewButton_Click(object sender, RoutedEventArgs e)
        {
            grid.SheetTabNewButtonVisible = (sender as MenuItem)?.IsChecked == true;
        }

        private void View_HorizontalScrollbar_Click(object sender, RoutedEventArgs e)
        {
            grid.HorizontalScrollBarVisible = (sender as MenuItem)?.IsChecked == true;
        }

        private void View_VerticalScrollbar_Click(object sender, RoutedEventArgs e)
        {
            grid.VerticalScrollBarVisible = (sender as MenuItem)?.IsChecked == true;
        }

        private void View_GuideLine_Click(object sender, RoutedEventArgs e)
        {
            grid.CurrentWorksheet?.SetSettings(WorksheetSettings.View_ShowGridLine, (sender as MenuItem)?.IsChecked == true);
        }

        private void View_PageBreaks_Click(object sender, RoutedEventArgs e)
        {
            grid.CurrentWorksheet?.SetSettings(WorksheetSettings.View_ShowPageBreaks, (sender as MenuItem)?.IsChecked == true);
        }
        #endregion // Menu - View

        #region Menu - Sheet

        private void freezeToCell_Click(object sender, RoutedEventArgs e)
        {
            grid.CurrentWorksheet?.FreezeToCell(grid.CurrentWorksheet.FocusPos);
        }

        private void Sheet_Append_100_Rows_Click(object sender, RoutedEventArgs e)
        {
            if (grid.CurrentWorksheet != null)
            {
                grid.DoAction(new Actions.InsertRowsAction(grid.CurrentWorksheet.Rows, 100));
            }
        }

        #endregion Menu - Sheet

    }
}
