//using Microsoft.AspNetCore.Mvc;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;

//namespace KonSanad.APIS.Filters
//{
//    public class SwaggerJsonBodyOperationFilter : IOperationFilter
//    {
//        public void Apply(OpenApiOperation operation, OperationFilterContext context)
//        {
//            // تحقق من وجود [FromBody]
//            var hasFromBodyAttribute = context.MethodInfo.GetParameters()
//                .Any(p => p.GetCustomAttributes(true).OfType<FromBodyAttribute>().Any());

//            if (hasFromBodyAttribute)
//            {
//                var parameter = context.MethodInfo.GetParameters()
//                    .First(p => p.GetCustomAttributes(true).OfType<FromBodyAttribute>().Any());

//                // امسح أي RequestBody موجود
//                operation.RequestBody = new OpenApiRequestBody
//                {
//                    Required = true,
//                    Content = new Dictionary<string, OpenApiMediaType>
//                    {
//                        ["application/json"] = new OpenApiMediaType
//                        {
//                            Schema = context.SchemaGenerator.GenerateSchema(
//                                parameter.ParameterType,
//                                context.SchemaRepository)
//                        }
//                    }
//                };

//                // امسح الـ Parameters اللي Swagger بيضيفها تلقائي
//                var parametersToRemove = operation.Parameters
//                    .Where(p => p.In == ParameterLocation.Query || p.In == ParameterLocation.Path)
//                    .ToList();

//                foreach (var param in parametersToRemove)
//                {
//                    operation.Parameters.Remove(param);
//                }
//            }
//        }
//    }
//}