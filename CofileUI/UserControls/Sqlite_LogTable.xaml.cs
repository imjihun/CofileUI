using CofileUI.Classes;
using CofileUI.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Excel = Microsoft.Office.Interop.Excel;

namespace CofileUI.UserControls
{
	/// <summary>
	/// UserControl1.xaml에 대한 상호 작용 논리
	/// </summary>

	public class DataBaseInfo
	{
		//private const string tmp_filename = "cofile.db";
		public static string Path;
		//public static string LoadDataBase(string local_file_name)
		//{
		//	Path = WindowMain.current?.enableConnect?.sshManager?.GetDataBase(AppDomain.CurrentDomain.BaseDirectory, local_file_name);
		//	return Path;
		//}
		public static bool bUpdated = false;

		static string path_root = MainSettings.Path.PathDirCofileDB;
		const int MAX_IDX_DB_NAME = 255;
		static string prev_db_name = "";
		static int idx_db_name = 0;
		static int Idx_db_name {
			get
			{
				return idx_db_name;
			}
			set
			{
				idx_db_name = value;
				if(idx_db_name > MAX_IDX_DB_NAME)
					idx_db_name = 0;
				else if(idx_db_name < 0)
					idx_db_name = MAX_IDX_DB_NAME;
			}
		}
		public static void RefreshUi(string db_name = "")
		{
			FileContoller.DeleteFilesInDirectory(path_root);
			db_name = db_name + (Idx_db_name++) + ".db";
			Path = WindowMain.current?.enableConnect?.sshManager?.GetDataBase(path_root, db_name);

			if(Sqlite_LogTable.current != null)
				Sqlite_LogTable.current.Refresh();
			if(Sqlite_StatusTable.current != null)
				Sqlite_StatusTable.current.Refresh();
			//FileContoller.DeleteFile(path_root + prev_db_name);
			prev_db_name = db_name;
			bUpdated = true;
		}
	}

	public partial class Sqlite_LogTable : UserControl
	{
		public static Sqlite_LogTable current;

		public Sqlite_LogTable()
		{
			current = this;
			InitializeComponent();

			foreach(var v in log_type)
				comboBox_type.Items.Add(v);
			foreach(var v in log_action)
				comboBox_action.Items.Add(v);
			foreach(var v in log_result)
				comboBox_result.Items.Add(v);

			comboBox_type.Items.Add("Type");
			comboBox_action.Items.Add("Action");
			comboBox_result.Items.Add("Result");
			comboBox_type.SelectedIndex = comboBox_type.Items.Count - 1;
			comboBox_action.SelectedIndex = comboBox_action.Items.Count - 1;
			comboBox_result.SelectedIndex = comboBox_result.Items.Count - 1;

			comboBox_type.DataContext = log_type;
			comboBox_action.DataContext = log_action;
			comboBox_result.DataContext = log_result;

			comboBox_change_count_page.ItemsSource = arr_cnt_page;
			comboBox_change_count_page.SelectedIndex = 0;
			comboBox_change_count_page.SelectionChanged += ComboBox_change_count_page_SelectionChanged;

			this.Loaded += (sender, e) => {
				if(!DataBaseInfo.bUpdated)
					DataBaseInfo.RefreshUi();
			};
			this.IsVisibleChanged += (sender, e) => { if(this.IsVisible && !DataBaseInfo.bUpdated) DataBaseInfo.RefreshUi(); };
		}

		DataTable current_table;
		DataTable Current_table { get { return current_table; } set { current_table = value; } }
		DataRow[] filtering_row;
		DataRow[] Filtering_row
		{
			get { return filtering_row; }
			set
			{
				filtering_row = value;
				max_idx_page = (Filtering_row.Length - 1) / cnt_page;
			}
		}
		public void Refresh()
		{
			try
			{
				Clear();

				//string path = DataBaseInfo.LoadDataBase("log.db");
				//if(path == null)
				//	return;

				if(DataBaseInfo.Path == null)
					return;
				string strConn = "Data Source=" + DataBaseInfo.Path;
				using(SQLiteConnection conn = new SQLiteConnection(strConn))
				{
					UpdateDataGrid(conn, "SELECT * From log ORDER BY no DESC");
					conn.Close();
				}

				//Log.ViewMessage("Loaded", "Log File", Status.current.richTextBox_status);
				Log.PrintLog("Loaded", "UserControls.Sqlite_LogTable.Refresh");
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message, "Sqlite_LogTable][Refresh", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "UserControls.Sqlite_LogTable.Refresh");
				//MessageBox.Show(e.Message, "Sqlite_LogTable");
			}

		}
		public void Clear()
		{
			Current_table = new DataTable();
			dataGrid.ItemsSource = Current_table.DefaultView;
			idx_page = 0;
			cnt_page = arr_cnt_page[0];
			max_idx_page = 0;
			label_inform_page.Content = "";
			label_total_count.Content = "";
			//dataGrid.Columns.Clear();
			////dataGrid.Items.Clear();
			//dataGrid.Items.Refresh();
		}

