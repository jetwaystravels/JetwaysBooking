using DomainLayer.Model;
using MongoDB.Driver;
using Nancy;
using OnionConsumeWebAPI.ApiService;
using System.Security.Cryptography;
using System.Xml.Serialization;
using OnionConsumeWebAPI.ErrorHandling;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;

namespace OnionConsumeWebAPI.Models
{
    public class MongoDBHelper
    {
        private static IMongoClient mongoClient;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        IMongoDatabase mDB;

        //  private readonly IOptionsSnapshot<AppSettings> _balSettings;
        //IOptionsSnapshot<AppSettings> serviceSettings

        Logger logger = new Logger();

        public MongoDBHelper(IConfiguration configuration)
        {
            // mongoClient = new MongoClient(ConfigurationManager.AppSettings["MongoDBConn"].ToString());
            _configuration = configuration;

            // pick from iconfig
            //_connectionString = _configuration.GetSection("ConnectionStrings").GetValue<string>("DefaultConnection");
            //mongoClient = new MongoClient(_configuration.GetSection("MongoDbSettings").GetValue<string>("ConnectionString"));
            //mDB = mongoClient.GetDatabase(_configuration.GetSection("MongoDbSettings").GetValue<string>("DatabaseName"));

            _connectionString = _configuration["ConnectionStrings:DefaultConnection"];
            mongoClient = new MongoClient(_configuration["MongoDbSettings:ConnectionString"]);
            mDB = mongoClient.GetDatabase(_configuration["MongoDbSettings:DatabaseName"]);

        }

        //public MongoDBHelper(MongoDbService mongoDbService, IConfiguration configuration)
        //{
        //    // mongoClient = new MongoClient(ConfigurationManager.AppSettings["MongoDBConn"].ToString());
        //    _mongoDbService = mongoDbService;
        //    this.configuration = configuration;
        //    // mongoClient = new MongoClient(this.configuration.GetSection("MongoDbSettings").ToString());
        //    mongoClient = new MongoClient(this.configuration.GetValue<string>("MongoDbSettings"));
        //    mDB = mongoClient.GetDatabase(this.configuration.GetValue<string>("DatabaseName"));
        //}

        public async Task<string> GetFlightSearchByKeyRef(string keyref)
        {
            string guid = "";
            try
            {
                MongoResponces srchData = new MongoResponces();

                //  _mongoDbService = new MongoDbService();

                srchData = await mDB.GetCollection<MongoResponces>("KeyLog").Find(Builders<MongoResponces>.Filter.Eq("KeyRef", keyref)).Sort(Builders<MongoResponces>.Sort.Descending("CreatedDate")).FirstOrDefaultAsync().ConfigureAwait(false);

                if (srchData != null)
                {
                    if (srchData.CreatedDate < DateTime.UtcNow)
                    {
                        srchData.Guid = null;
                    }
                    else
                    {
                        guid = srchData.Guid;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "GetFlightSearchByKeyRef methhod", _connectionString);

            }
            return guid;
        }

        public async Task<MongoResponces> GetALLFlightResulByGUIDRoundTrip(string guid)
        {
            MongoResponces srchDataALL = new MongoResponces();
            try
            {
                srchDataALL = await mDB.GetCollection<MongoResponces>("Result").Find(Builders<MongoResponces>.Filter.Eq("Guid", guid)).Sort(Builders<MongoResponces>.Sort.Descending("CreatedDate")).FirstOrDefaultAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "GetALLFlightResulByGUID methhod", _connectionString);
            }
            return srchDataALL;
        }

        public async Task<string> GetALLFlightResulByGUID(string guid)
        {
            MongoResponces srchDataALL = new MongoResponces();
            try
            {
                srchDataALL = await mDB.GetCollection<MongoResponces>("Result").Find(Builders<MongoResponces>.Filter.Eq("Guid", guid)).Sort(Builders<MongoResponces>.Sort.Descending("CreatedDate")).FirstOrDefaultAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "GetALLFlightResulByGUID methhod", _connectionString);
            }

