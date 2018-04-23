using System.Windows;
using DevExpress.Mvvm.UI;
using DevExpress.ReportServer.Printing;
using DevExpress.ReportServer.ServiceModel.ConnectionProviders;
using DevExpress.ReportServer.ServiceModel.DataContracts;
using DevExpress.Utils;
using DevExpress.Xpf.Printing;
using T258870.View;

namespace T258870.Service {
    public class ReportViewerService : ServiceBase, IReportViewerService {        
        public ConnectionProvider connectionProvider { get; set; }
        ReportViewerView reportViewerView { get; set; }
        public void Show(int reportId) {
            Guard.ArgumentNotNull(connectionProvider, "connectionProvider");
            RemoteDocumentSource remoteDocumentSource = new DevExpress.ReportServer.Printing.RemoteDocumentSource();
            remoteDocumentSource.ReportIdentity = new ReportIdentity(reportId);
            remoteDocumentSource.ReportServiceClientDemanded += remoteDocumentSource_ReportServiceClientDemanded;
            reportViewerView = new ReportViewerView();
            reportViewerView.DocumentPreviewControl.DocumentSource = remoteDocumentSource;
            remoteDocumentSource.CreateDocument();
            try {
                reportViewerView.ShowDialog();
            } catch {
                reportViewerView.Close();
                throw;
            }
        }

        void remoteDocumentSource_ReportServiceClientDemanded(object sender, ReportServiceClientDemandedEventArgs e) {
            e.Client = connectionProvider.CreateClient();
        }
    }
}
