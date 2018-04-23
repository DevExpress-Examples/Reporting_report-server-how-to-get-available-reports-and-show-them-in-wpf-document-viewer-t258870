Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports DevExpress.DocumentServices.ServiceModel
Imports DevExpress.Mvvm
Imports DevExpress.Mvvm.POCO
Imports DevExpress.ReportServer.ServiceModel.Client
Imports DevExpress.ReportServer.ServiceModel.ConnectionProviders
Imports DevExpress.ReportServer.ServiceModel.DataContracts
Imports DevExpress.XtraPrinting
Imports T258870.Service
Imports T258870.View

Namespace T258870.ViewModel
    Public Class MainViewModel
        Private Const ServerAddress As String = "https://reportserver.devexpress.com"
        Private ReadOnly serverConnection As ConnectionProvider = New GuestConnectionProvider(ServerAddress)

        Public Property SelectedReportObject() As ReportObjects
        Public Property ReportObjectCollection() As ObservableCollection(Of ReportObjects)

        Private privateIsBusy As Boolean
        Public Overridable Property IsBusy() As Boolean
            Get
                Return privateIsBusy
            End Get
            Protected Set(ByVal value As Boolean)
                privateIsBusy = value
            End Set
        End Property
        Protected Sub OnIsBusyChanged()
            Me.RaiseCanExecuteChanged(Sub(x) x.Preview())
            Me.RaiseCanExecuteChanged(Sub(x) x.Export())
        End Sub

        Protected Overridable ReadOnly Property MessageBoxService() As IMessageBoxService
            Get
                Return Nothing
            End Get
        End Property
        Protected Overridable ReadOnly Property SaveFileDialogService() As ISaveFileDialogService
            Get
                Return Nothing
            End Get
        End Property
        Protected Overridable ReadOnly Property reportViewerService() As IReportViewerService
            Get
                Return Nothing
            End Get
        End Property

        Public Sub New()
            Dim uiContext As SynchronizationContext = SynchronizationContext.Current

            ReportObjectCollection = New ObservableCollection(Of ReportObjects)()
            SelectedReportObject = New ReportObjects() With {.Id = -1}
            serverConnection.ConnectAsync().ContinueWith(Function(task)
                Dim client As IReportServerClient = task.Result
                client.SetSynchronizationContext(uiContext)
                Return client.GetReportsAsync(Nothing)
            End Function).Unwrap().ContinueWith(Sub(task)
                If task.IsFaulted Then
                    MessageBoxService.Show(task.Exception.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error)
                Else
                    FillReportListBox(task.Result)
                End If
End Sub, TaskScheduler.FromCurrentSynchronizationContext())
        End Sub

        Private Sub FillReportListBox(ByVal reports As IEnumerable(Of ReportCatalogItemDto))
            For Each reportDto In reports
                Dim item As New ReportObjects() With {.DisplayName = reportDto.Name, .Id = reportDto.Id}
                Me.ReportObjectCollection.Add(item)
            Next reportDto
        End Sub

        Public Function CanExport() As Boolean
            Return (Not IsBusy) AndAlso (SelectedReportObject.Id > 0)
        End Function

        Public Sub Export()
            SaveFileDialogService.Filter = "PDF files (*.pdf)|*.pdf"
            If SaveFileDialogService.ShowDialog() Then
                IsBusy = True
                ExportToPdf(serverConnection, SaveFileDialogService.GetFullFileName(), SelectedReportObject.Id)
            End If
        End Sub

        Private Sub ExportToPdf(ByVal serverConnection As ConnectionProvider, ByVal fileName As String, ByVal reportId As Integer)
            Task.Factory.ExportReportAsync(serverConnection.CreateClient(), New ReportIdentity(reportId), New PdfExportOptions(), Nothing, Nothing).ContinueWith(Sub(task)
                IsBusy = False
                Try
                    If task.IsFaulted Then
                        Throw New Exception(task.Exception.Flatten().InnerException.Message)
                    ElseIf task.IsCanceled Then
                        MessageBoxService.Show("Operation has been cancelled", "Export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation)
                        Return
                    Else
                        File.WriteAllBytes(fileName, task.Result)
                    End If
                Catch e As Exception
                    MessageBoxService.Show(e.Message, "Export", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error)
                End Try
            End Sub, TaskScheduler.FromCurrentSynchronizationContext())
        End Sub

        Public Function CanPreview() As Boolean
            Return (Not IsBusy) AndAlso (SelectedReportObject.Id > 0)
        End Function

        Public Sub Preview()
            reportViewerService.connectionProvider = serverConnection
            Try
                reportViewerService.Show(SelectedReportObject.Id)
            Catch e As Exception
                MessageBoxService.Show(e.Message, "Preview", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error)
            End Try

        End Sub
    End Class
End Namespace