            if (srchDataALL == null)
            {
                return string.Empty;
            }
            else
            {

                return srchDataALL.Response;
            }
        }

        public async Task<string> GetALLRightFlightResulByGUID(string guid)
        {
            MongoResponces srchDataALL = new MongoResponces();
            try
            {
                srchDataALL = await mDB.GetCollection<MongoResponces>("Result").Find(Builders<MongoResponces>.Filter.Eq("Guid", guid)).Sort(Builders<MongoResponces>.Sort.Descending("CreatedDate")).FirstOrDefaultAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "GetALLFlightResulByGUID methhod", _connectionString);
            }
            return srchDataALL.RightResponse;
        }

        public void SaveKeyRequest(string guid, string keyref)
        {
            try
            {
                MongoResponces srchData = new MongoResponces();
                srchData.CreatedDate = DateTime.UtcNow.AddMinutes(Convert.ToInt16(20));
                srchData.KeyRef = keyref;
                srchData.Guid = guid;

                mDB.GetCollection<MongoResponces>("KeyLog").InsertOneAsync(srchData);
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "SaveKeyRequest methhod", _connectionString);
            }
        }

        public void SaveFlightSearch(string Guid, string resp, string rightResponse)
        {
            try
            {
                MongoResponces srchData = new MongoResponces();
                MongoHelper mongoHelper = new MongoHelper();
                srchData.CreatedDate = DateTime.UtcNow.AddMinutes(Convert.ToInt16(20));
                srchData.Guid = Guid;
                srchData.Response = resp;
                if (!string.IsNullOrEmpty(rightResponse))
                {
                    srchData.RightResponse = rightResponse;
                }

                mDB.GetCollection<MongoResponces>("Result").InsertOneAsync(srchData);
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "SaveFlightSearch methhod", _connectionString);
            }
        }

        public void SaveMongoFlightToken(MongoSuppFlightToken mongoSuppFlightToken)
        {
            try
            {

                mongoSuppFlightToken.CreatedDate = DateTime.UtcNow.AddMinutes(Convert.ToInt16(20));


                mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").InsertOneAsync(mongoSuppFlightToken);
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "SaveMongoFlightToken methhod", _connectionString);
            }
        }

        public void UpdateMongoFlightToken(string guid, string supp, string Token, string Rtoken)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                if (string.IsNullOrEmpty(Rtoken))
                {
                    var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.Token, Token);
                    mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);
                }
                else
                {
                    var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.Token, Token).Set(s => s.RToken, Rtoken);
                    mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);
                }




            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateMongoFlightToken methhod", _connectionString);
            }

        }

        public void UpdateMongoFlightPassRequest(string guid, string supp, string PassreqL, string PassReqR)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                if (string.IsNullOrEmpty(PassReqR))
                {
                    var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.PassRequest, PassreqL);
                    mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);
                }
                else
                {
                    var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.PassRequest, PassreqL).Set(s => s.PassRequestR, PassReqR);
                    mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);
                }




            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateMongoFlightToken methhod", _connectionString);
            }

        }
        public void UpdatePassengerMongoFlightToken(string guid, string supp, string Passenger)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.PassengerRequest, Passenger);
                mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateMongoFlightToken methhod", _connectionString);
            }

        }


        public async Task<MongoSuppFlightToken> GetSuppFlightTokenByGUID(string guid, string supp)
        {
            MongoSuppFlightToken tokenData = new MongoSuppFlightToken();
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                tokenData = await mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").Find(filter).Sort(Builders<MongoSuppFlightToken>.Sort.Descending("CreatedDate")).FirstOrDefaultAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "GetALLFlightResulByGUID methhod", _connectionString);
            }
            return tokenData;
        }

        public void UpdateFlightTokenJourney(string guid, string supp, string journeykey)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.JourneyKey, journeykey);
                mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateFlightTokenJourney methhod", _connectionString);
            }

        }

        public void UpdateFlightTokenPassenger(string guid, string supp, string Passenger)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.PassRequest, Passenger);
                mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateFlightTokenPassenger methhod", _connectionString);
            }

        }

        public void UpdateCommitResponse(string guid, string supp, string CommitValue)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.CommResponse, CommitValue);
                mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateFlightTokenPassenger methhod", _connectionString);
            }

        }

        public void UpdateFlightTokenPassengerGDS(string guid, string supp, string Passenger)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.PassengerRequest, Passenger);
                //var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.OldPassengerRequest, Passenger);
                mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateFlightTokenPassenger methhod", _connectionString);
            }

        }

        public void UpdateFlightTokenOldPassengerGDS(string guid, string supp, string Passenger)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                //var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.PassengerRequest, Passenger);
                var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.OldPassengerRequest, Passenger);
                mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateFlightTokenPassenger methhod", _connectionString);
            }

        }
        public void UpdateFlightTokenContact(string guid, string supp, string Contact)
        {
            try
            {
                var filter = Builders<MongoSuppFlightToken>.Filter.And(Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSuppFlightToken>.Filter.Eq(emp => emp.Supp, supp));
                var update = Builders<MongoSuppFlightToken>.Update.Set(s => s.ContactRequest, Contact);
                mDB.GetCollection<MongoSuppFlightToken>("SearchFlightToken").UpdateOneAsync(filter, update);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "UpdateFlightTokenPassenger methhod", _connectionString);
            }

        }

        public void SaveRequest(SimpleAvailabilityRequestModel sCriteria, string Guid)
        {
            MongoRequest srchData = new MongoRequest();
            MongoHelper mongoHelper = new MongoHelper();
            try
            {
                srchData.Guid = Guid;
                srchData.CreatedDate = DateTime.UtcNow.AddMinutes(Convert.ToInt16(20));

                using (StringWriter stringWriter = new StringWriter())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SimpleAvailabilityRequestModel));
                    serializer.Serialize(stringWriter, sCriteria);
                    srchData.Request = mongoHelper.Zip(stringWriter.ToString());
                }

                mDB.GetCollection<MongoRequest>("Requests").InsertOneAsync(srchData);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "SaveRequest methhod", _connectionString);
            }
        }

        public async Task<SimpleAvailabilityRequestModel> GetRequests(string guid)
        {
            SimpleAvailabilityRequestModel sCriteria = new SimpleAvailabilityRequestModel();
            MongoRequest srchData = new MongoRequest();
            MongoHelper mongoHelper = new MongoHelper();
            //service1 src = new service1();
            try
            {
                await mDB.GetCollection<MongoRequest>("Requests").Find(Builders<MongoRequest>.Filter.Eq("Guid", guid)).FirstOrDefaultAsync().ConfigureAwait(false);

                if (srchData != null && srchData.Request != null)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SimpleAvailabilityRequestModel));

                    StringReader textReader = new StringReader(mongoHelper.UnZip(srchData.Request));

                    sCriteria = (SimpleAvailabilityRequestModel)serializer.Deserialize(textReader);

                    // sCriteria.LogDateTime = srchData.CreatedDate;
                }
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "GetRequests methhod", _connectionString);
            }

            return sCriteria;
        }


        public void SaveSearchLog(SimpleAvailabilityRequestModel requestModel, string Guid, string flightClass)
        {
            MongoHelper mongoHelper = new MongoHelper();

            SearchLog searchLog = new SearchLog();

            try
            {
                searchLog.TripType = requestModel.trip;
                searchLog.Log_WSGUID = Guid;
                searchLog.Log_SearchTypeID = 1;
                if (requestModel.origin.Contains("-"))
                {
                    searchLog.OrgCode = requestModel.origin.Split("-")[1];
                    searchLog.Origin = requestModel.origin.Split("-")[0];

                }
                else
                {

                    searchLog.OrgCode = requestModel.origin;
                    searchLog.Origin = requestModel.origin;
                }


                if (requestModel.destination.Contains("-"))
                {
                    searchLog.DestCode = requestModel.destination.Split("-")[1];
                    searchLog.Destination = requestModel.destination.Split("-")[0];
                }
                else
                {
                    searchLog.DestCode = requestModel.destination;
                    searchLog.Destination = requestModel.destination;
                }

                searchLog.Log_RefNumber = mongoHelper.Get8Digits();
                searchLog.DepartDateTime = requestModel.beginDate;
                searchLog.ArrivalDateTime = requestModel.endDate;

                if (requestModel.passengercount != null)
                {
                    searchLog.Adults = requestModel.passengercount.adultcount;
                    searchLog.Children = requestModel.passengercount.childcount;
                    searchLog.Infants = requestModel.passengercount.infantcount;
                }
                else
                {
                    searchLog.Adults = requestModel.adultcount;
                    searchLog.Children = requestModel.childcount;
                    searchLog.Infants = requestModel.infantcount;
                }
                TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                searchLog.Log_DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                searchLog.IP = mongoHelper.GetIp();
                searchLog.Device = mongoHelper.DeviceName();
                searchLog.FlightClass = flightClass;
                searchLog.Webref = "JET-" + mongoHelper.Get8Digits();
                // _mongoDbService.GetCollection<SearchLog>("LogSearchData").InsertOneAsync(searchLog);
                mDB.GetCollection<SearchLog>("LogSearchData").InsertOneAsync(searchLog);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "SaveSearchLog methhod", _connectionString);
            }
        }

        public async Task<SearchLog> GetFlightSearchLog(string Guid)
        {
            SearchLog srchData = null;
            try
            {
                // srchData = new SearchLog();

                //  _mongoDbService = new MongoDbService();l

                //  srchData = await _mongoDbService.GetCollection<SearchLog>("LogSearchData").Find(Builders<SearchLog>.Filter.Eq("Log_WSGUID", Guid)).Sort(Builders<SearchLog>.Sort.Descending("Log_DateTime")).FirstOrDefaultAsync().ConfigureAwait(false);
                srchData = await mDB.GetCollection<SearchLog>("LogSearchData").Find(Builders<SearchLog>.Filter.Eq("Log_WSGUID", Guid)).Sort(Builders<SearchLog>.Sort.Descending("Log_DateTime")).FirstOrDefaultAsync().ConfigureAwait(false);

                if (srchData != null)
                {
                    srchData.OrgCode = srchData.OrgCode.Trim();
                    srchData.DestCode = srchData.DestCode.Trim();
                    srchData.Origin = srchData.Origin.Trim();
                    srchData.Destination = srchData.Destination.Trim();
                }

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "GetFlightSearchLog methhod", _connectionString);

            }

            return srchData;

        }

        public void SaveResultSeatMealRequest(MongoSeatMealdetail mongoSeat)
        {
            MongoHelper mongoHelper = new MongoHelper();
            try
            {

                mongoSeat.CreatedDate = DateTime.UtcNow.AddMinutes(Convert.ToInt16(0));
                mDB.GetCollection<MongoSeatMealdetail>("SeatMealRequests").InsertOneAsync(mongoSeat);

            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "SaveResultSeatMealRequest methhod", _connectionString);
            }
        }

        public async Task<MongoSeatMealdetail> GetSuppSeatMealByGUID(string guid, string supp)
        {
            MongoSeatMealdetail seatMeal = new MongoSeatMealdetail();
            try
            {
                var filter = Builders<MongoSeatMealdetail>.Filter.And(Builders<MongoSeatMealdetail>.Filter.Eq(emp => emp.Guid, guid),
                Builders<MongoSeatMealdetail>.Filter.Eq(emp => emp.Supp, supp));
                seatMeal = await mDB.GetCollection<MongoSeatMealdetail>("SeatMealRequests").Find(filter).Sort(Builders<MongoSeatMealdetail>.Sort.Descending("CreatedDate")).FirstOrDefaultAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.WriteLog(ex, "GetSuppSeatMealByGUID methhod", _connectionString);
            }
            return seatMeal;
        }


    }
}
