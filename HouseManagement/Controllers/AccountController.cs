using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;

namespace HouseManagement.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginAsync(LoginUserModel model, string returnUrl)
        {
            var resulLogin = await ApiRequests.LoginToWebApiAsync(model.email, model.password);
            if (resulLogin.StatusCode == (int)HttpStatusCode.OK && resulLogin.Item as string != null)
            {
                var token = resulLogin.Item as string;
                var resultUser = await ApiGetRequests.LoadUserFromAPIAsync(token, Urles.UserUrl + $"/username/{model.email}");
                UserModel? user = resultUser.Item as UserModel;
                if (resultUser.StatusCode == 200 && user != null)
                {
                    var claims = new List<Claim>();
                    if (user.role == "FlatOwner")
                    {
                        var resultFlats = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/flatowner/{user.idFlatOwner}");
                        List<Flat>? flats = resultFlats.Item as List<Flat>;
                        if (resultFlats.StatusCode == 200 && flats != null)
                        {
                            claims.Add(new Claim(ClaimTypes.Name, user.email));
                            claims.Add(new Claim(ClaimTypes.Role, user.role));
                            claims.Add(new Claim("idUser", user.id));
                            claims.Add(new Claim("token", token));
                            claims.Add(new Claim("idFlatOwner", user.idFlatOwner.ToString()));
                            claims.Add(new Claim("countFlats", flats.Count.ToString()));
                            for (int i = 0; i < flats.Count; i++)
                            {
                                claims.Add(new Claim("idFlat" + i.ToString(), flats[i].id.ToString()));
                            }
                            claims.Add(new Claim("countHouses", user.idHouses.Count().ToString()));
                            claims.Add(new Claim("idHouseFlatOwner", user.idHouses[0].ToString()));
                        }
                        else Content("Невозможно получить данные");
                    }
                    if (user.role == "HouseAdmin")
                    {
                        claims.Add(new Claim(ClaimTypes.Name, user.email, ClaimValueTypes.String));
                        claims.Add(new Claim(ClaimTypes.Role, user.role, ClaimValueTypes.String));
                        claims.Add(new Claim("idUser", user.id));
                        claims.Add(new Claim("token", token));
                        claims.Add(new Claim("countHouses", user.idHouses.Count().ToString()));
                        for (int i = 0; i < user.idHouses.Count(); i++)
                        {
                            claims.Add(new Claim("idHouse" + i.ToString(), user.idHouses[i].ToString()));
                        }
                    }
                    if (user.role == "MainAdmin")
                    {
                        claims.Add(new Claim(ClaimTypes.Name, user.email));
                        claims.Add(new Claim(ClaimTypes.Role, user.role));
                        claims.Add(new Claim("idUser", user.id));
                        claims.Add(new Claim("token", token));
                    }

                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                    identity.AddClaims(claims);
                    var principal = new ClaimsPrincipal(identity);
                    var authProperties = new AuthenticationProperties
                    {
                        AllowRefresh = true,
                        ExpiresUtc = DateTimeOffset.Now.AddDays(30),
                        IsPersistent = true,
                    };
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(principal), authProperties);

                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Account");
                    }
                }
                return Content("Невозможно получить данные");
            }
            else if (resulLogin.StatusCode == 401)
            {
                ViewBag.Message = "Неверный логин или пароль";
                return View();
            }
            else
            {
                return new NotFoundResult();
            }
        }
        public ActionResult AccessDenied()
        {
            return LocalRedirect("/Account/Login");
        }
        public ActionResult Logout()
        {
            return LocalRedirect("/logout");
        }
        [Authorize]
        public async Task<ActionResult> IndexAsync()
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var id = UserProperties.GetIdUser(ref context);
            if (id != null)
            {
                var resultUser = await ApiGetRequests.LoadUseByIdFromAPIAsync(token, Urles.UserUrl + $"/{id}");
                var user = resultUser.Item as UserModel;
                if (resultUser.StatusCode == 200 && user != null)
                {
                    if (user.role == "FlatOwner")
                    {
                        var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{user.idFlatOwner}");
                        var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/flatowner/{user.idFlatOwner}");
                        FlatOwner? flatOwner = resultFlatOwner.Item as FlatOwner;
                        List<Flat>? flats = resultFlat.Item as List<Flat>;
                        if (flatOwner != null && user != null && flats != null)
                        {
                            ViewBag.Id = user.id;
                            ViewBag.FullName = flatOwner.fullName;
                            ViewBag.Flats = flats;
                            return View(user);
                        }
                    }
                    if (user.role == "HouseAdmin" || user.role == "MainAdmin")
                    {
                        ViewBag.Id = user.id;
                        return View(user);
                    }
                }
            }
            return new NotFoundResult();
        }
        [Authorize]
        public async Task<ActionResult> EditLoginAsync(string? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultUser = await ApiGetRequests.LoadUseByIdFromAPIAsync(token, Urles.UserUrl + $"/{id}");
                var user = resultUser.Item as UserModel;
                if (resultUser.StatusCode == 200 && user != null)
                {
                    user.password = "";
                    ViewBag.Id = user.id;
                    return View("EditLogin", user);
                }
            }
            return new NotFoundResult();
        }
        [Authorize]
        public async Task<ActionResult> EditPasswordAsync(string? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultUser = await ApiGetRequests.LoadUseByIdFromAPIAsync(token, Urles.UserUrl + $"/{id}");
                var user = resultUser.Item as UserModel;
                if (resultUser.StatusCode == 200 && user != null)
                {
                    user.password = "";
                    ViewBag.Id = user.id;
                    return View(user);
                }
            }
            return new NotFoundResult();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditLoginAsync([Bind("email, password, idHouse, idFlatOwner, role")] UserModel user, string id)
        {
            if (ModelState.IsValid)
            {
                user.id = id;
                user.password = "";
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiRequests.PutToApiAsync(user, Urles.UserUrl + $"/login/{user.id}", token);
                if (result.StatusCode == 201)
                    return RedirectToAction("Index");
                else if (result.StatusCode == 500 && result.Message != null)
                {
                    ViewBag.Message = result.Message;
                }
                else ViewBag.Message = "Невозможно отправить данные";

            }
            return View(user);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPasswordAsync([Bind("email, password, idHouse, idFlatOwner, role")] UserModel user, string id)
        {
            if (ModelState.IsValid)
            {
                if (user.password == null) ModelState.AddModelError("password", "Поле должно быть установлено");
                else
                {
                    user.id = id;
                    var context = HttpContext;
                    var token = UserProperties.GetToken(ref context);
                    var result = await ApiRequests.PutToApiAsync(user, Urles.UserUrl + $"/password/{user.id}", token);
                    if (result.StatusCode == 201)
                        return RedirectToAction("Index");
                    else if (result.StatusCode == 500 && result.Message != null)
                    {
                        ViewBag.Message = result.Message;
                    }
                    else ViewBag.Message = "Невозможно отправить данные";
                }

            }
            return View(user);
        }
    }
}
