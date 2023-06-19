using HouseManagement.Models;
using HouseManagement.OtherClasses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection;

namespace HouseManagement.Controllers
{

    [Authorize(Roles = "HouseAdmin, FlatOwner")]
    public class PaymentController : Controller
    {
        public async Task<IActionResult> IndexAsync(string searchFlat, string searchPeriod, string name, string searchHouse)
        {
            HttpContext context = HttpContext;
            var token = UserProperties.GetToken(ref context);
            var role = UserProperties.GetRole(ref context);

            var resultService = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
            List<Service>? services = resultService.Item as List<Service>;

            var url = "";
            if (role == "HouseAdmin") url = $"/admin/{UserProperties.GetIdUser(ref context)}";
            if (role == "FlatOwner") url = $"/flatowner/{UserProperties.GetIdFlatOwner(ref context)}";
            var resultFlat = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + url);
            List<Flat>? flats = resultFlat.Item as List<Flat>;


            if (role == "HouseAdmin") url = $"/house/{UserProperties.GetIdHouses(ref context)[0]}";
            if(role == "FlatOwner") url = $"/house/{UserProperties.GetIdHouseFlatOwner(ref context)}";
            var resultSettingService = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + url);
            List<SettingsService> settingsServices = resultSettingService.Item as List<SettingsService>;

