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
    public class MaterialsController : Controller
    {
        private ClientManagerEntities db = new ClientManagerEntities();

        // GET: ExpenceCategories
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult List()
        {
            var Materials = db.Materials;
            return View(Materials.ToList());
        }

        // GET: ExpenceCategories/Create
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Create()
        {
            ViewBag.Status = new SelectList(Utility.DefaultList.GetStatusList(), "Value", "Text", 1).ToList<SelectListItem>();
            return View();
        }

        // POST: ExpenceCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Create(Models.MaterialData materialData)
        {
            UserDetails userData = (UserDetails)this.Session["UserDetails"];
            JsonReponse data;
            try
            {
                int num = 0;
                if (string.IsNullOrEmpty(materialData.MaterialName) || string.IsNullOrEmpty(materialData.Description))
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
                    this.db.Materials.Add(new DBOperation.Material()
                    {
                        MaterialName = materialData.MaterialName,
                        Description = materialData.Description,
                        IsActive = materialData.IsActive,
                        CreatedBy = userData.Id,
                        CreatedOn = DateTime.Now
                    });
                    num = this.db.SaveChanges();

                    if (num > 0)
                        data = new JsonReponse()
                        {
                            message = "Material created successfully!",
                            status = "Success",
                            redirectURL = "/Materials/List"
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

        // GET: ExpenceCategories/Edit/5
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DBOperation.Material meterial = db.Materials.Find(id);
            if (meterial == null)
            {
                return HttpNotFound();
            }

           
            ViewBag.Status = new SelectList(Utility.DefaultList.GetStatusList(), "Value", "Text", meterial.IsActive ? 1 : 0).ToList<SelectListItem>();
            return View(meterial);
        }

        // POST: ExpenceCategories/Edit/5
        [HttpPost]
        [CustomAuthorize(new string[] { "Super Admin", "Super User", "Store Admin" })]
        public ActionResult Edit(Models.MaterialData materialData)
        {
            JsonReponse data;
            try
            {
                UserDetails userDetails = (UserDetails)this.Session["UserDetails"];
                DBOperation.Material entity = this.db.Materials.FirstOrDefault(wh => wh.MaterialId == materialData.MaterialId);
                if (entity == null)
                    data = new JsonReponse()
                    {
                        message = "There is no record for given Id",
                        status = "Failed",
                        redirectURL = ""
                    };
                else if (string.IsNullOrEmpty(materialData.MaterialName) || string.IsNullOrEmpty(materialData.Description))
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
                    this.db.Entry<DBOperation.Material>(entity).State = EntityState.Modified;
                    string str = String.Empty;
                    if (userDetails.UserRoles.Any<ClientManager.Models.UserRole>((Func<ClientManager.Models.UserRole, bool>)(wh => wh.RoleName.ToLower() == "super admin")))
                    {
                        entity.MaterialName = materialData.MaterialName;
                        entity.Description = materialData.Description;
                        entity.IsActive = materialData.IsActive;                       
                        str = "Material details Updated";
                    }
                   
                    entity.ModifiedBy = new int?(userDetails.Id);
                    entity.ModifiedOn = new DateTime?(DateTime.Now);

                    if (this.db.SaveChanges() > 0)
                        data = new JsonReponse()
                        {
                            message = "Material details saved successfully!",
                            status = "Success",
                            redirectURL = "/Materials/List"
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

                DBOperation.Material entity = this.db.Materials.FirstOrDefault(wh => wh.MaterialId == id);


                if (entity == null)
                {
                    data = new JsonReponse()
                    {
                        message = "There is no record for given Id",
                        status = "Failed",
                        redirectURL = "/Materials/List"
                    };
                }
                else
                {
                    this.db.Entry<DBOperation.Material>(entity).State = EntityState.Modified;
                    entity.IsActive = true;

                    if (this.db.SaveChanges() > 0)
                    {
                        data = new JsonReponse()
                        {
                            message = "Activated Successfully!",
                            status = "Success",
                            redirectURL = "/Materials/List"
                        };
                    }
                    else
                    {
                        data = new JsonReponse()
                        {
                            message = "Failed to update!",
                            status = "Error",
                            redirectURL = "/Materials/List"
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
                    redirectURL = "/Materials/List"
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

                DBOperation.Material entity = this.db.Materials.FirstOrDefault(wh => wh.MaterialId == id);


                if (entity == null)
                {
                    data = new JsonReponse()
                    {
                        message = "There is no record for given Id",
                        status = "Failed",
                        redirectURL = "/Materials/List"
                    };
                }
                else
                {
                    this.db.Entry<DBOperation.Material>(entity).State = EntityState.Modified;
                    entity.IsActive = false;

                    if (this.db.SaveChanges() > 0)
                    {
                        data = new JsonReponse()
                        {
                            message = "De-Activated Successfully!",
                            status = "Success",
                            redirectURL = "/Materials/List"
                        };
                    }
                    else
                    {
                        data = new JsonReponse()
                        {
                            message = "Failed to update!",
                            status = "Error",
                            redirectURL = "/Materials/List"
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