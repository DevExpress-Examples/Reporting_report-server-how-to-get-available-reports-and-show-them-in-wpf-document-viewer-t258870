Namespace T258870.ViewModel
    Public Class ReportObjects
        Public Property DisplayName() As String
        Public Property Id() As Integer
        Public Overrides Function ToString() As String
            Return DisplayName
        End Function
    End Class
End Namespace
