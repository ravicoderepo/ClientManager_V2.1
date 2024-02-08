﻿using ClientManager.Models;
using DBOperation;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace ClientManager.Controllers
{
    public class AccountController : Controller
    {
        private ClientManagerEntities db = new ClientManagerEntities();

        public ActionResult SiteUnderConstruction()
        {
            return View();
        }

        public ActionResult Index() => (ActionResult)this.RedirectToAction("Login");

        public ActionResult Login() => (ActionResult)this.View();

        public ActionResult Logout()
        {
            this.Session["UserDetails"] = (object)null;
            this.Session.Abandon();
            return (ActionResult)this.RedirectToAction("Login");
        }

        public ActionResult ForgotPassword() => (ActionResult)this.View();

        [HttpGet]
        public ActionResult ChangePassword() => (ActionResult)this.View(new ChangePassword());

        [HttpPost]
        public ActionResult ChangePassword(ChangePassword changePassword)
        {
            JsonReponse data = new JsonReponse();
            try
            {
                if (string.IsNullOrEmpty(changePassword.Email) || string.IsNullOrEmpty(changePassword.OldPassword) || string.IsNullOrEmpty(changePassword.NewPassword))
                {
                    data = new JsonReponse()
                    {
                        message = "All fields are required.",
                        status = "Failed",
                        redirectURL = ""
                    };
                }
                else
                {
                    User userData = this.db.Users.FirstOrDefault(wh => wh.Email == changePassword.Email & wh.Password == changePassword.OldPassword & wh.IsActive == true);
                    if (userData == null)
                    {
                        data = new JsonReponse()
                        {
                            message = "Email and Old Password is not mached.",
                            status = "Failed",
                            redirectURL = ""
                        };
                    }
                    else
                    {

                        this.db.Entry<DBOperation.User>(userData).State = EntityState.Modified;
                        string str = String.Empty;

                        userData.Password = changePassword.NewPassword;
                        userData.ModifiedBy = 1;
                        userData.ModifiedOn = new DateTime?(DateTime.Now);

                        if (this.db.SaveChanges() > 0)
                        {
                            data = new JsonReponse()
                            {
                                message = "Password changed successfully.",
                                status = "Success",
                                redirectURL = "/Account/Login"
                            };
                        }
                        else
                        {
                            data = new JsonReponse()
                            {
                                message = "Unable to process data, please try again later.",
                                status = "Error",
                                redirectURL = ""
                            };
                        }
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

        [HttpPost]
        public ActionResult SignIn(ClientManager.Models.Login userLogin)
        {
            JsonReponse data = (JsonReponse)null;
            try
            {
                if (string.IsNullOrEmpty(userLogin.Email) || string.IsNullOrEmpty(userLogin.Password))
                    data = new JsonReponse()
                    {
                        message = "User name and Password is required.",
                        status = "Failed",
                        redirectURL = ""
                    };
                else if (!string.IsNullOrEmpty(userLogin.Email))
                {
                    if (!string.IsNullOrEmpty(userLogin.Password))
                    {
                        User userData = this.db.Users.FirstOrDefault<User>((Expression<Func<User, bool>>)(wh => wh.Email == userLogin.Email & wh.Password == userLogin.Password & wh.IsActive == (bool?)true));
                        if (userData == null)
                        {
                            data = new JsonReponse()
                            {
                                message = "Invalid username/password.",
                                status = "Failed",
                                redirectURL = ""
                            };
                        }
                        else
                        {
                            UserDetails userDetails1 = new UserDetails();
                            userDetails1.Id = userData.Id;
                            userDetails1.Email = userData.Email;
                            userDetails1.FullName = userData.FullName;
                            userDetails1.IsActive = userData.IsActive;
                            userDetails1.CreatedBy = userData.CreatedBy;
                            userDetails1.CreatedOn = userData.CreatedOn;
                            userDetails1.ModifiedOn = userData.ModifiedOn;
                            userDetails1.ModifiedBy = userData.ModifiedBy;
                            userDetails1.ReportingManager = userData.ReportingManager;
                            userDetails1.UserRoles = userData.UserRoles1.Where<DBOperation.UserRole>((Func<DBOperation.UserRole, bool>)(wh => wh.UserId == userData.Id)).Select<DBOperation.UserRole, ClientManager.Models.UserRole>((Func<DBOperation.UserRole, ClientManager.Models.UserRole>)(sel => new ClientManager.Models.UserRole()
                            {
                                Id = sel.Id,
                                RoleId = sel.RoleId,
                                RoleName = sel.Role.RoleName
                            })).ToList<ClientManager.Models.UserRole>();
                            userDetails1.ReportingToMe = this.db.Users.Where<User>((Expression<Func<User, bool>>)(wh => wh.ReportingManager == (int?)userData.Id)).Select<User, int>((Expression<Func<User, int>>)(sel => sel.Id)).ToList<int>();
                            UserDetails userDetails2 = userDetails1;
                            this.Session["UserDetails"] = (object)userDetails2;
                            data = new JsonReponse()
                            {
                                message = "Valid Credentials",
                                status = "Success",
                                redirectURL = (userDetails2.UserRoles.Any(a => a.RoleName == "Store Admin") || userDetails2.UserRoles.Any(a => a.RoleName == "Accounts Manager")) ? "/Home/FinanceDashboard" : "/Home/MyDashboard"
                            };
                        }
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

        public ActionResult Register() => (ActionResult)this.View();

        [HttpPost]
        public ActionResult UserRegister(ClientManager.Models.Register userRegister)
        {
            JsonReponse data = (JsonReponse)null;
            try
            {
                if (string.IsNullOrEmpty(userRegister.Email) || string.IsNullOrEmpty(userRegister.FullName) || string.IsNullOrEmpty(userRegister.Password))
                    data = new JsonReponse()
                    {
                        message = "User name and Password is required.",
                        status = "Failed",
                        redirectURL = ""
                    };
                else if (!string.IsNullOrEmpty(userRegister.Email))
                {
                    if (!string.IsNullOrEmpty(userRegister.FullName))
                    {
                        if (!string.IsNullOrEmpty(userRegister.Password))
                        {
                            if (this.db.Users.Any<User>((Expression<Func<User, bool>>)(wh => wh.Email == userRegister.Email)))
                            {
                                data = new JsonReponse()
                                {
                                    message = "This Email Id already already registered.",
                                    status = "Failed",
                                    redirectURL = ""
                                };
                            }
                            else
                            {
                                this.db.Users.Add(new User()
                                {
                                    FullName = userRegister.FullName,
                                    Email = userRegister.Email,
                                    Password = userRegister.Password,
                                    IsActive = new bool?(true),
                                    CreatedBy = new int?(1),
                                    CreatedOn = new DateTime?(DateTime.Now)
                                });
                                if (this.db.SaveChanges() > 0)
                                    data = new JsonReponse()
                                    {
                                        message = "User registration completed successfully!",
                                        status = "Success",
                                        redirectURL = "/Account/Login"
                                    };
                                else
                                    data = new JsonReponse()
                                    {
                                        message = "User registration not completed, try again after sometime.",
                                        status = "Failed",
                                        redirectURL = ""
                                    };
                            }
                        }
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
    }
}