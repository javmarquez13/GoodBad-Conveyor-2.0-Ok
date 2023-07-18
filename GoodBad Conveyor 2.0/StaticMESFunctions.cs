using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodBad_Conveyor_2._0
{
    class StaticMESFunctions
    {
        public static List<string> ListByPanel(string SerialNumber)
        {
            List<string> _ListByPanel = new List<string>();
            DataTable _dtPanel = new DataTable();

            DataSet _dsQuery = new MES.Service().SelectBySerialNumber(SerialNumber);
            int _CustomerID = Convert.ToInt32(_dsQuery.Tables[0].Rows[0][2]);
            int _WIP_ID = Convert.ToInt32(_dsQuery.Tables[0].Rows[0][0]);

            if (SerialNumber.Length == Globals.SMOTHER_LENGH) _dtPanel = new MES.Service().ListByBoard(_WIP_ID + 1).Tables[0];
            if (SerialNumber.Length == Globals.SN_LENGH) _dtPanel = new MES.Service().ListByBoard(_WIP_ID).Tables[0];

            foreach (DataRow dr in _dtPanel.Rows)
                _ListByPanel.Add(dr[3].ToString());

            return _ListByPanel;
        }
    }
}
