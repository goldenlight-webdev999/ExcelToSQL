namespace GroGroup.Class
{
    using System;
    using System.Data;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml;
    using System.Threading;
    using System.Web;
    using System.Data.SqlClient;

    public class ExporterClass
    {
        public fileType destfileType = fileType.XML;
        private SaveFileDialog dlgSaveFile;
        private string strDlgFilter = "";
        public static SqlConnection Sconn;

        public string exportData(DataSet dsExport, fileType OutputFileType, string Filepath)
        {
            string str3;
            try
            {
                FileStream stream;
                StreamWriter writer;
                int num;
                int num2;
                DataTable table2;
                int num3;
                DataTable data = dsExport.Tables[0];
                if (data == null)
                {
                    return "No data to export!";
                }
                if (data.Rows.Count == 0)
                {
                    return "No data to export!";
                }
                string path = Filepath;
                string str2 = "";
                switch (OutputFileType.ToString().ToUpper())
                {
                    case "NONE":
                        return "true";

                    case "CSV":
                        this.strDlgFilter = "Comma seperated value File (*.csv) |*.csv";
                        stream = new FileStream(path, FileMode.Create);
                        writer = new StreamWriter(stream);
                        str2 = "";
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        num = 0;
                        goto Label_0194;

                    case "TILDE":
                        this.strDlgFilter = "Tilt(~) seperated value File (*.txt) |*.txt";
                        stream = new FileStream(path, FileMode.Create);
                        writer = new StreamWriter(stream);
                        str2 = "";
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        num = 0;
                        goto Label_0418;

                    case "TXT":
                        this.strDlgFilter = "Text Files (*.txt) |*.txt";
                        stream = new FileStream(path, FileMode.Create);
                        writer = new StreamWriter(stream);
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        num = 0;
                        goto Label_05AC;

                    case "XML":
                        //var t = new Thread((ThreadStart)(() =>
                        //{
                        //    this.strDlgFilter = "XML Data Files (*.xml) |*.xml";
                        //    this.dlgSaveFile = new SaveFileDialog();
                        //    this.dlgSaveFile.InitialDirectory = HttpContext.Current.Server.MapPath("/");
                        //    this.dlgSaveFile.Filter = this.strDlgFilter;
                        //    this.dlgSaveFile.Title = "Export to";
                        //    this.dlgSaveFile.ShowDialog();
                        //    dsExport.WriteXml(this.dlgSaveFile.FileName, XmlWriteMode.WriteSchema);
                        //}));
                        //t.SetApartmentState(ApartmentState.STA);
                        //t.Start();
                        //t.Join();
                        return "true";

                    case "HTM":
                        this.strDlgFilter = "Web Page Document (*.htm) |*.htm";
                        stream = new FileStream(path, FileMode.Create);
                        writer = new StreamWriter(stream);
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        str2 = "<html>\n";
                        str2 = ((((str2 + "\t<head>\n") + "\t\t<title>" + dsExport.Tables[0].TableName + " Data</title>\n") + "\t\t<meta name=\"GENERATOR\" content=\"Vedas Data Exporter V1.0.5\">\n" + "\t</head>\n") + "\t<body>\n" + "\t\t<br><br><br>\n") + "\t\t\t<table cellpadding=1 cellspacing=1 border=1>\n" + "\t\t\t\t<tr>\n";
                        num = 0;
                        goto Label_0746;

                    case "XLS":
                        this.strDlgFilter = "Microsoft Excel Files (*.xls) |*.xls";
                        ExporttoExcel(data, path);
                        goto Label_0CEE;

                    case "SDF":
                    {
                        this.strDlgFilter = "Text Files (*.txt) |*.txt";
                        stream = new FileStream(path, FileMode.Create);
                        writer = new StreamWriter(stream);
                        str2 = "";
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        table2 = new DataTable("recordWidth");
                        DataColumn column = new DataColumn("FieldName");
                        DataColumn column2 = new DataColumn("FieldWidth");
                        table2.Columns.Add(column);
                        table2.Columns.Add(column2);
                        num = 0;
                        goto Label_0A44;
                    }
                    default:
                        goto Label_0CEE;
                }
            Label_0167:
                str2 = str2 + "\"" + data.Columns[num].ColumnName + "\",";
                num++;
            Label_0194:
                if (num <= (data.Columns.Count - 1))
                {
                    goto Label_0167;
                }
                str2 = str2.Substring(0, str2.Length - 1);
                writer.WriteLine(str2);
                num = 0;
                while (num <= (data.Rows.Count - 1))
                {
                    str2 = "";
                    num2 = 0;
                    while (num2 <= (data.Columns.Count - 1))
                    {
                        str2 = str2 + data.Rows[num][num2].ToString().Replace(",", " ").Replace("\r", " ").Replace("\n", " ") + ",";
                        num2++;
                    }
                    str2 = str2.Substring(0, str2.Length - 1);
                    writer.WriteLine(str2);
                    num++;
                }
                writer.Close();
                stream.Close();
                goto Label_0CEE;
            Label_02E0:
                str2 = "";
                for (num2 = 0; num2 <= (data.Columns.Count - 1); num2++)
                {
                    if (data.Columns[num2].DataType == typeof(string))
                    {
                        str2 = str2 + "\"" + data.Rows[num][num2].ToString().Replace(",", " ").Replace("\r", " ").Replace("\n", " ").Trim() + "\"~";
                    }
                    else
                    {
                        str2 = str2 + data.Rows[num][num2].ToString().Replace(",", " ").Replace("\r", " ").Replace("\n", " ").Trim() + "~";
                    }
                }
                str2 = str2.Substring(0, str2.Length - 1);
                writer.WriteLine(str2);
                num++;
            Label_0418:
                if (num <= (data.Rows.Count - 1))
                {
                    goto Label_02E0;
                }
                writer.Close();
                stream.Close();
                goto Label_0CEE;
            Label_0479:
                str2 = "";
                num2 = 0;
                while (num2 < data.Columns.Count)
                {
                    if (data.Columns[num2].DataType == typeof(string))
                    {
                        str2 = str2 + "\"" + data.Rows[num][num2].ToString().Replace(",", " ").Replace("\r", " ").Replace("\n", " ").Trim() + "\",";
                    }
                    else
                    {
                        str2 = str2 + data.Rows[num][num2].ToString().Replace(",", " ").Replace("\r", " ").Replace("\n", " ").Trim() + ",";
                    }
                    num2++;
                }
                str2 = str2.Substring(0, str2.Length - 1);
                writer.WriteLine(str2);
                num++;
            Label_05AC:
                if (num < data.Rows.Count)
                {
                    goto Label_0479;
                }
                writer.Close();
                stream.Close();
                goto Label_0CEE;
            Label_06F6:
                str2 = str2 + "\t\t\t\t\t<td bgcolor='#ffcc66'><font size=3 face='tahoma,arial,verdana' color='#000000'><b> &nbsp;" + data.Columns[num].ColumnName.ToString().Replace("\r", " ").Replace("\n", " ") + " &nbsp;</b></font></td>\n";
                num++;
            Label_0746:
                if (num <= (data.Columns.Count - 1))
                {
                    goto Label_06F6;
                }
                str2 = str2 + "\t\t\t\t</tr>\n";
                num = 0;
                while (num <= (data.Rows.Count - 1))
                {
                    str2 = str2 + "\t\t\t\t<tr>\n";
                    for (num2 = 0; num2 <= (data.Columns.Count - 1); num2++)
                    {
                        str2 = str2 + "\t\t\t\t\t<td bgcolor='#ffffff'><font size=2 face='tahoma,arial,verdana' color='#000000'>&nbsp;" + data.Rows[num][num2].ToString().Replace("\r", " ").Replace("\n", " ") + "</font></td>\n";
                    }
                    str2 = str2 + "\t\t\t\t</tr>\n";
                    writer.WriteLine(str2);
                    str2 = "";
                    num++;
                }
                str2 = "\t\t\t</table>\n";
                str2 = str2 + "\t</body>\n" + "</html>\n";
                writer.WriteLine(str2);
                writer.Close();
                stream.Close();
                goto Label_0CEE;
            Label_0909:
                num3 = data.Columns[num].ColumnName.Length;
                DataRow row = table2.NewRow();
                row["FieldName"] = data.Columns[num].ColumnName.ToString();
                num2 = 0;
                while (num2 < data.Rows.Count)
                {
                    int length = data.Rows[num2][num].ToString().Length;
                    if (data.Columns[num].DataType == typeof(DateTime))
                    {
                        length = 10;
                    }
                    if (num3 < length)
                    {
                        num3 = length;
                    }
                    num2++;
                }
                row["FieldWidth"] = num3;
                if (str2.Trim() != "")
                {
                    str2 = str2 + "    ";
                }
                str2 = str2 + data.Columns[num].ColumnName.PadRight(num3);
                table2.Rows.Add(row);
                num++;
            Label_0A44:
                if (num < data.Columns.Count)
                {
                    goto Label_0909;
                }
                str2 = str2.Substring(0, str2.Length);
                DataView defaultView = table2.DefaultView;
                for (num = 0; num < data.Rows.Count; num++)
                {
                    str2 = "";
                    for (num2 = 0; num2 < data.Columns.Count; num2++)
                    {
                        if (str2.Trim() != "")
                        {
                            str2 = str2 + "    ";
                        }
                        int totalWidth = Convert.ToInt32(defaultView[num2]["FieldWidth"]);
                        if (data.Columns[num2].DataType == typeof(string))
                        {
                            str2 = str2 + data.Rows[num][num2].ToString().Replace(",", " ").Replace("\r", " ").Replace("\n", " ").PadRight(totalWidth);
                        }
                        else if (data.Columns[num2].DataType == typeof(DateTime))
                        {
                            if (data.Rows[num][num2].ToString().Trim() == "")
                            {
                                str2 = str2 + data.Rows[num][num2].ToString().Replace(",", " ").Replace("\r", " ").Replace("\n", " ").PadRight(totalWidth);
                            }
                            else
                            {
                                str2 = str2 + Convert.ToDateTime(data.Rows[num][num2]).ToString("yyyyMMdd").PadRight(totalWidth);
                            }
                        }
                        else
                        {
                            str2 = str2 + data.Rows[num][num2].ToString().Replace(",", " ").Replace("\r", " ").Replace("\n", " ").PadLeft(totalWidth);
                        }
                    }
                    writer.WriteLine(str2);
                }
                writer.Close();
                stream.Close();
            Label_0CEE:
                str3 = "true";
            }
            catch (Exception exception)
            {
                str3 = "Error in ExportData(ds,outputfile) method \n\n" + exception.Message;
            }
            return str3;
        }
        public static void ActivitychangeLog(string ActivityUser, string Location, string ClientMachineName, string ActivityName, string ActivityInfo, string ActivityType, string RowsCount, string ActivityInfo2, SqlConnection Sconn)
        {
            string sqlLoginlog = @"INSERT into OP_ActivityLog(ActivityDate,ActivityUser,LocationID,MachineName,ActivityType,ActivityName,ActivityInfo,ActivityInfo2,RowsCount)
                                                         VALUES('" + DateTime.Now.ToString() + "','" + ActivityUser + "','" + Location + "','" + ClientMachineName + "','" + ActivityType + "','" + ActivityName + "','" + ActivityInfo + "','" + ActivityInfo2 + "','" + RowsCount + "')";
            Execute(sqlLoginlog, Sconn);
        }
        public static void Execute(string lsql, SqlConnection Sconn)
        {
            if (Sconn.State == ConnectionState.Closed)
                Sconn.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = Sconn;
            cmd.CommandText = lsql;
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
            //oConn.Close();
        }
        public static void ExporttoExcel(DataTable data, string fileName)
        {
            DialogResult retry = DialogResult.Retry;
            while (retry == DialogResult.Retry)
            {
                try
                {
                    using (ExcelWriter writer = new ExcelWriter(fileName))
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartWorksheet("Sheet1");
                        writer.WriteStartRow();
                        foreach (DataColumn column in data.Columns)
                        {
                            writer.WriteExcelUnstyledCell(column.Caption);
                        }
                        writer.WriteEndRow();
                        foreach (DataRow row in data.Rows)
                        {
                            writer.WriteStartRow();
                            foreach (object obj2 in row.ItemArray)
                            {
                                writer.WriteExcelAutoStyledCell(obj2);
                            }
                            writer.WriteEndRow();
                        }
                        writer.WriteEndWorksheet();
                        writer.WriteEndDocument();
                        writer.Close();
                        retry = DialogResult.Cancel;
                    }
                }
                catch (Exception exception)
                {
                    retry = MessageBox.Show(exception.Message, "Excel Export", MessageBoxButtons.RetryCancel, MessageBoxIcon.Asterisk);
                }
            }
        }

        public class ExcelWriter : IDisposable
        {
            private XmlWriter _writer;

            public ExcelWriter(string outputFileName)
            {
                XmlWriterSettings settings = new XmlWriterSettings {
                    Indent = true
                };
                this._writer = XmlWriter.Create(outputFileName, settings);
            }

            public void Close()
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Already closed.");
                }
                this._writer.Close();
                this._writer = null;
            }

            public void Dispose()
            {
                if (this._writer != null)
                {
                    this._writer.Close();
                    this._writer = null;
                }
            }

            public void WriteEndDocument()
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                this._writer.WriteEndElement();
            }

            public void WriteEndRow()
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                this._writer.WriteEndElement();
            }

            public void WriteEndWorksheet()
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                this._writer.WriteEndElement();
                this._writer.WriteEndElement();
            }

            public void WriteExcelAutoStyledCell(object value)
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                if ((value == null) || (value.ToString() == ""))
                {
                    value = "";
                    this.WriteExcelStyledCell(value, CellStyle.General);
                }
                else if (((((value is short) || (value is int)) || ((value is long) || (value is sbyte))) || (((value is ushort) || (value is uint)) || (value is ulong))) || (value is byte))
                {
                    this.WriteExcelStyledCell(value, CellStyle.Number);
                }
                else if (((value is float) || (value is double)) || (value is decimal))
                {
                    this.WriteExcelStyledCell(value, CellStyle.Currency);
                }
                else if (value is DateTime)
                {
                    DateTime time = (DateTime) value;
                    this.WriteExcelStyledCell(value, (time.TimeOfDay.CompareTo(new TimeSpan(0, 0, 0, 0, 0)) == 0) ? CellStyle.ShortDate : CellStyle.DateTime);
                }
                else
                {
                    this.WriteExcelStyledCell(value, CellStyle.General);
                }
            }

            public void WriteExcelColumnDefinition(int columnWidth)
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                this._writer.WriteStartElement("Column", "urn:schemas-microsoft-com:office:spreadsheet");
                this._writer.WriteStartAttribute("Width", "urn:schemas-microsoft-com:office:spreadsheet");
                this._writer.WriteValue(columnWidth);
                this._writer.WriteEndAttribute();
                this._writer.WriteEndElement();
            }

            public void WriteExcelStyledCell(object value, CellStyle style)
            {
                if (value == null)
                {
                    MessageBox.Show("etet");
                }
                else
                {
                    if (this._writer == null)
                    {
                        throw new NotSupportedException("Cannot write after closing.");
                    }
                    this._writer.WriteStartElement("Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                    this._writer.WriteAttributeString("StyleID", "urn:schemas-microsoft-com:office:spreadsheet", style.ToString());
                    this._writer.WriteStartElement("Data", "urn:schemas-microsoft-com:office:spreadsheet");
                    switch (style)
                    {
                        case CellStyle.General:
                            this._writer.WriteAttributeString("Type", "urn:schemas-microsoft-com:office:spreadsheet", "String");
                            break;

                        case CellStyle.Number:
                        case CellStyle.Currency:
                            this._writer.WriteAttributeString("Type", "urn:schemas-microsoft-com:office:spreadsheet", "Number");
                            break;

                        case CellStyle.DateTime:
                        case CellStyle.ShortDate:
                            this._writer.WriteAttributeString("Type", "urn:schemas-microsoft-com:office:spreadsheet", "DateTime");
                            break;
                    }
                    this._writer.WriteValue(value);
                    this._writer.WriteEndElement();
                    this._writer.WriteEndElement();
                }
            }

            private void WriteExcelStyleElement(CellStyle style)
            {
                this._writer.WriteStartElement("Style", "urn:schemas-microsoft-com:office:spreadsheet");
                this._writer.WriteAttributeString("ID", "urn:schemas-microsoft-com:office:spreadsheet", style.ToString());
                this._writer.WriteEndElement();
            }

            private void WriteExcelStyleElement(CellStyle style, string NumberFormat)
            {
                this._writer.WriteStartElement("Style", "urn:schemas-microsoft-com:office:spreadsheet");
                this._writer.WriteAttributeString("ID", "urn:schemas-microsoft-com:office:spreadsheet", style.ToString());
                this._writer.WriteStartElement("NumberFormat", "urn:schemas-microsoft-com:office:spreadsheet");
                this._writer.WriteAttributeString("Format", "urn:schemas-microsoft-com:office:spreadsheet", NumberFormat);
                this._writer.WriteEndElement();
                this._writer.WriteEndElement();
            }

            private void WriteExcelStyles()
            {
                this._writer.WriteStartElement("Styles", "urn:schemas-microsoft-com:office:spreadsheet");
                this.WriteExcelStyleElement(CellStyle.General);
                this.WriteExcelStyleElement(CellStyle.Number, "General Number");
                this.WriteExcelStyleElement(CellStyle.DateTime, "General Date");
                this.WriteExcelStyleElement(CellStyle.Currency, "Currency");
                this.WriteExcelStyleElement(CellStyle.ShortDate, "Short Date");
                this._writer.WriteEndElement();
            }

            public void WriteExcelUnstyledCell(string value)
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                this._writer.WriteStartElement("Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                this._writer.WriteStartElement("Data", "urn:schemas-microsoft-com:office:spreadsheet");
                this._writer.WriteAttributeString("Type", "urn:schemas-microsoft-com:office:spreadsheet", "String");
                this._writer.WriteValue(value);
                this._writer.WriteEndElement();
                this._writer.WriteEndElement();
            }

            public void WriteStartDocument()
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                this._writer.WriteProcessingInstruction("mso-application", "progid=\"Excel.Sheet\"");
                this._writer.WriteStartElement("ss", "Workbook", "urn:schemas-microsoft-com:office:spreadsheet");
                this.WriteExcelStyles();
            }

            public void WriteStartRow()
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                this._writer.WriteStartElement("Row", "urn:schemas-microsoft-com:office:spreadsheet");
            }

            public void WriteStartWorksheet(string name)
            {
                if (this._writer == null)
                {
                    throw new NotSupportedException("Cannot write after closing.");
                }
                this._writer.WriteStartElement("Worksheet", "urn:schemas-microsoft-com:office:spreadsheet");
                this._writer.WriteAttributeString("Name", "urn:schemas-microsoft-com:office:spreadsheet", name);
                this._writer.WriteStartElement("Table", "urn:schemas-microsoft-com:office:spreadsheet");
            }

            public enum CellStyle
            {
                General,
                Number,
                Currency,
                DateTime,
                ShortDate
            }
        }

        public enum fileType
        {
            CSV,
            TXT,
            XLS,
            XLSX,
            XML,
            HTM,
            NONE,
            TILDE,
            SDF
        }
    }
}

