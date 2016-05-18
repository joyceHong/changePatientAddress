using ClassLibraryFoxDB;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace changePatientAddress
{
    public class viewAddressArrangeRule
    {
        public string keyword
        {
            get;
            set;
        }

        public string beforeKeyword
        {
            get;
            set;
        }

        /// <summary>
        /// 轉換字規則（國字數字、數字）
        /// </summary>
        public string rule
        {
            get;
            set;
        }

    }

    public class viewPatient
    {
        public string patientSickNo
        {
            get;
            set;
        }

        public string patientName
        {
            get;
            set;
        }

        /// <summary>
        /// 原本的地址
        /// </summary>
        public string orgAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 新的地址
        /// </summary>
        public string newAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 市
        /// </summary>
        public string city
        {
            get;
            set;
        }

        /// <summary>
        /// 區
        /// </summary>
        public string dist
        {
            get;
            set;
        }

        /// <summary>
        /// 路
        /// </summary>
        public string road
        {
            get;
            set;
        }

        /// <summary>
        /// 段
        /// </summary>
        public string sec
        {
            get;
            set;
        }

        /// <summary>
        /// 巷,弄,號,樓 …等
        /// </summary>
        public string doorNumber
        {
            get;
            set;
        }

    }

    public class entityPatients
    {
        public string _cooperPath
        {
            get;
            set;
        }

        private string _logCategory = System.Environment.CurrentDirectory; //取得目前的路徑

        WriteEvent.WrittingEventLog writeObj = new WriteEvent.WrittingEventLog();

        /// <summary>
        /// 取得此病患的地址
        /// </summary>
        /// <param name="dist"></param>
        /// <returns></returns>
        public IList<viewPatient> getPatients(string dist, IList<viewAddressArrangeRule>addressRules)
        {
            try
            {
                IList<viewPatient> liPatients = new List<viewPatient>();
                foxproDB.CooperFolder = _cooperPath;
                DataTable dt = foxproDB.selectQueryWithDataTable("select 病歷編號,病患姓名,地址 from patient where 地址 like '%台中市" + dist.Trim() + "%' or 地址 like '%台中巿" + dist.Trim() + "%'");

                foreach (DataRow dr in dt.Rows)
                {
                    string arrangeAddress = getAddress(dr["地址"].ToString(), dr["病歷編號"].ToString(), dr["病患姓名"].ToString(), addressRules);
                    liPatients.Add(new viewPatient()
                    {
                        patientSickNo = dr["病歷編號"].ToString(),
                        patientName = dr["病患姓名"].ToString(),
                        orgAddress = arrangeAddress
                    });
                }
                return liPatients;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //public void repleatPatientAddress(IList<viewPatient>liPatients)
        //{
        //    try
        //    {
        //        foxproDB.CooperFolder = @"c:\vfb";
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public DataTable getAreas()
        {
            try
            {
                foxproDB.CooperFolder = Directory.GetCurrentDirectory()+"\\db";
                DataTable dt = foxproDB.selectQueryWithDataTable("select ikey,區別 from importdist");
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cityID"></param>
        /// <param name="distID"></param>
        /// <returns></returns>
        public DataTable getStreetNumberData(string cityName , int distID)
        {
            try
            {
                foxproDB.CooperFolder = Directory.GetCurrentDirectory() + "\\db";
                DataTable dt = foxproDB.selectQueryWithDataTable(@"
                    select a.ikey,b.區別,c.里別,d.舊路名,d.新路名,a.舊門號,a.新門號 
                    from importStreetnumber as a 
                    left join importdist as b on (a.dist_id=b.ikey) 
                    left join importvillage as c on (a.village_id=c.ikey) 
                    left join importroad as d on (a.road_id=d.ikey) 
                    where a.dist_id=" + distID);
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string getAddress(string address,string patientSickNo, string patientName, IList<viewAddressArrangeRule> addressRules)
        {
            try
            {

                address = replaceChar(address); //轉換半形,F 轉樓

                //依據路名、巷、弄、號 找到關鍵字，拆解後，再進行規則轉換
                foreach (viewAddressArrangeRule addresRule in addressRules)
                {
                     address=  getRuleAddress(address, addresRule.beforeKeyword, addresRule.keyword, addresRule.rule);
                }
                return address;
            }
            catch (Exception ex)
            {
                writeObj.writeToFile("error addr :" + address);
                writeObj.writeToFileNoTime("errorAddress.txt", _logCategory, "病患編號:" + patientSickNo + "  病患名稱:" + patientName + "  地址:" + address + "\r\n");
                return address;
                //throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 事先整理住址，例如全形轉半形，國字數字轉阿拉伯等
        /// </summary>
        /// <param name="address"></param>
        /// <param name="beforeKey"></param>
        /// <param name="afterKey"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public string getRuleAddress(string address, string beforeKey, string afterKey, string rule)
        {
            try
            {
                //搜尋字串的部份片段
                int startIndex = 0;
                int endIndex = 0;

                getStartEndPosition(address, beforeKey, afterKey, out startIndex, out endIndex);

                //如果沒有afterKey (代表並無此關鍵字，略過)
                if (endIndex <= -1 || startIndex <= -1)
                    return address;

                //全形轉半型
                string newAddress = address.Substring(startIndex+1, endIndex - startIndex-1);

                if (newAddress == null)
                    return address;
                
                //根據自訂規則，轉成中文數字，阿拉拍數字等等
                switch (rule.ToUpper())
                {
                    case "CNUMBER":
                        newAddress = convertNumberChinese(newAddress);
                        break;

                    case "NUMBER":
                        newAddress = convertChineseNumber(newAddress);
                        break;
                }

                address = address.Remove(startIndex + 1, endIndex - startIndex-1).Insert(startIndex + 1, newAddress);
                return address;
            }
            catch (Exception ex)
            {                
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// 搜尋取得部份字串的起迄時段
        /// </summary>
        /// <param name="address"></param>
        /// <param name="beforeKey"></param>
        /// <param name="afterKey"></param>
        /// <param name="rule"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        public void getStartEndPosition(string address, string beforeKey, string afterKey,out int startIndex, out int endIndex)
        {
            try
            {
                startIndex = -1; //一開始初始化, 即使beforeKey為空值，預設為第一個值開始搜尋
                if(string.IsNullOrEmpty(beforeKey) == false){
                    char[] beforeKeys = beforeKey.ToCharArray();
                    foreach (char singleKey in beforeKeys)
                    {
                        startIndex = address.LastIndexOf(singleKey);
                        if (startIndex > 0)
                        {
                            //如果已找到值立刻跳出迴圈
                            break;
                        }
                    }
                }
                //取得最後一碼的位置
                endIndex = (string.IsNullOrEmpty(afterKey)) ? address.Length : address.LastIndexOf(afterKey);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 將部份
        /// </summary>
        public viewPatient splitPatientAddress(viewPatient viewPatientObj,IList<viewAddressArrangeRule>listSplitAddress)
        {
            try
            {
                //搜尋字串的部份片段
                int startIndex = -1;
                int endIndex = -1;
                string partAddress = "";
                foreach (viewAddressArrangeRule singleSplitAdress in listSplitAddress)
                {
                    getStartEndPosition(viewPatientObj.orgAddress, singleSplitAdress.beforeKeyword, singleSplitAdress.keyword, out startIndex, out endIndex);//取得開始,結束的搜尋位置

                    //當無afterKey關鍵字，會自動算出目前的字串長度，所以會超出陣列，需要減一
                    if (endIndex == viewPatientObj.orgAddress.Length)
                    {
                        endIndex -= 1;
                    }
                    else if (endIndex <= -1)
                    {
                        continue; //找不到關鍵字【略過】 ，跳下一迴圈
                    }
                    else if (endIndex < startIndex)
                    {
                        writeObj.writeToFile("無法正確分析 splitPatientAddress : " + viewPatientObj.orgAddress);
                        writeObj.writeToFileNoTime("errorAddress.txt", _logCategory, "病患編號:" + viewPatientObj.patientSickNo + "  病患姓名:" + viewPatientObj.patientName + "  地址:" + viewPatientObj.orgAddress + "\r\n");
                        //不合理的地址搜尋邏輯
                        continue;
                    }
                    
                    partAddress = viewPatientObj.orgAddress.Substring(startIndex+1, endIndex - startIndex); //取部份字串

                   

                    switch (singleSplitAdress.beforeKeyword)
                    {
                        case "市": //區
                            viewPatientObj.dist = partAddress;
                            break;
                        case "區"://市~ 路 
                            viewPatientObj.road = partAddress;
                            break;
                        case "路": //路 ~ 段
                            viewPatientObj.sec = partAddress;
                            break;
                        case "段街路": //段之後(含巷弄號樓…)
                            viewPatientObj.doorNumber = partAddress;
                            break;
                        default: // XX市
                            viewPatientObj.city = partAddress;
                            break;
                    }
                }
                return viewPatientObj;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool updatePatientAddress(string sickNo, string newAddress)
        {
            try
            {
                foxproDB.CooperFolder = _cooperPath;
                List<columnsData> liColumns = new List<columnsData>();            

                 liColumns.Add(new columnsData()
                {
                     strFileName="地址",
                     strValue=newAddress,
                     oledbTypeValue =  OleDbType.Char,
                });
                 foxproDB.updateWithParameter("patient", liColumns, " 病歷編號='" + sickNo + "'");
                 writeObj.writeToFile("更新地址: 病歷編號:" + sickNo + "  新地址:"+newAddress);
                 return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 全形轉半形, 將F==>樓， - 改之
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>        
        public string replaceChar(string address)
        {
            try
            {
                address = Strings.StrConv(address, VbStrConv.Narrow, 0); //全形轉半形
                address = address.Replace('F', '樓');
                address = address.Replace("-", "之");
                address = address.Replace("巿", "市");
                return address;
            }
            catch (Exception ex)
            {   
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 國字轉阿拉伯
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string convertChineseNumber(string address)
        {
            char[] strAddress = address.ToCharArray();
            //二十一  2 + 
            int addressNumber = 0;
            int count=0;
            int beforeNumber = 0;
            foreach (char singleChar in strAddress)
            {
                int number = 0;
                switch (singleChar)
                {
                    case '一':
                        number = 1;
                        break;
                    case '二':
                         number = 2;
                        break;
                    case '三':
                         number =3;
                        break;
                    case '四':
                       number =4;
                        break;
                    case '五':
                        number = 5;
                        break;
                    case '六':
                        number = 6;
                        break;
                    case '七':
                        number = 7;
                        break;
                    case '八':
                       number = 8;
                        break;
                    case '九':
                        number = 9;
                        break;
                    case '十':
                        if (count == 1)
                            number = (addressNumber * 10) - addressNumber; //為了轉換進位
                        else if (count == 3)
                            number = (beforeNumber * 10) - beforeNumber;
                        else
                            number = 10;
                        break;
                    case '百':
                        if (count == 1 )
                            number = (addressNumber * 100) - addressNumber; //為了轉換進位
                        else
                            number = 100;
                        break;
                    case '零':
                        if (count == 1)
                            number = (addressNumber * 100) - addressNumber; //為了轉換進位
                        else
                            number = 100;
                        break;
                    default:
                        return address;
                }
                addressNumber += number;
                beforeNumber = number;
                count ++;
            }
            return addressNumber.ToString();
        }

        /// <summary>
        /// 阿拉伯轉國字
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string convertNumberChinese(string address)
        {
            switch (address)
            {
                case "1":
                    return "一";
                case "2":
                    return "二";
                case "3":
                    return "三";
                case "4":
                    return "四";
                case "5":
                    return "五";
                case "6":
                    return "六";
                case "7":
                    return "七";
                case "8":
                    return "八";
                case "9":
                    return "九";
                case "10":
                    return "十";  
                default:
                    return address;
            }
        }
    }
}
