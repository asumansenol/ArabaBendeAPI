using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using ArabaBendeAPI.Models;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;

namespace ArabaBendeAPI.Controllers
{
    public class RequestController : ApiController
    {
        arabaDBEntities12 dbRes = new arabaDBEntities12();
        arabaDBEntities11 dbReq = new arabaDBEntities11();
        arabaDBEntities14 dbEmp = new arabaDBEntities14();
        arabaDBEntities10 dbVeh = new arabaDBEntities10();
        private IObjectContextAdapter ctx;

        // GET: api/Employees  
        public IHttpActionResult GetRequests()
        {
            var res = new List<Reservation>(dbRes.Reservations);
            var req = new List<Request>(dbReq.Requests);
            var veh = new List<Vehicle>(dbVeh.Vehicles);
            var emp = new List<Person>(dbEmp.Persons);
            var request = new List<SelectRequest>();
            IList<object> sourceList = new List<object>();
            string sqlString = @"select TerritorySummary.isim, 
	                                    TerritorySummary.PhotoUrl,
	                                    TerritorySummary.TelephoneNumber,
	                                    TerritorySummary.Plaka,
	                                    TerritorySummary.DurationCheck,
	                                TerritorySummary.CreationDate
                                from 
                                (select per.FirstName + ' ' + per.LastName isim ,
	                                per.TelephoneNumber,
	                                per.PhotoUrl,
	                                veh.Plaka,
	                                req.DurationCheck,
	                                req.CreationDate,
	                                ROW_NUMBER() OVER(Partition by veh.plaka ORDER BY req.CreationDate desc) xs
	                                from dbo.Persons per,
	                                dbo.Requests req,
	                                dbo.Vehicles veh
	                                where 
		                                per.PersonID = req.PersonID
	                                and req.VehicleID = veh.VehicleID
	                                and veh.EnabledFlag = 0
                                )AS TerritorySummary
                                where TerritorySummary.xs = 1";
            using (var context = new arabaDBEntities10())
            {
                request = context.Database.SqlQuery<SelectRequest>(sqlString).ToList();
            }
            foreach ( SelectRequest requ in request)
            {
                DateTimeOffset localTime, otherTime, universalTime;

                // Define local time in local time zone
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
                DateTime UpdatedTime = requ.CreationDate ?? DateTime.Now;
                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(UpdatedTime, cstZone);


                requ.CreationDateStr = cstTime.ToString().Split(' ')[1].Split(':')[0] +":"+ cstTime.ToString().Split(' ')[1].Split(':')[1];
            }
            sourceList.Add(request);
            var reservations = (from ep in emp
                               join e in res on ep.PersonID equals e.PersonID
                               join t in veh on e.VehicleID equals t.VehicleID
                               where e.BeginDate >= TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Turkey Standard Time")
                               || (e.BeginDate < TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Turkey Standard Time") 
                               && TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Turkey Standard Time") < e.EndDate)
                               orderby e.BeginDate ascending
                                select new
                               {
                                   Name = ep.FirstName + " " + ep.LastName,
                                   BeginDate = e.BeginDate.ToString(),
                                   PhotoUrl = ep.PhotoUrl,
                                   Plaka = t.Plaka,
                                   TelephoneNumber = ep.TelephoneNumber,
                                   EndDate = e.EndDate.ToString(),
                                   Aim = e.Aim,
                                   ReservationId=e.ReservationID,
                                   PersonId = e.PersonID
                                }).ToList();

            sourceList.Add(reservations);
            return Json(sourceList);
        }

        [HttpGet]
        [Route("api/GetRequestsAccToEmpId")]
        public IHttpActionResult GetRequestsAccToEmpId(int empId)
        {
            var req = new List<Request>(dbReq.Requests);
            var veh = new List<Vehicle>(dbVeh.Vehicles);
            var emp = new List<Person>(dbEmp.Persons);

            var entryPoint = (from ep in emp
                              join e in req on ep.PersonID equals e.PersonID
                              join t in veh on e.VehicleID equals t.VehicleID
                              where e.PersonID == empId
                              select new
                              {
                                  Plaka = t.Plaka,
                                  Duration = e.DurationCheck
                              }).ToList();

            return Json(entryPoint);
        }

        [HttpGet]
        [Route("api/GetRequestsAccToVehId")]
        public IHttpActionResult GetRequestsAccToVehId(int VehId)
        {
            var req = new List<Request>(dbReq.Requests);
            var emp = new List<Person>(dbEmp.Persons);
            var veh = new List<Vehicle>(dbVeh.Vehicles);

            var top1Req = (from r in req
                           join e in emp on r.PersonID equals e.PersonID
                           join t in veh on r.VehicleID equals t.VehicleID
                           where r.VehicleID == VehId
                              select new {  DurationCheck = r.DurationCheck,
                                            Name = e.FirstName +" "+ e.LastName,
                                            CreationDate = getTurkishTime(r.CreationDate),
                                            Plaka=t.Plaka
                              }).LastOrDefault();

            return Json(top1Req);
        }

        private string getTurkishTime(DateTime? creationDate)
        {
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time");
            DateTime UpdatedTime = creationDate ?? DateTime.Now;
            DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(UpdatedTime, cstZone);

            return cstTime.ToString().Split(' ')[1].Split(':')[0] + ":" + cstTime.ToString().Split(' ')[1].Split(':')[1];
        }

        // PUT: api/Employees/5  

        public HttpResponseMessage PutRequest(Request req)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }


            dbReq.Entry(req).State = EntityState.Modified;

            try
            {
                dbReq.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST: api/Employees  

        public IHttpActionResult PostRequest(RequestWithoutDate request)
        {
            Tuple<bool, string, IQueryable<Vehicle>> tuple;
            string msg = "";
            try
            {
           
                Vehicle result = dbVeh.Vehicles.SingleOrDefault(b => b.VehicleID == request.VehicleID);
                if (result != null && result.EnabledFlag == 0)
                {
                    result.EnabledFlag = 1;
                    dbVeh.SaveChanges();
                    msg = "Araba bırakılmıştır.";
                }
                else if (result != null && result.EnabledFlag == 1)
                {
                    result.EnabledFlag = 0;
                    dbVeh.SaveChanges();
                    msg = "Araba alınmıştır.";
                }
                Request req = new Request
                {
                    RequestID = request.RequestID,
                    PersonID = request.PersonID,
                    VehicleID = request.VehicleID,
                    CreationDate = DateTime.Now,
                    DurationCheck = request.DurationCheck,
                    RequestType = msg
                };
                dbReq.Requests.Add(req);
                dbReq.SaveChanges();

                VehicleController a = new VehicleController();
                tuple = new Tuple<bool, string, IQueryable<Vehicle>>(true, msg, a.GetVehicles());
            }
            catch
            {
                VehicleController a = new VehicleController();
                tuple = new Tuple<bool, string, IQueryable<Vehicle>>(false, "Bir hata meydaba gelmiştir, lütfen Uygulama Geliştirm Birimi ile iletişime geçiniz.", a.GetVehicles());
            }
            
                return Json(tuple);
        }




        // DELETE: api/Employees/5  

        public HttpResponseMessage DeleteRequest(Request req)
        {
            Request remove_req = dbReq.Requests.Find(req.RequestID);
            if (remove_req == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            dbReq.Requests.Remove(remove_req);
            try
            {
                dbReq.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dbReq.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        [Route("api/RequestExists")]
        private bool RequestExists(int id)
        {
            return dbReq.Requests.Count(e => e.RequestID == id) > 0;
        }
    }
}
