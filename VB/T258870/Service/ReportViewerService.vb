Imports System.Windows
Imports DevExpress.Mvvm.UI
Imports DevExpress.ReportServer.Printing
Imports DevExpress.ReportServer.ServiceModel.ConnectionProviders
Imports DevExpress.ReportServer.ServiceModel.DataContracts
Imports DevExpress.Utils
Imports DevExpress.Xpf.Printing
Imports T258870.View

Namespace T258870.Service
    Public Class ReportViewerService
        Inherits ServiceBase
        Implements IReportViewerService

        Public Property connectionProvider() As ConnectionProvider Implements IReportViewerService.connectionProvider
        Private Property reportViewerView() As ReportViewerView
        Public Sub Show(ByVal reportId As Integer) Implements IReportViewerService.Show
            Guard.ArgumentNotNull(connectionProvider, "connectionProvider")
            Dim remoteDocumentSource As RemoteDocumentSource = New DevExpress.ReportServer.Printing.RemoteDocumentSource()
            remoteDocumentSource.ReportIdentity = New ReportIdentity(reportId)
            AddHandler remoteDocumentSource.ReportServiceClientDemanded, AddressOf remoteDocumentSource_ReportServiceClientDemanded
            reportViewerView = New ReportViewerView()
            reportViewerView.DocumentPreviewControl.DocumentSource = remoteDocumentSource
            remoteDocumentSource.CreateDocument()
            Try
                reportViewerView.ShowDialog()
            Catch
                reportViewerView.Close()
                Throw
            End Try
        End Sub

        Private Sub remoteDocumentSource_ReportServiceClientDemanded(ByVal sender As Object, ByVal e As ReportServiceClientDemandedEventArgs)
            e.Client = connectionProvider.CreateClient()
        End Sub
    End Class
End Namespace