		string[] log_type = new string[] {"Sam", "Tail", "File"};
		string[] log_action = new string[] {"Encrypt", "Decrypt"};
		string[] log_result = new string[] {"Success", "Fail"};
		private void UpdateDataGrid(SQLiteConnection con, string sql)
		{
			try
			{
				SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(sql, con);
				//DataTable[] tables = new DataTable[1] {new DataTable() };
				//dataAdapter.Fill(0, 20000, tables);
				//Current_table = tables[0];

				DataSet dataSet = new DataSet();
				dataAdapter.Fill(dataSet);
				Current_table = dataSet.Tables[0];

				ChangeColumnIntToString(log_type, Current_table, "type");
				ChangeColumnIntToString(log_action, Current_table, "action");
				ChangeColumnIntToString(log_result, Current_table, "result");

				RefreshDataView(idx_page, cnt_page);
			}
			catch(Exception e)
			{
				Log.ErrorIntoUI(e.Message, "UpdateDataGrid", Status.current.richTextBox_status);
				Log.PrintError(e.Message, "UserControls.Sqlite_LogTable.UpdateDataGrid");
				//MessageBox.Show(e.Message, "Sqlite_LogTable");
			}
		}

		void ChangeColumnIntToString(string[] source, DataTable _table, string column_name)
		{
			// 열안에 데이터가 있으면 타입 변경 안됨.
			//table.Columns["type"].DataType = typeof(string)

			// 새로운 열을 추가하는 방식
			// 맨 앞 밑 줄은 DataGrid Column명에 표시 X
			string add_column_name = "_" + column_name;
			DataColumn new_type = new DataColumn(add_column_name, typeof(string));
			_table.Columns.Add(new_type);
			int new_idx = _table.Columns.IndexOf(column_name);
			new_type.SetOrdinal(new_idx);
			//System.Int64 idx;
			foreach(DataRow v in _table.Rows)
			{
				// integer 중 가장 큰 64비트로 캐스팅이 되는지 확인후 캐스팅진행.
				// int a = (short)2; 는 에러가 나지 않음.
				if(typeof(System.Int64).IsAssignableFrom(v[column_name].GetType()))
				{
					System.Int64 idx = (System.Int64)v[column_name];
					if(source.Length <= idx || idx < 0)
						idx = source.Length - 1;
					
					v[add_column_name] = source[idx];
				}
			}

			_table.Columns.RemoveAt(new_idx + 1);
		}

		// string filter_total = "";
		bool bFiltering = false;
		enum Filter_Idx
		{
			source_target = 0,
			startDate,
			endDate,
			type,
			action,
			result,
			MemberCount
		}
		string[] filter_string = new string[(int)Filter_Idx.MemberCount];
		int backup_idx_page = 0;
		private void OnKeyDownFilter(object sender, KeyEventArgs e)
		{
			if(e.Key != Key.Enter)
				return;

			if(textBox_filter.Text == "")
			{
				if(bFiltering)
				{
					idx_page = backup_idx_page;
					filter_string[(int)Filter_Idx.source_target] = null;
					bFiltering = false;

					RefreshDataView(idx_page, cnt_page);
				}
			}
			else
			{
				backup_idx_page = idx_page;
				idx_page = 0;
				//filter = "and (source LIKE '*" + textBox_filter.Text + "*' or target LIKE '*" + textBox_filter.Text + "*')";
				filter_string[(int)Filter_Idx.source_target] = "(source LIKE '*" + textBox_filter.Text + "*' or target LIKE '*" + textBox_filter.Text + "*')";
				bFiltering = true;

				RefreshDataView(idx_page, cnt_page);
			}
		}
		private void OnComboBoxChangedType(object sender, SelectionChangedEventArgs e)
		{
			if(comboBox_type.SelectedIndex < log_type.Length)
			{
				filter_string[(int)Filter_Idx.type] = "(_type = '" + log_type[comboBox_type.SelectedIndex] + "')";
				backup_idx_page = idx_page;
				idx_page = 0;
				RefreshDataView(idx_page, cnt_page);
			}
			else
			{
				filter_string[(int)Filter_Idx.type] = null;
				RefreshDataView(idx_page, cnt_page);
			}
			e.Handled = true;
		}
		private void OnComboBoxChangedAction(object sender, SelectionChangedEventArgs e)
		{
			if(comboBox_action.SelectedIndex < log_action.Length)
			{
				filter_string[(int)Filter_Idx.action] = "(_action = '" + log_action[comboBox_action.SelectedIndex] + "')";
				backup_idx_page = idx_page;
				idx_page = 0;
				RefreshDataView(idx_page, cnt_page);
			}
			else
			{
				filter_string[(int)Filter_Idx.action] = null;
				RefreshDataView(idx_page, cnt_page);
			}
			e.Handled = true;
		}
		private void OnComboBoxChangedResult(object sender, SelectionChangedEventArgs e)
		{
			if(comboBox_result.SelectedIndex < log_result.Length)
			{
				filter_string[(int)Filter_Idx.result] = "(_result = '" + log_result[comboBox_result.SelectedIndex] + "')";
				backup_idx_page = idx_page;
				idx_page = 0;
				RefreshDataView(idx_page, cnt_page);
			}
			else
			{
				filter_string[(int)Filter_Idx.result] = null;
				RefreshDataView(idx_page, cnt_page);
			}
			e.Handled = true;
		}

