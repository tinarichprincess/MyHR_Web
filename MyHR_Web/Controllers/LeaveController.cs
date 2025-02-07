﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyHR_Web.Models;
using MyHR_Web.ViewModel;
using Newtonsoft.Json;
using prjCoreDemo.ViewModel;

namespace MyHR_Web.Controllers
{
    public class LeaveController : FilterController
    {
        dbMyCompanyContext MyHr = new dbMyCompanyContext();

        public IActionResult LeaveList()  //請假清單查詢
        {

            List<TLeave> listLeave = GetLeaveList();  //從資料庫得下拉是選單
            ViewBag.LeaveCate = listLeave;

            List<TCheckStatus> listStatus = GetStatus();
            ViewBag.LeaveStatus = listStatus;

            ViewData[CDictionary.CURRENT_LOGINED_USERDEPARTMENT] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERDEPARTMENT);
            ViewData[CDictionary.CURRENT_LOGINED_USERDEPARTMENTID] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERDEPARTMENTID);
            ViewData[CDictionary.CURRENT_LOGINED_USERNAME] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERNAME);
            ViewData[CDictionary.CURRENT_LOGINED_GENDER] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_GENDER);
            ViewBag.Name = int.Parse(HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID));
            ViewBag.Gender =  HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_GENDER);

            int UserID = int.Parse(HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID));


            var table = from i in MyHr.TLeaveApplications
                        join d in MyHr.TUserDepartments on i.CDepartmentId equals d.CDepartmentId
                        join u in MyHr.TUsers on i.CEmployeeId equals u.CEmployeeId
                        where (i.CEmployeeId == UserID)
                        orderby i.CApplyDate descending /*依照申請日期降冪排序*/
                        select new TLeaveApplicationViewModel
                        {
                            CApplyNumber = i.CApplyNumber,
                            CEmployeeId = i.CEmployeeId,
                            CEmployeeName = u.CEmployeeName,
                            CDepartmentId = i.CDepartmentId,
                            CDepartmentName = d.CDepartment,
                            CApplyDate = i.CApplyDate,
                            CLeaveCategory = i.CLeaveCategory,
                            CLeaveStartTime = i.CLeaveStartTime,
                            CLeaveEndTime = i.CLeaveEndTime,
                            CReason = i.CReason,
                            CCheckStatus = i.CCheckStatus,
                            CLeaveHours = i.CLeaveHours,
                            Gender = u.CGender,
                            NONO = LeaveCate_Count(UserID), // 計算各個假別與之請假時數
                            Leave_Specil = Leave_CountSpecialDayoff(UserID),//計算特休


                        };

            ViewBag.Special = Leave_CountSpecialDayoff(UserID);
         
            
               return View(table);
            
         
            //-------------------------------------------------

            //foreach (var item in table)
            //{
            //    item.Leave_Specil = Leave_CountSpecialDayoff(UserID);// 想要只取一筆特休就好了，但是有連接問題
            //}

            
  





            //******************************************************************************************************
            //int useraccount = int.Parse(HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID));
            //List<TLeaveApplicationViewModel> list = new List<TLeaveApplicationViewModel>();

            //list = //from i in 
            //           MyHr.TLeaveApplications
            //            .Include(c => c.CDepartment)
            //            .Include(c => c.CLeaveCategoryNavigation)
            //            .Include(c => c.CCheckStatusNavigation)
            //            .Select(c => new TLeaveApplicationViewModel
            //            {
            //                CApplyNumber = c.CApplyNumber,
            //                CDepartmentId = c.CDepartment.CDepartmentId,
            //                CDepartmentName = c.CDepartment.CDepartment,
            //                CEmployeeId = c.CEmployeeId,
            //                CApplyDate = c.CApplyDate,
            //                CLeaveCategoryId = c.CLeaveCategoryNavigation.CLeaveId,
            //                CLeaveCategory = c.CLeaveCategoryNavigation.CLeaveCategory,
            //                CLeaveStartTime = c.CLeaveStartTime,
            //                CLeaveEndTime = c.CLeaveEndTime,
            //                CReason = c.CReason,
            //                CCheckStatusId = c.CCheckStatusNavigation.CCheckStatusId,
            //                CCheckStatus = c.CCheckStatusNavigation.CCheckStatus
            //            })
            //            .Where(c => c.CEmployeeId == useraccount)
            //            .OrderByDescending(c => c.CApplyDate).ToList();

            //return View(list);
            //int UserID = int.Parse(HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID));
            //******************************************************************************************************

        }

        public int Leave_CountSpecialDayoff(int Id)  //計算特休
        {
            int dayoff = 0;

            MyHr.TUsers.ToList();

            var onBoardDate = MyHr.TUsers.Local.Where(n => n.CEmployeeId == Id && n.COnBoardStatusId == 1).Select(c => c.COnBoardDay);

            DateTime Today = DateTime.Now;
            DateTime onBoard = onBoardDate.FirstOrDefault();

            int Month = (Today.Year - onBoard.Year) * 12 + (Today.Month - onBoard.Month);

            double Year = Month / 12;

          if(Year >0.5 && Year < 1)
            {
                dayoff = 24;
            }
          else if( Year >=1 && Year < 2)
            {
                dayoff = 56;
            }
            else if (Year >= 2 && Year < 3)
            {
                dayoff = 80;
            }
            else if (Year >= 3 && Year < 5)
            {
                dayoff = 112;
            }
            else if (Year >= 5 && Year < 10)
            {
                dayoff = 120;
            }
            else if (Year >= 10 && Year < 11)
            {
                dayoff = 128;
            }
            else if (Year >= 11 && Year < 12)
            {
                dayoff = 136;
            }
            else if (Year >= 12 && Year < 13)
            {
                dayoff = 144;
            }
            else if (Year >= 13 && Year < 14)
            {
                dayoff = 152;
            }
            else if (Year >= 14 && Year < 15)
            {
                dayoff = 160;
            }

            return dayoff;
            

        }


        public List<TLeaveApplicationViewModel> LeaveCate_Count(int ID)
        {

            var table = from i in MyHr.TLeaveApplications.AsEnumerable()
                        where i.CEmployeeId == ID && DateTime.Parse(i.CApplyDate).Year== DateTime.Now.Date.Year && i.CCheckStatus == 2
                        group i by i.CLeaveCategory into g
                        select new TLeaveApplicationViewModel
                        {
                            Category = g.Key,
                            CategoryCount = g.Sum(n => n.CLeaveHours),

                        };

        
            return table.ToList();
            
        }


        public IActionResult LeaveCreate()
        {


            List<TLeave> listLeave = GetLeaveList();  //從資料庫得下拉是選單
            ViewBag.LeaveCate = listLeave;
            //TempData.Keep["leave_cate"] = "listLeave";


            ViewData[CDictionary.CURRENT_LOGINED_USERDEPARTMENTID] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERDEPARTMENTID);
            ViewData[CDictionary.CURRENT_LOGINED_USERDEPARTMENT] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERDEPARTMENT);
            ViewData[CDictionary.CURRENT_LOGINED_USERID] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID);
            ViewData[CDictionary.CURRENT_LOGINED_USERNAME] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERNAME);
            ViewData["Today"] = DateTime.Now.ToString("yyyy-MM-dd");
            int UserID = int.Parse(HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID));

            var departmentsQuery = from d in MyHr.TUserDepartments
                                   orderby d.CDepartmentId // Sort by name.
                                   select d;



            //------------------------------------------------------------------不用了

            //List<TLeaveApplicationCreateViewModel> list = new List<TLeaveApplicationCreateViewModel>();

            //TLeaveApplicationCreateViewModel obj = new TLeaveApplicationCreateViewModel()
            //{
            //    NONO = LeaveCate_Count(UserID),
            //    Leave_Specil = Leave_CountSpecialDayoff(UserID)

            //};

            //list.Add(obj);
            //ViewBag.Day = list;

            //var Rsetodtheday_Query = (from i in MyHr.TLeaveApplications
            //                          where i.CEmployeeId == int.Parse(HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID))
            //                          select new TLeaveApplicationCreateViewModel
            //                          {
            //                              NONO = LeaveCate_Count(UserID), // 計算各個假別與之請假時數
            //                              Leave_Specil = Leave_CountSpecialDayoff(UserID),//計算特休
            //                          }).ToList();


            //DepartmentNameSL = new SelectList(departmentsQuery.AsNoTracking(),
            //            "DepartmentID", "Name", selectedDepartment);

            //--------------------------------------------------------------------------------------
            return View();
        }

        [HttpPost] /*我有修改TLeaveApplicationViewModel裡面的全域變數名稱*/
        [ValidateAntiForgeryToken]
        public IActionResult LeaveCreate(TLeaveApplicationCreateViewModel T)
        {
          
            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid) //[Reurired]在CORE可以成功使用
            {
                MyHr.TLeaveApplications.Add(T.Leave);
                //await MyHr.SaveChangesAsync();
                MyHr.SaveChanges();
                return RedirectToAction("LeaveList");
            }
            else
            {

                //ModelState.AddModelError("CReason", "幹這個錯誤訊息跳出來，就算輸入資料了，也不會驗證過，不懂存在的意義");
                
                List<TLeave> listLeave = GetLeaveList();  //★我怎麼這抹巧!!!★
                ViewBag.LeaveCate = listLeave;
                return View();
            }


            //return RedirectToAction("LeaveCreate");
        }


        public IActionResult Delete(int? Id) //刪除
        {
            if (Id != null)
            {
                TLeaveApplication T = MyHr.TLeaveApplications.FirstOrDefault(i => i.CApplyNumber == Id);
                if (T != null)
                {
                    MyHr.TLeaveApplications.Remove(T);
                    MyHr.SaveChanges();
                }
            }

            return RedirectToAction("LeaveList");
        }


        public IActionResult Edit(int? Id)
        {


            List<TLeave> listLeave = GetLeaveList();  //從資料庫取假別下拉是選單
            ViewBag.LeaveCate = listLeave;

         

            ViewData[CDictionary.CURRENT_LOGINED_USERDEPARTMENTID] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERDEPARTMENTID);
            ViewData[CDictionary.CURRENT_LOGINED_USERDEPARTMENT] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERDEPARTMENT);
            ViewData[CDictionary.CURRENT_LOGINED_USERID] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID);
            ViewData[CDictionary.CURRENT_LOGINED_USERNAME] = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERNAME);
            ViewData["Today"] = DateTime.Now.ToString("yyyy-MM-dd");

            if (Id != null)
            {
                TLeaveApplication T = MyHr.TLeaveApplications.FirstOrDefault(i => i.CApplyNumber == Id);
                if (T != null)
                {
                    return View(new TLeaveApplicationEditViewModel(T));
                }
            }
            return RedirectToAction("LeaveList");
        }

        [HttpPost]
        public IActionResult Edit(TLeaveApplicationCreateViewModel T)
        {

            var errors = ModelState.Values.SelectMany(v => v.Errors);

            if (ModelState.IsValid)
            {
                if (T != null)
                {
                    TLeaveApplication _revised = MyHr.TLeaveApplications.FirstOrDefault(i => i.CApplyNumber == T.CApplyNumber);

                    if (_revised != null)
                    {
                        _revised.CApplyDate = T.CApplyDate;
                        _revised.CLeaveCategory = T.CLeaveCategory;
                        _revised.CLeaveStartTime = T.CLeaveStartTime;
                        _revised.CLeaveEndTime = T.CLeaveEndTime;
                        _revised.CReason = T.CReason;
                        _revised.CLeaveHours = T.CLeaveHours;

                        MyHr.SaveChanges();
                    }
                }

                return RedirectToAction("LeaveList");
            }
            else
            {
                List<TLeave> listLeave = GetLeaveList();
                ViewBag.LeaveCate = listLeave;
                return View();
            }
            
           
        }

        private List<TLeave> GetLeaveList() //獲取類別種類(使用在下拉是選單)
         {
            try
            {
                List<TLeave> list = new List<TLeave>();
                list = (from i in MyHr.TLeaves
                             select i).ToList();
                return list;
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return null;
            }
         }

        private List<TCheckStatus> GetStatus() //獲取審核狀態(使用在下拉是選單)
        {
            try
            {
                List<TCheckStatus> list = new List<TCheckStatus>();
                list = MyHr.TCheckStatuses.ToList();
                return list;

            }
            catch(Exception ex)
            {
                string erroe = ex.ToString();
                return null;
            }
        }


        public IActionResult Mutiple_search(int? cate, int? status, string? start, string? end)
        {
            //List<TLeaveApplication> table = MyHr.TLeaveApplications.Where(n => n.CLeaveCategory == cate).ToList();


            //string str_json = JsonConvert.SerializeObject(table, Formatting.Indented);
            //return str_json;
            //顯示JSON字串
            //li_showData.Text = str_json;
            ViewBag.UserId = int.Parse(HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERID));
            ViewBag.UserName = HttpContext.Session.GetString(CDictionary.CURRENT_LOGINED_USERNAME);


            var table = MyHr.TLeaveApplications
                .Join(MyHr.TUserDepartments, d => d.CDepartmentId, u => u.CDepartmentId, (d, u) => new
                {

                    CApplyNumber = d.CApplyNumber,
                    CEmployeeId = d.CEmployeeId,
                    //CEmployeeName = u.CEmployeeName,
                    CDepartmentId = d.CDepartmentId,
                    CDepartmentName = u.CDepartment,
                    CApplyDate = d.CApplyDate,
                    CLeaveCategory = d.CLeaveCategory,
                    CLeaveStartTime = d.CLeaveStartTime,
                    CLeaveEndTime = d.CLeaveEndTime,
                    CReason = d.CReason,
                    CCheckStatus = d.CCheckStatus,
                    CLeaveHours = d.CLeaveHours,
                    
                    
                }).OrderBy(du => du.CApplyDate).AsEnumerable().Where(du =>
                (cate != null ? du.CLeaveCategory == cate:true) &&
                (status != null ? du.CCheckStatus == status : true) &&
                (start != null ? DateTime.Parse(du.CLeaveEndTime) >= DateTime.Parse(start) : true) &&
                (end !=null? DateTime.Parse(du.CLeaveStartTime) <= DateTime.Parse(end):true) &&
                ((end !=null && start!=null)? DateTime.Parse(du.CLeaveEndTime) >= DateTime.Parse(start) && DateTime.Parse(du.CLeaveStartTime) <= DateTime.Parse(end) : true)
                ).ToList();

            //int abc = table.Count;   ■ 這邊搜尋的資料並未針對USER，當沒有資料想要在前端做變化時，應當加入此條件。
            List<TLeaveApplicationViewModel> T = new List<TLeaveApplicationViewModel>();
           
            foreach (var item in table)
            {
                TLeaveApplicationViewModel obj = new TLeaveApplicationViewModel()
                {
                    CApplyNumber = item.CApplyNumber,
                    CEmployeeId = item.CEmployeeId,
                    CDepartmentId = item.CDepartmentId,
                    CDepartmentName = item.CDepartmentName,
                    CApplyDate = item.CApplyDate,
                    CLeaveCategory = item.CLeaveCategory,
                    CLeaveStartTime = item.CLeaveStartTime,
                    CLeaveEndTime = item.CLeaveEndTime,
                    CReason = item.CReason,
                    CCheckStatus = item.CCheckStatus,
                    CLeaveHours = item.CLeaveHours

                };
                T.Add(obj);
               
            }


            //foreach (TLeaveApplication C in table)                
            //    T.Add(new TLeaveApplicationViewModel(C,null));


           
            return PartialView("Mutiple_search", T);


        }


        public JsonResult newLeaveApply()
        {

            //int[] memberBox = new int[]
            //{
            //    2,16
            //};
            int[] categoryBox = new int[]
            {
               1,2,3,4,5,1
            };
            string[] dateBox = new string[]
            {

                "2021-05-01T10:15",
                "2021-05-01T07:20",
                "2021-05-01T09:38",
                "2021-09-01T08:50",
                "2021-09-01T10:20",
                "2021-09-02T11:40",
                "2021-11-06T16:15",
                "2021-11-04T10:26",
                "2021-11-01T08:33",
                "2021-05-01T10:45",
                "2021-09-15T09:56",
                "2021-04-01T12:44",
                "2021-10-01T14:20",
                "2021-11-01T07:19",
                "2021-05-01T09:44",
            };

            string[] reasonBox = new string[]
            {
                "隨機測試1",
                "隨機測試2",
                "隨機測試3",
                "隨機測試4",
                "隨機測試5",

            };

            int[] hoursBox = new int[]
            {
              2,3,4,5,2
            };

            Random random = new Random();
            var LeaveBox = MyHr.TLeaveApplications.ToList();

            for (int i = 0; i < 15; i++)
            {
                TLeaveApplication order = new TLeaveApplication()
                {
                   CDepartmentId = 2,
                   CEmployeeId = 17,
                   CApplyDate = dateBox[random.Next(0, dateBox.Count())],
                   CLeaveCategory= categoryBox[random.Next(0, 5)],
                   CLeaveStartTime = dateBox[random.Next(0, dateBox.Count())],
                   CLeaveEndTime = dateBox[random.Next(0, dateBox.Count())],
                   CReason = reasonBox[random.Next(0,4)],
                   CLeaveHours = hoursBox[random.Next(0, 4)],
                   CCheckStatus =1,

                };
                MyHr.TLeaveApplications.Add(order);
              
            }
            MyHr.SaveChanges();

            return Json("done");
        }


    }
}