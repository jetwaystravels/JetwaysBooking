using CoporateBooking.Comman;
using DomainLayer.Model;
using DomainLayer.ViewModel;
using Indigo;
using IndigoBookingManager_;
using Microsoft.AspNetCore.Mvc;
using Nancy.Json;
using Newtonsoft.Json;
using OnionArchitectureAPI.Services.Indigo;
using OnionConsumeWebAPI.Controllers;
using OnionConsumeWebAPI.Controllers.Indigo;
using OnionConsumeWebAPI.Extensions;
using Spicejet;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using Utility;
using Sessionmanager;
using Bookingmanager_;
using static DomainLayer.Model.ReturnTicketBooking;
using SpicejetBookingManager_;

namespace CoporateBooking.Controllers.common
{
    public class CancelBookingController : Controller
    {
        private _credentials _credentialsAirasia = null;

        public async Task<IActionResult> CancelActionAsync(int airline, string pnr, List<string> passengerKeys, string cancellationType)
        {

            if (airline == 1) // AirAsia
            {
                if (cancellationType == "complete")
                {
                    return await CancelCompleteAirAsiaBooking(pnr);
                }
                else if (cancellationType == "partial")
                {
                    return await CancelPartialAirAsiaBooking(pnr, passengerKeys);
                }
            }
            else if (airline == 2) // Air India Express or other
            {
                if (cancellationType == "complete")
                {
                    return await CancelCompleteAkasaBooking(pnr);
                }
                else if (cancellationType == "partial")
                {
                    return await CancelPartialAkasaBooking(pnr, passengerKeys);
                }
            }
            else if (airline == 3) // Indigo
            {
                if (cancellationType == "complete")
                {
                    return await CancelCompleteSpicejetBooking(pnr);
                }
                else if (cancellationType == "partial")
                {
                    return await CancelPartialOtherAirline(pnr, passengerKeys);
                }
            }
            else if (airline == 4) // Indigo
            {
                if (cancellationType == "complete")
                {
                    return await CancelCompleteIndigoBooking(pnr);
                }
                else if (cancellationType == "partial")
                {
                    return await CancelPartialOtherAirline(pnr, passengerKeys);
                }
            }

            ModelState.AddModelError("", "Unsupported airline or invalid cancellation type.");
            return View("Error");

        }

        private async Task<IActionResult> CancelCompleteAirAsiaBooking(string pnr)
        {
            using (HttpClient client = new HttpClient())
            {
                //client.BaseAddress = new Uri(AppUrlConstant.BaseURL);

                // 1. Get AirAsia credentials
                //HttpResponseMessage response = await client.GetAsync(AppUrlConstant.AirlineLogin);
                client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);

                // 1. Get Indigo credentials
                var url = $"{AppUrlConstant.Getsuppliercred}?flightclass={Uri.EscapeDataString("Corporate")}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve airline credentials.");
                    return View("Error");
                }