		#region First Prev Next Last Button click
		static int[] arr_cnt_page = new int[] {10,15,20 };
		private int idx_page = 0;
		private int cnt_page = arr_cnt_page[0];
		private int max_idx_page = 0;

		private void RefreshDataView(int _idx_page, int _cnt_page)
		{
			try
			{
				//string filter_page = _idx_page * _cnt_page + " < no and no <= " + (_idx_page + 1) * _cnt_page;
				//if(bFiltering)
				//{
				//	//current_table.DefaultView.RowFilter = filter_page + filter;
				//}
				//else
				//{
				//	//DataView dv = (from t in current_table.AsEnumerable()
				//	//			   //where r.IsMatch(t.Field<string>("SOME_COL"))
				//	//			   select t).AsDataView();
				//	//dataGrid.ItemsSource = current_table.AsEnumerable().Take(10).CopyToDataTable().DefaultView;
				//	DataView dv = (from t in current_table.AsEnumerable()
				//				   select t).AsDataView();
				//	//current_table.DefaultView.RowFilter = filter_page;

				//	//ObservableCollection<object> ItemsSource = new ObservableCollection<object>();
				////	for(int i = 0; i < cnt_page; i++)
				////	{
				////		ItemsSource.Add(_newTable.Rows[idx_page * cnt_page + i]);
				////	}
				////	dataGrid.ItemsSource = ItemsSource.CopyToDataTable().DefaultView;
				//}
				if(dataGrid == null || Current_table == null)
					return;

				dataGrid.ItemsSource = new DataView();

				Filtering_row = Current_table.Select();
				for(int i = 0; i < (int)Filter_Idx.MemberCount && Filtering_row.Length > 0; i++)
				{
					if(filter_string[i] == null || filter_string[i] == "")
						continue;

					Filtering_row = Filtering_row.CopyToDataTable().Select(filter_string[i]);
					//current_table.DefaultView.RowFilter = filter;
					//_newTable = current_table.DefaultView.ToTable();
				}

				List<DataRow> ItemsSource = new List<DataRow>();
				int start_idx = idx_page * cnt_page;
				for(int i = start_idx; i < start_idx + cnt_page && i < Filtering_row.Length; i++)
				{
					ItemsSource.Add(Filtering_row[i]);
				}
				if(ItemsSource.Count > 0)
					dataGrid.ItemsSource = ItemsSource.CopyToDataTable().DefaultView;
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.Sqlite_LogTable.RefreshDataView");
			}

			if(Filtering_row == null)
				return;

			label_inform_page.Content = _idx_page * _cnt_page + " ~ " + (_idx_page + 1) * _cnt_page;
			label_total_count.Content = "/ " + Filtering_row.Length + " 개 중";
		}
		private void btnFirst_Click(object sender, RoutedEventArgs e)
		{
			idx_page = 0;
			RefreshDataView(idx_page, cnt_page);
		}
		private void btnPrev_Click(object sender, RoutedEventArgs e)
		{
			idx_page--;
			if(idx_page < 0)
				idx_page = 0;

			RefreshDataView(idx_page, cnt_page);
		}
		private void btnNext_Click(object sender, RoutedEventArgs e)
		{
			idx_page++;
			if(idx_page > max_idx_page)
				idx_page = max_idx_page;
			RefreshDataView(idx_page, cnt_page);
		}
		private void btnLast_Click(object sender, RoutedEventArgs e)
		{
			idx_page = max_idx_page;
			RefreshDataView(idx_page, cnt_page);
		}
		private void ComboBox_change_count_page_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			int prev_idx_row;
			if(idx_page == max_idx_page)
				prev_idx_row = Filtering_row.Length;
			else
				prev_idx_row = idx_page * cnt_page;

