using System;
using System.Net;
using System.Linq;
using System.Data.Linq;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Device.Location;

namespace Ushahidi.Library.Data
{
    public class Database : DataContext
    {

        public Table<Deployments> Deployment {

            get
            {
                return this.GetTable<Deployments>();
            }
        }

        public Table<DeploymentCategory> DeploymentCategory
        {

            get
            {
                return this.GetTable<DeploymentCategory>();
            }
        }

        public Database(string Connection)
            : base(Connection)
        {

        }

    }


    [Table]
    public class Deployments
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int databaseID { get; set; }
        [Column]
        public string id { get; set; }
        [Column]
        public string name { get; set; }
        [Column]
        public string url { get; set; }
        [Column]
        public string description { get; set; }
        [Column]
        public string category_id { get; set; }
        [Column]
        public string latitude { get; set; }
        [Column]
        public string longitude { get; set; }
        [Column]
        public string discovery_date { get; set; }
        [Column]
        public string ss1 { get; set; }
        [Column]
        public bool isLocal { get; set; }


        public double Latitude
        {
            get
            {
                double d;
                double.TryParse(this.latitude, out d);
                return d;
            }
        }
        public double Longitude
        {
            get
            {
                double d;
                double.TryParse(this.latitude, out d);
                return d;
            }
        }

        public GeoCoordinate Location
        {
            get
            {
                GeoCoordinate geo = new GeoCoordinate(this.Latitude, this.Longitude);
                return geo;
            }
        }
    }

    [Table]
    public class DeploymentCategory
    {
        [Column(IsPrimaryKey=true)]
        public string id
        {

            get;
            set;
        }
         [Column]
        public string name
        {
            get;
            set;
        }

    }

}