                var results = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);
                var _credentialsAirasia = jsonObject.FirstOrDefault(cred => cred?.FlightCode == 1); // AirAsia

                // 2. Login and get token
                var login = new airlineLogin { credentials = _credentialsAirasia };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage tokenResponse = await client.PostAsJsonAsync(AppUrlConstant.AirasiaTokan, login);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "AirAsia token request failed.");
                    return View("Error");
                }

                var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
                dynamic tokenJson = JsonConvert.DeserializeObject<dynamic>(tokenResult);
                string token = tokenJson.data.token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 3. Retrieve booking
                var retrieveUrl = $"{AppUrlConstant.AirasiaPNRBooking}/{pnr}";
                var retrieveResponse = await client.GetAsync(retrieveUrl);
                if (!retrieveResponse.IsSuccessStatusCode)
                {
                    var err = await retrieveResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Booking retrieval failed: {err}");
                    return View("Error");
                }

                // 4. Delete journey
                var deleteResult = await HttpHelperService.SendDeleteAsync(client, $"{AppUrlConstant.DeleteBooking}");
                if (!deleteResult.Success)
                {
                    ModelState.AddModelError("", $"DELETE failed: {deleteResult.Error}");
                    return View("Error");
                }

                // 5. PUT commit
                var putResult = await HttpHelperService.SendPutAsync(client, $"{AppUrlConstant.AirasiaCommitBooking}");
                if (!putResult.Success)
                {
                    ModelState.AddModelError("", $"PUT failed: {putResult.Error}");
                    return View("Error");
                }

                // 6. Final GET booking
                var finalGet = await client.GetAsync($"{AppUrlConstant.AirasiaGetBoking}");
                if (!finalGet.IsSuccessStatusCode)
                {
                    var error = await finalGet.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Final booking status check failed. {error}");
                    return View("Error");
                }

                var _finalStatus = await finalGet.Content.ReadAsStringAsync();
                dynamic objcancelBooking = JsonConvert.DeserializeObject<dynamic>(_finalStatus);
                decimal balanceDue = objcancelBooking.data.breakdown.balanceDue;
                decimal totalAmount = objcancelBooking.data.breakdown.totalAmount;

                var email = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                Logs logs = new Logs();
                logs.WriteLogs(_finalStatus, "Cancel", "AirAsiaOneWay", "oneway");

                var cancelRequest = new
                {
                    RecordLocator = pnr,
                    Status = 3,
                    UserEmail = email,
                    BalanceDue = balanceDue,
                    TotalAmount = totalAmount
                };

                string jsonPayload = JsonConvert.SerializeObject(cancelRequest);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var responsecancel = await client.PostAsync(AppUrlConstant.CancleStatus, content);

                if (!responsecancel.IsSuccessStatusCode)
                {
                    string err = await responsecancel.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Cancellation status update failed: {err}");
                    return View("Error");
                }

                TempData["Success"] = "Booking cancellation session flow completed successfully.";
                TempData["FinalStatus"] = _finalStatus;

                string Message = "Booking cancellation completed successfully." + totalAmount;

                return RedirectToAction("MyBooking", "Booking", new { Mess = Message });
            }
        }

        private async Task<IActionResult> CancelCompleteAkasaBooking(string pnr)
        {
            using (HttpClient client = new HttpClient())
            {
                // 1. Get Akasha credentials
                client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);
                var url = $"{AppUrlConstant.Getsuppliercred}?flightclass={Uri.EscapeDataString("Corporate")}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve airline credentials.");
                    return View("Error");
                }

                var results = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);
                var _credentialsAkasa = jsonObject.FirstOrDefault(cred => cred?.FlightCode == 2); // Akasha

                // 2. Login and get token
                var login = new airlineLogin { credentials = _credentialsAkasa };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage tokenResponse = await client.PostAsJsonAsync(AppUrlConstant.AkasaTokan, login);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Akasa token request failed.");
                    return View("Error");
                }

                var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
                dynamic tokenJson = JsonConvert.DeserializeObject<dynamic>(tokenResult);
                string token = tokenJson.data.token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 3. Retrieve booking
                var retrieveUrl = $"{AppUrlConstant.AkasaAirPNRBooking}/{pnr}";
                var retrieveResponse = await client.GetAsync(retrieveUrl);
                if (!retrieveResponse.IsSuccessStatusCode)
                {
                    var err = await retrieveResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Booking retrieval failed: {err}");
                    return View("Error");
                }

                // 4. Delete journey
                var deleteResult = await HttpHelperService.SendDeleteAsync(client, $"{AppUrlConstant.AkasaDeleteBooking}");
                if (!deleteResult.Success)
                {
                    ModelState.AddModelError("", $"DELETE failed: {deleteResult.Error}");
                    return View("Error");
                }

                // 5. PUT commit
                var putResult = await HttpHelperService.SendPutAsync(client, $"{AppUrlConstant.AkasaAirCommitBooking}");
                if (!putResult.Success)
                {
                    ModelState.AddModelError("", $"PUT failed: {putResult.Error}");
                    return View("Error");
                }

                // 6. Final GET booking
                var finalGet = await client.GetAsync($"{AppUrlConstant.AkasaAirGetBooking}");
                if (!finalGet.IsSuccessStatusCode)
                {
                    var error = await finalGet.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Final booking status check failed. {error}");
                    return View("Error");
                }

                var _finalStatus = await finalGet.Content.ReadAsStringAsync();
                dynamic objcancelBooking = JsonConvert.DeserializeObject<dynamic>(_finalStatus);
                decimal balanceDue = objcancelBooking.data.breakdown.balanceDue;
                decimal totalAmount = objcancelBooking.data.breakdown.totalAmount;

                var email = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                Logs logs = new Logs();
                logs.WriteLogs(_finalStatus, "Cancel", "AkasaOneWay", "oneway");

                var cancelRequest = new
                {
                    RecordLocator = pnr,
                    Status = 3,
                    UserEmail = email,
                    BalanceDue = balanceDue,
                    TotalAmount = totalAmount
                };

                string jsonPayload = JsonConvert.SerializeObject(cancelRequest);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var responsecancel = await client.PostAsync(AppUrlConstant.CancleStatus, content);

                if (!responsecancel.IsSuccessStatusCode)
                {
                    string err = await responsecancel.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Cancellation status update failed: {err}");
                    return View("Error");
                }

                TempData["Success"] = "Booking cancellation session flow completed successfully.";
                TempData["FinalStatus"] = _finalStatus;

                string Message = "Booking cancellation completed successfully." + totalAmount;

                return RedirectToAction("MyBooking", "Booking", new { Mess = Message });
            }
        }
        private async Task<IActionResult> CancelCompleteIndigoBooking(string pnr)
        {
            string token = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                IndigoSessionmanager_.LogonRequest _logonRequestIndigoobj = new IndigoSessionmanager_.LogonRequest();
                IndigoSessionmanager_.LogonRequestData LogonRequestDataIndigoobj = new IndigoSessionmanager_.LogonRequestData();

                client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);

                // 1. Get Indigo credentials
                var url = $"{AppUrlConstant.Getsuppliercred}?flightclass={Uri.EscapeDataString("Corporate")}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve airline credentials.");
                    return View("Error");
                }

                var results = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);
                var _CredentialsIndigo = jsonObject.FirstOrDefault(cred => cred?.FlightCode == 4); // Indigo
                // 2. Login and get token
                #region Logon 
                _login obj_ = new _login();

                LogonRequestDataIndigoobj.AgentName = _CredentialsIndigo.username;
                LogonRequestDataIndigoobj.Password = _CredentialsIndigo.password;
                LogonRequestDataIndigoobj.DomainCode = _CredentialsIndigo.domain;
                _logonRequestIndigoobj.logonRequestData = LogonRequestDataIndigoobj;

                _getapiIndigo objIndigo = new _getapiIndigo();
                IndigoSessionmanager_.LogonResponse _IndigologonResponseobj = await objIndigo.Signature(_logonRequestIndigoobj);


                if (_IndigologonResponseobj != null && _IndigologonResponseobj.Signature != null)
                {
                    token = _IndigologonResponseobj.Signature;
                }
                else
                {
                    ModelState.AddModelError("", "AirAsia token request failed.");
                    return View("Error");
                }
                #endregion

                // 3. Retrieve booking
                _commit objcommit = new _commit();
                IndigoBookingManager_.BookingCommitResponse _BookingCommitResponse = new IndigoBookingManager_.BookingCommitResponse();
                IndigoBookingManager_.GetBookingResponse _getBookingResponse = null;
                _BookingCommitResponse.BookingUpdateResponseData = new IndigoBookingManager_.BookingUpdateResponseData();
                _BookingCommitResponse.BookingUpdateResponseData.Success = new IndigoBookingManager_.Success();
                _BookingCommitResponse.BookingUpdateResponseData.Success.RecordLocator = pnr;
                _getBookingResponse = await objcommit.GetBookingdetails(token, _BookingCommitResponse, "OneWay");

                // 4. cancel journey
                IndigoBookingManager_.CancelRequest _cancelRQ = new IndigoBookingManager_.CancelRequest();
                IndigoBookingManager_.CancelResponse _cancelRes = new IndigoBookingManager_.CancelResponse();
                _cancelRes = await objcommit.CancelJourney(token, _cancelRQ, "OneWay");

                decimal balanceDue = _cancelRes.BookingUpdateResponseData.Success.PNRAmount.BalanceDue;
                decimal totalAmount = _cancelRes.BookingUpdateResponseData.Success.PNRAmount.TotalCost;

                // 5. Add payment journey
                IndigoBookingManager_.AddPaymentToBookingResponse _BookingPaymentResponse = await objcommit.AddpaymenttoBook(token, balanceDue, "OneWay");
                if (_BookingPaymentResponse.BookingPaymentResponse.ValidationPayment.PaymentValidationErrors.Length > 0 && _BookingPaymentResponse.BookingPaymentResponse.ValidationPayment.PaymentValidationErrors[0].ErrorDescription.ToLower().Contains("not enough funds available"))
                {
                    //_AirLinePNRTicket.ErrorDesc = "Not enough funds available.";
                }


                //if (!responsecancel.IsSuccessStatusCode)
                //{
                //    string err = await responsecancel.Content.ReadAsStringAsync();
                //    ModelState.AddModelError("", $"Cancellation status update failed: {err}");
                //    return View("Error");
                //}
                // 6. Commit journey
                //_getBookingResponse
                IndigoBookingManager_.UpdateContactsRequest contactList = new IndigoBookingManager_.UpdateContactsRequest();
                List<passkeytype> passeengerlist = new List<passkeytype>();
                contactList.updateContactsRequestData = new IndigoBookingManager_.UpdateContactsRequestData();
                contactList.updateContactsRequestData.BookingContactList = new IndigoBookingManager_.BookingContact[1];
                contactList.updateContactsRequestData.BookingContactList[0] = new IndigoBookingManager_.BookingContact();
                contactList.updateContactsRequestData.BookingContactList[0].EmailAddress = _getBookingResponse.Booking.BookingContacts[0].EmailAddress;
                contactList.updateContactsRequestData.BookingContactList[0].HomePhone = _getBookingResponse.Booking.BookingContacts[0].HomePhone;
                passkeytype objpax = new passkeytype();
                objpax.first = _getBookingResponse.Booking.Passengers[0].Names[0].FirstName;
                objpax.middle = _getBookingResponse.Booking.Passengers[0].Names[0].MiddleName;
                objpax.last = _getBookingResponse.Booking.Passengers[0].Names[0].LastName;
                objpax.title = _getBookingResponse.Booking.Passengers[0].Names[0].Title;
                objpax.identifier = _getBookingResponse.Booking.PaxCount.ToString();
                passeengerlist.Add(objpax);

                _BookingCommitResponse = await objcommit.commitOnCancel(token, pnr, contactList, passeengerlist, "OneWay");

                _getBookingResponse = await objcommit.GetBookingdetails(token, _BookingCommitResponse, "OneWay");

                var email = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                Logs logs = new Logs();
                logs.WriteLogs(JsonConvert.SerializeObject(_cancelRes), "Cancel", "Indigooneway", "oneway");


                // 7.LogOut 
                IndigoSessionmanager_.LogoutRequest _logoutRequestobj = new IndigoSessionmanager_.LogoutRequest();
                IndigoSessionmanager_.LogoutResponse _logoutResponse = new IndigoSessionmanager_.LogoutResponse();
                _logoutRequestobj.ContractVersion = 456;
                _logoutRequestobj.Signature = token;
                _logoutResponse = await objIndigo.Logout(_logoutRequestobj);

                balanceDue = _getBookingResponse.Booking.BookingSum.BalanceDue;
                totalAmount = _getBookingResponse.Booking.BookingSum.TotalCost;

                var cancelRequest = new
                {
                    RecordLocator = pnr,
                    Status = 3,
                    UserEmail = email,
                    BalanceDue = balanceDue,
                    TotalAmount = totalAmount
                };

                string jsonPayload = JsonConvert.SerializeObject(cancelRequest);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var responsecancel = await client.PostAsync(AppUrlConstant.CancleStatus, content);

                if (!responsecancel.IsSuccessStatusCode)
                {
                    string err = await responsecancel.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Cancellation status update failed: {err}");
                    return View("Error");
                }

                TempData["Success"] = "Booking cancellation session flow completed successfully.";
                TempData["FinalStatus"] = JsonConvert.SerializeObject(_BookingCommitResponse);

                string Message = "Booking cancellation completed successfully." + totalAmount;

                return RedirectToAction("MyBooking", "Booking", new { Mess = Message });
            }
        }


        private async Task<IActionResult> CancelCompleteSpicejetBooking(string pnr)
        {
            string token = string.Empty;
            using (HttpClient client = new HttpClient())
            {
                SpicejetSessionManager_.LogonRequest _logonRequestobj = new SpicejetSessionManager_.LogonRequest();
                SpicejetSessionManager_.LogonRequestData LogonRequestDataobj = new SpicejetSessionManager_.LogonRequestData();

                client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);

                // 1. Get Indigo credentials
                var url = $"{AppUrlConstant.Getsuppliercred}?flightclass={Uri.EscapeDataString("Corporate")}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve airline credentials.");
                    return View("Error");
                }

                var results = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);
                var _CredentialsSpicejet = jsonObject.FirstOrDefault(cred => cred?.FlightCode == 3); // Spicejet
                // 2. Login and get token
                #region Logon 

                LogonRequestDataobj.AgentName = _CredentialsSpicejet.username;
                LogonRequestDataobj.Password = _CredentialsSpicejet.password;
                LogonRequestDataobj.DomainCode = _CredentialsSpicejet.domain;
                _logonRequestobj.logonRequestData = LogonRequestDataobj;

                _getapi objSpicejet = new _getapi();
                SpicejetSessionManager_.LogonResponse _SpicejetlogonResponseobj = await objSpicejet.Signature(_logonRequestobj);


                if (_SpicejetlogonResponseobj != null && _SpicejetlogonResponseobj.Signature != null)
                {
                    token = _SpicejetlogonResponseobj.Signature;
                }
                else
                {
                    ModelState.AddModelError("", "AirAsia token request failed.");
                    return View("Error");
                }
                #endregion

                // 3. Retrieve booking
                SpiceJetApiController objSpiceJet = new SpiceJetApiController();
                GetBookingRequest getBookingRequest = new GetBookingRequest();
                GetBookingResponse _getBookingResponse = new GetBookingResponse();
                SpicejetBookingManager_.BookingCommitResponse _BookingCommitResponse = new SpicejetBookingManager_.BookingCommitResponse();

                _BookingCommitResponse.BookingUpdateResponseData = new SpicejetBookingManager_.BookingUpdateResponseData();
                _BookingCommitResponse.BookingUpdateResponseData.Success = new SpicejetBookingManager_.Success();
                _BookingCommitResponse.BookingUpdateResponseData.Success.RecordLocator = pnr;


                getBookingRequest.Signature = token;
                getBookingRequest.ContractVersion = 420;
                getBookingRequest.GetBookingReqData = new GetBookingRequestData();
                getBookingRequest.GetBookingReqData.GetBookingBy = GetBookingBy.RecordLocator;
                getBookingRequest.GetBookingReqData.GetByRecordLocator = new GetByRecordLocator();
                getBookingRequest.GetBookingReqData.GetByRecordLocator.RecordLocator = _BookingCommitResponse.BookingUpdateResponseData.Success.RecordLocator;

                _getBookingResponse = await objSpiceJet.GetBookingdetails(getBookingRequest);

                // 4. cancel journey
                CancelRequest _cancelRQ = new CancelRequest();
                CancelResponse _cancelRes = new CancelResponse();
                _cancelRes = await objSpiceJet.CancelJourney(token, _cancelRQ, "OneWay");

                decimal balanceDue = _cancelRes.BookingUpdateResponseData.Success.PNRAmount.BalanceDue;
                decimal totalAmount = _cancelRes.BookingUpdateResponseData.Success.PNRAmount.TotalCost;

                // 5. Add payment journey
                AddPaymentToBookingRequest _bookingpaymentRequest = new AddPaymentToBookingRequest();
                AddPaymentToBookingResponse _BookingPaymentResponse = new AddPaymentToBookingResponse();
                _bookingpaymentRequest.Signature = token;
                _bookingpaymentRequest.ContractVersion = 420;
                _bookingpaymentRequest.addPaymentToBookingReqData = new AddPaymentToBookingRequestData();
                _bookingpaymentRequest.addPaymentToBookingReqData.MessageStateSpecified = true;
                _bookingpaymentRequest.addPaymentToBookingReqData.MessageState = MessageState.New;
                _bookingpaymentRequest.addPaymentToBookingReqData.WaiveFeeSpecified = true;
                _bookingpaymentRequest.addPaymentToBookingReqData.WaiveFee = false;
                _bookingpaymentRequest.addPaymentToBookingReqData.PaymentMethodTypeSpecified = true;
                _bookingpaymentRequest.addPaymentToBookingReqData.PaymentMethodType = RequestPaymentMethodType.AgencyAccount;
                _bookingpaymentRequest.addPaymentToBookingReqData.PaymentMethodCode = "AG";
                _bookingpaymentRequest.addPaymentToBookingReqData.QuotedCurrencyCode = "INR";
                _bookingpaymentRequest.addPaymentToBookingReqData.QuotedAmountSpecified = true;
                _bookingpaymentRequest.addPaymentToBookingReqData.QuotedAmount = balanceDue;
                //_bookingpaymentRequest.addPaymentToBookingReqData.AccountNumber = "OTI122";
                _bookingpaymentRequest.addPaymentToBookingReqData.InstallmentsSpecified = true;
                _bookingpaymentRequest.addPaymentToBookingReqData.Installments = 1;
                _bookingpaymentRequest.addPaymentToBookingReqData.ExpirationSpecified = true;
                _bookingpaymentRequest.addPaymentToBookingReqData.Expiration = Convert.ToDateTime("0001-01-01T00:00:00");
                _BookingPaymentResponse = await objSpiceJet.Addpayment(_bookingpaymentRequest);
                if (_BookingPaymentResponse.BookingPaymentResponse.ValidationPayment.PaymentValidationErrors.Length > 0 && _BookingPaymentResponse.BookingPaymentResponse.ValidationPayment.PaymentValidationErrors[0].ErrorDescription.ToLower().Contains("not enough funds available"))
                {
                    //_AirLinePNRTicket.ErrorDesc = "Not enough funds available.";
                }


                //if (!responsecancel.IsSuccessStatusCode)
                //{
                //    string err = await responsecancel.Content.ReadAsStringAsync();
                //    ModelState.AddModelError("", $"Cancellation status update failed: {err}");
                //    return View("Error");
                //}
                // 6. Commit journey
                //_getBookingResponse
                SpicejetBookingManager_.UpdateContactsRequest contactList = new SpicejetBookingManager_.UpdateContactsRequest();
                List<passkeytype> passeengerlist = new List<passkeytype>();
                contactList.updateContactsRequestData = new SpicejetBookingManager_.UpdateContactsRequestData();
                contactList.updateContactsRequestData.BookingContactList = new SpicejetBookingManager_.BookingContact[1];
                contactList.updateContactsRequestData.BookingContactList[0] = new SpicejetBookingManager_.BookingContact();
                contactList.updateContactsRequestData.BookingContactList[0].EmailAddress = _getBookingResponse.Booking.BookingContacts[0].EmailAddress;
                contactList.updateContactsRequestData.BookingContactList[0].HomePhone = _getBookingResponse.Booking.BookingContacts[0].HomePhone;
                passkeytype objpax = new passkeytype();
                objpax.first = _getBookingResponse.Booking.Passengers[0].Names[0].FirstName;
                objpax.middle = _getBookingResponse.Booking.Passengers[0].Names[0].MiddleName;
                objpax.last = _getBookingResponse.Booking.Passengers[0].Names[0].LastName;
                objpax.title = _getBookingResponse.Booking.Passengers[0].Names[0].Title;
                objpax.identifier = _getBookingResponse.Booking.PaxCount.ToString();
                passeengerlist.Add(objpax);

                BookingCommitRequest _bookingCommitRequest = new BookingCommitRequest();

                _bookingCommitRequest.Signature = token;
                _bookingCommitRequest.ContractVersion = 420;
                _bookingCommitRequest.BookingCommitRequestData = new BookingCommitRequestData();
                _bookingCommitRequest.BookingCommitRequestData.SourcePOS = GetPointOfSale();
                _bookingCommitRequest.BookingCommitRequestData.RecordLocator = pnr;
                _bookingCommitRequest.BookingCommitRequestData.CurrencyCode = "INR";
                _bookingCommitRequest.BookingCommitRequestData.PaxCount = Convert.ToInt16(passeengerlist.Count);
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts = new BookingContact[1];
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0] = new BookingContact();
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].TypeCode = "P";
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names = new BookingName[1];
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0] = new BookingName();
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].State = MessageState.New;
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].FirstName = passeengerlist[0].first;
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].MiddleName = passeengerlist[0].middle;
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].LastName = passeengerlist[0].last;
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].Names[0].Title = passeengerlist[0].title;
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].EmailAddress = contactList.updateContactsRequestData.BookingContactList[0].EmailAddress; //"vinay.ks@gmail.com"; //passeengerlist.Email;
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].HomePhone = "9457000000"; //contactList.updateContactsRequestData.BookingContactList[0].HomePhone; //"9457000000"; //passeengerlist.mobile;
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].AddressLine1 = "A";
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].AddressLine2 = "B";
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].City = "Delhi";
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].CountryCode = "IN";
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].CultureCode = "en-GB";
                _bookingCommitRequest.BookingCommitRequestData.BookingContacts[0].DistributionOption = DistributionOption.Email;

                IBookingManager bookingManager = null;
                BookingCommitResponse getBookiongCommitResponse = null;
                bookingManager = new BookingManagerClient();
                try
                {
                    getBookiongCommitResponse = await bookingManager.BookingCommitAsync(_bookingCommitRequest);
                    //return getBookiongCommitResponse;
                }
                catch (Exception ex)
                {
                    //Logs logs = new Logs();
                    //logs.WriteLogs("Request: " + JsonConvert.SerializeObject(_getbookingCommitRQ) + "\n\n Response: " + ex.ToString(), "BookingCommit", "SpicejetOneWay");
                    //return Ok(session);
                }
                //return getBookiongCommitResponse;


                _getBookingResponse = await objSpiceJet.GetBookingdetails(getBookingRequest);

                var email = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                Logs logs = new Logs();
                logs.WriteLogs(JsonConvert.SerializeObject(_cancelRes), "Cancel", "Indigooneway", "oneway");


                // 7.LogOut 
                SpicejetSessionManager_.LogoutRequest _logoutRequestobj = new SpicejetSessionManager_.LogoutRequest();
                SpicejetSessionManager_.LogoutResponse _logoutResponse = new SpicejetSessionManager_.LogoutResponse();
                _logoutRequestobj.ContractVersion = 420;
                _logoutRequestobj.Signature = token;
                _logoutResponse = await objSpicejet.Logout(_logoutRequestobj);

                balanceDue = _getBookingResponse.Booking.BookingSum.BalanceDue;
                totalAmount = _getBookingResponse.Booking.BookingSum.TotalCost;

                var cancelRequest = new
                {
                    RecordLocator = pnr,
                    Status = 3,
                    UserEmail = email,
                    BalanceDue = balanceDue,
                    TotalAmount = totalAmount
                };

                string jsonPayload = JsonConvert.SerializeObject(cancelRequest);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var responsecancel = await client.PostAsync(AppUrlConstant.CancleStatus, content);

                if (!responsecancel.IsSuccessStatusCode)
                {
                    string err = await responsecancel.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Cancellation status update failed: {err}");
                    return View("Error");
                }

                TempData["Success"] = "Booking cancellation session flow completed successfully.";
                TempData["FinalStatus"] = JsonConvert.SerializeObject(_BookingCommitResponse);

                string Message = "Booking cancellation completed successfully." + totalAmount;

                return RedirectToAction("MyBooking", "Booking", new { Mess = Message });
            }
        }
        public PointOfSale GetPointOfSale()
        {
            PointOfSale SourcePOS = null;
            try
            {
                SourcePOS = new PointOfSale();
                SourcePOS.State = Bookingmanager_.MessageState.New;
                SourcePOS.OrganizationCode = "APITESTID";
                SourcePOS.AgentCode = "AG";
                SourcePOS.LocationCode = "";
                SourcePOS.DomainCode = "WWW";
            }
            catch (Exception e)
            {
                string exp = e.Message;
                exp = null;
            }

            return SourcePOS;
        }
        private async Task<IActionResult> CancelPartialAirAsiaBooking(string pnr, List<string> passengerKeys)
        {
            string PartialMessage = string.Empty;
            // TODO: Call AirAsia SSR delete or passenger-specific cancel API if available
            using (HttpClient client = new HttpClient())
            {
                //client.BaseAddress = new Uri(AppUrlConstant.BaseURL);
                client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);
                // 1. Get AirAsia credentials
                var url = $"{AppUrlConstant.Getsuppliercred}?flightclass={Uri.EscapeDataString("Corporate")}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve airline credentials.");
                    return View("Error");
                }

                var results = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);
                var _credentialsAirasia = jsonObject.FirstOrDefault(cred => cred?.FlightCode == 1); // AirAsia

                // 2. Login and get token
                var login = new airlineLogin { credentials = _credentialsAirasia };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage tokenResponse = await client.PostAsJsonAsync(AppUrlConstant.AirasiaTokan, login);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "AirAsia token request failed.");
                    return View("Error");
                }

                var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
                dynamic tokenJson = JsonConvert.DeserializeObject<dynamic>(tokenResult);
                string token = tokenJson.data.token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 3. Retrieve booking
                var retrieveUrl = $"{AppUrlConstant.AirasiaPNRBooking}/{pnr}";
                var retrieveResponse = await client.GetAsync(retrieveUrl);
                if (!retrieveResponse.IsSuccessStatusCode)
                {
                    var err = await retrieveResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Booking retrieval failed: {err}");
                    return View("Error");
                }

                // 4. Divide Booking

                //Start :Request Body
                var dividePayload = new
                {
                    crsRecordLocators = new[]
                    {
                      new { recordLocatorKey = pnr }
                    },
                    passengerKeys = passengerKeys,
                    autoDividePayments = true,
                    bookingPaymentTransfers = new[]
                    {
                      new { bookingPaymentId = 0, transferAmount = 0 }
                    },
                    receivedBy = "",
                    overrideRestrictions = false,
                    childEmail = "",
                    cancelSourceBooking = false
                };
                //End :Request Body

                var divideContent = new StringContent(JsonConvert.SerializeObject(dividePayload), Encoding.UTF8, "application/json");
                var divideResponse = await client.PostAsync(AppUrlConstant.AirasiaBookingdivide, divideContent);

                if (!divideResponse.IsSuccessStatusCode)
                {
                    var error = await divideResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Divide Booking failed: {error}");
                    return View("Error");
                }

                // ✅ Deserialize success response
                var divideResponseContent = await divideResponse.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(divideResponseContent);
                var root = doc.RootElement;

                var data = root.GetProperty("data");

                // Navigate to the "passengers" dictionary
                var passengers = data.GetProperty("passengers");

                foreach (var passenger in passengers.EnumerateObject())
                {
                    string passengerKey = passenger.Name;
                    var passengerData = passenger.Value;

                    var nameObj = passengerData.GetProperty("name");
                    string firstName = nameObj.GetProperty("first").GetString();
                    string lastName = nameObj.GetProperty("last").GetString();
                    string title = nameObj.GetProperty("title").GetString();

                    //Console.WriteLine($"Passenger Key: {passengerKey}");
                    //Console.WriteLine($"Name: {title} {firstName} {lastName}");

                    PartialMessage += $"Passenger Key: {passengerKey}, Name: {title} {firstName} {lastName}\n";

                    // Check for infant data
                    //if (passengerData.TryGetProperty("infant", out var infant))
                    //{
                    //    var infantName = infant.GetProperty("name");
                    //    string infantFirst = infantName.GetProperty("first").GetString();
                    //    string infantLast = infantName.GetProperty("last").GetString();
                    //    string infantTitle = infantName.GetProperty("title").GetString();
                    //    string dob = infant.GetProperty("dateOfBirth").GetString();

                    //   // Console.WriteLine($"  Infant: {infantTitle} {infantFirst} {infantLast}, DOB: {dob}");
                    //    PartialMessage += $"  Infant: {infantTitle} {infantFirst} {infantLast}, DOB: {dob}\n";
                    //}
                }

                // Optional: log response
                Logs logs = new Logs();
                logs.WriteLogs(divideResponseContent, "DivideBooking", "AirAsiaOneWay", "oneway");

                // Deserialize response
                dynamic divideResult = JsonConvert.DeserializeObject<dynamic>(divideResponseContent);

                // Extract new record locator and key
                //string newRecordLocator = divideResult?.data?.booking?.recordLocator;
                string newRecordLocator = divideResult?.data?.recordLocator;
                string bookingKey = divideResult?.data?.bookingKey;

                // Store or pass forward for next operations
                TempData["NewPNR"] = newRecordLocator;
                TempData["BookingKey"] = bookingKey;


                // 5. PUT commit
                var putResult = await HttpHelperService.SendPutAsync(client, $"{AppUrlConstant.AirasiaCommitBooking}");
                if (!putResult.Success)
                {
                    ModelState.AddModelError("", $"PUT failed: {putResult.Error}");
                    return View("Error");
                }

                // 6. Final GET booking
                var finalGet = await client.GetAsync($"{AppUrlConstant.AirasiaGetBoking}");
                if (!finalGet.IsSuccessStatusCode)
                {
                    var error = await finalGet.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Final booking status check failed. {error}");
                    return View("Error");
                }

                var _finalStatus = await finalGet.Content.ReadAsStringAsync();
                dynamic objcancelBooking = JsonConvert.DeserializeObject<dynamic>(_finalStatus);
                decimal balanceDue = objcancelBooking.data.breakdown.balanceDue;
                decimal totalAmount = objcancelBooking.data.breakdown.totalAmount;


                var email = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                logs.WriteLogs(_finalStatus, "Cancel", "AirAsiaOneWay", "oneway");

                var cancelRequest = new
                {
                    RecordLocator = newRecordLocator,
                    Status = 1,
                    UserEmail = email,
                    BalanceDue = balanceDue,
                    TotalAmount = totalAmount
                };

                string jsonPayload = JsonConvert.SerializeObject(cancelRequest);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var responsecancel = await client.PostAsync(AppUrlConstant.CancleStatus, content);

                if (!responsecancel.IsSuccessStatusCode)
                {
                    string err = await responsecancel.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Cancellation status update failed: {err}");
                    return View("Error");
                }

                TempData["Success"] = "Booking cancellation session flow completed successfully.";
                TempData["FinalStatus"] = _finalStatus;
                return RedirectToAction("MyBooking", "Booking", new { Mess = PartialMessage });
            }
        }

        private async Task<IActionResult> CancelPartialAkasaBooking(string pnr, List<string> passengerKeys)
        {
            string PartialMessage = string.Empty;
            // TODO: Call Akasa SSR delete or passenger-specific cancel API if available
            using (HttpClient client = new HttpClient())
            {
                //client.BaseAddress = new Uri(AppUrlConstant.BaseURL);
                client.BaseAddress = new Uri(AppUrlConstant.AdminBaseURL);
                // 1. Get Akasa credentials
                var url = $"{AppUrlConstant.Getsuppliercred}?flightclass={Uri.EscapeDataString("Corporate")}";
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Failed to retrieve airline credentials.");
                    return View("Error");
                }

                var results = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonConvert.DeserializeObject<List<_credentials>>(results);
                var _credentialsAkasa = jsonObject.FirstOrDefault(cred => cred?.FlightCode == 2); // AirAsia

                // 2. Login and get token
                var login = new airlineLogin { credentials = _credentialsAkasa };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage tokenResponse = await client.PostAsJsonAsync(AppUrlConstant.AkasaTokan, login);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "AirAsia token request failed.");
                    return View("Error");
                }

                var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
                dynamic tokenJson = JsonConvert.DeserializeObject<dynamic>(tokenResult);
                string token = tokenJson.data.token;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 3. Retrieve booking
                var retrieveUrl = $"{AppUrlConstant.AkasaAirPNRBooking}/{pnr}";
                var retrieveResponse = await client.GetAsync(retrieveUrl);
                if (!retrieveResponse.IsSuccessStatusCode)
                {
                    var err = await retrieveResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Booking retrieval failed: {err}");
                    return View("Error");
                }

                // 4. Divide Booking

                //Start :Request Body
                var dividePayload = new
                {
                    crsRecordLocators = new[]
                    {
                      new { recordLocatorKey = pnr }
                    },
                    passengerKeys = passengerKeys,
                    autoDividePayments = true,
                    bookingPaymentTransfers = new[]
                    {
                      new { bookingPaymentId = 0, transferAmount = 0 }
                    },
                    receivedBy = "",
                    overrideRestrictions = false,
                    childEmail = "",
                    cancelSourceBooking = false
                };
                //End :Request Body

                var divideContent = new StringContent(JsonConvert.SerializeObject(dividePayload), Encoding.UTF8, "application/json");
                var divideResponse = await client.PostAsync(AppUrlConstant.AkasaAirBookingdivide, divideContent);

                if (!divideResponse.IsSuccessStatusCode)
                {
                    var error = await divideResponse.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Divide Booking failed: {error}");
                    return View("Error");
                }

                // ✅ Deserialize success response
                var divideResponseContent = await divideResponse.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(divideResponseContent);
                var root = doc.RootElement;

                var data = root.GetProperty("data");

                // Navigate to the "passengers" dictionary
                var passengers = data.GetProperty("passengers");

                foreach (var passenger in passengers.EnumerateObject())
                {
                    string passengerKey = passenger.Name;
                    var passengerData = passenger.Value;

                    var nameObj = passengerData.GetProperty("name");
                    string firstName = nameObj.GetProperty("first").GetString();
                    string lastName = nameObj.GetProperty("last").GetString();
                    string title = nameObj.GetProperty("title").GetString();

                    //Console.WriteLine($"Passenger Key: {passengerKey}");
                    //Console.WriteLine($"Name: {title} {firstName} {lastName}");

                    PartialMessage += $"Passenger Key: {passengerKey}, Name: {title} {firstName} {lastName}\n";

                    // Check for infant data
                    //if (passengerData.TryGetProperty("infant", out var infant))
                    //{
                    //    var infantName = infant.GetProperty("name");
                    //    string infantFirst = infantName.GetProperty("first").GetString();
                    //    string infantLast = infantName.GetProperty("last").GetString();
                    //    string infantTitle = infantName.GetProperty("title").GetString();
                    //    string dob = infant.GetProperty("dateOfBirth").GetString();

                    //   // Console.WriteLine($"  Infant: {infantTitle} {infantFirst} {infantLast}, DOB: {dob}");
                    //    PartialMessage += $"  Infant: {infantTitle} {infantFirst} {infantLast}, DOB: {dob}\n";
                    //}
                }

                // Optional: log response
                Logs logs = new Logs();
                logs.WriteLogs(divideResponseContent, "DivideBooking", "AkasaOneWay", "oneway");

                // Deserialize response
                dynamic divideResult = JsonConvert.DeserializeObject<dynamic>(divideResponseContent);

                // Extract new record locator and key
                //string newRecordLocator = divideResult?.data?.booking?.recordLocator;
                string newRecordLocator = divideResult?.data?.recordLocator;
                string bookingKey = divideResult?.data?.bookingKey;

                // Store or pass forward for next operations
                TempData["NewPNR"] = newRecordLocator;
                TempData["BookingKey"] = bookingKey;


                // 5. PUT commit
                var putResult = await HttpHelperService.SendPutAsync(client, $"{AppUrlConstant.AkasaAirCommitBooking}");
                if (!putResult.Success)
                {
                    ModelState.AddModelError("", $"PUT failed: {putResult.Error}");
                    return View("Error");
                }

                // 6. Final GET booking
                var finalGet = await client.GetAsync($"{AppUrlConstant.AkasaAirGetBooking}");
                if (!finalGet.IsSuccessStatusCode)
                {
                    var error = await finalGet.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Final booking status check failed. {error}");
                    return View("Error");
                }

                var _finalStatus = await finalGet.Content.ReadAsStringAsync();
                dynamic objcancelBooking = JsonConvert.DeserializeObject<dynamic>(_finalStatus);
                decimal balanceDue = objcancelBooking.data.breakdown.balanceDue;
                decimal totalAmount = objcancelBooking.data.breakdown.totalAmount;


                var email = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                logs.WriteLogs(_finalStatus, "Cancel", "AkasaOneWay", "oneway");

                var cancelRequest = new
                {
                    RecordLocator = newRecordLocator,
                    Status = 1,
                    UserEmail = email,
                    BalanceDue = balanceDue,
                    TotalAmount = totalAmount
                };

                string jsonPayload = JsonConvert.SerializeObject(cancelRequest);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var responsecancel = await client.PostAsync(AppUrlConstant.CancleStatus, content);

                if (!responsecancel.IsSuccessStatusCode)
                {
                    string err = await responsecancel.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Cancellation status update failed: {err}");
                    return View("Error");
                }

                TempData["Success"] = "Booking cancellation session flow completed successfully.";
                TempData["FinalStatus"] = _finalStatus;
                return RedirectToAction("MyBooking", "Booking", new { Mess = PartialMessage });
            }
        }

        private async Task<IActionResult> CancelCompleteOtherAirline(string pnr)
        {
            // TODO: Implement logic for airline 2 full cancellation
            ModelState.AddModelError("", "Complete cancellation for this airline is not yet implemented.");
            return View("Error");
        }

        private async Task<IActionResult> CancelPartialOtherAirline(string pnr, List<string> passengerKeys)
        {
            // TODO: Implement logic for airline 2 partial cancellation
            ModelState.AddModelError("", "Partial cancellation for this airline is not yet implemented.");
            return View("Error");
        }



        [HttpGet]
        public async Task<IActionResult> CancelRefundAsync(string flightid, string pnr)
        {
            List<RefundRequest> RefundRequestList = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Build the query string if needed
                    var apiUrl = $"{AppUrlConstant.GetRefund}?bookingId={flightid}&recordLocator={pnr}";

                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        RefundRequestList = JsonConvert.DeserializeObject<List<RefundRequest>>(result);

                        return Json(RefundRequestList); // Pass to Razor View

                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Failed to load refund data.";
                        return Json(RefundRequestList);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred: " + ex.Message;
                return Json(RefundRequestList);
            }



            //// Example response (replace with real refund logic)
            //var refundDetails = new
            //{
            //    RefundId = fid,
            //    PNR = p,
            //    Amount = 4500.00,
            //    Status = "Processed"
            //};

            //// Return as JSON
            //return Json(refundDetails);
        }

    }

}

