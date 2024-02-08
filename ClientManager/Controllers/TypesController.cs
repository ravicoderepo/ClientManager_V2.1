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
    public class TypesController : Controller
    {
        private ClientManagerEntities db = new ClientManagerEntities();

        // GET: ExpenceCategories
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult List()
        {
            var types = db.Types;
            return View(types.ToList());
        }

        // GET: ExpenceCategories/Create
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Create()
        {
            ViewBag.MaterialId = new SelectList(db.Materials, "MaterialId", "MaterialName", 1).ToList<SelectListItem>();
            ViewBag.Status = new SelectList(Utility.DefaultList.GetStatusList(), "Value", "Text", 1).ToList<SelectListItem>();
            return View();
        }

        // POST: ExpenceCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Create(Models.TypesData typesData)
        {
            UserDetails userData = (UserDetails)this.Session["UserDetails"];
            
            JsonReponse data;
            try
            {
                int num = 0;
                if (string.IsNullOrEmpty(typesData.TypeName) || string.IsNullOrEmpty(typesData.Description))
                {
                    data = new JsonReponse()
                    {
                        message = "Enter all required fields.",
                        status = "Failed",
                        redirectURL = ""
                    };
                }
                else
                {
                    this.db.Types.Add(new DBOperation.Type()
                    {
                        MaterialId = typesData.MaterialId,
                        TypeName = typesData.TypeName,
                        Description = typesData.Description,
                        IsActive = typesData.IsActive,
                        CreatedBy = userData.Id,
                        CreatedOn = DateTime.Now
                    });
                    num = this.db.SaveChanges();
                    if (num > 0)
                        data = new JsonReponse()
                        {
                            message = "Product Master created successfully!",
                            status = "Success",
                            redirectURL = "/Types/List"
                        };
                    else
                        data = new JsonReponse()
                        {
                            message = "Product Master creation not completed, try again after sometime.",
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

        // GET: ExpenceCategories/Edit/5
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DBOperation.Type type = db.Types.Find(id);
            if (type == null)
            {
                return HttpNotFound();
            }

            ViewBag.MaterialId = new SelectList(db.Materials, "MaterialId", "MaterialName", type.MaterialId).ToList<SelectListItem>();
            ViewBag.Status = new SelectList(Utility.DefaultList.GetStatusList(), "Value", "Text", type.IsActive?1:0).ToList<SelectListItem>();
            return View(type);
        }

        // POST: ExpenceCategories/Edit/5
        [HttpPost]
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Edit(Models.TypesData typesData)
        {
            JsonReponse data;
            try
            {
                UserDetails userDetails = (UserDetails)this.Session["UserDetails"];
                DBOperation.Type entity = this.db.Types.FirstOrDefault(wh => wh.TypeId == typesData.TypeId);
                if (entity == null)
                    data = new JsonReponse()
                    {
                        message = "There is no record for given Id",
                        status = "Failed",
                        redirectURL = ""
                    };
                else if (string.IsNullOrEmpty(typesData.TypeName) || string.IsNullOrEmpty(typesData.Description))
                {
                    data = new JsonReponse()
                    {
                        message = "Enter all required fields.",
                        status = "Failed",
                        redirectURL = ""
                    };
                }
                else
                {
                    this.db.Entry<DBOperation.Type>(entity).State = EntityState.Modified;
                    string str = String.Empty;
                    //if (userDetails.UserRoles.Any<ClientManager.Models.UserRole>((Func<ClientManager.Models.UserRole, bool>)(wh => wh.RoleName.ToLower() == "super admin")))
                    //{
                    entity.MaterialId = typesData.MaterialId;
                        entity.TypeName = typesData.TypeName;
                        entity.Description = typesData.Description;
                        entity.IsActive = typesData.IsActive;                       
                        str = "Type details Updated";
                    //}
                   
                    entity.ModifiedBy = new int?(userDetails.Id);
                    entity.ModifiedOn = new DateTime?(DateTime.Now);

                    if (this.db.SaveChanges() > 0)
                        data = new JsonReponse()
                        {
                            message = "Product Master details updated successfully!",
                            status = "Success",
                            redirectURL = "/Types/List"
                        };
                    else
                        data = new JsonReponse()
                        {
                            message = "Not completed, try again after sometime.",
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
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Activate(int? id)
        {
            JsonReponse data;
            try
            {

                DBOperation.Type entity = this.db.Types.FirstOrDefault(wh => wh.TypeId == id);
                UserDetails userDetails = (UserDetails)this.Session["UserDetails"];

                if (entity == null)
                {
                    data = new JsonReponse()
                    {
                        message = "There is no record for given Id",
                        status = "Failed",
                        redirectURL = "/Types/List"
                    };
                }
                else
                {
                    this.db.Entry<DBOperation.Type>(entity).State = EntityState.Modified;
                    entity.IsActive = true;
                    entity.ModifiedBy = new int?(userDetails.Id);
                    entity.ModifiedOn = new DateTime?(DateTime.Now);

                    if (this.db.SaveChanges() > 0)
                    {
                        data = new JsonReponse()
                        {
                            message = "Activated Successfully!",
                            status = "Success",
                            redirectURL = "/Types/List"
                        };
                    }
                    else
                    {
                        data = new JsonReponse()
                        {
                            message = "Failed to update!",
                            status = "Error",
                            redirectURL = "/Types/List"
                        };
                    }

                }
            }
            catch (Exception ex)
            {
                data = new JsonReponse()
                {
                    message = ex.Message,
                    status = "Error",
                    redirectURL = "/Types/List"
                };
            }
            return (ActionResult)this.Json((object)data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult DeActivate(int? id)
        {
            JsonReponse data;
            try
            {

                DBOperation.Type entity = this.db.Types.FirstOrDefault(wh => wh.TypeId == id);
                UserDetails userDetails = (UserDetails)this.Session["UserDetails"];

                if (entity == null)
                {
                    data = new JsonReponse()
                    {
                        message = "There is no record for given Id",
                        status = "Failed",
                        redirectURL = "/Types/List"
                    };
                }
                else
                {
                    this.db.Entry<DBOperation.Type>(entity).State = EntityState.Modified;
                    entity.IsActive = false;
                    entity.ModifiedBy = new int?(userDetails.Id);
                    entity.ModifiedOn = new DateTime?(DateTime.Now);

                    if (this.db.SaveChanges() > 0)
                    {
                        data = new JsonReponse()
                        {
                            message = "De-Activated Successfully!",
                            status = "Success",
                            redirectURL = "/Types/List"
                        };
                    }
                    else
                    {
                        data = new JsonReponse()
                        {
                            message = "Failed to update!",
                            status = "Error",
                            redirectURL = "/Types/List"
                        };
                    }

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
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult GetProductMaster(int materialId = 1)
        {
            var list = new SelectList(db.Types.Where(wh => wh.MaterialId == materialId), "TypeId", "TypeName", 0).ToList<SelectListItem>();
            list.Insert(0, new SelectListItem { Text = "Select", Value = "", Selected = true });
            return Json(list.ToList(), JsonRequestBehavior.AllowGet);

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
