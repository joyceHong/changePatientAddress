using ClassLibraryFoxDB;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace changePatientAddress
{
    public class entityImportAddress
    {
        WriteEvent.WrittingEventLog writeObj = new WriteEvent.WrittingEventLog();
        string _Cooper = Directory.GetCurrentDirectory() + "\\db"; 
        

        public int saveArea(string area)
        {
            try
            {
                foxproDB.CooperFolder = _Cooper;                
                object objArea= foxproDB.selectQueryWithExecuteScalar("select ikey from importdist where 區別='" + area.Trim() + "'");
                int returnIkey = 0;
                if (objArea != null)
                {
                    int.TryParse(objArea.ToString(), out returnIkey);
                }
                else
                {
                    List<columnsData> liColumnsData = new List<columnsData>();
                    int ikey = foxproDB.getIkey("importdist");
                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "ikey",
                        strValue = ikey.ToString(),
                        oledbTypeValue = OleDbType.Integer 
                    });

                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "區別",
                        strValue = area.Trim(),
                        oledbTypeValue = OleDbType.Char 
                    });

                    foxproDB.addWithParameter("importdist", liColumnsData);
                    returnIkey = ikey;
                }
                return returnIkey;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int saveiVillage(string villageName)
        {
            try
            {
                foxproDB.CooperFolder = _Cooper;  
                object objCity = foxproDB.selectQueryWithExecuteScalar("select ikey from importvillage where 里別='" + villageName.Trim() + "'");
                int returnIkey = 0;
                if (objCity != null)
                {
                    int.TryParse(objCity.ToString(), out returnIkey);
                }
                else
                {
                    List<columnsData> liColumnsData = new List<columnsData>();
                    int ikey = foxproDB.getIkey("importvillage");
                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "ikey",
                        strValue = ikey.ToString(),
                        oledbTypeValue = OleDbType.Integer
                    });

                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "里別",
                        strValue = villageName.Trim(),
                        oledbTypeValue = OleDbType.Char
                    });

                    foxproDB.addWithParameter("importvillage", liColumnsData);
                    returnIkey = ikey;
                }
                return returnIkey;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int saveLoad( string oldLoad, string newLoad)
        {
            try
            {
                foxproDB.CooperFolder = _Cooper;
                object objLoad = foxproDB.selectQueryWithExecuteScalar("select ikey from importroad where 舊路名='" + oldLoad + "'  and 新路名='" + newLoad + "'");
                int returnIkey = 0;
                if (objLoad != null)
                {
                    int.TryParse(objLoad.ToString(), out returnIkey);
                }
                else if (string.IsNullOrEmpty(newLoad) == false)
                {
                    List<columnsData> liColumnsData = new List<columnsData>();
                    int ikey = foxproDB.getIkey("importroad");
                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "ikey",
                        strValue = ikey.ToString(),
                        oledbTypeValue = OleDbType.Integer
                    });

                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "舊路名",
                        strValue = oldLoad.Trim(),
                        oledbTypeValue = OleDbType.Char
                    });

                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "新路名",
                        strValue = newLoad.Trim(),
                        oledbTypeValue = OleDbType.Char
                    });

                    foxproDB.addWithParameter("importroad", liColumnsData);
                    returnIkey = ikey;
                }
                return returnIkey;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int saveStreetNumber(int distID, int villageID, int roadID, string oldSteetNumber, string newStreetNumber)
        {
            try
            {
                foxproDB.CooperFolder = _Cooper;
                object objStreetNumber = foxproDB.selectQueryWithExecuteScalar("select ikey from importStreetNumber where 舊門號='" + oldSteetNumber + "'  and 新門號='" + newStreetNumber + "' and dist_id="+distID + " and village_id="+villageID + "  and road_id="+roadID);
                int returnIkey = 0;
                if (objStreetNumber != null)
                {
                    int.TryParse(objStreetNumber.ToString(), out returnIkey);
                }
                else if (string.IsNullOrEmpty(newStreetNumber) == false)
                {
                    if (string.IsNullOrEmpty(newStreetNumber.Trim()) == true)
                    {
                        //writeObj.writeToFile("156 <<<< old:" + oldSteetNumber + "  new" + newStreetNumber);
                        return 0;
                    }

                    List<columnsData> liColumnsData = new List<columnsData>();
                    int ikey = foxproDB.getIkey("importStreetNumber");
                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "ikey",
                        strValue = ikey.ToString(),
                        oledbTypeValue = OleDbType.Integer
                    });

                    liColumnsData.Add(new columnsData()
                   {
                       strFileName = "dist_id",
                       strValue = distID.ToString(),
                       oledbTypeValue = OleDbType.Integer
                   });


                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "village_id",
                        strValue = villageID.ToString(),
                        oledbTypeValue = OleDbType.Integer
                    });

                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "road_id",
                        strValue = roadID.ToString(),
                        oledbTypeValue = OleDbType.Integer
                    });

                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "舊門號",
                        strValue = oldSteetNumber,
                        oledbTypeValue = OleDbType.Char
                    });

                    liColumnsData.Add(new columnsData()
                    {
                        strFileName = "新門號",
                        strValue = newStreetNumber,
                        oledbTypeValue = OleDbType.Char
                    });

                    foxproDB.addWithParameter("importStreetNumber", liColumnsData);
                    writeObj.writeToFile("207 insert <<<< old:" + oldSteetNumber + "  new" + newStreetNumber);
                    returnIkey = ikey;
                }
                else
                {
                    //writeObj.writeToFile("211 <<<< old:" + oldSteetNumber + "  new" + newStreetNumber);
                }
                return returnIkey;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
