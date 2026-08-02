/*****************************************************
 * 
 * ReoGrid - .NET Spreadsheet Control
 * 
 * https://reogrid.net/
 *
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
 * PURPOSE.
 *
 * Author: Jingwood <jingwood at unvell.com>
 *
 * Copyright (c) 2012-2025 Jingwood <jingwood at unvell.com>
 * Copyright (c) 2012-2025 UNVELL Inc. All rights reserved.
 * 
 ****************************************************************************/using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Calcita.Common;
using Calcita.Events;
using Calcita.IO;
using Calcita.Main;
using Calcita.Interaction;

#if PRINT
using Calcita.Print;
#endif // PRINT

namespace Calcita
{
    public sealed partial class Workbook : IWorkbook
	{
		internal List<Worksheet> worksheets = [];

		#region Readonly
		private bool isReadonly = false;

		public bool Readonly
		{
			get
			{
				return isReadonly;
			}
			set
			{
				isReadonly = value;

				foreach (var sheet in this.worksheets)
				{
					sheet.SetSettings(WorksheetSettings.Edit_Readonly, value);
				}
			}
		}
		#endregion // Readonly

		/// <summary>
		/// Create workbook instance
		/// </summary>
		/// <param name="adapter">Control instance adapter</param>
		public Workbook()
		{
  
		}

		static Workbook()
		{
			FileFormatProviders[FileFormat.ReoGridFormat] = new ReoGridFileFormatProvider();
			FileFormatProviders[FileFormat.Excel2007] = new ExcelFileFormatProvider();
			FileFormatProviders[FileFormat.CSV] = new CSVFileFormatProvider();
		}

		/// <summary>
		/// Clear all worksheets.
		/// </summary>
		public void Clear()
		{
			this.ClearWorksheets();
		}

		#region Save & Load

		public static readonly Dictionary<FileFormat, IFileFormatProvider> FileFormatProviders = [];

		public void Save(string path)
		{
			this.Save(path, FileFormat._Auto);
		}

		public void Save(string path, IO.FileFormat fileFormat)
		{
			this.Save(path, fileFormat, Encoding.Default);
		}

		public void Save(string path, IO.FileFormat fileFormat, Encoding encoding)
		{
			if (fileFormat == IO.FileFormat._Auto)
			{
				foreach (var p in FileFormatProviders)
				{
					if (p.Value.IsValidFormat(path))
					{
						fileFormat = p.Key;
						break;
					}
				}

				if (fileFormat == FileFormat._Auto)
				{
					throw new NotSupportedException("Cannot determine a file format to load workbook from specified path, try specify the file format.");
				}
			}

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            Save(fs, fileFormat, encoding);
        }

		public void Save(System.IO.Stream stream, IO.FileFormat fileFormat)
		{
			this.Save(stream, fileFormat, Encoding.Default);
		}

		public void Save(System.IO.Stream stream, IO.FileFormat fileFormat, Encoding encoding)
		{
			if (!FileFormatProviders.TryGetValue(fileFormat, out var provider))
			{
				throw new FileFormatNotSupportException("Specified file format is not supported");
			}

            WorkbookSaving?.Invoke(this, EventArgs.Empty);
            try
			{
				provider.Save(this, stream, encoding, null);
			}
			finally
			{
                WorkbookSaved?.Invoke(this, EventArgs.Empty);
			}
		}

		public void Load(string path)
		{
			this.Load(path, IO.FileFormat._Auto);
		}

		public void Load(string path, IO.FileFormat fileFormat)
		{
			this.Load(path, fileFormat, Encoding.Default);
		}

		public void Load(string path, IO.FileFormat fileFormat, Encoding encoding)
		{
			if (fileFormat == IO.FileFormat._Auto)
			{
				foreach (var p in FileFormatProviders)
				{
					if (p.Value.IsValidFormat(path))
					{
						fileFormat = p.Key;
						break;
					}
				}

				if (fileFormat == FileFormat._Auto)
				{
					throw new NotSupportedException("Cannot determine the file format to load workbook from specified path, try specify explicitly the file format by argument.");
				}
			}

			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
			{
				this.Load(fs, fileFormat, encoding ?? Encoding.Default);
			}

			// for csv only
			if (fileFormat == FileFormat.CSV)
			{
				if (this.worksheets.Count > 0)
				{
					this.worksheets[0].Name = Path.GetFileNameWithoutExtension(path);
				}
			}
		}

		public void Load(System.IO.Stream stream, IO.FileFormat fileFormat)
		{
			this.Load(stream, fileFormat, Encoding.Default);
		}

