using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using ArabaBendeAPI.Models;
using System.Web.Http;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Newtonsoft.Json;

namespace ArabaBendeAPI.Controllers
{
    public class VehicleController : ApiController
    {
        private arabaDBEntities10 db = new arabaDBEntities10();

        // GET: api/Vehicles  
        public IQueryable<Vehicle> GetVehicles()
        {
            return db.Vehicles;
        }



        // PUT: api/Vehicles/5  

        public HttpResponseMessage PutVehicle(Vehicle Vehicle)
        {
            if (ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }


            db.Entry(Vehicle).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST: api/Vehicles  

        public IHttpActionResult PostVehicle([FromBody]  Vehicle Vehicle)
        {
            if (ModelState.IsValid)
            {
                db.Vehicles.Add(Vehicle);
                db.SaveChanges();
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, Vehicle);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = Vehicle.VehicleID }));
                return Json(Vehicle);
            }
            else
            {
                return Json(Vehicle);
            }

        }

        // DELETE: api/Vehicles/5  

        public HttpResponseMessage DeleteVehicle(Vehicle Vehicle)
        {
            Vehicle remove_Vehicle = db.Vehicles.Find(Vehicle.VehicleID);
            if (remove_Vehicle == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Vehicles.Remove(remove_Vehicle);
            try
            {
                db.SaveChanges();
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
                db.Dispose();
            }
            base.Dispose(disposing);
        }
      
        [HttpGet]
        [Route("api/VehicleInUsed")]
        public IQueryable<Vehicle> VehicleInUsed(int id)
        {
            Vehicle result = db.Vehicles.SingleOrDefault(b => b.VehicleID == id);
            if (result != null&& result.EnabledFlag==0)
            {
                result.EnabledFlag = 1;
                db.SaveChanges();
            }else if(result != null && result.EnabledFlag == 1)
            {
                result.EnabledFlag = 0;
                db.SaveChanges();
            }
            return GetVehicles();
        }
        [HttpGet]
        [Route("api/VehicleInUsedReservation")]
        public IQueryable<Vehicle> VehicleInUsedReservation(int id)
        {
            Vehicle result = db.Vehicles.SingleOrDefault(b => b.VehicleID == id);
            if (result != null && result.ReservationFlag == 0)
            {
                result.ReservationFlag = 1;
                db.SaveChanges();
            }
            else if (result != null && result.ReservationFlag == 1)
            {
                result.ReservationFlag = 0;
                db.SaveChanges();
            }
            return GetVehicles();
        }


    }
}