			cnt_page = arr_cnt_page[comboBox_change_count_page.SelectedIndex];
			idx_page = prev_idx_row / cnt_page;
			RefreshDataView(idx_page, cnt_page);
		}
		#endregion

		private void StartDateChanged()
		{
			if(dataPicker_start.IsEnabled && dataPicker_start.SelectedDate != null)
			{
				DateTime start = (DateTime)dataPicker_start.SelectedDate;
				filter_string[(int)Filter_Idx.startDate] = string.Format(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, "(time >= #{0}#)", start);
			}
			else
				filter_string[(int)Filter_Idx.startDate] = "";

			RefreshDataView(idx_page, cnt_page);
		}
		private void OnSelectedDateChangedStartDate(object sender, SelectionChangedEventArgs e)
		{
			StartDateChanged();
		}
		private void OnIsEnabledChangedStartDate(object sender, DependencyPropertyChangedEventArgs e)
		{
			StartDateChanged();
		}

		private void EndDateChanged()
		{
			if(dataPicker_end.IsEnabled && dataPicker_end.SelectedDate != null)
			{
				DateTime end = (DateTime)dataPicker_end.SelectedDate;
				end = end.AddSeconds(24 * 60 * 60 - 1);
				filter_string[(int)Filter_Idx.endDate] = string.Format(System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat, "(time <= #{0}#)", end);
			}
			else
				filter_string[(int)Filter_Idx.endDate] = "";

			RefreshDataView(idx_page, cnt_page);
		}
		private void OnSelectedDateChangedEndDate(object sender, SelectionChangedEventArgs e)
		{
			EndDateChanged();
		}
		private void OnIsEnabledChangedEndDate(object sender, DependencyPropertyChangedEventArgs e)
		{
			EndDateChanged();
		}
		private void OnClickRefresh(object sender, RoutedEventArgs e)
		{
			UserControls.DataBaseInfo.RefreshUi();
		}

		private void OnClickExport(object sender, RoutedEventArgs e)
		{
			if(WindowMain.current?.enableConnect?.sshManager?.IsConnected == true)
				ConfirmExport();
		}
		private void ConfirmExport()
		{
			MahApps.Metro.Controls.Dialogs.MetroDialogSettings setting = new MahApps.Metro.Controls.Dialogs.MetroDialogSettings()
			{
				AffirmativeButtonText = "All Export"
				, NegativeButtonText = "Only Current Page Export"
				, FirstAuxiliaryButtonText = "Cancel"
			};
			WindowMain.current.ShowMessageDialog("Export"
				, "전체 테이블을 Export 하시겠습니까?"
				, MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary
				, affirmative_callback: TotalExport
				, negative_callback: PageExport
				, settings: setting);
		}
		string xml = "xml";
		string csv = "csv";
		string xls = "xls";
		private SaveFileDialog MakeSaveFileDialog()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			string InitalDirectory = AppDomain.CurrentDomain.BaseDirectory;
			saveFileDialog.FileName = "cofile";
			saveFileDialog.DefaultExt = "xls";
			saveFileDialog.Filter = "CSV files (*." + csv + ")|*." + csv
										+ "|Xml files (*." + xml + ")|*." + xml;
										//+"|Excel files(*." + xls + ") | *." + xls;
			saveFileDialog.InitialDirectory = InitalDirectory;

			return saveFileDialog;
		}
		private void TotalExport()
		{
			SaveFileDialog sfd = MakeSaveFileDialog();
			if(sfd.ShowDialog() == true)
			{
				DataTable dt = Current_table;
				if(Filtering_row != null && Filtering_row.Length > 0)
					dt = Filtering_row.CopyToDataTable();

				Export(sfd.FileName, dt);
			}
		}
		private void PageExport()
		{
			SaveFileDialog sfd = MakeSaveFileDialog();
			if(sfd.ShowDialog() == true)
			{
				DataTable dt = null;

				if(Filtering_row != null)
				{
					List<DataRow> ItemsSource = new List<DataRow>();
					int start_idx = idx_page * cnt_page;
					for(int i = start_idx; i < start_idx + cnt_page && i < Filtering_row.Length; i++)
					{
						ItemsSource.Add(Filtering_row[i]);
					}
					if(ItemsSource.Count > 0)
						dt = ItemsSource.CopyToDataTable();
				}
				Export(sfd.FileName, dt);
			}
		}
		private int Export(string path, DataTable table)
		{
			string[] split = path.Split('.');
			string extension = split[split.Length - 1];

			if(table != null && Current_table != null)
			{
				table.TableName = Current_table.TableName;
				int retval = 0;
				if(extension == xml)
					retval = ExportXml(path, table);
				else if(extension == csv)
					retval = ExportCSV(path, table);
				else if(extension == xls)
					retval = ExportExcel(true, path, table);
				else
					retval = -1;

				if(retval == 0)
				{
					WindowMain.current.ShowMessageDialog("Export", "Export에 성공하였습니다." + Environment.NewLine + "Path = " + path);
					return 0;
				}
			}
			WindowMain.current.ShowMessageDialog("Export Error", "Export에 실패하였습니다." + Environment.NewLine + "Path = " + path);
			return -1;
		}
		private int ExportXml(string path, DataTable table)
		{
			if(table == null)
				return -1;

			try
			{
				table.WriteXml(path);
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.Sqlite_LogTable.ExportXml");
				return -2;
			}
			return 0;
		}
		private int ExportCSV(string path, DataTable table)
		{
			if(table == null)
				return -1;

			try
			{
				string delimiter = ",";
				string newLine = Environment.NewLine;
				StringBuilder sb = new StringBuilder();
				
				for(int i = 0; i < table.Rows.Count; i++)
				{
					sb.Append(table.Rows[i].ItemArray[0].ToString());
					for(int j = 1; j < table.Columns.Count; j++)
					{
						sb.Append(delimiter);
						sb.Append(table.Rows[i].ItemArray[j].ToString());
					}
					sb.Append(newLine);
				}
				if(FileContoller.Write(path, sb.ToString()))
					return 0;
				else
					return -3;
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.Sqlite_LogTable.ExportCSV");
				return -2;
			}
		}

		private int ExportExcel(bool captions, string path, DataTable table)
		{
			if(table == null)
				return -1;

			int num = 0;
			object missingType = Type.Missing;

			Excel.Application objApp;
			Excel._Workbook objBook;
			Excel.Workbooks objBooks;
			Excel.Sheets objSheets;
			Excel._Worksheet objSheet;
			Excel.Range range;
			
			string[] headers = new string[table.Columns.Count];
			string[] columns = new string[table.Columns.Count];

			for(int c = 0; c < table.Columns.Count; c++)
			{
				headers[c] = table.Columns[c].ToString();
				num = c + 65;
				columns[c] = Convert.ToString((char)num);
			}

			try
			{
				objApp = new Excel.Application();
				objBooks = objApp.Workbooks;
				objBook = objBooks.Add(Missing.Value);
				objSheets = objBook.Worksheets;
				objSheet = (Excel._Worksheet)objSheets.get_Item(1);

				if(captions)
				{
					for(int c = 0; c < table.Columns.Count; c++)
					{
						range = objSheet.get_Range(columns[c] + "1", Missing.Value);
						range.set_Value(Missing.Value, headers[c]);
					}
				}

				for(int i = 0; i < table.Rows.Count - 1; i++)
				{
					for(int j = 0; j < table.Columns.Count; j++)
					{
						range = objSheet.get_Range(columns[j] + Convert.ToString(i + 2),
																Missing.Value);
						range.set_Value(Missing.Value,
												table.Rows[i].ItemArray[j].ToString());
					}
				}

				objApp.Visible = false;
				objApp.UserControl = false;

				objBook.SaveAs(path,
							Excel.XlFileFormat.xlWorkbookNormal,
							missingType, missingType, missingType, missingType,
							Excel.XlSaveAsAccessMode.xlNoChange,
							missingType, missingType, missingType, missingType, missingType);
				objBook.Close(false, missingType, missingType);

				Cursor = Cursors.AppStarting;
			}
			catch(Exception e)
			{
				Log.PrintError(e.Message, "UserControls.Sqlite_LogTable.ExportExcel");
				return -2;
			}
			return 0;
		}
	}
}
