using System.Runtime.ExceptionServices;

namespace AnjUx.Shared.Extensions
{
    public static class ExceptionExtensions
    {
        public static Exception ReThrow(this Exception e)
        {
            ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(e);

            edi.Throw();

            // Retorna a própria exceção, para métodos em que o "throw" interrompe o fluxo de execução
            return e;
        }
    }
}
