namespace AnjUx.Client.Services
{
    public class LoadingService : IServiceBase
    {
        public event Action? OnLoadingChanged;

        private int isLoading = 0;

        public bool IsLoading
        {
            get => isLoading > 0;
            set
            {
                if (!value && isLoading == 0) return;

                int oldLoading = isLoading;

                if (value)
                {
                    isLoading++;
                    if (oldLoading == 0) OnLoadingChanged?.Invoke();
                }
                else
                {
                    isLoading--;
                    if (oldLoading > 0 && isLoading == 0) OnLoadingChanged?.Invoke();
                }
            }
        }


    }

}
