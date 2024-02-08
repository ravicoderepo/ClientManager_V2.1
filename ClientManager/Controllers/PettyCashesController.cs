﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClientManager.Infrastructure;
using ClientManager.Models;
using DBOperation;

namespace ClientManager.Controllers
{
    public class PettyCashesController : Controller
    {
        private ClientManagerEntities db = new ClientManagerEntities();

        // GET: PettyCashes
        [CustomAuthorize(new string[] { "Super Admin", "Super User","Store Admin", "Accounts Manager" })]
        public ActionResult List()
        {
            var pettyCashes = db.PettyCashes.Include(p => p.User).Include(p => p.User1);
            UserDetails userData = (UserDetails)this.Session["UserDetails"];
            // ViewBag.UserRoles = userData.UserRoles.Select(sel => sel.RoleName);

            ViewBag.ModeOfPayment = new SelectList(Utility.DefaultList.GetPaymentModeList(), "Value", "Text", (object)1).ToList<SelectListItem>();
            return View(pettyCashes.OrderByDescending(ord => ord.AmountRecivedDate).ToList());

        }

        // GET: PettyCashes
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin", "Accounts Manager" })]
        public ActionResult ListView(string AmountReceivedDateFrom = "", string AmountReceivedDateTo = "", string ModeOfPayment = "")
        {
            DateTime dtAmountRecivedDateFrom = new DateTime();
            DateTime dtAmountRecivedDateTo = new DateTime();

            var pettyCashes = db.PettyCashes.Include(p => p.User).Include(p => p.User1);
            UserDetails userData = (UserDetails)this.Session["UserDetails"];

            if (!string.IsNullOrEmpty(AmountReceivedDateFrom))
            {
                dtAmountRecivedDateFrom = DateTime.Parse(AmountReceivedDateFrom);
                pettyCashes = pettyCashes.Where(wh => wh.AmountRecivedDate >= dtAmountRecivedDateFrom);
            }

            if (!string.IsNullOrEmpty(AmountReceivedDateTo))
            {
                dtAmountRecivedDateTo = DateTime.Parse(AmountReceivedDateTo);
                pettyCashes = pettyCashes.Where(wh => wh.AmountRecivedDate <= dtAmountRecivedDateTo);
            }
            if (!string.IsNullOrEmpty(ModeOfPayment))
                pettyCashes = pettyCashes.Where(wh => wh.ModeOfPayment == ModeOfPayment);

            // ViewBag.UserRoles = userData.UserRoles.Select(sel => sel.RoleName);
            ViewBag.ModeOfPayment = new SelectList(Utility.DefaultList.GetPaymentModeList(), "Value", "Text", (object)1).ToList<SelectListItem>();

            return PartialView(pettyCashes.OrderByDescending(ord => ord.AmountRecivedDate).ToList());
        }

        // GET: PettyCashes/Create
        [CustomAuthorize(new string[] { "Super Admin", "Store Admin" })]
        public ActionResult Create()
        {
            ViewBag.ModeOfPayment = new SelectList(Utility.DefaultList.GetPaymentModeList(), "Value", "Text", (object)1).ToList<SelectListItem>();
            return View();
        }

        // POST: ExpenceCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(new string[] { "Super Admin", "Store Admin" })]
        public ActionResult Create(Models.PettyCashData PettyCashData)
        {
            UserDetails userData = (UserDetails)this.Session["UserDetails"];
            JsonReponse jsonReponse = (JsonReponse)null;
            string validationMessage = string.Empty;
            JsonReponse data;
            try
            {
                int num = 0;
                if (PettyCashData.AmountReceived <= 0 || PettyCashData.AmountRecivedDate == null || string.IsNullOrEmpty(PettyCashData.Description))
                {
                    jsonReponse = new JsonReponse()
                    {
                        message = "Enter all required fields.",
                        status = "Failed",
                        redirectURL = ""
                    };
                }
                else if (Convert.ToDateTime(PettyCashData.AmountRecivedDate.Date) > DateTime.Now.Date)
                {
                    //data = new JsonReponse()
                    //{
                    //    message = "Amount recived date should be lesser than or equal to today.",
                    //    status = "Failed",
                    //    redirectURL = ""
                    //};
                    validationMessage = "Amount recived date should be lesser than or equal to today.";
                }
                else
                {
                    this.db.PettyCashes.Add(new DBOperation.PettyCash()
                    {
                        AmountReceived = PettyCashData.AmountReceived,
                        AmountRecivedDate = PettyCashData.AmountRecivedDate,
                        ModeOfPayment = PettyCashData.ModeOfPayment,
                        Status = "",
                        Description = PettyCashData.Description,
                        CreatedBy = userData.Id,
                        CreatedOn = DateTime.Now
                    });
                    num = this.db.SaveChanges();

                    var expenceTracker = db.ExpenseTrackers.Include(p => p.User).Include(p => p.User1);
                    var TotalPettyCashAmount = db.PettyCashes.Where(wh => wh.AmountRecivedDate.Month == DateTime.Now.Month && wh.AmountRecivedDate.Year == DateTime.Now.Year).ToList();
                    var TotalApprovedExpenceAmount = expenceTracker.Where(wh => wh.ExpenseDate.Month == DateTime.Now.Month && wh.ExpenseDate.Year == DateTime.Now.Year && wh.Status == "Verified").ToList();
                    decimal? TotalPettyCash = (TotalPettyCashAmount != null && TotalPettyCashAmount.Count > 0) ? TotalPettyCashAmount.Sum(S => S.AmountReceived) : 0;
                    decimal? TotalApprovedExpence = (TotalApprovedExpenceAmount != null && TotalApprovedExpenceAmount.Count > 0) ? TotalApprovedExpenceAmount.Sum(s => s.ExpenseAmount) : 0;
                    var PendingPettyCash = (TotalPettyCash.Value - TotalApprovedExpence.Value).ToString("#,##,##0.00");

                    string EmailBody = Utility.Emails.GetEmailTemplate("PettyCashAdded").Replace("{PettyCashValue}", PettyCashData.AmountReceived.ToString()).Replace("{PaymentMode}", PettyCashData.ModeOfPayment).Replace("{AmountReceivedDate}", PettyCashData.AmountRecivedDate.ToShortDateString()).Replace("{PendingPettyCash}", PendingPettyCash).Replace("{Description}", (!string.IsNullOrEmpty(PettyCashData.Description) ? PettyCashData.Description : "N/A"));
                    Utility.Emails.SendEmail(Utility.ConfigSettings.ReadSetting("FinanceEmailIdTo"), Utility.ConfigSettings.ReadSetting("FinanceEmailIdCC"), "Petty Cash Added", EmailBody);
                }
                if (num > 0)
                    data = new JsonReponse()
                    {
                        message = "Petty Cash entry created successfully!",
                        status = "Success",
                        redirectURL = "/PettyCashes/List"
                    };
                else
                    data = new JsonReponse()
                    {
                        message = (string.IsNullOrEmpty(validationMessage)) ? "Petty Cash entry not completed, try again after sometime." : validationMessage,
                        status = "Failed",
                        redirectURL = ""
                    };
            }
            catch (Exception ex)
            {
                data = new JsonReponse()
                {
                    message = ex.Message,
                    status = "Error",
                    redirectURL = ""
                };
            }
            return (ActionResult)this.Json((object)data, JsonRequestBehavior.AllowGet);
        }