		public void Load(System.IO.Stream stream, IO.FileFormat fileFormat, Encoding encoding)
		{
			if (fileFormat == FileFormat._Auto)
			{
				throw new System.ArgumentException("File format 'Auto' is invalid for loading workbook from stream, try specify a file format.");
			}

			if (!FileFormatProviders.TryGetValue(fileFormat, out var provider))
			{
				throw new FileFormatNotSupportException("Specified file format is not supported.");
			}

            this.WorkbookLoading?.Invoke(this, EventArgs.Empty);

            encoding ??= Encoding.Default;

			try
			{
				provider.Load(this, stream, encoding, null);
			}
			finally
			{
				this.WorkbookLoaded?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Event raised when workbook loaded from stream or file
		/// </summary>
		public event EventHandler? WorkbookLoaded;
		public event EventHandler? WorkbookLoading;

        /// <summary>
        /// Event raised when workbook saved into stream or file
        /// </summary>
        public event EventHandler? WorkbookSaved;

        /// <summary>
        /// Event raised when workbook saved into stream or file
        /// </summary>
        public event EventHandler? WorkbookSaving;
        #endregion // Save & Load

        #region Worksheet Management

        internal string GetAvailableWorksheetName()
		{
			string name;
			int index = 1;
			while (!this.CheckWorksheetName(name = (LanguageResource.Sheet + index))) index++;
			return name;
		}

		public Worksheet CreateWorksheet(string? name = null)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = GetAvailableWorksheetName();
			}
			else
			{
				this.ValidateWorksheetName(name);
			}

			var sheet = new Worksheet(this, name);

			this.WorksheetCreated?.Invoke(this, new WorksheetCreatedEventArgs(sheet));

			return sheet;
		}

        public Worksheet AddWorksheet(string? sheetName)
        {
            var sheet = CreateWorksheet(sheetName);
            this.InsertWorksheet(this.worksheets.Count, sheet);
            return sheet;
        }

        public void AddWorksheet(Worksheet sheet)
		{
			this.InsertWorksheet(this.worksheets.Count, sheet);
		}

		public void NewWorksheet(string? name = null)
		{
			this.AddWorksheet(this.CreateWorksheet(name));
		}

		public void InsertWorksheet(int index, Worksheet sheet)
		{
			if (index < 0 || index > this.worksheets.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (sheet.Workbook != null && sheet.Workbook != this)
			{
				throw new WorkbookException("Specified worksheet belongs to another workbook, remove from another workbook firstly.");
			}

			this.ValidateWorksheetName(sheet.Name);

			this.worksheets.Insert(index, sheet);

			sheet.workbook = this;

			// event
			this.WorksheetInserted?.Invoke(this, new WorksheetInsertedEventArgs(sheet)
			{
				Index = index,
			});
		}

		public bool RemoveWorksheet(int index)
		{
			if (index < 0 || index >= this.worksheets.Count)
				throw new ArgumentOutOfRangeException(nameof(index));

			var sheet = this.worksheets[index];
			sheet.workbook = null;

			this.worksheets.RemoveAt(index);

			this.WorksheetRemoved?.Invoke(this, new WorksheetRemovedEventArgs(sheet));
			return true;
		}

		public bool RemoveWorksheet(Worksheet sheet)
		{
			int index = this.worksheets.IndexOf(sheet);

			if (index < 0 || index >= this.worksheets.Count)
			{
				throw new WorksheetNotFoundException("Specified worksheet cannot be found.");
			}

			return RemoveWorksheet(index);
		}

		/// <summary>
		/// Duplicate worksheet and insert the new instance into specified position
		/// </summary>
		/// <param name="index">zero-based number of worksheet to be duplicated</param>
		/// <param name="newIndex">position used to insert duplicated new instance</param>
		/// <param name="newName">New name to be apply to copied worksheet</param>
		/// <returns>instance of duplicated worksheet from specified worksheet</returns>
		public Worksheet CopyWorksheet(int index, int newIndex, string? newName = null)
		{
			if (newIndex < 0 || newIndex > this.worksheets.Count)
			{
                throw new ArgumentOutOfRangeException(nameof(newIndex));
			}

            return CopyWorksheet(this.worksheets[index], newIndex, newName);
		}

		/// <summary>
		/// Duplicate worksheet and insert the new instance into specified position
		/// </summary>
		/// <param name="sheet">worksheet to be duplicated. The worksheet passed here should be 
		/// already added into current workbook.</param>
		/// <param name="newIndex">position used to insert duplicated new instance</param>
		/// <param name="newName">New name to be apply</param>
		/// <returns>instance of duplicated worksheet from specified worksheet</returns>
		/// <exception cref="WorksheetNotFoundException">when specified worksheet does not belong to
		/// this workbook.</exception>
		/// <exception cref="ArgumentOutOfRangeException">when the position used to insert
		/// duplicated instace of worksheet is out of valid range of this workbook.</exception>
		public Worksheet CopyWorksheet(Worksheet sheet, int newIndex, string? newName = null)
		{
			if (sheet.workbook != this)
			{
				throw new WorksheetNotFoundException("Specified worksheet does not belong to this workbook.");
			}

			if (newIndex < 0 || newIndex > this.worksheets.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(newIndex));
			}

			var newSheet = sheet.Clone(newName);

            this.WorksheetCreated?.Invoke(this, new (newSheet));

            InsertWorksheet(newIndex, newSheet);

			return newSheet;
		}


