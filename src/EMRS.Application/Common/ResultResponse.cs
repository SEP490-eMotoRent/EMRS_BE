namespace EMRS.Application.Common
{
    public class ResultResponse<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }
        public int Code { get; private set; }

        private ResultResponse(bool success, string message, T? data, int code)
        {
            Success = success;
            Message = message;
            Data = data;
            Code = code;
        }

        public static ResultResponse<T> SuccessResult(string message, T data, int code = ResultCodes.Success)
            => new(true, message, data, code);

        public static ResultResponse<List<T>> SuccessList(string message, List<T> data)
        => new(true, message, data, ResultCodes.SuccessList);
        public static ResultResponse<T> Failure(string message, int code = ResultCodes.BadRequest)
            => new(false, message, default, code);

        public static ResultResponse<T> NotFound(string message)
            => new(false, message, default, ResultCodes.NotFound);

        public static ResultResponse<T> Unauthorized(string message)
            => new(false, message, default, ResultCodes.Unauthorized);

        public static ResultResponse<T> Forbidden(string message)
            => new(false, message, default, ResultCodes.Forbidden);


        public static ResultResponse<T> ServerError(string message)
            => new(false, message, default, ResultCodes.InternalServerError);
    }
}
