﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBad_Conveyor_2._0
{
    class StaticFunctions
    {
        /// <summary>
        /// Function to Verify CheckPoint in MES, and return a DataTable with the Status for each Panel Array
        /// </summary>
        /// <param name="SerialNumber">Unit Serial Number Variable</param>
        /// <param name="ProcessName">State the name of the process to verify</param>
        /// <returns name="DateTable">Return a DataTable with all serial numbers in panel and their information</returns>
        public static DataTable VerifyCheckPoint(string SerialNumber, string ProcessName)
        {
            DataTable _dtResult = new DataTable();
            DataTable _dtPanel = new DataTable();      
            
            DataSet _dsQuery = new MES.Service().SelectBySerialNumber(SerialNumber);
            int _CustomerID = Convert.ToInt32(_dsQuery.Tables[0].Rows[0][2]);
            int _WIP_ID = Convert.ToInt32(_dsQuery.Tables[0].Rows[0][0]);       
            
            if(SerialNumber.Length == Globals.SMOTHER_LENGH) _dtPanel = new MES.Service().ListByBoard(_WIP_ID + 1).Tables[0];
            if(SerialNumber.Length == Globals.SN_LENGH) _dtPanel = new MES.Service().ListByBoard(_WIP_ID).Tables[0];

            _dtResult = _dtPanel.Copy();
            _dtResult.Columns.Add("History");
            _dtResult.Columns.Add("Status");

            foreach (DataRow _dr in _dtPanel.Rows)
            {
                _dsQuery = new MES.Service().BoardHistoryReport(_dr[3].ToString(), _CustomerID);

                foreach (DataRow _drHistory in _dsQuery.Tables[0].Rows)
                {
                    string _TestType = _drHistory[0].ToString();
                    string _TempStep = _drHistory[9].ToString();
                    string _TempStatus = _drHistory[10].ToString();

                    if (_TestType == "TEST")
                    {
                        if (_TempStep.Contains(ProcessName))
                        {
                            _dtResult.AsEnumerable().Where(row => row.Field<string>("SerialNumber") == _dr[3].ToString())
                                                                .Select(b => b["History"] = ProcessName)
                                                                .ToList();

                            _dtResult.AsEnumerable().Where(row => row.Field<string>("SerialNumber") == _dr[3].ToString())
                                                                .Select(b => b["Status"] = _TempStatus)
                                                                .ToList();
                            break;
                        }

                        if (_TempStep.Contains("MRB"))
                        {
                            _dtResult.AsEnumerable().Where(row => row.Field<string>("SerialNumber") == _dr[3].ToString())
                                                                .Select(b => b["History"] = "MRB")
                                                                .ToList();

                            _dtResult.AsEnumerable().Where(row => row.Field<string>("SerialNumber") == _dr[3].ToString())
                                                    .Select(b => b["Status"] = _TempStatus)
                                                    .ToList();
                            break;
                        }
                    }
                }

            }

            return _dtResult;
        }

        public static void RejectedNotify(int Lane) 
        {
            if(Lane == 1) 
            {
                
            }

            if(Lane == 2) 
            {
                
            }
        }

        public static void NotUsed() 
        {

            DataSet _dsQuery = new MES.Service().SelectBySerialNumber(Globals.SERIAL_NUMBER1);

            Globals.CUSTOMER_ID = Convert.ToInt32(_dsQuery.Tables[0].Rows[0][2]);
            Globals.WIP_ID = Convert.ToInt32(_dsQuery.Tables[0].Rows[0][0]);

            _dsQuery = new MES.Service().BoardHistoryReport(Globals.SERIAL_NUMBER1, Globals.CUSTOMER_ID);


            foreach (DataRow _dr in _dsQuery.Tables[0].Rows)
            {
                string _TempProces = _dr[9].ToString();
                string _TempStatus = _dr[10].ToString();

                if (string.IsNullOrEmpty(_TempProces)) return;


                foreach (string _StepMatrix in Globals.DATA_MATRIX)
                {
                    string StepToCheck = ConfigFiles.reader("DATA_MATRIX", _StepMatrix, Globals.CONFIG_FILE);

                    DataTable EventsByStepMatrix = new DataTable();

                    try
                    {
                        EventsByStepMatrix = _dsQuery.Tables[0].AsEnumerable()
                        .Where(r => r.Field<string>("Test_Process") == StepToCheck)
                        .CopyToDataTable();
                    }
                    catch (Exception) { }

                    if (EventsByStepMatrix.Rows.Count >= 1)
                        Globals.COUNT_MATRIX1++;

                    else
                        Globals.STEPS_MISSING1 += StepToCheck + ", ";

                    if (!string.IsNullOrEmpty(Globals.STEPS_MISSING1))
                    {

                    }
                }
            }
        }

    }
}
