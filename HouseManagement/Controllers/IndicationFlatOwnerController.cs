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

    [Authorize(Roles = "FlatOwner")]
    public class IndicationFlatOwnerController : Controller
    {
        public async Task<IActionResult> IndexAsync(string searchFlat, string name)
        {
            HttpContext context = HttpContext;
            var idFlatOwner = UserProperties.GetIdFlatOwner(ref context);
            var idHouse = UserProperties.GetIdHouseFlatOwner(ref context);
            var token = UserProperties.GetToken(ref context);

            var resultIndication = await ApiGetRequests.LoadListIndicationsFromAPIAsync(token, Urles.IndicationUrl + $"/flatowner/{idFlatOwner}");
            List<Indication>? indications = resultIndication.Item as List<Indication>;

            var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + $"/flatowner/{idFlatOwner}");
            List<Counter>? counters = resultCounter.Item as List<Counter>;

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            List<Service>? services = resultService.Item as List<Service>;

            var resultSettingsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{idHouse}");
            List<SettingsService>? settingsServices = resultSettingsService.Item as List<SettingsService>;

            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/flatowner/{idFlatOwner}");
            List<Flat>? flats = resultFlat.Item as List<Flat>;

            if (resultIndication.StatusCode == 200 && resultCounter.StatusCode == 200 && resultService.StatusCode == 200 && resultFlat.StatusCode == 200 && resultSettingsService.StatusCode == 200)
            {
                List<string> names = new();
                List<string> numbersFlats = new();

                if (searchFlat != null && counters != null && indications != null && flats != null)
                {
                    var flat = flats.FirstOrDefault(it => it.flatNumber == searchFlat);
                    if (flat != null) counters = counters.Where(it => it.idFlat == flat.id).ToList();
                }
                if (name != null && name != "" && counters != null && services != null && indications != null)
                {
                    counters = counters.Where(it => it.idService == services.First(it => it.nameService == name).id).ToList();
                }

                var newIndications = new List<Indication>();
                if (counters != null && indications != null) foreach (var counter in counters)
                        newIndications.AddRange(indications.Where(it => it.idCounter == counter.id));
                indications = newIndications;

                if (services != null)
                {
                    List<string> namesFilters = services.Where(it => it.nameCounter != null).Select(it => it.nameService).ToList();
                    namesFilters.Insert(0, "");
                    ViewBag.NamesFilters = new SelectList(namesFilters);
                }
                else return new NotFoundResult();

                if (counters != null && indications != null && settingsServices != null && services != null)
                {
                    indications = indications.OrderByDescending(it => it.dateTransfer).ToList();
                    List<bool> can = new();
                    List<string> numbers = new();
                    var dateNow = DateTime.UtcNow;
                    foreach (var indication in indications)
                    {
                        var counter = counters.First(it => it.id == indication.idCounter);
                        var settinService = settingsServices.First(it => it.idService == counter.idService);
                        names.Add(services.First(it => it.id == counter.idService).nameService);
                        numbersFlats.Add(flats.First(it => it.id == counter.idFlat).flatNumber);
                        if (indication.dateTransfer.Year == dateNow.Year && indication.dateTransfer.Month == dateNow.Month && dateNow.Day >= settinService.startDateTransfer && dateNow.Day <= settinService.endDateTransfer) can.Add(true);
                        else can.Add(false);

                        numbers.Add(counter.number);
                    }
                    ViewBag.Can = can;
                    ViewBag.NumbersCounters = numbers;
                }

                ViewBag.Names = names;
                ViewBag.NumbersFlats = numbersFlats;
                if (indications != null)
                {
                    indications.ForEach(it => it.dateTransfer = DateTime.Parse(it.dateTransfer.ToShortDateString()));
                }

                return View("Index", indications);
            }
            return new NotFoundResult();
        }


        public async Task<ActionResult> SelectFlat(string search)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var idFlatOwner = UserProperties.GetIdFlatOwner(ref context);
            var idHouse = (int)UserProperties.GetIdHouseFlatOwner(ref context);
            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/flatowner/{idFlatOwner}");
            if (resultFlat.StatusCode == 200)
            {
                List<Flat>? flats = resultFlat.Item as List<Flat>;
                {
                    if (flats != null && search != null)
                    {
                        flats = flats.Where(it => it.flatNumber.ToLower().Contains(search.ToLower())).ToList();
                    }

                    return View("SelectFlat", flats);
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SelectFlat(int idFlat)
        {
            var context = HttpContext;
            var idFlatOwner = UserProperties.GetIdFlatOwner(ref context);
            var idHouse = (int)UserProperties.GetIdHouseFlatOwner(ref context);
            var token = UserProperties.GetToken(ref context);
            var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{idFlat}");
            if (resultFlat.StatusCode == 200)
            {
                Flat? flat = resultFlat.Item as Flat;
                ViewBag.Flat = flat;
                ViewBag.Type = "Индивидуальный прибор учета";

                var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
                var resultSettinsService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{idHouse}");
                if (resultService.StatusCode == 200 && resultSettinsService.StatusCode == 200)
                {
                    List<Service>? services = resultService.Item as List<Service>;
                    List<SettingsService>? settingsServices = resultSettinsService.Item as List<SettingsService>;
                    if (services != null && settingsServices != null)
                    {
                        string url = $"/flat/{idFlat}";

                        var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + url);
                        List<Counter>? counters = resultCounter.Item as List<Counter>;

                        url = $"/lastfromflat/{idFlat}";

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

                            ViewBag.Names = names;
                            ViewBag.Can = can;
                            ViewBag.DeadLines = deadLines;
                            return View("SelectCounter", newCounters);
                        }
                    }
                }
            }
            return new NotFoundResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SelectCounter(int idFlat, int idCounter)
        {
            var context = HttpContext;
            var idHouse = (int)UserProperties.GetIdHouseFlatOwner(ref context);
            var token = UserProperties.GetToken(ref context);
            var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{idFlat}");
            if (resultFlat.StatusCode == 200)
            {
                Flat? flat = resultFlat.Item as Flat;
                ViewBag.Flat = flat;
                ViewBag.Type = "Индивидуальный прибор учета";
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
                        ViewBag.Name = services.First(it => it.id == counter.idService).nameCounter;
                        ViewBag.Number = counter.number;
                        ViewBag.DateNow = DateTime.UtcNow.ToShortDateString();
                        if (indication != null) ViewBag.LastIndication = indication.value;
                        return View("AddInfoCreate");
                    }

                }
            }
            return new NotFoundResult();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddInfoCreateAsync([Bind("idCounter, value")] Indication indication, int idFlat)
        {
            var context = HttpContext;
            var idHouse = (int)UserProperties.GetIdHouseFlatOwner(ref context);
            var token = UserProperties.GetToken(ref context);
            if (ModelState.IsValid)
            {
                indication.dateTransfer = DateTime.Parse(DateTime.UtcNow.ToShortDateString());

                var result = await ApiRequests.PostToApiAsync(indication, Urles.IndicationUrl + "/fromOwner", token);
                if (result.StatusCode == 201) return RedirectToAction("Index");
                else if (result.StatusCode == 500 && result.Message != null)
                {
                    ViewBag.Message = result.Message;
                }
                else ViewBag.Message = "Невозможно отправить данные";
            }

            var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{idFlat}");
            if (resultFlat.StatusCode == 200)
            {
                Flat? flat = resultFlat.Item as Flat;
                ViewBag.Flat = flat;
                ViewBag.Type = "Индивидуальный прибор учета";

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
                        ViewBag.Name = services.First(it => it.id == counter.idService).nameCounter;
                        ViewBag.Number = counter.number;
                        ViewBag.DateNow = DateTime.UtcNow.ToShortDateString();
                        if (lastindication != null) ViewBag.LastIndication = lastindication.value;
                        return View(indication);
                    }

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
                            var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                            Flat? flat = resultFlat.Item as Flat;
                            var flats = UserProperties.GetIdFlats(ref context);
                            if (resultFlat.StatusCode == 200 && flat != null)
                            {
                                if (flats.Contains(flat.id))
                                {
                                    ViewBag.FlatNumber = flat.flatNumber;
                                    ViewBag.Type = "Индивидуальный прибор учета";
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
                var result = await ApiRequests.PutToApiAsync(indication, Urles.IndicationUrl + $"/fromOwner/{indication.id}", token);
                if (result.StatusCode == 201)
                    return RedirectToAction("Index");
                else if (result.StatusCode == 500 && result.Message != null)
                {
                    ViewBag.Message = result.Message;
                }
                else ViewBag.Message = "Невозможно отправить данные";

            }
            var resultCounter = await ApiGetRequests.LoadCounterFromAPIAsync(token, Urles.CounterUrl + $"/{indication.idCounter}");
            var counter = resultCounter.Item as Counter;

            if (resultCounter.StatusCode == 200 && counter != null)
            {
                var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{counter.idFlat}");
                Flat? flat = resultFlat.Item as Flat;
                if (resultFlat.StatusCode == 200 && flat != null)
                {
                    var flats = UserProperties.GetIdFlats(ref context);
                    if (flats.Contains(flat.id))
                    {
                        ViewBag.FlatNumber = flat.flatNumber;
                        ViewBag.Type = "Индивидуальный прибор учета";
                    }

                }

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