            var resultHouse = new ResultsRequest();
            List<House> houses = new();
            if (role == "HouseAdmin")
            {
                url = $"/byuser/{UserProperties.GetIdUser(ref context)}";
                resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + url);
                houses = resultHouse.Item as List<House>;
            }
            if (role == "FlatOwner")
            {
                url = $"/{UserProperties.GetIdHouseFlatOwner(ref context)}";
                resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + url);
                houses.Add(resultHouse.Item as House);
            }
            

            if (role == "HouseAdmin") url = $"/admin/{UserProperties.GetIdUser(ref context)}";
            else url = $"/flatowner/{UserProperties.GetIdFlatOwner(ref context)}";

            var resultPayment = await ApiGetRequests.LoadListPaymentsFromAPIAsync(token, Urles.PaymentUrl + url);
            List<Payment>? payments = resultPayment.Item as List<Payment>;

            if (services != null && settingsServices != null && houses != null)
            {
                List<string> names = new();
                List<string> numbers = new();
                List<string> namesHouses = new();
                settingsServices = settingsServices.Where(it => it.paymentStatus == true).ToList();

                if (flats != null && payments != null)
                {
                    if (searchFlat != null)
                    {
                        flats = flats.Where(it => it.flatNumber.ToLower().Contains(searchFlat.ToLower())).ToList();
                        var newPayments = new List<Payment>();
                        foreach (var flat in flats)
                        {
                            newPayments.AddRange(payments.Where(it => it.idFlat == flat.id).ToList());
                        }
                        payments = newPayments;
                    }

                    if (searchHouse != null)
                    {
                        var housesSearch = houses.Where(it => it.name.ToLower().Contains(searchHouse.ToLower())).ToList();
                        var newFlats = new List<Flat>();
                        foreach (var house in housesSearch)
                        {
                            newFlats.AddRange(flats.Where(it => it.idHouse == house.id));
                        }
                        flats = newFlats;
                        var newPayments = new List<Payment>();
                        foreach (var flat in flats)
                        {
                            newPayments.AddRange(payments.Where(it => it.idFlat == flat.id).ToList());
                        }
                        payments = newPayments;
                    }
                    if (searchPeriod != null) payments = payments.Where(it => it.period.Contains(searchPeriod.ToLower())).ToList();
                    if (name != null && name != "") payments = payments.Where(it => it.idService == services.First(it => it.nameService == name).id).ToList();

                    foreach (var payment in payments)
                    {
                        var flat = flats.First(it => it.id == payment.idFlat);
                        numbers.Add(flats.First(it => it.id == payment.idFlat).flatNumber);
                        names.Add(services.First(it => it.id == payment.idService).nameService);
                        namesHouses.Add(houses.First(it => it.id == flat.idHouse).name);
                    }
                }



                ViewBag.NamesHouses = namesHouses;
                ViewBag.FlatNumbers = numbers;
                ViewBag.Names = names;
                //filters
                List<string> statusesFilters = new List<string> { "", "Оплачено", "Не оплачено" };
                ViewBag.StatusesFilters = new SelectList(statusesFilters);
                var namesFilters = services.Select(it => it.nameService).ToList();
                namesFilters.Insert(0, null);
                ViewBag.NamesFilters = new SelectList(namesFilters);

                if (role == "HouseAdmin")
                    return View("Index", payments);
                else
                    return View("IndexForOwner", payments);
            }
            return new NotFoundResult();
        }
        [Authorize(Roles = "HouseAdmin")]
        public IActionResult Create()
        {
            ViewBag.Period = GetPeriod();
            return View();
        }

        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync()
        {
            var period = GetPeriod();

            var dateNow = DateTime.UtcNow;
            int yearNow = dateNow.Year;
            int monthNow = dateNow.Month;
            int yearLast = dateNow.Year;
            int monthLast = dateNow.Month;
            if (monthNow == 1)
            {
                monthLast = 12;
                yearLast -= 1;
            }
            else monthLast -= 1;

            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            var resultHouse = await ApiGetRequests.LoadListHousesFromAPIAsync(token, Urles.HouseUrl + $"/byuser/{UserProperties.GetIdUser(ref context)}");
            var houses = resultHouse.Item as List<House>;

            List<Payment> payments = new();

            if (resultHouse.StatusCode != 200 || houses == null) return new NotFoundResult();
            foreach (House house in houses)
            {
                var resultsFlats = await ApiGetRequests.LoadListFlatsFromAPIAsync(token, Urles.FlatUrl + $"/house/{house.id}");
                var flats = resultsFlats.Item as List<Flat>;
                var resultsServices = await ApiGetRequests.LoadListServicesFromAPIAsync(token, Urles.ServicetUrl);
                var services = resultsServices.Item as List<Service>;
                var resultsSettingServices = await ApiGetRequests.LoadListSettingsServicesFromAPIAsync(token, Urles.SettingsServiceUrl + $"/house/{house.id}");
                var settingsServices = resultsSettingServices.Item as List<SettingsService>;

                var resultIndicationLastHouse = await ApiGetRequests.LoadListIndicationsFromAPIAsync(token, Urles.IndicationUrl + $"/month/{monthLast}/year/{yearLast}/house/{house.id}");
                var indicationsLastHouse = resultIndicationLastHouse.Item as List<Indication>;
                var resultIndicationNowHouse = await ApiGetRequests.LoadListIndicationsFromAPIAsync(token, Urles.IndicationUrl + $"/month/{monthNow}/year/{yearNow}/house/{house.id}");
                var indicationsNowHouse = resultIndicationNowHouse.Item as List<Indication>;


                if (flats != null && services != null && settingsServices != null)
                {
                    settingsServices = settingsServices.Where(it => it.paymentStatus == true).ToList();
                    foreach (var flat in flats)
                    {
                        var resultIndicationLast = await ApiGetRequests.LoadListIndicationsFromAPIAsync(token, Urles.IndicationUrl + $"/month/{monthLast}/year/{yearLast}/flat/{flat.id}");
                        var indicationsLast = resultIndicationLast.Item as List<Indication>;
                        var resultIndicationNow = await ApiGetRequests.LoadListIndicationsFromAPIAsync(token, Urles.IndicationUrl + $"/month/{monthNow}/year/{yearNow}/flat/{flat.id}");
                        var indicationsNow = resultIndicationNow.Item as List<Indication>;


                        var resultUsableArea = await ApiGetRequests.LoadStringAPIAsync(token, Urles.FlatUrl + $"/house/usablearea/{house.id}");
                        var amountUsableAreaString = resultUsableArea.Item as string;
                        decimal amountUsableArea = 0;
                        if (amountUsableAreaString != null) amountUsableArea = Convert.ToDecimal(amountUsableAreaString.Replace('.', ','));

                        var resultTotalArea = await ApiGetRequests.LoadStringAPIAsync(token, Urles.FlatUrl + $"/house/totalarea/{house.id}");
                        var amountTotalAreaString = resultTotalArea.Item as string;
                        decimal amountTotalArea = 0;
                        if (amountTotalAreaString != null) amountTotalArea = Convert.ToDecimal(amountTotalAreaString.Replace('.', ','));

                        var resultPeople = await ApiGetRequests.LoadStringAPIAsync(token, Urles.FlatUrl + $"/house/people/{house.id}");
                        var allPeopleString = resultPeople.Item as string;
                        int allPeople = 0;
                        if (allPeopleString != null) allPeople = Convert.ToInt32(allPeopleString);

                        foreach (var settingService in settingsServices)
                        {
                            if (settingService.paymentPeriod.Split(',').Contains(monthLast.ToString()))
                            {
                                decimal? amountWithInd = 0;
                                decimal? amountWithNoInd = 0;
                                var service = services.Find(it => it.id == settingService.idService);
                                int? countPeople = 0;
                                if (flat.numberOfRegisteredResidents != 0) countPeople = flat.numberOfRegisteredResidents;
                                else countPeople = flat.numberOfOwners;

                                var payment = new Payment();
                                payment.idFlat = flat.id;
                                payment.idService = settingService.idService;
                                payment.penalties = "0";
                                payment.period = period;

                                if (settingService.haveCounter == true && settingService.typeIMD == true)
                                {
                                    var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + $"/flat/{flat.id}");
                                    var counters = resultCounter.Item as List<Counter>;
                                    int countCounters = 0;
                                    var counterService = counters.Where(it => it.idService == settingService.idService).ToList();
                                    if (counterService.Count != 0)
                                    {
                                        countCounters = counterService.Count;
                                        var indicationService = new List<Indication>();
                                        foreach (var counter in counterService)
                                        {
                                            var last = indicationsLast.FirstOrDefault(it => it.idCounter == counter.id);
                                            var now = indicationsNow.FirstOrDefault(it => it.idCounter == counter.id);
                                            if (last != null && now != null)
                                            {
                                                amountWithInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Math.Abs(Convert.ToDecimal(now.value - last.value));
                                            }
                                            else
                                            {
                                                if (service.nameService != "Отопление")
                                                {
                                                    amountWithNoInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Convert.ToDecimal(settingService.valueNormative.Replace('.', ',')) * countPeople;
                                                }
                                                else
                                                {
                                                    amountWithNoInd += Convert.ToDecimal(settingService.valueRate) * Convert.ToDecimal(flat.usableArea.Replace('.', ','));
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (service.nameService != "Отопление")
                                        {
                                            amountWithNoInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Convert.ToDecimal(settingService.valueNormative.Replace('.', ',')) * countPeople;
                                        }
                                        else
                                        {
                                            amountWithNoInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Convert.ToDecimal(flat.usableArea.Replace('.', ','));
                                        }
                                    }

                                    if (countCounters != 0) payment.amount = Convert.ToDouble(amountWithInd + (amountWithNoInd / countCounters)).ToString();
                                    else payment.amount = Convert.ToDouble(amountWithNoInd).ToString();
                                }
                                else if (settingService.haveCounter == true && settingService.typeIMD == false)
                                {
                                    var resultCounter = await ApiGetRequests.LoadListCountersFromAPIAsync(token, Urles.CounterUrl + $"/house/ghAndUsed/{house.id}");
                                    var counters = resultCounter.Item as List<Counter>;
                                    int countCounters = 0;
                                    var counterService = counters.Where(it => it.idService == settingService.idService).ToList();
                                    countCounters = counterService.Count;
                                    if (countCounters != 0)
                                    {
                                        var indicationService = new List<Indication>();
                                        foreach (var counter in counterService)
                                        {
                                            var last = indicationsLastHouse.FirstOrDefault(it => it.idCounter == counter.id);
                                            var now = indicationsNowHouse.FirstOrDefault(it => it.idCounter == counter.id);
                                            if (last != null && now != null)
                                            {
                                                amountWithInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Math.Abs(Convert.ToDecimal(now.value - last.value));
                                            }
                                            else
                                            {
                                                if (service.nameService != "Отопление")
                                                {
                                                    amountWithNoInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Convert.ToDecimal(settingService.valueNormative.Replace('.', ',')) * allPeople;
                                                }
                                                else
                                                {
                                                    amountWithNoInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Convert.ToDecimal(amountUsableArea);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (service.nameService != "Отопление")
                                        {
                                            amountWithNoInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Convert.ToDecimal(settingService.valueNormative.Replace('.', ',')) * allPeople;
                                        }
                                        else
                                        {
                                            amountWithNoInd += Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * Convert.ToDecimal(amountUsableArea);
                                        }
                                    }

                                    if (countCounters != 0) payment.amount = (Convert.ToDouble(amountWithInd + (amountWithNoInd / countCounters)) / flats.Count).ToString();
                                    else payment.amount = (Convert.ToDouble(amountWithNoInd) / flats.Count).ToString();
                                }
                                else
                                {
                                    if (service.nameService == "Холодное водоснабжение" || service.nameService == "Горячее водоснабжение" || service.nameService == "Электроэнергия" ||
                                        service.nameService == "Газ" || service.nameService == "Водоотведение")
                                    {
                                        payment.amount = Convert.ToDouble(Convert.ToDouble(settingService.valueRate.Replace('.', ',')) * Convert.ToDouble(settingService.valueNormative.Replace('.', ',')) * countPeople).ToString();
                                    }
                                    if (service.nameService == "Отопление")
                                    {
                                        payment.amount = (Convert.ToDouble(Convert.ToDouble(settingService.valueRate.Replace('.', ',')) * Convert.ToDouble(amountUsableArea)) / flats.Count).ToString();
                                    }
                                    if (service.nameService == "Кап. ремонт")
                                    {
                                        payment.amount = (Convert.ToDouble(Convert.ToDecimal(settingService.valueRate.Replace('.', ',')) * amountTotalArea) / flats.Count).ToString();
                                    }
                                    if (service.nameService == "Домофон" || service.nameService == "Лифт" || service.nameService == "Вывоз мусора")
                                    {
                                        payment.amount = settingService.valueRate;
                                    }
                                }

                                payment.amount = Math.Round(Convert.ToDouble(payment.amount), 2).ToString();
                                payments.Add(payment);

                            }
                        }
                    }
                }
            }

            payments.ForEach(it => it.amount = it.amount.Replace(',', '.'));
            var result = await ApiRequests.PostToApiAsync(payments, Urles.PaymentUrl, token);
            if (result.StatusCode == 200) return RedirectToAction("Index");
            else if (result.StatusCode == 500 && result.Message != null)
            {
                ViewBag.Message = result.Message;
            }
            else ViewBag.Message = "Невозможно отправить данные";
            ViewBag.Period = period;

            return View();
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
        [Authorize(Roles = "HouseAdmin")]
        public async Task<ActionResult> EditAsync(int? id)
        {
            if (id != null)
            {
                var context = HttpContext;
                var token = UserProperties.GetToken(ref context);

                var resultPayment = await ApiGetRequests.LoadPaymentFromAPIAsync(token, Urles.PaymentUrl + $"/{id}");
                var payment = resultPayment.Item as Payment;

                if (payment != null)
                {
                    var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{payment.idService}");
                    var service = resultService.Item as Service;

                    var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{payment.idFlat}");
                    var flat = resultFlat.Item as Flat;

                    if (service != null && flat != null)
                    {
                        var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                        if (resultHouse.StatusCode != 200) return new NotFoundResult();
                        House house = resultHouse.Item as House;
                        ViewBag.NameHouse = house.name;
                        ViewBag.NameService = service.nameService;
                        ViewBag.NumberFlat = flat.flatNumber;
                        ViewBag.Period = payment.period;
                        return View(payment);
                    }
                }
            }
            return new NotFoundResult();
        }
        [Authorize(Roles = "HouseAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAsync([Bind("id, idService, idFlat, period, amount, penalties, status")] Payment payment)
        {
            var context = HttpContext;
            var token = UserProperties.GetToken(ref context);

            if (ModelState.IsValid)
            {
                var result = await ApiRequests.PutToApiAsync(payment, Urles.PaymentUrl + $"/{payment.id}", token);
                if (result.StatusCode == 201)
                    return RedirectToAction("Index");
                else ViewBag.Message = "Невозможно сохранить данные";
            }
            var resultService = await ApiGetRequests.LoadServiceFromAPIAsync(token, Urles.ServicetUrl + $"/{payment.idService}");
            var service = resultService.Item as Service;

            var resultFlat = await ApiGetRequests.LoadFlatFromAPIAsync(token, Urles.FlatUrl + $"/{payment.idFlat}");
            var flat = resultFlat.Item as Flat;

            if (service != null && flat != null)
            {
                var resultHouse = await ApiGetRequests.LoadHouseFromAPIAsync(token, Urles.HouseUrl + $"/{flat.idHouse}");
                if (resultHouse.StatusCode != 200) return new NotFoundResult();
                House house = resultHouse.Item as House;
                ViewBag.NameHouse = house.name;
                ViewBag.NameService = service.nameService;
                ViewBag.NumberFlat = flat.flatNumber;
                ViewBag.Period = payment.period;
                return View(payment);
            }

            return new NotFoundResult();
        }
    }
}