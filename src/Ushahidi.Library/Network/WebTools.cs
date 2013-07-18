using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using RestSharp;
using Ushahidi.Library.Data;

namespace Ushahidi.Library.Network
{

	/// <summary>
	/// Performs all web operations 
	/// - Upload the reports
	/// </summary>
	public class WebTools
	{

		/// <summary>
		/// Reports to be uploaded.
		/// </summary>
		public IList<UploadReport> Uploads { get; private set; }

		/// <summary>
		/// Single Upload complete Event.
		/// </summary>
		public event EventHandler<SingleUploadFinishedEventArgs> SingleUploadFinished;

		/// <summary>
		/// All uploads complete Event
		/// </summary>
		public event EventHandler<AllUploadsFinishedEventArgs> AllUploadsFinished;

		/// <summary>
		/// Any operation failure tracking event.
		/// </summary>
		public event EventHandler<OperationFailedEventArgs> OperationFailed;


		/// <summary>
		/// Fired when deployments have been downloaded sucessfully.
		/// </summary>
		public event EventHandler<DownloadCompleteArgs> DataDownloadComplete;

		/// <summary>
		/// Fired when there is info can't be fetched from the server.
		/// When an error message is returned from the server.
		/// </summary>
		public event EventHandler<DownloadCompleteArgs> DataDownloadCompleteWithError;

		/// <summary>
		/// Server URL
		/// </summary>
		public static string TrackerUrl = "http://tracker.ushahidi.com/list/";

		private Settings LocalSettings;

		public WebTools()
		{

		}

		public WebTools(Settings mysettings)
		{
			LocalSettings = mysettings;
		}

		public void ReportUpload(IList<UploadReport> incidents)
		{
			_ReportUpload(incidents);
		}


		/// <summary>
		/// Single Report Upload.
		/// </summary>
		/// <param name="Report"> A Report</param>
		public void ReportUpload(UploadReport incident)
		{
			List<UploadReport> reports = new List<UploadReport>(){
				incident
			};

			_ReportUpload(reports);
		}

	  public  void SearchDeployments(string search)
		{

			_DownloadDeployments(new List<Parameter>() { }, "?q=" + search);


		}
	   

		/// <summary>
		/// Download all deployments
		/// </summary>
	   public void DownloadDeployments()
		{
			List<Parameter> parameters = new List<Parameter>(){
			 new  Parameter(){ Name = "return_vars", Type= ParameterType.GetOrPost, Value="name,latitude,longitude,description,url,category_id,discovery_date,id,ss1"},   
			 new  Parameter(){ Name = "distance", Type= ParameterType.GetOrPost, Value=LocalSettings.Distance},
			 new  Parameter(){ Name = "lat", Type= ParameterType.GetOrPost, Value=LocalSettings.Location.Latitude},
			 new  Parameter(){ Name = "lon", Type= ParameterType.GetOrPost, Value=LocalSettings.Location.Longitude},
			 new  Parameter(){ Name = "units", Type= ParameterType.GetOrPost, Value="km"},   
			 new  Parameter(){ Name = "has_description", Type= ParameterType.GetOrPost, Value=true},     
			  new  Parameter(){ Name = "minpopularity", Type= ParameterType.GetOrPost, Value=10} 
		  
			};

			_DownloadDeployments(parameters);

		}

		private void _DownloadDeployments(List<Parameter> Parameters, string urlPart="")
		{

			var restClient = new RestClient(TrackerUrl+urlPart);
			var request = new RestRequest(Method.GET);
			foreach (Parameter p in Parameters)
			{

				request.AddParameter(p);

			}            
			restClient.ExecuteAsync(request, response =>
			{

				if (response.StatusCode == HttpStatusCode.OK)
				{

					JToken job = JToken.Parse(response.Content);
					List<Deployments> deployments = new List<Deployments>();
					foreach (JObject jvalue in job.Values())
					{


						Deployments deploy = new Deployments
						{
							id = (string)jvalue.SelectToken("id"),
							description = (string)jvalue.SelectToken("description"),
							url = (string)jvalue.SelectToken("url"),
							name = (string)jvalue.SelectToken("name"),
							latitude = (string)jvalue.SelectToken("latitude"),
							longitude = (string)jvalue.SelectToken("longitude"),
							category_id = (string)jvalue.SelectToken("category_id"),
							discovery_date = (string)jvalue.SelectToken("discovery_date"),
							ss1 = (string)jvalue.SelectToken("ss1")
						};

						deployments.Add(deploy);
					}



					DataDownloadComplete.Invoke(this, new DownloadCompleteArgs()
					{
						DownloadObject = deployments,
						Response = response

					});

				}

			});



		}

