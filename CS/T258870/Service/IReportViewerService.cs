using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.ReportServer.ServiceModel.ConnectionProviders;
using DevExpress.Xpf.Printing;

namespace T258870.Service {
    public interface IReportViewerService {
        ConnectionProvider connectionProvider { get; set; }
        void Show(int reportId);
    }
}
