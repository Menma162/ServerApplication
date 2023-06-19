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
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "HouseAdmin")]
    public class IndicationController : Controller
    {
        public async Task<IActionResult> IndexAsync(string searchFlat, string searchHouse, string name, string type, string period)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            string url = "";
            if (period != null)
            {
                url = $"/lastfromadmin/{UserProperties.GetIdUser(ref context)}";
                ViewBag.Period = period;
            }
            else url = $"/admin/{UserProperties.GetIdUser(ref context)}";

            var resultIndication = await ApiGetRequests.LoadListIndicationsFromAPIAsync(token, Urles.IndicationUrl + url);
            List<Indication>? indications = resultIndication.Item as List<Indication>;

            var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
            List<Counter>? counters = resultCounter.Item as List<Counter>;

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            List<Service>? services = resultService.Item as List<Service>;

            var resultSettingsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
            List<SettingsService>? settingsServices = resultSettingsService.Item as List<SettingsService>;

            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
            List<Flat>? flats = resultFlat.Item as List<Flat>;

            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
            var houses = resultHouse.Item as List<House>;

            if (resultIndication.StatusCode == 200 && resultCounter.StatusCode == 200 && resultService.StatusCode == 200 && resultFlat.StatusCode == 200 && resultSettingsService.StatusCode == 200 && resultHouse.StatusCode == 200)
            {
                List<string> names = new();
                List<string> numbersFlats = new();
                List<string> types = new();

                //filters
                List<string> typesFilters = new List<string> { "", "Индивидуальный прибор учета", "Общедомовой прибор учета" };
                ViewBag.TypesFilters = new SelectList(typesFilters);
                List<string> statusesFilters = new List<string> { "", "Используется", "Не используется" };
                ViewBag.StatusesFilters = new SelectList(statusesFilters);

                if (searchFlat != null && counters != null && indications != null && flats != null)
                {
                    var flat = flats.FirstOrDefault(it => it.flatNumber == searchFlat);
                    if (flat != null) counters = counters.Where(it => it.idFlat == flat.id).ToList();
                }
                if (name != null && name != "" && counters != null && services != null && indications != null)
                {
                    counters = counters.Where(it => it.idService == services.First(it => it.nameService == name).id).ToList();
                }
                if (type == "Индивидуальный прибор учета" && counters != null) counters = counters.Where(it => it.IMDOrGHMD == true).ToList();
                if (type == "Общедомовой прибор учета" && counters != null) counters = counters.Where(it => it.IMDOrGHMD == false).ToList();
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

                var newIndications = new List<Indication>();
                if (counters != null && indications != null)
                    foreach (var counter in counters)
                    {
                        newIndications.AddRange(indications.Where(it => it.idCounter == counter.id));
                    }

                indications = newIndications;

                if (services != null)
                {
                    List<string> namesFilters = services.Where(it => it.nameCounter != null).Select(it => it.nameService).ToList();
                    namesFilters.Insert(0, "");
                    ViewBag.NamesFilters = new SelectList(namesFilters);
                }
                else return new NotFoundResult();

                var namesHouses = new List<string>();

                if (counters != null && indications != null && settingsServices != null && services != null)
                {
                    indications = indications.OrderByDescending(it => it.dateTransfer).ToList();
                    List<bool> can = new();
                    List<string> numbers = new();
                    var dateNow = DateTime.UtcNow;
                    foreach (var indication in indications)
                    {
                        var counter = counters.First(it => it.id == indication.idCounter);
                        namesHouses.Add(houses.First(it => it.id == counter.idHouse).name);
                        var settinService = settingsServices.First(it => it.idService == counter.idService);
                        names.Add(services.First(it => it.id == counter.idService).nameService);
                        if (counter.IMDOrGHMD == true) types.Add("Индивидуальный прибор учета");
                        else types.Add("Общедомовой прибор учета");
                        if (counter.idFlat != null && flats != null) numbersFlats.Add(flats.First(it => it.id == counter.idFlat).flatNumber);
                        else numbersFlats.Add("");
                        if (indication.dateTransfer.Year == dateNow.Year && indication.dateTransfer.Month == dateNow.Month && dateNow.Day >= settinService.startDateTransfer && dateNow.Day <= settinService.endDateTransfer) can.Add(true);
                        else can.Add(false);
                        numbers.Add(counter.number);
                    }
                    ViewBag.Can = can;
                    ViewBag.NumbersCounters = numbers;
                }

                ViewBag.NamesHouses = namesHouses;
                ViewBag.Names = names;
                ViewBag.NumbersFlats = numbersFlats;
                ViewBag.Types = types;
                if (indications != null)
                {
                    indications.ForEach(it => it.dateTransfer = DateTime.Parse(it.dateTransfer.ToShortDateString()));
                }

                return View("Index", indications);
            }
            return new NotFoundResult();
        }


        public async Task<IActionResult> UntransmittedReadings(string searchFlat, string name, string type, string searchHouse)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + $"/untransmittedReadings/{UserProperties.GetIdUser(ref context)}");
            List<Counter>? counters = resultCounter.Item as List<Counter>;

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            List<Service>? services = resultService.Item as List<Service>;

            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/admin/{UserProperties.GetIdUser(ref context)}");
            List<Flat>? flats = resultFlat.Item as List<Flat>;

            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
            var houses = resultHouse.Item as List<House>;

            if (resultCounter.StatusCode == 200 && resultService.StatusCode == 200 && resultFlat.StatusCode == 200 && resultHouse.StatusCode == 200)
            {
                if (searchFlat != null && counters != null && flats != null)
                {
                    flats = flats.Where(it => it.flatNumber.ToLower().Contains(searchFlat.ToLower())).ToList();
                    var newCounters = new List<Counter>();
                    foreach(var flat in flats)
                    {
                        newCounters.AddRange(counters.Where(it => it.idFlat == flat.id).ToList());
                    }
                    counters = newCounters;
                }
                if (name != null && name != "" && counters != null && services != null) counters = counters.Where(it => it.idService == services.First(it => it.nameCounter == name).id).ToList();
                if (type == "Индивидуальный прибор учета" && counters != null) counters = counters.Where(it => it.IMDOrGHMD == true).ToList();
                if (type == "Общедомовой прибор учета" && counters != null) counters = counters.Where(it => it.IMDOrGHMD == false).ToList();
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

                List<string> names = new();
                List<Flat?> newFlats = new();
                List<string> types = new();

                //filters
                List<string> typesFilters = new List<string> { "", "Индивидуальный прибор учета", "Общедомовой прибор учета" };
                ViewBag.TypesFilters = new SelectList(typesFilters);
                List<string> statusesFilters = new List<string> { "", "Используется", "Не используется" };
                ViewBag.StatusesFilters = new SelectList(statusesFilters);

                if (services != null)
                {
                    List<string> namesFilters = services.Where(it => it.nameCounter != null).Select(it => it.nameCounter).ToList();
                    namesFilters.Insert(0, "");
                    ViewBag.NamesFilters = new SelectList(namesFilters);
                }
                else return new NotFoundResult();

                var namesHouses = new List<string>();

                if (counters != null && services != null)
                    foreach (var counter in counters)
                    {
                        namesHouses.Add(houses.First(it => it.id == counter.idHouse).name);
                        names.Add(services.First(it => it.id == counter.idService).nameCounter);
                        if (counter.IMDOrGHMD == true) types.Add("Индивидуальный прибор учета");
                        else types.Add("Общедомовой прибор учета");
                        if (counter.idFlat != null && flats != null) newFlats.Add(flats.First(it => it.id == counter.idFlat));
                        else newFlats.Add(null);
                    }

                ViewBag.NamesHouses = namesHouses;
                ViewBag.Names = names;
                ViewBag.Flats = newFlats;
                ViewBag.Types = types;

                ViewBag.NamesHouses = namesHouses;
                ViewBag.Names = names;
                ViewBag.Types = types;

                if (counters != null)
                {
                    counters.ForEach(it => it.dateLastVerification = DateTime.Parse(it.dateLastVerification.ToShortDateString()));
                    counters.ForEach(it => it.dateNextVerification = DateTime.Parse(it.dateNextVerification.ToShortDateString()));
                }

                ViewBag.Period = GetPeriod();

                return View(counters);
            }
            return new NotFoundResult();
        }

        public async Task<IActionResult> SummaryData()
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var result = await ApiGetRequests.LoadListSummaryDataFromAPIAsync(token, Urles.IndicationUrl + $"/summary/{UserProperties.GetIdUser(ref context)}");
            List<SummaryData>? summary = result.Item as List<SummaryData>;

            if(result.StatusCode != 200 || summary == null) return new NotFoundResult();
            ViewBag.Period = GetPeriod();

            return View(summary);
        }

        private static string GetPeriod()
        {
            var dateNow = DateTime.UtcNow;
            int year = dateNow.Year;
            int month = dateNow.Month;
            if (month == 1)
            {
                month = 12;
                year -= 1;
            }
            else month -= 1;
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month) + " " + year.ToString();
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
                {
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
                    return View("SelectFlat", flats);
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectFlat(int? idFlat, int idHouse)
        {
            return RedirectToAction("SelectCounter", new { idFlat = idFlat, idHouse = idHouse });
        }

        public async Task<ActionResult> SelectCounter(int? idFlat, int idHouse)
        {
            var context = HttpContext;
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

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            var resultSettinsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{idHouse}");
            if (resultService.StatusCode == 200 && resultSettinsService.StatusCode == 200)
            {
                List<Service>? services = resultService.Item as List<Service>;
                List<SettingsService>? settingsServices = resultSettinsService.Item as List<SettingsService>;
                if (services != null && settingsServices != null)
                {

                    string url = "";
                    if (idFlat == null) url = $"/house/ghAndUsed/{idHouse}";
                    else url = $"/flat/{idFlat}";

                    var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + url);
                    List<Counter>? counters = resultCounter.Item as List<Counter>;

                    if (idFlat == null) url = $"/lastfromhouse/{idHouse}";
                    else url = $"/lastfromflat/{idFlat}";

                    var resultIndication = await ApiGetRequests.LoadListIndicationsFromAPIAsync(token, Urles.IndicationUrl + url);
                    List<Indication>? indications = resultIndication.Item as List<Indication>;

                    if (resultCounter.StatusCode == 200 && resultIndication.StatusCode == 200)
                    {
                        List<string> names = new();
                        List<string> deadLines = new List<string>();
                        List<bool> can = new List<bool>();
                        var day = DateTime.UtcNow.Day;
                        var newCounters = new List<Counter>();
                        if (counters != null) foreach (var counter in counters)
                            {
                                var settings = settingsServices.First(it => it.idService == counter.idService);
                                if (settings.paymentStatus == true)
                                {
                                    newCounters.Add(counter);
                                    deadLines.Add("С " + settings.startDateTransfer + " по " + settings.endDateTransfer + " число");
                                    names.Add(services.First(it => it.id == counter.idService).nameCounter);
                                    if (day >= settings.startDateTransfer && day <= settings.endDateTransfer && indications.FirstOrDefault(it => it.idCounter == counter.id) == null) can.Add(true);
                                    else can.Add(false);
                                }
                            }

                        ViewBag.IdHouse = idHouse;
                        ViewBag.Names = names;
                        ViewBag.Can = can;
                        ViewBag.DeadLines = deadLines;
                        return View("SelectCounter", newCounters);
                    }
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectCounter(int? idFlat, int idHouse, int idCounter)
        {
            return RedirectToAction("AddInfoCreate", new { idFlat = idFlat, idCounter = idCounter });
        }

        public async Task<ActionResult> AddInfoCreateAsync(int? idFlat, int idCounter)
        {
            var context = HttpContext;
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
            ViewBag.IdCounter = idCounter;

            var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{idCounter}");
            var counter = new Counter();
            if (resultCounter.StatusCode == 200)
                counter = resultCounter.Item as Counter;

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            var resultLast = await ApiGetRequests.LoadIndicationFromAPIAsync(token, Urles.IndicationUrl + $"/lastmonthfromcounter/{idCounter}");

            if (resultService.StatusCode == 200 && resultLast.StatusCode == 200)
            {
                List<Service>? services = resultService.Item as List<Service>;
                Indication? indication = resultLast.Item as Indication;
                if (services != null && counter != null)
                {
                    var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                    if (resultHouse.StatusCode != 200) return new NotFoundResult();
                    House house = resultHouse.Item as House;
                    ViewBag.NameHouse = house.name;

                    ViewBag.Name = services.First(it => it.id == counter.idService).nameCounter;
                    ViewBag.Number = counter.number;
                    ViewBag.DateNow = DateTime.UtcNow.ToShortDateString();
                    if (indication != null) ViewBag.LastIndication = indication.value;
                    return View("AddInfoCreate");
                }

            }
            return new NotFoundResult();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddInfoCreateAsync([Bind("idCounter, value")] Indication indication, int? idFlat)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            if (ModelState.IsValid)
            {
                indication.dateTransfer = DateTime.Parse(DateTime.UtcNow.ToShortDateString());

                var result = await ApiRequests.PostToApiAsync(indication, Urles.IndicationUrl, token);
                if (result.StatusCode == 201) return RedirectToAction("Index");
                else if (result.StatusCode == 500 && result.Message != null) ViewBag.Message = result.Message;
                else ViewBag.Message = "Невозможно отправить данные";
            }

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
            ViewBag.IdCounter = indication.idCounter;

            var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{indication.idCounter}");
            var counter = new Counter();
            if (resultCounter.StatusCode == 200)
                counter = resultCounter.Item as Counter;

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            var resultLast = await ApiGetRequests.LoadIndicationFromAPIAsync(token, Urles.IndicationUrl + $"/lastmonthfromcounter/{indication.idCounter}");

            if (resultService.StatusCode == 200 && resultLast.StatusCode == 200)
            {
                List<Service>? services = resultService.Item as List<Service>;
                Indication? lastindication = resultLast.Item as Indication;
                if (services != null && counter != null)
                {
                    var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                    if (resultHouse.StatusCode != 200) return new NotFoundResult();
                    House house = resultHouse.Item as House;
                    ViewBag.NameHouse = house.name;

                    ViewBag.Name = services.First(it => it.id == counter.idService).nameCounter;
                    ViewBag.Number = counter.number;
                    ViewBag.DateNow = DateTime.UtcNow.ToShortDateString();
                    if (lastindication != null) ViewBag.LastIndication = lastindication.value;
                    return View(indication);
                }

            }
            return new NotFoundResult();
        }

        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);

                var result = await ApiGetRequests.LoadIndicationFromAPIAsync(token, Urles.IndicationUrl + $"/{id}");
                Indication? indication = result.Item as Indication;
                if (result.StatusCode == 200 && indication != null)
                {
                    var dateNow = DateTime.UtcNow;
                    if (indication.dateTransfer.Year == dateNow.Year && indication.dateTransfer.Month == dateNow.Month)
                    {
                        var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{indication.idCounter}");
                        var counter = resultCounter.Item as Counter;

                        if (resultCounter.StatusCode == 200 && counter != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            if (counter.idFlat != null)
                            {
                                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                                Flat? flat = resultFlat.Item as Flat;
                                if (resultFlat.StatusCode == 200 && flat != null)
                                {
                                    ViewBag.FlatNumber = flat.flatNumber;
                                    ViewBag.Type = "Индивидуальный прибор учета";
                                }
                            }
                            else ViewBag.Type = "Общедомовой прибор учета";
                            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
                            var resultLast = await ApiGetRequests.LoadIndicationFromAPIAsync(token, Urles.IndicationUrl + $"/lastmonthfromcounter/{indication.idCounter}");

                            List<Service>? services = resultService.Item as List<Service>;
                            Indication? lastindication = resultLast.Item as Indication;

                            if (resultService.StatusCode == 200 && resultLast.StatusCode == 200 && services != null && counter != null)
                            {
                                ViewBag.Name = services.First(it => it.id == counter.idService).nameCounter;
                                ViewBag.Number = counter.number;
                                ViewBag.Date = indication.dateTransfer.ToShortDateString();
                                if (lastindication != null) ViewBag.LastIndication = lastindication.value;
                                return View(indication);
                            }
                        }
                    }

                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("id, dateTransfer, idCounter, value")] Indication indication)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            if (ModelState.IsValid)
            {
                var result = await ApiRequests.PutToApiAsync(indication, Urles.IndicationUrl + $"/{indication.id}", token);
                if (result.StatusCode == 201)
                    return RedirectToAction("Index");
                else if (result.StatusCode == 500 && result.Message != null) ViewBag.Message = result.Message;
                else ViewBag.Message = "Невозможно отправить данные";

            }
            var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{indication.idCounter}");
            var counter = resultCounter.Item as Counter;

            if (resultCounter.StatusCode == 200 && counter != null)
            {
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House house = resultHouse.Item as House;
                ViewBag.NameHouse = house.name;
                if (counter.idFlat != null)
                {
                    var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                    Flat? flat = resultFlat.Item as Flat;
                    if (resultFlat.StatusCode == 200 && flat != null)
                    {
                        ViewBag.FlatNumber = flat.flatNumber;
                        ViewBag.Type = "Индивидуальный прибор учета";
                    }
                }
                else ViewBag.Type = "Общедомовой прибор учета";

                var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
                var resultLast = await ApiGetRequests.LoadIndicationFromAPIAsync(token, Urles.IndicationUrl + $"/lastmonthfromcounter/{indication.idCounter}");

                List<Service>? services = resultService.Item as List<Service>;
                Indication? lastindication = resultLast.Item as Indication;

                if (resultService.StatusCode == 200 && resultLast.StatusCode == 200 && services != null && counter != null)
                {
                    ViewBag.Name = services.First(it => it.id == counter.idService).nameCounter;
                    ViewBag.Number = counter.number;
                    ViewBag.Date = indication.dateTransfer.ToShortDateString();
                    if (lastindication != null) ViewBag.LastIndication = lastindication.value;
                    return View(indication);

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

                var resultIndication = await ApiGetRequests.LoadIndicationFromAPIAsync(token, Urles.IndicationUrl + $"/{id}");
                var indication = resultIndication.Item as Indication;

                if (resultIndication.StatusCode == 200 && indication != null)
                {
                    var dateNow = DateTime.UtcNow;
                    if (indication.dateTransfer.Year == dateNow.Year && indication.dateTransfer.Month == dateNow.Month)
                    {
                        var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{indication.idCounter}");
                        var counter = resultCounter.Item as Counter;

                        if (resultCounter.StatusCode == 200 && counter != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            if (counter.idFlat != null)
                            {
                                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                                Flat? flat = resultFlat.Item as Flat;
                                if (resultFlat.StatusCode == 200 && flat != null)
                                {
                                    ViewBag.FlatNumber = flat.flatNumber;
                                    ViewBag.Type = "Индивидуальный прибор учета";
                                }
                            }
                            else ViewBag.Type = "Общедомовой прибор учета";

                            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);

                            List<Service>? services = resultService.Item as List<Service>;

                            if (resultService.StatusCode == 200 && services != null && counter != null)
                            {
                                ViewBag.Name = services.First(it => it.id == counter.idService).nameCounter;
                                ViewBag.Number = counter.number;
                                ViewBag.Date = indication.dateTransfer.ToShortDateString();
                                return View(indication);
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
            var result = await ApiRequests.DeleteToApiAsync(Urles.IndicationUrl + $"/{id}", token);
            if (result.StatusCode == 204) return RedirectToAction("Index");
            else
            {
                if (result.StatusCode == 500 && result.Message != null) ViewBag.Message = result.Message;
                var resultIndication = await ApiGetRequests.LoadIndicationFromAPIAsync(token, Urles.IndicationUrl + $"/{id}");
                var indication = resultIndication.Item as Indication;

                if (resultIndication.StatusCode == 200 && indication != null)
                {
                    var dateNow = DateTime.UtcNow;
                    if (indication.dateTransfer.Year == dateNow.Year && indication.dateTransfer.Month == dateNow.Month)
                    {
                        var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{indication.idCounter}");
                        var counter = resultCounter.Item as Counter;

                        if (resultCounter.StatusCode == 200 && counter != null)
                        {
                            var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{counter.idHouse}");
                            if (resultHouse.StatusCode != 200) return new NotFoundResult();
                            House house = resultHouse.Item as House;
                            ViewBag.NameHouse = house.name;
                            if (counter.idFlat != null)
                            {
                                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                                Flat? flat = resultFlat.Item as Flat;
                                if (resultFlat.StatusCode == 200 && flat != null)
                                {
                                    ViewBag.FlatNumber = flat.flatNumber;
                                    ViewBag.Type = "Индивидуальный прибор учета";
                                }
                            }
                            else ViewBag.Type = "Общедомовой прибор учета";

                            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);

                            List<Service>? services = resultService.Item as List<Service>;

                            if (resultService.StatusCode == 200 && services != null && counter != null)
                            {
                                ViewBag.Name = services.First(it => it.id == counter.idService).nameCounter;
                                ViewBag.Number = counter.number;
                                ViewBag.Date = indication.dateTransfer.ToShortDateString();
                                ViewBag.Message = "Невозможно удалить данные";
                                return View(indication);
                            }
                        }
                    }
                }
            }
            return new NotFoundResult();
        }
    }
}