using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.DocumentServices.ServiceModel;
using DevExpress.Mvvm;
using DevExpress.Mvvm.POCO;
using DevExpress.ReportServer.ServiceModel.Client;
using DevExpress.ReportServer.ServiceModel.ConnectionProviders;
using DevExpress.ReportServer.ServiceModel.DataContracts;
using DevExpress.XtraPrinting;
using T258870.Service;
using T258870.View;

namespace T258870.ViewModel {
    public class MainViewModel {
        const string ServerAddress = "https://reportserver.devexpress.com";
        readonly ConnectionProvider serverConnection = new GuestConnectionProvider(ServerAddress);

        public ReportObjects SelectedReportObject { get; set; }
        public ObservableCollection<ReportObjects> ReportObjectCollection { get; set; }

        public virtual bool IsBusy { get; protected set; }
        protected void OnIsBusyChanged() {
            this.RaiseCanExecuteChanged(x => x.Preview());
            this.RaiseCanExecuteChanged(x => x.Export());
        }
        
        protected virtual IMessageBoxService MessageBoxService { get { return null; } }
        protected virtual ISaveFileDialogService SaveFileDialogService { get { return null; } }
        protected virtual IReportViewerService reportViewerService { get { return null; } }

        public MainViewModel() {
            SynchronizationContext uiContext = SynchronizationContext.Current;

            ReportObjectCollection = new ObservableCollection<ReportObjects>();
            SelectedReportObject = new ReportObjects() { Id = -1};
            serverConnection
                .ConnectAsync()
                .ContinueWith(task => {
                    IReportServerClient client = task.Result;
                    client.SetSynchronizationContext(uiContext);
                    return client.GetReportsAsync(null);
                }).Unwrap()
                .ContinueWith(task => {
                    if(task.IsFaulted) {
                        MessageBoxService.Show(task.Exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    } else {
                        FillReportListBox(task.Result);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        void FillReportListBox(IEnumerable<ReportCatalogItemDto> reports) {
            foreach(var reportDto in reports) {
                ReportObjects item = new ReportObjects() { DisplayName = reportDto.Name, Id = reportDto.Id };
                this.ReportObjectCollection.Add(item);
            }
        }

        public bool CanExport() {
            return !IsBusy && (SelectedReportObject.Id > 0);
        }

        public void Export() {
            SaveFileDialogService.Filter = "PDF files (*.pdf)|*.pdf";
            if(SaveFileDialogService.ShowDialog()) {
                IsBusy = true;
                ExportToPdf(serverConnection, SaveFileDialogService.GetFullFileName(), SelectedReportObject.Id);
            }
        }

        void ExportToPdf(ConnectionProvider serverConnection, string fileName, int reportId) {
            Task.Factory.ExportReportAsync(serverConnection.CreateClient(), new ReportIdentity(reportId), new PdfExportOptions(), null, null)
                .ContinueWith(task => {
                    IsBusy = false;
                    try { 
                        if(task.IsFaulted) {
                            throw new Exception(task.Exception.Flatten().InnerException.Message);
                        } else if(task.IsCanceled) {
                            MessageBoxService.Show("Operation has been cancelled", "Export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
                            return;
                        } else {                        
                            File.WriteAllBytes(fileName, task.Result);
                        }
                    } catch(Exception e) {
                        MessageBoxService.Show(e.Message, "Export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());  
        }

        public bool CanPreview() {
            return !IsBusy && (SelectedReportObject.Id > 0);
        }

        public void Preview() {
            reportViewerService.connectionProvider = serverConnection;
            try {
                reportViewerService.Show(SelectedReportObject.Id);                
            } catch(Exception e) {
                MessageBoxService.Show(e.Message, "Preview", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            
        }
    }
}
