using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft;
using System.IO;
using Newtonsoft.Json.Linq;


namespace Ushahidi.Library.Network
{
   public class WebProxy
    {
        HttpClient client;
        

        public WebProxy()
        {
            client = new HttpClient();
        }

       
       public List<Incident> Incidents
        {
            get;
            set;
        }

        public async Task GetIncidents(Deployment deployment)
        {
 
          var value= await this._GetIncidents(deployment);
          this.Incidents = value;
        }


        public async Task<List<Incident>> _GetIncidents(Deployment deployment) {
            string url = deployment.url +"api?task=incidents";

            var response= await client.GetStringAsync(new Uri(url));

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RootObject));
            RootObject root;
            using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(response.ToString())))
            {
                root = serializer.ReadObject(stream) as RootObject;
            }

            return root.payload.incidents;
           
        }



        public async Task<List<Deployment>> GetDeployments(int mDistance=0, double lat=0,double lon=0)
        {


            List<Deployment> deployments = new List<Deployment>();

            string url = "http://tracker.ushahidi.com/list/";

            StringBuilder fullUrl = new StringBuilder(url);           

            fullUrl.Append("?return_vars=name,latitude,longitude,description,url,category_id,discovery_date,id,ss1");
            fullUrl.Append("&units=km");
           // fullUrl.Append("&distance=" + mDistance);
            //fullUrl.Append("&lat=" + (lat));
            //fullUrl.Append("&lon=" + lat);
            client.MaxResponseContentBufferSize = Int32.MaxValue;

           var response = await client.GetStringAsync(new Uri(fullUrl.ToString()));

            JsonObject objs = JsonObject.Parse(response);           

            foreach (JsonValue jsonvalue in objs.Values)
            {
                
                
                Object objects = jsonvalue.GetObject();
                JsonObject jsonobj = (JsonObject)objects;
                try
                {
                    Deployment deploy = new Deployment
                    {
                        id = jsonobj.GetNamedString("id"),
                        description = jsonobj.GetNamedString("description"),
                        url = jsonobj.GetNamedString("url"),
                        name = jsonobj.GetNamedString("name"),                        
                        latitude = jsonobj.GetNamedString("latitude"),
                        longitude = jsonobj.GetNamedString("longitude"),

                    };


                    try { deploy.ss1 = jsonobj.GetNamedString("ss1"); }
                    catch (Exception exp) { }


                    deployments.Add(deploy);
                }
                     
                catch (Exception ex)
                {

                }

              
            }
            return deployments;

          
        }




            
    }
}