		/// <summary>
		/// Move worksheet from a position to another position
		/// </summary>
		/// <param name="index">Worksheet in this position to be moved</param>
		/// <param name="newIndex">Target position moved to</param>
		/// <returns>Instance of moved worksheet</returns>
		public Worksheet MoveWorksheet(int index, int newIndex)
		{
			if (index < 0 || index > this.worksheets.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (newIndex < 0 || newIndex > this.worksheets.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(newIndex));
			}

			var sheet = this.worksheets[index];

			if (index == newIndex) 
                return sheet;

			this.worksheets.RemoveAt(index);
			this.worksheets.Insert(newIndex, sheet);

            this.WorksheetMoved?.Invoke(this, new (sheet)
            {
                Index = index,
                NewIndex = newIndex
            });

			return sheet;
		}

		/// <summary>
		/// Create a cloned worksheet and put into specified position
		/// </summary>
		/// <param name="sheet">Instance of worksheet to be moved, the worksheet must be already added into this workbook</param>
		/// <param name="newIndex">Target position moved to</param>
		/// <returns>New instance of copid worksheet</returns>
		public Worksheet MoveWorksheet(Worksheet sheet, int newIndex)
		{
			if (sheet.workbook != this)
			{
				throw new WorksheetNotFoundException("Specified worksheet does not belong to this workbook.");
			}

			int index = GetWorksheetIndex(sheet);

			return MoveWorksheet(index, newIndex);
		}

		/// <summary>
		/// Get index of specified worksheet from the collection in this workbook
		/// </summary>
		/// <param name="sheet">worksheet to be get</param>
		/// <returns>zero-based number of worksheet in this workbook's collection</returns>
		public int GetWorksheetIndex(Worksheet sheet)
		{
			return this.worksheets.IndexOf(sheet);
		}

		/// <summary>
		/// Get the index of specified worksheet by name from workbook.
		/// </summary>
		/// <param name="sheet">Worksheet to get.</param>
		/// <returns>Zero-based number of worksheet in worksheet collection of workbook.</returns>
		public int GetWorksheetIndex(string name)
		{
			var sheet = this.GetWorksheetByName(name);
			return sheet == null ? -1 : this.GetWorksheetIndex(sheet);
		}

		/// <summary>
		/// Find worksheet by specified name
		/// </summary>
		/// <param name="name">Name to find worksheet</param>
		/// <returns>Instance of worksheet that is found by specified name; otherwise return null</returns>
		public Worksheet? GetWorksheetByName(string name)
		{
			return this.worksheets.FirstOrDefault(w => string.Compare(w.Name, name, true) == 0);
		}

        #region Collection of worksheet

        /// <summary>
        /// Collection of worksheets
        /// </summary>
        public WorksheetCollection Worksheets => field ??= new WorksheetCollection(this);

        #endregion Collection of worksheet

        /// <summary>
        /// Event raised when new worksheet is created
        /// </summary>
        public event EventHandler<WorksheetCreatedEventArgs>? WorksheetCreated;

		/// <summary>
		/// Event raised when new worksheet is inserted
		/// </summary>
		public event EventHandler<WorksheetInsertedEventArgs>? WorksheetInserted;

		/// <summary>
		/// Event raised when new worksheet is removed
		/// </summary>
		public event EventHandler<WorksheetRemovedEventArgs>? WorksheetRemoved;

		/// <summary>
		/// Event raised when new worksheet is inserted
		/// </summary>
		public event EventHandler<WorksheetMovedEventArgs>? WorksheetMoved;

		/// <summary>
		/// Event raised before name of worksheet changing
		/// </summary>
		public event EventHandler<WorksheetNameChangingEventArgs>? BeforeWorksheetNameChange;

		/// <summary>
		/// Event raised when name of worksheet is changed
		/// </summary>
		public event EventHandler<WorksheetNameChangingEventArgs>? WorksheetNameChanged;

		/// <summary>
		/// Event raised when background color of worksheet name is changed.
		/// </summary>
		public event EventHandler<WorksheetEventArgs>? WorksheetNameBackColorChanged;

		/// <summary>
		/// Event raised when text color of worksheet name is changed.
		/// </summary>
		public event EventHandler<WorksheetEventArgs>? WorksheetNameTextColorChanged;

