﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using System.Xml;

namespace XmlFileCompare
{
    public partial class FileContents : System.Web.UI.Page
    {
        DataTable dtInsert = null;
        DataTable dtDelete = null;
        DataTable dtUpdate = null;
        DataTable dtUpdateJoin = null;
        string filePath;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //get the xml files path from web config
                filePath = WebConfigurationManager.AppSettings["xmlFilePath"].ToString();
                var files = Directory.GetFiles(filePath, "*.xml");
                IEnumerable<string> data = files.Select(purgePath);

                //load the list of xml files to the drop down list box
                ddFileNames.DataSource = data;
                ddFileNames.DataBind();
                ddFileNames.Items.Insert(0, new ListItem("--Please Select--", "0"));
            }

        }
        #region Read and load xml file
        //Read the xml and load it to a gridview
        private void ReadXml(string filename)
        {

            filePath = WebConfigurationManager.AppSettings["xmlFilePath"].ToString();
            string fileNameWithPath = filePath + "\\" + filename + ".xml";

            clearGrids();

            GridViewBind(fileNameWithPath);
        }
        private void GridViewBind(string fileName)
        {
            try
            {
                DataSet fileDataSet = new DataSet();
                fileDataSet.ReadXml(fileName);
                dtInsert = null;
                dtDelete = null;

                if (fileDataSet.Tables.Count > 0)
                {

                    //loop through the dataset and bind insert,delete,update and insert and delete to each grids
                    for (int i = 0; i < fileDataSet.Tables.Count; i++)
                    {
                        if (fileDataSet.Tables[i].TableName == "Insert")
                        {
                            grdViewInsert.Visible = true;
                            dtInsert = RemoveColumns(fileDataSet.Tables[i]);
                        }
                        else if (fileDataSet.Tables[i].TableName == "Delete")
                        {
                            grdViewDelete.Visible = true;
                            dtDelete = RemoveColumns(fileDataSet.Tables[i]);
                        }
                        else if (fileDataSet.Tables[i].TableName == "Update")
                        {
                            dtUpdate = fileDataSet.Tables[i];
                        }
                        //Readxml method assigns an update_Id to theboth update and OldValues element 
                        //join the update and oldvalues using the update_Id and combine them to return 1 row
                        else if (fileDataSet.Tables[i].TableName == "OldValues")
                        {
                            DataTable dtOldValues = fileDataSet.Tables[i];
                            //join update table and oldvalues table to be shown in one grid
                            var allUpdateRows = (from rowsupdate in dtUpdate.AsEnumerable()
                                                 join rowsOldValues in dtOldValues.AsEnumerable()
                                                 on rowsupdate["Update_Id"] equals rowsOldValues["Update_Id"]
                                                 select new
                                                 {
                                                     dtOldValues,
                                                     dtUpdate

                                                 }).ToList();


                            //create a new table with the columns from dtoldvalues and dtupdate
                            dtUpdateJoin = CreateUpdateTable(dtOldValues, dtUpdate);

                            //loop through the join result from the linq and insert the combined row to the new table with the columns from dtoldvalues and dtupdate
                            int j = 0;
                            DataRow dr = dtUpdateJoin.NewRow();

                            foreach (DataRow itemOldValue in allUpdateRows[j].dtOldValues.Rows)
                            {
                                DataRow itemUpdate = allUpdateRows[j].dtUpdate.Rows[j];
                                dr = AddDataToDataRow(itemOldValue, itemUpdate);
                                dtUpdateJoin.Rows.Add(dr);
                                j++;

                            }

                            //Remove the update_id field as it is not present in file, it was generated by Readxml method when parsing the xml
                            dtUpdateJoin.Columns.Remove("Updated Value-Update_Id");
                            dtUpdateJoin.Columns.Remove("Old Value-Update_Id");

                            //bind the update gridview to the new table rows
                            grdViewUpdate.Visible = true;
                            grdViewUpdate.DataSource = dtUpdateJoin;
                            grdViewUpdate.DataMember = "Update";
                            grdViewUpdate.DataBind();
                        }
                    }


                    //if the file has both insert and delete data, then seperate the entries that is present in both inserts and deletes
                    if (dtInsert != null && dtDelete != null)
                    {
                        ShowInsertAndDeletes();
                        ShowInserts();
                        ShowDeletes();
                    }
                    else if (dtInsert != null) //if there is only insert data, no need to do the comparison logic above
                    {

                        grdViewInsert.DataSource = dtInsert;
                        grdViewInsert.DataMember = "Insert";
                        grdViewInsert.DataBind();
                        grdViewInsert.Visible = true;

                    }
                    else if (dtDelete != null) //if there is only delete data, no need to do the comparison logic above
                    {
                        grdViewDelete.DataSource = dtDelete;
                        grdViewDelete.DataMember = "Delete";
                        grdViewDelete.DataBind();
                        grdViewDelete.Visible = true;
                    }
                    else     //no insert and delete data, hide both grids
                    {
                        grdViewDelete.Visible = false;
                        grdViewInsert.Visible = false;
                    }
                    if (dtUpdate != null && grdViewUpdate.Rows.Count == 0) //if update grid is not bound yet in the cmobine logic of update values and oldvalues
                    {
                        grdViewUpdate.Visible = false;
                        grdViewInsert.Visible = false;

                        grdViewUpdate.DataSource = dtUpdate;
                        grdViewUpdate.DataMember = "Update";

                        grdViewUpdate.DataBind();

                    }
                }

            }
            catch (Exception ex)
            {
                WriteError(ex.Message + ex.StackTrace);
                divDetails.Visible = false;
                labelErrorMsg.Visible = true;
                labelErrorMsg.Text = "An Error happened while loading the xml file. Please check the errorlog";

            }
            
        }

        private DataRow AddDataToDataRow(DataRow itemOldValue, DataRow itemUpdate)
        {
            DataRow dr = dtUpdateJoin.NewRow();

            int i = 0;
            for (int j = 0; j < itemOldValue.ItemArray.Count(); j++)
            {
                dr[i] = itemOldValue[j];
                i++;
            }
            for (int j = 0; j < itemUpdate.ItemArray.Count(); j++)
            {
                dr[i] = itemUpdate[j];
                i++;
            }

            return dr;

        }

        private void ShowInsertAndDeletes()
        {
            //get  the rows that is present in both insert and delete and show them in "insert & delete" grid
            var rows = dtInsert.AsEnumerable().Intersect
                   (dtDelete.AsEnumerable(), DataRowComparer.Default);
            if (rows.Any())
            {
                grdViewInsertandDelete.Visible = true;
                DataTable dtCompare = rows.CopyToDataTable();
                grdViewInsertandDelete.DataSource = dtCompare;
                grdViewInsertandDelete.DataBind();
                string fileName = ddFileNames.SelectedValue.ToString();
                //if (dataTable.Columns.Contains(fileName+ "_Id"))
                // ((DataControlField)grdViewInsertandDelete.Columns
                //.Cast<DataControlField>()
                //.Where(fld => (fld.HeaderText == fileName + "_Id"))
                //.SingleOrDefault()).Visible = false;

                //int countOfColumns = grdViewInsertandDelete.Rows[0].Cells.Count;
                //grdViewInsertandDelete.Columns[countOfColumns-1].Visible = true;
            }
        }

        private void ShowDeletes()
        {

            //Get all entries  that is not common with the insert grid
            var deleteOnlyRows = dtDelete.AsEnumerable().Except
            (dtInsert.AsEnumerable(), DataRowComparer.Default);
            if (deleteOnlyRows.Any())
            {
                grdViewDelete.Visible = true;
                grdViewDelete.DataSource = deleteOnlyRows.CopyToDataTable();
                grdViewDelete.DataMember = "Deletes";
                grdViewDelete.DataBind();
            }
            else
            {
                grdViewDelete.Columns.Clear();
                grdViewDelete.DataBind();
                grdViewDelete.Visible = false;
            }


        }


        private string purgePath(string fullName)
        {
            return System.IO.Path.GetFileNameWithoutExtension(fullName);
        }

        protected void ddFileNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            divDetails.Visible = true;
            labelErrorMsg.Text = "";
            labelErrorMsg.Visible = false;
            ReadXml(ddFileNames.SelectedValue);
        }



        private void ShowInserts()
        {
            //Get all entries  that is not common with the delete table
            var insertOnlyRows = dtInsert.AsEnumerable().Except
            (dtDelete.AsEnumerable(), DataRowComparer.Default);
            if (insertOnlyRows.Any())
            {
                grdViewInsert.DataSource = insertOnlyRows.CopyToDataTable();
                grdViewInsert.DataMember = "Inserts";
                grdViewInsert.DataBind();
            }
            else
            {
                grdViewInsert.Columns.Clear();
                grdViewInsert.DataBind();
                grdViewInsert.Visible = false;
            }
        }

        //Remove the colmns that is not needed to be displayed
        private DataTable RemoveColumns(DataTable dataTable)
        {
            try
            {


                string fileName = ddFileNames.SelectedValue;
                DataTable dt = new DataTable();
                var fullPath = System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/FileColumns.xml");
                StreamReader Reader = new StreamReader(fullPath, true);
                DataSet ds = new DataSet();
                ds.ReadXml(Reader);

                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                    DataRow[] drRows = dt.Select("FileName='" + fileName + "'");
                    if (drRows.Length > 0)
                    {
                        string strColumns = drRows[0]["Columns"].ToString();
                        string[] columns = strColumns.Split(',');
                        foreach (string column in columns)
                        {
                            if (dataTable.Columns.Contains(column))
                            {
                                dataTable.Columns.Remove(column);
                            }
                        }
                    }
                }
                if (dataTable.Columns.Contains("OldValues"))
                {
                    dataTable.Columns.Remove("OldValues");
                }

                if (dataTable.Columns.Contains("Lockid"))
                {
                    dataTable.Columns.Remove("Lockid");
                }

                //if (dataTable.Columns.Contains("BaseCommonCodeKey"))
                //{
                //    dataTable.Columns.Remove("BaseCommonCodeKey");
                //}
                if (dataTable.Columns.Contains("__ID__"))
                {
                    dataTable.Columns.Remove("__ID__");
                }
                if (dataTable.Columns.Contains("CommonCodeKey"))
                {
                    dataTable.Columns.Remove("CommonCodeKey");
                }
                Reader.Close();
            }
            catch (Exception ex)
            {

              
               // Response.Write("<script>alert('Exception:An Error happened while trying to remove unwanted columns from file')</script>");
                // Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "An Error happened while trying to remove unwanted columns from file", true);
                throw ex;
            }

            return dataTable;
        }


        //Clear the gridviews
        private void clearGrids()
        {
            grdViewInsert.Columns.Clear();
            grdViewInsert.Visible = false;

            grdViewDelete.Columns.Clear();
            grdViewDelete.Visible = false;

            grdViewUpdate.Columns.Clear();
            grdViewUpdate.Visible = false;

            grdViewInsertandDelete.Columns.Clear();
            grdViewInsertandDelete.Visible = false;
        }





        private DataTable CreateUpdateTable(DataTable dt1, DataTable dt2)
        {
            DataTable table = new DataTable();

            foreach (DataColumn column in dt1.Columns)
            {
                //if the columns exists in both tables then prefix the name with the "updated value" for the updated columns
                if (dt2.Columns.Contains(column.ColumnName))
                {
                    table.Columns.Add("Old Value-" + column.ColumnName, column.DataType);
                }

            }
            foreach (DataColumn column in dt2.Columns)
            {
                //if the columns exists in both tables then prefix the name with the "updated value" for the updated columns
                if ((table.Columns.Contains(column.ColumnName)) || (dt1.Columns.Contains(column.ColumnName)))
                {
                    table.Columns.Add("Updated Value-" + column.ColumnName, column.DataType);
                }
                else
                {
                    table.Columns.Add(column.ColumnName, column.DataType);
                }
            }
            return table;
        }


        //show the old values in red color and updated values in green color
        protected void grdViewUpdate_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                for (int i = 0; i < dtUpdateJoin.Columns.Count; i++)
                {
                    if (grdViewUpdate.HeaderRow.Cells[i].Text.Contains("Updated Value-"))
                    {

                        e.Row.Cells[i].ForeColor = Color.Green;
                    }

                    if (grdViewUpdate.HeaderRow.Cells[i].Text.Contains("Old Value-"))
                    {
                        e.Row.Cells[i].ForeColor = Color.Red;
                    }
                }
            }
        }
        #endregion

        protected void grdViewInsertandDelete_RowCreated(object sender, GridViewRowEventArgs e)
        {
            int count = e.Row.Cells.Count;
            e.Row.Cells[count - 1].Visible = false; // hides the last column which was autogenerated

        }
        protected void grdViewInsert_RowCreated(object sender, GridViewRowEventArgs e)
        {
            int count = e.Row.Cells.Count;
            e.Row.Cells[count - 1].Visible = false; // hides the last column which was autogenerated

        }
        protected void grdViewDelete_RowCreated(object sender, GridViewRowEventArgs e)
        {
            int count = e.Row.Cells.Count;
            e.Row.Cells[count - 1].Visible = false; // hides the last column which was autogenerated

        }
        protected void grdViewUpdate_RowCreated(object sender, GridViewRowEventArgs e)
        {
            int count = e.Row.Cells.Count;
            e.Row.Cells[count - 1].Visible = false; //hides the last column which was autogenerated

        }

        public static void WriteError(string errorMessage)
        {
           
               

                string path = "~/Error/ErrorLog" + DateTime.Today.ToString("dd-mm-yy") + ".txt";
                if (!File.Exists(System.Web.HttpContext.Current.Server.MapPath(path)))
                {
                    File.Create(System.Web.HttpContext.Current.Server.MapPath(path)).Close();
                }
                using (StreamWriter w = File.AppendText(System.Web.HttpContext.Current.Server.MapPath(path)))
                {
                    w.WriteLine("\r\nLog Entry : ");
                    w.WriteLine("{0}", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                    string err = "Error in: " + System.Web.HttpContext.Current.Request.Url.ToString() +
                                  ". Error Message:" + errorMessage;
                    w.WriteLine(err);
                    w.WriteLine("__________________________");
                    w.Flush();
                    w.Close();
                }
            }
           

        }
    }