		/// <summary>
		/// Try to download incidents from the server.
		/// </summary>
		/// <param name="deployment"></param>
		public void DownloadIncidents(Deployments deployment)
		{
			var restClient = new RestClient(deployment.url + "/api?task=incidents&by=all&limit=100");
			var request = new RestRequest(Method.GET);
			//request.AddParameter("by", "all");           
			//request.AddParameter("limit", "100");
			restClient.ExecuteAsync(request, response =>
			 {

				 if (response.Content.Trim() == string.Empty)
				 {
					 DataDownloadComplete.Invoke(this, new DownloadCompleteArgs()
					 {
						 DownloadObject = new List<Incident>(),
						 Response = response

					 });
				 }
				 else
				 {

					 DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RootObject));
					 RootObject root;
					 using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(response.Content)))
					 {
						 root = serializer.ReadObject(stream) as RootObject;
					 }

					 if (root.payload == null)
					 {
						 RestSharp.Deserializers.JsonDeserializer dj = new RestSharp.Deserializers.JsonDeserializer();
						 Error requestError = dj.Deserialize<Error>(response);
						 DataDownloadCompleteWithError.Invoke(this, new DownloadCompleteArgs()
						 {
							 DownloadObject = requestError,
							 Response = response

						 });

					 }
					 else
					 {
						 DataDownloadComplete.Invoke(this, new DownloadCompleteArgs()
						 {
							 DownloadObject = root.payload.incidents,
							 Response = response

						 });
					 }
				 }

			 });
		}

		public void DownloadCategories(Deployments deployment)
		{
			var restClient = new RestClient(deployment.url + "/api?task=categories");
			var request = new RestRequest(Method.GET);

			restClient.ExecuteAsync(request, response =>
			 {
				 DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RootObjectCategory));
				 RootObjectCategory root;
				 using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(response.Content)))
				 {
					 root = serializer.ReadObject(stream) as RootObjectCategory;
				 }

				 if (root.payload == null)
				 {


					 DataDownloadCompleteWithError.Invoke(this, new DownloadCompleteArgs()
					 {
						 DownloadObject = new Error() { message = "Categories download Failed", code = "0" },
						 Response = response

					 });
				 }
				 else
				 {
					 DataDownloadComplete.Invoke(this, new DownloadCompleteArgs()
					 {
						 DownloadObject = root,
						 Response = response
					 });
				 }
			 });
		}

		public void DownloadCategories(Deployments deployment, string IncidentId)
		{
			var restClient = new RestClient(deployment.url + "/api?task=comments&by=reportid&id=" + IncidentId);
			var request = new RestRequest(Method.GET);

			restClient.ExecuteAsync(request, response =>
			{
				DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(RootObjectComments));
				RootObjectComments root;
				using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(response.Content)))
				{
					root = serializer.ReadObject(stream) as RootObjectComments;
				}

				if (root.payload == null)
				{
					DataDownloadCompleteWithError.Invoke(this, new DownloadCompleteArgs()
					{
						DownloadObject = new Error() { message = "Comments download Failed", code = "0" },
						Response = response

					});
				}
				else
				{
					DataDownloadComplete.Invoke(this, new DownloadCompleteArgs()
					{
						DownloadObject = root,
						Response = response
					});
				}
			});
		}


		public void DownloadDeploymentCategories()
		{
			var restClient = new RestClient(TrackerUrl);
			var request = new RestRequest(Method.GET);
			request.AddParameter("cat", "all");
			restClient.ExecuteAsync(request, response =>
			{
				try
				{
					JToken job = JToken.Parse(response.Content);
					List<DeploymentCategory> deploymentcategories = new List<DeploymentCategory>();
					foreach (JObject jvalue in job.Values())
					{
						DeploymentCategory dpCategory = new DeploymentCategory()
						{
							id = (string)jvalue.SelectToken("id"),
							name = (string)jvalue.SelectToken("name")
						};

					}

					DataDownloadComplete.Invoke(this, new DownloadCompleteArgs()
					{
						DownloadObject = deploymentcategories,
						Response = response
					});
				}
				catch
				{

					DataDownloadCompleteWithError.Invoke(this, new DownloadCompleteArgs()
					{
						DownloadObject = new Error() { code = "", message = "Download of categories failed" },
						Response = response
					});
				}

			});
		}



		public void SingleUpload(UploadReport report)
		{

			string userId = "10";
			string userHash = "40a6fe73f24b4c73f0d7943c8a41adcb";

			int hour  = (report.IncidentDate.Hour % 12);


			//preparing RestRequest by adding server url, parameteres and files...
			RestRequest request = new RestRequest(report.Deployment.url+ "/api?task=report", Method.POST);
			 request.AddParameter("user_id", userId);
			 request.AddParameter("user_hash", userHash);
			 request.AddParameter("task", "report");
			 request.AddParameter("incident_title", report.incidenttitle);
			 request.AddParameter("incident_description", report.incidentdescription);
			 request.AddParameter("incident_date", DateTime.Now.ToString("MM/dd/yyyy"));
			 request.AddParameter("incident_hour",(hour==0 ? 0 : hour).ToString());
			 request.AddParameter("incident_minute", report.IncidentDate.Minute.ToString());
			 request.AddParameter("incident_ampm", (report.IncidentDate.Hour / 12) == 0 ? "am" : "pm");
			 request.AddParameter("incident_category", report.CategoryList);
			 request.AddParameter("latitude", report.locationlatitude);		
			  request.AddParameter("longitude", report.locationlongitude);
			  request.AddParameter("location_name", report.locationname);
			  //request.AddParameter("incident_video", "");

			  request.AddFile("incident_photo[]", report.Photos[0].Image, report.Photos[0].FileName);
			

			//calling server with restClient
			RestClient restClient = new RestClient();
			restClient.ExecuteAsync(request, (response) =>
			{
				if (response.StatusCode == HttpStatusCode.OK)
				{
					

					//SingleUploadFinished.Invoke(this, new SingleUploadFinishedEventArgs({   }));
					
				}
				else
				{
					
				}
			});


		}

		/// <summary>
		/// Internal report Upload method, uploads a list of reports 
		/// </summary>
		/// <param name="Reports">A list of reports</param>
		private void _ReportUpload(IList<UploadReport> Incidents)
		{
			Uploads = Incidents;

			int upload_count = Incidents.Count;
			foreach (UploadReport r in Incidents)
			{

				SingleUpload(r);
			}
		}
	}

	/// <summary>
	/// Event arguments passed when all Report uploads have completed.
	/// </summary>
	public class AllUploadsFinishedEventArgs : EventArgs
	{
		/// <summary>
		/// List of Reports that were uploaded
		/// </summary>
		public IList<Incident> Uploads { get; set; }


		public AllUploadsFinishedEventArgs() : this(null) { }


		public AllUploadsFinishedEventArgs(IList<Incident> uploads)
			: base()
		{
			this.Uploads = uploads;
		}
	}


	/// <summary>
	/// Event arguments passed one Report uploads has completed.
	/// </summary>
	public class SingleUploadFinishedEventArgs : EventArgs
	{
		/// <summary>
		/// The report that was Uploaded
		/// </summary>
		public UploadReport UploadedReport { get; set; }

		/// <summary>
		/// Upload Response Object that was send to the server
		/// </summary>
		public UploadResponse Response { get; set; }

		public SingleUploadFinishedEventArgs() : this(null, null) { }

		public SingleUploadFinishedEventArgs(UploadReport uploadedReport, UploadResponse response)
		{

			this.UploadedReport = uploadedReport;
			this.Response = response;
		}

	}


	public class DownloadCompleteArgs : EventArgs
	{


		public object DownloadObject { get; set; }

		public IRestResponse Response
		{
			get;
			set;
		}

		public DownloadCompleteArgs(object DowloadObj, IRestResponse response)
		{
			this.Response = response;
			this.DownloadObject = DowloadObj;
		}
		public DownloadCompleteArgs() : this(null, null) { }
	}



	/// <summary>
	/// Event arguments passed  when there is an exception
	/// </summary>
	public class OperationFailedEventArgs : EventArgs
	{
		/// <summary>
		/// The Web request Object 
		/// </summary>
		public object State { get; set; }

		/// <summary>
		/// The exception that caused the failure
		/// </summary>
		public Exception Exception { get; set; }

		public OperationFailedEventArgs() : this(null) { }

		public OperationFailedEventArgs(Exception ex)
		{

			this.Exception = ex;
		}
		public OperationFailedEventArgs(object obj)
		{

			this.State = obj;
		}
		public OperationFailedEventArgs(Exception ex, object obj)
		{
			this.Exception = ex;
			this.State = obj;
		}

	}

}
