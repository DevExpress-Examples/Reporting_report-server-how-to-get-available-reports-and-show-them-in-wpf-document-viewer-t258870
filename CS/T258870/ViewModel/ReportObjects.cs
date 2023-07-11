
namespace T258870.ViewModel {
    public class ReportObjects {
        public string DisplayName { get; set; }
        public int Id { get; set; }
        public override string ToString() {
            return DisplayName;
        }
    }
}
