
namespace Ushahidi.Library.Network
{
    /// <summary>
    /// A Response object that represents the response 
    /// from the sever after an upload operation
    /// </summary>
    public class UploadResponse
    {
        /// <summary>
        /// A message from the server
        /// describes the result of the request (Failure or Success)
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// A simple feedback message from the server
        /// </summary>
        public string info { get; set; }
        /// <summary>
        /// An indicator if the operation was successful or not.
        /// </summary>
        public bool success
        {
            get
            {
                if (message == "success")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
