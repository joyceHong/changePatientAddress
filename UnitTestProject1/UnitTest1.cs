using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using changePatientAddress;
using System.Collections.Generic;
using System.Data;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void convertChineseNumber()
        {
            entityPatients entityPatient = new entityPatients();
            Assert.AreEqual("146", entityPatient.convertChineseNumber("一百四十六"));
            //entityAddressObj
        }

        [TestMethod]
        public void convertNumberToChinese()
        {
            entityPatients entityPatient = new entityPatients();
            Assert.AreEqual("一", entityPatient.convertNumberChinese("1"));
        }

        [TestMethod]
        public void halfChinese()
        {
            entityPatients entityPatient = new entityPatients();
            Assert.AreEqual("1", entityPatient.replaceChar("１"));
        }

        [TestMethod]
        public void searchText()
        {
            string str = "新北市中和區連城路五零七號2樓之2";
            entityPatients entityPatient = new entityPatients();
            Assert.AreEqual("507", entityPatient.getRuleAddress(str,"路","號","number"));
        }

        [TestMethod]
        public void search()
        {
            #region MyRegion
            IList<viewAddressArrangeRule> _addressRules = new List<viewAddressArrangeRule>();
            _addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "路",
                keyword = "段",
                rule = "Cnumber", //數字要轉國字
            });

            _addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "段,路",
                keyword = "號",
                rule = "number" //國字要轉字
            });

            _addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "段,路",
                keyword = "巷",
                rule = "number"
            });

            _addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "巷",
                keyword = "弄",
                rule = "number"
            });

            _addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "弄,巷,段,路",
                keyword = "號",
                rule = "number"
            });

            _addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "號",
                keyword = "樓",
                rule = "cnumber"
            });

            _addressRules.Add(new viewAddressArrangeRule()
            {
                beforeKeyword = "之",
                rule = "number"
            }); 
            #endregion

            string str = "台中市西區五權七街112巷7號5樓";
            entityPatients entityPatient = new entityPatients();
            Assert.AreEqual("台中市西區五權七街112巷7號五樓", entityPatient.getAddress(str,"","", _addressRules));
        }

        [TestMethod]
        public void splitAddress()
        {
            
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

            viewPatient viewObj = new viewPatient();
            //viewObj.orgAddress = "台中市中區中正路278之1號"; //passing
            viewObj.orgAddress = "台中市西區五權七街112巷5樓7號";    
  
            entityPatients entityPatient = new entityPatients();
            viewObj.orgAddress = entityPatient.replaceChar(viewObj.orgAddress);

            entityPatient.splitPatientAddress(viewObj, splitPatientRule);
        }


        [TestMethod]
        public void getStreetNumberData()
        {
            contrReplaceAddress controlObj = new contrReplaceAddress();
            controlObj._cooperPath = @"d:\cooper";

            controlObj.replacePatientAddress();
        }


    }
}
