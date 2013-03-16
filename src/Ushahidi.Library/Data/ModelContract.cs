using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Device.Location;
using System.Windows;

namespace Ushahidi.Library.Data
{

    public class IncidentObj
    {
        public string incidentid { get; set; }
        public string incidenttitle { get; set; }
        public string incidentdescription { get; set; }
        public string incidentdate { get; set; }
        public string incidentmode { get; set; }
        public string incidentactive { get; set; }
        public string incidentverified { get; set; }
        public string locationid { get; set; }
        public string locationname { get; set; }
        public string locationlatitude { get; set; }
        public string locationlongitude { get; set; }
        public double Latitude
        {
            get
            {
                double d;
                double.TryParse(this.locationlatitude, out d);
                return d;
            }
        }
        public double Longitude
        {
            get
            {
                double d;
                double.TryParse(this.locationlongitude, out d);
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

        public string LocationText
    {
        get{
            return string.Format("{0},{1}", this.locationlongitude, this.locationlatitude);
    }
    }

        public DateTime date
        {
            get
            {
                return DateTime.Parse(incidentdate);
            }
        }

        public string DateText
        {
            get
            {
                return this.date.ToLongDateString();
            }
        }

        public string RelativeDate
        {
            get
            {
                RelativeDateTimeConverter rv = new RelativeDateTimeConverter();
                return rv.Convert(DateTime.Parse(incidentdate));
            }
        }
    }

    public class Medium
    {
        public string id { get; set; }
        public string type { get; set; }
        public string link { get; set; }
        public string thumb { get; set; }
    }

    public class Incident
    {
        public IncidentObj incident { get; set; }
        public List<Category> categories { get; set; }
        public List<Medium> media { get; set; }
        public List<object> comments { get; set; }
        public string CategoryText {
            get
            {
                string x="";
                int i = 0;
                if (this.categories == null)
                {
                    return "";
                }
                foreach (Category c in this.categories){

                    if (i == 0) { x += c.category.title; }
                    else { x +=", "+ c.category.title; }
                    i++;
                }
                return x;
            }
        }

        public Visibility isVisible {
            get
            {
                if (this.media!=null && this.media.Count > 0)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

    }

    public class Payload
    {
        public string domain { get; set; }
        public List<Incident> incidents { get; set; }
    }

    public class Error
    {
        public string code { get; set; }
        public string message { get; set; }
    }
    public class RootObject
    {
        public Payload payload { get; set; }
        public Error error { get; set; }
    }  

    #region "Category"

    public class CategoryObj
    {
        public string id { get; set; }
        public string parent_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string color { get; set; }
        public string position { get; set; }
        public string icon { get; set; }
    }

    public class Category
    {
        public CategoryObj category { get; set; }
    }

    public class PayloadCategory
    {
        public string domain { get; set; }
        public List<Category> categories { get; set; }
    }

   

    public class RootObjectCategory
    {
        public PayloadCategory payload { get; set; }
        public Error error { get; set; }
    }



   

    #endregion


#region Comments

    public class Comment
    {
        
        public string  id {get;set;}
        public string  incident_id {get;set;}
        public string  comment_author {get;set;}
        public string  comment_email {get;set;}
        public string  comment_description {get;set;}
        public string  comment_rating {get;set;}
        public string  comment_date {get;set;}
    } 


    public class PayloadComments
    {
        public string domain { get; set; }
        public List<Comment> comments { get; set; }
    }


    public class RootObjectComments
    {
        public PayloadComments payload { get; set; }
        public Error error { get; set; }
    }

#endregion

}
