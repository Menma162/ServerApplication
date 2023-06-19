using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "HouseAdmin")]
    public class FlatController : Controller
    {
        public async Task<IActionResult> IndexAsync(string search, string searchHouse)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
            var houses = resultHouse.Item as List<House>;

            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
            List<Flat>? flats = resultFlat.Item as List<Flat>;
            if (search != null) flats = flats.Where(it => it.flatNumber.ToLower().Contains(search.ToLower())).ToList();
            if (searchHouse != null)
            {
                var housesSearch = houses.Where(it => it.name.ToLower().Contains(searchHouse.ToLower())).ToList();
                var newFlats = new List<Flat>();
                foreach (var house in housesSearch)
                {
                    newFlats.AddRange(flats.Where(it => it.idHouse == house.id));
                }
                flats = newFlats;
            }

            if (resultFlat.StatusCode == 200)
            {
                List<string> names = new();

                foreach (Flat flat in flats)
                    names.Add(houses.First(it => it.id == flat.idHouse).name);

                ViewBag.Names = names;

                return View("Index", flats);
            }
            return new NotFoundResult();
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync([Bind("flatNumber, personalAccount, totalArea, usableArea, entranceNumber, numberOfRooms, numberOfRegisteredResidents, numberOfOwners")] Flat flat, int idHouse)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            if (ModelState.IsValid)
            {
                double totalArea = 0;
                double usableArea = 0;
                double.TryParse(flat.totalArea.Replace('.', ','), out totalArea);
                double.TryParse(flat.usableArea.Replace('.', ','), out usableArea);
                if (totalArea >= usableArea)
                {
                    flat.idHouse = idHouse;
                    var result = await ApiRequests.PostToApiAsync(flat, Urles.FlatUrl, token);
                    if (result.StatusCode == 201) return RedirectToAction("Index");
                    else if (result.StatusCode == 500 && result.Message != null)
                    {
                        ViewBag.Message = result.Message;
                    }
                    else ViewBag.Message = "Невозможно отправить данные";
                }
                else ModelState.AddModelError("totalArea", "Общая площадь должна быть больше полезной");
            }
            ViewBag.IdHouse = idHouse.ToString();
            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{idHouse}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            House house = resultHouse.Item as House;
            ViewBag.NameHouse = house.name;
            return View(flat);
        }

        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var result = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{id}");
                if (result.StatusCode == 200)
                {
                    Flat? flat = result.Item as Flat;
                    {
                        if (flat != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            return View(flat);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("id, idHouse, flatNumber, personalAccount, totalArea, usableArea, entranceNumber, numberOfRooms, numberOfRegisteredResidents, numberOfOwners, idFlatOwner")] Flat flat)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            if (ModelState.IsValid)
            {
                double totalArea = 0;
                double usableArea = 0;
                double.TryParse(flat.totalArea.Replace('.', ','), out totalArea);
                double.TryParse(flat.usableArea.Replace('.', ','), out usableArea);
                if (totalArea >= usableArea)
                {
                    var result = await ApiRequests.PutToApiAsync(flat, Urles.FlatUrl + $"/{flat.id}", token);
                    if (result.StatusCode == 201)
                        return RedirectToAction("Index");
                    else if (result.StatusCode == 500 && result.Message != null)
                    {
                        ViewBag.Message = result.Message;
                    }
                    else ViewBag.Message = "Невозможно отправить данные";

                }
                else ModelState.AddModelError("totalArea", "Общая площадь должна быть больше полезной");

            }
            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            House house = resultHouse.Item as House;
            ViewBag.NameHouse = house.name;
            return View(flat);
        }

        public async Task<ActionResult> DeleteAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{id}");
                ResultsRequest resultFlatOwner = null;
                if (resultFlat.StatusCode == 200)
                {
                    Flat? flat = resultFlat.Item as Flat;
                    {
                        if (flat != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;

                            //var resultFlatOwners = await ApiGetRequests.LoadListFlatOwnersFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{flat.id}");
                            //if (resultFlatOwners.StatusCode != 200 || resultFlatOwners.Item) return new NotFoundResult();
                            //List<FlatOwner> flatOwners = resultFlatOwners.Item as List<FlatOwner>;
                            //ViewBag.NameHouse = house.name;

                            //if (flat.idFlatOwner != null)
                            //{
                            //    resultFlatOwner = await ApiGetRequests.LoadFlatOwnerFromAPIAsync(token, Urles.FlatOwnerUrl + $"/{flat.idFlatOwner}");
                            //    if (resultFlatOwner.StatusCode == 200)
                            //    {
                            //        FlatOwner flatOwner = resultFlatOwner.Item as FlatOwner;
                            //        ViewBag.FullName = flatOwner.fullName;
                            //    }
                            //}
                            return View(flat);
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
            var result = await ApiRequests.DeleteToApiAsync(Urles.FlatUrl + $"/{id}", token);
            if (result.StatusCode == 204) return RedirectToAction("Index");
            else
            {
                Flat? flat = new Flat();
                var resultGet = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{id}");
                if (resultGet.StatusCode == 200)
                {
                    flat = resultGet.Item as Flat;
                    var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                    if (resultHouse.StatusCode != 200) return new NotFoundResult();
                    House house = resultHouse.Item as House;
                    ViewBag.NameHouse = house.name;
                }
                if (result.StatusCode == 500 && result.Message != null)
                {
                    ViewBag.Message = result.Message;
                    return View(flat);
                }
                else
                {
                    ViewBag.Message = "Невозможно удалить данные";
                    return View(flat);
                }
            }
        }

        public async Task<ActionResult> DetailsAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{id}");
                ResultsRequest resultFlatOwner = null;
                if (resultFlat.StatusCode == 200)
                {
                    Flat? flat = resultFlat.Item as Flat;
                    {
                        if (flat != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;

                            var resultFlatOwners = await ApiGetRequests.LoadListFlatOwnersFromAPIAsync(token, Urles.ResidenceUrl + $"/byflat/{flat.id}");
                            List<FlatOwner> flatOwners = resultFlatOwners.Item as List<FlatOwner>;
                            if (resultFlatOwners.StatusCode != 200 || flatOwners == null) return new NotFoundResult();
                            ViewBag.FlatOwners = flatOwners;
                            ViewBag.id = flat.id;
                            return View(flat);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }
        public async Task<ActionResult> SelectFlatOwner(int id)
        {
            ViewBag.IdFlat = id;
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var resultFlatOwner = await ApiGetRequests.LoadListFlatOwnersFromAPIAsync(token, Urles.ResidenceUrl + $"/{id}/{UserProperties.GetIdUser(ref context)}");
            List<FlatOwner> flatOwners = resultFlatOwner.Item as List<FlatOwner>;
            if (resultFlatOwner.StatusCode != 200 || flatOwners == null) return new NotFoundResult();
            flatOwners = flatOwners.Distinct().ToList();

            ViewBag.FlatOwners = flatOwners;
            List<ModelCheckBox> modelCheckBox = new();
            foreach (var flatOwner in flatOwners)
                modelCheckBox.Add(new ModelCheckBox(flatOwner.id, false));
            return View(modelCheckBox);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SelectFlatOwner(List<ModelCheckBox> ModelCheckBox, int idFlat)
        {
            ViewBag.IdFlat = idFlat;
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            List<Residence> residences = new List<Residence>();
            foreach(var model in ModelCheckBox)
            {
                if(model.selected == true) residences.Add(new Residence(model.id, idFlat));
            }
            var result = await ApiRequests.PostToApiAsync(residences, Urles.ResidenceUrl + $"/{idFlat}", token);
            if (result.StatusCode == 201) return RedirectToAction("Details", new { @id = idFlat });
            else if (result.StatusCode == 500 && result.Message != null) ViewBag.Message = result.Message;
            else ViewBag.Message = "Не удалось изменить владельцев для квартиры";

            var resultFlatOwner = await ApiGetRequests.LoadListFlatOwnersFromAPIAsync(token, Urles.ResidenceUrl + $"/havenoFlat/{UserProperties.GetIdUser(ref context)}");
            List<FlatOwner> flatOwners = resultFlatOwner.Item as List<FlatOwner>;
            if (resultFlatOwner.StatusCode != 200 || flatOwners == null) return new NotFoundResult();

            ViewBag.FlatOwners = flatOwners;
            List<ModelCheckBox> modelCheckBox = new();
            foreach (var flatOwner in flatOwners)
                modelCheckBox.Add(new ModelCheckBox(flatOwner.id, false));
            return View(modelCheckBox);
        }
    }
}