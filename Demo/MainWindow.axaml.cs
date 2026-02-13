using System;
using System.IO;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Calcita.CellTypes;
using Calcita.Chart;
using Calcita.Drawing.Shapes;

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
            grid.Reset();
        }

        private async void File_Open_Click(object sender, RoutedEventArgs e)
        {
            var storage = this.StorageProvider;
            var options = new FilePickerOpenOptions(){AllowMultiple = false};
            var res = await storage.OpenFilePickerAsync(options);
          //  var dlg = new OpenFileDialog();

        //	dlg.DefaultExt = ".xlsx";
        //	dlg.Filter = "Supported file format(*.xlsx;*.rgf;*.xml)|*.xlsx;*.rgf;*.xml|Excel 2007 Document(*.xlsx)|*.xlsx|ReoGrid Format(*.rgf;*.xml)|*.rgf;*.xml";

            // Process open file dialog box results 
            //if (dlg.ShowDialog() == true)
            //{
            //	// Open document 
            //	try
            //	{
            //		grid.Load(dlg.FileName);
            //	}
            //	catch (Exception ex)
            //	{
            //		MessageBox.Show(this, "Loading error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            //	}
            //}
        }

        private void File_Save_Click(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            //dlg.DefaultExt = ".xlsx";
            //dlg.Filter = "Excel 2007 Document|*.xlsx|ReoGrid Format|*.rgf";

            //// Process open file dialog box results 
            //if (dlg.ShowDialog() == true)
            //{
            //	// Open document 
            //	grid.Save(dlg.FileName);

            //	System.Diagnostics.Process.Start(dlg.FileName);
            //}
        }

        private void File_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion // Menu - File

        #region Menu - View
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
            grid.CurrentWorksheet.SetSettings(WorksheetSettings.View_ShowGridLine, (sender as MenuItem)?.IsChecked == true);
        }

        private void View_PageBreaks_Click(object sender, RoutedEventArgs e)
        {
            grid.CurrentWorksheet.SetSettings(WorksheetSettings.View_ShowPageBreaks, (sender as MenuItem)?.IsChecked == true);
        }
        #endregion // Menu - View

        #region Menu - Sheet

        private void freezeToCell_Click(object sender, RoutedEventArgs e)
        {
            grid.CurrentWorksheet.FreezeToCell(grid.CurrentWorksheet.FocusPos);
        }

        private void Sheet_Append_100_Rows_Click(object sender, RoutedEventArgs e)
        {
            grid.DoAction(new Actions.InsertRowsAction(grid.CurrentWorksheet.Rows, 100));
        }

        #endregion Menu - Sheet

    }
}
