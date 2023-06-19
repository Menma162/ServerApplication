using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Xml.Linq;

namespace HouseManagement.Controllers
{

    [Authorize]
    public class UserController : Controller
    {
        public async Task<IActionResult> IndexAsync(string search)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var role = UserProperties.GetRole(ref context);

            var url = "";
            if (role == "HouseAdmin")
                url = $"/house/houseAdmin/{UserProperties.GetIdUser(ref context)}";
            else url = $"/house/mainAdmin";

            var resultUser = await ApiGetRequests.LoadListUsersFromAPIAsync(token, Urles.UserUrl + url);
            List<UserModel>? users = resultUser.Item as List<UserModel>;

            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl);
            List<House>? houses = resultHouse.Item as List<House>;

            if (users == null || houses == null) return new NotFoundResult();

            if (search != null) users = users.Where(it => it.email.ToLower().Contains(search.ToLower())).ToList();
            List<string> fullNames = new();
            List<string> roles = new();
            List<List<string>> namesHouses = new();
            if (role == "MainAdmin")
            {
                foreach (var user in users)
                {
                    fullNames.Add("-");
                    roles.Add("Администратор");
                    List<string> name = new();
                    foreach (var id in user.idHouses)
                    {
                        name.Add("-- " + houses.First(it => it.id == Convert.ToInt32(id)).name);
                    }
                    namesHouses.Add(name);
                }
            }
            if (role == "HouseAdmin")
            {
                var resultFlatOwner = await ApiGetRequests.LoadListFlatOwnersFromAPIAsync(token, Urles.FlatOwnerUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
                List<FlatOwner>? flatOwners = resultFlatOwner.Item as List<FlatOwner>;
                if (flatOwners == null) return new NotFoundResult();
                foreach (var user in users)
                {
                    if (user.role == "HouseAdmin")
                    {
                        fullNames.Add("-");
                        roles.Add("Администратор");
                    }
                    else
                    {
                        fullNames.Add(flatOwners.First(it => it.id == user.idFlatOwner).fullName);
                        roles.Add("Владелец квартиры");
                    }
                    List<string> name = new();
                    foreach (var id in user.idHouses)
                    {
                        name.Add("-- " + houses.First(it => it.id == Convert.ToInt32(id)).name);
                    }
                    namesHouses.Add(name);
                }
            }

            ViewBag.User = fullNames;
            ViewBag.NamesHouses = namesHouses;
            ViewBag.Roles = roles;
            return View("Index", users);
        }
        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> SelectHouseOne(string search)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
            if (resultHouse.StatusCode == 200)
            {
                List<House>? houses = resultHouse.Item as List<House>;
                if (search != null && houses != null) houses = houses.Where(it => it.name.ToLower().Contains(search.ToLower())).ToList();
                return View(houses);
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> SelectFlatOwner(int id, string search)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            ViewBag.IdHouse = id;
            var resultFlatOwner = await ApiGetRequests.LoadListFlatOwnersFromAPIAsync(token, Urles.FlatOwnerUrl + $"/house/haveNotUser/{id}");
            if (resultFlatOwner.StatusCode == 200)
            {
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{id}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House house = resultHouse.Item as House;
                ViewBag.NameHouse = house.name;

                List<FlatOwner>? flatOwners = resultFlatOwner.Item as List<FlatOwner>;
                if (search != null) flatOwners = flatOwners.Where(it => it.fullName.ToLower().Contains(search.ToLower())).ToList();
                return View(flatOwners);
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectFlatOwner(int idHouse, int idFlatOwner)
        {
            return RedirectToAction("CreateFlatOwnerUser", new { @idHouse = idHouse, @idFlatOwner = idFlatOwner });
        }


        [Authorize(Roles = "HouseAdmin, MainAdmin")]
        public async Task<ActionResult> CreateAdminUserAsync(List<int> houses)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var names = new List<string>();
            if (houses == null || houses.Count == 0) houses = UserProperties.GetIdHouses(ref context);
            foreach (var house in houses)
            {
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{house}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House? houseItem = resultHouse.Item as House;
                names.Add(houseItem.name);
            }

            ViewBag.NameHouse = names;
            ViewBag.Houses = houses;
            ViewBag.Role = "Администратор";
            return View();
        }

        [Authorize(Roles = "HouseAdmin, MainAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAdminUserAsync([Bind("email, password")] UserModel userModel, List<int>? Houses)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            ViewBag.Role = "Администратор";
            if (ModelState.IsValid)
            {
                if (userModel.password == null) ModelState.AddModelError("password", "Поле должно быть установлено");
                else
                {
                    userModel.role = "HouseAdmin";
                    if (Houses == null || Houses.Count == 0) userModel.idHouses = UserProperties.GetIdHouses(ref context);
                    else userModel.idHouses = Houses;

                    var result = await ApiRequests.PostToApiAsync(userModel, Urles.UserUrl, token);
                    if (result.StatusCode == 200) return RedirectToAction("Index");
                    else if (result.StatusCode == 500 && result.Message != null)
                    {
                        ViewBag.Message = result.Message;
                    }
                    else ViewBag.Message = "Невозможно отправить данные";
                }
            }
            if (Houses != null)
            {
                var names = new List<string>();
                foreach (var house in Houses)
                {
                    var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{house}");
                    if (resultHouse.StatusCode != 200) return new NotFoundResult();
                    House? houseItem = resultHouse.Item as House;
                    names.Add(houseItem.name);
                }

                ViewBag.Houses = Houses;
                ViewBag.NameHouse = names;
            }

            return View(userModel);
        }

        [Authorize(Roles = "MainAdmin")]
        public async Task<ActionResult> SelectHouse()
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + "/notUses");
            if (resultHouse.StatusCode == 200)
            {
                List<House>? houses = resultHouse.Item as List<House>;
                ViewBag.Houses = houses;
                List<ModelCheckBox> modelCheckBox = new();
                foreach (var house in houses)
                    modelCheckBox.Add(new ModelCheckBox(house.id, false));
                return View(modelCheckBox);
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "MainAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SelectHouse(List<ModelCheckBox> ModelCheckBox)
        {
            if (ModelCheckBox.Count == 0 || ModelCheckBox.Where(it => it.selected == true).Count() == 0)
            {
                ViewBag.Message = "Необходимо выбрать дома";
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + "/notUses");
                if (resultHouse.StatusCode == 200)
                {
                    List<House>? houses = resultHouse.Item as List<House>;
                    ViewBag.Houses = houses;
                    List<ModelCheckBox> modelCheckBox = new();
                    foreach (var house in houses)
                        modelCheckBox.Add(new ModelCheckBox(house.id, false));
                    return View(modelCheckBox);
                }
            }
            else
            {
                List<int> houses = ModelCheckBox.Where(it => it.selected == true).Select(it => it.id).ToList();
                return RedirectToAction("CreateAdminUser", new { @houses = houses });
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> CreateFlatOwnerUserAsync(int idHouse, int idFlatOwner)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{idFlatOwner}");
            if (resultFlatOwner.StatusCode == 200)
            {
                FlatOwner flatOwner = resultFlatOwner.Item as FlatOwner;
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flatOwner.idHouse}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House house = resultHouse.Item as House;
                ViewBag.NameHouse = house.name;
                ViewBag.Role = "Владелец квартиры";
                ViewBag.Email = flatOwner.email;
                ViewBag.IdFlatOwner = idFlatOwner;
                ViewBag.IdHouse = idHouse;
                return View();
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateFlatOwnerUserAsync([Bind("email, password, idFlatOwner")] UserModel userModel, int idHouse, int idFlatOwner)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            ViewBag.Role = "Владелец квартиры";
            ViewBag.IdFlatOwner = idFlatOwner;
            ViewBag.IdHouse = idHouse;
            if (ModelState.IsValid)
            {
                if (userModel.password == null) ModelState.AddModelError("password", "Поле должно быть установлено");
                else
                {
                    userModel.role = "FlatOwner";
                    userModel.idHouses = new List<int>() { idHouse };

                    var result = await ApiRequests.PostToApiAsync(userModel, Urles.UserUrl, token);
                    if (result.StatusCode == 200) return RedirectToAction("Index");
                    else if (result.StatusCode == 500 && result.Message != null)
                    {
                        ViewBag.Message = result.Message;
                    }
                    else ViewBag.Message = "Невозможно отправить данные";
                    var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{idFlatOwner}");
                    if (resultFlatOwner.StatusCode == 200)
                    {
                        FlatOwner flatOwner = resultFlatOwner.Item as FlatOwner;
                        var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flatOwner.idHouse}");
                        if (resultHouse.StatusCode != 200) return new NotFoundResult();
                        House house = resultHouse.Item as House;
                        ViewBag.NameHouse = house.name;
                        ViewBag.Email = flatOwner.email;
                        return View();
                    }
                }
            }
            return View(userModel);
        }

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
                    if (user.role != "HouseAdmin")
                    {
                        var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{user.idFlatOwner}");
                        FlatOwner? flatOwner = resultFlatOwner.Item as FlatOwner;
                        if (flatOwner != null && user != null)
                        {
                            user.password = "";
                            ViewBag.Id = user.id;
                            ViewBag.FullName = flatOwner.fullName;
                            return View("EditLogin", user);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

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
                    if (user.role != "HouseAdmin")
                    {
                        var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{user.idFlatOwner}");
                        FlatOwner? flatOwner = resultFlatOwner.Item as FlatOwner;
                        if (flatOwner != null && user != null)
                        {
                            ViewBag.Id = user.id;
                            ViewBag.FullName = flatOwner.fullName;
                            return View("EditPassword", user);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditLoginAsync([Bind("email, password, idHouses, idFlatOwner, role")] UserModel user, string fullName, string id)
        {
            ViewBag.FullName = fullName;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPasswordAsync([Bind("email, password, idHouses, idFlatOwner, role")] UserModel user, string fullName, string id)
        {
            ViewBag.FullName = fullName;
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
        [Authorize(Roles = "HouseAdmin, MainAdmin")]
        public async Task<ActionResult> DeleteAsync(string? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultUser = await ApiGetRequests.LoadUseByIdFromAPIAsync(token, Urles.UserUrl + $"/{id}");
                var user = resultUser.Item as UserModel;
                var resultHouses = await ApiGetRequests.LoadStringWithoutDeserializeAPIAsync(token, Urles.HouseUrl + $"/names/{id}");
                if (resultUser.StatusCode == 200 && user != null && resultHouses.StatusCode == 200)
                {
                    List<string> names = new();
                    var houses = (resultHouses.Item as string).Split(',');
                    foreach (var house in houses)
                    {
                        if (house != "") names.Add(house);
                    }
                    ViewBag.NamesHouses = names;
                    if (user.role == "FlatOwner")
                    {
                        var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{user.idFlatOwner}");
                        FlatOwner? flatOwner = resultFlatOwner.Item as FlatOwner;
                        if (flatOwner != null)
                        {
                            ViewBag.FullName = flatOwner.fullName;
                            ViewBag.Email = user.email;
                            ViewBag.Id = id;
                            return View(user);
                        }
                    }
                    if (user.role == "HouseAdmin")
                    {
                        ViewBag.Email = user.email;
                        ViewBag.Id = id;
                        return View(user);
                    }
                }
            }
            return new NotFoundResult();
        }

        [Authorize(Roles = "HouseAdmin, MainAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var role = UserProperties.GetRole(ref context);
            var result = await ApiRequests.DeleteToApiAsync(Urles.UserUrl + $"/{id}", token);
            if (result.StatusCode == 204) return RedirectToAction("Index");
            else
            {
                var resultUser = await ApiGetRequests.LoadUseByIdFromAPIAsync(token, Urles.UserUrl + $"/{id}");
                var user = resultUser.Item as UserModel;

                var resultHouses = await ApiGetRequests.LoadStringWithoutDeserializeAPIAsync(token, Urles.HouseUrl + $"/names/{id}");

                if (resultUser.StatusCode == 200 && user != null && resultHouses.StatusCode == 200)
                {
                    List<string> names = new();
                    var houses = (resultHouses.Item as string).Split(',');
                    foreach (var house in houses)
                    {
                        if (house != "") names.Add(house);
                    }
                    ViewBag.NamesHouses = names;
                    if (user.role == "FlatOwner" && role == "HouseAdmin")
                    {
                        var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{user.idFlatOwner}");
                        FlatOwner? flatOwner = resultFlatOwner.Item as FlatOwner;
                        if (flatOwner != null && user != null)
                        {
                            ViewBag.FullName = flatOwner.fullName;
                            ViewBag.Email = flatOwner.email;
                            ViewBag.Id = id;
                            ViewBag.Message = "Невозможно удалить данные";
                            return View();
                        }
                    }
                    if (user.role == "HouseAdmin" && role == "MainAdmin")
                    {
                        ViewBag.Email = user.email;
                        ViewBag.Id = id;
                        ViewBag.Message = "Невозможно удалить данные";
                        return View(user);
                    }
                }
            }
            return new NotFoundResult();
        }
    }
}