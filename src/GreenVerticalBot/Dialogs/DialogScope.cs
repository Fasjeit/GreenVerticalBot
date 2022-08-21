//using Microsoft.Extensions.DependencyInjection;

//namespace GreenVerticalBot.Dialogs
//{
//    internal class DialogScope : IDisposable
//    {
//        private bool disposedValue;
//        public IServiceScope ServiceScope { get; private set; }

//        public readonly Dictionary<string, object> Properties;

//        public DialogScope(IServiceProvider serviceProvider)
//        {
//            this.ServiceScope = serviceProvider.CreateScope();
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!this.disposedValue)
//            {
//                if (disposing)
//                {
//                    // dispose managed state (managed objects)
//                    this.ServiceScope.Dispose();
//                }

//                // free unmanaged resources (unmanaged objects) and override finalizer
//                // set large fields to null
//                this.disposedValue = true;
//            }
//        }

//        public void Dispose()
//        {
//            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//            this.Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}
