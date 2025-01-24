using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
 
namespace WakalaPlus.Shared
{
    public class ServerResponse
    {
        [Key]
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public int TotalCount { get; set; }
        public object? Data { get; set; }
        public IEnumerable<object>? DataList { get; set; }
        [JsonIgnore]
        public object? Object1 { get; set; }
        [JsonIgnore]
        public object? Object2 { get; set; }
        [JsonIgnore]
        public IEnumerable<object>? ObjectList1 { get; set; }
        [JsonIgnore]
        public IEnumerable<object>? ObjectList2 { get; set; }
    }
    public class ExecutionResult
    {
        public ServerResponse _serverResponse;
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }

        public ExecutionResult()
        {
           // LogWriter.logDir = Functions.GetLogFolderPath();

            this._serverResponse = new ServerResponse();
            this._serverResponse.Success = true;
            this._serverResponse.Message = "";
            this._serverResponse.TotalCount = 0;
            this._serverResponse.Data = null;
            this._serverResponse.DataList = null;
            this._serverResponse.StatusCode = (int)HttpStatusCode.OK;
        }

        public void SetBadRequestError(string Message)
        {
            this._serverResponse.Success = false;
            this._serverResponse.Message = Message;
            this._serverResponse.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        public void SetUnauthorized()
        {
            this._serverResponse.Success = true;
            this._serverResponse.Message = "ACCESS DENIED";
            this._serverResponse.Data = false;
            this._serverResponse.StatusCode = (int)HttpStatusCode.BadRequest;
        }

        public void SetAuthorized()
        {
            this._serverResponse.Success = true;
            this._serverResponse.Message = "";
            this._serverResponse.Data = true;
            this._serverResponse.StatusCode = (int)HttpStatusCode.OK;
        }

        public void SetGeneralInfo(
            string className,
            string functionName,
            string message)
        {
            this._serverResponse.Success = true;
            this._serverResponse.Message = message;
            this._serverResponse.StatusCode = (int)HttpStatusCode.OK;

         }

        public void SetGeneralError(
            string className,
            string functionName,
            string message)
        {
            this._serverResponse.Success = false;
            this._serverResponse.Message = message;
            this._serverResponse.StatusCode = (int)HttpStatusCode.BadRequest;

         
        }

        public void SetInternalServerError(
            string className,
            string functionName,
            Exception ex)
        {
            this._serverResponse.Success = false;
            this._serverResponse.Message = ex.Message;
            this._serverResponse.StatusCode = (int)HttpStatusCode.InternalServerError;

            
        }

        public void SetData(object data)
        {
            this._serverResponse.Data = data;
        }

        public void SetObject1(object object1)
        {
            this._serverResponse.Object1 = object1;
        }

        public void SetObject2(object object2)
        {
            this._serverResponse.Object2 = object2;
        }

        public void SetObjectList1(IEnumerable<object> objectList1)
        {
            this._serverResponse.ObjectList1 = objectList1;
        }

        public void SetObjectList2(IEnumerable<object> objectList2)
        {
            this._serverResponse.ObjectList2 = objectList2;
        }

        public void SetDataList(IEnumerable<object> dataList)
        {
            this._serverResponse.DataList = dataList;
        }

        public void SetTotalCount(int totalCount)
        {
            this._serverResponse.TotalCount = totalCount;
        }

        public ServerResponse GetServerResponse()
        {
            return this._serverResponse;
        }

        public bool GetSuccess()
        {
            return this._serverResponse.Success;
        }

        public int GetStatusCode()
        {
            return this._serverResponse.StatusCode;
        }

        public object? GetData()
        {
            return this._serverResponse.Data;
        }

        public object? GetObject1()
        {
            return this._serverResponse.Object1;
        }

        public object? GetObject2()
        {
            return this._serverResponse.Object2;
        }

        public IEnumerable<object>? GetObjectList1()
        {
            return this._serverResponse.ObjectList1;
        }

        public IEnumerable<object>? GetObjectList2()
        {
            return this._serverResponse.ObjectList2;
        }

        public IEnumerable<object>? GetDataList()
        {
            return this._serverResponse.DataList;
        }

        public void SetSuccess(string message)
        {
            this.IsSuccess = true;
            this.Message = message;
        }

    }
}
