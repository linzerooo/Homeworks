using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using MyHttpServer.Attributes;
using Newtonsoft.Json;

namespace MyHttpServer.Handlers;

public class ControllersHandler : Handler
{
    public override void HandleRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            using var response = context.Response;

            var strParams = request.Url!
                .Segments
                .Skip(1)
                .Select(s => s.Replace("/", ""))
                .ToArray();

            var controllerName = strParams[0];
            var methodName = strParams[1];

            var assembly = Assembly.GetExecutingAssembly();

            var controller = assembly.GetTypes()
                .Where(t => Attribute.IsDefined(t, typeof(ControllerAttribute)))
                .FirstOrDefault(c =>
                    ((ControllerAttribute)Attribute.GetCustomAttribute(c, typeof(ControllerAttribute))!)
                    .ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase));

            var method = controller?.GetMethods()
                .FirstOrDefault(x => x.GetCustomAttributes(true)
                    .Any(attr => attr.GetType().Name.Equals($"{request.HttpMethod}Attribute",
                                     StringComparison.OrdinalIgnoreCase) &&
                                 ((HttpMethodAttribute)attr).ActionName.Equals(methodName,
                                     StringComparison.OrdinalIgnoreCase)));
            
            var id =  method!.Name.Equals("Delete") || method.Name.Equals("GetById") || method.Name.Equals("Update")  ? strParams[2] : null;

            var queryParams = method.GetParameters()
                .Select((p, i) =>
                {
                    if (p.ParameterType == typeof(int) && i == 0)
                    {
                        // Преобразование параметра id в int
                        return Convert.ChangeType(id, p.ParameterType);
                    }

                    return Convert.ChangeType(strParams[i], p.ParameterType);
                })
                .ToArray();
            
            if (request is { HttpMethod: "POST", HasEntityBody: true } &&
                methodName.Equals("SendToEmail", StringComparison.OrdinalIgnoreCase))
            {
                var encoding = request.ContentEncoding;
                var reader = new StreamReader(request.InputStream, encoding);

                var parsedData = HttpUtility.ParseQueryString(reader.ReadToEnd());
                var email = parsedData["email"];
                var password = parsedData["password"];

                var resultFromMethod = method.Invoke(Activator.CreateInstance(controller!),
                    new object[] { email!, password! });
                ProcessResult(resultFromMethod, response, context);
            }
            else if (methodName.Equals("Add", StringComparison.OrdinalIgnoreCase))
            {
                var login = strParams[2];
                var password = strParams[3];
                var resultFromMethod = method.Invoke(Activator.CreateInstance(controller!),
                    new object[] { login, password });
                ProcessResult(resultFromMethod, response, context);
            }
            else if (methodName.Equals("getbyid", StringComparison.OrdinalIgnoreCase))
            {
                var resultFromMethod = method.Invoke(Activator.CreateInstance(controller!), new object[] { id! });
                ProcessResult(resultFromMethod, response, context);
            }
            else if (methodName.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                var resultFromMethod = method.Invoke(Activator.CreateInstance(controller!), new object[] { id! });
                ProcessResult(resultFromMethod, response, context);
            }
            else if (methodName.Equals("update", StringComparison.OrdinalIgnoreCase))
            {
                var login = strParams[3];
                var password = strParams[4];
                var resultFromMethod = method.Invoke(Activator.CreateInstance(controller!),
                    new object[] { id!, login, password });
                ProcessResult(resultFromMethod, response, context);
            }
            else
            {
                var resultFromMethod = method.Invoke(Activator.CreateInstance(controller!), queryParams);
                ProcessResult(resultFromMethod, response, context);
            }
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private static void ProcessResult<T>(T result, HttpListenerResponse response, HttpListenerContext context)
    {
        switch (result)
        {
            case string resultOfString:
            {
                response.ContentType = StaticFilesHandler.GetContentType(context.Request.Url!.LocalPath);
                var buffer = Encoding.UTF8.GetBytes(resultOfString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                break;
            }
            case not null:
            {
                response.ContentType = StaticFilesHandler.GetContentType(context.Request.Url!.LocalPath);
                var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                break;
            }
        }
    }
}