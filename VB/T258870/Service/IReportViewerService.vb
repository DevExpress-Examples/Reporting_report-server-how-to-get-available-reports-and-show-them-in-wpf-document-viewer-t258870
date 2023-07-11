Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports DevExpress.ReportServer.ServiceModel.ConnectionProviders
Imports DevExpress.Xpf.Printing

Namespace T258870.Service
    Public Interface IReportViewerService
        Property connectionProvider() As ConnectionProvider
        Sub Show(ByVal reportId As Integer)
    End Interface
End Namespace
