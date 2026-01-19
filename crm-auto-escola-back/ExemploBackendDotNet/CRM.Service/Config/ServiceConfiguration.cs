using System.Reflection;

namespace MPS.Exemplo.Service.Config
{
    public static class ServiceConfiguration
    {
        public static Assembly GetAssembly() => typeof(ServiceConfiguration).Assembly;
    }
}
