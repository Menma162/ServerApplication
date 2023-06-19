using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using System.Data;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "HouseAdmin")]
    public class FlatOwnerController : Controller
    {
        public async Task<IActionResult> IndexAsync(string search, string searchHouse)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
            var houses = resultHouse.Item as List<House>;

            var resultFlatOwner = await ApiGetRequests.LoadListFlatOwnersFromAPIAsync(token, Urles.FlatOwnerUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
            List<FlatOwner>? flatOwners = resultFlatOwner.Item as List<FlatOwner>;

            if (resultFlatOwner.StatusCode != 200) return new NotFoundResult();

            if (search != null) flatOwners = flatOwners.Where(it => it.fullName.ToLower().Contains(search.ToLower())).ToList();
            if (searchHouse != null)
            {
                var housesSearch = houses.Where(it => it.name.ToLower().Contains(searchHouse.ToLower())).ToList();
                var newFlatsOwners = new List<FlatOwner>();
                foreach (var house in housesSearch)
                {
                    newFlatsOwners.AddRange(flatOwners.Where(it => it.idHouse == house.id));
                }
                flatOwners = newFlatsOwners;
            }
            List<string> names = new();

            foreach (FlatOwner flatOwner in flatOwners)
                names.Add(houses.First(it => it.id == flatOwner.idHouse).name);

            ViewBag.Names = names;
            return View("Index", flatOwners);
        }

        public async Task<ActionResult> SelectHouse(string search)
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

        public async Task<ActionResult> CreateAsync(int id)
        {
            var context = HttpContext;
            if (!UserProperties.GetIdHouses(ref context).Contains(id)) return new NotFoundResult();
            var token = UserProperties.GetToken(ref context);
            ViewBag.IdHouse = id.ToString();
            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{id}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            House house = resultHouse.Item as House;
            ViewBag.NameHouse = house.name;
            return View(new FlatOwner());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("fullName, phoneNumber, dateOfRegistration, email")] FlatOwner flatOwner, int idHouse)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            if (ModelState.IsValid)
            {
                flatOwner.idHouse = idHouse;
                var result = await ApiRequests.PostToApiAsync(flatOwner, Urles.FlatOwnerUrl, token);
                if (result.StatusCode == 201) return RedirectToAction("Index");
                else if (result.StatusCode == 500 && result.Message != null)
                {
                    ViewBag.Message = result.Message;
                }
                else ViewBag.Message = "Невозможно отправить данные";
            }
            ViewBag.IdHouse = idHouse.ToString();
            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{idHouse}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            House house = resultHouse.Item as House;
            ViewBag.NameHouse = house.name;
            return View(flatOwner);
        }

        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{id}");
                if (result.StatusCode == 200)
                {
                    FlatOwner? flatOwner = result.Item as FlatOwner;
                    {
                        if (flatOwner != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flatOwner.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            return View(flatOwner);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("id, idHouse, fullName, phoneNumber, dateOfRegistration, email")] FlatOwner flatOwner)
        {
            if (ModelState.IsValid)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiRequests.PutToApiAsync(flatOwner, Urles.FlatOwnerUrl + $"/{flatOwner.id}", token);
                if (result.StatusCode == 201)
                    return RedirectToAction("Index");
                else if (result.StatusCode == 500 && result.Message != null)
                {
                    ViewBag.Message = result.Message;
                }
                else ViewBag.Message = "Невозможно отправить данные";

                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flatOwner.idHouse}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House house = resultHouse.Item as House;
                ViewBag.NameHouse = house.name;
            }
            return View(flatOwner);
        }

        public async Task<ActionResult> DeleteAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{id}");
                if (resultFlatOwner.StatusCode == 200)
                {
                    FlatOwner? flatOwner = resultFlatOwner.Item as FlatOwner;
                    {
                        if (flatOwner != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flatOwner.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            return View(flatOwner);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var result = await ApiRequests.DeleteToApiAsync(Urles.FlatOwnerUrl + $"/{id}", token);
            if (result.StatusCode == 204) return RedirectToAction("Index");
            else
            {
                FlatOwner? flatOwner = new FlatOwner();
                var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{id}");
                if (resultFlatOwner.StatusCode == 200)
                {
                    flatOwner = resultFlatOwner.Item as FlatOwner;
                }
                else if (result.StatusCode == 500 && result.Message != null)
                {
                    ViewBag.Message = result.Message;
                }
                    var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flatOwner.idHouse}");
                    if (resultHouse.StatusCode != 200) return new NotFoundResult();
                    House house = resultHouse.Item as House;
                    ViewBag.NameHouse = house.name;
                    ViewBag.Message = "Невозможно удалить данные";
                    return View(flatOwner);
            }
        }

        public async Task<ActionResult> DetailsAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{id}");
                if (resultFlatOwner.StatusCode == 200)
                {
                    FlatOwner? flatOwner = resultFlatOwner.Item as FlatOwner;
                    {
                        if (flatOwner != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flatOwner.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name; 

                            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/flatowner/{id}");
                            if (resultFlat.StatusCode == 200)
                            {
                                List<Flat> flats = resultFlat.Item as List<Flat>;
                                if (flats != null && flats.Count != 0) ViewBag.Flats = flats;
                            }
                            ViewBag.Id = id;
                            return View(flatOwner);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }
    }
}