using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WriteEvent;

namespace changePatientAddress
{
    public class contrReplaceAddress
    {
        public string _cooperPath
        {
            get;
            set;
        }

        /// <summary>
        /// 抓取目前進度
        /// </summary>
        public int currentProgress
        {
            get;
            set;
        }
       
        WrittingEventLog writeObj = new WrittingEventLog();
        entityPatients entityPatient = new entityPatients();
        public void replacePatientAddress()
        {
            #region 預計要把舊地址部份，轉換成符合規定的條件
             IList<viewAddressArrangeRule> addressRules = new List<viewAddressArrangeRule>(); //來取得國字/數字等自動切換
            addressRules.Add(new viewAddressArrangeRule()
                {
                    beforeKeyword = "路",
                    keyword = "段",
                    rule = "cnumber", //數字要轉國字
                });

            addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "段街路",  //如果先找到段，就直接當作開始index，因為要精準的抓【數字】改變格式 (路名範圍:小->大)
                keyword = "號",
                rule = "number" //國字要轉字
            });

            addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "段街路",
                keyword = "巷",
                rule = "number"
            });

            addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "巷",
                keyword = "弄",
                rule = "number"
            });

            addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "弄巷段街路",
                keyword = "號",
                rule = "number"
            });

            addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "號",
                keyword = "樓",
                rule = "cnumber"
            });

            addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "底",
                keyword = "層",
                rule = "cnumber"
            }); 

            addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "之",
                rule = "number"
            });

            
            #endregion

            #region 病患的地址分段
            IList<viewAddressArrangeRule> splitPatientRule = new List<viewAddressArrangeRule>();//取部份字串，來比對新地址
            splitPatientRule.Add(new viewAddressArrangeRule()
            {
                keyword = "市",
            });

            splitPatientRule.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "市",
                keyword = "區",
            });

            splitPatientRule.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "區",
                keyword = "路",
            });

            splitPatientRule.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "區",
                keyword = "街",
            });

            splitPatientRule.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "路",
                keyword = "段",
            });

            //取得段之後的所有字串
            splitPatientRule.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "段街路",
            });
            #endregion

            DataTable dtAreas = entityPatient.getAreas();
            entityPatient._cooperPath = _cooperPath;
            int current = 0;
            new Thread(() =>
            {
                foreach (DataRow dr in dtAreas.Rows)
                {
                    currentProgress += threadDistUpdate(addressRules, splitPatientRule, dr);
                }
            }).Start();
        }

        private int threadDistUpdate(IList<viewAddressArrangeRule> addressRules, IList<viewAddressArrangeRule> splitPatientRule, DataRow dr)
        {
            int distID = 0;
            int.TryParse(dr["ikey"].ToString(), out distID);
            DataTable dtSteetNumbers = entityPatient.getStreetNumberData("台中市", distID); //取得此區域的所有新/舊門牌號碼
            IList<viewPatient> liPatients = entityPatient.getPatients(dr["區別"].ToString(), addressRules); //取得此區域的所有病患地址

            foreach (viewPatient patient in liPatients)
            {
                entityPatient.splitPatientAddress(patient, splitPatientRule);
                try
                {

                    var queryAccordPatientStreet = (from q in dtSteetNumbers.AsEnumerable()
                                                    where q.Field<string>("區別").Trim().Equals(patient.dist.Trim()) &&
                                                    q.Field<string>("舊路名").Trim().Equals((patient.road + patient.sec).Trim()) &&
                                                    q.Field<string>("舊門號").Trim().Equals(patient.doorNumber.Trim())
                                                    select q).FirstOrDefault();
                    if (queryAccordPatientStreet == null)
                    {
                        continue;
                    }
                  
                    string newRloadName = queryAccordPatientStreet.ItemArray[4].ToString().Trim();
                    string newDoorNumber = queryAccordPatientStreet.ItemArray[6].ToString().Trim();
                    string newAddress = patient.city.Trim() + patient.dist.Trim() + newRloadName.Trim() + newDoorNumber.Trim(); //重組新地址
                    entityPatient.updatePatientAddress(patient.patientSickNo, newAddress);
                }
                catch (Exception ex)
                {
                    string errorPatientData = JsonConvert.SerializeObject(patient);
                    writeObj.writeToFile("病患資料:" + errorPatientData + "error Msg;"+ex.Message);
                }
            }
            return 15;
        }
    }
}
