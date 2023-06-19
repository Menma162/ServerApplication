using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "HouseAdmin")]
    public class CounterController : Controller
    {
        public async Task<IActionResult> IndexAsync(string searchFlat, string name, string type, string status, string searchHouse)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
            List<Counter>? counters = resultCounter.Item as List<Counter>;

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            List<Service>? services = resultService.Item as List<Service>;

            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
            List<Flat>? flats = resultFlat.Item as List<Flat>;

            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
            var houses = resultHouse.Item as List<House>;

            if (resultCounter.StatusCode == 200 && resultService.StatusCode == 200 && resultFlat.StatusCode == 200 && resultHouse.StatusCode == 200)
            {
                List<string> names = new();
                List<string> numbersFlats = new();
                List<string> types = new();

                //filters
                List<string> typesFilters = new List<string> { "", "Индивидуальный прибор учета", "Общедомовой прибор учета" };
                ViewBag.TypesFilters = new SelectList(typesFilters);
                List<string> statusesFilters = new List<string> { "", "Используется", "Не используется" };
                ViewBag.StatusesFilters = new SelectList(statusesFilters);

                if (searchFlat != null && counters != null && flats != null)
                {
                    var flat = flats.FirstOrDefault(it => it.flatNumber == searchFlat);
                    if (flat != null) counters = counters.Where(it => it.idFlat == flat.id).ToList();
                }
                if (name != null && name != "" && counters != null && services != null) counters = counters.Where(it => it.idService == services.First(it => it.nameCounter == name).id).ToList();
                if (type == "Индивидуальный прибор учета" && counters != null) counters = counters.Where(it => it.IMDOrGHMD == true).ToList();
                if (type == "Общедомовой прибор учета" && counters != null) counters = counters.Where(it => it.IMDOrGHMD == false).ToList();
                if (status == "Используется" && counters != null) counters = counters.Where(it => it.used == true).ToList();
                if (status == "Не используется" && counters != null) counters = counters.Where(it => it.used == false).ToList();
                if (services != null)
                {
                    List<string> namesFilters = services.Where(it => it.nameCounter != null).Select(it => it.nameCounter).ToList();
                    namesFilters.Insert(0, "");
                    ViewBag.NamesFilters = new SelectList(namesFilters);
                }
                else return new NotFoundResult();
                if (searchHouse != null)
                {
                    var housesSearch = houses.Where(it => it.name.ToLower().Contains(searchHouse.ToLower())).ToList();
                    var newCounters = new List<Counter>();
                    foreach (var house in housesSearch)
                    {
                        newCounters.AddRange(counters.Where(it => it.idHouse == house.id));
                    }
                    counters = newCounters;
                }

                var namesHouses = new List<string>();

                if (counters != null && services != null)
                    foreach (var counter in counters)
                    {
                        namesHouses.Add(houses.First(it => it.id == counter.idHouse).name);
                        names.Add(services.First(it => it.id == counter.idService).nameCounter);
                        if (counter.IMDOrGHMD == true) types.Add("Индивидуальный прибор учета");
                        else types.Add("Общедомовой прибор учета");
                        if (counter.idFlat != null && flats != null) numbersFlats.Add(flats.First(it => it.id == counter.idFlat).flatNumber);
                        else numbersFlats.Add("-");
                    }

                ViewBag.NamesHouses = namesHouses;
                ViewBag.Names = names;
                ViewBag.NumbersFlats = numbersFlats;
                ViewBag.Types = types;
                if (counters != null)
                {
                    counters.ForEach(it => it.dateLastVerification = DateTime.Parse(it.dateLastVerification.ToShortDateString()));
                    counters.ForEach(it => it.dateNextVerification = DateTime.Parse(it.dateNextVerification.ToShortDateString()));
                }

                return View("Index", counters);
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

        public async Task<ActionResult> SelectFlat(string search, int id)
        {
            var context = HttpContext;
            if (!UserProperties.GetIdHouses(ref context).Contains(id)) return new NotFoundResult();
            var token = UserProperties.GetToken(ref context);
            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/house/{id}");
            if (resultFlat.StatusCode == 200)
            {
                if (!UserProperties.GetIdHouses(ref context).Contains(id)) return new NotFoundResult();
                ViewBag.IdHouse = id.ToString();
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{id}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House house = resultHouse.Item as House;
                ViewBag.NameHouse = house.name;

                List<Flat>? flats = resultFlat.Item as List<Flat>;
                if (flats != null && search != null)
                {
                    flats = flats.Where(it => it.flatNumber.ToLower().Contains(search.ToLower())).ToList();
                }

                if (flats != null) flats.Add(null);
                else
                {
                    flats = new List<Flat>
                        {
                            null
                    };
                }
                var resultSettinsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{id}");
                List<SettingsService>? settingsServices = resultSettinsService.Item as List<SettingsService>;
                if (resultSettinsService.StatusCode != 200 || settingsServices == null) return new NotFoundResult();
                bool canGHM = true, canIMD = true;
                if (settingsServices.Where(it => it.typeIMD == true && it.haveCounter == true && it.paymentStatus == true).Count() == 0) canGHM = false;
                if (settingsServices.Where(it => it.typeIMD == false && it.haveCounter == true && it.paymentStatus == true).Count() == 0) canIMD = false;
                if (canGHM == false) ViewBag.Message = "Добавление индивидуальных приборов недоступно, проверьте настройки начисляемых услуг";
                if (canIMD == false) ViewBag.Message = "Добавление общедомовых приборов недоступно, проверьте настройки начисляемых услуг"; 
                if (canIMD == false && canGHM == false) ViewBag.Message = "Добавление индивидуальных и общедомовых приборов недоступно, проверьте настройки начисляемых услуг";
                ViewBag.CanGHM = canGHM;
                ViewBag.CanIMD = canIMD;
                return View("SelectFlat", flats);
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectFlat(int? idFlat, int idHouse)
        {
            return RedirectToAction("AddInfoCreate", new { @idFlat = idFlat, @idHouse = idHouse });
        }

        public async Task<ActionResult> AddInfoCreateAsync(int? idFlat, int idHouse)
        {
            var context = HttpContext;
            if (!UserProperties.GetIdHouses(ref context).Contains(idHouse)) return new NotFoundResult();
            var token = UserProperties.GetToken(ref context);
            if (idFlat != null)
            {
                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{idFlat}");
                if (resultFlat.StatusCode == 200)
                {
                    Flat? flat = resultFlat.Item as Flat;
                    ViewBag.Flat = flat;
                    ViewBag.Type = "Индивидуальный прибор учета";
                }
            }
            else ViewBag.Type = "Общедомовой прибор учета";

            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{idHouse}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            House house = resultHouse.Item as House;
            ViewBag.NameHouse = house.name;
            ViewBag.IdHouse = idHouse;

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            var resultSettinsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{idHouse}");
            if (resultService.StatusCode == 200 && resultSettinsService.StatusCode == 200)
            {
                List<Service>? services = resultService.Item as List<Service>;
                List<SettingsService>? settingsServices = resultSettinsService.Item as List<SettingsService>;
                if (services != null && settingsServices != null)
                {
                    if (idFlat != null && settingsServices.Where(it => it.typeIMD == true && it.haveCounter == true && it.paymentStatus == true).Count() == 0) return new NotFoundResult();
                    if (idFlat == null && settingsServices.Where(it => it.typeIMD == false && it.haveCounter == true && it.paymentStatus == true).Count() == 0) return new NotFoundResult();

                    List<string> namesIMD = new();
                    List<string> namesHG = new();
                    foreach (var settingsService in settingsServices)
                    {
                        if (idFlat != null && settingsService.haveCounter == true && settingsService.typeIMD == true && settingsService.paymentStatus == true) namesIMD.Add(services.First(it => it.id == settingsService.idService).nameCounter);
                        else if (idFlat == null && settingsService.haveCounter == true && settingsService.typeIMD == false && settingsService.paymentStatus == true) namesHG.Add(services.First(it => it.id == settingsService.idService).nameCounter);
                    }

                    if (idFlat != null) ViewBag.Names = new SelectList(namesIMD);
                    else ViewBag.Names = new SelectList(namesHG);

                    return View("AddInfoCreate", new Counter());
                }
            }
            return new NotFoundResult();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddInfoCreateAsync([Bind("idFlat, number, dateLastVerification, dateNextVerification, idHouse")] Counter counter, string name)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            if (counter.idFlat != null)
            {
                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                if (resultFlat.StatusCode == 200)
                {
                    Flat? flat = resultFlat.Item as Flat;
                    ViewBag.Flat = flat;
                    ViewBag.Type = "Индивидуальный прибор учета";
                }
            }
            else ViewBag.Type = "Общедомовой прибор учета";

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            var resultSettinsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{counter.idHouse}");

            List<Service>? services = null;
            List<SettingsService>? settingsServices = null;

            if (resultService.StatusCode == 200 && resultSettinsService.StatusCode == 200)
            {
                services = resultService.Item as List<Service>;
                settingsServices = resultSettinsService.Item as List<SettingsService>;
                if (services != null && settingsServices != null)
                {
                    List<string> namesIMD = new();
                    List<string> namesHG = new();
                    foreach (var settingsService in settingsServices)
                    {
                        if (counter.idFlat != null && settingsService.haveCounter == true && settingsService.typeIMD == true && settingsService.paymentStatus == true) namesIMD.Add(services.First(it => it.id == settingsService.idService).nameCounter);
                        else if (counter.idFlat == null && settingsService.haveCounter == true && settingsService.typeIMD == false && settingsService.paymentStatus == true) namesHG.Add(services.First(it => it.id == settingsService.idService).nameCounter);
                    }

                    if (counter.idFlat != null) ViewBag.Names = new SelectList(namesIMD);
                    else ViewBag.Names = new SelectList(namesHG);
                }
            }

            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
            if (resultHouse.StatusCode != 200) return new NotFoundResult();
            House house = resultHouse.Item as House;
            ViewBag.NameHouse = house.name;
            ViewBag.IdHouse = counter.idHouse;

            if (ModelState.IsValid)
            {
                if (counter.dateNextVerification > counter.dateLastVerification)
                {
                    if (counter.idFlat == null) counter.IMDOrGHMD = false;
                    else counter.IMDOrGHMD = true;
                    counter.idService = services.First(it => it.nameCounter == name).id;
                    counter.used = true;

                    var result = await ApiRequests.PostToApiAsync(counter, Urles.CounterUrl, token);
                    if (result.StatusCode == 201) return RedirectToAction("Index");
                    else if (result.StatusCode == 500 && result.Message != null)
                    {
                        ViewBag.Message = result.Message;
                    }
                    else ViewBag.Message = "Невозможно отправить данные";
                }
                else ModelState.AddModelError("dateNextVerification", "Дата следующей поверки должна быть больше предыдущей");
            }

            if (services != null && settingsServices != null)
                return View(counter);
            else return new NotFoundResult();
        }

        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);

                var result = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{id}");
                if (result.StatusCode == 200)
                {
                    Counter? counter = result.Item as Counter;
                    {
                        if (counter != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            if (counter.idFlat != null)
                            {
                                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                                if (resultFlat.StatusCode == 200)
                                {
                                    Flat? flat = resultFlat.Item as Flat;
                                    if (flat != null) ViewBag.FlatNumber = flat.flatNumber;
                                    ViewBag.Type = "Индивидуальный прибор учета";
                                }

                            }
                            else ViewBag.Type = "Общедомовой прибор учета";

                            var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{counter.idService}");

                            if (resultService.StatusCode == 200)
                            {
                                Service? service = resultService.Item as Service;
                                if (service != null)
                                {
                                    ViewBag.Name = service.nameCounter;
                                    return View(counter);
                                }
                            }

                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("id, idFlat, number, dateLastVerification, dateNextVerification, used, IMDOrGHMD, idService, idHouse")] Counter counter)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            if (ModelState.IsValid)
            {
                if (counter.dateNextVerification > counter.dateLastVerification)
                {
                    var result = await ApiRequests.PutToApiAsync(counter, Urles.CounterUrl + $"/{counter.id}", token);
                    if (result.StatusCode == 201)
                        return RedirectToAction("Index");
                    if (result.StatusCode == 500 && result.Message != null)
                    {
                        ViewBag.Message = result.Message;
                    }
                    else ViewBag.Message = "Невозможно отправить данные";

                }
                else ModelState.AddModelError("dateNextVerification", "Дата следующей поверки должна быть больше предыдущей");

            }
            if (counter.idFlat != null)
            {
                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                if (resultFlat.StatusCode == 200)
                {
                    Flat? flat = resultFlat.Item as Flat;
                    if (flat != null) ViewBag.FlatNumber = flat.flatNumber;
                    ViewBag.Type = "Индивидуальный прибор учета";
                }

            }
            else ViewBag.Type = "Общедомовой прибор учета";

            var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{counter.idService}");

            if (resultService.StatusCode == 200)
            {
                Service? service = resultService.Item as Service;
                if (service != null)
                {
                    var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                    if (resultHouse.StatusCode != 200) return new NotFoundResult();
                    House house = resultHouse.Item as House;
                    ViewBag.NameHouse = house.name;
                    ViewBag.Name = service.nameCounter;
                    return View(counter);
                }
            }

            return new NotFoundResult();
        }

        public async Task<ActionResult> DeleteAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{id}");
                if (resultCounter.StatusCode == 200)
                {
                    Counter? counter = resultCounter.Item as Counter;
                    {
                        if (counter != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            if (counter.idFlat != null)
                            {
                                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                                if (resultFlat.StatusCode == 200)
                                {
                                    Flat? flat = resultFlat.Item as Flat;
                                    if (flat != null) ViewBag.FlatNumber = flat.flatNumber;
                                    ViewBag.Type = "Индивидуальный прибор учета";
                                }

                            }
                            else ViewBag.Type = "Общедомовой прибор учета";

                            var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{counter.idService}");

                            if (resultService.StatusCode == 200)
                            {
                                Service? service = resultService.Item as Service;
                                if (service != null)
                                {
                                    if (counter != null)
                                    {
                                        counter.dateLastVerification = DateTime.Parse(counter.dateLastVerification.ToShortDateString());
                                        counter.dateNextVerification = DateTime.Parse(counter.dateNextVerification.ToShortDateString());
                                    }
                                    ViewBag.Name = service.nameCounter;
                                    return View(counter);
                                }
                            }
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
            var result = await ApiRequests.DeleteToApiAsync(Urles.CounterUrl + $"/{id}", token);
            if (result.StatusCode == 204) return RedirectToAction("Index");
            else
            {
                var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{id}");
                if (resultCounter.StatusCode == 200)
                {
                    Counter? counter = resultCounter.Item as Counter;
                    {
                        if (counter != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            if (counter != null)
                            {
                                counter.dateLastVerification = DateTime.Parse(counter.dateLastVerification.ToShortDateString());
                                counter.dateNextVerification = DateTime.Parse(counter.dateNextVerification.ToShortDateString());
                            }
                            if (counter.idFlat != null)
                            {
                                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                                if (resultFlat.StatusCode == 200)
                                {
                                    Flat? flat = resultFlat.Item as Flat;
                                    if (flat != null) ViewBag.FlatNumber = flat.flatNumber;
                                    ViewBag.Type = "Индивидуальный прибор учета";
                                }

                            }
                            else ViewBag.Type = "Общедомовой прибор учета";

                            var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{counter.idService}");

                            if (resultService.StatusCode == 200)
                            {
                                Service? service = resultService.Item as Service;
                                if (service != null)
                                {
                                    ViewBag.Name = service.nameCounter;
                                }
                            }

                            if (result.StatusCode == 500 && result.Message != null)
                            {
                                ViewBag.Message = result.Message;
                                return View(counter);
                            }
                            else
                            {
                                ViewBag.Message = "Невозможно удалить данные";
                                return View(counter);
                            }
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        public async Task<ActionResult> DetailsAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);
                var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{id}");
                if (resultCounter.StatusCode == 200)
                {
                    Counter? counter = resultCounter.Item as Counter;
                    {
                        if (counter != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            if (counter != null)
                            {
                                counter.dateLastVerification = DateTime.Parse(counter.dateLastVerification.ToShortDateString());
                                counter.dateNextVerification = DateTime.Parse(counter.dateNextVerification.ToShortDateString());
                            }
                            if (counter.idFlat != null)
                            {
                                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                                if (resultFlat.StatusCode == 200)
                                {
                                    Flat? flat = resultFlat.Item as Flat;
                                    if (flat != null) ViewBag.Flat = flat;
                                    ViewBag.Type = "Индивидуальный прибор учета";
                                }

                            }
                            else ViewBag.Type = "Общедомовой прибор учета";

                            var resultIndication = await ApiGetRequests.LoadIndicationFromAPIAsync(token, Urles.IndicationUrl + $"/lastfromcounter/{counter.id}");
                            if (resultIndication.StatusCode == 200)
                            {
                                Indication indication = resultIndication.Item as Indication;
                                if (indication != null) ViewBag.Indication = indication;
                            }

                            var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{counter.idService}");

                            if (resultService.StatusCode == 200)
                            {
                                Service? service = resultService.Item as Service;
                                if (service != null)
                                {
                                    ViewBag.Name = service.nameCounter;
                                    ViewBag.Id = id;
                                    return View(counter);
                                }
                            }
                        }
                    }
                }
            }
            return new NotFoundResult();
        }
    }
}