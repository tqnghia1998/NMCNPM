﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DbModel
{
    /// <summary>
    /// Provides HTTP calls for sending and receiving information from a HTTP server
    /// </summary>
    public static class WebRequests
    {
        /// <summary>
        /// POSTs a web request to an URL and returns the raw http web response
        /// </summary>
        /// <remarks>IMPORTANT: Remember to close the returned <see cref="HttpWebResponse"/> stream once done</remarks>
        /// <param name="url">The URL</param>
        /// <param name="content">The content to post</param>
        /// <param name="sendType">The format to serialize the content into</param>
        /// <param name="returnType">The expected type of content to be returned from the server</param>
        /// <param name="configureRequest">Allows caller to customize and configure the request prior to the content being written and sent</param>
        /// <param name="bearerToken">If specified, provides the Authorization header with `bearer token-here` for things like JWT bearer tokens</param>
        /// <returns></returns>
        public static async Task<HttpWebResponse> PostAsync(
            string url, 
            object content = null, 
            KnownContentSerializers sendType = KnownContentSerializers.Json, 
            KnownContentSerializers returnType = KnownContentSerializers.Json, 
            Action<HttpWebRequest> configureRequest = null, 
            string bearerToken = null)
        {
            #region Setup
            
            // Create the web request
            var request = WebRequest.CreateHttp(url);

            // Make it a POST request method
            request.Method = HttpMethod.Post.ToString();

            // Set the appropriate
            request.Accept = returnType.ToMimeString();

            // Set the content type
            request.ContentType = sendType.ToMimeString();

            // If we have a bearer token...
            if (bearerToken != null)
            {
                // Add bearer token to header
                request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {bearerToken}");
            }

            // Any custom work
            configureRequest?.Invoke(request);

            #endregion

            #region Write Content

            if(content == null)
            {
                // Set content length to 0
                request.ContentLength = 0;
            }
            else
            {
                // Create content to write
                string contentString = string.Empty;

                // Serialize to Json
                if(sendType == KnownContentSerializers.Json)
                {
                    // Serialize content to Json string
                    contentString = JsonConvert.SerializeObject(content);
                }
                else if(sendType == KnownContentSerializers.Xml)
                {
                    // Create Xml serializer
                    var xmlSerializer = new XmlSerializer(content.GetType());

                    // Create a string writer to receive the serialized string
                    using (var stringWriter = new StringWriter())
                    {
                        // Serialize the object to a string
                        xmlSerializer.Serialize(stringWriter, content);

                        // Extract the string from the writer
                        contentString = stringWriter.ToString();
                    }
                }

                var contentUTF8 = Encoding.UTF8.GetBytes(contentString);

                // Get body stream...
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    await requestStream.WriteAsync(contentUTF8, 0, contentUTF8.Length);
                }
            }

            #endregion

            // Wrap call...
            try
            {
                // Return the raw server response
                return await request.GetResponseAsync() as HttpWebResponse;
            }
            catch(WebException ex)
            {
                // If we got a response
                if (ex.Response is HttpWebResponse httpResponse)
                {
                    // return the response
                    return httpResponse;
                }

                // Otherwise, we don't have any information to be able to return
                // So re-throw
                throw;
            }
        }

        /// <summary>
        /// POSTs a web request to an URL and returns a response of the expected data type
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="content">The content to post</param>
        /// <param name="sendType">The format to serialize the content into</param>
        /// <param name="returnType">The expected type of content to be returned from the server</param>
        /// <param name="configureRequest">Allows caller to customize and configure the request prior to the content being written and sent</param>
        /// <param name="bearerToken">If specified, provides the Authorization header with `bearer token-here` for things like JWT bearer tokens</param>
        public static async Task<WebRequestResult<TResponse>> PostAsync<TResponse>(
            string url, 
            object content = null, 
            KnownContentSerializers sendType = KnownContentSerializers.Json, 
            KnownContentSerializers returnType = KnownContentSerializers.Json, 
            Action<HttpWebRequest> configureRequest = null, 
            string bearerToken = null)
        {
            // Create server response holder
            var serverResponse = default(HttpWebResponse);

            try
            {
                // Make the standard Post call first
                serverResponse = await PostAsync(url, content, sendType, returnType, configureRequest, bearerToken);
            }
            catch (Exception ex)
            {
                // If we got unexpected error, return that
                return new WebRequestResult<TResponse>
                {
                    // Include exception message
                    ErrorMessage = ex.Message
                };
            }

            // Create a result
            var result = serverResponse.CreateWebRequestResult<TResponse>();

            // If the response status code is not 200...
            if (result.StatusCode != HttpStatusCode.OK)
            {
                // Call failed
                // Return no error message so the client can display its own based on the status code
                
                // Done
                return result;
            }

            // If we have no content to deserialize...
            if (string.IsNullOrEmpty(result.RawServerResponse))
            {
                // Done
                return result;
            }

            // Deserialize raw respone
            try
            {
                // If the server response type was not the expected type...
                if (!serverResponse.ContentType.ToLower().Contains(returnType.ToMimeString().ToLower()))
                {
                    // Fail due to unexpected return type
                    result.ErrorMessage = $"Server did not return data in expected type. Expected {returnType.ToMimeString()}, receive {serverResponse.ContentType}";

                    // Done 
                    return result;
                }

                // Json ?
                if (returnType == KnownContentSerializers.Json)
                {
                    // Deserialize Json string
                    result.ServerResponse = JsonConvert.DeserializeObject<TResponse>(result.RawServerResponse);

                    // Done 
                    return result;
                }
                // Xml ?
                else if (returnType == KnownContentSerializers.Xml)
                {
                    // Create Xml serializer
                    var xmlSerializer = new XmlSerializer(typeof(TResponse));

                    // Create a memory stream for the raw string data
                    using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(result.RawServerResponse)))
                    {
                        // Deserialize Xml string
                        result.ServerResponse = (TResponse)xmlSerializer.Deserialize(memoryStream);
                    }
                }
                // Unknown
                else
                {
                    // If deserialize failed then set error message
                    result.ErrorMessage = "Unknown return type, cannot deserialize server response to the expected type";

                    // Done
                    return result;
                }
            }
            catch(Exception ex)
            {
                // If deserialize failed then set error message
                result.ErrorMessage = "Failed to deserialize server response to the expected type";

                // Done
                return result;
            }

            // Return result
            return result;
        }
    }
}