        // GET: ExpenceCategories/Edit/5
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin", "Accounts Manager" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DBOperation.PettyCash pettyCash = db.PettyCashes.Find(id);
            if (pettyCash == null)
            {
                return HttpNotFound();
            }

            ViewBag.ModeOfPayment = new SelectList(Utility.DefaultList.GetPaymentModeList(), "Value", "Text", pettyCash.ModeOfPayment).ToList<SelectListItem>();
            return View(pettyCash);
        }

        // POST: ExpenceCategories/Edit/5
        [HttpPost]
        [CustomAuthorize(new string[] { "Super Admin", "Store Admin" })]
        public ActionResult Edit(Models.PettyCashData PettyCashData)
        {
            JsonReponse data;
            try
            {
                UserDetails userDetails = (UserDetails)this.Session["UserDetails"];
                DBOperation.PettyCash entity = this.db.PettyCashes.FirstOrDefault(wh => wh.Id == PettyCashData.Id);
                if (entity == null)
                {
                    data = new JsonReponse()
                    {
                        message = "There is no record for given Id",
                        status = "Failed",
                        redirectURL = ""
                    };
                }
                else if (PettyCashData.AmountReceived <= 0 || PettyCashData.AmountRecivedDate == null || string.IsNullOrEmpty(PettyCashData.Description))
                {
                    data = new JsonReponse()
                    {
                        message = "Enter all required fields.",
                        status = "Failed",
                        redirectURL = ""
                    };
                }
                else if (Convert.ToDateTime(PettyCashData.AmountRecivedDate.Date) > DateTime.Now.Date)
                {
                    data = new JsonReponse()
                    {
                        message = "Amount recived date should be lesser than or equal to today.",
                        status = "Failed",
                        redirectURL = ""
                    };
                }
                else
                {
                    this.db.Entry<DBOperation.PettyCash>(entity).State = EntityState.Modified;

                    entity.AmountReceived = PettyCashData.AmountReceived;
                    entity.AmountRecivedDate = PettyCashData.AmountRecivedDate;
                    entity.ModeOfPayment = PettyCashData.ModeOfPayment;
                    entity.Status = "";
                    entity.ModifiedBy = new int?(userDetails.Id);
                    entity.ModifiedOn = new DateTime?(DateTime.Now);
                    entity.Description = PettyCashData?.Description;
                    if (this.db.SaveChanges() > 0)
                        data = new JsonReponse()
                        {
                            message = "Petty Cash updated successfully!",
                            status = "Success",
                            redirectURL = "/PettyCashes/List"
                        };
                    else
                        data = new JsonReponse()
                        {
                            message = "Petty Cash update not completed, try again after sometime.",
                            status = "Failed",
                            redirectURL = ""
                        };
                }
            }
            catch (Exception ex)
            {
                data = new JsonReponse()
                {
                    message = ex.Message,
                    status = "Error",
                    redirectURL = ""
                };
            }
            return (ActionResult)this.Json((object)data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(new string[] { "Store Admin" })]
        public ActionResult Delete(int id)
        {
            UserDetails userDetails = (UserDetails)this.Session["UserDetails"];

            JsonReponse data;
            try
            {
                if (id <= 0)
                    return (ActionResult)new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                PettyCash entity = this.db.PettyCashes.Find(id);

                if (entity == null)
                    return (ActionResult)this.HttpNotFound();

                this.db.PettyCashes.Remove(entity);
                this.db.SaveChanges();

                data = new JsonReponse()
                {
                    message = "PettyCash Entry Deleted.",
                    status = "Success",
                    redirectURL = "/PettyCashes/List?" + DateTime.Now.Ticks.ToString()
                };
            }
            catch (Exception ex)
            {
                data = new JsonReponse()
                {
                    message = ex.Message,
                    status = "Error",
                    redirectURL = ""
                };
            }
            return (ActionResult)this.Json((object)data, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