		internal bool CheckWorksheetName(string name)
		{
			return this.worksheets.All(s => string.Compare(s.Name, name, true) != 0);
		}

		internal void ValidateWorksheetName(string name)
		{
			if (!CheckWorksheetName(name))
			{
				throw new Exception("Specified name is already used by another worksheet.");
			}
		}

		internal string NotifyWorksheetNameChange(Worksheet sheet, string name)
		{
			if (this.BeforeWorksheetNameChange != null)
			{
				var arg = new WorksheetNameChangingEventArgs(sheet, name);
				this.BeforeWorksheetNameChange(this, arg);
				return arg.NewName;
			}
			else
			{
				return name;
			}
		}

		internal void RaiseWorksheetNameChangedEvent(Worksheet worksheet)
		{
			int index = GetWorksheetIndex(worksheet);

			if (index >= 0 && index < this.worksheets.Count)
			{
                this.WorksheetNameChanged?.Invoke(this, new WorksheetNameChangingEventArgs(worksheet, worksheet.Name));
            }
		}

		internal void RaiseWorksheetNameBackColorChangedEvent(Worksheet worksheet)
		{
			int index = GetWorksheetIndex(worksheet);

			if (index >= 0 && index < this.worksheets.Count)
			{
                this.WorksheetNameBackColorChanged?.Invoke(this, new (worksheet));
            }
		}

		internal void RaiseWorksheetNameTextColorChangedEvent(Worksheet worksheet)
		{
			int index = GetWorksheetIndex(worksheet);

			if (index >= 0 && index < this.worksheets.Count)
			{
                this.WorksheetNameTextColorChanged?.Invoke(this, new (worksheet));
            }
		}

		internal void ClearWorksheets()
		{
			while (this.worksheets.Count > 0)
			{
				var sheet = this.worksheets[this.worksheets.Count - 1];

				this.worksheets.Remove(sheet);
				sheet.workbook = null;

                this.WorksheetRemoved?.Invoke(this, new (sheet));
            }
		}

		public int WorksheetCount { get { return this.worksheets.Count; } }

        /// <summary>
        /// create a workbook with one blank worksheet, and return the instance of workbook.
        /// </summary>
        /// <returns></returns>
        public static Workbook CreateBlankWorkbook()
        {
            var wb = new Workbook();
            wb.AddWorksheet(wb.CreateWorksheet());
            return wb;
        }

        public bool IsEmpty
		{
			get
			{
				foreach (var sheet in worksheets)
				{
					if (sheet.MaxContentRow > 0 || sheet.MaxContentCol > 0)
					{
						return false;
					}
				}

				return true;
			}
		}

        #endregion // Worksheet Management

		#region Internal Exceptions

		/// <summary>
		/// Event is used to notify if there are any internal exceptions happen on worksheets
		/// </summary>
		public event EventHandler<ExceptionHappenEventArgs>? ExceptionHappened;

		/// <summary>
		/// Notify that there are exceptions happen on any worksheet. 
		/// The event ExceptionHappened of workbook will be invoked.
		/// </summary>
		/// <param name="sheet">Worksheet where the exception happened</param>
		/// <param name="ex">Exception to describe the details of error information</param>
		public void NotifyExceptionHappen(Worksheet sheet, Exception ex)
		{
			Logger.Log("workbook", "internal exception: " + ex.Message);

            this.ExceptionHappened?.Invoke(this, new ExceptionHappenEventArgs(sheet, ex));
        }
		#endregion // Internal Exceptions

		#region Appearance
		//private ControlAppearanceStyle controlStyle;

		///// <summary>
		///// Control Style Settings
		///// </summary>
		//internal ControlAppearanceStyle ControlStyle
		//{
		//	get;
		//	set;
		//}

		///// <summary>
		///// Set the style of grid control.
		///// </summary>
		///// <param name="controlStyle"></param>
		//public void SetControlStyle(ControlAppearanceStyle controlStyle)
		//{
		//	this.controlStyle = controlStyle;

		//	//foreach (var sheet in this.worksheets)
		//	//{
		//	//	sheet.controlStyle = this.controlStyle;
		//	//}

		//	if (this.controlAdapter != null && this.controlAdapter.IsVisible)
		//	{
		//		this.controlAdapter.Invalidate();
		//	}
		//}
		#endregion

#if PRINT
		public PrintSession CreatePrintSession()
		{
			var ps = new PrintSession();

			foreach (var sheet in this.worksheets)
			{
				ps.worksheets.Add(sheet);
			}

			ps.Init();

			return ps;
		}
#endif // PRINT

		public void Dispose()
		{
			this.Clear();
		}
	}
}


